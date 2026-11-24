using APS.Data;
using APS.Web.Resources;
using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace APS.Web.Models
{
    public class ProbeDetails
    {
        [Required(ErrorMessageResourceName = "probe_type_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "probe_type", ResourceType = typeof(ApsStrings))]
        public ProbeType ProbeType { get; set; }

        [Required(ErrorMessageResourceName = "author_name_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "author_name", ResourceType = typeof(ApsStrings))]
        public string AuthorName { get; set; }

        [Required(ErrorMessageResourceName = "book_type_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "book_type", ResourceType = typeof(ApsStrings))]
        public string BookTitle { get; set; }

        [Required(ErrorMessageResourceName = "scope_manuscript_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "scope_manuscript", ResourceType = typeof(ApsStrings))]
        public string ScopeOfManusript { get; set; }

        [Required(ErrorMessageResourceName = "prober_major_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "prober_major", ResourceType = typeof(ApsStrings))]
        public string ProberMajor { get; set; }

        [Required(ErrorMessageResourceName = "book_attachment_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "book_attachment", ResourceType = typeof(ApsStrings))]
        public HttpPostedFileBase Attachment { get; set; }

        [Required(ErrorMessageResourceName = "publish_type_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publish_type", ResourceType = typeof(ApsStrings))]
        public PublishType PublishType { get; set; }

        [Required(ErrorMessageResourceName = "publisher_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string Publisher { get; set; }

        [Required(ErrorMessageResourceName = "publisher_place_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string PublisherPlace { get; set; }

        [Required(ErrorMessageResourceName = "publisher_date_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publisher_date", ResourceType = typeof(ApsStrings))]
        public DateTime DateOfPublish { get; set; }
    }
}