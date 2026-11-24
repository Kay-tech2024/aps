using APS.Web.Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class registrationtestModel
    {
        [Required(ErrorMessageResourceName = "fullname_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "fullname", ResourceType = typeof(ApsStrings))]
        public string FullName { get; set; }

        [Required(ErrorMessageResourceName = "mobile_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "mobile", ResourceType = typeof(ApsStrings))]
        public string MobileNumber { get; set; }

        //[Required(ErrorMessageResourceName = "birthdate_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "birthdate", ResourceType = typeof(ApsStrings))]
        public DateTime? BirthDate { get; set; }

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

        [Required(ErrorMessageResourceName = "password_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "password", ResourceType = typeof(ApsStrings))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "password_confirmation_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [DataType(DataType.Password)]
        [Display(Name = "password_confirmation", ResourceType = typeof(ApsStrings))]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}