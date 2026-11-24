using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APS.Core.Workflows
{
    public class OperationCommandRequestWorkflow : IRequestWorkflow
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
            if (request == null || request.TypeEnum != RequestType.OperationCommand)
                return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            var list = new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();

            foreach (var st in GetRequestStatus(request))
            {
                switch (st)
                {
                    case RequestStatus.OperationCommandPendingOperationCommandInitiation:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.SubmitOperationCommandToViceDeanOfAcademicResearch, RequestStatus.OperationCommandPendingOperationCommandPreparation, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToProofreader, RequestStatus.OperationCommandPendingPendingCorrection, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingPendingCorrection:
                        if (Utils.HasRolesAny(ApsUserRole.Proofreader) && request.RequestUsers.Count(rr => rr.UserId == Utils.CurrentUser.Id && rr.Action == null) > 0)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Submit,
                                RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading,
                                null,
                                new FileType[] { FileType.CorrectedPublication }
                            ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.SubmitOperationCommandToViceDeanOfAcademicResearch, RequestStatus.OperationCommandPendingOperationCommandPreparation, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToResearcher, RequestStatus.OperationCommandPendingResearcherProofreadingAcceptance, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToProofreader, RequestStatus.OperationCommandPendingPendingCorrection, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingResearcherProofreadingAcceptance:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.PublicationRequest.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.Resubmit,
                                RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading, null, null
                            ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandPreparation:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ApproveAndForwardToDeanOfAcademicResearch,
                                RequestStatus.OperationCommandPendingOperationCommandApproval,
                                null,
                              null
                            ));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                              RequestAction.ReturnToPUAP,
                              RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading,
                              null,
                              null
                            ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandApproval:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveAndForwardToUniversityPressDirector, RequestStatus.OperationCommandPendingOperationCommandValidation, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.RejectAndReturnToOfficerOfPrintingUnit, RequestStatus.OperationCommandPendingOperationCommandInitiationAfterProofReading, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandValidation:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityPressDirector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.CertifyAndForwardToUniversityPressOfficer,
                                RequestStatus.OperationCommandPendingOperationCommandExecution,
                                null,
                                null
                            ));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToDeanshipOfAcademicResearch,
                                RequestStatus.OperationCommandPendingOperationCommandApproval,
                                null,
                                 null
                            ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandExecution:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityPressOfficer))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.OperationCommandExecutedAndForwardToPUAP,
                                RequestStatus.OperationCommandPendingOperationCommandForward,
                                null,
                                null
                            ));

                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.ReturnToUniversityPressDirector,
                                RequestStatus.OperationCommandPendingOperationCommandValidation,
                                null,
                                null
                            ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandExecution2:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityPressOfficer))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.OperationCommandExecutedAndForwardToPUAP,
                                RequestStatus.OperationCommandPendingOperationCommandForward2,
                                null,
                                null
                            ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandExecution3:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityPressOfficer))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(
                                RequestAction.OperationCommandExecutedAndForwardToPUAP,
                                RequestStatus.OperationCommandPendingOperationCommandForward3,
                                null,
                                null
                            ));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandForward:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressOfficer, RequestStatus.OperationCommandPendingOperationCommandExecution, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToResearcher, RequestStatus.OperationCommandPendingResearcherComments, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandForward1AndHalf:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToResearcher, RequestStatus.OperationCommandPendingResearcherComments, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressOfficer, RequestStatus.OperationCommandPendingOperationCommandExecution2, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandForward2:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToResearcher, RequestStatus.OperationCommandPendingResearcherComments2, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressOfficer, RequestStatus.OperationCommandPendingOperationCommandExecution2, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandForward3:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToResearcher, RequestStatus.OperationCommandPendingResearcherAcceptance, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressOfficer, RequestStatus.OperationCommandPendingOperationCommandExecution2, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingResearcherComments:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.PublicationRequest.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Accept, RequestStatus.OperationCommandPendingISBNRequestSubmission, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.AcceptWithComments, RequestStatus.OperationCommandPendingOperationCommandForward1AndHalf, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingResearcherComments2:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.PublicationRequest.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Accept, RequestStatus.OperationCommandPendingISBNRequestSubmission, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.AcceptWithComments, RequestStatus.OperationCommandPendingOperationCommandForward2AndHalf, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandForward2AndHalf:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToResearcher, RequestStatus.OperationCommandPendingResearcherComments2, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressOfficer, RequestStatus.OperationCommandPendingOperationCommandExecution3, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandForwardResearcherRejection:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToResearcher, RequestStatus.OperationCommandPendingResearcherAcceptance, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressOfficer, RequestStatus.OperationCommandPendingOperationCommandExecution, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingResearcherAcceptance:
                        if (Utils.HasRolesAny(ApsUserRole.Researcher) && request.PublicationRequest.CreatedBy == Utils.CurrentUser.Id)
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Approve, RequestStatus.OperationCommandPendingISBNRequestSubmission, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.Reject, RequestStatus.OperationCommandPendingOperationCommandResearcherRejected, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandResearcherRejected:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.CloseRequest, RequestStatus.Rejected, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingISBNRequestSubmission:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.SubmitISBNRequestToKFNL, RequestStatus.OperationCommandPendingISBN, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingISBN:
                        if (Utils.HasRolesAny(ApsUserRole.KingFahadNationalLibraryOfficer))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.SubmitISBNToImamUniversity, RequestStatus.OperationCommandAfterISBN, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandAfterISBN:
                        if (Utils.HasRolesAny(ApsUserRole.OfficerOfPrintingUnit))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ForwardToViceDeanOfAcademicResearch, RequestStatus.OperationCommandPendingOperationCommandFinalApprovalViceDean, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToKFNLOfficer, RequestStatus.OperationCommandPendingISBN, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandFinalApprovalViceDean:
                        if (Utils.HasRolesAny(ApsUserRole.ViceDeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveAndForwardToDeanOfAcademicResearch, RequestStatus.OperationCommandPendingOperationCommandFinalApproval, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.RejectAndReturnToPUAP, RequestStatus.OperationCommandAfterISBN, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingOperationCommandFinalApproval:
                        if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ApproveAndForwardToUniversityPressDirector, RequestStatus.OperationCommandPendingPrintingManager, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.RejectAndReturnToViceDeanOfAcademicResearch, RequestStatus.OperationCommandPendingOperationCommandFinalApprovalViceDean, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingPrintingManager:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityPressDirector))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.SubmitToPressOfficer, RequestStatus.OperationCommandPendingPrinting, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToDeanshipOfAcademicResearch, RequestStatus.OperationCommandPendingOperationCommandFinalApproval, null, null));
                        }
                        else
                        {
                            return new List<Tuple<RequestAction, RequestStatus, string[], FileType[]>>();
                        }

                        break;

                    case RequestStatus.OperationCommandPendingPrinting:
                        if (Utils.HasRolesAny(ApsUserRole.UniversityPressOfficer))
                        {
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.PrintingCompleted, RequestStatus.Completed, null, null));
                            list.Add(new Tuple<RequestAction, RequestStatus, string[], FileType[]>(RequestAction.ReturnToUniversityPressDirector, RequestStatus.OperationCommandPendingPrintingManager, null, null));
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