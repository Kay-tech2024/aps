using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APS.Core.Workflows
{
    public class DistributionRequestWorkflow : IRequestWorkflow
    {
        public List<FieldModel> Fields
        {
            get
            {
                return new List<FieldModel>
                {
                };
            }
        }

        public List<RequestAction> GetAllowedActions(RequestListModel request)
        {
            var dict = GetAllowedActionsTransitionsWithFiles(request);
            return dict.Select(a => a.Item1).ToList();
        }

        public List<Tuple<RequestAction, RequestStatus, string[], FileType[]>> GetAllowedActionsTransitionsWithFiles(RequestListModel request, params string[] parameters)
        {
            if (request == null || request.TypeEnum != RequestType.Distribution)
                return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();


            var list = new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            foreach (var st in GetRequestStatus(request))
            {
                switch (st)
                {
                    case RequestStatus.DistributionPendingDistributionListPreparation:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.PrepareDistributionList, RequestStatus.DistributionPendingDistributionListApprovalViceDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.DistributionPendingDistributionListApprovalViceDean:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveAndForwardToDeanOfAcademicResearch, RequestStatus.DistributionPendingDistributionListApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.RejectAndReturnToOfficerOfPrintingUnit, RequestStatus.DistributionPendingDistributionListPreparation, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.DistributionPendingDistributionListApproval:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveApproval, RequestStatus.DistributionPendingCopyDistribution, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveRejection, RequestStatus.DistributionPendingDistributionListApprovalViceDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.DistributionPendingCopyDistribution:
                        if (Utils.HasRolesAny(ApsUserRole.PublicRelationsDirector) && request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 0)
                        {
                            if (request.RequestUsers.Count(rr => rr.UserId == request.PublicationRequest.CreatedBy && rr.Action == null) > 0)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToResearcher, RequestStatus.DistributionPendingResearcherConfirmation, null, null));
                            }

                            if (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 1)
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmDeliveringCopiesToResaercher, RequestStatus.DistributionPendingCopyDistribution, null, null));
                            else
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmDeliveringCopiesToResaercher, RequestStatus.DistributionPendingDistributionClosure, null, null));
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 0)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmReceivalOfCopies, RequestStatus.DistributionPendingCopyDistribution, null, null));
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit) && request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 0)
                        {
                            if (request.RequestUsers.Count(rr => rr.UserRoles.Count(x => x == ApsUserRole.KingFahadNationalLibraryOfficer.ToString()) > 0 && rr.Action == null) > 0)
                            {
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToKFNL, RequestStatus.DistributionPendingResearcherConfirmation, null, null));
                            }

                            if (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 1)
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmDeliveringCopiesToKFNL, RequestStatus.DistributionPendingCopyDistribution, null, null));
                            else
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmDeliveringCopiesToKFNL, RequestStatus.DistributionPendingDistributionClosure, null, null));
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.KingFahadNationalLibraryOfficer) && request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 0)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmReceivalOfCopies, RequestStatus.DistributionPendingCopyDistribution, null, null));
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.DeanOfLibraryAffairs) && request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 0)
                        {
                            if (request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 1)
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmDeliveringCopiesToAgencies, RequestStatus.DistributionPendingCopyDistribution, null, null));
                            else
                                list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmDeliveringCopiesToAgencies, RequestStatus.DistributionPendingDistributionClosure, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.DistributionPendingResearcherConfirmation:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.PublicationRequest.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ConfirmReceivalOfCopies, RequestStatus.DistributionPendingResearcherConfirmation, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.DistributionPendingKFNLCopiesDistributionByPrintingUnit:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Submit, RequestStatus.DistributionPendingCopiesDistributionByLibraryAffairs, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.DistributionPendingCopiesDistributionByLibraryAffairs:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfLibraryAffairs))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Submit, RequestStatus.DistributionPendingDistributionClosure, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.DistributionPendingDistributionClosure:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.CloseRequest, RequestStatus.Completed, null, null));
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