using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APS.Core.Workflows
{
    public class ProofReaderRewardWorkflow : IRequestWorkflow
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
            if (request == null || request.TypeEnum != RequestType.ProofreaderReward)
                return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            var list = new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            foreach (var st in GetRequestStatus(request))
            {
                switch (st)
                {
                    case RequestStatus.RewardPendingApprovalByViceDean:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToDeanOfAcademicResearch,
                                RequestStatus.RewardPendingApprovalByDean,
                                null, null)
                            );
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.RewardPendingApprovalByDean:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveAndForwardToFiancialDepartmentDirector, RequestStatus.RewardPendingFinancialDepartmentDirector, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Reject, RequestStatus.RewardPendingApprovalByViceDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.RewardPendingFinancialDepartmentDirector:
                        if (Utils.HasRolesAny(ApsUserRole.FinancialDepartmentDirecor))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToAccountant,
                                RequestStatus.RewardPendingAccountant,
                                null, null)
                            );

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToDeanshipOfAcademicResearch,
                                RequestStatus.RewardPendingApprovalByDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.RewardPendingAccountant:
                        if (Utils.HasRolesAny(ApsUserRole.Accountant))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.PaymentCompleted, RequestStatus.Completed, null, null));
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

        public List<Tuple<RequestAction, RequestStatus, string[]>> GetAllowedActionsTransitions(RequestListModel request)
        {
            throw new NotImplementedException();
        }
    }
}