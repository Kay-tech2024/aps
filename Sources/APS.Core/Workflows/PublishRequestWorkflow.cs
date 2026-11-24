using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APS.Core.Workflows
{
    public class PublishRequestWorkflow : IRequestWorkflow
    {
        public List<RequestAction> GetAllowedActions(RequestListModel request)
        {
            var dict = GetAllowedActionsTransitionsWithFiles(request);
            return dict.Select(a => a.Item1).ToList();
        }

        public List<Tuple<RequestAction, RequestStatus, string[], FileType[]>> GetAllowedActionsTransitionsWithFiles(RequestListModel request, params string[] parameters)
        {
            if (request == null || (request.TypeEnum != RequestType.PublishRequest && request.TypeEnum != RequestType.RepublishRequest))
                return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            var list = new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            foreach (var st in GetRequestStatus(request))
            {
                switch (st)
                {
                    case RequestStatus.PublishRequestNotSubmitted:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.CreatedBy == Utils.CurrentUser.Id)
                        {
                            if (request.NoOfParticipant > 0)
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Submit,
                                RequestStatus.PublishRequestPendingCoresearchers, null, new FileType[] { FileType.Publication }));
                            else
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Submit,
                                RequestStatus.PublishRequestPendingResearchUnitSecretary, null, new FileType[] { FileType.Publication }));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Cancel,
                                RequestStatus.Canceled, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PublishRequestPendingCoresearchers:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.RequestCoresearchers.Count(cr => cr.ResearcherId == Utils.ResearcherId && cr.DidResearcherAgree == null) > 0)
                        {
                            if (request.RequestCoresearchers.Count(cr => cr.DidResearcherAgree == null) == 1)
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.AcceptedByCoResearcher,
                                RequestStatus.PublishRequestPendingResearchUnitSecretary, null, null));
                            else
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.AcceptedByCoResearcher,
                                RequestStatus.PublishRequestPendingCoresearchers, null, null));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectAndReturnToResearcher,
                                RequestStatus.PublishRequestRejectedByCoresearchers, new string[] { request.CreatedBy }, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ResubmitAndReturnToCoresearchers,
                                RequestStatus.PublishRequestPendingCoresearchersAfterModified, null, new FileType[] { FileType.Publication }));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Cancel,
                                RequestStatus.Canceled, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingCoresearchersAfterModified:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.RequestCoresearchers.Count(cr => cr.ResearcherId == Utils.ResearcherId && cr.DidResearcherAgree == null) > 0)
                        {
                            if (request.RequestCoresearchers.Count(cr => cr.DidResearcherAgree == null) == 1)
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.AcceptedByCoResearcher,
                                RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModifications, null, null));
                            else
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.AcceptedByCoResearcher,
                                RequestStatus.PublishRequestPendingCoresearchersAfterModified, null, null));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectAndReturnToResearcher,
                                RequestStatus.PublishRequestRejectedByCoresearchersAfterRefereeAndModified, new string[] { request.CreatedBy }, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestRejectedByCoresearchers:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ResubmitAndReturnToCoresearchers,
                                RequestStatus.PublishRequestPendingCoresearchers, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Cancel,
                                RequestStatus.Canceled, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingResearchUnitSecretary:
                        if (Utils.HasRolesAny(ApsUserRole.ResearchUnitScretary) && request.CollegeId == Utils.UserUtils.CurrentUser.CollegeId)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToResearchUnitHead,
                                RequestStatus.PublishRequestPendingResearchUnitHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToResearcher,
                                RequestStatus.PublishRequestPendingModificationsAfterRUS, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingModificationsAfterRUS:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Resubmit,
                                RequestStatus.PublishRequestPendingCoresearchers, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Cancel,
                                RequestStatus.Canceled, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingResearchUnitHead:
                    case RequestStatus.PublishRequestPendingResearchUnitCommittee:
                        if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead) && request.CollegeId == Utils.UserUtils.CurrentUser.CollegeId)
                        {
                            if (st != RequestStatus.PublishRequestPendingResearchUnitCommittee)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingResearchUnitCommittee, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToViceDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectAndForwardToViceDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToInitialReferee,
                                RequestStatus.PublishRequestPendingInitialRefereeForRUH, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToResearchUnitSecretary,
                                RequestStatus.PublishRequestPendingResearchUnitSecretary, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingResearchUnitCommittee && Utils.HasRolesAny(ApsUserRole.ResearchUnitCouncilMember) && request.CollegeId == Utils.UserUtils.CurrentUser.CollegeId && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Approve,
                                RequestStatus.PublishRequestPendingResearchUnitCommittee, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Reject,
                                RequestStatus.PublishRequestPendingResearchUnitCommittee, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingInitialRefereeForRUH:
                        if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.CreatedByUserRole == ApsUserRole.ResearchUnitHead.ToString() && rr.IsDeleted == false) > 0)
                        {
                            var referee = request.RequestReferees.Where(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.CreatedByUserRole == ApsUserRole.ResearchUnitHead.ToString()).FirstOrDefault();
                            if (referee.DidRefereeAgree != true && referee.Action == null)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.AcceptToReview,
                                RequestStatus.PublishRequestPendingInitialRefereeForRUH, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectToReview,
                                RequestStatus.PublishRequestPendingInitialRefereeForRUH, null, null));
                            }
                            else if (referee.DidRefereeAgree == true && referee.Action != (byte)RequestAction.Approve && referee.Action != (byte)RequestAction.Reject)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.Approve,
                                    RequestStatus.PublishRequestPendingInitialRefereeForRUH, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.Reject,
                                    RequestStatus.PublishRequestPendingInitialRefereeForRUH, null, null));
                            }
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead) && request.CollegeId == Utils.UserUtils.CurrentUser.CollegeId)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToViceDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectAndForwardToViceDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToInitialReferee,
                                RequestStatus.PublishRequestPendingInitialRefereeForRUH, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToResearchUnitSecretary,
                                RequestStatus.PublishRequestPendingResearchUnitSecretary, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingInitialRefereeForDAR:
                        if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.Action == null && rr.RoundNo == 1 && rr.CreatedByUserRole == ApsUserRole.DeanOfAcademicResearch.ToString() && rr.IsDeleted == false) > 0)
                        {
                            //var lastStatusId = request.RequestHistoryLogs.OrderByDescending(x => x.Id).FirstOrDefault().Status;
                            //var referee = request.RequestReferees.Where(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.RoundNo == 1 && rr.Action == null).FirstOrDefault();
                            //var createdUserRole = referee.CreatedByUserRole;
                            //if (createdUserRole == ApsUserRole.DeanOfAcademicResearch.ToString() && (lastStatusId == (byte)RequestStatus.PublishRequestPendingDeanOfAcademicResearchPublishApproval || lastStatusId == (byte)RequestStatus.PublishRequestPendingInitialRefereeForDAR))
                            //{
                            var referee = request.RequestReferees.Where(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.CreatedByUserRole == ApsUserRole.DeanOfAcademicResearch.ToString()).FirstOrDefault();
                            if (referee.DidRefereeAgree != true)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.AcceptToReview,
                                RequestStatus.PublishRequestPendingInitialRefereeForDAR, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectToReview,
                                RequestStatus.PublishRequestPendingInitialRefereeForDAR, null, null));
                            }
                            else if (referee.DidRefereeAgree == true && referee.Action != (byte)RequestAction.Approve && referee.Action != (byte)RequestAction.Reject)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.Approve,
                                    RequestStatus.PublishRequestPendingInitialRefereeForDAR, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.Reject,
                                    RequestStatus.PublishRequestPendingInitialRefereeForDAR, null, null));
                                //list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                //    RequestAction.ApproveWithComments,
                                //    RequestStatus.PublishRequestPendingInitialRefereeForDAR, null, null));
                            }
                            //}
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToUnversityRector,
                                RequestStatus.PublishRequestPendingUniversityRectorPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectAndForwardToUniversityRector,
                                RequestStatus.PublishRequestPendingUniversityRectorPublishRejected, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToViceDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToInitialReferee,
                                RequestStatus.PublishRequestPendingInitialRefereeForDAR, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingDeanOfAcademicResearchPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToResearchUnitHead,
                                RequestStatus.PublishRequestPendingResearchUnitHead, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingDeanOfAcademicResearchPublishApproval:
                    case RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            if (st != RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToUnversityRector,
                                RequestStatus.PublishRequestPendingUniversityRectorPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectAndForwardToUniversityRector,
                                RequestStatus.PublishRequestPendingUniversityRectorPublishRejected, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToViceDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingViceDeanOfAcademicResearchPublishApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToInitialReferee,
                                RequestStatus.PublishRequestPendingInitialRefereeForDAR, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee && Utils.HasRolesAny(ApsUserRole.DeanshipOfAcademicResearchCouncilMember) && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Approve,
                                RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Reject,
                                RequestStatus.PublishRequestPendingDeanshipOfAcademicResearchCommitee, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingUniversityRectorPublishRejected:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityRector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveRejection,
                                RequestStatus.Rejected, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToDeanshipOfAcademicResearch,
                                RequestStatus.PublishRequestPendingDeanOfAcademicResearchPublishApproval, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingUniversityRectorPublishApproval:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityRector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveApproval,
                                RequestStatus.PublishRequestPendingPAERefereeSelection, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToDeanshipOfAcademicResearch,
                                RequestStatus.PublishRequestPendingDeanOfAcademicResearchPublishApproval, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAERefereeSelection:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAESecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToPAECommitteeHead,
                                RequestStatus.PublishRequestPendingPAERefereeSelectionHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToResearchUnitSecretary,
                                RequestStatus.PublishRequestPendingResearchUnitSecretary, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToViceDeanOfAcademicResearch,
                                RequestStatus.PublishRequestPendingPAERefereeSelectionViceDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PublishRequestPendingPAERefereeSelectionViceDean:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAERefereeSelection, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PublishRequestPendingPAERefereeSelectionHead:
                    case RequestStatus.PublishRequestPendingPAERefereeSelectionMembers:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
                        {
                            if (st != RequestStatus.PublishRequestPendingPAERefereeSelectionMembers)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingPAERefereeSelectionMembers, null, null));
                            }

                            if (request.TypeEnum == RequestType.RepublishRequest)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.ForwardToAcademicCouncil,
                                    RequestStatus.PublishRequestPendingAcademicCouncilAfterReferees, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SelectReferees,
                                RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelection, null, null));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAERefereeSelection, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingPAERefereeSelectionMembers && Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember) && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            if (request.TypeEnum == RequestType.RepublishRequest)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToAcademicCouncil,
                                RequestStatus.PublishRequestPendingPAERefereeSelectionMembers, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SelectReferees,
                                RequestStatus.PublishRequestPendingPAERefereeSelectionMembers, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelection:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilSecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToAcademicCouncilHead,
                                RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAERefereeSelection, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead:
                    case RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                        {
                            if (st != RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.SendToCommiteeMembers,
                                    RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers, null, null));

                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.SelectReferees,
                                    RequestStatus.PublishRequestPendingUniversityRectorAFterRefereeSelection, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToUnversityRector,
                                RequestStatus.PublishRequestPendingUniversityRectorAFterRefereeSelection, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectAndReturnToPAECommitee,
                                RequestStatus.PublishRequestPendingPAERefereeSelectionHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncilSecretary,
                                RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelection, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers && Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember) && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Approve,
                                RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Reject,
                                RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionMembers, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PublishRequestPendingUniversityRectorAFterRefereeSelection:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityRector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveApproval,
                                RequestStatus.PublishRequestPendingReferees, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncil,
                                RequestStatus.PublishRequestPendingAcademicCouncilRefereeSelectionHead, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingReferees:
                        if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.IsDeleted == false) > 0)
                        {
                            var totalAccepted = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)Data.RequestAction.Approve && rr.IsDeleted == false);
                            var totalAcceptedWithComments = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)Data.RequestAction.ApproveWithComments && rr.IsDeleted == false);
                            var totalRejected = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)Data.RequestAction.Reject && rr.IsDeleted == false);
                            var totalRejectToReview = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)Data.RequestAction.RejectToReview && rr.IsDeleted == false);

                            if (request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.Action == null && rr.DidRefereeAgree == null && rr.IsDeleted == false) > 0)
                            {
                                list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.AcceptToReview, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.RejectToReview, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                            }
                            else if (request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.SubmittedOn == null && rr.IsDeleted == false) > 0)
                            {
                                if (totalAccepted >= 1 || totalAcceptedWithComments >= 1)
                                {
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Approve, RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.ApproveWithComments, RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Reject, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                }
                                else if (totalRejected >= 1)
                                {
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Approve, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.ApproveWithComments, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Reject, RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                }
                                else
                                {
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Approve, RequestStatus.PublishRequestPendingReferees, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.ApproveWithComments, RequestStatus.PublishRequestPendingReferees, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Reject, RequestStatus.PublishRequestPendingReferees, null, null));
                                }
                            }
                            else
                            {
                                return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                            }
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingSpareReferee:
                        if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.RequestReviewSentOn != null && rr.IsDeleted == false) > 0)
                        {
                            if (request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.Action == null && rr.DidRefereeAgree == null && rr.IsDeleted == false) > 0)
                            {
                                list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.AcceptToReview, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.RejectToReview, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                            }
                            else if (request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.SubmittedOn == null && rr.IsDeleted == false) > 0)
                            {
                                var totalAccepted = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)Data.RequestAction.Approve || rr.Action == (byte)Data.RequestAction.ApproveModifications) && rr.IsDeleted == false);
                                var totalAcceptedWithComments = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)Data.RequestAction.ApproveWithComments && rr.IsDeleted == false);
                                var totalRejected = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)Data.RequestAction.Reject || rr.Action == (byte)Data.RequestAction.RejectModifications) && rr.IsDeleted == false);

                                if (totalAccepted >= 1 || totalAcceptedWithComments >= 1)
                                {
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Approve, RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.ApproveWithComments, RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));

                                    if (totalRejected >= 1)
                                    {
                                        list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Reject, RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                    }
                                    else
                                    {
                                        list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Reject, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    }
                                }
                                else if (totalRejected >= 1)
                                {
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Approve, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.ApproveWithComments, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Reject, RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                }
                                else
                                {
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Approve, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.ApproveWithComments, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(Data.RequestAction.Reject, RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                }
                            }
                            else
                            {
                                return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                            }
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;
                    /*
                case RequestStatus.PublishRequestPendingReferees:
                    if (parameters.Count() != 0)
                    {
                        var grade1 = string.IsNullOrWhiteSpace(parameters[1]) ? 0 : Convert.ToDecimal(parameters[1]);
                        var action1 = parameters[0];

                        if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Count(rr => rr.RefereeUserId != Utils.CurrentUser.Id && rr.RoundNo == 2 && rr.Action != null && rr.IsDeleted == false) > 0)
                        {
                            var referee2 = request.RequestReferees.Where(rr => rr.RefereeUserId != Utils.CurrentUser.Id && rr.RoundNo == 2 && rr.Action != null && rr.IsDeleted == false).FirstOrDefault();
                            var grade2 = referee2.Grade == null ? 0 : Convert.ToDecimal(referee2.Grade);
                            var action2 = ((byte)referee2.Action).ToString();
                            var avg = (grade1 + grade2) / 2;
                            if (avg >= 80)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.Approve,
                                    RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.Reject,
                                    RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                    RequestAction.ApproveWithComments,
                                    RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                            }
                            else
                            {
                                if (action1.Contains("Approve") || (action2 == "2" || action2 == "4"))
                                {
                                    list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                        RequestAction.Approve,
                                        RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                        RequestAction.Reject,
                                        RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                    list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                        RequestAction.ApproveWithComments,
                                        RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                }
                                else
                                {
                                    list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                        RequestAction.Approve,
                                        RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                    list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                        RequestAction.Reject,
                                        RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                    list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                        RequestAction.ApproveWithComments,
                                        RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                }
                            }
                        }
                        else
                        {
                            //return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Approve,
                                RequestStatus.PublishRequestPendingReferees, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Reject,
                                RequestStatus.PublishRequestPendingReferees, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveWithComments,
                                RequestStatus.PublishRequestPendingReferees, null, null));
                        }
                    }
                    else if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.RoundNo == 2 && rr.Action == null && rr.IsDeleted == false) > 0)
                    {
                        if (request.RequestReferees.Count(rr => rr.RoundNo == 2 && rr.Action == null && rr.IsDeleted == false) == 1)
                        {
                            var totalResponses = request.RequestReferees.Count(rr => rr.RoundNo == 2 && rr.Action != null && rr.Action != (byte)RequestAction.Canceled && rr.IsDeleted == false);
                            var totalAccepted = request.RequestReferees.Count(rr => rr.RoundNo == 2 && rr.Action == (byte)RequestAction.Approve && rr.IsDeleted == false);
                            var totalAcceptedWithComments = request.RequestReferees.Count(rr => rr.RoundNo == 2 && rr.Action == (byte)RequestAction.ApproveWithComments && rr.IsDeleted == false);
                            var totalRejected = request.RequestReferees.Count(rr => rr.RoundNo == 2 && rr.Action == (byte)RequestAction.Reject && rr.IsDeleted == false);

                            if (totalAccepted + totalAcceptedWithComments == 1)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Approve,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Reject,
                            RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.ApproveWithComments,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                            }
                            else if (totalRejected == 1)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Approve,
                            RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Reject,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.ApproveWithComments,
                            RequestStatus.PublishRequestPendingSpareReferee, null, null));
                            }
                            else
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Approve,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Reject,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.ApproveWithComments,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                            }
                        }
                        else
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Approve,
                            RequestStatus.PublishRequestPendingReferees, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Reject,
                            RequestStatus.PublishRequestPendingReferees, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.ApproveWithComments,
                            RequestStatus.PublishRequestPendingReferees, null, null));
                        }
                    }
                    else
                    {
                        return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                    }
                    break;

                case RequestStatus.PublishRequestPendingSpareReferee:
                    if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.RoundNo == 3 && rr.Action == null && rr.IsDeleted == false) > 0)
                    {
                        list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Approve,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                        list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.Reject,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                        list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                            RequestAction.ApproveWithComments,
                            RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                    }
                    else
                    {
                        return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                    }
                    break;
                    */
                    case RequestStatus.PublishRequestPendingPAEAfterReferees:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAESecretary))
                        {
                            var totalResponses = request.RequestReferees.Where(rr => rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.IsDeleted == false);
                            var totalAccepted = request.RequestReferees.Where(rr => rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)Data.RequestAction.Approve || rr.Action == (byte)Data.RequestAction.ApproveModifications) && rr.IsDeleted == false);
                            var totalAcceptedWithComments = request.RequestReferees.Where(rr => rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)Data.RequestAction.ApproveWithComments && rr.IsDeleted == false);
                            var totalRejected = request.RequestReferees.Where(rr => rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)Data.RequestAction.Reject || rr.Action == (byte)Data.RequestAction.RejectModifications) && rr.IsDeleted == false);

                            if (totalRejected >= 2)
                            {
                                list.Add(new Tuple<Data.RequestAction, RequestStatus, string[], FileType[]>(
                                   Data.RequestAction.Reject,
                                   RequestStatus.Rejected,
                                   null, null
                               ));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                  RequestAction.ForwardToPAECommitteeHead,
                                  RequestStatus.PublishRequestPendingPAEAfterRefereesHead,
                                  null,
                                  null
                              ));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                  RequestAction.ReturnToViceDeanOfAcademicResearch,
                                  RequestStatus.PublishRequestPendingPAEAfterRefereesViceDean,
                                  null,
                                  null
                              ));
                        }

                        break;

                    case RequestStatus.PublishRequestPendingPAEAfterRefereesViceDean:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                  RequestAction.ReturnToPAECommitteeSecretary,
                                  RequestStatus.PublishRequestPendingPAEAfterReferees,
                                  null,
                                  null
                              ));
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAEAfterRefereesHead:
                    case RequestStatus.PublishRequestPendingPAEAfterRefereesMembers:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
                        {
                            if (st != RequestStatus.PublishRequestPendingPAEAfterRefereesMembers)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                      RequestAction.SendToCommiteeMembers,
                                      RequestStatus.PublishRequestPendingPAEAfterRefereesMembers,
                                      null,
                                      null
                                  ));
                            }

                            var totalAccepted = request.RequestReferees.Where(rr => rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)Data.RequestAction.Approve || rr.Action == (byte)Data.RequestAction.ApproveModifications) && rr.IsDeleted == false);
                            var totalAcceptedWithComments = request.RequestReferees.Where(rr => rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)RequestAction.ApproveWithComments && rr.IsDeleted == false);
                            var totalRejected = request.RequestReferees.Where(rr => rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)RequestAction.Reject || rr.Action == (byte)Data.RequestAction.RejectModifications) && rr.IsDeleted == false);

                            if (totalAccepted >= 2)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                   RequestAction.ApproveAndForwardToAcademicCouncil,
                                   RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApprove,
                                   null,
                                   null
                               ));
                            }
                            else if (totalRejected >= 2)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                   RequestAction.RejectAndForwardToAcademicCouncil,
                                   RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesReject,
                                   null,
                                   null
                               ));
                            }
                            else if (totalAcceptedWithComments >= 1)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                   RequestAction.ApproveWithCommentsAndForwardToAcademicCouncil,
                                   RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveWithComments,
                                   null,
                                   null
                               ));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                                   RequestAction.ReturnToPAECommitteeSecretary,
                                   RequestStatus.PublishRequestPendingPAEAfterReferees,
                                   null,
                                   null
                               ));
                        }
                        else if (st == RequestStatus.PublishRequestPendingPAEAfterRefereesMembers && Utils.HasRolesAny(ApsUserRole.CommitteeforPAEMember) && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(

                               RequestAction.ProvideFeedback,
                               RequestStatus.PublishRequestPendingPAEAfterRefereesMembers,
                               null,
                               null
                           ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApprove:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilSecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToAcademicCouncilHead,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveHead:
                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                        {
                            if (st != RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveApprovalAndForwardToUniversityRector,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRectorApproved, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommittee,
                                RequestStatus.PublishRequestPendingPAEAfterRefereesHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncilSecretary,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApprove, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers && Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember) && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ProvideFeedback,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveMembers, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesReject:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilSecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToAcademicCouncilHead,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectHead:
                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                        {
                            if (st != RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveRejectionAndForwardToUniversityRector,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRectorRejected, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommittee,
                                RequestStatus.PublishRequestPendingPAEAfterRefereesHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncilSecretary,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesReject, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers && Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember) && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ProvideFeedback,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveWithComments:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilSecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToAcademicCouncilHead,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsHead:
                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveWithCommentsMembers:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                        {
                            if (st != RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers, null, null));
                            }

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommittee,
                                RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncilSecretary,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveWithComments, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers && Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember) && (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) == 0 || request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id) > 0))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ProvideFeedback,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectMembers, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterReferees:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilSecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToAcademicCouncilHead,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesHead, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAERefereeSelection, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesHead:
                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesMembers:
                        if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.SendToCommiteeMembers,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesMembers, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Approve,
                                RequestStatus.Completed, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Reject,
                                RequestStatus.Rejected, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommittee,
                                RequestStatus.PublishRequestPendingPAERefereeSelection, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncilSecretary,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterReferees, null, null));
                        }
                        else if (st == RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesMembers && Utils.HasRolesAny(ApsUserRole.AcademicCouncilMember))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Approve,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesMembers, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Reject,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesMembers, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommittee,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesMembers, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsHead:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithComments, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncil,
                                RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsHead, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithComments:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAESecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToResearcher,
                                RequestStatus.PublishRequestPendingModificationsAfterReferees, null, null));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                               RequestAction.ForwardToViceDeanOfAcademicResearch,
                               RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsViceDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithCommentsViceDean:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                               RequestAction.ReturnToPAECommitteeSecretary,
                               RequestStatus.PublishRequestPendingPAECouncilAfterRefereesApproveWithComments, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingModifiedAfterReferees:
                    case RequestStatus.PublishRequestPendingModificationsAfterReferees:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ResubmitAndForwardToPAECommitee,
                                RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModifications, null, new FileType[] { FileType.Publication }));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Cancel,
                                RequestStatus.Canceled, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModifications:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteeforPAESecretary))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToReferees,
                                RequestStatus.PublishRequestPendingRefereesAfterRefereesAndResearcherModifications, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToResearcher,
                                RequestStatus.PublishRequestPendingModificationsAfterReferees, null, null));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                               RequestAction.ForwardToViceDeanOfAcademicResearch,
                               RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModificationsViceDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModificationsViceDean:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ResubmitAndForwardToPAECommitee,
                                RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModifications, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModificationsHead:
                        if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ForwardToReferees,
                                RequestStatus.PublishRequestPendingRefereesAfterRefereesAndResearcherModifications, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToPAECommitteeSecretary,
                                RequestStatus.PublishRequestPendingPAEAfterRefereesAndResearcherModifications, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingRefereesAfterRefereesAndResearcherModifications:
                        if (Utils.HasRolesAny(ApsUserRole.Referee) && request.RequestReferees.Where(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.RefereeUserId == Utils.CurrentUser.Id && rr.Action == (byte)RequestAction.ApproveWithComments && rr.IsDeleted == false) > 0)
                        {
                            var totalAccepted = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)Data.RequestAction.Approve || rr.Action == (byte)Data.RequestAction.ApproveWithComments || rr.Action == (byte)Data.RequestAction.ApproveModifications) && rr.IsDeleted == false);
                            //var totalAcceptedWithComments = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => rr.Action == (byte)Data.RequestAction.ApproveWithComments && rr.IsDeleted == false);
                            var totalRejected = request.RequestReferees.Where(x => x.CreatedByUserRole == ApsUserRole.AcademicCouncilHead.ToString()).Count(rr => (rr.Action == (byte)Data.RequestAction.Reject || rr.Action == (byte)Data.RequestAction.RejectModifications) && rr.IsDeleted == false);
                            if (totalAccepted == 2 || totalRejected == 2)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveModifications,
                                RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectModifications,
                                RequestStatus.PublishRequestPendingPAEAfterReferees, null, null));
                            }
                            else
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveModifications,
                                RequestStatus.PublishRequestPendingSpareReferee, null, null));
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.RejectModifications,
                                RequestStatus.PublishRequestPendingSpareReferee, null, null));
                            }
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRectorApproved:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityRector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveApproval,
                                RequestStatus.Completed, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncil,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesApproveHead, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;

                    case RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRectorRejected:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityRector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveRejection,
                                RequestStatus.Rejected, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToAcademdicCouncil,
                                RequestStatus.PublishRequestPendingAcademicCouncilAfterRefereesRejectHead, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }
                        break;
                }
            }
            return list.Distinct().ToList();
        }

        public List<FieldModel> Fields
        {
            get
            {
                return new List<FieldModel>
                {
                };
            }
        }

        public List<string> GetAllowedActions(RequestListModel request, Translator translator = null)
        {
            return new List<string>();
        }

        public List<RequestHistoryLog> GetRequestHistory(RequestListModel request, Translator translator = null)
        {
            throw new NotImplementedException();
        }

        public RequestStatus[] GetRequestStatus(RequestListModel request)
        {
            return new RequestStatus[] { request.StatusEnum };
        }

        public string GetRequestStatus(RequestListModel request, Translator translator = null)
        {
            throw new NotImplementedException();
        }

        public bool IsActionable(RequestListModel request, Translator translator = null)
        {
            throw new NotImplementedException();
        }

        public bool SaveRequest(RequestModel model, out RequestListModel request, out Dictionary<string, RequestErrorCodes> errors)
        {
            throw new NotImplementedException();
        }

        public bool ValidateRequestModel(RequestModel model, out Dictionary<string, RequestErrorCodes> errors)
        {
            throw new NotImplementedException();
        }

        List<Tuple<RequestAction, RequestStatus, string[]>> IRequestWorkflow.GetAllowedActionsTransitions(RequestListModel request)
        {
            throw new NotImplementedException();
        }
    }
}