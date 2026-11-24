using APS.Data;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class VacationRequestViewModel : RequestViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "vacation_type")]
        public VacationType? VacationType { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "job_title")]
        public string JobTitle { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "rank")]
        public string Rank { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "vacation_in_days")]
        public uint? VacationInDays { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "vacation_start_date")]
        public string VacationStartDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "vacation_address")]
        public string VacationAddress { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "mobile")]
        public string MobileNumber { get; set; }
    }
}