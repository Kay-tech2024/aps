using APS.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class AuthorData
    {
        //[Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "author_name_requerd")]
        public string AuthorName { get; set; }

        //[Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "employer_required")]
        public string Employer { get; set; }

        public int? BankAccountType { get; set; }
        public bool IsPartOfScienceProduct { get; set; }

        //[Required(ErrorMessageResourceType =typeof(ApsStrings),ErrorMessageResourceName ="City is Requerd")]
        public string City { get; set; }

        //[Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "Country is Requerd")]
        public string Country { get; set; }

        public string FilePath { get; set; }
        public string IBAN { get; set; }
        public bool IsAgree { get; set; }
        public bool IsNotAgree { get; set; }

        public string PartnerEmail { get; set; }

        public string PartnerMobile { get; set; }

        public string PartnerName { get; set; }

        [Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "city")]
        public string ParticipantName { get; set; }

        //[Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "Participant Email Required")]
        public string ParticipantEmail { get; set; }

        //[Required(ErrorMessageResourceType = typeof(ApsStrings), ErrorMessageResourceName = "MobileNumber Required")]
        public string ParticipanMobileNumber { get; set; }
    }
}