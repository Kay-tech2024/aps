using APS.Core.Models;
using APS.Core.Workflows;
using APS.Data;
using APS.Data.DbModels;
using APS.TimeMachine;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace APS.Core.Managers
{
    public class RequestsManager : AbstractManager
    {
        public Request GetSingleRequest(long? id)
        {
            return RequestUtils.GetRequests(null).SingleOrDefault(c => c.Id == id);
        }

        public IQueryable<Request> GetRequests(long? CollegeId = null, long? groupId = null, string userId = null, RequestType? type = null, RequestTab? status = null, string queryStr = null, int? pageNo = null, int? pageSize = null, string sortBy = null, string sortDir = null, Translator translator = null)
        {
            long total = 0;
            return GetRequests(CollegeId, groupId, userId, type, status, queryStr, pageNo, pageSize, sortBy, sortDir, translator, out total);
        }

        public IQueryable<Request> GetRequests(long? collegeId, long? CollegeId, string userId, RequestType? type, RequestTab? status, string queryStr, int? pageNo, int? pageSize, string sortBy, string sortDir, Translator translator, out long total)
        {
            total = 0;
            if (Roles == null || Roles.Length == 0)
                return new List<Request>().AsQueryable();

            var subpredicate = PredicateBuilder.BaseAnd<Request>();

            if (CollegeId != null)
            {
                subpredicate = subpredicate.And(r => r.CreatedByUser.CollegeId == CollegeId);
            }

            if (userId != null)
            {
                subpredicate = subpredicate.And(r => r.CreatedBy == userId);
            }

            if (type != null)
            {
                subpredicate = subpredicate.And(r => r.Type == (byte)type);
            }

            if (Utils.HasRolesAny(ApsUserRole.Researcher))
            {
                subpredicate = subpredicate.And(r => r.CreatedBy == CurrentUser.Id || (r.PublicationRequest.CreatedBy == CurrentUser.Id && r.Type != (byte)RequestType.ProofreaderReward && r.Type != (byte)RequestType.RefereeReward));
            }
            else
            {
                subpredicate = subpredicate.And(r => r.Status != (long)RequestStatus.Canceled && r.Status != (long)RequestStatus.PublishRequestNotSubmitted && r.Status != (long)RequestStatus.PublishRequestPendingCoresearchers && r.Status != (long)RequestStatus.PublishRequestRejectedByCoresearchers);

                if (Utils.HasRolesAny(ApsUserRole.Referee))
                {
                    subpredicate = subpredicate.And(r => r.RequestReferees.Count(rr => rr.Referee.UserId == CurrentUser.Id && rr.RefereeId == rr.Referee.Id && rr.RequestReviewSentOn != null && rr.IsDeleted == false) > 0);
                }
                else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember))
                {
                    subpredicate = subpredicate.And(r => r.RequestUsers.Count(u => u.UserId == CurrentUser.Id) > 0 ||
                        r.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers ||
                        r.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsMembers ||
                        r.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesMembers ||
                        r.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers ||
                        r.Status == (long)RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers
                    );
                }
                else if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember))
                {
                    subpredicate = subpredicate.And(r => r.RequestUsers.Count(u => u.UserId == CurrentUser.Id) > 0 ||
                        r.Status == (long)RequestStatus.PublishRequestPendingPAEAfterRefereesMembers ||
                        r.Status == (long)RequestStatus.PublishRequestPendingPAERefereeSelectionMembers
                    );
                }
                else if (Utils.HasRolesAny(ApsUserRole.DeanshipOfAcademicResearchCouncilMember))
                {
                    subpredicate = subpredicate.And(r => r.RequestUsers.Count(u => u.UserId == CurrentUser.Id) > 0 ||
                        r.Status == (long)RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee
                    );
                }
                else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitCouncilMember))
                {
                    subpredicate = subpredicate.And(r => r.RequestUsers.Count(u => u.UserId == CurrentUser.Id) > 0 ||
                        r.Status == (long)RequestStatus.PublishRequestPendingResearchUnitCommittee
                    );
                }
            }

            if (Utils.IsCollegeUser)
            {
                subpredicate = subpredicate.And(r => r.CollegeId == CurrentUser.CollegeId);
            }

            var isSystemAdmin = Utils.HasRolesAny(ApsUserRole.SystemAdmin);

            var predicate = PredicateBuilder.BaseOr<Request>();
            predicate = predicate.Or(r => IsSystemAdmin);

            predicate = predicate.Or(r => r.RequestHistoryLogs.Count(h => h.CreatedBy == CurrentUser.Id) > 0);

            predicate = predicate.Or(subpredicate);
            predicate = predicate.Or(r => r.RewardRequest != null && r.RewardRequest.UserId == CurrentUser.Id);
            predicate = predicate.And(r => r.IsDeleted == false && (r.RequestRevisionId == null || r.RequestRevisionId == 0));

            var query = DbContext.Requests.Where(predicate);

            if (status != null)
            {
                switch (status.Value)
                {
                    case RequestTab.Pending:

                        query = query.Where(r => r.Status != (long)RequestStatus.Completed && r.Status != (long)RequestStatus.Rejected && r.Status != (long)RequestStatus.Canceled);
                        break;

                    case RequestTab.Archive:
                        query = query.Where(r => r.Status == (long)RequestStatus.Completed || r.Status == (long)RequestStatus.Rejected || r.Status == (long)RequestStatus.Canceled);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(queryStr))
            {
                var predicate2 = PredicateBuilder.BaseOr<Request>();

                predicate2 = predicate2.Or(r => r.Title.Contains(queryStr));
                queryStr = queryStr.Trim().ToLower();
                long t = 0;

                if (IsValidNationalId(queryStr))
                {
                    predicate2 = predicate2.Or(r => r.CreatedByUser.UserProfile != null && r.CreatedByUser.UserProfile.NationalId == queryStr);
                }
                else if (long.TryParse(queryStr, out t))
                {
                    predicate2 = predicate2.Or(r => r.CreatedByUser.UserProfile != null && r.CreatedByUser.UserProfile.EmployeeId.Contains(queryStr));
                    predicate2 = predicate2.Or(r => r.Id != 0 && r.Id.ToString().Contains(queryStr));
                }
                else
                {
                    predicate2 = predicate2.Or(r => r.CreatedByUser.FullName.Contains(queryStr));
                    predicate2 = predicate2.Or(r => r.College.NameArabic.Contains(queryStr));

                    var names = Utils.GetNameComponents(queryStr);
                    if (names != null && names.FirstName != null)
                    {
                        predicate2 = predicate2.Or(r => r.CreatedByUser.UserProfile != null && r.CreatedByUser.UserProfile.FirstName == names.FirstName && (names.LastName == null || r.CreatedByUser.UserProfile.LastName == names.LastName));
                    }
                }

                if (translator != null)
                {
                    {
                        var requestTypes = Enum.GetValues(typeof(RequestType));

                        var matchingTypes = new List<RequestType>();

                        foreach (var r in requestTypes)
                        {
                            var arabic = translator.GetLangValueByString(r.ToString(), true, "ar");
                            var english = translator.GetLangValueByString(r.ToString(), true, "en");

                            if (arabic != null && arabic.Contains(queryStr))
                                matchingTypes.Add((RequestType)r);
                            else if (english != null && english.Contains(queryStr))
                                matchingTypes.Add((RequestType)r);
                        }

                        if (matchingTypes.Count > 0)
                        {
                            foreach (var x in matchingTypes)
                            {
                                predicate2 = predicate2.Or(a => a.Type == (byte)x);
                            }
                        }
                    }

                    {
                        var statuses = Enum.GetValues(typeof(RequestStatus));

                        var matchingStatuses = new List<RequestStatus>();

                        foreach (var r in statuses)
                        {
                            var arabic = translator.GetLangValueByString(r.ToString(), true, "ar");
                            var english = translator.GetLangValueByString(r.ToString(), true, "en");

                            if (arabic != null && arabic.Contains(queryStr))
                                matchingStatuses.Add((RequestStatus)r);
                            else if (english != null && english.Contains(queryStr))
                                matchingStatuses.Add((RequestStatus)r);
                        }

                        if (matchingStatuses.Count > 0)
                        {
                            foreach (var x in matchingStatuses)
                            {
                                predicate2 = predicate2.Or(a => a.Status == (long)x);
                            }
                        }
                    }
                }

                query = query.Where(predicate2);
            }

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (string.IsNullOrWhiteSpace(sortDir))
                    sortDir = "desc";
                else
                    sortDir = sortDir.ToLower().Trim();

                switch (sortBy.Trim().ToUpper())
                {
                    case "ID":
                        query = sortDir == "asc" ? query.OrderBy(r => r.Id) : query.OrderByDescending(r => r.Id);
                        break;

                    case "UNIQUEID":
                        query = sortDir == "asc" ? query.OrderBy(r => r.UniqueId) : query.OrderByDescending(r => r.UniqueId);
                        break;

                    case "TITLE":
                        query = sortDir == "asc" ? query.OrderBy(r => r.Title).ThenBy(r => r.PublicationRequest.Title) : query.OrderByDescending(r => r.PublicationRequest.Title).ThenByDescending(r => r.Title);
                        break;

                    case "COLLEGENAME":
                        var isArabic = Utils.IsUserLanguageArabic;

                        query = sortDir == "asc" ? query.OrderBy(r => !IsArabic ? r.College.NameEnglish : r.College.NameArabic) : query.OrderByDescending(r => !IsArabic ? r.College.NameEnglish : r.College.NameArabic);
                        break;

                    case "NAME":
                        query = sortDir == "asc" ? query.OrderBy(r => r.CreatedByUser.FullName) : query.OrderByDescending(r => r.CreatedByUser.FullName);
                        break;

                    case "TYPE":
                        query = sortDir == "asc" ? query.OrderBy(r => r.Type) : query.OrderByDescending(r => r.Type);
                        break;

                    case "DATECREATED":
                    case "DATECREATEDACTUAL":
                        query = sortDir == "asc" ? query.OrderBy(r => r.Created) : query.OrderByDescending(r => r.Created);
                        break;

                    case "STATUS":
                        query = sortDir == "asc" ? query.OrderBy(r => r.Status) : query.OrderByDescending(r => r.Status);
                        break;

                    case "DATEUPDATED":
                    case "DATEUPDATEDACTUAL":
                    default:
                        query = sortDir == "asc" ? query.OrderBy(r => r.Updated) : query.OrderByDescending(r => r.Updated);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(r => r.Updated);
            }

            total = query.LongCount();

            if (pageSize >= 1 && pageNo >= 1)
            {
                query = query.Skip((pageNo.Value - 1) * pageSize.Value).Take(pageSize.Value);
            }

            return query;
        }

   
        public List<RequestAction> GetRequestActions(RequestListModel request)
        {
            var workflow = (IRequestWorkflow)null;
            switch (request.TypeEnum)
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

            if (workflow == null) return new List<RequestAction>();
            return workflow.GetAllowedActions(request);
        }

        public bool CanSeePublishPublishRequsts
        {
            get
            {
                if (Utils.HasRolesAny(ApsUserRole.None, ApsUserRole.Accountant, ApsUserRole.FinancialDepartmentDirecor, ApsUserRole.Webmaster))
                    return false;

                return true;
            }
        }

        public static int GetCoResearcherResponseTimeDays()
        {
            var coResearcherResponseSetting = Utils.db.Settings.Where(x => x.Key == Constants.CORESEARCHER_RESPONSE_DAYS).FirstOrDefault();
            int coResearcherResponseTimeDays = 5; // Default Value
            if (coResearcherResponseSetting != null)
            {
                coResearcherResponseTimeDays = Convert.ToInt32(coResearcherResponseSetting.Value);
            }

            return coResearcherResponseTimeDays;
        }

        public void UpdateCoResearcherResponseFoReviewRequest()
        {
            Utils.db.Configuration.LazyLoadingEnabled = false;

            int CoResearcherResponseTimeDays = GetCoResearcherResponseTimeDays();
            int totalMins = CoResearcherResponseTimeDays * 24 * 60;
            var notResponded = Utils.db.RequestCoresearchers.Where(x => x.Created != null && !x.IsDeleted && !x.IsKeyUsed).ToList();
            List<long> requestIds = new List<long>();
            foreach (var coresearcher in notResponded)
            {
                if (TM.Now.Subtract(coresearcher.Created).TotalMinutes > totalMins)
                {
                    coresearcher.DidResearcherAgree = false;
                    coresearcher.IsKeyUsed = true;
                    coresearcher.Updated = TM.Now;
                    coresearcher.UpdatedBy = "00000000-0000-0000-0000-000000000000";
                    Utils.db.SaveChanges();
                    if (!requestIds.Contains(coresearcher.RequestId))
                    {
                        requestIds.Add(coresearcher.RequestId);
                    }
                }
            }

            if (requestIds.Count > 0)
            {
                foreach (var requestId in requestIds)
                {
                    UpdateRequestStatusAfterCoResearcherResponseForRequestUpdate(requestId, Utils.SystemAdminUserId, ApsUserRole.SystemAdmin);
                }
            }
        }

        public void UpdateRequestStatusAfterCoResearcherResponseForRequestUpdate(long requestId, string userId, ApsUserRole _role)
        {
            var isAllResponded = true;
            var request = DbContext.Requests.Where(x => x.Id == requestId && !x.IsDeleted).FirstOrDefault();

            foreach (var r in request.RequestCoresearchers)
            {
                if (!r.IsKeyUsed)
                {
                    isAllResponded = false;
                }
            }

            if (isAllResponded)
            {
                request.Status = (byte)RequestStatus.PublishRequestRejectedByCoresearchers;
                request.CurrentlyWith = request.CreatedBy;

                if (request.CurrentlyWithUser != null)
                    Utils.db.Entry(request.CurrentlyWithUser).State = EntityState.Detached;

                Utils.db.Entry(request).State = EntityState.Modified;
                Utils.db.SaveChanges();
            }

            Utils.db.RequestHistoryLogs.Add(new RequestHistoryLog
            {
                Created = DateTime.Now,
                CreatedBy = Utils.CurrentUser == null ? userId : Utils.CurrentUser.Id,
                Url = Utils.GetFullUrl(),
                IpAddress = Utils.GetUserIP(),
                RequestId = request.Id,
                UserAgent = HttpContextManager.Current.Request.UserAgent,
                Action = (byte)RequestAction.RejectToReviewBySystem,
                Status = request.Status,
                Type = (byte)RequestHistoryTypes.RejectByCoResearcher
            });
        }
    }
}