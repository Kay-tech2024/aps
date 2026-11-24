using APS.Data;
using System;
using System.Collections.Generic;

namespace APS.Core.Models
{
    public class RequestListModel
    {
        public long Id { get; set; }
        public long? PublicationRequestId { get; set; }
        public DateTime PublicationRequestUpdated { get; set; }
        public string UniqueId { get; set; }
        public string Title { get; set; }
        public string DepartmentName { get; set; }
        public string CollegeName { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public RequestType TypeEnum { get; set; }
        public string DateCreated { get; set; }
        public string DateCreatedActual { get; set; }
        public string DateUpdated { get; set; }
        public string DateUpdatedActual { get; set; }
        public DateTime DateCreatedActualGreg { get; set; }
        public DateTime DateUpdatedActualGreg { get; set; }
        public string Status { get; set; }
        public RequestStatus StatusEnum { get; set; }
        public bool Actionable { get; set; }
        public DateTime Updated { get; set; }
        public int Index { get; set; }

        public string CreatedBy { get; set; }
        public int? NoOfParticipant { get; set; }
        public List<RequestCoresearcherListModel> RequestCoresearchers { get; set; }
        public int? CollegeId { get; set; }
        public List<RequestRefereeListModel> RequestReferees { get; set; }
        public List<RequestUserListModel> RequestUsers { get; set; }
        public PublicationRequestListModel PublicationRequest { get; set; }
    }

    public class RequestCoresearcherListModel
    {
        public bool? DidResearcherAgree { get; set; }
        public long? ResearcherId { get; set; }
    }

    public class RequestRefereeListModel
    {
        public string RefereeUserId { get; set; }
        public string CreatedByUserRole { get; set; }
        public bool IsDeleted { get; set; }
        public bool? DidRefereeAgree { get; set; }
        public byte? Action { get; set; }
        public int RoundNo { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public DateTime? RequestReviewSentOn { get; set; }
    }

    public class RequestUserListModel
    {
        public string UserId { get; set; }
        public List<string> UserRoles { get; set; }
        public byte? Action { get; set; }
    }

    public class PublicationRequestListModel
    {
        public string CreatedBy { get; set; }
    }
}