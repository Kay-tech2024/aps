using APS.Core.Models;
using APS.Data;
using System.Web.Mvc;

namespace APS.Web.Models
{
    public class BankViewModel
    {
        public string AccountType { get; set; }
        public int? BankId { get; set; }
        public string BankOther { get; set; }
        public string AccountOwnerName { get; set; }
        public string IbanNo { get; set; }
        public int? CountryId { get; set; }
        public long? RequestId { get; set; }
        public int? CityId { get; set; }

        public int? NoOfParticipant { get; set; }
        public int? IsScientificProduction { get; set; }
        public bool HasParticipant { get; set; }
        public AttachedFile FileUpload { get; set; }

        public PublicationType? PublicationType { get; set; }
        public RequestType Type { get; internal set; }

        public int? BankAccountTypeId { get; set; }

        public int? BankAccountNumberTypeId { get; set; }

        public string SwiftCode { get; set; }

        public string RoutingNumber { get; set; }
        public SelectListItem[] BankAccountNumberTypes { get; set; }
    }
}