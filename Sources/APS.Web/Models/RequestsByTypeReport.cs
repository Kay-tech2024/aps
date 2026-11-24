namespace APS.Web.Models
{
    public class RequestsByTypeReport
    {
        public string PublicationType { get; set; } // PublicationType (Primary key) (length: 12)
        public int? SubmittedRequests { get; set; } // SubmittedRequests
        public int? ApprovedRequests { get; set; } // ApprovedRequests
        public int? RejectedRequests { get; set; } // RejectedRequests
        public int? PrintedRequests { get; set; } // PrintedRequests
    }
}