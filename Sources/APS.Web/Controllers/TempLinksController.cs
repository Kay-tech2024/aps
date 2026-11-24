using APS.Core;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [AllowAnonymous]
    public class TempLinksController : BaseController
    {
        // GET: TempLinks
        [HttpGet]
        public ActionResult Index(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Response.Write(ApsStrings.Invalidkey);
                Response.End();

                return Content("");
            }

            var linkData = DbContext.UserTemporaryUrls.FirstOrDefault(l => l.Key == key);

            if (linkData != null)
            {
                if (linkData.IsUsed)
                {
                    return Content("هذا الرابط مستخدم");
                }

                if (linkData.Expires < DateTime.Now)
                {
                    return Content("هذا الرابط منتهية صلاحيته");
                }

                var user = DbContext.Users.FirstOrDefault(u => u.Id == linkData.UserId);

                if (user == null)
                {
                    return Content("هذا الرابط غير صالح");
                }

                switch ((TempUrlType)linkData.Type)
                {
                    case TempUrlType.EmailActivation:
                        if (user.EmailConfirmed == true)
                        {
                            return Content("تم تفعيل هذا الحساب مسبقًا");
                        }

                        linkData.IsUsed = true;
                        user.EmailConfirmed = true;
                        user.Updated = DateTime.Now;
                        DbContext.SaveChanges();

                        if (user.IsActivated == true)
                            return Content("تم التأكد من بريدك الإلكتروني وبإمكانك الدخول الآن");
                        else
                            return Content("تم التأكد من بريدك الإلكتروني ولكن حسابك لم يتم تفعيله بعد. يرجى التواصل مع مسؤول النظام.");

                    case TempUrlType.ResetPassword:

                        var model = new ResetPasswordViewModel
                        {
                            Key = key,
                            Email = user.Email
                        };
                        return View("~/Views/TempLinks/ResetPassword.cshtml", model);
                }

                return Content("هذا الرابط غير صالح للاستخدام");
            }
            else
            {
                var coresearcher = Utils.db.RequestCoresearchers
                    .Include("Researcher")
                    .OrderByDescending(t => t.Updated)
                    .FirstOrDefault(t => t.Key == key);

                if (coresearcher == null)
                {
                    Response.Write(ApsStrings.InvalidLink);
                    Response.End();

                    return Content("");
                }

                if (coresearcher.IsKeyUsed)
                {
                    Response.Write(ApsStrings.LinkisAlreadyused);
                    Response.End();

                    return Content("");
                }
                return RedirectToAction("CoResearch", new
                {
                    Key = key
                });
            }
        }

        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Key)) return RedirectToAction("Login", "Account");

            var linkData = DbContext.UserTemporaryUrls.FirstOrDefault(l => l.Key == model.Key);

            ModelState.Clear();

            if (linkData == null)
            {
                TempData["message"] = "هذا الرابط غير صالح";
                TempData["message_type"] = "danger";
                return RedirectToAction("Login", "Account");
            }

            if (linkData.IsUsed)
            {
                TempData["message"] = "هذا الرابط مستخدم";
                TempData["message_type"] = "danger";
                return RedirectToAction("Login", "Account");
            }

            if (linkData.Expires < DateTime.Now)
            {
                TempData["message"] = "هذا الرابط منتهية صلاحيته";
                TempData["message_type"] = "danger";
                return RedirectToAction("Login", "Account");
            }

            var user = DbContext.Users.FirstOrDefault(u => u.Id == linkData.UserId);

            if (user == null)
            {
                TempData["message"] = "هذا الرابط غير صالح";
                TempData["message_type"] = "danger";
                return RedirectToAction("Login", "Account");
            }

            model.Email = user.Email;

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", ApsStrings.password_required);
            }
            else if (model.ConfirmPassword != model.Password)
            {
                ModelState.AddModelError("ConfirmPassword", ApsStrings.password_confirmation_mismatch);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    linkData.IsUsed = true;
                    DbContext.SaveChanges();

                    TempData["message"] = "تم تغيير كلمة المرور بنجاح";
                    TempData["message_type"] = "success";

                    return RedirectToAction("Login", "Account");
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
            }

            return View("ResetPassword", model);
        }

        [HttpGet]
        public ActionResult CoResearch(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                Response.Write(ApsStrings.Invalidkey);
                Response.End();
                return null;
            }

            var coresearcher = Utils.db.RequestCoresearchers
                                    .Include("Researcher")
                                    .Include("Request")
                                    .Include("Request.BookRequest")
                                    .Include("Request.HandbookRequest")
                                    .Include("Request.ReviewRequest")
                                    .Include("Request.EncyclopediaRequest")
                                    .Include("Request.DictionaryRequest")
                                    .Include("Request.ThesisRequest")
                                    .OrderByDescending(t => t.Updated)
                                    .FirstOrDefault(t => t.Key == key);

            if (coresearcher == null)
            {
                Response.Write(ApsStrings.InvalidLink);
                Response.End();
                return null;
            }

            if (coresearcher.IsKeyUsed)
            {
                Response.Write(ApsStrings.LinkisAlreadyused);
                Response.End();
                return null;
            }

            var researcher = coresearcher.Researcher;

            var ResearcherRequest = coresearcher;
            var request = coresearcher.Request;
            var model = new CoResearcherMessageViewModel();
            //ViewBag.AutherMajors = Utils.db.Majors.ToList();
            model.RequestId = request.Id;
            model.RequestStatus = request.Status;
            if (request.BookRequest != null)
            {
                model.Title = request.BookRequest.BookTitle;
                model.type = request.BookRequest.BookType;
                model.MajorAuthorId = request.BookRequest.MajorId;
                model.Name = researcher.FirstName + "" + researcher.SecondName + " " + researcher.ThirdName + " " + researcher.LastName;
                model.mobileNo = researcher.MobileNo;
                model.Email = researcher.Email;
                model.OriginalTitle = request.BookRequest.OriginalTitleBook;
                model.ScientificType = "Book";
                model.PublisherName = request.BookRequest.Publisher;
                model.PublisherDate = request.BookRequest.PublishingDate;
                model.PublishType = request.BookRequest.PublishType;
                model.IsArabic = request.BookRequest.IsArabic;
                model.TranslatedTo = request.BookRequest.TranslatedTo == null ? "لايوجد" : request.BookRequest.TranslatedTo;
                model.Key = coresearcher.Key;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.BookRequest.MajorId).ToList();
            }
            else if (request.HandbookRequest != null)
            {
                model.Title = request.HandbookRequest.HandbookTitle;
                model.type = request.HandbookRequest.HandbookType;
                model.MajorAuthorId = request.HandbookRequest.MajorId;
                model.Name = researcher.FirstName + "" + researcher.SecondName + " " + researcher.ThirdName + " " + researcher.LastName;
                model.mobileNo = researcher.MobileNo;
                model.Email = researcher.Email;
                model.OriginalTitle = request.HandbookRequest.OriginalHandbookTitle;
                model.ScientificType = "Handbook";
                model.PublisherName = request.HandbookRequest.Publisher;
                model.PublisherDate = request.HandbookRequest.PublisherDate;
                model.PublishType = request.HandbookRequest.PublishType;
                model.IsArabic = request.HandbookRequest.IsArabic;
                model.TranslatedTo = request.HandbookRequest.TranslatedTo == null ? "لايوجد" : request.HandbookRequest.TranslatedTo;
                model.Key = coresearcher.Key;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.HandbookRequest.MajorId).ToList();
            }
            else if (request.ReviewRequest != null)
            {
                model.Title = request.ReviewRequest.BookTitle;
                model.AuthersName = request.ReviewRequest.AuthersName;
                model.ScopOfManuscript = request.ReviewRequest.ScopOfManuscript;
                model.Name = researcher.FirstName + "" + researcher.SecondName + " " + researcher.ThirdName + " " + researcher.LastName;
                model.mobileNo = researcher.MobileNo;
                model.Email = researcher.Email;
                model.type = request.ReviewRequest.ReviewType;
                model.ScientificType = "Review";
                model.MajorId = request.ReviewRequest.ReviewMajor;
                model.PublisherDate = request.ReviewRequest.PublisherDate;
                model.PublishType = request.ReviewRequest.PublishType;
                model.Key = coresearcher.Key;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.ReviewRequest.ReviewMajor).ToList();
            }
            else if (request.EncyclopediaRequest != null)
            {
                model.Title = request.EncyclopediaRequest.EncyclopediaTitle;
                model.MajorId = request.EncyclopediaRequest.MajorOfEncyclopedia;
                model.MajorAuthorId = request.EncyclopediaRequest.MajorOfAuther;
                model.Name = researcher.FirstName + "" + researcher.SecondName + " " + researcher.ThirdName + " " + researcher.LastName;
                model.mobileNo = researcher.MobileNo;
                model.Email = researcher.Email;
                model.OriginalTitle = request.EncyclopediaRequest.OriginalEncyclopediaTitle;
                model.ScientificType = "Encyclopedia";
                model.NoOfRecommendations = request.EncyclopediaRequest.NoOfRecommendations;
                model.RecommendationDate = request.EncyclopediaRequest.RecommendationDate.Date.ToString();
                model.PublisherName = request.EncyclopediaRequest.Publisher;
                model.PublisherDate = request.EncyclopediaRequest.PublisherDate;
                model.PublishType = request.EncyclopediaRequest.PublishType;
                model.IsArabic = request.EncyclopediaRequest.IsArabic;
                model.TranslatedTo = request.EncyclopediaRequest.TranslatedTo == null ? "لايوجد" : request.EncyclopediaRequest.TranslatedTo;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.EncyclopediaRequest.MajorOfAuther).ToList();
                ViewBag.Major = Utils.db.Majors.Where(e => e.Id == request.EncyclopediaRequest.MajorOfEncyclopedia).ToList();
                model.Key = coresearcher.Key;
            }
            else if (request.DictionaryRequest != null)
            {
                model.Title = request.DictionaryRequest.DictionaryTitle;
                model.MajorId = request.DictionaryRequest.DictionaryMajor;
                model.MajorAuthorId = request.DictionaryRequest.MajorOfAuther;
                model.Name = researcher.FirstName + "" + researcher.SecondName + " " + researcher.ThirdName + " " + researcher.LastName;
                model.mobileNo = researcher.MobileNo;
                model.Email = researcher.Email;
                model.OriginalTitle = request.DictionaryRequest.OriginalDictionaryTitle;
                model.ScientificType = "Dictionary";
                model.NoOfRecommendations = request.DictionaryRequest.NoOfRecommendations;
                model.RecommendationDate = request.DictionaryRequest.RecommendationDate.Date.ToString();
                model.PublisherName = request.DictionaryRequest.Publisher;
                model.PublisherDate = request.DictionaryRequest.PublisherDate;
                model.PublishType = request.DictionaryRequest.PublishType;
                model.IsArabic = request.DictionaryRequest.IsArabic;
                model.TranslatedTo = request.DictionaryRequest.TranslatedTo == null ? "لايوجد" : request.DictionaryRequest.TranslatedTo;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.DictionaryRequest.MajorOfAuther).ToList();
                ViewBag.Major = Utils.db.Majors.Where(e => e.Id == request.DictionaryRequest.DictionaryMajor).ToList();

                model.Key = coresearcher.Key;
            }
            else if (request.ThesisRequest != null)
            {
                model.Title = request.ThesisRequest.ThesisTitle;
                model.MajorAuthorId = request.ThesisRequest.SciDepartmentId;
                model.Name = researcher.FirstName + "" + researcher.SecondName + " " + researcher.ThirdName + " " + researcher.LastName;
                model.mobileNo = researcher.MobileNo;
                model.Email = researcher.Email;
                model.CommitteeMembers = request.ThesisRequest.CommitteeMembers;
                model.ScientificType = "Thesis";
                model.Supervisor = request.ThesisRequest.Supervisor;
                model.Supervisor = request.ThesisRequest.AssistantSupervisor;
                model.DateOfDiscussion = request.ThesisRequest.DateOfDiscussion;
                model.Degree = request.ThesisRequest.Degree;
                model.PublisherName = request.ThesisRequest.Publisher;
                model.PublisherDate = request.ThesisRequest.PublisherDate;
                model.PublishType = request.ThesisRequest.PublishType;
                model.Gpa = request.ThesisRequest.Gpa;
                model.Key = coresearcher.Key;
                //ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.ThesisRequest.MajorId).ToList();
            }

            model.CoResearcherFiles = request.RequestFiles
                                                .Where(f => string.IsNullOrWhiteSpace(f.Description) ? false : f.Description.StartsWith("Co-Researcher"))
                                                .OrderByDescending(f => f.Created)
                                                .Select(c => new AttachedFile
                                                {
                                                    FileId = c.Id.ToString(),
                                                    Type = (FileType)c.Type,
                                                    FileName = Path.GetFileName(c.FileName),
                                                    CreatedUserName = WebUtils.GetUserNameForFileUploads(c.Description ?? "", c.CreatedByUser),
                                                    CreatedUserRole = WebUtils.GetUserRoleForFileUploads(c.Description ?? "", c.CreatedByUser)
                                                })
                                                .ToList();
            return View(model);
        }

        //private string GetCoresearcherName(string Description, string coResearcherName)
        //{
        //    if (Description.StartsWith("Co-Researcher"))
        //    {
        //        var names = Description.Split('=');
        //        var userName = names[1];
        //        if (!string.IsNullOrWhiteSpace(userName))
        //        {
        //            var userId = 0;
        //            int.TryParse(userName, out userId);
        //            var coresearcher = DbContext.RequestCoresearchers.Where(x => x.Id == userId).FirstOrDefault();
        //            if (coresearcher != null)
        //            {
        //                coResearcherName = coresearcher.Researcher.GetFullName();
        //                return coResearcherName;
        //            }
        //        }
        //    }

        //    return coResearcherName;
        //}

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CoResearch(CoResearcherMessageViewModel model)
        {
            var File = Request.Files["FileUpload"];

            string key = model.Key;
            if (model.SelectedAction != null)
            {
                if (model.SelectedAction == "Reject")
                {
                    model.IsAccepted = false;
                }
                else
                {
                    model.IsAccepted = true;
                }
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                Response.Write(ApsStrings.Invalidkey);
                Response.End();
                return null;
            }

            var coresearcher = Utils.db.RequestCoresearchers
                .Include("Researcher")
                .Include("Request")
                .Include("Request.RequestCoresearchers")
                .OrderByDescending(t => t.Updated)
                .FirstOrDefault(t => t.Key == key);

            if (coresearcher == null)
            {
                Response.Write(ApsStrings.InvalidLink);
                Response.End();
                return null;
            }

            if (coresearcher.IsKeyUsed)
            {
                Response.Write(ApsStrings.LinkisAlreadyused);
                Response.End();
                return null;
            }

            Session["KEY"] = key;
            Session["RESEARCHER_ID"] = coresearcher.Id;

            var request = (Request)null;
            var CoResearcherRequest = coresearcher;

            if (CoResearcherRequest == null || CoResearcherRequest.Request == null)
            {
                Response.Write(ApsStrings.InvalidLink);
                Response.End();
                return null;
            }

            request = CoResearcherRequest.Request;

            model.RequestId = request.Id;
            model.RequestStatus = request.Status;
            if (request.BookRequest != null)
            {
                model.Title = request.BookRequest.BookTitle;
                model.type = request.BookRequest.BookType;
                model.MajorAuthorId = request.BookRequest.MajorId;
                model.Name = coresearcher.Researcher.FirstName + "" + coresearcher.Researcher.SecondName + " " + coresearcher.Researcher.ThirdName + " " + coresearcher.Researcher.LastName;
                model.mobileNo = coresearcher.Researcher.MobileNo;
                model.Email = coresearcher.Researcher.Email;
                model.OriginalTitle = request.BookRequest.OriginalTitleBook;
                model.ScientificType = "Book";
                model.PublisherName = request.BookRequest.Publisher;
                model.PublisherDate = request.BookRequest.PublishingDate;
                model.PublishType = request.BookRequest.PublishType;
                model.IsArabic = request.BookRequest.IsArabic;
                model.TranslatedTo = request.BookRequest.TranslatedTo == null ? "لايوجد" : request.BookRequest.TranslatedTo;
                model.Key = coresearcher.Key;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.BookRequest.MajorId).ToList();
            }
            else if (request.HandbookRequest != null)
            {
                model.Title = request.HandbookRequest.HandbookTitle;
                model.type = request.HandbookRequest.HandbookType;
                model.MajorAuthorId = request.HandbookRequest.MajorId;
                model.Name = coresearcher.Researcher.FirstName + "" + coresearcher.Researcher.SecondName + " " + coresearcher.Researcher.ThirdName + " " + coresearcher.Researcher.LastName;
                model.mobileNo = coresearcher.Researcher.MobileNo;
                model.Email = coresearcher.Researcher.Email;
                model.OriginalTitle = request.HandbookRequest.OriginalHandbookTitle;
                model.ScientificType = "Handbook";
                model.PublisherName = request.HandbookRequest.Publisher;
                model.PublisherDate = request.HandbookRequest.PublisherDate;
                model.PublishType = request.HandbookRequest.PublishType;
                model.IsArabic = request.HandbookRequest.IsArabic;
                model.TranslatedTo = request.HandbookRequest.TranslatedTo == null ? "لايوجد" : request.HandbookRequest.TranslatedTo;
                model.Key = coresearcher.Key;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.HandbookRequest.MajorId).ToList();
            }
            else if (request.ReviewRequest != null)
            {
                model.Title = request.ReviewRequest.BookTitle;
                model.AuthersName = request.ReviewRequest.AuthersName;
                model.ScopOfManuscript = request.ReviewRequest.ScopOfManuscript;
                model.Name = coresearcher.Researcher.FirstName + "" + coresearcher.Researcher.SecondName + " " + coresearcher.Researcher.ThirdName + " " + coresearcher.Researcher.LastName;
                model.mobileNo = coresearcher.Researcher.MobileNo;
                model.Email = coresearcher.Researcher.Email;
                model.type = request.ReviewRequest.ReviewType;
                model.ScientificType = "Review";
                model.MajorId = request.ReviewRequest.ReviewMajor;
                model.PublisherDate = request.ReviewRequest.PublisherDate;
                model.PublishType = request.ReviewRequest.PublishType;
                model.Key = coresearcher.Key;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.ReviewRequest.ReviewMajor).ToList();
            }
            else if (request.EncyclopediaRequest != null)
            {
                model.Title = request.EncyclopediaRequest.EncyclopediaTitle;
                model.MajorId = request.EncyclopediaRequest.MajorOfEncyclopedia;
                model.MajorAuthorId = request.EncyclopediaRequest.MajorOfAuther;
                model.Name = coresearcher.Researcher.FirstName + "" + coresearcher.Researcher.SecondName + " " + coresearcher.Researcher.ThirdName + " " + coresearcher.Researcher.LastName;
                model.mobileNo = coresearcher.Researcher.MobileNo;
                model.Email = coresearcher.Researcher.Email;
                model.OriginalTitle = request.EncyclopediaRequest.OriginalEncyclopediaTitle;
                model.ScientificType = "Encyclopedia";
                model.NoOfRecommendations = request.EncyclopediaRequest.NoOfRecommendations;
                model.RecommendationDate = request.EncyclopediaRequest.RecommendationDate.Date.ToString();
                model.PublisherName = request.EncyclopediaRequest.Publisher;
                model.PublisherDate = request.EncyclopediaRequest.PublisherDate;
                model.PublishType = request.EncyclopediaRequest.PublishType;
                model.IsArabic = request.EncyclopediaRequest.IsArabic;
                model.TranslatedTo = request.EncyclopediaRequest.TranslatedTo == null ? "لايوجد" : request.EncyclopediaRequest.TranslatedTo;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.EncyclopediaRequest.MajorOfAuther).ToList();
                ViewBag.Major = Utils.db.Majors.Where(e => e.Id == request.EncyclopediaRequest.MajorOfEncyclopedia).ToList();
                model.Key = coresearcher.Key;
            }
            else if (request.DictionaryRequest != null)
            {
                model.Title = request.DictionaryRequest.DictionaryTitle;
                model.MajorId = request.DictionaryRequest.DictionaryMajor;
                model.MajorAuthorId = request.DictionaryRequest.MajorOfAuther;
                model.Name = coresearcher.Researcher.FirstName + "" + coresearcher.Researcher.SecondName + " " + coresearcher.Researcher.ThirdName + " " + coresearcher.Researcher.LastName;
                model.mobileNo = coresearcher.Researcher.MobileNo;
                model.Email = coresearcher.Researcher.Email;
                model.OriginalTitle = request.DictionaryRequest.OriginalDictionaryTitle;
                model.ScientificType = "Dictionary";
                model.NoOfRecommendations = request.DictionaryRequest.NoOfRecommendations;
                model.RecommendationDate = request.DictionaryRequest.RecommendationDate.Date.ToString();
                model.PublisherName = request.DictionaryRequest.Publisher;
                model.PublisherDate = request.DictionaryRequest.PublisherDate;
                model.PublishType = request.DictionaryRequest.PublishType;
                model.IsArabic = request.DictionaryRequest.IsArabic;
                model.TranslatedTo = request.DictionaryRequest.TranslatedTo == null ? "لايوجد" : request.DictionaryRequest.TranslatedTo;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.DictionaryRequest.MajorOfAuther).ToList();
                ViewBag.Major = Utils.db.Majors.Where(e => e.Id == request.DictionaryRequest.DictionaryMajor).ToList();

                model.Key = coresearcher.Key;
            }
            else if (request.ThesisRequest != null)
            {
                model.Title = request.ThesisRequest.ThesisTitle;
                model.MajorAuthorId = request.ThesisRequest.SciDepartmentId;
                model.Name = coresearcher.Researcher.FirstName + "" + coresearcher.Researcher.SecondName + " " + coresearcher.Researcher.ThirdName + " " + coresearcher.Researcher.LastName;
                model.mobileNo = coresearcher.Researcher.MobileNo;
                model.Email = coresearcher.Researcher.Email;
                model.CommitteeMembers = request.ThesisRequest.CommitteeMembers;
                model.ScientificType = "Thesis";
                model.Supervisor = request.ThesisRequest.Supervisor;
                model.Supervisor = request.ThesisRequest.AssistantSupervisor;
                model.DateOfDiscussion = request.ThesisRequest.DateOfDiscussion;
                model.Degree = request.ThesisRequest.Degree;
                model.PublisherName = request.ThesisRequest.Publisher;
                model.PublisherDate = request.ThesisRequest.PublisherDate;
                model.PublishType = request.ThesisRequest.PublishType;
                model.Gpa = request.ThesisRequest.Gpa;
                model.Key = coresearcher.Key;
                ViewBag.AutherMajors = Utils.db.Majors.Where(e => e.Id == request.ThesisRequest.MajorId).ToList();
            }

            if (model.IsAccepted == null)
            {
                ModelState.AddModelError("IsAccepted", "يجب الموافقة او الرفض");
                //return View(model);
            }

            if (ModelState.IsValid)
            {
                var action = (byte)RequestAction.Unspecified;
                var historyType = (byte)RequestHistoryTypes.Unspecificed;
                if (model.IsAccepted.Value)
                {
                    coresearcher.DidResearcherAgree = true;
                    coresearcher.IsKeyUsed = true;
                    action = (byte)RequestAction.Approve;
                    historyType = (byte)RequestHistoryTypes.ApprovedByCoresearcher;
                }
                else
                {
                    coresearcher.DidResearcherAgree = false;
                    coresearcher.IsKeyUsed = true;
                    action = (byte)RequestAction.Reject;
                    historyType = (byte)RequestHistoryTypes.RejectByCoResearcher;
                }
                Utils.db.Entry(coresearcher).State = EntityState.Modified;
                Utils.db.SaveChanges();

                var isAllResponded = true;
                var isAllAccepted = true;

                foreach (var r in coresearcher.Request.RequestCoresearchers)
                {
                    if (r.DidResearcherAgree != true)
                    {
                        isAllAccepted = false;
                    }

                    if (!r.IsKeyUsed)
                    {
                        isAllResponded = false;
                    }
                }

                if (isAllAccepted)
                {
                    if (request.Status == (long)RequestStatus.PublishRequestPendingCoresearchersAfterRefereesModified || request.Status == (long)RequestStatus.PublishRequestPendingCoresearchersAfterModified)
                    {
                        request.Status = (long)RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModifications;
                    }
                    else
                    {
                        request.Status = (byte)RequestStatus.PublishRequestPendingResearchUnitSecretary;
                        var sec = Utils.db.Users.OrderByDescending(u => u.Updated).FirstOrDefault(u => u.Roles.Count(r => r.Name == ApsUserRole.ResearchUnitHead.ToString()) > 0);

                        if (sec != null)
                            request.CurrentlyWith = sec.Id;

                        if (request.CurrentlyWithUser != null)
                            Utils.db.Entry(request.CurrentlyWithUser).State = EntityState.Detached;
                    }

                    Utils.db.Entry(request).State = EntityState.Modified;
                    Utils.db.SaveChanges();
                }
                else if (isAllResponded)
                {
                    if (request.Status == (long)RequestStatus.PublishRequestPendingCoresearchersAfterRefereesModified || request.Status == (long)RequestStatus.PublishRequestPendingCoresearchersAfterModified)
                    {
                        request.Status = (long)RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified;
                    }
                    else
                    {
                        request.Status = (byte)RequestStatus.PublishRequestRejectedByCoresearchers;
                        request.CurrentlyWith = request.CreatedBy;

                        if (request.CurrentlyWithUser != null)
                            Utils.db.Entry(request.CurrentlyWithUser).State = EntityState.Detached;
                    }

                    Utils.db.Entry(request).State = EntityState.Modified;
                    Utils.db.SaveChanges();
                }
                // File Upload

                if (File != null && File.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(File.FileName);
                    var extension = Path.GetExtension(fileName);

                    var f = new Data.DbModels.RequestFile
                    {
                        Id = Guid.NewGuid(),
                        Type = (byte)FileType.CoResearcherOpinion,
                        CreatedBy = Utils.CurrentUser == null ? "00000000-0000-0000-0000-000000000000" : Utils.CurrentUser.Id,
                        FileName = Path.GetFileName(File.FileName),
                        Extension = extension,
                        RequestId = request.Id,
                        Description = "Co-Researcher file with researcher Id = " + coresearcher.ResearcherId,
                        Created = DateTime.Now
                    };

                    f.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), f.Id + extension);
                    File.EnhancedSaveAs(f.FilePath);

                    Utils.db.RequestFiles.Add(f);
                }

                // History Logs
                Utils.db.RequestHistoryLogs.Add(new RequestHistoryLog
                {
                    Created = DateTime.Now,
                    CreatedBy = "00000000-0000-0000-0000-000000000000",
                    Url = Utils.GetFullUrl(),
                    IpAddress = Utils.GetUserIP(),
                    RequestId = request.Id,
                    UserAgent = HttpContextManager.Current.Request.UserAgent,
                    Action = action,
                    Status = request.Status,
                    Type = historyType,
                    Comments = "[Coresearcher]Responded by :" + coresearcher.ResearcherId
                });

                Utils.db.SaveChanges();

                return Content("تم الإرسال بنجاح");
            }
            return View(model);
        }
    }
}