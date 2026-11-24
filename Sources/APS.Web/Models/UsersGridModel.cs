using APS.Data;
using System.Collections.Generic;

namespace APS.Web.Models
{
    public class UsersGridModel
    {
        public int Current { get; set; }
        public int RowCount { get; set; }
        public Dictionary<string, string> Sort { get; set; }
        public string SearchPhrase { get; set; }
        public bool Special { get; set; }
        public bool Incomplete { get; set; }
        public string Type { get; set; }
        public long? RequestId { get; set; }
        public bool ShowAll { get; set; }
        public ApsUserRole RequestRefereeRole { get; set; }
    }
}