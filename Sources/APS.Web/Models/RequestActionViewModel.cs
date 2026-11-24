using APS.Data;

namespace APS.Web.Models
{
    public class RequestActionViewModel
    {
        public RequestAction Action { get; set; }

        public string Display { get; set; }

        public string Value { get; set; }

        public bool IsReturn { get; set; }

        public string UserId { get; set; }
    }
}