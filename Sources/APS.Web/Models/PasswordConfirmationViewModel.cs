using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class PasswordConfirmationViewModel
    {
        [Required(ErrorMessage = " كلمة السر القديمة Requiredة")]
        [StringLength(100, ErrorMessage = "{0} يجب أن يكون {2} حرفا على الأقل.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر القديمة")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = " كلمة السر Requiredة")]
        [StringLength(100, ErrorMessage = "{0} يجب أن يكون {2} حرفا على الأقل.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة السر")]
        [Compare("Password", ErrorMessage = "Password والتأكيد Password غير متطابقة.")]
        public string ConfirmPassword { get; set; }
    }

    public class DeleteConfirmation
    {
        public string Id { get; set; }

        [Required(ErrorMessage = " كلمة السر Requiredة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة السر")]
        public string Password { get; set; }
    }
}