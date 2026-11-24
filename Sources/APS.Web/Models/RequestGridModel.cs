using APS.Data;
using System.Collections.Generic;

namespace APS.Web.Models
{
    public class RequestGridModel
    {
        public int Current { get; set; }
        public int RowCount { get; set; }
        public Dictionary<string, string> Sort { get; set; }
        public string SearchPhrase { get; set; }
        public RequestTab? Status { get; set; }

        public long? p { get; set; }

        public long? j { get; set; }

        public string u { get; set; }
    }
}