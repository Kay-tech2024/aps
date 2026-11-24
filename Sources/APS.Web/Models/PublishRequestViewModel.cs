using APS.Data;
using APS.Web.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class PublishRequestViewModel : RequestViewModel
    {
        public PublicationType PubType { get; set; }

        public AuthorViewModel AuthorDetails { get; set; }

        public List<RefereeViewModel> Referees { get; set; }
        public List<RefereeViewModel> CouncilMembers { get; set; }

        public List<RequestActionViewModel> AllowedActions { get; set; }

        public int? PageCount { get; set; }

        public double? Price { get; set; }

        public bool CanSelectReferees { get; set; }
        public bool CanViewReferees { get; set; }
        public bool CanSeeCouncilMembers { get; set; }

        public int MinRefereeCount { get; set; }
        public int MaxRefereeCount { get; set; }

        [Required(ErrorMessageResourceName = "isbn_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "isbn", ResourceType = typeof(ApsStrings))]
        public string ISBN { get; set; }

        public List<DistributionListModel> DistributionList { get; set; } = new List<DistributionListModel>();

        public long? NoOfCopies { get; set; }
    }
}