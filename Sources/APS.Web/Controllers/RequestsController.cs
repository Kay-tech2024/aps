using APS.Core;
using APS.Core.Models;
using APS.Core.Workflows;
using APS.Data;
using APS.Data.DbModels;
using APS.TimeMachine;
using APS.Web.App_Start.Code;
using APS.Web.App_Start.Extensions;
using APS.Web.Models;
using APS.Web.Resources;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    public class RequestsController : BaseController
    {
        public ActionResult Index()
        {
            return RedirectToLastIndexPage("Index", "Home");
        }

        [HttpGet]
        public ActionResult Create()
        {
            var user = Utils.GetCurrentUserDetails();

            var model = new PublishRequestViewModel
            {
                Name = user.DbUser.FullName,
            };

            var pendingRequest = DbContext.Requests.FirstOrDefault(r => r.Type == (byte)RequestType.PublishRequest && r.CreatedBy == CurrentUser.Id && r.Status == (long)RequestStatus.Pending);
            if (pendingRequest != null)
            {
                TempData["message"] = ApsStrings.similar_request_already_exists;
                TempData["message_type"] = "danger";
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [AllowAnonymous]
        public JsonResult UpdateRequestReviewResponse(ExternalRefereeRequestReviewResponseViewModel model)
        {
            var _response = new AjaxRequestResponse();
            string _publicHashLink = Utils.Decrypt(model.ParamsHash);
            bool DidRefereeAgree = model.Action == Data.RequestAction.AcceptToReview.ToString() ? true : false; ;
            if (!String.IsNullOrEmpty(_publicHashLink))
            {
                string[] _publicHashLinkParts = _publicHashLink.Split(Core.Constants.ExternalRefereeParamsConnector);
                string refereeUserId = String.Empty;
                var request = Utils.db.Requests.Find(model.RequestId);
                if (request != null)
                {
                    RequestReferee rr = null;
                    if (request.Status == (long)RequestStatus.PublishRequestPendingReferees || request.Status == (long)RequestStatus.PublishRequestPendingSpareReferee)
                    {
                        rr = request.RequestReferees.Where(r => r.RefereeId == model.RefereeId && r.DidRefereeAgree == null && r.IsDeleted == false
                   && r.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString()).FirstOrDefault();
                    }
                    else
                    {
                        rr = request.RequestReferees.Where(r => r.RefereeId == model.RefereeId && r.DidRefereeAgree == null && r.IsDeleted == false
                           && r.CreatedByUser.Roles.FirstOrDefault().Name != ApsUserRole.AcademicCouncilHead.ToString()).FirstOrDefault();
                    }
                    if (rr != null)
                    {
                        refereeUserId = rr.Referee.UserId;

                        rr.DidRefereeAgree = DidRefereeAgree;
                        rr.RequestReviewAcceptOn = TM.Now;
                        rr.ReceivedOn = TM.Now;
                        rr.Action = (model.Action == RequestAction.AcceptToReview.ToString()) ? (byte)RequestAction.AcceptToReview : (byte)RequestAction.RejectToReview;
                        rr.Comments = String.Empty;
                        if (!rr.DidRefereeAgree.Value)
                        {
                            rr.SubmittedOn = TM.Now;
                        }
                        rr.Updated = TM.Now;
                        rr.UpdatedBy = !String.IsNullOrEmpty(refereeUserId) ? refereeUserId : Utils.SystemAdminUserId;

                        Utils.db.Entry(rr).State = EntityState.Modified;
                        Utils.db.SaveChanges();

                        //var request = Utils.db.Requests.Find(rr.RequestId);
                        //if (request != null)
                        //{
                        long currentStatus = request.Status;
                        if (request.Status == (long)RequestStatus.PublishRequestPendingReferees)
                        {
                            request.Status = (long)RequestStatus.PublishRequestPendingSpareReferee;
                            request.Updated = TM.Now;
                            Utils.db.SaveChanges();
                        }
                        Utils.db.RequestHistoryLogs.Add(new RequestHistoryLog
                        {
                            RequestId = request.Id,
                            Status = currentStatus,
                            Action = rr.DidRefereeAgree.Value == true ? (byte)Data.RequestAction.AcceptToReview : (byte)Data.RequestAction.RejectToReview,
                            IpAddress = Utils.GetUserIP(),
                            UserAgent = Request.UserAgent,
                            Url = Utils.GetFullUrl(),
                            Created = TM.Now,
                            CreatedBy = !String.IsNullOrEmpty(refereeUserId) ? refereeUserId : Utils.SystemAdminUserId,
                        });
                        Utils.db.SaveChanges();
                        if (request.Status == (long)RequestStatus.PublishRequestPendingReferees || request.Status == (long)RequestStatus.PublishRequestPendingSpareReferee)
                        {
                            var _RefereeUtil = new RefereeUtil();
                            if (!DidRefereeAgree)
                            {
                                _RefereeUtil.SendNotificationToNextRefereeForFundingRequestReview(rr.RequestId, 0, RefereeType.Normal);
                            }
                            _RefereeUtil.ReturnToCommitteForPAEHeadToSelectMoreRefereeIfRequired(rr.RequestId);
                        }

                        //}
                    }
                }

                var session = UserSession;
                var db = Utils.db;
                var data = db.UserSessionDatas.Where(x => x.SessionId == UserSession.Id).ToList();
                SaveKeyValue<string>("Email", model.Email, data);
                SaveKeyValue<string>("ExternalRefereeHashParams", model.ParamsHash, data);

                if (!DidRefereeAgree || !String.IsNullOrEmpty(refereeUserId))
                {
                    _response.RedirectURL = Url.Action("Login", "Account", new { role = model.RoleCode });
                }
                else
                {
                    _response.RedirectURL = Url.Action("Register", "Account", new { role = model.RoleCode });
                }
            }
            else
            {
                _response.Status = false;
            }

            return Json(_response);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(PublishRequestViewModel model)
        {
            var pendingRequest = DbContext.Requests.FirstOrDefault(r => r.Type == (byte)RequestType.PublishRequest && r.CreatedBy == CurrentUser.Id && r.Status == (long)RequestStatus.Pending);
            if (pendingRequest != null)
            {
                TempData["message"] = ApsStrings.similar_request_already_exists;
                TempData["message_type"] = "danger";

                return RedirectToLastIndexPage("Index", "Home");
            }

            var user = Utils.GetCurrentUserDetails();

            ModelState.Remove("Action");

            if (ModelState.IsValid)
            {
                try
                {
                    var groupUser = DbContext.Users.FirstOrDefault();

                    var request = new Request
                    {
                        UniqueId = GenerateUniqueRequestId(),
                        Type = (byte)RequestType.PublishRequest,
                        LastStatusChanged = TM.Now,
                        LastStatusChangedBy = CurrentUser.Id,
                        Comments = model.Comments,
                        Status = (long)RequestStatus.Pending,
                        CurrentlyWith = groupUser.Id,
                        Created = TM.Now,
                        CreatedBy = CurrentUser.Id,
                        Updated = TM.Now,
                        UpdatedBy = CurrentUser.Id
                    };

                    var data = new List<RequestData>();

                    foreach (var prop in typeof(PublishRequestViewModel).GetProperties())
                    {
                        if (prop.PropertyType.FullName.Contains("List")) continue;
                        var value = model.GetPropValue(prop.Name);
                        if (value == null) continue;

                        data.Add(new RequestData
                        {
                            Name = prop.Name,
                            Value = value.ToString(),
                            Created = TM.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = TM.Now,
                            UpdatedBy = CurrentUser.Id
                        });
                    }

                    request.RequestDatas = data;

                    DbContext.Requests.Add(request);
                    DbContext.SaveChanges();

                    var log = new RequestHistoryLog
                    {
                        RequestId = request.Id,
                        Action = (byte)RequestActions.Submitted,
                        Comments = model.Comments,
                        IpAddress = Utils.GetUserIP(),
                        UserAgent = Request.UserAgent,
                        Url = Utils.GetFullUrl(),
                        Type = (byte)RequestHistoryTypes.SubmittedByMainResearcher,
                        CurrentlyWith = groupUser.Id,
                        Created = TM.Now,
                        CreatedBy = CurrentUser.Id,
                    };

                    request.LastActionId = log.Id;

                    var notification = new UserNotification
                    {
                        RequestId = request.Id,
                        Actionable = true,
                        IsEmailSent = false,
                        UserId = groupUser.Id,
                        Created = TM.Now,
                        CreatedBy = CurrentUser.Id,
                    };

                    DbContext.RequestHistoryLogs.Add(log);
                    DbContext.UserNotifications.Add(notification);
                    DbContext.SaveChanges();

                    TempData["message"] = string.Format(ApsStrings.request_created_successfully, request.UniqueId);
                    TempData["message_type"] = "success";
                    return RedirectToLastIndexPage("Index", "Home");
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

            model.Name = user.DbUser.UserProfile.GetFullName();

            return View(model);
        }

        [HttpGet]
        public ActionResult View(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return RedirectToLastIndexPage("Index", "Home");

            if (Utils.HasRolesAny(ApsUserRole.Researcher))
            {
                var req = DbContext.Requests.FirstOrDefault(r => r.Id.ToString() == id && r.CreatedBy == CurrentUser.Id);

                if (req != null && (req.Type == (byte)RequestType.PublishRequest || req.Type == (byte)RequestType.RepublishRequest))
                {
                    switch ((RequestStatus)req.Status)
                    {
                        case RequestStatus.PublishRequestNotSubmitted:
                            return RedirectToAction("Create", "Publications", new { requestId = req.Id });
                    }
                }
            }

            var request = RequestUtils.GetRequests()
                .Where(r => r.UniqueId == id || r.Id.ToString() == id).FirstOrDefault();

            if (request == null)
            {
                TempData["message"] = string.Format(ApsStrings.request_not_found, id);
                TempData["message_type"] = "danger";
                return RedirectToLastIndexPage("Index", "Home");
            }

            ViewBag.ShowRequestHistory = true;
            ViewBag.CanViewRefereeComments = false;

            var pendingUAction = request.RequestUsers
                .Where(ru => ru.Action == null && ru.UserId == CurrentUser.Id && ru.ReceivedOn == null)
                .OrderByDescending(a => a.AssignedOn)
                .FirstOrDefault();

            var isMember = Roles.First().ToString().EndsWith("Member");

            if (pendingUAction != null)
            {
                pendingUAction.ReceivedOn = TM.Now;
                DbContext.SaveChanges();
            }
            else if (isMember)
            {
                if (request.RequestUsers.Count(ru => ru.UserId == CurrentUser.Id) == 0)
                {
                    var reqUser = new RequestUser
                    {
                        RequestId = request.Id,
                        UserId = CurrentUser.Id,
                        AssignedOn = TM.Now,
                        AssignedBy = CurrentUser.Id,
                        Action = null,
                        SubmittedOn = null,
                        ReceivedOn = TM.Now,
                    };

                    DbContext.RequestUsers.Add(reqUser);
                    DbContext.SaveChanges();
                }
            }
            ViewBag.ShowRefereeFeedbackForm = false;
            ViewBag.RefereeId = 0;
            ViewBag.AllowToUploadAttachment = true;
            ViewBag.HideGrade = true;
            if (Utils.HasRolesAny(ApsUserRole.Referee))
            {
                RequestReferee pendingRAction = null;
                if (request.Status == (long)RequestStatus.PublishRequestPendingSpareReferee || request.Status == (long)RequestStatus.PublishRequestPendingReferees)
                {
                    pendingRAction = request.RequestReferees.Where(ru => ru.Referee.UserId == CurrentUser.Id && ru.RequestId == request.Id && ru.IsDeleted == false && ru.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString()).FirstOrDefault();
                }
                else
                {
                    pendingRAction = request.RequestReferees.Where(ru => ru.Referee.UserId == CurrentUser.Id && ru.RequestId == request.Id && ru.IsDeleted == false).FirstOrDefault();
                }
                if (pendingRAction != null)
                {
                    ViewBag.RefereeId = pendingRAction.RefereeId;
                    if (pendingRAction.Action == null)
                    {
                        pendingRAction.ReceivedOn = TM.Now;
                        DbContext.SaveChanges();
                        if (pendingRAction.DidRefereeAgree == null)
                        {
                            ViewBag.AllowToUploadAttachment = false;
                            ViewBag.HideGrade = false;
                        }
                    }
                    else if (pendingRAction.Action != null && pendingRAction.Action != (byte)RequestAction.RejectToReview)
                    {
                        ViewBag.ShowRefereeFeedbackForm = true;
                    }
                }
            }

            var viewName = "View";
            var modelType = typeof(RequestViewModel);
            var model = (RequestViewModel)null;
            var type = (RequestType)request.Type;
            var user = Utils.GetCurrentUserDetails();
            var distributionList = new List<DistributionEntityViewModel>();
            var isResearcher = Utils.HasRolesAny(ApsUserRole.Researcher);
            var isReferee = Utils.HasRolesAny(ApsUserRole.Referee);

            switch (type)
            {
                case RequestType.PublishRequest:
                case RequestType.RepublishRequest:
                default:

                    if (type == RequestType.PublishRequest || type == RequestType.RepublishRequest)
                    {
                        viewName = "ViewPublishRequest";
                    }
                    else
                    {
                        var req = DbContext.Requests.Find(request.Id);
                        var researcher = new RequestSummaryViewModel2();
                        if (Utils.HasRolesAny(ApsUserRole.KingFahadNationalLibraryOfficer))
                        {
                            researcher.UserProfilesNationalId = req.CreatedByUser.UserProfile.NationalId;
                            researcher.ResearchersFirstName = req.CreatedByUser.UserProfile.GetFullName();
                            researcher.UserProfilesGender = req.CreatedByUser.UserProfile.Gender == true ? ApsStrings.male : (req.CreatedByUser.UserProfile.Gender == false ? ApsStrings.female : "");
                            researcher.ResearchersDateOfBirth = req.CreatedByUser.UserProfile.BirthDate == null ? "" : req.CreatedByUser.UserProfile.BirthDate.Value.FormatDate(false, true, true);
                            researcher.ResearchersEmail = req.CreatedByUser.Email;
                            researcher.ResearchersMobileNo = req.CreatedByUser.PhoneNumber;
                            researcher.RequestsTitle = req.Title;
                            researcher.RequestsCollegeId = req.College.NameArabic;
                            researcher.RequestsType = LangHelpers.Instance[((RequestType)req.Type).ToString()];
                            researcher.RequestsPublicationType = LangHelpers.Instance["type_" + ((PublicationType)req.PublicationType).ToString()];
                            researcher.BookRequestsBookTitle = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) ? "" : req.PublicationRequest.BookRequest.BookTitle;
                            researcher.BookRequestsOriginalTitleBook = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) ? "" : req.PublicationRequest.BookRequest.OriginalTitleBook;
                            researcher.BookRequestsBookType = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) || !(req.PublicationRequest.BookRequest.BookType > 0) ? "" : LangHelpers.Instance[((BookType)req.PublicationRequest.BookRequest.BookType).ToString()];
                            researcher.BookRequestsIsArabic = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) ? "" : req.PublicationRequest.BookRequest.IsArabic ? ApsStrings.yes : ApsStrings.no;
                            researcher.BookRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) ? "" : req.PublicationRequest.BookRequest.IsTranslated ? ApsStrings.yes : ApsStrings.no;
                            researcher.BookRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) ? "" : req.PublicationRequest.BookRequest.TranslatedTo;
                            researcher.BookRequestsMajorId = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) || req.PublicationRequest.BookRequest.Major == null ? "" : req.PublicationRequest.BookRequest.Major.NameArabic;
                            researcher.BookRequestsNoOfRecommendations = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) || !(req.PublicationRequest.BookRequest.NoOfRecommendations > 0) ? "" : req.PublicationRequest.BookRequest.NoOfRecommendations.Value + "";
                            researcher.BookRequestsRecommendationDate = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) || req.PublicationRequest.BookRequest.RecommendationDate == null ? "" : req.PublicationRequest.BookRequest.RecommendationDate.Value.FormatDate(false, ArabicFormat: true, includeSuffix: true);
                            researcher.BookRequestsPublishType = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) || req.PublicationRequest.BookRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.PublicationRequest.BookRequest.PublishType.Value).ToString()];
                            researcher.BookRequestsPublisher = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) ? "" : req.PublicationRequest.BookRequest.Publisher;
                            researcher.BookRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) ? "" : req.PublicationRequest.BookRequest.PublisherPlace;
                            researcher.BookRequestsPublishingDate = req.PublicationType != (byte)PublicationType.Book || req.PublicationRequest.BookRequest == null || !(req.PublicationRequest.BookRequest.RequestId > 0) || req.PublicationRequest.BookRequest.PublishingDate == null ? "" : req.PublicationRequest.BookRequest.PublishingDate.Value.FormatDate(false, ArabicFormat: true, includeSuffix: true);
                            researcher.ReviewRequestsReviewType = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) || !(req.PublicationRequest.ReviewRequest.ReviewType > 0) ? "" : LangHelpers.Instance[((ProbeType)req.PublicationRequest.ReviewRequest.ReviewType).ToString()];
                            researcher.ReviewRequestsAuthersName = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) ? "" : req.PublicationRequest.ReviewRequest.AuthersName;
                            researcher.ReviewRequestsBookTitle = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) ? "" : req.PublicationRequest.ReviewRequest.BookTitle;
                            researcher.ReviewRequestsScopOfManuscript = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) ? "" : req.PublicationRequest.ReviewRequest.ScopOfManuscript.ToString();
                            researcher.ReviewRequestsReviewMajor = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) || !(req.PublicationRequest.ReviewRequest.ReviewMajor > 0) ? "" : req.PublicationRequest.ReviewRequest.Major.NameArabic;
                            researcher.ReviewRequestsPublishType = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) || req.PublicationRequest.ReviewRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.PublicationRequest.ReviewRequest.PublishType.Value).ToString()];
                            researcher.ReviewRequestsPublisher = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) ? "" : req.PublicationRequest.ReviewRequest.Publisher;
                            researcher.ReviewRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) ? "" : req.PublicationRequest.ReviewRequest.PublisherPlace;
                            researcher.ReviewRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Probe || req.PublicationRequest.ReviewRequest == null || !(req.PublicationRequest.ReviewRequest.RequestId > 0) || req.PublicationRequest.ReviewRequest.PublisherDate == null ? "" : req.PublicationRequest.ReviewRequest.PublisherDate.Value.FormatDate(false, true, true);
                            researcher.DictionaryRequestsDictionaryMajor = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) || !(req.PublicationRequest.DictionaryRequest.DictionaryMajor > 0) ? "" : req.PublicationRequest.DictionaryRequest.Major_DictionaryMajor.NameArabic;
                            researcher.DictionaryRequestsIsArabic = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) ? "" : req.PublicationRequest.DictionaryRequest.IsArabic == true ? ApsStrings.yes : ApsStrings.no;
                            researcher.DictionaryRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) ? "" : req.PublicationRequest.DictionaryRequest.IsTranslated == true ? ApsStrings.yes : ApsStrings.no;
                            researcher.DictionaryRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) ? "" : req.PublicationRequest.DictionaryRequest.TranslatedTo;
                            researcher.DictionaryRequestsDictionaryTitle = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) ? "" : req.PublicationRequest.DictionaryRequest.DictionaryTitle;
                            researcher.DictionaryRequestsOriginalDictionaryTitle = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) ? "" : req.PublicationRequest.DictionaryRequest.OriginalDictionaryTitle;
                            researcher.DictionaryRequestsMajorOfAuther = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) || !(req.PublicationRequest.DictionaryRequest.MajorOfAuther > 0) ? "" : req.PublicationRequest.DictionaryRequest.Major_MajorOfAuther.NameArabic;
                            researcher.DictionaryRequestsNoOfRecommendations = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) || !(req.PublicationRequest.DictionaryRequest.NoOfRecommendations > 0) ? "" : req.PublicationRequest.DictionaryRequest.NoOfRecommendations + "";
                            researcher.DictionaryRequestsRecommendationDate = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) || req.PublicationRequest.DictionaryRequest.RecommendationDate == null ? "" : req.PublicationRequest.DictionaryRequest.RecommendationDate.FormatDate(false, true, true);
                            researcher.DictionaryRequestsPublishType = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) || req.PublicationRequest.DictionaryRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.PublicationRequest.DictionaryRequest.PublishType.Value).ToString()];
                            researcher.DictionaryRequestsPublisher = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) ? "" : req.PublicationRequest.DictionaryRequest.Publisher;
                            researcher.DictionaryRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) ? "" : req.PublicationRequest.DictionaryRequest.PublisherPlace;
                            researcher.DictionaryRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Dictionary || req.PublicationRequest.DictionaryRequest == null || !(req.PublicationRequest.DictionaryRequest.RequestId > 0) || req.PublicationRequest.DictionaryRequest.PublisherDate == null ? "" : req.PublicationRequest.DictionaryRequest.PublisherDate.Value.FormatDate(false, true, true);
                            researcher.EncyclopediaRequestsMajorOfEncyclopedia = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) || !(req.PublicationRequest.EncyclopediaRequest.MajorOfEncyclopedia > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.Major_MajorOfEncyclopedia.NameArabic;
                            researcher.EncyclopediaRequestsIsArabic = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.IsArabic == true ? ApsStrings.yes : ApsStrings.no;
                            researcher.EncyclopediaRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.IsTranslated == true ? ApsStrings.yes : ApsStrings.no;
                            researcher.EncyclopediaRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.TranslatedTo;
                            researcher.EncyclopediaRequestsEncyclopediaTitle = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.EncyclopediaTitle;
                            researcher.EncyclopediaRequestsOriginalEncyclopediaTitle = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.OriginalEncyclopediaTitle;
                            researcher.EncyclopediaRequestsMajorOfAuther = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) || !(req.PublicationRequest.EncyclopediaRequest.MajorOfAuther > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.Major_MajorOfAuther.NameArabic;
                            researcher.EncyclopediaRequestsNoOfRecommendations = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) || !(req.PublicationRequest.EncyclopediaRequest.NoOfRecommendations > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.NoOfRecommendations + "";
                            researcher.EncyclopediaRequestsRecommendationDate = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) || req.PublicationRequest.EncyclopediaRequest.RecommendationDate == null ? "" : req.PublicationRequest.EncyclopediaRequest.RecommendationDate.FormatDate(false, true, true);
                            researcher.EncyclopediaRequestsPublishType = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) || req.PublicationRequest.EncyclopediaRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.PublicationRequest.EncyclopediaRequest.PublishType).ToString()];
                            researcher.EncyclopediaRequestsPublisher = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.Publisher;
                            researcher.EncyclopediaRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) ? "" : req.PublicationRequest.EncyclopediaRequest.PublisherPlace;
                            researcher.EncyclopediaRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Encyclopedia || req.PublicationRequest.EncyclopediaRequest == null || !(req.PublicationRequest.EncyclopediaRequest.RequestId > 0) || req.PublicationRequest.EncyclopediaRequest.PublisherDate == null ? "" : req.PublicationRequest.EncyclopediaRequest.PublisherDate.Value.FormatDate(false, true, true);
                            researcher.HandbookRequestsHandbookType = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) || req.PublicationRequest.HandbookRequest.HandbookType == 0 ? "" : LangHelpers.Instance[((HandBookType)req.PublicationRequest.HandbookRequest.HandbookType).ToString()];
                            researcher.HandbookRequestsIsArabic = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) ? "" : req.PublicationRequest.HandbookRequest.IsArabic == true ? ApsStrings.yes : ApsStrings.no;
                            researcher.HandbookRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) ? "" : req.PublicationRequest.HandbookRequest.IsTranslated == true ? ApsStrings.yes : ApsStrings.no;
                            researcher.HandbookRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) ? "" : req.PublicationRequest.HandbookRequest.TranslatedTo;
                            researcher.HandbookRequestsHandbookTitle = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) ? "" : req.PublicationRequest.HandbookRequest.HandbookTitle;
                            researcher.HandbookRequestsOriginalHandbookTitle = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) ? "" : req.PublicationRequest.HandbookRequest.OriginalHandbookTitle;
                            researcher.HandbookRequestsMajorId = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) || !(req.PublicationRequest.HandbookRequest.MajorId > 0) ? "" : req.PublicationRequest.HandbookRequest.Major.NameArabic;
                            researcher.HandbookRequestsPublishType = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) || req.PublicationRequest.HandbookRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.PublicationRequest.HandbookRequest.PublishType).ToString()];
                            researcher.HandbookRequestsPublisher = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) ? "" : req.PublicationRequest.HandbookRequest.Publisher;
                            researcher.HandbookRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) ? "" : req.PublicationRequest.HandbookRequest.PublisherPlace;
                            researcher.HandbookRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Handbook || req.PublicationRequest.HandbookRequest == null || !(req.PublicationRequest.HandbookRequest.RequestId > 0) || req.PublicationRequest.HandbookRequest.PublisherDate == null ? "" : req.PublicationRequest.HandbookRequest.PublisherDate.Value.FormatDate(false, true, true);

                            researcher.ThesisRequestsSciDepartmentId = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) || !(req.PublicationRequest.ThesisRequest.SciDepartmentId > 0) ? "" : req.PublicationRequest.ThesisRequest.SciDepartmentId + "";
                            researcher.ThesisRequestsSupervisor = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.Supervisor;
                            researcher.ThesisRequestsAsstSupervisor = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.AssistantSupervisor;
                            researcher.ThesisRequestsThesisTitle = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.ThesisTitle;
                            researcher.ThesisRequestsCommitteeMembers = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.CommitteeMembers;
                            researcher.ThesisRequestsDegree = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) || !(req.PublicationRequest.ThesisRequest.Degree > 0) ? "" : LangHelpers.Instance[((Degree)req.PublicationRequest.ThesisRequest.Degree).ToString()];
                            researcher.ThesisRequestsGPA = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : LangHelpers.Instance[req.PublicationRequest.ThesisRequest.Gpa];
                            researcher.ThesisRequestsDateOfDiscussion = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) || req.PublicationRequest.ThesisRequest.DateOfDiscussion == null ? "" : req.PublicationRequest.ThesisRequest.DateOfDiscussion.FormatDate(false, true, true);
                            researcher.ThesisRequestsPublishType = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) || !(req.PublicationRequest.ThesisRequest.PublishType > 0) ? "" : LangHelpers.Instance[((PublishType)req.PublicationRequest.ThesisRequest.PublishType).ToString()];
                            researcher.ThesisRequestsPublisher = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.Publisher;
                            researcher.ThesisRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.PublisherPlace;
                            researcher.ThesisRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) || req.PublicationRequest.ThesisRequest.PublisherDate == null ? "" : req.PublicationRequest.ThesisRequest.PublisherDate.Value.FormatDate(false, true, true);
                            researcher.ThesisRequestsMajorId = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : (!(req.PublicationRequest.ThesisRequest.MajorId > 0) ? req.PublicationRequest.ThesisRequest.MajorOther : req.PublicationRequest.ThesisRequest.Major.NameArabic);
                            researcher.ThesisRequestsCollegeId = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : (!(req.PublicationRequest.ThesisRequest.CollegeId > 0) ? req.PublicationRequest.ThesisRequest.CollegeOther : req.PublicationRequest.ThesisRequest.College.NameArabic);
                            researcher.ThesisRequestsUniversityOther = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.UniversityOther;
                            researcher.ThesisRequestsMajorOther = req.PublicationType != (byte)PublicationType.Thesis || req.PublicationRequest.ThesisRequest == null || !(req.PublicationRequest.ThesisRequest.RequestId > 0) ? "" : req.PublicationRequest.ThesisRequest.MajorOther;

                            researcher.RequestsIsScientificProduction = req.IsScientificProduction == 1 ? ApsStrings.yes : ApsStrings.no;
                        }

                        if (Utils.HasRolesAny(ApsUserRole.KingFahadNationalLibraryOfficer) ||
                            (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit) && (request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandInitiation || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading
                            || request.Status == (long)RequestStatus.OperationCommandPendingResearcherProofreadingAcceptance || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandPreparation || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward || request.Status == (long)RequestStatus.OperationCommandPendingResearcherComments || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward1AndHalf || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandExecution2 || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward2 || request.Status == (long)RequestStatus.OperationCommandPendingResearcherComments2 || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward2AndHalf || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandExecution3 || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward3 || request.Status == (long)RequestStatus.OperationCommandPendingResearcherAcceptance)) ||
                            (Utils.HasRolesAny(ApsUserRole.Proofreader) && request.Status == (long)RequestStatus.OperationCommandPendingPendingCorrection) ||
                            (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch) && (request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandApproval || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandValidation)) ||
                            (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch) && (request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandPreparation || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandApproval)) ||
                            (Utils.HasRolesAny(ApsUserRole.Researcher) && (request.Status == (long)RequestStatus.OperationCommandPendingResearcherProofreadingAcceptance || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading || request.Status == (long)RequestStatus.OperationCommandPendingResearcherComments || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward1AndHalf || request.Status == (long)RequestStatus.OperationCommandPendingResearcherComments2 || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward2AndHalf || request.Status == (long)RequestStatus.OperationCommandPendingResearcherAcceptance || request.Status == (long)RequestStatus.OperationCommandPendingISBNRequestSubmission)) ||
                            (Utils.HasRolesAny(ApsUserRole.UniversityPressDirector) && (request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandValidation || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandExecution)) ||
                             (Utils.HasRolesAny(ApsUserRole.UniversityPressOfficer) && (request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandExecution || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandExecution2 || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward2 || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward2AndHalf || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandExecution3 || request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandForward3))
                            )
                        {
                            researcher.Files = Utils.db.RequestFiles.Where(x => req.PublicationRequestId == x.RequestId)
                           .ToList().Select(f => new AttachedFile
                           {
                               FileId = f.Id.ToString(),
                               Type = (FileType)f.Type,
                               FileName = Path.GetFileName(f.FileName)
                           }).ToList();
                        }
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit) && request.Status == (long)RequestStatus.DistributionPendingDistributionListPreparation)
                        {
                            distributionList = Utils.db.DistributionEntities
                                                    .Where(x => x.IsDeleted == false)
                                                    .ToList()
                                                    .Select(d => new DistributionEntityViewModel
                                                    {
                                                        Name = d.Name,
                                                        Id = d.Id,
                                                        NoOfCopies = d.NoOfCopies
                                                    }).ToList();
                        }
                        else if ((Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch) && (request.Status == (long)RequestStatus.DistributionPendingDistributionListApprovalViceDean || request.Status == (long)RequestStatus.DistributionPendingDistributionListApproval)) ||
                            (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch) && (request.Status == (long)RequestStatus.DistributionPendingDistributionListApproval || request.Status == (long)RequestStatus.DistributionPendingCopyDistribution)) ||
                            (Utils.HasRolesAny(ApsUserRole.PublicRelationsDirector) && (request.Status == (long)RequestStatus.DistributionPendingCopyDistribution || request.Status == (long)RequestStatus.DistributionPendingDistributionClosure)) ||
                            (Utils.HasRolesAny(ApsUserRole.Researcher) && request.Status == (long)RequestStatus.DistributionPendingResearcherConfirmation) ||
                            (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit) && (request.Status == (long)RequestStatus.DistributionPendingKFNLCopiesDistributionByPrintingUnit || request.Status == (long)RequestStatus.DistributionPendingDistributionClosure || request.Status == (long)RequestStatus.DistributionPendingDistributionListApprovalViceDean || request.Status == (long)RequestStatus.Completed)) ||
                            (Utils.HasRolesAny(ApsUserRole.DeanOfLibraryAffairs) && (request.Status == (long)RequestStatus.DistributionPendingCopiesDistributionByLibraryAffairs || request.Status == (long)RequestStatus.DistributionPendingCopyDistribution))
                            )
                        {
                            distributionList = Utils.db.RequestDistributionListAgencies
                                                    .Where(x => x.RequestId == request.Id)
                                                    .ToList()
                                                    .Select(d => new DistributionEntityViewModel
                                                    {
                                                        Name = d.Name,
                                                        Id = d.Id,
                                                        NoOfCopies = d.NoOfCopies,
                                                        IsSelected = true
                                                    }).ToList();
                        }

                        ViewBag.Researcher = researcher;
                        viewName = "ViewGeneric";
                    }

                    model = new PublishRequestViewModel();
                    model.RequestType = (RequestType)request.Type;
                    modelType = typeof(PublishRequestViewModel);
                    model.Title = request.Title;

                    model.DistributionEntities = distributionList;

                    var model2 = (PublishRequestViewModel)model;
                    model2.StatusNO = request.Status;
                    model2.PubType = (PublicationType)request.PublicationType;

                    if (isReferee)
                    {
                        ViewBag.ShowRequestHistory = false;
                    }

                    if (isResearcher && request.CreatedBy == CurrentUser.Id && request.Status == (long)RequestStatus.PublishRequestPendingModificationsAfterReferees)
                    {
                        ViewBag.CanViewRefereeComments = true;
                    }

                    if (type == RequestType.PublishRequest || type == RequestType.RepublishRequest)
                    {
                        if (request.CreatedByUser != null && request.CreatedByUser.Researchers.FirstOrDefault() != null)
                        {
                            model2.AuthorDetails = new AuthorViewModel
                            {
                                Id = request.CreatedByUser.Researchers.First().Id + "",
                                Name = request.CreatedByUser.Researchers.First().GetFullName()
                            };
                        }
                        else
                        {
                            model2.AuthorDetails = new AuthorViewModel();
                        }
                    }
                    else if (type == RequestType.OperationCommand || type == RequestType.Pricing || type == RequestType.Distribution)
                    {
                        model2.AuthorDetails = new AuthorViewModel
                        {
                            Id = request.PublicationRequest == null ? "" : request.PublicationRequest.CreatedByUser.Researchers.First().Id + "",
                            Name = request.PublicationRequest == null ? "" : request.PublicationRequest.CreatedByUser.Researchers.First().GetFullName()
                        };
                        model2.ISBN = request.Isbn;
                        model2.NoOfCopies = request.NoOfCopies;
                    }
                    else if (type == RequestType.ProofreaderReward || type == RequestType.RefereeReward || type == RequestType.ResearcherReward)
                    {
                        model2.AuthorDetails = new AuthorViewModel
                        {
                            Id = request.RewardRequest.User.Id,
                            Name = request.RewardRequest.User.UserProfile.GetFullName(),
                        };

                        if (request.RewardRequest.User.UserProfile != null)
                        {
                            model2.AuthorDetails.AccountBank = request.RewardRequest.User.UserProfile.Bank == null ? "" : request.RewardRequest.User.UserProfile.Bank.NameArabic;
                            model2.AuthorDetails.AccountName = request.RewardRequest.User.UserProfile.BankAccountHolder;
                            model2.AuthorDetails.AccountIBAN = request.RewardRequest.User.UserProfile.BankIban;
                            model2.AuthorDetails.IbanImage = request.RewardRequest.User.UserFiles.Where(x => x.Type == (byte)FileType.IbanPhotocopy).Select(x => new AttachedFile { IsDownloaded = true, FileId = x.Id.ToString(), FileName = x.FileName, Type = (FileType)x.Type }).FirstOrDefault();
                            if (model2.AuthorDetails.IbanImage == null)
                                model2.AuthorDetails.IbanImage = request.RewardRequest.User.UserFiles.Where(x => x.Type == (byte)FileType.SwiftPhotocopy).Select(x => new AttachedFile { IsDownloaded = true, FileId = x.Id.ToString(), FileName = x.FileName, Type = (FileType)x.Type }).FirstOrDefault();
                        }
                    }
                    if (type == RequestType.Distribution)
                    {
                        var noOfCopies = DbContext.Requests.Where(x => x.PublicationRequestId == request.PublicationRequestId && x.Type == (byte)RequestType.OperationCommand).FirstOrDefault().NoOfCopies;
                        model2.NoOfCopies = noOfCopies;
                    }

                    switch (model2.PubType)
                    {
                        case PublicationType.Book:
                            break;
                    }

                    var workflow = (IRequestWorkflow)null;

                    switch ((RequestType)request.Type)
                    {
                        case RequestType.PublishRequest:
                        case RequestType.RepublishRequest:
                            workflow = new PublishRequestWorkflow();
                            break;

                        case RequestType.OperationCommand:
                            workflow = new OperationCommandRequestWorkflow();
                            break;

                        case RequestType.Pricing:
                            workflow = new PricingRequestWorkflow();
                            break;

                        case RequestType.Distribution:
                            workflow = new DistributionRequestWorkflow();
                            break;

                        case RequestType.RefereeReward:
                            workflow = new RefereeRewardWorkflow();
                            break;

                        case RequestType.ResearcherReward:
                            workflow = new ResearcherRewardWorkflow();
                            break;

                        case RequestType.ProofreaderReward:
                            workflow = new ProofReaderRewardWorkflow();
                            break;
                    }

                    model.AllowEdit = false;

                    if (request.Type == (byte)RequestType.Pricing && request.Status == (long)RequestStatus.PricingPendingPricingPrepartion)
                    {
                        model2.PageCount = request.NoOfPages;
                        model2.Price = request.AmountSar;
                        model.AllowEdit = true;
                    }
                    else if (((RequestType)request.Type).ToString().EndsWith("Reward"))
                    {
                        var rewardAmount = (double)0;
                        var publishReqId = Utils.RequestUtils.DbContext.Requests.Where(x => x.Id == request.PublicationRequestId).FirstOrDefault().PublicationRequestId;
                        var publishReq = Utils.RequestUtils.DbContext.Requests.Where(x => x.Id == publishReqId).FirstOrDefault();
                        if (request.Status == (long)RequestStatus.RewardPendingApprovalByViceDean)
                        {
                            switch ((RequestType)request.Type)
                            {
                                case RequestType.ResearcherReward:
                                    switch ((PublicationType)request.PublicationType)
                                    {
                                        case PublicationType.Book:
                                        case PublicationType.Dictionary:
                                        case PublicationType.Encyclopedia:
                                        case PublicationType.Probe:
                                        case PublicationType.Handbook:
                                            var noOfPages = (int)publishReq.NoOfPages;
                                            var avgGrade = (double)publishReq.RequestReferees
                                                           .Where(rr => rr.Action != null)
                                                           .OrderByDescending(rr => rr.Grade)
                                                           .Skip(0)
                                                           .Take(2)
                                                           .Average(x => x.Grade);
                                            rewardAmount = (noOfPages * avgGrade) >= 50000 ? 50000 : (noOfPages * avgGrade);
                                            break;

                                        case PublicationType.Thesis:
                                            if (publishReq.ThesisRequest != null && publishReq.ThesisRequest.Degree > 0)
                                            {
                                                if (((Degree)publishReq.ThesisRequest.Degree) == Degree.Phd)
                                                {
                                                    rewardAmount = DbContext.ResearcherRewards.FirstOrDefault().Phd;
                                                }
                                                else
                                                {
                                                    rewardAmount = DbContext.ResearcherRewards.FirstOrDefault().Master;
                                                }
                                            }
                                            else
                                            {
                                                rewardAmount = DbContext.ResearcherRewards.FirstOrDefault().Master;
                                            }
                                            break;
                                    }
                                    break;

                                case RequestType.RefereeReward:
                                    break;

                                case RequestType.ProofreaderReward:
                                    break;
                            }
                        }
                        else
                        {
                            rewardAmount = (double)request.RewardRequest.Amount;
                        }

                        model2.Price = rewardAmount;
                        model.AllowEdit = true;
                    }
                    else if (((RequestType)request.Type) == RequestType.OperationCommand && request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandFinalApproval)
                    {
                        model.AllowEdit = true;
                    }

                    if (request.Type == (byte)RequestType.Pricing || request.Type == (byte)RequestType.OperationCommand)
                    {
                        model2.PageCount = request.NoOfPages;
                        if (request.AmountSar != null)
                        {
                            model2.Price = request.AmountSar;
                        }
                        else
                        {
                            if (request.NoOfPages != null)
                                model2.Price = Math.Round((double)request.NoOfPages / 20);
                        }
                    }

                    if (request.Type == (byte)RequestType.Distribution)
                    {
                        var isArabic = Utils.IsUserLanguageArabic;
                        model2.DistributionList = request.RequestDistributionListAgencies.Select(x => new DistributionListModel
                        {
                            Id = x.AgencyId,
                            //Name = isArabic ? x.Agency.NameArabic : x.Agency.NameEnglish,
                            Name = x.DistributionEntity.Name,
                            NoOfCopies = x.NoOfCopies
                        }).ToList();

                        var otherList = DbContext.Agencies.Select(x => new DistributionListModel
                        {
                            Id = x.Id,
                            Name = isArabic ? x.NameArabic : x.NameEnglish,
                            NoOfCopies = 0
                        }).ToList();

                        model2.DistributionList.AddRange(otherList.Where(x => model2.DistributionList.Count(x2 => x2.Id == x.Id) == 0));

                        model2.DistributionList = model2.DistributionList.OrderByDescending(x => x.Name).ToList();
                    }

                    var requestModel = ToListModel(request.Id);

                    model2.AllowedActions = workflow.GetAllowedActions(requestModel).Select(a => new RequestActionViewModel
                    {
                        Action = a,
                        Display = LangHelpers.Instance["action_" + a.ToString()],
                        Value = a.ToString()
                    }).ToList();

                    model2.AllowedActions.Insert(0, new RequestActionViewModel
                    {
                        Value = "",
                        Display = ApsStrings.please_select_action
                    });

                    model2.CanSelectReferees = model2.AllowedActions.Count(a => a.Action == RequestAction.ForwardToInitialReferee || a.Action == RequestAction.SelectReferees) > 0;

                    if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead) && (request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead || request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers))
                    {
                        model2.CanSelectReferees = true;
                    }

                    model2.CanViewReferees = request.RequestReferees.Count(rr => rr.Action != null && rr.Action != (byte)RequestAction.Canceled && rr.IsDeleted == false) > 0;

                    if (!model2.CanViewReferees && Utils.HasRolesAny(ApsUserRole.CommitteeforPAESecretary, ApsUserRole.CommitteForPAEHead, ApsUserRole.AcademicCouncilHead, ApsUserRole.AcademicCouncilMember, ApsUserRole.AcademicCouncilSecretary, ApsUserRole.UniversityRector, ApsUserRole.DeanOfAcademicResearch, ApsUserRole.ViceDeanOfAcademicResearch, ApsUserRole.UniversityRector))
                    {
                        model2.CanViewReferees = true;
                    }

                    if (model2.CanSelectReferees)
                    {
                        if (model2.AllowedActions.Count(a => a.Action == RequestAction.ForwardToInitialReferee) > 0)
                        {
                            var referees1 = UserUtils.GetReferees(null, false, LangHelpers.Instance);
                            model2.MinRefereeCount = 1;
                            model2.MaxRefereeCount = referees1.Count();
                        }
                        else
                        {
                            if (isMember)
                            {
                                model2.MinRefereeCount = 1;
                            }
                            else
                            {
                                model2.MinRefereeCount = 3;
                            }

                            model2.MaxRefereeCount = 10;
                        }
                    }

                    model2.CanSeeCouncilMembers = false;

                    var role = (ApsUserRole?)null;

                    if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead, ApsUserRole.CommitteeforPAEMember))
                    {
                        role = ApsUserRole.CommitteeforPAEMember;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch, ApsUserRole.DeanshipOfAcademicResearchCouncilMember))
                    {
                        role = ApsUserRole.DeanshipOfAcademicResearchCouncilMember;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead, ApsUserRole.AcademicCouncilMember))
                    {
                        role = ApsUserRole.AcademicCouncilMember;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead, ApsUserRole.ResearchUnitCouncilMember))
                    {
                        role = ApsUserRole.ResearchUnitCouncilMember;
                    }

                    if (role != null)
                    {
                        model2.CanSeeCouncilMembers = true;

                        model2.CouncilMembers = request.RequestUsers
                            .Where(ru => ru.Action != null && ru.Action != (byte)RequestAction.Canceled && ru.User.Roles.Count(r => r.Name == role.ToString()) > 0)
                            .ToList()
                            .Select(r => new RefereeViewModel
                            {
                                Id = r.UserId,
                                FullName = r.User.UserProfile.GetFullName(),
                                Action = r.Action == null ? "" : LangHelpers.Instance[((RequestAction)r.Action.Value).ToString()],
                                Comments = r.Comments,
                                SubmittedOn = r.SubmittedOn,
                                FileId = r.FileId == null ? "" : r.FileId.ToString(),
                                FileName = string.IsNullOrWhiteSpace(r.FileName) ? (r.RequestFile == null ? ApsStrings.member_opinion : LangHelpers.Instance[((FileType)r.RequestFile.Type).ToString()]) : r.FileName
                            })
                            .OrderByDescending(r => r.SubmittedOn)
                            .ToList();

                        var newList = new List<RefereeViewModel>();

                        foreach (var cm in model2.CouncilMembers)
                        {
                            if (newList.Count(x => x.Id == cm.Id) > 0) continue;

                            newList.Add(cm);
                        }

                        model2.CouncilMembers = newList;
                    }

                    var referees = request.RequestReferees.Where(r => r.IsDeleted == false)
                        .OrderByDescending(r => r.Updated)
                        .Select((r, ii) => new RefereeViewModel
                        {
                            Id = r.RefereeId + "",
                            UserId = r.Referee.UserId,
                            FullName = isResearcher ? ApsStrings.referee + " " + (ii + 1) : r.Referee.GetFullName(),
                            MajorName = r.Referee.MajorId == null ? "" : r.Referee.Major.NameArabic,
                            Comments = r.Comments,
                            Grade = r.Grade == null ? 0 : (int)r.Grade,
                            FileId = r.RequestFileId == null ? request.RequestFiles.Where(x => x.CreatedBy == r.Referee.UserId).Select(f => f.Id.ToString()).FirstOrDefault() : r.RequestFileId.Value.ToString(),
                            FileName = r.RequestFileId == null ? request.RequestFiles.Where(x => x.CreatedBy == r.Referee.UserId).Select(f => f.FileName).FirstOrDefault() : r.RequestFile.FileName,
                            ReviewSentOnStr = r.RequestReviewSentOn != null ? r.RequestReviewSentOn.Value.FormatDate(true) : "",
                            //NumberOfVotes = r.NumberOfVotes,
                            Action = r.Action == null ? "" : LangHelpers.Instance[((RequestAction)r.Action.Value).ToString()],
                            SubmittedOn = r.SubmittedOn,
                            CreatedUserRole = r.CreatedByUser.Roles.First().Name.ToString(),
                        })
                    .ToList();

                    if (model2.CanSelectReferees)
                    {
                        referees = referees.OrderByDescending(r => r.NumberOfVotes).ToList();
                    }
                    else
                    {
                        referees = referees.OrderByDescending(r => r.SubmittedOn).ToList();
                    }

                    var index = 0;
                    model2.Referees = new List<RefereeViewModel>();
                    var createdUserRole = "";
                    var createdUserRole2 = "";
                    if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
                    {
                        createdUserRole = "ResearchUnitHead";
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                    {
                        createdUserRole = "DeanOfAcademicResearch";
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead, ApsUserRole.CommitteeforPAESecretary, ApsUserRole.CommitteeforPAEMember) ||
                     (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead) && (RequestStatus)request.Status == RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead)
                     )
                    {
                        createdUserRole = "CommitteForPAEHead";
                        createdUserRole2 = "AcademicCouncilHead";
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead, ApsUserRole.Researcher, ApsUserRole.UniversityRector, ApsUserRole.AcademicCouncilMember))
                    {
                        createdUserRole = "AcademicCouncilHead";
                    }
                    foreach (var r in referees)
                    {
                        if (model2.Referees.Count(r2 => r2.Id == r.Id) > 0) continue;
                        //if (!createdUserRole.Contains(r.CreatedUserRole)) continue;
                        if (createdUserRole2 == "")
                        {
                            if (!createdUserRole.Contains(r.CreatedUserRole)) continue;
                        }
                        else
                        {
                            if (!createdUserRole.Contains(r.CreatedUserRole) && !createdUserRole2.Contains(r.CreatedUserRole)) continue;
                        }

                        model2.Referees.Add(r);
                        index++;
                    }

                    // Number of Votes
                    foreach (var r in model2.Referees)
                    {
                        if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
                        {
                            r.NumberOfVotes = request.RequestReferees.Where(r2 => r2.RefereeId.ToString() == r.Id && (r2.CreatedByUser.Roles.FirstOrDefault().Name == "CommitteeforPAEMember") && r2.IsDeleted == false).Sum(x => x.NumberOfVotes);
                        }
                    }

                    if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead) && (RequestStatus)request.Status == RequestStatus.PublishRequestPendingPAEAfterRefereesHead)
                    {
                        if (model2.Referees.Count == 0)
                        {
                            model2.Referees = referees.Where(x => x.CreatedUserRole == "CommitteForPAEHead" || x.CreatedUserRole == "AcademicCouncilHead").ToList();
                        }
                    }
                    if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember))
                    {
                        var selectedreferees = request.RequestReferees.Where(x => x.CreatedBy == CurrentUser.Id && x.IsDeleted == false).Select((r, ii) => new RefereeViewModel
                        {
                            Id = r.RefereeId + "",
                            UserId = r.Referee.UserId,
                            FullName = isResearcher ? ApsStrings.referee + " " + (ii + 1) : r.Referee.GetFullName(),
                            MajorName = r.Referee.MajorId == null ? "" : r.Referee.Major.NameArabic,
                            Comments = r.Comments,
                            Grade = r.Grade == null ? 0 : (int)r.Grade,
                            FileId = r.RequestFileId == null ? null : r.RequestFileId.Value.ToString(),
                            FileName = r.RequestFileId == null ? null : r.RequestFile.FileName,
                            ReviewSentOnStr = r.RequestReviewSentOn != null ? r.RequestReviewSentOn.Value.FormatDate(true) : "",
                            //NumberOfVotes = r.NumberOfVotes,
                            Action = r.Action == null ? "" : LangHelpers.Instance[((RequestAction)r.Action.Value).ToString()],
                            SubmittedOn = r.SubmittedOn,
                            CreatedUserRole = r.CreatedByUser.Roles.First().Name.ToString(),
                        }).ToList();
                        model2.Referees = selectedreferees;
                    }
                    break;
            }

            if (model == null) model = new RequestViewModel();

            model.Request = request;
            var data = request.RequestDatas.ToList();
            foreach (var prop in modelType.GetProperties())
            {
                if (prop.PropertyType.FullName.Contains("List")) continue;
                var entry = data.FirstOrDefault(r => r.Name == prop.Name);
                if (entry == null) continue;

                if (prop.PropertyType.FullName == typeof(uint?).FullName)
                {
                    uint tmp = 0;
                    if (uint.TryParse(entry.Value, out tmp))
                    {
                        model.SetPropValue(prop.Name, tmp);
                    }
                }
                else if (prop.PropertyType.FullName == typeof(int?).FullName)
                {
                    int tmp = 0;
                    if (int.TryParse(entry.Value, out tmp))
                    {
                        model.SetPropValue(prop.Name, tmp);
                    }
                }
                else if (prop.PropertyType.FullName == typeof(DateTime?).FullName)
                {
                    DateTime tmp = default(DateTime);
                    if (DateTime.TryParse(entry.Value, out tmp))
                    {
                        model.SetPropValue(prop.Name, tmp);
                    }
                }
                else if (prop.PropertyType.FullName == typeof(PaymentMethod?).FullName)
                {
                    PaymentMethod tmp = PaymentMethod.Unspecified;
                    if (Enum.TryParse(entry.Value, true, out tmp))
                    {
                        model.SetPropValue(prop.Name, tmp);
                    }
                }
                else if (prop.PropertyType.FullName == typeof(VacationType?).FullName)
                {
                    VacationType tmp = VacationType.Unspecified;
                    if (Enum.TryParse(entry.Value, true, out tmp))
                    {
                        model.SetPropValue(prop.Name, tmp);
                    }
                }
                else
                {
                    model.SetPropValue(prop.Name, entry.Value);
                }
            }

            model.Id = request.Id;
            //model.Comments = request.UpdatedBy == CurrentUser.Id ? request.Comments : null;
            model.Name = user.DbUser.UserProfile.GetFullName();
            model.UniqueId = request.UniqueId;
            model.Type = LangHelpers.Instance[(((RequestType)request.Type).ToString())];
            model.Status = LangHelpers.Instance["state_" + (((RequestStatus)request.Status).ToString())];
            if (Utils.HasRolesAny(ApsUserRole.DeanOfLibraryAffairs) && (RequestStatus)request.Status == RequestStatus.DistributionPendingCopyDistribution)
            {
                model.Status = ApsStrings.entitylist_dla;
            }
            model.Date = request.Created.FormatDate();
            model.CurrentlyWithUserId = request.CurrentlyWith;
            model.CreatedByUserId = request.CreatedBy;

            model.CommentThread = request.RequestHistoryLogs.Where(h => h.Action > 0)
                .ToList()
                .OrderByDescending(h => h.Created)
                .Select
                 (
                             h => new CommentViewModel
                             {
                                 RequestType = (RequestType)request.Type,
                                 IsAction = h.Action > 0,
                                 Action = LangHelpers.Instance["history_" + ((RequestAction)h.Action).ToString()],
                                 Comment = h.Comments,
                                 //Username = h.CreatedByUser != null && h.CreatedByUser.UserProfile != null ? h.CreatedByUser.UserProfile.GetFullName() : "",
                                 Username = WebUtils.GetUserNameFromHistory(h.Comments, h.CreatedByUser),
                                 Role = LangHelpers.Instance[WebUtils.GetUserRoleFromHistory(h.Comments, h.CreatedByUser).ToString()],
                                 Date = h.Created,
                                 DateStr = h.Created.FormatDate(true, true)
                             }
                 )
                 .ToList();

            model.CommentThread = model.CommentThread.OrderByDescending(m => m.Date).ThenByDescending(m => m.IsAction ? 0 : 1).ToList();
            ViewBag.CutOffMarks = (new RefereeUtil()).GetRefereeRequestFeedbackRejectCutOffMarks();
            ViewBag.CurrentUserId = CurrentUser.Id;
            model.Files = request.RequestFiles.Where(rf => !((FileType)rf.Type).ToString().EndsWith("Opinion") || rf.Type == (byte)FileType.CoResearcherOpinion).Select(c => new AttachedFile
            {
                FileId = c.Id.ToString(),
                Type = (FileType)c.Type,
                FileName = Path.GetFileName(c.FileName),
                CreatedUserName = WebUtils.GetUserNameForFileUploads(c.Description ?? "", c.CreatedByUser),
                CreatedUserRole = WebUtils.GetUserRoleForFileUploads(c.Description ?? "", c.CreatedByUser)
            }).ToList();

            return View(viewName, model);
        }

        //private ApsUserRole GetUserRoleForFileUpload(string Description, User CreatedByUser)
        //{
        //    var apsUserRole = ApsUserRole.None;
        //    Enum.TryParse(CreatedByUser.Roles.FirstOrDefault()?.Name, out apsUserRole);
        //    if (!string.IsNullOrWhiteSpace(Description) && Description.StartsWith("Co-Researcher"))
        //    {
        //        apsUserRole = ApsUserRole.CoResearcher;
        //    }
        //    return apsUserRole;
        //}

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

        private string GetHistoryUsetName(string Description, string coResearcherName)
        {
            if (Description.StartsWith("Co-Researcher"))
            {
                var names = Description.Split('=');
                var userName = names[1];
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    var userId = 0;
                    int.TryParse(userName, out userId);
                    var coresearcher = DbContext.RequestCoresearchers.Where(x => x.Id == userId).FirstOrDefault();
                    if (coresearcher != null)
                    {
                        coResearcherName = coresearcher.Researcher.GetFullName();
                        return coResearcherName;
                    }
                }
            }

            return coResearcherName;
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult View(string id, PublishRequestViewModel model)
        {
            if (Utils.HasRolesAny(ApsUserRole.Researcher))
            {
                var req = DbContext.Requests.FirstOrDefault(r => r.Id.ToString() == id && r.CreatedBy == CurrentUser.Id);

                if (req != null && (req.Type == (byte)RequestType.PublishRequest || req.Type == (byte)RequestType.RepublishRequest))
                {
                    switch ((RequestStatus)req.Status)
                    {
                        case RequestStatus.PublishRequestNotSubmitted:
                            return RedirectToAction("Create", "Publications", new { requestId = req.Id });
                    }
                }
            }

            var result = View(id, model);
            return result;
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (Utils.HasRolesAny(ApsUserRole.Researcher))
            {
                var req = DbContext.Requests.FirstOrDefault(r => r.Id.ToString() == id && r.CreatedBy == CurrentUser.Id);

                if (req != null && (req.Type == (byte)RequestType.PublishRequest || req.Type == (byte)RequestType.RepublishRequest))
                {
                    switch ((RequestStatus)req.Status)
                    {
                        case RequestStatus.PublishRequestNotSubmitted:
                            return RedirectToAction("Create", "Publications", new { requestId = req.Id });
                    }
                }
            }

            var result = View(id);

            return result;
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(string id, PublishRequestViewModel model)
        {
            //return RedirectToAction("View", "Requests", new { id = model.Id });
            if (model == null || string.IsNullOrWhiteSpace(id)) return RedirectToLastIndexPage("Index", "Home");

            var request = RequestUtils.GetRequests()
                .Include("RequestReferees")
                .Include("RequestReferees.Referee")
                .Where(r => r.Id == model.Id)
                .FirstOrDefault();

            if (request == null)
            {
                TempData["message"] = string.Format(ApsStrings.request_not_found, model.Id.ToString());
                TempData["message_type"] = "danger";
                return RedirectToLastIndexPage("Index", "Home");
            }
            if (Utils.HasRolesAny(ApsUserRole.Proofreader) && (model.Files == null && model.Files.Count > 0))
            {
                TempData["message"] = "الرجاء تحميل الملف";
                TempData["message_type"] = "danger";
                return RedirectToAction("View", new { id = id });
            }
            if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit) && request.Type == (byte)RequestStatus.DistributionPendingDistributionListPreparation && (model.DistributionEntities == null && model.DistributionEntities.Count > 0))
            {
                TempData["message"] = "يرجى تحديد النسخ.";
                TempData["message_type"] = "danger";
                return RedirectToAction("View", new { id = id });
            }
            else if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit) && request.Status == (byte)RequestStatus.DistributionPendingDistributionListPreparation)
            {
                var total = 0;
                var noOfCopies = DbContext.Requests.Where(x => x.PublicationRequestId == request.PublicationRequestId && x.Type == (byte)RequestType.OperationCommand).FirstOrDefault().NoOfCopies;
                foreach (var e in model.DistributionEntities)
                {
                    if (e.IsSelected)
                    {
                        total += (int)e.NoOfCopies;
                    }
                }
                if (noOfCopies < total)
                {
                    TempData["message"] = "يجب أن يكون عدد النسخ المختارة أقل من العدد الفعلي للنسخ المراد طباعتها.";
                    TempData["message_type"] = "danger";
                    return RedirectToAction("View", new { id = id });
                }
                else if (total == 0)
                {
                    TempData["message"] = "الرجاء تحديد عدد النسخ المراد طباعتها.";
                    TempData["message_type"] = "danger";
                    return RedirectToAction("View", new { id = id });
                }
            }
            if (Utils.HasRolesAny(ApsUserRole.Researcher) && model.Action == RequestAction.ResubmitAndForwardToPAECommitee && request.Status == (long)RequestStatus.PublishRequestPendingModificationsAfterReferees)
            {
                TempData["message"] = Resources.ApsStrings.update_request_before_submitting;
                TempData["message_type"] = "danger";

                return RedirectToAction("View", "Requests", new { id = model.Id });
            }

            var workflow = (IRequestWorkflow)null;

            switch ((RequestType)request.Type)
            {
                case RequestType.PublishRequest:
                case RequestType.RepublishRequest:
                    workflow = new PublishRequestWorkflow();
                    break;

                case RequestType.OperationCommand:
                    workflow = new OperationCommandRequestWorkflow();
                    break;

                case RequestType.Pricing:
                    workflow = new PricingRequestWorkflow();
                    break;

                case RequestType.Distribution:
                    workflow = new DistributionRequestWorkflow();
                    break;

                case RequestType.RefereeReward:
                    workflow = new RefereeRewardWorkflow();
                    break;

                case RequestType.ResearcherReward:
                    workflow = new ResearcherRewardWorkflow();
                    break;

                case RequestType.ProofreaderReward:
                    workflow = new ProofReaderRewardWorkflow();
                    break;
            }

            var requestModel = ToListModel(request.Id);
            var allowedActions = workflow.GetAllowedActionsTransitionsWithFiles(requestModel, model.Action.ToString(), model.Grade.ToString());
            var transition = allowedActions.FirstOrDefault(a => a.Item1 == model.Action);
            var skipToSpareReferee = false;

            if (allowedActions.Count == 0)
            {
                return RedirectToAction("View", new { id = id });
            }
            else if (transition == null && !skipToSpareReferee)
            {
                if (model.Action == default(RequestAction))
                {
                    TempData["message"] = "يجب اختيار الإجراء";
                    TempData["message_type"] = "danger";
                }
                else
                {
                    TempData["message"] = "هذا الإجراء غير متاح";
                    TempData["message_type"] = "danger";
                }

                return RedirectToAction("View", new { id = id });
            }

            if (!(Utils.HasRolesAny(ApsUserRole.KingFahadNationalLibraryOfficer) && request.Status == (long)RequestStatus.OperationCommandPendingISBN))
            {
                ModelState.Remove("ISBN");
            }
            if (!skipToSpareReferee)
            {
                if (transition.Item1.ToString().StartsWith("Return") || transition.Item1.ToString().StartsWith("Reject"))
                {
                    if (string.IsNullOrWhiteSpace(model.Comments))
                    {
                        TempData["message"] = "يرجى ذكر السبب";
                        TempData["message_type"] = "danger";
                        return RedirectToAction("View", new { id = id });
                    }
                    model.Comments = model.Comments.Trim();
                }
            }

            var canSelectReferees = allowedActions.Count(a => a.Item1 == RequestAction.ForwardToInitialReferee || a.Item1 == RequestAction.SelectReferees) > 0;

            if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead) && (request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead || request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers))
            {
                canSelectReferees = true;
            }

            model.AllowedActions = workflow.GetAllowedActions(requestModel).Select(a => new RequestActionViewModel
            {
                Action = a,
                Display = LangHelpers.Instance["action_" + a.ToString()],
                Value = a.ToString()
            }).ToList();

            var isMember = Roles.First().ToString().EndsWith("Member");

            if (canSelectReferees && (model.Action == RequestAction.ForwardToInitialReferee || model.Action == RequestAction.SelectReferees))
            {
                if (model.AllowedActions.Count(a => a.Action == RequestAction.ForwardToInitialReferee) > 0)
                {
                    //var referees = UserUtils.GetReferees(null, false, LangHelpers.Instance);
                    model.MinRefereeCount = 1;
                    model.MaxRefereeCount = 1;
                }
                else if (model.AllowedActions.Count(a => a.Action == RequestAction.SelectReferees) > 0)
                {
                    if (isMember)
                    {
                        model.MinRefereeCount = 1;
                    }
                    else
                    {
                        model.MinRefereeCount = 3;
                    }

                    model.MaxRefereeCount = 10;
                }

                if ((model.Referees == null || model.Referees.Count < model.MinRefereeCount || model.Referees.Count > model.MaxRefereeCount))
                {
                    TempData["message"] = string.Format("يجب اختيار محكمين من {0} إلى {1} كحد أقصى", model.MinRefereeCount, model.MaxRefereeCount);
                    TempData["message_type"] = "danger";

                    return RedirectToAction("View", "Requests", new { id = model.Id });
                }
            }

            if (request.Status == (long)RequestStatus.PublishRequestPendingReferees && Utils.HasRolesAny(ApsUserRole.Referee))
            {
                if ((model.Action == RequestAction.Approve || model.Action == RequestAction.ApproveWithComments) && model.Grade < 75)
                {
                    TempData["message"] = "يجب أن تكون الدرجة أكبر من 75٪ للموافقة عليها.";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("View", "Requests", new { id = model.Id });
                }
                if (model.Action == RequestAction.Reject && model.Grade >= 75)
                {
                    TempData["message"] = "يجب أن تكون الدرجةأقل من 75٪ للرفض.";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("View", "Requests", new { id = model.Id });
                }
            }

            ModelState.Clear();

            var db = Utils.db;

            if (request.Type == (byte)RequestType.Pricing && request.Status == (long)RequestStatus.PricingPendingPricingPrepartion)
            {
                if (model.PageCount == null || model.PageCount <= 0)
                {
                    TempData["message"] = "يجب تحديد عدد صفحات المادة العلمية";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("View", "Requests", new { id = model.Id });
                }

                model.Price = (int)Math.Ceiling(model.PageCount.Value / 20.0);

                if (model.Price > 50) model.Price = 50;
            }
            else if (((RequestType)request.Type).ToString().EndsWith("Reward") && request.Status == (long)RequestStatus.RewardPendingApprovalByViceDean)
            {
                if (model.Price == null || model.Price <= 0)
                {
                    TempData["message"] = "يجب تحديد قيمة المكافأة";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("View", "Requests", new { id = model.Id });
                }
            }
            else if (request.Type == (byte)RequestType.OperationCommand && request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandFinalApproval)
            {
                if (model.NoOfCopies == null || model.NoOfCopies <= 0)
                {
                    TempData["message"] = "يجب تحديد عدد النسخ";
                    TempData["message_type"] = "danger";

                    return RedirectToAction("View", "Requests", new { id = model.Id });
                }
            }

            try
            {
                RequestFile submittedFile = null;

                if (Request.Files.Count > 0)
                {
                    var type = FileType.Other;

                    if (Utils.HasRolesAny(ApsUserRole.Referee))
                    {
                        type = FileType.RefereeOpinion;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember))
                    {
                        type = FileType.AcademicCouncilMemberOpinion;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.DeanshipOfAcademicResearchCouncilMember))
                    {
                        type = FileType.DeanshipOfAcademicResearchCouncilMemberOpinion;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitCouncilMember))
                    {
                        type = FileType.ResearchUnitMemberOpinion;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember))
                    {
                        type = FileType.ResearchUnitMemberOpinion;
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.UniversityRector) && model.Action == RequestAction.ApproveApproval)
                    {
                        request.RefereesApproved = true;
                    }

                    for (var i = 0; i < Request.Files.Count; i++)
                    {
                        var File = Request.Files[i];
                        var mf = model.Files[i];
                        if (File != null && File.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(File.FileName);
                            var extension = Path.GetExtension(fileName);
                            if (mf != null)
                            {
                                var oldfile = db.RequestFiles.Where(x => x.Id.ToString() == mf.FileId).FirstOrDefault();
                                if (oldfile != null)
                                {
                                    oldfile.FileName = Path.GetFileName(File.FileName);
                                    oldfile.Extension = extension;
                                    oldfile.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), oldfile.Id.ToString() + extension);
                                    oldfile.Created = TM.Now;
                                    oldfile.RequestId = request.Id;
                                    oldfile.CreatedBy = Utils.CurrentUser.Id;
                                    File.EnhancedSaveAs(oldfile.FilePath);
                                    db.Entry(oldfile).State = EntityState.Modified;
                                }
                                else
                                {
                                    submittedFile = new Data.DbModels.RequestFile
                                    {
                                        Id = Guid.NewGuid(),
                                        Type = (byte)type,
                                        CreatedBy = Utils.CurrentUser.Id,
                                        FileName = Path.GetFileName(File.FileName),
                                        Extension = extension,
                                        RequestId = request.Id,
                                        Created = TM.Now
                                    };

                                    submittedFile.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), submittedFile.Id + extension);
                                    File.EnhancedSaveAs(submittedFile.FilePath);
                                    db.RequestFiles.Add(submittedFile);
                                }
                            }
                        }
                    }
                }

                db.SaveChanges();

                if ((RequestStatus)request.Status == RequestStatus.PublishRequestPendingPAEAfterRefereesHead || (RequestStatus)request.Status == RequestStatus.PublishRequestPendingPAEAfterRefereesMembers)
                {
                    var isMailSent = false;
                    if (transition.Item1.ToString().StartsWith("Approve"))
                    {
                        isMailSent = NotificationOnGradeBasedRefereeAction(request.Id, true);
                    }
                    else if (transition.Item1.ToString().StartsWith("Reject"))
                    {
                        isMailSent = NotificationOnGradeBasedRefereeAction(request.Id, false);
                    }
                }

                if (request.Type == (byte)RequestType.Pricing && request.Status == (long)RequestStatus.PricingPendingPricingPrepartion)
                {
                    //request.AmountSar = model.Price;
                    request.NoOfPages = model.PageCount;
                    var price = Math.Round((double)request.NoOfPages / 20);
                    request.AmountSar = price > 50 ? 50 : price;
                }
                else if (((RequestType)request.Type).ToString().EndsWith("Reward") && request.Status == (long)RequestStatus.RewardPendingApprovalByViceDean)
                {
                    request.RewardRequest.Amount = model.Price;
                    db.Entry(request.RewardRequest).State = EntityState.Modified;
                }
                else if (((RequestType)request.Type) == RequestType.OperationCommand && request.Status == (long)RequestStatus.OperationCommandPendingOperationCommandFinalApproval)
                {
                    request.AmountSar = model.Price;
                }

                var currentStatus = request.Status;

                if (model.Referees == null) model.Referees = new List<RefereeViewModel>();

                if (canSelectReferees && model.Referees.Count > 0)
                {
                    List<RequestReferee> oldReferees = new List<RequestReferee>();
                    if (request.Status == (long)RequestStatus.PublishRequestPendingInitialRefereeForDAR || request.Status == (long)RequestStatus.PublishRequestPendingInitialRefereeForRUH)
                    {
                        oldReferees = db.RequestReferees.Where(rr => rr.RequestId == request.Id && rr.Action == null
                       && rr.IsDeleted == false && rr.CreatedBy == Utils.CurrentUser.Id).ToList();
                    }
                    else
                    {
                        oldReferees = db.RequestReferees.Where(rr => rr.RequestId == request.Id && rr.Action == null
                        && rr.IsDeleted == false && rr.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString()).ToList();
                    }

                    foreach (var rr in oldReferees)
                    {
                        rr.Action = (byte)RequestAction.Canceled;
                        rr.IsDeleted = true;
                        rr.Updated = TM.Now;
                        rr.UpdatedBy = CurrentUser.Id;

                        db.Entry(rr).State = EntityState.Modified;
                    }

                    db.SaveChanges();
                    request.RefereesApproved = false;
                    var reviewIndex = db.RequestReferees
                        .Where(rr => rr.RequestId == request.Id && rr.IsDeleted == false && rr.CreatedBy == Utils.CurrentUser.Id)
                        .Max(p => p.RequestReviewOrderNumber);
                    if (reviewIndex == null)
                    {
                        reviewIndex = 0;
                    }
                    foreach (var referee in model.Referees)
                    {
                        var rid = (int)0;

                        if (!int.TryParse(referee.Id, out rid) || rid <= 0) continue;

                        var strHash = Convert.ToBase64String(SHA256CryptoServiceProvider.Create().ComputeHash(Encoding.ASCII.GetBytes(TM.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + referee.Id))).Replace("+", "").Replace("/", "").Replace("\\", "");
                        if (strHash.Length > 128)
                        {
                            strHash = strHash.Substring(0, 128);
                        }

                        if (strHash.EndsWith("="))
                            strHash = strHash.Substring(0, strHash.Length - 1);

                        var roundNo = -1;

                        if (model.Action == RequestAction.ForwardToInitialReferee)
                        {
                            roundNo = 1;
                        }
                        else if (reviewIndex < 2)
                        {
                            roundNo = 2;
                        }
                        else
                        {
                            roundNo = 3;
                        }
                        var rNo = roundNo;
                        if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember, ApsUserRole.AcademicCouncilMember))
                        {
                            rNo = (-1) * roundNo;
                        }
                        RequestReferee rr;
                        if (request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead && (model.Action == RequestAction.SelectReferees || model.Action == RequestAction.ApproveAndForwardToUnversityRector) && Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                        {
                            rr = request.RequestReferees.FirstOrDefault(x => x.RefereeId == rid && x.IsDeleted == false && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.CommitteForPAEHead.ToString());
                            if (rr != null)
                            {
                                rr.CreatedBy = CurrentUser.Id;
                                rr.Created = TM.Now;
                            }
                        }
                        else
                        {
                            rr = request.RequestReferees.FirstOrDefault(x => x.RefereeId == rid && x.IsDeleted == false && x.CreatedBy == Utils.CurrentUser.Id);
                        }
                        if (rr == null)
                        {
                            rr = new RequestReferee
                            {
                                RefereeId = rid,
                                RequestId = request.Id,
                                RoundNo = rNo,
                                Key = strHash,
                                IsKeyUsed = false,
                                NumberOfVotes = 1,
                                Created = TM.Now,
                                CreatedBy = CurrentUser.Id,
                                Updated = TM.Now,
                                UpdatedBy = CurrentUser.Id,
                                IsDeleted = false,
                                RequestReviewOrderNumber = (reviewIndex + 1)
                            };

                            db.RequestReferees.Add(rr);
                        }
                        else
                        {
                            rr.RequestReviewOrderNumber = (reviewIndex + 1);
                            rr.Updated = TM.Now;
                            rr.UpdatedBy = CurrentUser.Id;
                            rr.Action = null;
                            rr.RequestReviewSentOn = null;
                            rr.SubmittedOn = null;
                            db.Entry(rr).State = EntityState.Modified;
                        }
                        reviewIndex++;
                    }

                    db.SaveChanges();
                }
                if (request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead && model.Action == RequestAction.SelectReferees && Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                {
                    var rr = request.RequestReferees.Where(x => x.IsDeleted == false && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.CommitteForPAEHead.ToString()).ToList();
                    if (rr.Count() > 0)
                    {
                        foreach (var r in rr)
                        {
                            r.IsDeleted = true;
                            db.Entry(r).State = EntityState.Modified;
                        }
                        db.SaveChanges();
                    }
                }
                if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember))
                {
                    var selectedbymember = request.RequestReferees.Where(x => x.CreatedBy == CurrentUser.Id && x.IsDeleted == false).ToList();
                    if (selectedbymember.Count() > 0)
                    {
                        foreach (var referee in selectedbymember)
                        {
                            var IsDeselected = model.Referees.Any(x => x.Id == referee.RefereeId.ToString());
                            if (!IsDeselected)
                            {
                                referee.NumberOfVotes = 0;
                                referee.IsDeleted = true;
                                db.Entry(referee).State = EntityState.Modified;
                            }
                        }
                        db.SaveChanges();
                    }
                }
                if (Utils.HasRolesAny(ApsUserRole.Researcher) && model.Action == RequestAction.ResubmitAndReturnToCoresearchers && (request.Status == (long)RequestStatus.PublishRequestRejectedByCoresearchers || request.Status == (long)RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified))
                {
                    var sent = false;
                    List<string> Listoflinks = new List<string>();
                    foreach (var coresearcher in request.RequestCoresearchers)
                    {
                        var strHash = Convert.ToBase64String(SHA256CryptoServiceProvider.Create().ComputeHash(Encoding.ASCII.GetBytes(DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + coresearcher.Researcher.FirstName))).Replace("+", "").Replace("/", "").Replace("\\", "");
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
                                RequestId = request.Id,
                                UserAgent = HttpContextManager.Current.Request.UserAgent,
                                Status = (long)APS.Data.RequestStatus.Pending,
                                Type = (byte)RequestHistoryTypes.SendLinkToCoResearcher,
                            });
                            db.SaveChanges();
                            Listoflinks.Add(string.Format(Utils.GetApplicationUrl() + "l/" + coresearcher.Key));
                            TempData["coresearhcersmslink"] = Listoflinks;
                        }
                        else
                        {
                            TempData["message"] = "لم يتم إرسال الرسالة النصية على الجوال :" + coresearcher.Researcher.MobileNo + "<br>" + errorMsg;
                            TempData["message_type"] = "danger";
                            //req.Status = (long)RequestStatus.PublishRequestNotSubmitted;
                            //db.Entry(req).State = System.Data.Entity.EntityState.Modified;
                            //db.SaveChanges();
                            return RedirectToAction("View", "Requests", new { id = model.Id });
                        }
                    }
                }
                if ((transition.Item2 == RequestStatus.Completed || transition.Item2 == RequestStatus.Rejected) & Utils.HasRolesAny(ApsUserRole.UniversityRector))
                {
                    var oldReferees = db.RequestReferees.Where(rr => rr.RequestId == request.Id && rr.Action == null
                    && rr.IsDeleted == false && rr.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString()).ToList();
                    foreach (var rr in oldReferees)
                    {
                        rr.Action = (byte)RequestAction.Canceled;
                        rr.IsDeleted = true;
                        rr.Updated = TM.Now;
                        rr.UpdatedBy = CurrentUser.Id;

                        db.Entry(rr).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                var _refreeUtil = new RefereeUtil();
                if (model.Action == APS.Data.RequestAction.ApproveApproval && Utils.HasRolesAny(ApsUserRole.UniversityRector))
                {
                    _refreeUtil.SendNotificationToNextRefereeForFundingRequestReview(model.Id.Value, 0, RefereeType.Normal);
                    var _requestRefrees = Utils.db.RequestReferees.Where(x => x.RequestId == request.Id && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString()).ToList();
                    int totalAccepted = _requestRefrees.Count(rr => rr.Action == (byte)Data.RequestAction.Approve || rr.Action == (byte)Data.RequestAction.ApproveModifications);
                    int totalAcceptedWithComments = _requestRefrees.Count(rr => rr.Action == (byte)Data.RequestAction.ApproveWithComments);
                    int totalRejected = _requestRefrees.Count(rr => rr.Action == (byte)Data.RequestAction.Reject || rr.Action == (byte)Data.RequestAction.RejectModifications);
                    var sendnext = totalAccepted + totalAcceptedWithComments + totalRejected;
                    if (sendnext == 0)
                    {
                        _refreeUtil.SendNotificationToNextRefereeForFundingRequestReview(model.Id.Value, 0, RefereeType.Normal);
                    }
                }
                if (model.Action == APS.Data.RequestAction.ForwardToReferees && Utils.HasRolesAny(ApsUserRole.CommitteeforPAESecretary))
                {
                    var rrs = request.RequestReferees.Where(x => x.IsDeleted == false && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString() && x.Action == (byte)Data.RequestAction.ApproveWithComments).ToList();
                    foreach (var r in rrs)
                    {
                        _refreeUtil.SendNotificationToRefereeForFundingRequestReview(model.Id.Value, r.RefereeId, r);
                    }
                }
                if (model.Action == APS.Data.RequestAction.ForwardToInitialReferee)
                {
                    _refreeUtil.SendNotificationToNextRefereeForFundingRequestReview(model.Id.Value, 0);
                }
                if (Utils.HasRolesAny(ApsUserRole.Referee))
                {
                    RequestReferee rr = null;
                    if (request.Status == (long)RequestStatus.PublishRequestPendingInitialRefereeForDAR)
                    {
                        rr = request.RequestReferees.FirstOrDefault(r => r.Referee.UserId == CurrentUser.Id && r.IsDeleted == false && r.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.DeanOfAcademicResearch.ToString());
                    }
                    else if (request.Status == (long)RequestStatus.PublishRequestPendingInitialRefereeForRUH)
                    {
                        rr = request.RequestReferees.FirstOrDefault(r => r.Referee.UserId == CurrentUser.Id && r.IsDeleted == false && r.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.ResearchUnitHead.ToString());
                    }
                    else if (request.Status == (long)RequestStatus.PublishRequestPendingReferees || request.Status == (long)RequestStatus.PublishRequestPendingSpareReferee || request.Status == (long)RequestStatus.PublishRequestPendingRefereesAfterRefereesAndResearcherModifications)
                    {
                        rr = request.RequestReferees.FirstOrDefault(r => r.Referee.UserId == CurrentUser.Id && r.IsDeleted == false && r.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString());
                    }
                    else
                    {
                        rr = request.RequestReferees.FirstOrDefault(r => r.Referee.UserId == CurrentUser.Id && r.IsDeleted == false);
                    }
                    bool updateRequestStatus = false;
                    bool sendInvitationToNextReferee = false;

                    if (rr != null)
                    {
                        if (rr.ReceivedOn == null)
                        {
                            rr.ReceivedOn = TM.Now;
                        }
                        if (rr.DidRefereeAgree == null)
                        {
                            rr.DidRefereeAgree = model.Action.ToString() == Data.RequestAction.AcceptToReview.ToString() ? true : false;
                            rr.RequestReviewAcceptOn = TM.Now;
                            rr.Action = (byte)model.Action;
                            if (!rr.DidRefereeAgree.Value)
                            {
                                rr.SubmittedOn = TM.Now;
                                rr.Comments = model.Comments;
                                updateRequestStatus = true;

                                // Send Notification to Next Reserve Referee
                                _refreeUtil.SendNotificationToNextRefereeForFundingRequestReview(rr.RequestId, 0, RefereeType.Normal);
                                if (request.Status == (long)RequestStatus.PublishRequestPendingReferees || request.Status == (long)RequestStatus.PublishRequestPendingSpareReferee)
                                {
                                    sendInvitationToNextReferee = true;
                                }
                            }
                            rr.Updated = TM.Now;
                            rr.UpdatedBy = CurrentUser.Id;
                        }
                        else
                        {
                            updateRequestStatus = true;
                            rr.SubmittedOn = TM.Now;
                            rr.Action = (byte)model.Action;
                            rr.Grade = model.Grade;
                            rr.DidRefereeAgree = true;
                            rr.Comments = model.Comments;
                            if (submittedFile != null)
                            {
                                rr.RequestFileId = submittedFile.Id;
                            }

                            if (transition.Item2 != RequestStatus.PublishRequestPendingPAEAfterReferees && transition.Item2 != RequestStatus.PublishRequestPendingInitialRefereeForDAR && transition.Item2 != RequestStatus.PublishRequestPendingInitialRefereeForRUH)
                            {
                                sendInvitationToNextReferee = true;
                            }
                        }

                        db.Entry(rr).State = EntityState.Modified;
                        db.SaveChanges();
                        if (model.Action == RequestAction.ApproveModifications || model.Action == RequestAction.RejectModifications)
                        {
                            var sendnext = _refreeUtil.ReturnToCommitteForPAEHeadToSelectMoreRefereeIfRequired(request.Id);
                            if (!sendnext)
                            {
                                sendInvitationToNextReferee = true;
                            }
                        }
                        if (updateRequestStatus)
                        {
                            request.Status = (long)transition.Item2;
                        }

                        if (sendInvitationToNextReferee)
                        {
                            int pendingToRespond = 1;
                            if (request.Status == (long)RequestStatus.PublishRequestPendingReferees || request.Status == (long)RequestStatus.PublishRequestPendingSpareReferee || request.Status == (long)RequestStatus.PublishRequestPendingPAEAfterReferees)
                            {
                                pendingToRespond = Utils.db.RequestReferees.Where(p => p.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString() && p.RequestId == rr.RequestId && p.RequestReviewSentOn != null && p.SubmittedOn == null && p.IsDeleted == false).Count();
                                if (pendingToRespond == 0)
                                {
                                    var _requestRefrees = Utils.db.RequestReferees.Where(x => x.RequestId == rr.RequestId && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString() && x.IsDeleted == false).ToList();
                                    int totalAccepted = _requestRefrees.Count(r => r.Action == (byte)Data.RequestAction.Approve || r.Action == (byte)Data.RequestAction.ApproveModifications);
                                    int totalAcceptedWithComments = _requestRefrees.Count(r => r.Action == (byte)Data.RequestAction.ApproveWithComments);
                                    int totalRejected = _requestRefrees.Count(r => r.Action == (byte)Data.RequestAction.Reject || r.Action == (byte)Data.RequestAction.RejectModifications);
                                    if (totalAccepted + totalAcceptedWithComments <= 1 && totalRejected <= 1)
                                        _refreeUtil.SendNotificationToNextRefereeForFundingRequestReview(rr.RequestId, 0, RefereeType.Normal);
                                }
                            }
                            else
                            {
                                pendingToRespond = Utils.db.RequestReferees.Where(p => p.RequestId == rr.RequestId && p.RequestReviewSentOn != null && p.SubmittedOn == null && p.IsDeleted == false).Count();
                                if (pendingToRespond == 0)
                                {
                                    _refreeUtil.SendNotificationToNextRefereeForFundingRequestReview(rr.RequestId, 0);
                                }
                            }
                        }
                    }
                }
                else
                {
                    request.Status = (long)transition.Item2;
                }

                request.LastStatusChanged = TM.Now;
                request.LastStatusChangedBy = CurrentUser.Id;
                if (!skipToSpareReferee)
                {
                    request.CurrentlyWith = transition.Item3 == null || transition.Item3.Length == 0 ? null : transition.Item3[0];
                }

                request.Updated = TM.Now;
                request.UpdatedBy = CurrentUser.Id;

                db.SaveChanges();
                if (!skipToSpareReferee)
                {
                    if (transition.Item1 == RequestAction.SendToCommiteeMembers)
                    {
                        switch (Roles.First())
                        {
                            case ApsUserRole.CommitteForPAEHead:
                                {
                                    var members = DbContext.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.CommitteeforPAEMember.ToString()) > 0).Select(u => u.Id).ToList();
                                    foreach (var u in members)
                                    {
                                        var ruser = new RequestUser
                                        {
                                            RequestId = request.Id,
                                            UserId = u,
                                            AssignedOn = TM.Now,
                                            AssignedBy = CurrentUser.Id,
                                            Action = null,
                                        };

                                        DbContext.RequestUsers.Add(ruser);
                                    }

                                    DbContext.SaveChanges();
                                    break;
                                }
                            case ApsUserRole.DeanOfAcademicResearch:
                                {
                                    var members = DbContext.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.DeanshipOfAcademicResearchCouncilMember.ToString()) > 0).Select(u => u.Id).ToList();
                                    foreach (var u in members)
                                    {
                                        var ruser = new RequestUser
                                        {
                                            RequestId = request.Id,
                                            UserId = u,
                                            AssignedOn = TM.Now,
                                            AssignedBy = CurrentUser.Id,
                                            Action = null,
                                        };

                                        DbContext.RequestUsers.Add(ruser);
                                    }

                                    DbContext.SaveChanges();
                                    break;
                                }
                            case ApsUserRole.ResearchUnitHead:
                                {
                                    var members = DbContext.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.ResearchUnitCouncilMember.ToString()) > 0 && u.CollegeId == CurrentUser.CollegeId).Select(u => u.Id).ToList();
                                    foreach (var u in members)
                                    {
                                        var ruser = new RequestUser
                                        {
                                            RequestId = request.Id,
                                            UserId = u,
                                            AssignedOn = TM.Now,
                                            AssignedBy = CurrentUser.Id,
                                            Action = null,
                                        };

                                        DbContext.RequestUsers.Add(ruser);
                                    }

                                    DbContext.SaveChanges();
                                    break;
                                }
                        }
                    }
                }

                var pendingUAction = request.RequestUsers
                    .Where(ru => ru.Action == null && ru.UserId == CurrentUser.Id)
                    .OrderByDescending(a => a.AssignedOn)
                    .FirstOrDefault();

                if (pendingUAction != null)
                {
                    pendingUAction.Comments = model.Comments;
                    pendingUAction.Action = (byte)transition.Item1;
                    pendingUAction.SubmittedOn = TM.Now;

                    if (submittedFile != null)
                    {
                        pendingUAction.FileId = submittedFile.Id;
                        pendingUAction.FileName = submittedFile.FileName;
                    }
                }
                else if (isMember)
                {
                    if (request.RequestUsers.Count(ru => ru.UserId == CurrentUser.Id) == 0)
                    {
                        var reqUser = new RequestUser
                        {
                            RequestId = request.Id,
                            UserId = CurrentUser.Id,
                            AssignedOn = TM.Now,
                            AssignedBy = CurrentUser.Id,
                            //Action = skip ? null : (byte)transition.Item1,
                            SubmittedOn = TM.Now,
                            FileId = submittedFile.Id,
                            FileName = submittedFile.FileName,
                            ReceivedOn = TM.Now,
                            Comments = model.Comments
                        };
                        if (!skipToSpareReferee)
                        {
                            reqUser.Action = (byte)transition.Item1;
                        }
                        DbContext.RequestUsers.Add(reqUser);
                        DbContext.SaveChanges();
                    }
                    if (request.Status == (long)RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee && Utils.HasRolesAny(ApsUserRole.DeanshipOfAcademicResearchCouncilMember))
                    {
                        if (request.RequestUsers.Count(ru => ru.UserId == CurrentUser.Id) > 0)
                        {
                            var reqUser = request.RequestUsers.Where(ru => ru.UserId == CurrentUser.Id).FirstOrDefault();
                            reqUser.Action = (byte)transition.Item1;
                            if (submittedFile != null)
                            {
                                reqUser.FileId = submittedFile.Id;
                                reqUser.FileName = submittedFile.FileName;
                            }
                            else
                            {
                                reqUser.FileId = null;
                                reqUser.FileName = "";
                            }
                            reqUser.SubmittedOn = TM.Now;
                            reqUser.ReceivedOn = TM.Now;
                            reqUser.Comments = model.Comments;

                            DbContext.Entry(reqUser).State = System.Data.Entity.EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                    else if (request.Status == (long)RequestStatus.PublishRequestPendingResearchUnitCommittee && Utils.HasRolesAny(ApsUserRole.ResearchUnitCouncilMember))
                    {
                        if (request.RequestUsers.Count(ru => ru.UserId == CurrentUser.Id) > 0)
                        {
                            var reqUser = request.RequestUsers.Where(ru => ru.UserId == CurrentUser.Id).FirstOrDefault();
                            reqUser.Action = (byte)transition.Item1;
                            if (submittedFile != null)
                            {
                                reqUser.FileId = submittedFile.Id;
                                reqUser.FileName = submittedFile.FileName;
                            }
                            else
                            {
                                reqUser.FileId = null;
                                reqUser.FileName = "";
                            }
                            reqUser.SubmittedOn = TM.Now;
                            reqUser.ReceivedOn = TM.Now;
                            reqUser.Comments = model.Comments;

                            DbContext.Entry(reqUser).State = System.Data.Entity.EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember)
                        && (request.Status == (long)RequestStatus.PublishRequestPendingPAERefereeSelectionMembers ||
                        request.Status == (long)RequestStatus.PublishRequestPendingPAEAfterRefereesMembers))
                    {
                        if (request.RequestUsers.Count(ru => ru.UserId == CurrentUser.Id) > 0)
                        {
                            var reqUser = request.RequestUsers.Where(ru => ru.UserId == CurrentUser.Id).FirstOrDefault();
                            reqUser.Action = (byte)transition.Item1;
                            if (submittedFile != null)
                            {
                                reqUser.FileId = submittedFile.Id;
                                reqUser.FileName = submittedFile.FileName;
                            }
                            else
                            {
                                reqUser.FileId = null;
                                reqUser.FileName = "";
                            }
                            reqUser.SubmittedOn = TM.Now;
                            reqUser.ReceivedOn = TM.Now;
                            reqUser.Comments = model.Comments;

                            DbContext.Entry(reqUser).State = System.Data.Entity.EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember)
                        && (request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers ||
                        request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers ||
                        request.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers
                        ))
                    {
                        if (request.RequestUsers.Count(ru => ru.UserId == CurrentUser.Id) > 0)
                        {
                            var reqUser = request.RequestUsers.Where(ru => ru.UserId == CurrentUser.Id).FirstOrDefault();
                            reqUser.Action = (byte)transition.Item1;
                            if (submittedFile != null)
                            {
                                reqUser.FileId = submittedFile.Id;
                                reqUser.FileName = submittedFile.FileName;
                            }
                            else
                            {
                                reqUser.FileId = null;
                                reqUser.FileName = "";
                            }
                            reqUser.SubmittedOn = TM.Now;
                            reqUser.ReceivedOn = TM.Now;
                            reqUser.Comments = model.Comments;

                            DbContext.Entry(reqUser).State = System.Data.Entity.EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                }

                if (request.Type == (byte)RequestType.PublishRequest || request.Type == (byte)RequestType.RepublishRequest)
                {
                    if (Utils.HasRolesAny(ApsUserRole.Referee, ApsUserRole.Proofreader))
                    {
                        var rr1 = request.RequestReferees.FirstOrDefault(r => r.Referee.UserId == CurrentUser.Id);
                        if (rr1.Action != (byte)RequestAction.AcceptToReview && rr1.Action != (byte)RequestAction.RejectToReview && rr1.Action != (byte)RequestAction.ApproveWithComments)
                        {
                            var refReward = new Request
                            {
                                PublicationRequestId = request.Id,
                                CollegeId = request.CollegeId,
                                Title = request.Title,
                                PublicationType = request.PublicationType,
                                Type = (byte)(Utils.HasRolesAny(ApsUserRole.Referee) ? RequestType.RefereeReward : RequestType.ProofreaderReward),
                                Status = (long)RequestStatus.RewardPendingApprovalByViceDean,
                                LastStatusChanged = TM.Now,
                                LastStatusChangedBy = CurrentUser.Id,
                                Created = TM.Now,
                                CreatedBy = CurrentUser.Id,
                                Updated = TM.Now,
                                UpdatedBy = CurrentUser.Id,
                            };

                            db.Requests.Add(refReward);
                            db.SaveChanges();

                            var refRewardReq = new RewardRequest
                            {
                                RequestId = refReward.Id,
                                UserId = CurrentUser.Id,
                                Created = TM.Now,
                                CreatedBy = CurrentUser.Id,
                                Updated = TM.Now,
                                UpdatedBy = CurrentUser.Id,
                            };

                            db.RewardRequests.Add(refRewardReq);
                            db.SaveChanges();
                        }
                    }

                    if (request.Status == (long)RequestStatus.Completed)
                    {
                        var opCommandRequest = new Request
                        {
                            PublicationRequestId = request.Id,
                            CollegeId = request.CollegeId,
                            Title = request.Title,
                            PublicationType = request.PublicationType,
                            Type = (byte)RequestType.OperationCommand,
                            Status = (long)RequestStatus.OperationCommandPendingOperationCommandInitiation,
                            LastStatusChanged = TM.Now,
                            LastStatusChangedBy = CurrentUser.Id,
                            Created = TM.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = TM.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        db.Requests.Add(opCommandRequest);
                        db.SaveChanges();
                    }
                }
                else if (request.Type == (byte)RequestType.OperationCommand)
                {
                    //STATUS == OperationCommandPendingOperationCommandInitiationAND ACTION = ForwardToProofreader
                    if (request.Status == (byte)RequestStatus.OperationCommandPendingPendingCorrection && model.Action == RequestAction.ForwardToProofreader)
                    {
                        var proofReaderId = DbContext.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.Proofreader.ToString()) > 0).FirstOrDefault().Id;

                        var reqUser = new RequestUser
                        {
                            RequestId = request.Id,
                            UserId = proofReaderId,
                            AssignedOn = TM.Now,
                            AssignedBy = CurrentUser.Id,
                            Action = null,
                            SubmittedOn = null,
                            ReceivedOn = TM.Now,
                        };

                        DbContext.RequestUsers.Add(reqUser);
                        DbContext.SaveChanges();
                    }

                    if (request.Status == (long)RequestStatus.Completed)
                    {
                        var pricingRequest = new Request
                        {
                            PublicationRequestId = request.PublicationRequestId,
                            CollegeId = request.CollegeId,
                            Title = request.Title,
                            PublicationType = request.PublicationType,
                            Type = (byte)RequestType.Pricing,
                            Status = (long)RequestStatus.PricingPendingPricingPrepartion,
                            LastStatusChanged = TM.Now,
                            LastStatusChangedBy = CurrentUser.Id,
                            Created = TM.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = TM.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        db.Requests.Add(pricingRequest);
                        db.SaveChanges();

                        var refReward = new Request
                        {
                            PublicationRequestId = request.Id,
                            CollegeId = request.CollegeId,
                            Title = request.Title,
                            PublicationType = request.PublicationType,
                            Type = (byte)RequestType.ResearcherReward,
                            Status = (long)RequestStatus.RewardPendingApprovalByViceDean,
                            LastStatusChanged = TM.Now,
                            LastStatusChangedBy = CurrentUser.Id,
                            Created = TM.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = TM.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        db.Requests.Add(refReward);
                        db.SaveChanges();

                        var refRewardReq = new RewardRequest
                        {
                            RequestId = refReward.Id,
                            UserId = request.PublicationRequest.CreatedBy,
                            Created = TM.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = TM.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        db.RewardRequests.Add(refRewardReq);
                        db.SaveChanges();
                    }
                    if (Utils.HasRolesAny(ApsUserRole.KingFahadNationalLibraryOfficer) && request.Status == (long)RequestStatus.OperationCommandAfterISBN)
                    {
                        request.Isbn = model.ISBN;
                    }
                    //if (model.Files != null)
                    //{
                    //    for (var i = 0; i < model.Files.Count; i++)
                    //    {
                    //        var File = Request.Files[i];
                    //        if (File != null && File.ContentLength > 0)
                    //        {
                    //            var fileName = Path.GetFileName(File.FileName);
                    //            var extension = Path.GetExtension(fileName);

                    //            var f = new RequestFile
                    //            {
                    //                Id = Guid.NewGuid(),
                    //                PageCount = 0,
                    //                Type = (byte)FileType.CorrectedPublication,
                    //                CreatedBy = Utils.CurrentUser.Id,
                    //                FileName = fileName,
                    //                Extension = extension,
                    //                RequestId = request.Id,
                    //                Created = TM.Now
                    //            };

                    //            f.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), f.Id + extension);
                    //            File.EnhancedSaveAs(f.FilePath);
                    //            db.RequestFiles.Add(f);
                    //        }
                    //    }
                    //}
                    if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch) && request.Status == (long)RequestStatus.OperationCommandPendingPrintingManager)
                    {
                        request.NoOfCopies = model.NoOfCopies;
                    }
                }
                else if (request.Type == (byte)RequestType.Pricing)
                {
                    if (request.Status == (long)RequestStatus.Completed)
                    {
                        var distributionRequest = new Request
                        {
                            PublicationRequestId = request.PublicationRequestId,
                            CollegeId = request.CollegeId,
                            Title = request.Title,
                            PublicationType = request.PublicationType,
                            Type = (byte)RequestType.Distribution,
                            Status = (long)RequestStatus.DistributionPendingDistributionListPreparation,
                            LastStatusChanged = TM.Now,
                            LastStatusChangedBy = CurrentUser.Id,
                            Created = TM.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = TM.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        db.Requests.Add(distributionRequest);
                        db.SaveChanges();
                    }
                }
                else if (request.Type == (byte)RequestType.Distribution)
                {
                    if (request.Status == (long)RequestStatus.DistributionPendingCopyDistribution && model.Action == RequestAction.ApproveApproval)
                    {
                        var dprId = DbContext.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.PublicRelationsDirector.ToString()) > 0).FirstOrDefault().Id;

                        var reqUser = new RequestUser
                        {
                            RequestId = request.Id,
                            UserId = dprId,
                            AssignedOn = TM.Now,
                            AssignedBy = CurrentUser.Id,
                            Action = null,
                            SubmittedOn = null,
                            ReceivedOn = TM.Now,
                        };

                        DbContext.RequestUsers.Add(reqUser);
                        DbContext.SaveChanges();
                    }
                    if (request.Status == (long)RequestStatus.DistributionPendingDistributionListApprovalViceDean)
                    {
                        var distributionEntities = DbContext.RequestDistributionListAgencies.Where(x => x.RequestId == request.Id);
                        if (distributionEntities == null || distributionEntities.Count() == 0)
                        {
                            foreach (var e in model.DistributionEntities)
                            {
                                if (e.IsSelected)
                                {
                                    var reqEntity = new RequestDistributionListAgency
                                    {
                                        RequestId = request.Id,
                                        AgencyId = e.Id,
                                        Name = e.Name,
                                        NoOfCopies = (int)e.NoOfCopies,
                                        Created = TM.Now,
                                        CreatedBy = CurrentUser.Id,
                                        Updated = TM.Now,
                                        UpdatedBy = CurrentUser.Id
                                    };
                                    DbContext.RequestDistributionListAgencies.Add(reqEntity);
                                }
                            }
                        }
                        else
                        {
                            foreach (var e in model.DistributionEntities)
                            {
                                var de = distributionEntities.Where(x => x.Id == e.Id).FirstOrDefault();
                                if (de.Id != 0)
                                {
                                    de.NoOfCopies = (int)e.NoOfCopies;
                                    de.Name = e.Name;
                                    de.Updated = TM.Now;
                                    de.UpdatedBy = CurrentUser.Id;
                                    if (e.IsSelected)
                                    {
                                        DbContext.Entry(de).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        DbContext.Entry(de).State = System.Data.Entity.EntityState.Deleted;
                                    }
                                }
                            }
                        }

                        DbContext.SaveChanges();
                    }
                    else if (request.Status == (long)RequestStatus.DistributionPendingDistributionListApproval || (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch) && request.Status == (long)RequestStatus.DistributionPendingCopyDistribution))
                    {
                        var distributionEntities = DbContext.RequestDistributionListAgencies.Where(x => x.RequestId == request.Id);
                        foreach (var e in model.DistributionEntities)
                        {
                            var de = distributionEntities.Where(x => x.Id == e.Id).FirstOrDefault();
                            if (de.Id != 0)
                            {
                                de.NoOfCopies = (int)e.NoOfCopies;
                                de.Name = e.Name;
                                de.Updated = TM.Now;
                                de.UpdatedBy = CurrentUser.Id;
                                if (e.IsSelected)
                                {
                                    DbContext.Entry(de).State = EntityState.Modified;
                                }
                                else
                                {
                                    DbContext.Entry(de).State = System.Data.Entity.EntityState.Deleted;
                                }
                            }
                        }
                        DbContext.SaveChanges();
                    }
                }
                if ((transition.Item2 == RequestStatus.PublishRequestPendingUniversityRectorPublishApproval || transition.Item2 == RequestStatus.PublishRequestPendingUniversityRectorPublishRejected) && Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                {
                    var oldReferees = db.RequestReferees.Where(rr => rr.RequestId == request.Id && rr.Action == null
                   && rr.IsDeleted == false && rr.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.DeanOfAcademicResearch.ToString()).ToList();
                    foreach (var rr in oldReferees)
                    {
                        rr.Action = (byte)RequestAction.Canceled;
                        rr.Updated = TM.Now;
                        rr.UpdatedBy = CurrentUser.Id;
                        db.Entry(rr).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                if (transition.Item2 == RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval && Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
                {
                    var oldReferees = db.RequestReferees.Where(rr => rr.RequestId == request.Id && rr.Action == null
                   && rr.IsDeleted == false && rr.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.ResearchUnitHead.ToString()).ToList();
                    foreach (var rr in oldReferees)
                    {
                        rr.Action = (byte)RequestAction.Canceled;
                        rr.Updated = TM.Now;
                        rr.UpdatedBy = CurrentUser.Id;
                        db.Entry(rr).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }

                var history = new RequestHistoryLog
                {
                    RequestId = request.Id,
                    Status = currentStatus,
                    CurrentlyWith = CurrentUser.Id,
                    Comments = model.Comments,
                    IpAddress = Utils.GetUserIP(),
                    UserAgent = Request.UserAgent,
                    Url = Utils.GetFullUrl(),
                    Action = (byte)model.Action,
                    Created = TM.Now,
                    CreatedBy = CurrentUser.Id,
                };

                db.RequestHistoryLogs.Add(history);
                db.SaveChanges();

                request.LastActionId = history.Id;
                request.Comments = model.Comments;
                db.Entry(request).State = EntityState.Modified;

                db.SaveChanges();

                if (Utils.HasRolesAny(ApsUserRole.Referee) && request.Status != (long)RequestStatus.PublishRequestPendingInitialRefereeForDAR && request.Status != (long)RequestStatus.PublishRequestPendingInitialRefereeForRUH)
                {
                    _refreeUtil.ReturnToCommitteForPAEHeadToSelectMoreRefereeIfRequired(request.Id);
                }

                var isSent = false;
                switch (request.Type)
                {
                    case (byte)RequestType.PublishRequest:
                    case (byte)RequestType.RepublishRequest:
                        if (model.Action == RequestAction.ApproveApproval && request.Status == (long)RequestStatus.Completed)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, true);
                        }
                        else if (model.Action == RequestAction.ApproveRejection && request.Status == (long)RequestStatus.Rejected)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, false);
                        }
                        break;

                    case (byte)RequestType.Distribution:
                        if (model.Action == RequestAction.CloseRequest && request.Status == (long)RequestStatus.Completed)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, true);
                        }
                        break;

                    case (byte)RequestType.OperationCommand:
                        if (model.Action == RequestAction.PrintingCompleted && request.Status == (long)RequestStatus.Completed)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, true);
                        }
                        else if (model.Action == RequestAction.CloseRequest && request.Status == (long)RequestStatus.Rejected)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, false);
                        }
                        break;

                    case (byte)RequestType.Pricing:
                        if (model.Action == RequestAction.Approve && request.Status == (long)RequestStatus.Completed)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, true);
                        }
                        break;

                    case (byte)RequestType.ProofreaderReward:
                        if (model.Action == RequestAction.PaymentCompleted && request.Status == (long)RequestStatus.Completed)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, true);
                        }
                        break;

                    case (byte)RequestType.RefereeReward:
                        if (model.Action == RequestAction.PaymentCompleted && request.Status == (long)RequestStatus.Completed)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, true);
                        }
                        break;

                    case (byte)RequestType.ResearcherReward:
                        if (model.Action == RequestAction.PaymentCompleted && request.Status == (long)RequestStatus.Completed)
                        {
                            isSent = NotificationOnFinalProcedureAction(request.Id, true);
                        }
                        break;
                }

                TempData["message"] = string.Format(ApsStrings.request_updated_successfully, model.Id.ToString());
                TempData["message_type"] = "success";

                return RedirectToAction("View", "Requests", new { id = model.Id });
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

            return RedirectToAction("View", "Requests", new { id = model.Id });
        }

        public ActionResult ViewPublishRequest(long id)
        {
            return Redirect(Utils.GetApplicationUrl() + "View/" + id);
        }

        public ActionResult ViewGeneric(long id)
        {
            return Redirect(Utils.GetApplicationUrl() + "View/" + id);
        }

        private string GenerateUniqueRequestId()
        {
            // format 201608220112
            var date = TM.Now;
            var s1 = DbContext.Requests.Where(c => c.Created.Year == date.Year && c.Created.Month == date.Month && c.Created.Day == date.Day).OrderByDescending(c => c.Id).FirstOrDefault();

            var id1 = (long)0;
            if (s1 != null && s1.UniqueId.Length == 12 && long.TryParse(s1.UniqueId.Substring(8), out id1))
            {
                return TM.Now.ToString("yyyyMMdd") + (id1 + 1).ToString().PadLeft(4, '0');
            }

            return date.ToString("yyyyMMdd", new CultureInfo("en-US")) + (1 + "").PadLeft(4, '0');
        }

        [HttpGet]
        public ActionResult RefereesAgreement(string id)
        {
            var model = new RefereeAgreementViewModel();
            model.Id = id;
            model.TemplateId = ((byte)TemplateKey.RefereesAgreement).ToString();
            model.AgreementText = TemplateUtils[TemplateKey.RefereesAgreement].BodyArabic;
            model.ConditionsFile = TemplateUtils[TemplateKey.RefereesAgreement].FileName;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult RefereesAgreement(RefereeAgreementViewModel model)
        {
            var db = Utils.db;
            var request = db.RequestReferees.Where(x => x.RequestId.ToString() == model.Id && x.Referee.UserId == CurrentUser.Id && x.IsDeleted == false).FirstOrDefault();
            request.DidRefereeAgree = (model.IsRefereeAgree == "approved") ? true : false;
            db.Entry(request).State = EntityState.Modified;
            db.SaveChanges();
            if (model.IsRefereeAgree == "approved")
            {
                return RedirectToAction("View", "Requests", new { id = model.Id });
            }
            else if (model.IsRefereeAgree == "Reject")
            {
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}