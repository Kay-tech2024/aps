using APS.Core;
using System;
using System.Net.Mail;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    public class TestController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Email()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Email(string Email, string Mobile)
        {
            if (string.IsNullOrEmpty(Email))
            {
                TempData["message"] = "Please Enter E-Mail";
                TempData["message_type"] = "danger";
            }
            else
            {
                try
                {
                    var error = "";
                    var errorMsg = "";
                    if (!string.IsNullOrEmpty(Mobile))
                    {
                        var smssend = SmsService.SendMessage("This is Test SMS From APS", new string[] { Mobile }, out errorMsg);
                        if (smssend)
                        {
                            var IsSend = EmailService.SendMessage(SmtpSettings, "Testing E-Mail Service", "This is Test Mail From APS", new MailAddress(Email), null, null);
                            if (IsSend)
                            {
                                TempData["message"] = "E-Mail AND SMS Send Successfully !";
                                TempData["message_type"] = "success";
                            }
                            else
                            {
                                TempData["message"] = error;
                                TempData["message_type"] = "danger";
                            }
                        }
                        else
                        {
                            TempData["message"] = errorMsg;
                            TempData["message_type"] = "danger";
                        }
                    }
                    else
                    {
                        var IsSend = EmailService.SendMessage(SmtpSettings, "Testing E-Mail Service", "This is Test Mail From APS", new MailAddress(Email), null, null);
                        if (IsSend)
                        {
                            TempData["message"] = "E-Mail Send Successfully !";
                            TempData["message_type"] = "success";
                        }
                        else
                        {
                            TempData["message"] = error;
                            TempData["message_type"] = "danger";
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["message"] = ex.Message;
                    TempData["message_type"] = "danger";
                }
            }
            return View();
        }
    }
}