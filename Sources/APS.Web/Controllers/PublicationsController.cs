using APS.Core;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using APS.TimeMachine;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    public class PublicationsController : BaseController
    {
        private ApsDbEntities db = Utils.db;
        private List<SelectListItem> lstSelect = new List<SelectListItem>();

        public PublicationsController()
        {
            lstSelect.Add(new SelectListItem { Text = ApsStrings.yes, Value = "1" });
            lstSelect.Add(new SelectListItem { Text = ApsStrings.no, Value = "0" });
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create(long? requestId)
        {
            var model = new PublishOrderViewModel();

            if (requestId > 0)
            {
                var req = db.Requests
                    .Include("CreatedByUser")
                    .Include("CreatedByUser.Researchers")
                    .Where(e => e.Id == requestId && e.CreatedBy == CurrentUser.Id)
                    .FirstOrDefault();

                if (req == null)
                {
                    TempData["message"] = "لا يوجد طلب بهذا الرقم";
                    TempData["message_type"] = "danger";
                    return RedirectToAction("Index", "Home");
                }

                if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
                {
                    if (((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                        && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                    {
                        return RedirectToAction("View", "Requests", new { id = req.Id });
                    }
                }
                model.DepartmentId = 0;
                model.CollegeId = 0;
                if (req.CreatedByUser.Researchers.Count > 0)
                {
                    model.ResearcherName = req.CreatedByUser.Researchers.First().GetFullName();
                    model.MajorId = req.CreatedByUser.Researchers.First().MajorId;
                    //model.CollegeId = req.CreatedByUser.Researchers.First().CollegeId;
                    model.ScientificMaterialTypeId = ((PublicationType)req.PublicationType).ToString();
                    //model.DepartmentId = req.CreatedByUser.Researchers.First().DepartmentId;
                    model.DepartmentId = req.DepartmentId ?? 0;
                    model.CollegeId = req.CollegeId ?? 0;
                    switch ((RequestType)req.Type)
                    {
                        case RequestType.PublishRequest:
                            model.PublishType = PublishTimeType.NewPublish;
                            break;

                        case RequestType.RepublishRequest:
                            model.PublishType = PublishTimeType.Republish;
                            break;
                    }
                }
                //if (model.CollegeId == null)
                //{
                //    model.CollegeId = req.CollegeId;
                //}
            }
            else
            {
                var user = db.Users.Include("Researchers").Include("UserProfile").FirstOrDefault(u => u.Id == CurrentUser.Id);

                if (user.Researchers.Count > 0)
                {
                    model.ResearcherName = user.Researchers.FirstOrDefault().GetFullName();
                    // model.CollegeId = user.Researchers.FirstOrDefault().CollegeId;
                    //model.DepartmentId = user.Researchers.FirstOrDefault().DepartmentId;
                    model.MajorId = user.Researchers.FirstOrDefault().MajorId;
                }
                else
                {
                    if (user.UserProfile != null)
                        model.ResearcherName = user.UserProfile.GetFullName();
                    // model.CollegeId = user.CollegeId;
                    model.MajorId = user.MajorId;
                    //model.DepartmentId = user.DepartmentId;
                }
                if (user.UserProfile != null)
                    model.ResearcherName = user.UserProfile.GetFullName();
            }
            ViewBag.RequestId = model.RequestId = requestId;
            ViewBag.Colleges = db.Colleges.OrderBy(r => r.NameArabic).ToList();
            ViewBag.Majors = db.Majors.Where(x => false).ToList();
            ViewBag.TranslatedTo = db.TranslatedToLanguages.Where(x => false).ToList();
            var list = DbContext.Departments
                            .Where(c => c.CollegeId == model.CollegeId)
                            .ToArray()
                            .Select(c => new SelectListItem
                            {
                                Text = c.LocalizedName(),
                                Value = c.Id.ToString()
                            })
                            .OrderBy(c => c.Text)
                            .ToList();
            ViewBag.Departments = list.ToArray();

            var prevRequests = db.Requests.Where(r => r.Type == (byte)Data.RequestType.PublishRequest && r.CreatedBy == CurrentUser.Id).ToList();
            model.PreviousPublishRequests = new SelectList(prevRequests, "Id", "Title", null);

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(PublishOrderViewModel model)
        {
            var req = (Request)null;

            if (model.RequestId > 0)
            {
                req = db.Requests.Where(e => e.Id == model.RequestId.Value && e.CreatedBy == CurrentUser.Id).FirstOrDefault();
                if (req == null)
                {
                    TempData["message"] = "لا يوجد طلب بهذا الرقم";
                    TempData["message_type"] = "danger";
                    return RedirectToAction("Index", "Home");
                }

                if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
                {
                    if (((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                         && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                    {
                        return RedirectToAction("View", "Requests", new { id = req.Id });
                    }
                }
            }

            var pubtype = PublicationType.Unspecificed;
            if (string.IsNullOrWhiteSpace(model.ScientificMaterialTypeId) || model.ScientificMaterialTypeId == "0")
            {
                ModelState.AddModelError("ScientificMaterialTypeId", ApsStrings.scientific_material_type_required);
            }
            else
            {
                if (model.ScientificMaterialTypeId == PublicationType.Book.ToString())
                {
                    pubtype = PublicationType.Book;
                }
                else if (model.ScientificMaterialTypeId == PublicationType.Handbook.ToString())
                {
                    pubtype = PublicationType.Handbook;
                }
                else if (model.ScientificMaterialTypeId == PublicationType.Thesis.ToString())
                {
                    pubtype = PublicationType.Thesis;
                }
                else if (model.ScientificMaterialTypeId == PublicationType.Probe.ToString())
                {
                    pubtype = PublicationType.Probe;
                }
                else if (model.ScientificMaterialTypeId == PublicationType.Encyclopedia.ToString())
                {
                    pubtype = PublicationType.Encyclopedia;
                }
                else if (model.ScientificMaterialTypeId == PublicationType.Dictionary.ToString())
                {
                    pubtype = PublicationType.Dictionary;
                }
                if (pubtype == PublicationType.Unspecificed)
                {
                    ModelState.AddModelError("ScientificMaterialTypeId", ApsStrings.scientific_material_type_required);
                }
            }

            if (model.PublishType == PublishTimeType.Republish)
            {
                if (model.PublishRequestId == null)
                {
                    ModelState.AddModelError("PublishRequestId", ApsStrings.select_publish_request);
                }
            }
            else
            {
                model.PublishRequestId = null;
            }

            if (model.CollegeId == null || model.CollegeId <= 0)
            {
                ModelState.AddModelError("CollegeId", ApsStrings.faculty_required);
            }

            if (model.DepartmentId == null || model.DepartmentId <= 0)
            {
                ModelState.AddModelError("DepartmentId", ApsStrings.request_create_department_required);
            }

            if (ModelState.IsValid)
            {
                bool redirectToRequestPage = false;
                var request = req;
                if (request != null)
                {
                    if (request.Status == (long)RequestStatus.PublishRequestPendingModificationsAfterReferees)
                    {
                        (new RefereeUtil()).CreateRequestRevision(request.Id, false);
                        //redirectToRequestPage = true;
                        req.Status = (long)APS.Data.RequestStatus.PublishRequestPendingModifiedAfterReferees;
                    }
                    else if (request.Status == (long)RequestStatus.PublishRequestPendingModificationsAfterRUS)
                    {
                        redirectToRequestPage = true;
                    }
                }

                var name = Utils.GetNameComponents(model.ResearcherName);

                var Researcher = db.Researchers.Where(e => e.UserId == Utils.CurrentUser.Id).ToList();

                if (Researcher.Count == 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.MajorOther))
                    {
                        var user = Utils.GetCurrentUserDetails(DbContext);
                        user.DbUser.MajorOther = model.MajorOther;
                    }
                    try
                    {
                        db.Researchers.Add(new Data.DbModels.Researcher
                        {
                            FirstName = name.FirstName,
                            SecondName = name.SecondName,
                            ThirdName = name.ThirdName,
                            LastName = name.LastName,
                            CollegeId = model.CollegeId,
                            Email = "",
                            MobileNo = "",
                            DateOfBirth = TM.Now,
                            Address = "",
                            MajorId = model.MajorId,
                            CreatedBy = Utils.CurrentUser.Id,
                            UpdatedBy = Utils.CurrentUser.Id,
                            Created = TM.Now,
                            Updated = TM.Now,
                            UserId = Utils.CurrentUser.Id,
                            DepartmentId = model.DepartmentId
                        });
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                        string error = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                    }
                }
                else
                {
                    Researcher.First().FirstName = name.FirstName;
                    Researcher.First().SecondName = name.SecondName;
                    Researcher.First().ThirdName = name.ThirdName;
                    Researcher.First().LastName = name.LastName;
                    Researcher.First().MajorId = model.MajorId;
                    //Researcher.First().CollegeId = model.CollegeId;
                    //Researcher.First().DepartmentId = model.DepartmentId;
                    db.Entry(Researcher.First()).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                if (req == null)
                {
                    req = db.Requests.Add(new Data.DbModels.Request
                    {
                        PublicationRequestId = model.PublishRequestId,
                        CollegeId = model.CollegeId.Value,
                        DepartmentId = model.DepartmentId.Value,
                        Created = DateTime.Now,
                        CreatedBy = Utils.CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = Utils.CurrentUser.Id,
                        Status = (long)APS.Data.RequestStatus.PublishRequestNotSubmitted,
                        PublicationType = (byte)pubtype,
                        Type = model.PublishType == PublishTimeType.NewPublish ? (byte)APS.Data.RequestType.PublishRequest : (byte)APS.Data.RequestType.RepublishRequest,
                    });
                    db.SaveChanges();
                }
                else
                {
                    req.Type = model.PublishType == PublishTimeType.NewPublish ? (byte)APS.Data.RequestType.PublishRequest : (byte)APS.Data.RequestType.RepublishRequest;
                    req.PublicationRequestId = model.PublishRequestId;
                    req.PublicationType = (byte)pubtype;
                    req.CollegeId = model.CollegeId.Value;
                    req.DepartmentId = model.DepartmentId.Value;
                    req.Updated = TM.Now;
                    req.UpdatedBy = CurrentUser.Id;
                    db.Entry(req).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                db.RequestHistoryLogs.Add(new Data.DbModels.RequestHistoryLog
                {
                    Created = TM.Now,
                    CreatedBy = Utils.CurrentUser.Id,
                    Url = Utils.GetFullUrl(),
                    IpAddress = Utils.GetUserIP(),
                    RequestId = req.Id,
                    UserAgent = HttpContextManager.Current.Request.UserAgent,
                    Status = (long)APS.Data.RequestStatus.PublishRequestNotSubmitted,
                    Type = (byte)RequestHistoryTypes.RequestCreated,
                });
                db.SaveChanges();
                TempData["message"] = "تمت إضافة البيانات الاساسية للطلب بنجاح";
                TempData["message_type"] = "success";
                if (redirectToRequestPage)
                {
                    return Redirect(Utils.GetApplicationUrl() + "Requests/View/" + request.Id);
                }
                else
                {
                    return RedirectToAction("PublicationDetails", new { requestid = req.Id, RequestType = model.PublishType.ToString() });
                }
            }
            ViewBag.RequestId = model.RequestId;
            ViewBag.Colleges = db.Colleges.ToList();
            var list = DbContext.Departments
                            .Where(c => c.CollegeId == model.CollegeId)
                            .ToArray()
                            .Select(c => new SelectListItem
                            {
                                Text = c.LocalizedName(),
                                Value = c.Id.ToString()
                            })
                            .OrderBy(c => c.Text)
                            .ToList();
            ViewBag.Departments = list.ToArray();
            ViewBag.Majors = db.Majors.Where(x => model.CollegeId == x.Id).ToList();
            ViewBag.TranslatedTo = db.TranslatedToLanguages.Where(x => model.TranslatedToId == x.Id).ToList();

            var prevRequests = db.Requests.Where(r => r.Type == (byte)Data.RequestType.PublishRequest && r.CreatedBy == CurrentUser.Id).ToList();
            model.PreviousPublishRequests = new SelectList(prevRequests, "Id", "Title", null);

            return View(model);
        }

        [HttpGet]
        public ActionResult PublicationDetails(long? requestId)
        {
            var model = (PublicationViewModel)null;
            //var request = db.Requests.Where(x => x.Id == requestId.Value).FirstOrDefault();
            //var finalRequestId = requestId;
            //if(request!=null)
            //{
            //    if(request.Status==(long)RequestStatus.PublishRequestPendingModifiedAfterReferees)
            //    {
            //        finalRequestId = db.Requests.Where(x => x.RequestRevisionId == request.Id).Select(x=>x.Id).SingleOrDefault();
            //    }
            //}
            var res = CommonGet(requestId, ref model);
            if (res != null) return res;
            var viewName = model.PublicationType.ToString() + "Details";
            return View(viewName, model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult PublicationDetails(PublicationViewModel model)
        {
            var res = CommonPost(ref model);
            if (res != null) return res;
            var viewName = model.PublicationType.ToString() + "Details";
            return View(viewName, model);
        }

        public ActionResult BankDetails(int? requestid)
        {
            var req = db.Requests
                .Include("CreatedByUser")
                .Include("CreatedByUser.Researchers")
                .FirstOrDefault(e => e.Id == requestid && e.CreatedBy == CurrentUser.Id);
            if (req == null)
            {
                TempData["message"] = "هذا الطلب غير صالح";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Home");
            }
            if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
            {
                if (((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModifiedAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                    && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                {
                    return RedirectToAction("View", "Requests", new { id = req.Id });
                }
            }

            var model = new BankViewModel();

            var researcher = req.CreatedByUser.UserProfile;
            if (researcher != null)
            {
                model.BankId = researcher.BankId ?? 0;
                model.BankOther = researcher.BankName;
                model.IbanNo = researcher.BankIban;
                model.AccountOwnerName = researcher.BankAccountHolder;
                model.CountryId = researcher.NationalityId;
                model.CityId = researcher.CityId;
                model.BankAccountTypeId = researcher.BankAccountTypeId;
                model.BankAccountNumberTypeId = researcher.BankAccountNumberTypeId;
                model.SwiftCode = researcher.SwiftCode;
                model.RoutingNumber = researcher.RoutingNumber;
            }

            model.PublicationType = (PublicationType)req.PublicationType;
            if (req.NoOfParticipant > 0)
                model.HasParticipant = true;
            model.NoOfParticipant = req.NoOfParticipant;
            model.IsScientificProduction = req.IsScientificProduction == null ? 0 : req.IsScientificProduction.Value;
            model.RequestId = req.Id;
            ViewBag.Countries = new SelectList(DbContext.Countries
                .OrderBy(c => c.NameArabic == "المملكة العربية السعودية" ? 1 : 2)
                .ThenBy(c => c.NameArabic), "Id", "NameArabic");

            var countryId = researcher == null ? 0 : researcher.NationalityId;

            ViewBag.Cities = db.Cities.Where(r => countryId == r.CountryId).ToList();
            ViewBag.SelectYN = lstSelect;

            ViewBag.RequestId = model.RequestId = requestid;
            ViewBag.Majors = db.Majors.ToList();
            ViewBag.TranslatedTo = db.TranslatedToLanguages.ToList();

            var banks = DbContext.Banks
                    .ToArray()
                    .Select(c => new SelectListItem
                    {
                        Text = c.LocalizedName(),
                        Value = c.Id.ToString()
                    })
                    .OrderBy(c => c.Text)
                    .ToList();

            banks.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });

            ViewBag.Banks = banks.ToArray();

            //       ViewBag.Banks = db.Banks.ToList();

            model.Type = (RequestType)req.Type;
            model.PublicationType = (PublicationType)req.PublicationType;

            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("iban", false, null), Value = "1" });
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("swift", false, null), Value = "2" });
                //list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });
                model.BankAccountNumberTypes = list.ToArray();
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult BankDetails(BankViewModel model)
        {
            var req = db.Requests.FirstOrDefault(e => e.Id == model.RequestId && e.CreatedBy == CurrentUser.Id);

            if (req == null)
            {
                TempData["message"] = "هذا الطلب غير صالح";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Home");
            }

            if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
            {
                if (((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModifiedAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                    && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                {
                    return RedirectToAction("View", "Requests", new { id = req.Id });
                }
            }
            model.RequestId = req.Id;
            var fileType = (byte)FileType.IbanPhotocopy;
            if (!(model.BankId > 0) && string.IsNullOrWhiteSpace(model.BankOther))
            {
                ModelState.AddModelError("BankId", ApsStrings.bank_required);
            }

            if (req.PublicationType == (byte)PublicationType.Thesis)
            {
                model.HasParticipant = false;
                model.NoOfParticipant = 0;
            }
            else
            {
                if (model.HasParticipant == true && model.NoOfParticipant <= 0)
                {
                    ModelState.AddModelError("NoOfParticipant", "يجب إدخال عدد الباحثين المشاركين");
                }
            }
            if (model.BankAccountNumberTypeId == (byte)BankAccountNumberType.Iban)
            {
                fileType = (byte)FileType.IbanPhotocopy;
                if (string.IsNullOrWhiteSpace(model.IbanNo))
                {
                    ModelState.AddModelError("IbanNo", "رقم IBAN مطلوب");
                }
                else if (!Utils.ValidateIBAN(model.IbanNo))
                {
                    ModelState.AddModelError("IbanNo", "رقم IBAN غير صالح");
                }
            }
            else if (model.BankAccountNumberTypeId == (byte)BankAccountNumberType.Swift)
            {
                fileType = (byte)FileType.SwiftPhotocopy;
                if (string.IsNullOrWhiteSpace(model.SwiftCode))
                {
                    ModelState.AddModelError("SwiftCode", LangHelpers.Instance["swift_required"]);
                }
                else if (!Utils.ValidateSWIFT(model.SwiftCode))
                {
                    ModelState.AddModelError("SwiftCode", LangHelpers.Instance["invalid_swift"]);
                }
            }

            if (ModelState.IsValid)
            {
                var File = Request.Files["FileUpload"];

                if (File == null || File.ContentLength == 0)
                {
                    ModelState.AddModelError("FileUpload", ApsStrings.attached_file_thes);
                }

                if (model.NoOfParticipant == null)
                    model.NoOfParticipant = 0;

                req.NoOfParticipant = model.NoOfParticipant;
                req.IsScientificProduction = model.IsScientificProduction;
                db.Entry(req).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var user = DbContext.Users.FirstOrDefault(u => u.Id == CurrentUser.Id);
                if (user != null && user.UserProfile != null)
                {
                    user.UserProfile.BankId = model.BankId == 0 ? null : model.BankId;
                    user.UserProfile.BankName = model.BankOther;
                    user.UserProfile.BankIban = model.IbanNo;
                    user.UserProfile.BankAccountHolder = model.AccountOwnerName;
                    user.UserProfile.NationalityId = model.CountryId;
                    user.UserProfile.CityId = model.CityId;

                    user.UserProfile.BankAccountTypeId = model.BankAccountTypeId;
                    user.UserProfile.BankAccountNumberTypeId = model.BankAccountNumberTypeId;
                    user.UserProfile.SwiftCode = model.SwiftCode;
                    user.UserProfile.RoutingNumber = model.RoutingNumber;

                    db.SaveChanges();
                }
                if (File != null && File.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(File.FileName);
                    var extension = Path.GetExtension(fileName);

                    var f = new Data.DbModels.RequestFile
                    {
                        Id = Guid.NewGuid(),
                        Type = (byte)FileType.Other,
                        CreatedBy = Utils.CurrentUser.Id,
                        FileName = Path.GetFileName(File.FileName),
                        Extension = extension,
                        RequestId = model.RequestId.Value,
                        Description = "thesis request file",
                        Created = DateTime.Now
                    };

                    f.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), f.Id + extension);
                    File.EnhancedSaveAs(f.FilePath);

                    db.RequestFiles.Add(f);
                }
                db.SaveChanges();

                TempData["message"] = "تمت إضافة البيانات البنكية بنجاح";
                TempData["message_type"] = "success";

                if (model.HasParticipant == true && model.NoOfParticipant > 0)
                {
                    return RedirectToAction("Participant", new { requestid = model.RequestId, NoOfParticipant = model.NoOfParticipant });
                }
                else
                {
                    return RedirectToAction("Agreement", new { requestid = model.RequestId });
                }
            }

            ViewBag.RequestId = model.RequestId = req.Id;
            ViewBag.Majors = db.Majors.ToList();
            ViewBag.TranslatedTo = db.TranslatedToLanguages.ToList();
            ViewBag.Banks = db.Banks.ToList();

            ViewBag.Countries = new SelectList(DbContext.Countries
                .OrderBy(c => c.NameArabic == "المملكة العربية السعودية" ? 1 : 2)
                .ThenBy(c => c.NameArabic), "Id", "NameArabic")
                .ToList();

            var researcher = req.CreatedByUser.UserProfile;

            if (researcher != null)
            {
                model.BankId = researcher.BankId;
                model.BankOther = researcher.BankName;
                model.IbanNo = researcher.BankIban;
                model.AccountOwnerName = researcher.BankAccountHolder;
                model.CountryId = researcher.NationalityId;
                model.CityId = researcher.CityId;
            }

            var countryId = researcher == null ? 0 : researcher.NationalityId;

            ViewBag.Cities = db.Cities.Where(r => countryId == r.CountryId).ToList();
            ViewBag.SelectYN = lstSelect;

            model.Type = (RequestType)req.Type;
            model.PublicationType = (PublicationType)req.PublicationType;
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("iban", false, null), Value = "1" });
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("swift", false, null), Value = "2" });
                //list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });
                model.BankAccountNumberTypes = list.ToArray();
            }
            return View(model);
        }

        public ActionResult Participant(long? requestid)
        {
            var req = db.Requests.Include("RequestCoresearchers").Include("RequestCoresearchers.Researcher").FirstOrDefault(e => e.Id == requestid && e.CreatedBy == CurrentUser.Id);

            if (req == null)
            {
                TempData["message"] = "هذا الطلب غير صالح";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Home");
            }

            if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
            {
                if (((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModifiedAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                    && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                {
                    return RedirectToAction("View", "Requests", new { id = req.Id });
                }
            }

            var model = new ParticipantDataViewModel();

            if (req.RequestCoresearchers != null && req.RequestCoresearchers.Count > 0)
            {
                model.ParticipantDetail = new List<SingleParticipantDataViewModel>();

                foreach (var coresearcher in req.RequestCoresearchers)
                {
                    model.ParticipantDetail.Add(new SingleParticipantDataViewModel
                    {
                        ParticipantName = coresearcher.Researcher.GetFullName(),
                        ParticipantEmail = coresearcher.Researcher.Email,
                        ParticipanMobileNumber = coresearcher.Researcher.MobileNo
                    });
                }
                if (req.NoOfParticipant > req.RequestCoresearchers.Count)
                {
                    for (var i = 0; i < (req.NoOfParticipant - req.RequestCoresearchers.Count); i++)
                    {
                        model.ParticipantDetail.Add(new SingleParticipantDataViewModel());
                    }
                }
            }
            else
            {
                model.ParticipantDetail = new List<SingleParticipantDataViewModel>();

                for (var i = 0; i < req.NoOfParticipant; i++)
                {
                    model.ParticipantDetail.Add(new SingleParticipantDataViewModel());
                }
            }

            ViewBag.RequestId = model.RequestId = requestid;
            ViewBag.NoOfParticipant = req.NoOfParticipant;

            model.Type = (RequestType)req.Type;
            model.PublicationType = (PublicationType)req.PublicationType;

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Participant(ParticipantDataViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var req = db.Requests.Include("RequestCoresearchers").FirstOrDefault(e => e.Id == model.RequestId && e.CreatedBy == CurrentUser.Id);

            if (req == null)
            {
                TempData["message"] = "هذا الطلب غير صالح";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Home");
            }

            if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
            {
                if (((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModifiedAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                    && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                {
                    return RedirectToAction("View", "Requests", new { id = req.Id });
                }
            }

            if (req.NoOfParticipant > 0 && (model.ParticipantDetail == null || model.ParticipantDetail.Count != req.NoOfParticipant))
            {
                ModelState.AddModelError("ParticipantDetail", "يرجى إدخال بيانات جميع المشاركين");
            }
            else if (req.NoOfParticipant > 0)
            {
                var distinctNames = model.ParticipantDetail
                                        .Where(p => !string.IsNullOrWhiteSpace(p.ParticipantName))
                                        .Select(p => (p.ParticipantName == null ? "" : p.ParticipantName).Trim()).Distinct()
                                        .ToList();

                var distinctPhones = model.ParticipantDetail
                                        .Where(p => !string.IsNullOrWhiteSpace(p.ParticipanMobileNumber))
                                        .Select(p => (p.ParticipanMobileNumber == null ? "" : p.ParticipanMobileNumber).Trim()).Distinct()
                                        .ToList();

                var distinctEmails = model.ParticipantDetail
                                        .Where(p => !string.IsNullOrWhiteSpace(p.ParticipantEmail))
                                        .Select(p => (p.ParticipantEmail == null ? "" : p.ParticipantEmail).Trim()).Distinct()
                                        .ToList();

                if (distinctNames.Count != distinctPhones.Count || distinctPhones.Count != distinctEmails.Count)
                {
                    ModelState.AddModelError("ParticipantDetail", "يرجى التأكد من عدم تكرار بيانات الباحثين");
                }
            }
            if (model.ParticipantDetail != null && req.NoOfParticipant == 0)
            {
                return RedirectToAction("Agreement", new { requestid = model.RequestId });
            }
            foreach (var x in model.ParticipantDetail)
            {
                var number = (PhoneNumber)null;
                try
                {
                    var util = PhoneNumberUtil.GetInstance();
                    number = util.Parse(x.ParticipanMobileNumber, "SA");

                    x.ParticipanMobileNumber = number.CountryCode + "" + number.NationalNumber;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ParticipantDetail", "يرجى التأكد من دقة أرقام الجوال للباحثين");
                }
            }

            if (ModelState.IsValid)
            {
                // Delete the older researchers
                foreach (var coRes in db.RequestCoresearchers.Where(y => y.RequestId == req.Id))
                {
                    db.Entry(coRes).State = System.Data.Entity.EntityState.Deleted;
                }
                foreach (var requestFile in db.RequestFiles.Where(y => y.RequestId == req.Id
                && (y.Description != null && y.Description.StartsWith("Co-Researcher file with researcher Id"))
                && y.Type == (byte)FileType.CoResearcherOpinion))
                {
                    db.Entry(requestFile).State = System.Data.Entity.EntityState.Deleted;
                }

                db.SaveChanges();

                foreach (var x in model.ParticipantDetail)
                {
                    var name = Utils.GetNameComponents(x.ParticipantName);

                    var researcher = db.Researchers.FirstOrDefault(u => u.Email == x.ParticipantEmail);

                    if (researcher != null)
                    {
                        //if (researcher.FirstName == null)
                        //{
                        researcher.FirstName = name.FirstName;
                        researcher.SecondName = name.SecondName;
                        researcher.ThirdName = name.ThirdName;
                        researcher.LastName = name.LastName;

                        //}
                        //if (researcher.MobileNo == null)
                        researcher.MobileNo = x.ParticipanMobileNumber;

                        db.SaveChanges();
                    }
                    else
                    {
                        researcher = db.Researchers.Add(new Data.DbModels.Researcher
                        {
                            FirstName = name.FirstName,
                            SecondName = name.SecondName,
                            ThirdName = name.ThirdName,
                            LastName = name.LastName,
                            Email = x.ParticipantEmail,
                            MobileNo = x.ParticipanMobileNumber,
                            UserId = null,
                            CreatedBy = Utils.CurrentUser.Id,
                            UpdatedBy = Utils.CurrentUser.Id,
                            Created = DateTime.Now,
                            Updated = DateTime.Now,
                            DateOfBirth = DateTime.Now,
                            Address = "",
                        });

                        db.SaveChanges();
                    }

                    var coresearcher = (RequestCoresearcher)null;
                    if (req.RequestCoresearchers.Count(y => y.ResearcherId == researcher.Id) > 0)
                    {
                        coresearcher = req.RequestCoresearchers.FirstOrDefault(y => y.ResearcherId == researcher.Id);
                    }
                    else
                    {
                        coresearcher = db.RequestCoresearchers.Add(new RequestCoresearcher
                        {
                            ResearcherId = researcher.Id,
                            RequestId = req.Id,
                            CreatedBy = Utils.CurrentUser.Id,
                            UpdatedBy = Utils.CurrentUser.Id,
                            Created = DateTime.Now,
                            Updated = DateTime.Now,
                        });
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("Agreement", new { requestid = model.RequestId });
            }

            ViewBag.RequestId = req.Id;
            ViewBag.NoOfParticipant = req.NoOfParticipant;

            model.Type = (RequestType)req.Type;
            model.PublicationType = (PublicationType)req.PublicationType;

            return View(model);
        }

        public ActionResult Agreement(long? requestid)
        {
            var req = db.Requests
                          .Include("BookRequest")
                          .Include("RequestFiles")
                          .FirstOrDefault(r => r.Id == requestid.Value && r.CreatedBy == CurrentUser.Id);

            if (req == null)
            {
                TempData["message"] = "هذا الطلب غير صالح";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Home");
            }

            var model = new AgreementViewModel();

            model.Type = (RequestType)req.Type;
            model.PublicationType = (PublicationType)req.PublicationType;

            ViewBag.Majors = db.Majors.ToList();
            ViewBag.TranslatedTo = db.TranslatedToLanguages.ToList();
            ViewBag.RequestId = model.RequestId = requestid;

            switch ((PublicationType)req.PublicationType)
            {
                case PublicationType.Book:
                    model.AgreementText = TemplateUtils[TemplateKey.BookAgreement].BodyArabic;
                    break;

                case PublicationType.Thesis:
                    model.AgreementText = TemplateUtils[TemplateKey.ThesisAgreeement].BodyArabic;
                    break;

                case PublicationType.Probe:
                    model.AgreementText = TemplateUtils[TemplateKey.ProbeAgreement].BodyArabic;
                    break;

                case PublicationType.Dictionary:
                    model.AgreementText = TemplateUtils[TemplateKey.DicationaryAgreeement].BodyArabic;
                    break;

                case PublicationType.Encyclopedia:
                    model.AgreementText = TemplateUtils[TemplateKey.EncyclopediaAgreement].BodyArabic;
                    break;

                case PublicationType.Handbook:
                    model.AgreementText = TemplateUtils[TemplateKey.HandbookAgreement].BodyArabic;
                    break;

                default:
                    TempData["message"] = "هذا الطلب غير صالح";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("Index", "Home");
            }

            model.AgreementText = model.AgreementText
                .Replace("{date_hijri}", DateTime.Now.FormatDate(false, true, true))
                .Replace("{date_greg}", DateTime.Now.FormatDate(false, false, true))
                .Replace("{participants}", req.NoOfParticipant > 0 ? string.Join(ApsStrings.and, req.RequestCoresearchers.Select(x => x.Researcher.GetFullName())) : ApsStrings.no_partcipants);

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Agreement(AgreementViewModel entity)
        {
            bool sent = false;
            var req = db.Requests
                .Include("RequestFiles")
                .Include("CreatedByUser")
                .Include("CreatedByUser.Researchers")
                .Where(e => e.Id == entity.RequestId && e.CreatedBy == CurrentUser.Id).FirstOrDefault();
            /// By default a researcher has to approve the request and then send
            entity.IsApproved = true;
            if (req == null)
            {
                TempData["message"] = "لا يوجد طلب بهذا الرقم";
                TempData["message_type"] = "danger";
                return RedirectToAction("Index", "Home");
            }

            if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
            {
                if (((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModifiedAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                    && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                {
                    return RedirectToAction("View", "Requests", new { id = req.Id });
                }
            }

            if (req != null)
            {
                if (req.NoOfParticipant == null)
                {
                    req.NoOfParticipant = 0;
                }

                if (string.IsNullOrWhiteSpace(req.Title))
                {
                    TempData["message"] = "لم يتم إدخال عنوان البحث";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("PublicationDetails", new { requestId = req.Id });
                }

                if (req.RequestFiles.Count == 0)
                {
                    TempData["message"] = "لم يتم رفع البحث";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("PublicationDetails", new { requestId = req.Id });
                }

                if (req.CreatedByUser.UserProfile == null || (string.IsNullOrWhiteSpace(req.CreatedByUser.UserProfile.BankIban) && string.IsNullOrWhiteSpace(req.CreatedByUser.UserProfile.SwiftCode)))
                {
                    TempData["message"] = "لم يتم إدخال البيانات البنكية";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("BankDetails", new { requestId = req.Id });
                }

                if (entity.IsApproved == null)
                {
                    TempData["message"] = "يجب الموافقة على الإقرار";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("Agreement", new { requestId = req.Id });
                }

                if (entity.IsApproved != null)
                {
                    if (entity.IsApproved.Value)
                    {
                        if (req.Status == (long)RequestStatus.PublishRequestPendingModifiedAfterReferees || req.Status == (long)RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                        {
                            req.Status = req.NoOfParticipant == 0 ? (long)RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModifications : (long)RequestStatus.PublishRequestPendingCoresearchersAfterRefereesModified;
                        }
                        else
                        {
                            req.Status = req.NoOfParticipant == 0 ? (long)APS.Data.RequestStatus.PublishRequestPendingResearchUnitSecretary : (long)APS.Data.RequestStatus.PublishRequestPendingCoresearchers;
                        }
                    }
                    else
                    {
                        TempData["message"] = "لقد قمت برفض الاتفاقية. تم إلغاء الطلب.";
                        TempData["message_type"] = "danger";

                        req.Status = (long)APS.Data.RequestStatus.RejectedByResearcher;
                    }
                }

                db.Entry(req).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                if (req.NoOfParticipant == 0)
                {
                    foreach (var requestFile in db.RequestFiles.Where(y => y.RequestId == req.Id
                && (y.Description != null && y.Description.StartsWith("Co-Researcher file with researcher Id"))
                && y.Type == (byte)FileType.CoResearcherOpinion))
                    {
                        db.Entry(requestFile).State = System.Data.Entity.EntityState.Deleted;
                    }
                }

                db.SaveChanges();

                if (entity.IsApproved == true && req.NoOfParticipant > 0)
                {
                    List<string> Listoflinks = new List<string>();
                    foreach (var coresearcher in req.RequestCoresearchers)
                    {
                        var strHash = Convert.ToBase64String(SHA256CryptoServiceProvider.Create().ComputeHash(Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + coresearcher.Researcher.Id.ToString()))).Replace("+", "").Replace("/", "").Replace("\\", "");
                        if (strHash.Length > 128)
                        {
                            strHash = strHash.Substring(0, 128);
                        }

                        if (strHash.EndsWith("="))
                            strHash = strHash.Substring(0, strHash.Length - 1);
                        var errorMsg = (string)null;

                        var msg = (string)null;
                        msg = string.Format(ApsStrings.sms_verification_message, "جامعة الامام سعود", Utils.GetApplicationUrl() + "l/" + strHash);

                        sent = SmsService.SendMessage(msg, new string[] { coresearcher.Researcher.MobileNo }, out errorMsg);
                        if (Utils.IsLocalhost)
                        {
                            sent = true;
                        }

                        if (sent)
                        {
                            coresearcher.Key = strHash;
                            coresearcher.IsKeyUsed = false;
                            coresearcher.DidResearcherAgree = null;

                            db.Entry(coresearcher).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                            db.RequestHistoryLogs.Add(new Data.DbModels.RequestHistoryLog
                            {
                                Created = DateTime.Now,
                                CreatedBy = Utils.CurrentUser.Id,
                                Url = Utils.GetFullUrl(),
                                IpAddress = Utils.GetUserIP(),
                                RequestId = req.Id,
                                UserAgent = HttpContextManager.Current.Request.UserAgent,
                                Status = (long)APS.Data.RequestStatus.Pending,
                                Type = (byte)RequestHistoryTypes.SendLinkToCoResearcher,
                            });
                            db.SaveChanges();
                            Listoflinks.Add(string.Format(Utils.GetApplicationUrl() + "l/" + coresearcher.Key));
                            TempData["coresearhcersmslink"] = Listoflinks;
                            TempData["message"] = "لقد تم إرسال الطلب بنجاح رقم الطلب هو :" + entity.RequestId;
                            TempData["message_type"] = "success";
                        }
                        else
                        {
                            TempData["message"] = "لم يتم إرسال الرسالة النصية على الجوال :" + coresearcher.Researcher.MobileNo + "<br>" + errorMsg;
                            TempData["message_type"] = "danger";
                            if (req.Status == (long)RequestStatus.PublishRequestPendingModifiedAfterReferees)
                            {
                                req.Status = (long)RequestStatus.PublishRequestPendingModificationsAfterReferees;
                            }
                            else
                            {
                                req.Status = (long)RequestStatus.PublishRequestNotSubmitted;
                            }
                            db.Entry(req).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
            if (sent)
            {
                db.RequestHistoryLogs.Add(new Data.DbModels.RequestHistoryLog
                {
                    Created = DateTime.Now,
                    CreatedBy = Utils.CurrentUser.Id,
                    Url = Utils.GetFullUrl(),
                    IpAddress = Utils.GetUserIP(),
                    RequestId = entity.RequestId.Value,
                    UserAgent = HttpContextManager.Current.Request.UserAgent,
                    Status = req.Status,
                    Type = (byte)RequestHistoryTypes.SubmittedByMainResearcher,
                });

                db.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }

        public bool AllowForRequestStep(long? id, RequestStep step, out Data.DbModels.Request req, out ActionResult result)
        {
            req = GetRequest(id);
            result = null;

            if (req == null && step == RequestStep.New)
            {
                return true;
            }
            if (req == null && step == RequestStep.New)
            {
                return true;
            }

            if (step == RequestStep.Unspecified || (req == null && step != RequestStep.New))
            {
                TempData["message"] = ApsStrings.request_not_found;
                TempData["message_type"] = "danger";
                result = RedirectToLastIndexPage("Index", "Home");

                return false;
            }

            if (result != null)
            {
                return false;
            }

            if (Utils.CurrentUser.Id == null)
            {
                result = RedirectToAction("LogOff", "Account");
                return false;
            }

            if (step == RequestStep.ScintificType)
            {
                return true;
            }
            if (req == null && step == RequestStep.Bank)
            {
                return true;
            }
            if (req == null && step == RequestStep.Participants)
            {
                return true;
            }
            return true;
        }

        [NonAction]
        private ActionResult CommonGet(long? requestId, ref PublicationViewModel model)
        {
            if (model == null)
                model = new PublicationViewModel();

            if (requestId == null) requestId = 0;

            var req = db.Requests
                            .FirstOrDefault(r => r.Id == requestId.Value && r.CreatedBy == CurrentUser.Id);

            if (req == null)
            {
                TempData["message"] = "هذا الطلب غير صالح";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Home");
            }

            if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
            {
                if (((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModifiedAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterRUS && ((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModificationsAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                    && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                {
                    return RedirectToAction("View", "Requests", new { id = req.Id });
                }
            }

            model.RequestId = req.Id;
            model.isArabic = (bool)req.IsArabic;

            model.Files = req.RequestFiles.Where(f => f.Type != (byte)FileType.CoResearcherOpinion).Select(c => new AttachedFile
            {
                FileId = c.Id.ToString(),
                Type = (FileType)c.Type,
                PageCount = c.PageCount,
                FileName = Path.GetFileName(c.FileName)
            }).ToList();

            model.CommitteeMembers = new List<string>();

            if (req.PublicationType == (byte)PublicationType.Thesis)
            {
                if (req.ThesisRequest != null)
                {
                    if (req.ThesisRequest.CommitteeMembers != null)
                    {
                        model.CommitteeMembers = req.ThesisRequest.CommitteeMembers.Split('|').ToList();
                    }

                    model.DateOfDiscussion = req.ThesisRequest.DateOfDiscussion.FormatDate();
                    model.Degree = (Degree)req.ThesisRequest.Degree;
                    model.GPA = req.ThesisRequest.Gpa;
                    model.Publisher = req.ThesisRequest.Publisher;
                    model.DateOfPublish = req.ThesisRequest.PublisherDate == null ? "" : req.ThesisRequest.PublisherDate.Value.FormatDate();
                    model.SciDepartmentId = req.ThesisRequest.SciDepartmentId;
                    model.ThesisTitle = req.ThesisRequest.ThesisTitle;
                    model.Supervisor = req.ThesisRequest.Supervisor;
                    model.AsstSupervisor = req.ThesisRequest.AssistantSupervisor;
                    model.UniversityOther = req.ThesisRequest.UniversityOther;
                    model.CollegeOther = req.ThesisRequest.CollegeOther;
                }

                if (string.IsNullOrWhiteSpace(model.ThesisTitle))
                {
                    model.ThesisTitle = req.Title;
                }
            }
            else if (req.PublicationType == (byte)PublicationType.Book)
            {
                if (req.BookRequest != null)
                {
                    //var recommendationDate = Utils.StringToDate(req.BookRequest.RecommendationDate.Value);
                    model.BookTitle = req.BookRequest.BookTitle;
                    model.PublishType = (PublishType)req.BookRequest.PublishType.Value;
                    model.BookType = (BookType)req.BookRequest.BookType;
                    model.isArabic = req.BookRequest.IsArabic;
                    //model.IsTranslated = req.BookRequest.IsTranslated;
                    model.AuthorMajorId = req.BookRequest.MajorId;
                    model.TranslatedTo = req.BookRequest.TranslatedTo;
                    model.OriginalBookTitle = req.BookRequest.OriginalTitleBook;
                    model.RecommmendationNumber = req.BookRequest.NoOfRecommendations;
                    model.RecommmendationDate = req.BookRequest.RecommendationDate == null ? "" : req.BookRequest.RecommendationDate.Value.ToString("yyyy/MM/dd");
                    model.PublisherPlace = req.BookRequest.PublisherPlace;
                    model.Publisher = req.BookRequest.Publisher;
                }

                if (string.IsNullOrWhiteSpace(model.BookTitle))
                {
                    model.BookTitle = req.Title;
                }
            }
            else if (req.PublicationType == (byte)PublicationType.Handbook)
            {
                if (req.HandbookRequest != null)
                {
                    model.HandBookTitle = req.HandbookRequest.HandbookTitle;
                    model.PublishType = (PublishType)req.HandbookRequest.PublishType.Value;
                    model.HandBookType = (HandBookType)req.HandbookRequest.HandbookType;
                    model.isArabic = req.HandbookRequest.IsArabic;
                    //model.IsTranslated = req.HandbookRequest.IsTranslated;
                    model.AuthorMajorId = req.HandbookRequest.MajorId;
                    model.TranslatedTo = req.HandbookRequest.TranslatedTo;
                    model.OriginalHandBookTitle = req.HandbookRequest.OriginalHandbookTitle;
                    model.PublisherPlace = req.HandbookRequest.PublisherPlace;
                    model.Publisher = req.HandbookRequest.Publisher;
                }

                if (string.IsNullOrWhiteSpace(model.HandBookTitle))
                {
                    model.HandBookTitle = req.Title;
                }
            }
            else if (req.PublicationType == (byte)PublicationType.Encyclopedia)
            {
                if (req.EncyclopediaRequest != null)
                {
                    model.RequestId = req.EncyclopediaRequest.RequestId;
                    model.PublishType = (PublishType)req.EncyclopediaRequest.PublishType;
                    model.EncyclopediaTitle = req.EncyclopediaRequest.EncyclopediaTitle;
                    model.isArabic = req.EncyclopediaRequest.IsArabic;
                    //model.IsTranslated = req.EncyclopediaRequest.IsTranslated;
                    model.MajorOfEncyclopediaId = req.EncyclopediaRequest.MajorOfEncyclopedia;
                    model.AuthorMajorId = req.EncyclopediaRequest.MajorOfAuther;
                    model.RecommmendationNumber = req.EncyclopediaRequest.NoOfRecommendations;
                    model.TranslatedTo = req.EncyclopediaRequest.TranslatedTo;
                    model.OriginalEncyclopediaTitle = req.EncyclopediaRequest.OriginalEncyclopediaTitle;
                    model.PublisherPlace = req.EncyclopediaRequest.PublisherPlace;
                    model.Publisher = req.EncyclopediaRequest.Publisher;
                }

                if (string.IsNullOrWhiteSpace(model.EncyclopediaTitle))
                {
                    model.EncyclopediaTitle = req.Title;
                }
            }
            else if (req.PublicationType == (byte)PublicationType.Probe)
            {
                if (req.ReviewRequest != null)
                {
                    model.BookTitle = req.ReviewRequest.BookTitle;
                    if (req.ReviewRequest.PublishType != null)
                    {
                        model.PublishType = (PublishType)req.ReviewRequest.PublishType.Value;
                    }

                    model.ProbeType = (ProbeType)req.ReviewRequest.ReviewType;

                    model.PublisherPlace = req.ReviewRequest.PublisherPlace;
                    model.Publisher = req.ReviewRequest.Publisher;
                    model.AuthorName = req.ReviewRequest.AuthersName;
                    model.ScopeOfManusript = req.ReviewRequest.ScopOfManuscript;
                    model.ProberMajorId = req.ReviewRequest.ReviewMajor;
                }

                if (string.IsNullOrWhiteSpace(model.BookTitle))
                {
                    model.BookTitle = req.Title;
                }
            }
            else if (req.PublicationType == (byte)PublicationType.Dictionary)
            {
                if (req.DictionaryRequest != null)
                {
                    model.DictionaryTitle = req.DictionaryRequest.DictionaryTitle;

                    if (req.DictionaryRequest.PublishType > 0)
                        model.PublishType = (PublishType)req.DictionaryRequest.PublishType;
                    model.PublisherPlace = req.DictionaryRequest.PublisherPlace;
                    model.Publisher = req.DictionaryRequest.Publisher;
                }

                if (string.IsNullOrWhiteSpace(model.DictionaryTitle))
                {
                    model.DictionaryTitle = req.Title;
                }
            }

            if (model.CommitteeMembers.Count < 3)
            {
                while (true)
                {
                    model.CommitteeMembers.Add("");

                    if (model.CommitteeMembers.Count >= 3) break;
                };
            }

            if (req.TranslatedToId > 0)
            {
                model.IsTranslated = true;
                model.TranslatedToId = req.TranslatedToId;
                model.TranslatedTo = req.TranslatedTo;
            }

            ViewBag.FileUploadRequired = true;

            ViewBag.Departments = db.Departments.ToList();
            ViewBag.Colleges = db.Colleges.ToList();
            ViewBag.Majors = db.Majors.ToList();
            ViewBag.TranslatedTo = db.TranslatedToLanguages.ToList();
            ViewBag.RequestId = model.RequestId = requestId;
            ViewBag.RequestType = ((RequestType)req.Type).ToString();

            ViewBag.AutherMajors = ViewBag.Majors;

            ViewBag.GPAs = new List<SelectListItem>
            {
                new SelectListItem{ Text=ApsStrings.excellent,  Value="Excellent"},
                new SelectListItem{ Text=ApsStrings.very_good,  Value="VeryGood"},
                new SelectListItem{ Text=ApsStrings.good,  Value="Good"},
                new SelectListItem{ Text=ApsStrings.none,  Value="None"},
            };

            ViewBag.Majors = db.Majors.ToList();
            ViewBag.AutherMajors = db.Majors.ToList();
            model.PublicationType = (PublicationType)req.PublicationType;

            return null;
        }

        [NonAction]
        private ActionResult CommonPost(ref PublicationViewModel model)
        {
            if (model == null || !(model.RequestId > 0))
            {
                return RedirectToAction("Index", "Home");
            }

            var requestId = model.RequestId.Value;
            var req = db.Requests
                .FirstOrDefault(r => r.Id == requestId && r.CreatedBy == CurrentUser.Id);

            if (req == null)
            {
                TempData["message"] = "هذا الطلب غير صالح";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Home");
            }

            if (req.Type == (byte)Data.RequestType.PublishRequest || req.Type == (byte)Data.RequestType.RepublishRequest)
            {
                if (((RequestStatus)req.Status) != RequestStatus.PublishRequestPendingModifiedAfterReferees && ((RequestStatus)req.Status) != RequestStatus.PublishRequestNotSubmitted && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchers
                    && ((RequestStatus)req.Status) != RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified)
                {
                    return RedirectToAction("View", "Requests", new { id = req.Id });
                }
            }
            foreach (var f in model.FileNames)
            {
                var names = Utils.GetCurrentUserDetails().FullName.Split(' ').ToList();
                foreach (var name in names)
                {
                    if (f.Contains(name))
                    {
                        TempData["message"] = "يرجى حذف البيانات الشخصية ووسائل الاتصال الشخصية من المادة العلمية";
                        TempData["message_type"] = "danger";
                    }
                }
            }

            switch ((PublicationType)req.PublicationType)
            {
                case PublicationType.Thesis:

                    if (model.Type == RequestType.RepublishRequest)
                    {
                        if (string.IsNullOrWhiteSpace(model.Publisher))
                        {
                            ModelState.AddModelError("Publisher", ApsStrings.publisher_required);
                        }

                        if (string.IsNullOrWhiteSpace(model.PublisherPlace))
                        {
                            ModelState.AddModelError("PublisherPlace", ApsStrings.publisher_place_required);
                        }
                    }

                    if (string.IsNullOrWhiteSpace(model.ThesisTitle))
                    {
                        ModelState.AddModelError("ThesisTitle", ApsStrings.thesis_title_required);
                    }
                    if (string.IsNullOrWhiteSpace(model.GPA))
                    {
                        ModelState.AddModelError("GPa", ApsStrings.gpa_required);
                    }
                    if (string.IsNullOrWhiteSpace(model.DateOfDiscussion))
                    {
                        ModelState.AddModelError("DateOfDiscussion", ApsStrings.discussiondate_required);
                    }

                    req.Title = model.ThesisTitle;

                    break;

                case PublicationType.Book:

                    req.Title = model.BookTitle;

                    if (string.IsNullOrWhiteSpace(model.BookTitle))
                    {
                        ModelState.AddModelError("BookTitle", ApsStrings.book_title_required);
                    }

                    if (!(model.AuthorMajorId > 0))
                    {
                        ModelState.AddModelError("AuthorMajorId", ApsStrings.author_major_required);
                    }

                    if (model.IsTranslated)
                    {
                        if (string.IsNullOrWhiteSpace(model.OriginalBookTitle))
                        {
                            ModelState.AddModelError("OriginalBookTitle", ApsStrings.original_book_title_required);
                        }
                        if (model.TranslatedToId == 4 && string.IsNullOrWhiteSpace(model.TranslatedTo))
                        {
                            ModelState.AddModelError("TranslatedTo", ApsStrings.translated_To_Language_required);
                        }
                    }
                    if (model.BookType == BookType.Systematic)
                    {
                        if (string.IsNullOrWhiteSpace(model.RecommmendationDate))
                        {
                            ModelState.AddModelError("RecommmendationDate", ApsStrings.recommendation_date_required);
                        }
                        if ((model.RecommmendationNumber ?? 0) == 0)
                        {
                            ModelState.AddModelError("RecommmendationNumber", ApsStrings.recommendation_number_required);
                        }
                    }

                    break;

                case PublicationType.Handbook:

                    if (string.IsNullOrWhiteSpace(model.HandBookTitle))
                    {
                        ModelState.AddModelError("HandBookTitle", ApsStrings.handbook_title_required);
                    }

                    if (!(model.AuthorMajorId > 0))
                    {
                        ModelState.AddModelError("AuthorMajorId", ApsStrings.author_major_required);
                    }

                    req.Title = model.HandBookTitle;

                    if (model.IsTranslated)
                    {
                        if (string.IsNullOrWhiteSpace(model.OriginalHandBookTitle))
                        {
                            ModelState.AddModelError("OriginalHandBookTitle", ApsStrings.original_handbook_title_required);
                        }
                        if (model.TranslatedToId == 4 && string.IsNullOrWhiteSpace(model.TranslatedTo))
                        {
                            ModelState.AddModelError("TranslatedTo", ApsStrings.translated_To_Language_required);
                        }
                    }
                    break;

                case PublicationType.Probe:
                    if (string.IsNullOrWhiteSpace(model.BookTitle))
                    {
                        ModelState.AddModelError("BookTitle", ApsStrings.book_title_required);
                    }
                    if (string.IsNullOrWhiteSpace(model.ScopeOfManusript))
                    {
                        ModelState.AddModelError("ScopeOfManusript", ApsStrings.scope_manuscript_required);
                    }

                    req.Title = model.BookTitle;
                    break;

                case PublicationType.Encyclopedia:

                    if (string.IsNullOrWhiteSpace(model.EncyclopediaTitle))
                    {
                        ModelState.AddModelError("EncyclopediaTitle", ApsStrings.encyclopedia_title_required);
                    }

                    if (!(model.AuthorMajorId > 0))
                    {
                        ModelState.AddModelError("AuthorMajorId", ApsStrings.author_major_required);
                    }

                    if (!(model.RecommmendationNumber > 0))
                        model.RecommmendationNumber = 0;

                    req.Title = model.EncyclopediaTitle;

                    if (model.IsTranslated)
                    {
                        if (string.IsNullOrWhiteSpace(model.OriginalEncyclopediaTitle))
                        {
                            ModelState.AddModelError("OriginalEncyclopediaTitle", ApsStrings.original_encyclopedia_title_required);
                        }
                        if (model.TranslatedToId == 4 && string.IsNullOrWhiteSpace(model.TranslatedTo))
                        {
                            ModelState.AddModelError("TranslatedTo", ApsStrings.translated_To_Language_required);
                        }
                    }

                    break;

                case PublicationType.Dictionary:

                    if (string.IsNullOrWhiteSpace(model.DictionaryTitle))
                    {
                        ModelState.AddModelError("DictionaryTitle", ApsStrings.dictionary_title_required);
                    }

                    if (!(model.AuthorMajorId > 0))
                    {
                        ModelState.AddModelError("AuthorMajorId", ApsStrings.author_major_required);
                    }

                    if (!(model.RecommmendationNumber > 0))
                        model.RecommmendationNumber = 0;

                    req.Title = model.DictionaryTitle;

                    if (model.IsTranslated)
                    {
                        if (string.IsNullOrWhiteSpace(model.OriginalDictionaryTitle))
                        {
                            ModelState.AddModelError("OriginalDictionaryTitle", ApsStrings.original_dictionary_title_required);
                        }
                        if (model.TranslatedToId == 4 && string.IsNullOrWhiteSpace(model.TranslatedTo))
                        {
                            ModelState.AddModelError("TranslatedTo", ApsStrings.translated_To_Language_required);
                        }
                    }

                    break;
            }

            var prevFile = req.RequestFiles.FirstOrDefault(f => f.Type == (byte)FileType.Publication);

            var invalidType = false;
            var foundFiles = 0;

            var uploadedFile = (HttpPostedFileBase)null;
            for (var i = 0; i < Request.Files.Count; i++)
            {
                uploadedFile = Request.Files[i];
                if (uploadedFile != null && uploadedFile.ContentLength > 0)
                {
                    var extension = Path.GetExtension(uploadedFile.FileName);
                    if (extension == null || (extension.ToLower() != ".pdf" && extension.ToLower() != ".doc" && extension.ToLower() != ".docx"))
                    {
                        invalidType = true;
                    }

                    foundFiles++;
                }
            }

            if (foundFiles == 0)
            {
                if (prevFile == null)
                {
                    ModelState.AddModelError("FileUpload", ApsStrings.attached_file_thes);
                }
            }
            else if (invalidType)
            {
                var extension = Path.GetExtension(uploadedFile.FileName);
                if (extension == null || (extension.ToLower() != ".pdf" && extension.ToLower() != ".doc" && extension.ToLower() != ".docx"))
                {
                    ModelState.AddModelError("FileUpload", ApsStrings.file_type_pdf_doc);
                }
            }

            if (model.PageCounts == null) model.PageCounts = new List<int>();
            if (model.FileNames == null) model.FileNames = new List<string>();

            if (model.PageCounts.Count != foundFiles)
            {
                TempData["message"] = ApsStrings.must_enter_page_numbers_for_all_files;
                TempData["message_type"] = "danger";

                ModelState.AddModelError("FileUpload", ApsStrings.must_enter_page_numbers_for_all_files);
            }

            foreach (var pageCount in model.PageCounts)
            {
                if (pageCount == 0)
                {
                    TempData["message"] = ApsStrings.must_enter_page_numbers_for_all_files;
                    TempData["message_type"] = "danger";

                    ModelState.AddModelError("FileUpload", ApsStrings.must_enter_page_numbers_for_all_files);
                    break;
                }
            }

            if (model.FileNames.Count != foundFiles)
            {
                TempData["message"] = ApsStrings.must_enter_file_names_for_all_files;
                TempData["message_type"] = "danger";

                ModelState.AddModelError("FileUpload", ApsStrings.must_enter_file_names_for_all_files);
            }

            model.RequestId = req.Id;

            if (ModelState.IsValid)
            {
                try
                {
                    req.IsArabic = model.isArabic;
                    var TranslatedTo = string.Empty;
                    if (model.IsTranslated)
                    {
                        req.TranslatedToId = model.TranslatedToId;
                        req.TranslatedTo = db.TranslatedToLanguages.Where(x => x.Id == req.TranslatedToId).FirstOrDefault().NameArabic;

                        if (model.TranslatedToId == 4)
                        {
                            req.TranslatedTo = model.TranslatedTo;
                        }
                        TranslatedTo = req.TranslatedTo;
                    }
                    else
                    {
                        req.TranslatedTo = null;
                        req.TranslatedToId = null;
                        req.TranslatedToLanguage = null;
                    }
                    req.NoOfPages = model.PageCounts.Sum();
                    db.Entry(req).State = System.Data.Entity.EntityState.Modified;

                    switch ((PublicationType)req.PublicationType)
                    {
                        case PublicationType.Book:
                            {
                                var recommendationDate = Utils.StringToDate(model.RecommmendationDate);
                                if (req.BookRequest == null)
                                {
                                    db.BookRequests.Add(new Data.DbModels.BookRequest
                                    {
                                        Created = TM.Now,
                                        CreatedBy = Utils.CurrentUser.Id,
                                        Updated = TM.Now,
                                        UpdatedBy = Utils.CurrentUser.Id,
                                        BookTitle = model.BookTitle,
                                        RequestId = model.RequestId.Value,
                                        PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental,
                                        BookType = model.BookType == BookType.Cultural ? (byte)BookType.Cultural : (model.BookType == BookType.PublicScience ? (byte)BookType.PublicScience : (byte)BookType.Systematic),
                                        IsArabic = model.isArabic,
                                        IsTranslated = model.IsTranslated,
                                        MajorId = model.AuthorMajorId.Value,
                                        TranslatedTo = TranslatedTo,
                                        OriginalTitleBook = model.OriginalBookTitle,
                                        NoOfRecommendations = model.RecommmendationNumber,
                                        PublisherPlace = model.PublisherPlace,
                                        PublishingDate = TM.Now,
                                        RecommendationDate = recommendationDate,
                                        Publisher = model.Publisher,
                                    });
                                }
                                else
                                {
                                    req.BookRequest.Updated = TM.Now;
                                    req.BookRequest.UpdatedBy = Utils.CurrentUser.Id;
                                    req.BookRequest.BookTitle = model.BookTitle;
                                    req.BookRequest.RequestId = model.RequestId.Value;
                                    req.BookRequest.PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental;
                                    req.BookRequest.BookType = model.BookType == BookType.Cultural ? (byte)BookType.Cultural : (model.BookType == BookType.PublicScience ? (byte)BookType.PublicScience : (byte)BookType.Systematic);
                                    req.BookRequest.IsArabic = model.isArabic;
                                    req.BookRequest.IsTranslated = model.IsTranslated;
                                    if (model.IsTranslated)
                                    {
                                        req.BookRequest.TranslatedTo = TranslatedTo;
                                        req.BookRequest.OriginalTitleBook = model.OriginalBookTitle;
                                    }
                                    else
                                    {
                                        req.BookRequest.TranslatedTo = null;
                                        req.BookRequest.OriginalTitleBook = null;
                                    }
                                    req.BookRequest.MajorId = model.AuthorMajorId.Value;
                                    req.BookRequest.NoOfRecommendations = model.RecommmendationNumber;
                                    req.BookRequest.PublisherPlace = model.PublisherPlace;
                                    req.BookRequest.PublishingDate = TM.Now;
                                    req.BookRequest.RecommendationDate = recommendationDate;
                                    req.BookRequest.Publisher = model.Publisher;

                                    db.Entry(req.BookRequest).State = System.Data.Entity.EntityState.Modified;
                                }
                                break;
                            }
                        case PublicationType.Thesis:
                            {
                                var dateOfDiscussion = Utils.StringToDate(model.DateOfDiscussion);
                                if (dateOfDiscussion == null) dateOfDiscussion = default(DateTime);

                                var dateOfPublish = Utils.StringToDate(model.DateOfPublish);
                                var committeeMembers = null as string;

                                if (model.CommitteeMembers != null && model.CommitteeMembers.Count > 0)
                                {
                                    committeeMembers = string.Join("|", model.CommitteeMembers);
                                }

                                if (req.ThesisRequest == null)
                                {
                                    db.ThesisRequests.Add(new Data.DbModels.ThesisRequest
                                    {
                                        CommitteeMembers = committeeMembers,
                                        RequestId = model.RequestId.Value,
                                        Created = TM.Now,
                                        CreatedBy = Utils.CurrentUser.Id,
                                        Updated = TM.Now,
                                        UpdatedBy = Utils.CurrentUser.Id,
                                        DateOfDiscussion = dateOfDiscussion.Value,
                                        Degree = model.Degree == Degree.Master ? (byte)Degree.Master : (byte)Degree.Phd,
                                        Gpa = model.GPA,
                                        Publisher = model.Publisher,
                                        PublisherDate = dateOfPublish,
                                        SciDepartmentId = model.SciDepartmentId,
                                        ThesisTitle = model.ThesisTitle,
                                        Supervisor = model.Supervisor,
                                        AssistantSupervisor = model.AsstSupervisor,
                                        CollegeOther = model.CollegeOther
                                    });
                                }
                                else
                                {
                                    req.ThesisRequest.CommitteeMembers = committeeMembers;
                                    req.ThesisRequest.RequestId = model.RequestId.Value;
                                    req.ThesisRequest.Created = TM.Now;
                                    req.ThesisRequest.CreatedBy = Utils.CurrentUser.Id;
                                    req.ThesisRequest.Updated = TM.Now;
                                    req.ThesisRequest.UpdatedBy = Utils.CurrentUser.Id;
                                    req.ThesisRequest.DateOfDiscussion = dateOfDiscussion.Value;
                                    req.ThesisRequest.Degree = model.Degree == Degree.Master ? (byte)Degree.Master : (byte)Degree.Phd;
                                    req.ThesisRequest.Gpa = model.GPA;
                                    req.ThesisRequest.Publisher = model.Publisher;
                                    req.ThesisRequest.PublisherDate = dateOfPublish;
                                    req.ThesisRequest.SciDepartmentId = model.SciDepartmentId;
                                    req.ThesisRequest.ThesisTitle = model.ThesisTitle;
                                    req.ThesisRequest.Supervisor = model.Supervisor;
                                    req.ThesisRequest.AssistantSupervisor = model.AsstSupervisor;
                                    req.ThesisRequest.UniversityOther = model.UniversityOther;
                                    req.ThesisRequest.CollegeOther = model.CollegeOther;

                                    db.Entry(req.ThesisRequest).State = System.Data.Entity.EntityState.Modified;
                                }
                                break;
                            }
                        case PublicationType.Handbook:
                            {
                                if (req.HandbookRequest == null)
                                {
                                    db.HandbookRequests.Add(new Data.DbModels.HandbookRequest
                                    {
                                        Created = TM.Now,
                                        CreatedBy = Utils.CurrentUser.Id,
                                        Updated = TM.Now,
                                        UpdatedBy = Utils.CurrentUser.Id,
                                        HandbookTitle = model.HandBookTitle,
                                        RequestId = model.RequestId.Value,
                                        PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental,
                                        HandbookType = model.HandBookType == HandBookType.Cultural ? (byte)HandBookType.Cultural : (model.HandBookType == HandBookType.ChildrenStories ? (byte)HandBookType.ChildrenStories : (byte)HandBookType.Other),
                                        IsArabic = model.isArabic,
                                        IsTranslated = model.IsTranslated,
                                        MajorId = model.AuthorMajorId.Value,
                                        TranslatedTo = TranslatedTo,
                                        OriginalHandbookTitle = model.OriginalHandBookTitle,
                                        PublisherDate = TM.Now,
                                        PublisherPlace = model.PublisherPlace,
                                        Publisher = model.Publisher,
                                    });
                                }
                                else
                                {
                                    req.HandbookRequest.Updated = TM.Now;
                                    req.HandbookRequest.UpdatedBy = Utils.CurrentUser.Id;
                                    req.HandbookRequest.HandbookTitle = model.HandBookTitle;
                                    req.HandbookRequest.RequestId = model.RequestId.Value;
                                    req.HandbookRequest.PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental;
                                    req.HandbookRequest.HandbookType = model.HandBookType == HandBookType.Cultural ? (byte)HandBookType.Cultural : (model.HandBookType == HandBookType.ChildrenStories ? (byte)HandBookType.ChildrenStories : (byte)HandBookType.Other);
                                    req.HandbookRequest.IsArabic = model.isArabic;
                                    req.HandbookRequest.IsTranslated = model.IsTranslated;
                                    req.HandbookRequest.MajorId = model.AuthorMajorId.Value;
                                    req.HandbookRequest.TranslatedTo = TranslatedTo;
                                    req.HandbookRequest.OriginalHandbookTitle = model.OriginalHandBookTitle;
                                    req.HandbookRequest.PublisherDate = TM.Now;
                                    req.HandbookRequest.PublisherPlace = model.PublisherPlace;
                                    req.HandbookRequest.Publisher = model.Publisher;

                                    db.Entry(req.HandbookRequest).State = System.Data.Entity.EntityState.Modified;
                                }
                                break;
                            }
                        case PublicationType.Probe:
                            {
                                var dateOfPublish = Utils.StringToDate(model.DateOfPublish);

                                if (req.ReviewRequest == null)
                                {
                                    db.ReviewRequests.Add(new Data.DbModels.ReviewRequest
                                    {
                                        Created = TM.Now,
                                        CreatedBy = Utils.CurrentUser.Id,
                                        Updated = TM.Now,
                                        UpdatedBy = Utils.CurrentUser.Id,
                                        ScopOfManuscript = model.ScopeOfManusript,
                                        RequestId = model.RequestId.Value,
                                        BookTitle = model.BookTitle,
                                        PublisherDate = dateOfPublish,
                                        PublisherPlace = "",
                                        Publisher = "",
                                        AuthersName = model.AuthorName,
                                        ReviewMajor = model.ProberMajorId,
                                        //ReviewType = model.ProbeType == ProbeType.ManuScript ? (byte)ProbeType.ManuScript : (byte)ProbeType.RareBook,
                                        ReviewType = (byte)model.ProbeType,
                                    });
                                }
                                else
                                {
                                    req.ReviewRequest.Updated = TM.Now;
                                    req.ReviewRequest.UpdatedBy = Utils.CurrentUser.Id;
                                    req.ReviewRequest.ScopOfManuscript = model.ScopeOfManusript;
                                    req.ReviewRequest.RequestId = model.RequestId.Value;
                                    req.ReviewRequest.BookTitle = model.BookTitle;
                                    req.ReviewRequest.PublisherDate = dateOfPublish;
                                    req.ReviewRequest.PublisherPlace = "";
                                    req.ReviewRequest.Publisher = "";
                                    req.ReviewRequest.AuthersName = model.AuthorName;
                                    req.ReviewRequest.ReviewMajor = model.ProberMajorId;
                                    //req.ReviewRequest.ReviewType = model.ProbeType == ProbeType.ManuScript ? (byte)ProbeType.ManuScript : (byte)ProbeType.RareBook;
                                    req.ReviewRequest.ReviewType = (byte)model.ProbeType;
                                    db.Entry(req.ReviewRequest).State = System.Data.Entity.EntityState.Modified;
                                }
                                break;
                            }
                        case PublicationType.Encyclopedia:
                            {
                                var dateOfPublish = Utils.StringToDate(model.DateOfPublish);
                                var recommendationDate = Utils.StringToDate(model.RecommmendationDate);

                                if (req.EncyclopediaRequest == null)
                                {
                                    db.EncyclopediaRequests.Add(new Data.DbModels.EncyclopediaRequest
                                    {
                                        Created = TM.Now,
                                        CreatedBy = Utils.CurrentUser.Id,
                                        Updated = TM.Now,
                                        UpdatedBy = Utils.CurrentUser.Id,
                                        RequestId = model.RequestId.Value,
                                        PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental,
                                        EncyclopediaTitle = model.EncyclopediaTitle,
                                        IsArabic = model.isArabic,
                                        IsTranslated = model.IsTranslated,
                                        MajorOfEncyclopedia = model.MajorOfEncyclopediaId,
                                        MajorOfAuther = model.AuthorMajorId.Value,
                                        NoOfRecommendations = model.RecommmendationNumber.Value,
                                        TranslatedTo = TranslatedTo,
                                        OriginalEncyclopediaTitle = model.OriginalEncyclopediaTitle,
                                        //RecommendationDate = recommendationDate == null ? default(DateTime) : recommendationDate.Value,
                                        RecommendationDate = recommendationDate ?? DateTime.Now,
                                        PublisherDate = dateOfPublish,
                                        PublisherPlace = model.PublisherPlace,
                                        Publisher = model.Publisher,
                                    });
                                }
                                else
                                {
                                    req.EncyclopediaRequest.Updated = TM.Now;
                                    req.EncyclopediaRequest.UpdatedBy = Utils.CurrentUser.Id;
                                    req.EncyclopediaRequest.RequestId = model.RequestId.Value;
                                    req.EncyclopediaRequest.PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental;
                                    req.EncyclopediaRequest.EncyclopediaTitle = model.EncyclopediaTitle;
                                    req.EncyclopediaRequest.IsArabic = model.isArabic;
                                    req.EncyclopediaRequest.IsTranslated = model.IsTranslated;
                                    req.EncyclopediaRequest.MajorOfEncyclopedia = model.MajorOfEncyclopediaId;
                                    req.EncyclopediaRequest.MajorOfAuther = model.AuthorMajorId.Value;
                                    req.EncyclopediaRequest.NoOfRecommendations = model.RecommmendationNumber.Value;
                                    req.EncyclopediaRequest.TranslatedTo = TranslatedTo;
                                    req.EncyclopediaRequest.OriginalEncyclopediaTitle = model.OriginalEncyclopediaTitle;
                                    //req.EncyclopediaRequest.RecommendationDate = recommendationDate == null ? default(DateTime) : recommendationDate.Value;
                                    req.EncyclopediaRequest.RecommendationDate = recommendationDate ?? DateTime.Now;
                                    req.EncyclopediaRequest.PublisherDate = dateOfPublish;
                                    req.EncyclopediaRequest.PublisherPlace = model.PublisherPlace;
                                    req.EncyclopediaRequest.Publisher = model.Publisher;

                                    db.Entry(req.EncyclopediaRequest).State = System.Data.Entity.EntityState.Modified;
                                }
                                break;
                            }
                        case PublicationType.Dictionary:
                            {
                                var dateOfPublish = Utils.StringToDate(model.DateOfPublish);
                                var recommendationDate = Utils.StringToDate(model.RecommmendationDate);

                                req.Title = model.DictionaryTitle;
                                req.PublicationType = (byte)PublicationType.Dictionary;
                                db.Entry(req).State = System.Data.Entity.EntityState.Modified;

                                if (req.DictionaryRequest == null)
                                {
                                    db.DictionaryRequests.Add(new Data.DbModels.DictionaryRequest
                                    {
                                        Created = DateTime.Now,
                                        CreatedBy = Utils.CurrentUser.Id,
                                        Updated = DateTime.Now,
                                        UpdatedBy = Utils.CurrentUser.Id,
                                        RequestId = model.RequestId.Value,
                                        PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental,
                                        DictionaryTitle = model.DictionaryTitle,
                                        IsArabic = model.isArabic,
                                        IsTranslated = model.IsTranslated,
                                        DictionaryMajor = model.MajorOfDictionaryID,
                                        MajorOfAuther = model.AuthorMajorId.Value,
                                        NoOfRecommendations = model.RecommmendationNumber.Value,
                                        TranslatedTo = TranslatedTo,
                                        OriginalDictionaryTitle = model.OriginalDictionaryTitle,
                                        //RecommendationDate = recommendationDate == null ? default(DateTime) : recommendationDate.Value,
                                        RecommendationDate = recommendationDate ?? DateTime.Now,
                                        PublisherDate = dateOfPublish,
                                        PublisherPlace = model.PublisherPlace,
                                        Publisher = model.Publisher,
                                    });
                                }
                                else
                                {
                                    req.DictionaryRequest.Updated = TM.Now;
                                    req.DictionaryRequest.UpdatedBy = Utils.CurrentUser.Id;
                                    req.DictionaryRequest.RequestId = model.RequestId.Value;
                                    req.DictionaryRequest.PublishType = model.PublishType == PublishType.Commercial ? (byte)PublishType.Commercial : (byte)PublishType.Governmental;
                                    req.DictionaryRequest.DictionaryTitle = model.DictionaryTitle;
                                    req.DictionaryRequest.IsArabic = model.isArabic;
                                    req.DictionaryRequest.IsTranslated = model.IsTranslated;
                                    req.DictionaryRequest.DictionaryMajor = model.MajorOfDictionaryID;
                                    req.DictionaryRequest.MajorOfAuther = model.AuthorMajorId.Value;
                                    req.DictionaryRequest.NoOfRecommendations = model.RecommmendationNumber.Value;
                                    req.DictionaryRequest.TranslatedTo = TranslatedTo;
                                    req.DictionaryRequest.OriginalDictionaryTitle = model.OriginalDictionaryTitle;
                                    //req.DictionaryRequest.RecommendationDate = recommendationDate == null ? default(DateTime) : recommendationDate.Value;
                                    req.DictionaryRequest.RecommendationDate = recommendationDate ?? DateTime.Now;
                                    req.DictionaryRequest.PublisherDate = TM.Now;
                                    req.DictionaryRequest.PublisherPlace = model.PublisherPlace;
                                    req.DictionaryRequest.Publisher = model.Publisher;

                                    db.Entry(req.DictionaryRequest).State = System.Data.Entity.EntityState.Modified;
                                }
                                break;
                            }
                    }
                    var index = 0;
                    for (var i = 0; i < Request.Files.Count; i++)
                    {
                        var File = Request.Files[i];
                        if (File != null && File.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(File.FileName);
                            var extension = Path.GetExtension(fileName);

                            var f = new Data.DbModels.RequestFile
                            {
                                Id = Guid.NewGuid(),
                                PageCount = model.PageCounts[index],
                                Type = (byte)FileType.Publication,
                                CreatedBy = Utils.CurrentUser.Id,
                                FileName = string.IsNullOrWhiteSpace(model.FileNames[index]) ? Path.GetFileName(File.FileName) : model.FileNames[index],
                                Extension = extension,
                                RequestId = model.RequestId.Value,
                                Created = DateTime.Now
                            };

                            f.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), f.Id + extension);
                            File.EnhancedSaveAs(f.FilePath);

                            db.RequestFiles.Add(f);

                            index++;
                        }
                    }

                    db.SaveChanges();
                    TempData["message"] = "تمت إضافة بيانات المادة العلمية بنجاح";
                    TempData["message_type"] = "success";
                }
                catch (Exception ex)
                {
                    TempData["message"] = ex.Message;
                    TempData["message_type"] = "danger";
                }

                return RedirectToAction("BankDetails", new { requestid = model.RequestId });
            }

            model.Files = req.RequestFiles.Select(c => new AttachedFile
            {
                FileId = c.Id.ToString(),
                Type = (FileType)c.Type,
                PageCount = c.PageCount,
                FileName = Path.GetFileName(c.FileName)
            }).ToList();

            ViewBag.Departments = db.Departments.ToList();
            ViewBag.Colleges = db.Colleges.ToList();
            ViewBag.Majors = db.Majors.ToList();
            ViewBag.TranslatedTo = db.TranslatedToLanguages.ToList();
            ViewBag.RequestType = ((RequestType)req.Type).ToString();
            ViewBag.AutherMajors = ViewBag.Majors;

            switch ((PublicationType)req.PublicationType)
            {
                case PublicationType.Thesis:
                    {
                        if (model.CommitteeMembers == null)
                        {
                            model.CommitteeMembers = new List<string>();
                        }

                        if (model.CommitteeMembers.Count < 3)
                        {
                            while (true)
                            {
                                model.CommitteeMembers.Add("");

                                if (model.CommitteeMembers.Count >= 3) break;
                            };
                        }
                        ViewBag.GPAs = new List<SelectListItem>
                        {
                            new SelectListItem{ Text=ApsStrings.excellent,  Value="Excellent"},
                            new SelectListItem{ Text=ApsStrings.very_good,  Value="VeryGood"},
                            new SelectListItem{ Text=ApsStrings.good,  Value="Good"},
                            new SelectListItem{ Text=ApsStrings.none,  Value="None"},
                        };
                        break;
                    }
            }
            model.PublicationType = (PublicationType)req.PublicationType;
            ViewBag.FileUploadRequired = true;

            return null;
        }
    }
}