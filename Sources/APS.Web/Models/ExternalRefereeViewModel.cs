using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class ExternalRefereeViewModel
    {
        [Required(ErrorMessageResourceName = "email_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [EmailAddress(ErrorMessageResourceName = "email_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "name_required")]
        public string FullName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "mobile_required")]
        public string MobileNumber { get; set; }
    }
}