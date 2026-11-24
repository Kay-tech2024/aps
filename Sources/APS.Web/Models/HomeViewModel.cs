using System.Collections.Generic;

namespace APS.Web.Models
{
    public class HomeViewModel
    {
        public List<RequestSummaryViewModel2> Requests { get; set; }
    }

    public class RequestSummaryViewModel
    {
        public long Id { get; set; }
        public string UniqueId { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Date { get; set; }
    }
}