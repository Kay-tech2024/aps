using APS.Core;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.Models;
using APS.Web.Resources;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    public class AboutController : BaseController
    {
        // GET: About
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ContactUs()
        {
            var model = new ContactUsViewModel();
            return View(model);
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ContactUs(ContactUsViewModel model)
        {
            var contactUs = new ContactedU();
            var db = Utils.db;
            try
            {
                contactUs.FullName = model.FullName;
                contactUs.EmailAddress = model.EmailAddress;
                contactUs.Subject = model.Subject;
                contactUs.Message = model.Message;
                contactUs.Created = DateTime.Now;
                db.ContactedUs.Add(contactUs);
                db.SaveChanges();

                var allSystemAdmins = db.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.SystemAdmin.ToString()) > 0).ToList();
                var emails = string.Join(";", allSystemAdmins.Select(x => x.Email).ToArray());
                var names = string.Join(",", allSystemAdmins.Select(x => x.FullName).ToArray());
                var isSent = SendContactUsMessage(model, emails, names);
                if (isSent)
                {
                    TempData["message"] = "تم التسجيل بنجاح";
                    TempData["message_type"] = "success";
                    model.Message = "";
                }
                else
                {
                    TempData["message"] = "لم يتم إرسال رسالتك. يرجى الاتصال بعد بعض الوقت.";
                    TempData["message_type"] = "danger";
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                Logger.LogExceptionAsync(ex, model);
                TempData["message"] = ApsStrings.failed + " " + ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                TempData["message_type"] = "danger";
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, model);

                var ex2 = ex;
                while (ex2.InnerException != null)
                {
                    ex2 = ex2.InnerException;
                }
                TempData["message"] = ApsStrings.failed + " " + ex2.Message;
                TempData["message_type"] = "danger";
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult UserGuide()
        {
            //return File(Utils.MapPath("~/Content/files/ضوابط النشر العلمي.pdf"), MimeMapping.GetMimeMapping("testpdf.pdf"));
            return File(Utils.MapPath("~/Content/files/دليل المستخدم.pdf"), MimeMapping.GetMimeMapping("testpdf.pdf"));
        }

        [AllowAnonymous]
        public ActionResult ApsGuide()
        {
            //return File(Utils.MapPath("~/Content/files/ضوابط النشر العلمي.pdf"), MimeMapping.GetMimeMapping("testpdf.pdf"));
            return File(Utils.MapPath("~/Content/files/ضوابط النشر العلمي.pdf"), MimeMapping.GetMimeMapping("testpdf.pdf"));
        }

        [AllowAnonymous]
        public ActionResult PublishingPriorities()
        {
            //return File(Utils.MapPath("~/Content/files/ضوابط النشر العلمي.pdf"), MimeMapping.GetMimeMapping("testpdf.pdf"));
            return File(Utils.MapPath("~/Content/files/أولويات البحث والنشر العلمي.pdf"), MimeMapping.GetMimeMapping("testpdf.pdf"));
        }
    }
}