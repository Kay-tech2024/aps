using APS.Data;
using APS.Web.App_Start.Code;
using APS.Web.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace APS.Web.Models
{
    public class RefereeRegisterViewModel
    {
        public IEnumerable<DropDown> CountryOptions
        {
            get
            {
                return Lookups.CountryOptions;
            }
        }

        public ApsUserRole? Role { get; set; }

        [Required(ErrorMessageResourceName = "national_id_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "national_id", ResourceType = typeof(ApsStrings))]
        public string NationalId { get; set; }

        [Required(ErrorMessageResourceName = "fullname_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "fullname", ResourceType = typeof(ApsStrings))]
        public string FullName { get; set; }

        [Required(ErrorMessageResourceName = "mobile_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "mobile", ResourceType = typeof(ApsStrings))]
        public string MobileNumber { get; set; }

        [Required(ErrorMessageResourceName = "university_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "university", ResourceType = typeof(ApsStrings))]
        public string UniversityOther { get; set; }

        [Required(ErrorMessageResourceName = "college_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "college", ResourceType = typeof(ApsStrings))]
        public string CollegeOther { get; set; }

        [Display(Name = "department", ResourceType = typeof(ApsStrings))]
        public string DepartmentOther { get; set; }

        [Required(ErrorMessageResourceName = "major_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "major", ResourceType = typeof(ApsStrings))]
        public string MajorOther { get; set; }

        //[Required(ErrorMessageResourceName = "birthdate_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "birthdate", ResourceType = typeof(ApsStrings))]
        public string BirthDate { get; set; }

        //[Required(ErrorMessageResourceName = "university_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "university", ResourceType = typeof(ApsStrings))]
        public int? UniversityId { get; set; }

        //[Required(ErrorMessageResourceName = "major_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "major", ResourceType = typeof(ApsStrings))]
        public int? MajorId { get; set; }

        //[Required(ErrorMessageResourceName = "nationality_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "nationality", ResourceType = typeof(ApsStrings))]
        public int? NationalityId { get; set; }

        //[Required(ErrorMessageResourceName = "country_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "country", ResourceType = typeof(ApsStrings))]
        public int? CountryId { get; set; }

        //[Required(ErrorMessageResourceName = "college_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "college", ResourceType = typeof(ApsStrings))]
        public int? CollegeId { get; set; }

        [Required(ErrorMessageResourceName = "address_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "address", ResourceType = typeof(ApsStrings))]
        public string Address { get; set; }

        [Required(ErrorMessageResourceName = "email_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "email", ResourceType = typeof(ApsStrings))]
        public string Email { get; set; }

        [Display(Name = "email_confirmation", ResourceType = typeof(ApsStrings))]
        [Compare("Email")]
        public string EmailConfirmation { get; set; }

        //[Display(Name = "password", ResourceType = typeof(ApsStrings))]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }
        [Required(ErrorMessageResourceName = "password_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        //[Display(Name = "New password")]
        [Display(Name = "password", ResourceType = typeof(ApsStrings))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "password_confirmation", ResourceType = typeof(ApsStrings))]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        //[DataType(DataType.Password)]
        //[Display(Name = "Password confirmation")]
        //[Compare("Password", ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "password_confirmation_mismatch")]
        // public string ConfirmPassword { get; set; }

        [Display(Name = "bank_name", ResourceType = typeof(ApsStrings))]
        public string BankName { get; set; }

        [Display(Name = "bank_branch", ResourceType = typeof(ApsStrings))]
        public string BankBranch { get; set; }

        [Display(Name = "account_holder_name", ResourceType = typeof(ApsStrings))]
        public string BankAccountHolderName { get; set; }

        [Display(Name = "iban", ResourceType = typeof(ApsStrings))]
        public string BankAccountIBAN { get; set; }

        [Display(Name = "resume", ResourceType = typeof(ApsStrings))]
        public HttpPostedFileBase File { get; set; }

        public string SmsKey { get; set; }

        [Display(Name = "sms_code", ResourceType = typeof(ApsStrings))]
        public string SmsCode { get; set; }

        [Required]
        [Display(Name = "Activate user immediately")]
        public bool IsActivated { get; set; } = true;

        [Required]
        [Display(Name = "Force password change")]
        public bool ForcePasswordChange { get; set; } = true;

        [Unique(TableName = "Users")]
        [Required(ErrorMessageResourceName = "username_required", ErrorMessageResourceType = typeof(Resources.ApsStrings))]
        [Display(Name = "Username")]
        public string UserName { get; set; }
    }
}