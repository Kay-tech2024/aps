using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;

namespace APS.Core.Workflows
{
    public interface IRequestWorkflow
    {
        List<FieldModel> Fields { get; }

        List<RequestAction> GetAllowedActions(RequestListModel request);

        List<string> GetAllowedActions(RequestListModel request, Translator translator = null);

        List<Tuple<RequestAction, RequestStatus, string[], FileType[]>> GetAllowedActionsTransitionsWithFiles(RequestListModel request, params string[] parameters);

        List<Tuple<RequestAction, RequestStatus, string[]>> GetAllowedActionsTransitions(RequestListModel request);

        RequestStatus[] GetRequestStatus(RequestListModel request);

        string GetRequestStatus(RequestListModel request, Translator translator = null);

        List<RequestHistoryLog> GetRequestHistory(RequestListModel request, Translator translator = null);

        bool IsActionable(RequestListModel request, Translator translator = null);

        bool ValidateRequestModel(RequestModel model, out Dictionary<string, RequestErrorCodes> errors);

        bool SaveRequest(RequestModel model, out RequestListModel request, out Dictionary<string, RequestErrorCodes> errors);
    }
}