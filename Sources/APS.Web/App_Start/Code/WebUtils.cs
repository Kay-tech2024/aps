using APS.Core;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.Models;
using APS.Web.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace APS.Web.App_Start.Code
{
    public static class WebUtils
    {
        public static RequestSummaryViewModel2 GetRequestSummary(long requestId)
        {
            var db = Utils.db;

            var req = Utils.db.Requests.Find(requestId);

            return GetRequestSummary(req);
        }

        public static RequestSummaryTabbedViewModel GetTabbedRequestSummary(long requestId)
        {
            var RequestSummary = new RequestSummaryTabbedViewModel();

            RequestSummaryTabViewModel _summaryData = new RequestSummaryTabViewModel();
            _summaryData.TabId = "0";
            _summaryData.TabName = LangHelpers.Instance["current_version"];
            _summaryData.RequestId = requestId;
            RequestSummary.tabs.Add(_summaryData);

            var revisions = Utils.db.Requests.Where(x => x.RequestRevisionId == requestId && x.IsDeleted == false).OrderByDescending(x => x.Created).ToList();
            int tabId = revisions.Count();
            foreach (var revision in revisions)
            {
                _summaryData = new RequestSummaryTabViewModel();
                _summaryData.TabId = tabId.ToString();
                _summaryData.TabName = LangHelpers.Instance["revision"] + " " + tabId.ToString();
                _summaryData.RequestId = revision.Id;
                RequestSummary.tabs.Add(_summaryData);
                tabId--;
            }

            return RequestSummary;
        }

        public static RequestSummaryViewModel2 GetRequestSummary(Request req)
        {
            if (req == null) return null;

            var model = new RequestSummaryViewModel2();

            // researcher info
            model.UserProfilesNationalId = req.CreatedByUser.UserProfile.NationalId;
            model.ResearchersFirstName = req.CreatedByUser.UserProfile.GetFullName();
            model.UserProfilesGender = req.CreatedByUser.UserProfile.Gender == true ? ApsStrings.male : (req.CreatedByUser.UserProfile.Gender == false ? ApsStrings.female : "");
            model.ResearchersDateOfBirth = req.CreatedByUser.UserProfile.BirthDate == null ? "" : req.CreatedByUser.UserProfile.BirthDate.Value.FormatDate(false, true, true);
            model.ResearchersEmail = req.CreatedByUser.Email;
            model.ResearchersMobileNo = req.CreatedByUser.PhoneNumber;

            // bank information
            model.UserProfilesBankName = !(req.CreatedByUser.UserProfile.BankId > 0) ? req.CreatedByUser.UserProfile.BankName : req.CreatedByUser.UserProfile.Bank.NameArabic;
            model.UserProfilesBankBranch = req.CreatedByUser.UserProfile.BankBranch;
            model.UserProfilesBankAccountHolder = req.CreatedByUser.UserProfile.BankAccountHolder;
            model.UserProfilesBankIban = req.CreatedByUser.UserProfile.BankIban;
            model.UserProfilesBankAccountNumberTypeId = (int)req.CreatedByUser.UserProfile.BankAccountNumberTypeId;
            model.UserProfilesBankSwift = req.CreatedByUser.UserProfile.SwiftCode;
            model.UserProfilesBankRouting = req.CreatedByUser.UserProfile.RoutingNumber;
            model.UserProfilesBankAccountType = LangHelpers.Instance[((BankAccountType)req.CreatedByUser.UserProfile.BankAccountTypeId).ToString()];
            model.UserProfilesBankAccountNumberType = LangHelpers.Instance[((BankAccountNumberType)req.CreatedByUser.UserProfile.BankAccountNumberTypeId).ToString()];

            // Request information
            model.RequestsId = req.Id.ToString();
            model.RequestsTitle = req.Title;
            model.RequestsCollegeId = !(req.CollegeId > 0) ? "" : req.College.NameArabic;
            model.RequestDepartmentId = !(req.DepartmentId > 0) ? "" : req.Department.NameArabic;
            model.RequestsType = LangHelpers.Instance[((RequestType)req.Type).ToString()];
            model.RequestsPublicationType = LangHelpers.Instance["type_" + ((PublicationType)req.PublicationType).ToString()];
            model.BookRequestsBookTitle = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) ? "" : req.BookRequest.BookTitle;
            model.BookRequestsOriginalTitleBook = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) ? "" : req.BookRequest.OriginalTitleBook;
            model.BookRequestsBookType = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) || !(req.BookRequest.BookType > 0) ? "" : LangHelpers.Instance[((BookType)req.BookRequest.BookType).ToString()];
            model.BookRequestsIsArabic = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) ? "" : req.BookRequest.IsArabic ? ApsStrings.yes : ApsStrings.no;
            model.BookRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) ? "" : req.BookRequest.IsTranslated ? ApsStrings.yes : ApsStrings.no;
            model.BookRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) ? "" : req.BookRequest.TranslatedTo;
            model.BookRequestsMajorId = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) || req.BookRequest.Major == null ? "" : req.BookRequest.Major.NameArabic;
            model.BookRequestsNoOfRecommendations = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) || !(req.BookRequest.NoOfRecommendations > 0) ? "" : req.BookRequest.NoOfRecommendations.Value + "";
            model.BookRequestsRecommendationDate = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) || req.BookRequest.RecommendationDate == null ? "" : req.BookRequest.RecommendationDate.Value.FormatDate(ArabicFormat: true, includeSuffix: true);
            model.BookRequestsPublishType = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) || req.BookRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.BookRequest.PublishType.Value).ToString()];
            model.BookRequestsPublisher = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) ? "" : req.BookRequest.Publisher;
            model.BookRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) ? "" : req.BookRequest.PublisherPlace;
            model.BookRequestsPublishingDate = req.PublicationType != (byte)PublicationType.Book || req.BookRequest == null || !(req.BookRequest.RequestId > 0) || req.BookRequest.PublishingDate == null ? "" : req.BookRequest.PublishingDate.Value.FormatDate(ArabicFormat: true, includeSuffix: true);
            model.ReviewRequestsReviewType = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) || !(req.ReviewRequest.ReviewType > 0) ? "" : LangHelpers.Instance[((ProbeType)req.ReviewRequest.ReviewType).ToString()];
            model.ReviewRequestsAuthersName = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) ? "" : req.ReviewRequest.AuthersName;
            model.ReviewRequestsBookTitle = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) ? "" : req.ReviewRequest.BookTitle;
            model.ReviewRequestsScopOfManuscript = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) ? "" : req.ReviewRequest.ScopOfManuscript.ToString();
            model.ReviewRequestsReviewMajor = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) || !(req.ReviewRequest.ReviewMajor > 0) ? "" : req.ReviewRequest.Major.NameArabic;
            model.ReviewRequestsPublishType = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) || req.ReviewRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.ReviewRequest.PublishType.Value).ToString()];
            model.ReviewRequestsPublisher = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) ? "" : req.ReviewRequest.Publisher;
            model.ReviewRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) ? "" : req.ReviewRequest.PublisherPlace;
            model.ReviewRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Probe || req.ReviewRequest == null || !(req.ReviewRequest.RequestId > 0) || req.ReviewRequest.PublisherDate == null ? "" : req.ReviewRequest.PublisherDate.Value.FormatDate(false, true, true);
            model.DictionaryRequestsDictionaryMajor = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) || !(req.DictionaryRequest.DictionaryMajor > 0) ? "" : req.DictionaryRequest.Major_DictionaryMajor.NameArabic;
            model.DictionaryRequestsIsArabic = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) ? "" : req.DictionaryRequest.IsArabic == true ? ApsStrings.yes : ApsStrings.no;
            model.DictionaryRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) ? "" : req.DictionaryRequest.IsTranslated == true ? ApsStrings.yes : ApsStrings.no;
            model.DictionaryRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) ? "" : req.DictionaryRequest.TranslatedTo;
            model.DictionaryRequestsDictionaryTitle = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) ? "" : req.DictionaryRequest.DictionaryTitle;
            model.DictionaryRequestsOriginalDictionaryTitle = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) ? "" : req.DictionaryRequest.OriginalDictionaryTitle;
            model.DictionaryRequestsMajorOfAuther = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) || !(req.DictionaryRequest.MajorOfAuther > 0) ? "" : req.DictionaryRequest.Major_MajorOfAuther.NameArabic;
            model.DictionaryRequestsNoOfRecommendations = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) || !(req.DictionaryRequest.NoOfRecommendations > 0) ? "" : req.DictionaryRequest.NoOfRecommendations + "";
            model.DictionaryRequestsRecommendationDate = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) || req.DictionaryRequest.RecommendationDate == null ? "" : req.DictionaryRequest.RecommendationDate.FormatDate(false, true, true);
            model.DictionaryRequestsPublishType = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) || req.DictionaryRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.DictionaryRequest.PublishType.Value).ToString()];
            model.DictionaryRequestsPublisher = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) ? "" : req.DictionaryRequest.Publisher;
            model.DictionaryRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) ? "" : req.DictionaryRequest.PublisherPlace;
            model.DictionaryRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Dictionary || req.DictionaryRequest == null || !(req.DictionaryRequest.RequestId > 0) || req.DictionaryRequest.PublisherDate == null ? "" : req.DictionaryRequest.PublisherDate.Value.FormatDate(false, true, true);
            model.EncyclopediaRequestsMajorOfEncyclopedia = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) || !(req.EncyclopediaRequest.MajorOfEncyclopedia > 0) ? "" : req.EncyclopediaRequest.Major_MajorOfEncyclopedia.NameArabic;
            model.EncyclopediaRequestsIsArabic = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) ? "" : req.EncyclopediaRequest.IsArabic == true ? ApsStrings.yes : ApsStrings.no;
            model.EncyclopediaRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) ? "" : req.EncyclopediaRequest.IsTranslated == true ? ApsStrings.yes : ApsStrings.no;
            model.EncyclopediaRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) ? "" : req.EncyclopediaRequest.TranslatedTo;
            model.EncyclopediaRequestsEncyclopediaTitle = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) ? "" : req.EncyclopediaRequest.EncyclopediaTitle;
            model.EncyclopediaRequestsOriginalEncyclopediaTitle = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) ? "" : req.EncyclopediaRequest.OriginalEncyclopediaTitle;
            model.EncyclopediaRequestsMajorOfAuther = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) || !(req.EncyclopediaRequest.MajorOfAuther > 0) ? "" : req.EncyclopediaRequest.Major_MajorOfAuther.NameArabic;
            model.EncyclopediaRequestsNoOfRecommendations = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) || !(req.EncyclopediaRequest.NoOfRecommendations > 0) ? "" : req.EncyclopediaRequest.NoOfRecommendations + "";
            model.EncyclopediaRequestsRecommendationDate = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) || req.EncyclopediaRequest.RecommendationDate == null ? "" : req.EncyclopediaRequest.RecommendationDate.FormatDate(false, true, true);
            model.EncyclopediaRequestsPublishType = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) || req.EncyclopediaRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.EncyclopediaRequest.PublishType).ToString()];
            model.EncyclopediaRequestsPublisher = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) ? "" : req.EncyclopediaRequest.Publisher;
            model.EncyclopediaRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) ? "" : req.EncyclopediaRequest.PublisherPlace;
            model.EncyclopediaRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Encyclopedia || req.EncyclopediaRequest == null || !(req.EncyclopediaRequest.RequestId > 0) || req.EncyclopediaRequest.PublisherDate == null ? "" : req.EncyclopediaRequest.PublisherDate.Value.FormatDate(false, true, true);
            model.HandbookRequestsHandbookType = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) || req.HandbookRequest.HandbookType == 0 ? "" : LangHelpers.Instance[((HandBookType)req.HandbookRequest.HandbookType).ToString()];
            model.HandbookRequestsIsArabic = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) ? "" : req.HandbookRequest.IsArabic == true ? ApsStrings.yes : ApsStrings.no;
            model.HandbookRequestsIsTranslated = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) ? "" : req.HandbookRequest.IsTranslated == true ? ApsStrings.yes : ApsStrings.no;
            model.HandbookRequestsTranslatedTo = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) ? "" : req.HandbookRequest.TranslatedTo;
            model.HandbookRequestsHandbookTitle = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) ? "" : req.HandbookRequest.HandbookTitle;
            model.HandbookRequestsOriginalHandbookTitle = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) ? "" : req.HandbookRequest.OriginalHandbookTitle;
            model.HandbookRequestsMajorId = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) || !(req.HandbookRequest.MajorId > 0) ? "" : req.HandbookRequest.Major.NameArabic;
            model.HandbookRequestsPublishType = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) || req.HandbookRequest.PublishType == null ? "" : LangHelpers.Instance[((PublishType)req.HandbookRequest.PublishType).ToString()];
            model.HandbookRequestsPublisher = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) ? "" : req.HandbookRequest.Publisher;
            model.HandbookRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) ? "" : req.HandbookRequest.PublisherPlace;
            model.HandbookRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Handbook || req.HandbookRequest == null || !(req.HandbookRequest.RequestId > 0) || req.HandbookRequest.PublisherDate == null ? "" : req.HandbookRequest.PublisherDate.Value.FormatDate(false, true, true);

            model.ThesisRequestsSciDepartmentId = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) || !(req.ThesisRequest.SciDepartmentId > 0) ? "" : req.ThesisRequest.SciDepartmentId + "";
            model.ThesisRequestsSupervisor = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.Supervisor;
            model.ThesisRequestsAsstSupervisor = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.AssistantSupervisor;
            model.ThesisRequestsThesisTitle = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.ThesisTitle;
            model.ThesisRequestsCommitteeMembers = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.CommitteeMembers;
            model.ThesisRequestsDegree = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) || !(req.ThesisRequest.Degree > 0) ? "" : LangHelpers.Instance[((Degree)req.ThesisRequest.Degree).ToString()];
            model.ThesisRequestsGPA = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : LangHelpers.Instance[req.ThesisRequest.Gpa];
            model.ThesisRequestsDateOfDiscussion = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) || req.ThesisRequest.DateOfDiscussion == null ? "" : req.ThesisRequest.DateOfDiscussion.FormatDate(false, true, true);
            model.ThesisRequestsPublishType = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) || !(req.ThesisRequest.PublishType > 0) ? "" : LangHelpers.Instance[((PublishType)req.ThesisRequest.PublishType).ToString()];
            model.ThesisRequestsPublisher = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.Publisher;
            model.ThesisRequestsPublisherPlace = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.PublisherPlace;
            model.ThesisRequestsPublisherDate = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) || req.ThesisRequest.PublisherDate == null ? "" : req.ThesisRequest.PublisherDate.Value.FormatDate(false, true, true);
            model.ThesisRequestsMajorId = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : (!(req.ThesisRequest.MajorId > 0) ? req.ThesisRequest.MajorOther : req.ThesisRequest.Major.NameArabic);
            model.ThesisRequestsCollegeId = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : (!(req.ThesisRequest.CollegeId > 0) ? req.ThesisRequest.CollegeOther : req.ThesisRequest.College.NameArabic);
            model.ThesisRequestsUniversityOther = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.UniversityOther;
            model.ThesisRequestsMajorOther = req.PublicationType != (byte)PublicationType.Thesis || req.ThesisRequest == null || !(req.ThesisRequest.RequestId > 0) ? "" : req.ThesisRequest.MajorOther;

            model.RequestsIsScientificProduction = req.IsScientificProduction == 1 ? ApsStrings.yes : ApsStrings.no;

            // Participants
            model.RequestsNoOfParticipant = !(req.NoOfParticipant > 0) ? "" : req.NoOfParticipant.Value + "";
            model.Participants = req.PublicationType == (byte)PublicationType.Thesis || req.NoOfParticipant <= 0 ? new List<string>() : req.RequestCoresearchers.Select(r => r.Researcher.GetFullName()).ToList();

            // Files
            model.Files = req.RequestFiles.Select(c => new AttachedFile
            {
                FileId = c.Id.ToString(),
                CreatedUserName = c.CreatedByUser.UserName,
                CreatedUserRole = GetUserRoleForFileUploads(c.Description ?? "", c.CreatedByUser),
                Type = (FileType)c.Type,
                PageCount = c.PageCount,
                FileName = Path.GetFileName(c.FileName)
            }).ToList();

            Utils.db.DiscardChanges();

            return model;
        }

        public static ApsUserRole GetUserRoleForFileUploads(string Description, User CreatedByUser)
        {
            var apsUserRole = ApsUserRole.None;
            Enum.TryParse(CreatedByUser.Roles.FirstOrDefault()?.Name, out apsUserRole);
            if (!string.IsNullOrWhiteSpace(Description) && Description.StartsWith("Co-Researcher"))
            {
                apsUserRole = ApsUserRole.CoResearcher;
            }
            return apsUserRole;
        }

        public static ApsUserRole GetUserRoleFromHistory(string Comments, User CreatedByUser)
        {
            var apsUserRole = ApsUserRole.None;
            Enum.TryParse(CreatedByUser.Roles.FirstOrDefault()?.Name, out apsUserRole);
            if (!string.IsNullOrWhiteSpace(Comments) && Comments.StartsWith("[Coresearcher]Responded by"))
            {
                apsUserRole = ApsUserRole.CoResearcher;
            }
            return apsUserRole;
        }

        public static string GetUserNameForFileUploads(string Description, User CreatedByUser)
        {
            var coResearcherName = CreatedByUser?.UserProfile != null ? CreatedByUser.UserProfile.GetFullName() : "";
            if (Description.StartsWith("Co-Researcher"))
            {
                var names = Description.Split('=');
                var userName = names[1];
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    var userId = 0;
                    int.TryParse(userName, out userId);
                    var coresearcher = Utils.db.Researchers.Where(x => x.Id == userId).FirstOrDefault();
                    if (coresearcher != null)
                    {
                        coResearcherName = coresearcher.GetFullName();
                        return coResearcherName;
                    }
                }
            }

            return coResearcherName;
        }

        public static string GetUserNameFromHistory(string Comments, User CreatedByUser)
        {
            var coResearcherName = CreatedByUser?.UserProfile != null ? CreatedByUser.UserProfile.GetFullName() : "";
            if (Comments != null)
            {
                if (Comments.StartsWith("[Coresearcher]Responded by"))
                {
                    var names = Comments.Split(':');
                    var userName = names[1];
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        var userId = 0;
                        int.TryParse(userName, out userId);
                        var coresearcher = Utils.db.Researchers.Where(x => x.Id == userId).FirstOrDefault();
                        if (coresearcher != null)
                        {
                            coResearcherName = coresearcher.GetFullName();
                            return coResearcherName;
                        }
                    }
                }
            }

            return coResearcherName;
        }

        public static string GetUserRoleDisplayName(User _user)
        {
            string roleName = _user.Roles.FirstOrDefault().Name;
            if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
            {
                roleName = LangHelpers.Instance["CommitteeforPAEMember"];
            }
            else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
            {
                roleName = LangHelpers.Instance["DeanshipOfAcademicResearchCouncilMember"];
            }
            else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
            {
                roleName = LangHelpers.Instance["AcademicCouncilMember"];
            }
            else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
            {
                roleName = LangHelpers.Instance["ResearchUnitCouncilMember"];
            }

            return roleName;
        }
    }
}