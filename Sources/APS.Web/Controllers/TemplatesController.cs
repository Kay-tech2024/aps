using APS.Core;
using APS.Core.Managers;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(ApsUserRole.SystemAdmin)]
    public class TemplatesController : BaseController
    {
        public ActionResult Index()
        {
            var list = GetInstance<TemplatesManager>().Templates.Select(t => new TemplateViewModel
            {
                TemplateKey = ((TemplateKey)t.TemplateKey).ToString(),
                SubjectEnglish = t.SubjectEnglish,
                SubjectArabic = t.SubjectArabic,
            });

            return View(list);
        }

        public ActionResult Edit(TemplateKey id)
        {
            if (id == TemplateKey.Unspecified)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Template template = GetInstance<TemplatesManager>()[id];

            if (template == null)
            {
                return HttpNotFound();
            }

            var model = new TemplateViewModel
            {
                TemplateKey = id.ToString(),
                SubjectEnglish = template.SubjectEnglish,
                BodyEnglish = template.BodyEnglish.StartsWith("<div  dir='ltr'>") ? template.BodyEnglish : "<div  dir='ltr'>" + template.BodyEnglish + "</div>",
                SubjectArabic = template.SubjectArabic,
                BodyArabic = template.BodyEnglish.StartsWith("<div  dir='rtl'>") ? template.BodyArabic : "<div  dir='rtl'>" + template.BodyArabic + "</div>",
                IsHtml = template.IsHtml,
            };

            return View(model);
        }

        // POST: Templates/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(TemplateViewModel model)
        {
            var key = TemplateKey.Unspecified;

            if (model == null || !Enum.TryParse<TemplateKey>(model.TemplateKey, out key))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var template = GetInstance<TemplatesManager>()[key];
                    var fileName = string.Empty;
                    var extension = string.Empty;
                    var filePath = string.Empty;
                    if (model.File != null)
                    {
                        fileName = Path.GetFileName(model.File.FileName);
                        extension = Path.GetExtension(fileName);
                        fileName = template.TemplateKey.ToString();
                        filePath = Path.Combine(Server.MapPath("~/DocumentsPdf/"), fileName + extension);
                    }

                    template.BodyEnglish = model.BodyEnglish.StartsWith("<div  dir='ltr'>") ? model.BodyEnglish : "<div  dir='ltr'>" + model.BodyEnglish + "</div>";
                    template.BodyArabic = model.BodyArabic.StartsWith("<div  dir='rtl'>") ? model.BodyArabic : "<div  dir='rtl'>" + model.BodyArabic + "</div>";
                    template.SubjectEnglish = model.SubjectEnglish;
                    template.SubjectArabic = model.SubjectArabic;
                    template.IsHtml = model.IsHtml;
                    template.FileName = fileName;
                    template.FilePath = filePath;
                    template.Extension = extension;

                    GetInstance<TemplatesManager>()[key] = template;

                    model.File.EnhancedSaveAs(filePath);
                    DbContext.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    Logger.LogExceptionAsync(ex, model);
                    TempData["message"] = "Failed to update template (" + ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage + ")";
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

                    TempData["message"] = "Failed to update template (" + ex2.Message + ")";
                    TempData["message_type"] = "danger";
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}