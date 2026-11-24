using APS.Core;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(Privilages.majors_view_list)]
    public class MajorsController : BaseController
    {
        [CustomAuthorize(Privilages.majors_view_list)]
        public ActionResult Index()
        {
            var query = DbContext.Majors.Where(b => !b.IsDeleted);

            var providers = query.Select(p => new MajorViewModel
            {
                Id = p.Id,
                Name = IsArabic && !string.IsNullOrEmpty(p.NameArabic) ? p.NameArabic : p.NameEnglish,
                NameEnglish = p.NameEnglish,
                NameArabic = p.NameArabic,
            }).OrderBy(p => IsArabic && !string.IsNullOrEmpty(p.NameArabic) ? p.NameArabic : p.NameEnglish).ToList();

            return View(providers);
        }

        public ActionResult Create()
        {
            var model = new MajorViewModel();
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(MajorViewModel model)
        {
            if (DbContext.Majors.Count(j => j.NameEnglish == model.NameEnglish) > 0)
            {
                ModelState.AddModelError("NameEnglish", "هذا التخصص موجود سابقا  \"" + model.NameEnglish + "\"");
            }
            else if (DbContext.Majors.Count(j => j.NameArabic == model.NameArabic) > 0)
            {
                ModelState.AddModelError("NameEnglish", "هذا التخصص موجود سابقا  \"" + model.NameArabic + "\"");
            }

            if (ModelState.IsValid)
            {
                var dep = new Major
                {
                    NameEnglish = model.NameEnglish,
                    NameArabic = model.NameArabic,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    CreatedBy = CurrentUser.Id,
                    UpdatedBy = CurrentUser.Id
                };

                try
                {
                    DbContext.Majors.Add(dep);

                    DbContext.SaveChanges();

                    TempData["message"] = ApsStrings.major_created_successfully;
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Logger.LogExceptionAsync(ex, model);
                    TempData["message"] = ApsStrings.major_creation_failed + model.NameEnglish + " (" + ex.SanitizeException() + ")";
                    TempData["message_type"] = "danger";
                }
            }

            return View(model);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                TempData["message"] = ApsStrings.major_not_found;
                TempData["message_type"] = "danger";

                return RedirectToAction("Index");
            }

            Major major = DbContext.Majors.Find(id);

            if (major == null)
            {
                TempData["message"] = ApsStrings.major_not_found;
                TempData["message_type"] = "danger";

                return RedirectToAction("Index");
            }

            var model = new MajorViewModel
            {
                Id = major.Id,
                NameEnglish = major.NameEnglish,
                NameArabic = major.NameArabic,
            };

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(MajorViewModel model)
        {
            var major = DbContext.Majors.SingleOrDefault(p => p.Id == model.Id);

            if (major == null)
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(model.NameEnglish))
            {
                var major2 = DbContext.Majors.FirstOrDefault(j => !j.IsDeleted && j.NameEnglish == model.NameEnglish);
                if (major2 != null && major2.Id != major.Id)
                {
                    TempData["message"] = ApsStrings.major_must_be_unique;
                    TempData["message_type"] = "danger";
                    ModelState.AddModelError("EnglishName", ApsStrings.major_must_be_unique);
                }
            }

            if (ModelState.IsValid)
            {
                major.NameEnglish = model.NameEnglish;
                major.NameArabic = model.NameArabic;

                major.Updated = DateTime.Now;
                major.UpdatedBy = CurrentUser.Id;

                try
                {
                    DbContext.Entry(major).State = System.Data.Entity.EntityState.Modified;
                    DbContext.SaveChanges();

                    TempData["message"] = ApsStrings.major_updated_successfully;
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Logger.LogExceptionAsync(ex, model);
                    TempData["message"] = ApsStrings.major_update_failed + " " + model.NameEnglish + " " + " (" + ex.SanitizeException() + ")";
                    TempData["message_type"] = "danger";
                }
            }

            return View(model);
        }

        [AjaxOnly]
        public dynamic AjaxDeleteMajor(int? id)
        {
            var ids = Guid.Empty;

            if (id == null || id <= 0)
                return new { success = false, msg = "INVALID_ID" };

            try
            {
                var major = DbContext.Majors.SingleOrDefault(u => u.Id == id);

                if (major == null)
                {
                    return new { success = false, msg = "NOT_FOUND" };
                }

                try
                {
                    DbContext.Entry(major).State = System.Data.Entity.EntityState.Deleted;
                    DbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                }

                major.IsDeleted = true;
                DbContext.Entry(major).State = System.Data.Entity.EntityState.Modified;
                DbContext.SaveChanges();
                return new { success = true, msg = "DELETED" };
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, id);
                return new { success = false, msg = ex.SanitizeException() };
            }
        }
    }
}