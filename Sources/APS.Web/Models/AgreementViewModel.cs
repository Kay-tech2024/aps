using APS.Data;

namespace APS.Web.Models
{
    public class AgreementViewModel
    {
        public RequestType Type { get; set; }
        public PublicationType PublicationType { get; set; }
        public long? RequestId { get; set; }
        public bool? IsApproved { get; set; }

        public string AgreementText { get; set; }
    }
}