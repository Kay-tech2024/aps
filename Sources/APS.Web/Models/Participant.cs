using APS.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class Participant
    {
        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "Participant Name Required")]
        public string ParticipantName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "Participant Email Required")]
        public string ParticipantEmail { get; set; }

        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "MobileNumber Required")]
        public string ParticipanMobileNumber { get; set; }
    }
}