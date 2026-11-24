using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class DelegationViewModel
    {
        public DelegationViewModel()
        {
            Delegations = new List<DelegationDetailsViewModel>();
        }

        public long Id { get; set; }
        public string DelegatorId { get; set; }
        public string DelegateeId { get; set; }

        [Required(ErrorMessageResourceName = "email_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [Display(Name = "email", ResourceType = typeof(Resources.ApsStrings))]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "effective_from_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [Display(Name = "effective_from", ResourceType = typeof(Resources.ApsStrings))]
        public string EffectiveFrom { get; set; }

        [Display(Name = "effective_to", ResourceType = typeof(Resources.ApsStrings))]
        public string EffectiveTo { get; set; }

        public bool IsAlways { get; set; }

        public List<DelegationDetailsViewModel> Delegations { get; set; }
    }

    public class DelegationDetailsViewModel
    {
        public string DelegateeName { get; set; }
        public string DelegateeEmail { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}