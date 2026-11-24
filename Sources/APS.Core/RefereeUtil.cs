using APS.Core.Managers;
using APS.Data;
using APS.Data.DbModels;
using APS.TimeMachine;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace APS.Core
{
    public class RefereeUtil
    {
        protected SmtpSettings _smtpSettings;

        public T GetInstance<T>() where T : new()
        {
            if (Utils.HasOwinContext)
            {
                var instance = HttpContext.Current.GetOwinContext().Get<T>(typeof(T).FullName);
                if (instance == null)
                {
                    instance = new T();
                    HttpContext.Current.GetOwinContext().Set<T>(typeof(T).FullName, instance);
                }

                return instance;
            }

            return new T();
        }

        public TemplatesManager TemplateUtils
        {
            get
            {
                return GetInstance<TemplatesManager>();
            }
        }

        public SmtpSettings SmtpSettings
        {
            get
            {
                if (_smtpSettings == null)
                {
                    _smtpSettings = Utils.GetSmtpSettings(Utils.db);
                }

                return _smtpSettings;
            }
        }

        public void SendNotificationToNextRefereeForFundingRequestReview(long requestId, int PreRequestReviewOrderNumber, RefereeType refereeType = RefereeType.Unspecified)
        {
            RequestReferee _requestRefree = null;
            if (refereeType == RefereeType.Normal)
            {
                _requestRefree = Utils.db.RequestReferees.Include("Referee").Where(r => r.RequestId == requestId && r.RequestReviewSentOn == null && r.IsDeleted == false && r.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString()).FirstOrDefault();
            }
            else
            {
                if (PreRequestReviewOrderNumber == 0)
                {
                    var _obj = Utils.db.RequestReferees.Where(p => p.RequestId == requestId && p.RequestReviewSentOn != null && p.IsDeleted == false && p.CreatedByUser.Roles.FirstOrDefault().Name != ApsUserRole.AcademicCouncilHead.ToString()).ToList();
                    if (_obj.Count > 0)
                    {
                        PreRequestReviewOrderNumber = _obj.Max(p => p.RequestReviewOrderNumber).Value;
                    }
                }

                _requestRefree = Utils.db.RequestReferees.Include("Referee").Where(x => x.RequestId == requestId && x.CreatedByUser.Roles.FirstOrDefault().Name != ApsUserRole.AcademicCouncilHead.ToString() && x.RequestReviewOrderNumber == (PreRequestReviewOrderNumber + 1) && x.IsDeleted == false).FirstOrDefault();
            }
            if (_requestRefree != null)
            {
                this.SendNotificationToRefereeForFundingRequestReview(requestId, _requestRefree.RefereeId, _requestRefree);
                _requestRefree.RequestReviewSentOn = TM.Now;
                _requestRefree.Updated = TM.Now;
                Utils.db.SaveChanges();
            }
        }

        public bool ReturnToCommitteForPAEHeadToSelectMoreRefereeIfRequired(long requestId)
        {
            var _requestRefrees = Utils.db.RequestReferees.Where(x => x.RequestId == requestId && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString() && x.IsDeleted == false).ToList();
            int notSubmitted = _requestRefrees.Count(x => x.SubmittedOn == null);
            // int rejectToReview = _requestRefrees.Count(rr => rr.Action == (byte)Data.RequestAction.RejectToReview);
            int totalAccepted = _requestRefrees.Count(rr => rr.Action == (byte)Data.RequestAction.Approve || rr.Action == (byte)Data.RequestAction.ApproveModifications);
            int totalAcceptedWithComments = _requestRefrees.Count(rr => rr.Action == (byte)Data.RequestAction.ApproveWithComments);
            int totalRejected = _requestRefrees.Count(rr => rr.Action == (byte)Data.RequestAction.Reject || rr.Action == (byte)Data.RequestAction.RejectModifications);

            //int _totalResponse = totalAccepted + totalAcceptedWithComments + totalRejected;
            if (notSubmitted == 0 && totalAccepted + totalAcceptedWithComments <= 1 && totalRejected <= 1)
            {
                var request = Utils.db.Requests.Find(requestId);
                request.Status = (long)RequestStatus.PublishRequestPendingPAERefereeSelectionHead;
                request.Updated = TM.Now;
                Utils.db.SaveChanges();

                return true;
            }

            return false;
        }

        public long CreateRequestRevision(long RequestId, bool AllowMoreThanOneRevision)
        {
            long RevisionRequestId = 0;
            int totalRows = Utils.db.Requests.Where(x => x.RequestRevisionId == RequestId).Count();
            if (totalRows == 0 || AllowMoreThanOneRevision)
            {
                var request = Utils.db.Requests.AsNoTracking().Where(x => x.Id == RequestId).FirstOrDefault();
                if (request != null)
                {
                    DbContextTransaction trans = null;
                    try
                    {
                        using (trans = Utils.db.Database.BeginTransaction())
                        {
                            // Inserting into Requests
                            request.RequestRevisionId = RequestId;
                            request.Created = TM.Now;
                            request.Updated = TM.Now;
                            request.Id = 0;

                            Utils.db.Requests.Add(request);
                            Utils.db.SaveChanges();
                            RevisionRequestId = request.Id;

                            if (RevisionRequestId != 0)
                            {
                                // Inserting into RequestCoresearchers
                                var RequestCoresearchers = Utils.db.RequestCoresearchers.AsNoTracking().Where(x => x.RequestId == RequestId).ToList();
                                if (RequestCoresearchers.Count() > 0)
                                {
                                    foreach (var _data in RequestCoresearchers)
                                    {
                                        _data.Created = TM.Now;
                                        _data.Updated = TM.Now;
                                        _data.RequestId = RevisionRequestId;
                                        _data.Id = 0;
                                        Utils.db.RequestCoresearchers.Add(_data);
                                    }
                                }

                                // Inserting into RequestFundingSources
                                var RequestFundingSources = Utils.db.RequestDatas.AsNoTracking().Where(x => x.RequestId == RequestId).ToList();
                                if (RequestFundingSources.Count() > 0)
                                {
                                    foreach (var _data in RequestFundingSources)
                                    {
                                        _data.Created = TM.Now;
                                        _data.Updated = TM.Now;
                                        _data.RequestId = RevisionRequestId;
                                        _data.Id = 0;
                                        Utils.db.RequestDatas.Add(_data);
                                    }
                                }

                                // Inserting into RequestTimeline
                                var RequestTimeline = Utils.db.RequestDistributionListAgencies.AsNoTracking().Where(x => x.RequestId == RequestId).ToList();
                                if (RequestTimeline.Count() > 0)
                                {
                                    foreach (var _data in RequestTimeline)
                                    {
                                        _data.Created = TM.Now;
                                        _data.Updated = TM.Now;
                                        _data.RequestId = RevisionRequestId;
                                        _data.Id = 0;
                                        Utils.db.RequestDistributionListAgencies.Add(_data);
                                    }
                                }

                                // Inserting into RequestFiles
                                var RequestFiles = Utils.db.RequestFiles.AsNoTracking().Where(x => x.RequestId == RequestId).ToList();
                                if (RequestFiles.Count() > 0)
                                {
                                    foreach (var _data in RequestFiles)
                                    {
                                        _data.Created = TM.Now;
                                        _data.RequestId = RevisionRequestId;
                                        _data.Id = Guid.NewGuid();
                                        Utils.db.RequestFiles.Add(_data);
                                    }
                                }

                                Utils.db.SaveChanges();
                                trans.Commit();
                            }
                        }
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                        Logger.LogExceptionAsync(ex);
                        if (trans != null)
                        {
                            try
                            {
                                trans.Rollback();
                            }
                            catch
                            {
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogExceptionAsync(ex);
                        if (trans != null)
                        {
                            try
                            {
                                trans.Rollback();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }

            return RevisionRequestId;
        }

        public void CheckRefereeFundingReviewRequestExpiryAndSendNotificationToNextReferee()
        {
            try
            {
                Utils.db.Configuration.LazyLoadingEnabled = false;
                int externalRefereeResponseTimeDays = this.GetExtrnalRefereeResponseTimeDays();
                int totalMins = externalRefereeResponseTimeDays * 24 * 60;
                var requestsNotResponded = Utils.db.RequestReferees.Where(x => x.RequestReviewSentOn != null && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString() && x.Action == null && x.IsDeleted == false).ToList();
                if (requestsNotResponded != null)
                {
                    foreach (var referee in requestsNotResponded)
                    {
                        if (TM.Now.Subtract(referee.RequestReviewSentOn.Value).TotalMinutes > totalMins)
                        {
                            referee.Action = (byte)Data.RequestAction.RejectToReviewBySystem;
                            referee.Updated = TM.Now;
                            referee.DidRefereeAgree = false;
                            referee.SubmittedOn = TM.Now;
                            Utils.db.SaveChanges();
                            this.SendNotificationToNextRefereeForFundingRequestReview(referee.RequestId, 0, RefereeType.Normal);
                            var req_returned = ReturnToCommitteForPAEHeadToSelectMoreRefereeIfRequired(referee.RequestId);
                            if (!req_returned)
                            {
                                var request = Utils.db.Requests.Find(referee.RequestId);
                                if (request != null)
                                {
                                    long currentStatus = request.Status;
                                    if (request.Status == (long)RequestStatus.PublishRequestPendingReferees)
                                    {
                                        request.Status = (long)RequestStatus.PublishRequestPendingSpareReferee;
                                        request.Updated = TM.Now;
                                        Utils.db.SaveChanges();

                                        Utils.db.RequestHistoryLogs.Add(new RequestHistoryLog
                                        {
                                            RequestId = request.Id,
                                            Url = "",
                                            Status = currentStatus,
                                            Action = (byte)(RequestAction.RejectToReviewBySystem),
                                            UserAgent = "",
                                            IpAddress = "",
                                            Created = TM.Now,
                                            CreatedBy = Utils.SystemAdminUserId,
                                        });

                                        Utils.db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }

                int refereeResponseTimeDays = this.GetRefereeResponseTimeDays();
                totalMins = externalRefereeResponseTimeDays * 24 * 60;
                byte _actionCode = (byte)Data.RequestAction.AcceptToReview;
                var requestsNotReviewed = Utils.db.RequestReferees.Where(x => x.RequestReviewSentOn != null && x.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString() && x.Action == _actionCode && x.IsDeleted == false).ToList();
                if (requestsNotReviewed != null)
                {
                    foreach (var referee in requestsNotReviewed)
                    {
                        if (TM.Now.Subtract(referee.RequestReviewAcceptOn.Value).TotalMinutes > totalMins)
                        {
                            referee.Action = (byte)Data.RequestAction.AcceptToReviewButNotResponded;
                            referee.Updated = TM.Now;
                            referee.SubmittedOn = TM.Now;

                            Utils.db.SaveChanges();
                            this.SendNotificationToNextRefereeForFundingRequestReview(referee.RequestId, 0, RefereeType.Normal);
                            var req_returned = ReturnToCommitteForPAEHeadToSelectMoreRefereeIfRequired(referee.RequestId);
                            if (!req_returned)
                            {
                                var request = Utils.db.Requests.Find(referee.RequestId);
                                if (request != null)
                                {
                                    long currentStatus = request.Status;
                                    if (request.Status == (long)RequestStatus.PublishRequestPendingReferees)
                                    {
                                        request.Status = (long)RequestStatus.PublishRequestPendingSpareReferee;
                                        request.Updated = TM.Now;
                                        Utils.db.SaveChanges();

                                        Utils.db.RequestHistoryLogs.Add(new RequestHistoryLog
                                        {
                                            RequestId = request.Id,
                                            Url = "",
                                            Status = currentStatus,
                                            Action = (byte)(RequestAction.AcceptToReviewButNotResponded),
                                            UserAgent = "",
                                            IpAddress = "",
                                            Created = TM.Now,
                                            CreatedBy = Utils.SystemAdminUserId,
                                        });

                                        Utils.db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int GetExtrnalRefereeResponseTimeDays()
        {
            var externalRefereeResponseSetting = Utils.db.Settings.Where(x => x.Key == APS.Core.Constants.REFEREE_EXTERNAL_ACCEPTANCE_DAYS).FirstOrDefault();
            int externalRefereeResponseTimeDays = 5; // Default Value
            if (externalRefereeResponseSetting != null)
            {
                externalRefereeResponseTimeDays = Convert.ToInt32(externalRefereeResponseSetting.Value);
            }

            return externalRefereeResponseTimeDays;
        }

        public int GetRefereeRequestFeedbackRejectCutOffMarks()
        {
            var cutoffMarksObj = Utils.db.Settings.Where(x => x.Key == APS.Core.Constants.REFEREE_REQUEST_FEEDBACK_REJECT_CUTOFF).FirstOrDefault();
            int cutoffMarks = 75; // Default Value
            if (cutoffMarksObj != null)
            {
                cutoffMarks = Convert.ToInt32(cutoffMarksObj.Value);
            }

            return cutoffMarks;
        }

        public int GetRefereeResponseTimeDays()
        {
            var externalRefereeResponseSetting = Utils.db.Settings.Where(x => x.Key == APS.Core.Constants.REFEREE_RESPONSE_DAYS).FirstOrDefault();
            int externalRefereeResponseTimeDays = 5; // Default Value
            if (externalRefereeResponseSetting != null)
            {
                externalRefereeResponseTimeDays = Convert.ToInt32(externalRefereeResponseSetting.Value);
            }

            return externalRefereeResponseTimeDays;
        }

        public void SendNotificationToRefereeForFundingRequestReview(long requestId, int refereeId, RequestReferee _data = null)
        {
            try
            {
                if (_data == null)
                {
                    _data = Utils.db.RequestReferees.Where(x => x.RequestId == requestId && x.RefereeId == refereeId && x.IsDeleted == false).FirstOrDefault();
                }

                if (_data != null)
                {
                    Request requestData = _data.Request;
                    Referee refereeData = _data.Referee;
                    Template _smsTemplte = null;
                    Template _emailTemplte = null;

                    int externalRefereeResponseTimeDays = this.GetExtrnalRefereeResponseTimeDays();
                    if (refereeData != null && requestData != null)
                    {
                        string fullName = refereeData.FirstName + " " + refereeData.LastName;
                        if (refereeData.UserId == null)
                        {// external referee
                            _smsTemplte = TemplateUtils[TemplateKey.ExternalRefereeSMSNotification];
                            _emailTemplte = TemplateUtils[TemplateKey.ExternalRefereeEmailNotification];
                        }
                        else
                        {
                            _smsTemplte = TemplateUtils[TemplateKey.InternalRefereeSMSNotification];
                            _emailTemplte = TemplateUtils[TemplateKey.InternalRefereeEmailNotification];
                        }

                        if (!String.IsNullOrEmpty(refereeData.MobileNo))
                        {
                            string body = Utils.IsUserLanguageArabic ? _smsTemplte.BodyArabic : _smsTemplte.BodyEnglish;
                            body = body.Replace("{FullName}", fullName);
                            SmsService.SendMessage(body, new string[] { refereeData.MobileNo });
                        }
                        if (_emailTemplte != null)
                        {
                            // send Email
                            string subject = Utils.IsUserLanguageArabic ? _emailTemplte.SubjectArabic : _emailTemplte.SubjectEnglish;
                            string body = Utils.IsUserLanguageArabic ? _emailTemplte.BodyArabic : _emailTemplte.BodyEnglish;
                            // Pattren: RequestId~RefereeId~RefereeEmail
                            string baseURL = System.Configuration.ConfigurationManager.AppSettings["ApplicationUrl"];

                            string _publicLinkHash = Utils.Encrypt(requestId.ToString() + Core.Constants.ExternalRefereeParamsConnector.ToString() + refereeData.Id + Core.Constants.ExternalRefereeParamsConnector.ToString() + refereeData.Email);
                            string internalLink = baseURL + "/Requests/View/" + requestId.ToString();
                            string _publicLink = baseURL + "/Account/ExternalReferee?_h=" + HttpUtility.UrlEncode(_publicLinkHash);
                            string RequestTitle = requestData.Title;
                            body = body.Replace(" {FullName}", fullName).
                                        Replace("{RequestTitle}", RequestTitle).
                                        Replace("{REQUEST_DETAIL_PUBLIC_LINK}", _publicLink).
                                        Replace("{REQUEST_DETAIL_INTERNAL_LINK}", internalLink);

                            EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(refereeData.Email), null, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}