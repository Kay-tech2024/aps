using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APS.Core.Workflows
{
    public class PricingRequestWorkflow : IRequestWorkflow
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
            if (request == null || request.TypeEnum != RequestType.Pricing)
                return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            var history = (List<RequestHistoryLog>)null;


            var list = new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            foreach (var st in GetRequestStatus(request))
            {
                switch (st)
                {
                    case RequestStatus.PricingPendingPricingPrepartion:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToUniversityPressDirector, RequestStatus.PricingPendingPricingApprovalByUniversityPress, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PricingPendingPricingApprovalByUniversityPress:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityPressDirector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveAndForwardToDeanshipOfLibraryAffairs, RequestStatus.PricingPendingPricingApprovalByLibraryAffairs, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToViceDeanOfAcademicResearch, RequestStatus.PricingPendingPricingPrepartion, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PricingPendingPricingApprovalByLibraryAffairs:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfLibraryAffairs))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveAndAndForardToDeanOfAcademicResearch, RequestStatus.PricingPendingPricingApprovalByDean, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressDirector, RequestStatus.PricingPendingPricingApprovalByUniversityPress, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.PricingPendingPricingApprovalByDean:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Approve, RequestStatus.Completed, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToDeanshipOfLibraryAffairs, RequestStatus.PricingPendingPricingApprovalByLibraryAffairs, null, null));
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