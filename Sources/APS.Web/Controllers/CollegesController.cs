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
    [CustomAuthorize(Privilages.colleges_view_list)]
    public class CollegesController : BaseController
    {
        [CustomAuthorize(Privilages.colleges_view_list)]
        public ActionResult Index()
        {
            var query = DbContext.Colleges.Where(b => !b.IsDeleted);

            var providers = query.Select(p => new CollegeViewModel
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
            var model = new CollegeViewModel();
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(CollegeViewModel model)
        {
            if (DbContext.Colleges.Count(j => j.NameEnglish == model.NameEnglish) > 0)
            {
                ModelState.AddModelError("NameEnglish", "هذه الكلية موجود سابقا  \"" + model.NameEnglish + "\"");
            }
            else if (DbContext.Colleges.Count(j => j.NameArabic == model.NameArabic) > 0)
            {
                ModelState.AddModelError("NameEnglish", "هذه الكلية موجود سابقا  \"" + model.NameArabic + "\"");
            }

            if (ModelState.IsValid)
            {
                var dep = new College
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
                    DbContext.Colleges.Add(dep);

                    DbContext.SaveChanges();

                    TempData["message"] = ApsStrings.college_created_successfully;
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Logger.LogExceptionAsync(ex, model);
                    TempData["message"] = ApsStrings.college_creation_failed + model.NameEnglish + " (" + ex.SanitizeException() + ")";
                    TempData["message_type"] = "danger";
                }
            }

            return View(model);
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                TempData["message"] = ApsStrings.college_not_found;
                TempData["message_type"] = "danger";

                return RedirectToAction("Index");
            }

            College college = DbContext.Colleges.Find(id);

            if (college == null)
            {
                TempData["message"] = ApsStrings.college_not_found;
                TempData["message_type"] = "danger";

                return RedirectToAction("Index");
            }

            var model = new CollegeViewModel
            {
                Id = college.Id,
                NameEnglish = college.NameEnglish,
                NameArabic = college.NameArabic,
            };

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(CollegeViewModel model)
        {
            var college = DbContext.Colleges.SingleOrDefault(p => p.Id == model.Id);

            if (college == null)
            {
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrWhiteSpace(model.NameEnglish))
            {
                var college2 = DbContext.Colleges.FirstOrDefault(j => !j.IsDeleted && j.NameEnglish == model.NameEnglish);
                if (college2 != null && college2.Id != college.Id)
                {
                    TempData["message"] = ApsStrings.college_must_be_unique;
                    TempData["message_type"] = "danger";
                    ModelState.AddModelError("EnglishName", ApsStrings.college_must_be_unique);
                }
            }

            if (ModelState.IsValid)
            {
                college.NameEnglish = model.NameEnglish;
                college.NameArabic = model.NameArabic;

                college.Updated = DateTime.Now;
                college.UpdatedBy = CurrentUser.Id;

                try
                {
                    DbContext.Entry(college).State = System.Data.Entity.EntityState.Modified;
                    DbContext.SaveChanges();

                    TempData["message"] = ApsStrings.college_updated_successfully;
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Logger.LogExceptionAsync(ex, model);
                    TempData["message"] = ApsStrings.college_update_failed + " " + model.NameEnglish + " " + " (" + ex.SanitizeException() + ")";
                    TempData["message_type"] = "danger";
                }
            }

            return View(model);
        }

        [AjaxOnly]
        public dynamic AjaxDeleteCollege(int? id)
        {
            var ids = Guid.Empty;

            if (id == null || id <= 0)
                return new { success = false, msg = "INVALID_ID" };

            try
            {
                var college = DbContext.Colleges.SingleOrDefault(u => u.Id == id);

                if (college == null)
                {
                    return new { success = false, msg = "NOT_FOUND" };
                }

                try
                {
                    DbContext.Entry(college).State = System.Data.Entity.EntityState.Deleted;
                    DbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                }

                college.IsDeleted = true;
                DbContext.Entry(college).State = System.Data.Entity.EntityState.Modified;
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