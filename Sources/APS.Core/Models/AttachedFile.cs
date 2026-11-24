using APS.Data;
using System;
using System.Web;

namespace APS.Core.Models
{
    public class AttachedFile
    {
        public string FileId { get; set; }

        public FileType Type { get; set; }
        public bool IsDownloaded { get; set; }

        public string FileName { get; set; }

        public string Description { get; set; }

        public int? PageCount { get; set; }

        public DateTime Date { get; set; }

        public ApsUserRole CreatedUserRole { get; set; }

        public string CreatedUserName { get; set; }

        public HttpPostedFileBase Content { get; set; }
    }
}