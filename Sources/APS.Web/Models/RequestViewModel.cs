using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace APS.Web.Models
{
    public class RequestViewModel
    {
        public long? Id { get; set; }
        public Request Request { get; set; }

        public string UniqueId { get; set; }

        public RequestType RequestType { get; set; }

        public string Type { get; set; }
        public string Title { get; set; }

        public bool AllowEdit { get; set; }
        public string Status { get; set; }
        public long StatusNO { get; set; }
        public string Date { get; set; }

        public string CreatedByUserId { get; set; }

        public string CurrentlyWithUserId { get; set; }
        public string CurrentlyWithUserName { get; set; }

        [Editable(false)]
        public string Name { get; set; }

        public string Comments { get; set; }

        public List<CommentViewModel> CommentThread { get; set; } = new List<CommentViewModel>();

        public List<AttachedFile> Files { get; set; } = new List<AttachedFile>();

        public List<RequestHistoryModel> History { get; set; }

        public SelectListItem[] ReturnableTo { get; set; }

        public RequestAction Action { get; set; }
        public string AssignToUserId { get; set; }

        public int Grade { get; set; }

        public List<DistributionEntityViewModel> DistributionEntities { get; set; } = new List<DistributionEntityViewModel>();

        public bool IsRefereeReject { get; set; }
    }

    public class RefereeAgreementViewModel
    {
        public string Id { get; set; }

        [Required]
        public string IsRefereeAgree { get; set; }

        public string AgreementText { get; set; }
        public string ConditionsFile { get; set; }
        public string TemplateId { get; set; }
    }
}