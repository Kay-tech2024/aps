using APS.Data;
using APS.Web.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class ParticipantDataViewModel
    {
        public long? RequestId { get; set; }

        public List<SingleParticipantDataViewModel> ParticipantDetail { get; set; }
        public RequestType Type { get; internal set; }
        public PublicationType PublicationType { get; internal set; }
    }

    public class SingleParticipantDataViewModel
    {
        [Required(ErrorMessageResourceName = "participant_name_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "participant_name", ResourceType = typeof(ApsStrings))]
        public string ParticipantName { get; set; }

        [Required(ErrorMessageResourceName = "email_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "book_type", ResourceType = typeof(ApsStrings))]
        public string ParticipantEmail { get; set; }

        [Required(ErrorMessageResourceName = "mobile_no_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "book_type", ResourceType = typeof(ApsStrings))]
        public string ParticipanMobileNumber { get; set; }
    }
}