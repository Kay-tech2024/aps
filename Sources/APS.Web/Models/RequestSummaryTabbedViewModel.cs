using System.Collections.Generic;

namespace APS.Web.Models
{
    public class RequestSummaryTabbedViewModel
    {
        public List<RequestSummaryTabViewModel> tabs { get; set; }

        public RequestSummaryTabbedViewModel()
        {
            tabs = new List<RequestSummaryTabViewModel>();
        }
    }

    public class RequestSummaryTabViewModel
    {
        public string TabName { get; set; }
        public string TabId { get; set; } = "0";
        public long RequestId { get; set; }
    }
}