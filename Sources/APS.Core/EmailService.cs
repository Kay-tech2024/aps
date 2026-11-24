using APS.Core.Managers;
using MimeTypes;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace APS.Core
{
    public class SmtpSettings
    {
        [Required(ErrorMessage = "اسم الخادم Required")]
        public string Host { get; set; }

        [Required(ErrorMessage = "رقم المنفذ Required")]
        public int Port { get; set; }

        public string FromName { get; set; }

        [Required(ErrorMessage = "البريد المرسل Required")]
        public string FromEmail { get; set; }

        [Required(ErrorMessage = "البريد المرفق Required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "البريد المرسل Required")]
        public string EmailCC { get; set; }

        public string Password { get; set; }

        public bool SSL { get; set; }
    }

    public class EmailService : AbstractManager
    {
        public static bool SendMessage(SmtpSettings settings, string Subject, string MessageBody, MailAddress[] ToAddress, MailAddress[] CC, string[] attachments)
        {
            return SendMessage(settings, Subject, MessageBody, ToAddress, CC, null, attachments);
        }

        public static bool SendMessage(SmtpSettings settings, string Subject, string MessageBody, MailAddress ToAddress, MailAddress[] CC, string[] attachments)
        {
            return SendMessage(settings, Subject, MessageBody, new MailAddress[] { ToAddress }, CC, null, attachments);
        }

        public static bool SendMessage(SmtpSettings settings, string Subject, string MessageBody, MailAddress[] ToAddress, MailAddress[] CC, MailAddress[] BCC, string[] attachments)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtpClient = new SmtpClient();
                string msg = string.Empty;
                var fromAddress = string.IsNullOrWhiteSpace(settings.FromName) ? new MailAddress(settings.FromEmail) : new MailAddress(settings.FromEmail, settings.FromName);
                message.From = fromAddress;

                if (Utils.IsLocalhost)
                {
                    settings.Host = "smtp.gmail.com";
                    settings.Port = 587;
                    settings.SSL = true;
                    settings.Username = "kaytechnology.test@gmail.com";
                    settings.Password = "__SET_ME__";
                }

                foreach (var to in ToAddress)
                {
                    message.To.Add(to);
                }

                if (!string.IsNullOrWhiteSpace(settings.EmailCC))
                {
                    message.Bcc.Add(settings.EmailCC);
                }

                message.Subject = Subject;
                message.IsBodyHtml = true;

                if (CC != null)
                {
                    foreach (var a in CC)
                    {
                        message.CC.Add(a);
                    }
                }

                if (BCC != null)
                {
                    foreach (var a in BCC)
                    {
                        message.Bcc.Add(a);
                    }
                }

                if (Utils.IsLocalhost)
                {
                    var oldBody = MessageBody;

                    MessageBody = "<div><em>Test message. Supposed to be sent to:</em><ul>";

                    foreach (var to in ToAddress)
                    {
                        MessageBody += "<li>" + to.Address + "</li>";
                    }

                    MessageBody += "</ul></div><br/><hr/>" + oldBody;

                    message.To.Clear();
                    message.CC.Clear();
                    message.Bcc.Clear();

                    message.Bcc.Add(new MailAddress("a.abujalila@kay-ict.com", "Abdulrahman Abujailila"));
                    message.To.Add(new MailAddress("omar.shaaban@kay-ict.com", "Omar Shaaban"));
                    message.Bcc.Add(new MailAddress("syed.hyder@kay-ict.com", "Syed Hyder"));
                    message.Bcc.Add(new MailAddress("gousebaig@kay-ict.com", "gousebaig"));

                    message.Subject = "(FAKE EMAIL) " + message.Subject;
                    message.Body = MessageBody;
                }

                message.Body = MessageBody;

                if (attachments != null && attachments.Length > 0)
                {
                    foreach (var a in attachments)
                    {
                        if (File.Exists(a))
                            message.Attachments.Add(new System.Net.Mail.Attachment(a, MimeTypeMap.GetMimeType(Path.GetExtension(a))));
                    }
                }
                if (Constants.UseSendGrid)
                {
                    var apiKey = Constants.SendGridAPIKey;
                    var sendGridClient = new SendGridClient(apiKey);

                    var msgSendGrid = new SendGridMessage()
                    {
                        From = new EmailAddress(settings.FromEmail, settings.FromName),
                        Subject = message.Subject,
                        PlainTextContent = Utils.StripHTML(message.Body),
                        HtmlContent = message.Body
                    };

                    var emails = new HashSet<string>();

                    foreach (var to in message.To)
                    {
                        if (to.Address == null) continue;
                        var email = to.Address.Trim().ToLower();
                        if (emails.Contains(email)) continue;
                        emails.Add(email);

                        msgSendGrid.AddTo(new EmailAddress(to.Address, to.DisplayName));
                    }

                    foreach (var cc in message.CC)
                    {
                        if (cc.Address == null) continue;
                        var email = cc.Address.Trim().ToLower();
                        if (emails.Contains(email)) continue;
                        emails.Add(email);

                        msgSendGrid.AddCc(new EmailAddress(cc.Address, cc.DisplayName));
                    }

                    foreach (var bcc in message.Bcc)
                    {
                        if (bcc.Address == null) continue;
                        var email = bcc.Address.Trim().ToLower();
                        if (emails.Contains(email)) continue;
                        emails.Add(email);

                        msgSendGrid.AddBcc(new EmailAddress(bcc.Address, bcc.DisplayName));
                    }

                    if (attachments != null && attachments.Length > 0)
                    {
                        msgSendGrid.Attachments = new List<SendGrid.Helpers.Mail.Attachment>();
                        foreach (var a in attachments)
                        {
                            if (File.Exists(a))
                            {
                                var attachment = new System.Net.Mail.Attachment(a, MimeTypeMap.GetMimeType(Path.GetExtension(a)));
                                msgSendGrid.Attachments.Add(Utils.GetSendGridAttachment(attachment));
                            }
                        }
                    }

                    var response = sendGridClient.SendEmailAsync(msgSendGrid);
                    response.Wait();

                    var isSuccess = response.Result.StatusCode == HttpStatusCode.Accepted;

                    if (!isSuccess)
                    {
                        var msg2 = response.Result.Body.ReadAsStringAsync().Result;
                        //err = msg2;
                        Logger.LogExceptionAsync(new Exception(msg2));
                        return false;
                    }
                }
                else
                {
                    smtpClient.Host = settings.Host; // "smtp.gmail.com";   // We use gmail as our smtp client
                    smtpClient.Port = settings.Port; //587;
                    smtpClient.EnableSsl = settings.SSL;
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(settings.Username, settings.Password);
                    smtpClient.Timeout = 360000;

                    smtpClient.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, settings);
                return false;
                //throw ex;
            }
        }
    }
}