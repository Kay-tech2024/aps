using APS.Data;
using System;

namespace APS.Web.Models
{
    public class CommentViewModel
    {
        public bool IsAction { get; set; }
        public string Action { get; set; }
        public string Comment { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public RequestType RequestType { set; get; }
        public DateTime Date { get; set; }
        public string DateStr { get; set; }

        public bool Hide { get; set; } = false;
    }
}