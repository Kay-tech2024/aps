using APS.Data;
using APS.Web.Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class ScientificLetterInformationViewModel
    {
        [Required(ErrorMessageResourceName = "sci_letterinformation_title_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "sci_letterinformation_title", ResourceType = typeof(ApsStrings))]
        public string LetterTitle { get; set; }

        [Required(ErrorMessageResourceName = "degree_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "degree", ResourceType = typeof(ApsStrings))]
        public string Degree { get; set; }

        [Required(ErrorMessageResourceName = "supervisor_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "supervisor", ResourceType = typeof(ApsStrings))]
        public string Supervisor { get; set; }

        [Required(ErrorMessageResourceName = "discussion_committeemembers_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "discussion_committeemembers", ResourceType = typeof(ApsStrings))]
        public string CommitteeMember { get; set; }

        [Required(ErrorMessageResourceName = "scientific_department_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "scientific_department", ResourceType = typeof(ApsStrings))]
        public string ScientificDepartment { get; set; }

        [Required(ErrorMessageResourceName = "evaluation_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "evaluation", ResourceType = typeof(ApsStrings))]
        public string Evaluation { get; set; }

        [Required(ErrorMessageResourceName = "discussiondate_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "discussion_date", ResourceType = typeof(ApsStrings))]
        public DateTime? DateOfDiscussion { get; set; }

        [Required(ErrorMessageResourceName = "scientific_material_type_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "scientific_material_type", ResourceType = typeof(ApsStrings))]
        public ScientificMaterialType ScientificMaterialType { get; set; }

        [Required(ErrorMessageResourceName = "publisher_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string Publisher { get; set; }

        [Required(ErrorMessageResourceName = "publisher_place_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string PublisherPlace { get; set; }

        [Required(ErrorMessageResourceName = "publisher_date_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publisher_date", ResourceType = typeof(ApsStrings))]
        public DateTime DateOfPublish { get; set; }

        [Display(Name = "reason_for_reject", ResourceType = typeof(ApsStrings))]
        public string ReasonForRejection { get; set; }
    }
}