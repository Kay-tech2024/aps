namespace APS.Web.Models
{
    public class ExternalRefereeRequestReviewResponseViewModel
    {
        public long RequestId { get; set; }
        public int RefereeId { get; set; }
        public int RoleCode { get; set; }
        public string Email { get; set; }
        public string Action { get; set; }
        public string ParamsHash { get; set; }
    }
}