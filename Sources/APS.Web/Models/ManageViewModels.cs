using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangeUserPasswordViewModel
    {
        [Editable(false)]
        public long? Id { get; set; }

        [Editable(false)]
        [Required]
        public string UserId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [StringLength(100, ErrorMessage = "{0} يجب أن يكون {2} حرفا على الأقل.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد Password")]
        [Compare("Password", ErrorMessage = "Password والتأكيد Password غير متطابقة.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; }

        public string RoleId { get; set; }
    }

    public class UsersInfoViewModel
    {
        public List<UserInfo> Users { get; set; }

        public List<RoleViewModel> AllRoles { get; set; }
    }

    public class UserInfo
    {
        public UserViewModel User { get; set; }

        public RoleViewModel Role { get; set; }
    }

    public class RoleViewModel
    {
        public string RoleId { get; set; }

        public string RoleName { get; set; }
    }
}