using APS.Core;
using APS.Data.DbModels;
using APS.Services;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    public class SettingsController : BaseController
    {
        [CustomAuthorize(Privilages.settings_email, Privilages.settings_sms, Privilages.settings_tawafuq, Privilages.settings_webservices, Privilages.settings_change_rag)]
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize(Privilages.settings_email)]
        public ActionResult Email()
        {
            return View(SmtpSettings);
        }

        [CustomAuthorize(Privilages.settings_email)]
        [HttpPost]
        public ActionResult Email(SmtpSettings model)
        {
            if (ModelState.IsValid)
            {
                var setts = DbContext.Settings.Where(s => s.Key.StartsWith("smtp")).ToList();

                var setting = (Setting)null;

                if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:host")) != null)
                {
                    setting.Value = model.Host;
                }
                else
                {
                    DbContext.Settings.Add(new Setting
                    {
                        Key = "smtp:Host",
                        Value = model.Host
                    });
                }

                if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:port")) != null)
                {
                    setting.Value = model.Port.ToString();
                }
                else
                {
                    DbContext.Settings.Add(new Setting
                    {
                        Key = "smtp:Port",
                        Value = model.Port.ToString()
                    });
                }

                if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:fromemail")) != null)
                {
                    setting.Value = model.FromEmail;
                }
                else
                {
                    DbContext.Settings.Add(new Setting
                    {
                        Key = "smtp:FromEmail",
                        Value = model.FromEmail
                    });
                }

                if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:username")) != null)
                {
                    setting.Value = model.Username;
                }
                else
                {
                    DbContext.Settings.Add(new Setting
                    {
                        Key = "smtp:Username",
                        Value = model.Username
                    });
                }

                if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:emailcc")) != null)
                {
                    setting.Value = model.EmailCC;
                }
                else
                {
                    DbContext.Settings.Add(new Setting
                    {
                        Key = "smtp:EmailCC",
                        Value = model.EmailCC
                    });
                }

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:password")) != null)
                    {
                        setting.Value = model.Password;
                    }
                    else
                    {
                        DbContext.Settings.Add(new Setting
                        {
                            Key = "smtp:Password",
                            Value = model.Password
                        });
                    }
                }

                if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:ssl")) != null)
                {
                    setting.Value = model.SSL.ToString();
                }
                else
                {
                    DbContext.Settings.Add(new Setting
                    {
                        Key = "smtp:SSL",
                        Value = model.SSL.ToString()
                    });
                }

                DbContext.SaveChanges();
                TempData["message"] = "Update is successful";
                TempData["message_type"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "Update failed";
                TempData["message_type"] = "danger";
            }
            return View(model);
        }

        [CustomAuthorize(Privilages.settings_sms)]
        public ActionResult Sms()
        {
            var MalathSmsSettings = Utils.MalathSmsSettings;
            var MobilySmsSettings = Utils.MobilySmsSettings;

            var model = new SmsSettingsViewModel() { Providers = new List<SmsProviderViewModel>(), PreferredProvider = MalathSmsSettings.Type };
            model.Providers.Add(new SmsProviderViewModel
            {
                Type = SmsProvider.MalathSMS,
                SenderName = MalathSmsSettings.Sender,
                Username = MalathSmsSettings.Username,
                Password = MalathSmsSettings.Password
            });

            model.Providers.Add(new SmsProviderViewModel
            {
                Type = SmsProvider.MobilyWS,
                SenderName = MobilySmsSettings.Sender,
                Username = MobilySmsSettings.Username,
                Password = MobilySmsSettings.Password
            });

            return View(model);
        }

        [CustomAuthorize(Privilages.settings_sms)]
        [HttpPost]
        public ActionResult Sms(SmsSettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var setts = DbContext.Settings.Where(s => s.Key.StartsWith("sms")).ToList();

                var setting = (Setting)null;

                if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:type")) != null)
                {
                    setting.Value = model.PreferredProvider.ToString();
                }
                else
                {
                    DbContext.Settings.Add(new Setting
                    {
                        Key = "sms:type",
                        Value = model.PreferredProvider.ToString()
                    });
                }

                foreach (var p in model.Providers)
                {
                    var suffix = ":" + (p.Type == SmsProvider.MalathSMS ? "malath" : "mobily");

                    if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:sender" + suffix)) != null)
                    {
                        setting.Value = p.SenderName;
                    }
                    else
                    {
                        DbContext.Settings.Add(new Setting
                        {
                            Key = "sms:sender" + suffix,
                            Value = p.SenderName
                        });
                    }

                    if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:username" + suffix)) != null)
                    {
                        setting.Value = p.Username;
                    }
                    else
                    {
                        DbContext.Settings.Add(new Setting
                        {
                            Key = "sms:Username" + suffix,
                            Value = p.Username
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(p.Password) && p.Password != "**********")
                    {
                        if ((setting = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:password" + suffix)) != null)
                        {
                            setting.Value = p.Password;
                        }
                        else
                        {
                            DbContext.Settings.Add(new Setting
                            {
                                Key = "sms:Password" + suffix,
                                Value = p.Password
                            });
                        }
                    }
                }

                DbContext.SaveChanges();
                TempData["message"] = "SMS settings updated successfully";
                TempData["message_type"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["message"] = "SMS settings failed to update";
                TempData["message_type"] = "danger";
            }
            return View(model);
        }

        private bool SendNotification(string email)
        {
            try
            {
                var to = new MailAddress(email);

                return EmailService.SendMessage(SmtpSettings, "Test Email", "This is test message", to, null, null);
            }
            catch (DbEntityValidationException ex)
            {
                Logger.LogExceptionAsync(ex, email);
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, email);
                return false;
            }
        }

        [ValidateAntiForgeryToken]
        [CustomAuthorize(Privilages.settings_email)]
        [HttpPost]
        public ActionResult SendTestMessage(string Email)
        {
            if (SendNotification(Email))
            {
                TempData["message"] = "Send test message";
                TempData["message_type"] = "success";
                return RedirectToAction("Email");
            }
            else
            {
                if (TempData["message"] == null)
                {
                    TempData["message"] = "Failed to send test message";
                    TempData["message_type"] = "danger";
                }
                return RedirectToAction("Email");
            }
        }

        [CustomAuthorize(Privilages.settings_sms)]
        [HttpPost]
        public ActionResult SendTestSmsMessage(string mobileNumber, SmsProvider? provider)
        {
            try
            {
                var MalathSmsSettings = Utils.MalathSmsSettings;
                var MobilySmsSettings = Utils.MobilySmsSettings;

                string error = null;
                var status = false;

                if (provider == SmsProvider.MalathSMS)
                {
                    status = SmsServiceMalath.SendMessage(MalathSmsSettings, "This is test message", new string[] { mobileNumber }, out error);
                }
                else
                {
                    status = SmsServiceMobily.SendMessage(MobilySmsSettings, "This is test message", new string[] { mobileNumber }, out error);
                }

                if (!status)
                {
                    TempData["message"] = error;
                    TempData["message_type"] = "danger";
                }
                else
                {
                    TempData["message"] = "Test message was sent successfully";
                    TempData["message_type"] = "success";
                }
            }
            catch (DbEntityValidationException ex)
            {
                Logger.LogExceptionAsync(ex, mobileNumber);

                TempData["message"] = "Failed to send test message (" + ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage + ")";
                TempData["message_type"] = "danger";
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, mobileNumber);
                TempData["message"] = "Failed to send test message (exception: " + ex.Message + ")";
                TempData["message_type"] = "danger";
            }

            return RedirectToAction("Sms");
        }
    }
}