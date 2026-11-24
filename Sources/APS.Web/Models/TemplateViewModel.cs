using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace APS.Web.Models
{
    public class TemplateViewModel
    {
        [Required]
        public string TemplateKey { get; set; }

        public string SubjectEnglish { get; set; }

        [AllowHtml]
        public string BodyEnglish { get; set; }

        public string SubjectArabic { get; set; }

        [AllowHtml]
        public string BodyArabic { get; set; }

        public bool IsHtml { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}