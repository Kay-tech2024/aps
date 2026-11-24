using APS.Core.Models;
using APS.Data;
using APS.Web.App_Start.Code;
using APS.Web.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace APS.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessageResourceName = "email_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [Display(Name = "email", ResourceType = typeof(Resources.ApsStrings))]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedDepartment { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Departments { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        public string Department { get; set; }
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "email_required")]
        [EmailAddress]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "username_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "username")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "password_required")]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "keep_me_signed_in")]
        public bool RememberMe { get; set; } = false;
    }

    public class ResetPasswordViewModel
    {
        [EmailAddress]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "email")]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "mobile")]
        public string MobileNumber { get; set; }

        public string SmsKey { get; set; }

        [Display(Name = "sms_code", ResourceType = typeof(ApsStrings))]
        public string SmsCode { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "password")]
        public string Password { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "password_confirmation")]
        public string ConfirmPassword { get; set; }

        public string Key { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "email_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "email")]
        public string Email { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "name_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "fullname")]
        public string FullName { get; set; }

        [EmailAddress]
        [Unique(TableName = "Users")]
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "email_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "email")]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "mobile")]
        public string MobileNo { get; set; }

        [Unique(TableName = "Users")]
        [Required(ErrorMessageResourceName = "username_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "role_required")]
        [Display(Name = "Role")]
        public string RoleId { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "college")]
        public int? CollegeId { get; set; }

        public int? UniversityId { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "college")]
        public string CollegeName { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "major")]
        public int? MajorId { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "major")]
        public string MajorName { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "national_id")]
        public string NationalId { get; set; }

        [Display(Name = "Role")]
        public string RoleName { get; set; }

        [Required(ErrorMessageResourceName = "password_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password confirmation")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "password_confirmation_mismatch")]
        public string ConfirmPassword { get; set; }

        public string UserRoleId { get; set; }

        [Required]
        [Display(Name = "Activate user immediately")]
        public bool IsActivated { get; set; } = true;

        [Required]
        [Display(Name = "Force password change")]
        public bool ForcePasswordChange { get; set; } = true;

        [Display(Name = "Send credentials to email")]
        public bool SendCredentialsViaEmail { get; set; } = true;

        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();

        public List<CollegeViewModel> Colleges { get; set; } = new List<CollegeViewModel>();

        public List<MajorViewModel> Majors { get; set; } = new List<MajorViewModel>();
    }

    public class MemberCreateViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "name_required")]
        [LocalizedDisplay("fullname")]
        public string FullName { get; set; }

        [EmailAddress]
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "email_required")]
        [LocalizedDisplay("email")]
        public string Email { get; set; }

        [LocalizedDisplay("password")]
        public string Password { get; set; }

        [LocalizedDisplay("mobile")]
        public string MobileNo { get; set; }

        [Unique(TableName = "Users")]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "role_required")]
        [LocalizedDisplay("Role")]
        public string RoleId { get; set; }

        [LocalizedDisplay("college")]
        public int? CollegeId { get; set; }

        public int? UniversityId { get; set; }

        [LocalizedDisplay("college")]
        public string CollegeName { get; set; }

        [LocalizedDisplay("major")]
        public int? MajorId { get; set; }

        [LocalizedDisplay("major")]
        public string MajorName { get; set; }

        [LocalizedDisplay("national_id")]
        public string NationalId { get; set; }

        [LocalizedDisplay("Role")]
        public string RoleName { get; set; }

        public string UserRoleId { get; set; }

        [Required]
        [LocalizedDisplay("activate_immediately")]
        public bool IsActivated { get; set; } = true;

        [Required]
        [LocalizedDisplay("Force password change")]
        public bool ForcePasswordChange { get; set; } = true;

        [LocalizedDisplay("Send credentials to email")]
        public bool SendCredentialsViaEmail { get; set; } = true;

        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();

        public List<CollegeViewModel> Colleges { get; set; } = new List<CollegeViewModel>();

        public List<MajorViewModel> Majors { get; set; } = new List<MajorViewModel>();

        [LocalizedDisplay("nationality")]
        public int? NationalityId { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Editable(false)]
        public string UserId { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(Name = "name", ResourceType = typeof(Resources.ApsStrings))]
        public string FullName { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "national_id")]
        public string NationalId { get; set; }

        //[Unique(TableName = "Users")]
        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "email_required")]
        [EmailAddress]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "email")]
        public string Email { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "mobile")]
        public string MobileNo { get; set; }

        //[Unique(TableName = "Users")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "username")]
        public string UserName { get; set; }

        //    [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "field_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "role")]
        public string RoleId { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "college")]
        public int? CollegeId { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "college")]
        public string CollegeName { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "major")]
        public int? MajorId { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "major")]
        public string MajorName { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "role")]
        public string RoleName { get; set; }

        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "password")]
        public string CurrentPassword { get; set; }

        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        //[Display(Name = "Password confirmation")]
        [LocalizedDisplay("password_confirmation")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "password_confirmation_mismatch")]
        public string ConfirmPassword { get; set; }

        public string UserRoleId { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "activation")]
        public bool IsActivated { get; set; }

        [Required]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "force_password_change")]
        public bool ForcePasswordChange { get; set; }

        [Required]
        [LocalizedDisplay("PhoneNumberConfirmed")]
        public bool PhoneNumberConfirmed { get; set; }

        [Required]
        [LocalizedDisplay("EmailConfirmed")]
        public bool EmailConfirmed { get; set; }

        //[Required]
        //[LocalizedDisplay("force_password_change")]
        //public bool ForcePasswordChange { get; set; }

        [LocalizedDisplay("nationality")]
        public int? NationalityId { get; set; }

        [LocalizedDisplay("birthdate")]
        public string BirthDate { get; set; }

        [LocalizedDisplay("CountryId")]
        public int? CountryId { get; set; }

        [LocalizedDisplay("CityId")]
        public int? CityId { get; set; }

        [LocalizedDisplay("address")]
        public string Address { get; set; }

        [LocalizedDisplay("Qualification")]
        public QualificationType? Qualification { get; set; }

        [LocalizedDisplay("researcher_type")]
        public ResearcherRole? RoleInProject { get; set; }

        [LocalizedDisplay("university")]
        public int? UniversityId { get; set; }

        [LocalizedDisplay("university")]
        public string UniversityName { get; set; }

        [LocalizedDisplay("university")]
        public string UniversityOther { get; set; }

        [LocalizedDisplay("college")]
        public string CollegeOther { get; set; }

        [LocalizedDisplay("major")]
        public string MajorOther { get; set; }

        [LocalizedDisplay("department")]
        public string DepartmentOther { get; set; }

        [LocalizedDisplay("department")]
        public int? DepartmentID { get; set; }

        [LocalizedDisplay("Specialization")]
        public string Specialization { get; set; }

        [LocalizedDisplay("Bank")]
        public int? Bank { get; set; }

        [LocalizedDisplay("BankIbanPhotocopy")]
        public HttpPostedFileBase BankIbanPhotocopy { get; set; }

        public AttachedFile BankIbanPhotocopyReadonly { get; set; }

        [LocalizedDisplay("bank_other")]
        public string BankOther { get; set; }

        [LocalizedDisplay("bank_name")]
        public string BankName { get; set; }

        [LocalizedDisplay("bank_branch")]
        public string BankBranch { get; set; }

        [LocalizedDisplay("account_holder_name")]
        public string BankAccountHolderName { get; set; }

        [LocalizedDisplay("BankAccountIBAN")]
        public string BankAccountIBAN { get; set; }

        [LocalizedDisplay("BankAccountNumberTypeId")]
        public int? BankAccountNumberTypeId { get; set; }

        [LocalizedDisplay("BankAccountTypeId")]
        public int? BankAccountTypeId { get; set; }

        [LocalizedDisplay("SwiftCode")]
        public string SwiftCode { get; set; }

        [LocalizedDisplay("RoutingNumber")]
        public string RoutingNumber { get; set; }

        public List<RoleViewModel> Roles { get; set; } = new List<RoleViewModel>();

        public List<CollegeViewModel> Colleges { get; set; } = new List<CollegeViewModel>();

        public List<MajorViewModel> Majors { get; set; } = new List<MajorViewModel>();

        public List<ExtraRoleViewModel> ExtraRoles { get; set; } = new List<ExtraRoleViewModel>();

        public int RoleGroupCode { get; set; }

        // Lookups
        public List<CityViewModel> Cities { get; set; }

        public SelectListItem[] Universities { get; set; }
        public SelectListItem[] Departments { get; set; }
        public SelectListItem[] Banks { get; set; }
        public SelectListItem[] QualificationTypes { get; set; }
        public SelectListItem[] Countries { get; set; }

        public SelectListItem[] BankAccountNumberTypes { get; set; }
    }

    public class ExtraRoleViewModel
    {
        public long? Id { get; set; }
        public string RoleId { get; set; }
        public ApsUserRole Role { get; set; }
        public int? CollegeId { get; set; }
        public string EffectiveFrom { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
        public string EffectiveTo { get; set; }
        public DateTime? EffectiveToDate { get; set; }
        public bool IsActive { get; set; } = false;
    }

    public class UserPreferenceModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ContactUsViewModel
    {
        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "name_required")]
        [Display(ResourceType = typeof(ApsStrings), Name = "fullname")]
        public string FullName { get; set; }

        [EmailAddress]
        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "email_required")]
        [Display(ResourceType = typeof(ApsStrings), Name = "email")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "email_required")]
        [Display(ResourceType = typeof(ApsStrings), Name = "subject")]
        public string Subject { get; set; }

        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "contactUsMessage_required")]
        [Display(ResourceType = typeof(ApsStrings), Name = "contactUsMessage")]
        public string Message { get; set; }
    }
}