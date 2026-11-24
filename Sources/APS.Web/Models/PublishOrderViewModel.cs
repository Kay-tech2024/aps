using APS.Data;
using APS.Web.App_Start.Code;
using APS.Web.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace APS.Web.Models
{
    public class PublishOrderViewModel
    {
        public long? RequestId { get; set; }

        public class NameComponents

        {
            public string FirstName { get; set; }

            public string SecondName { get; set; }

            public string ThirdName { get; set; }

            public string LastName { get; set; }
        }

        public IEnumerable<DropDown> FacultyOptions
        {
            get
            {
                return Lookups.FacultyOptions;
            }
        }

        public IEnumerable<DropDown> MajorOptions
        {
            get
            {
                return Lookups.MajorOptions;
            }
        }

        public IEnumerable<DropDown> ScientificMaterialTypeOptions
        {
            get
            {
                return Lookups.ScientificMaterialTypeOptions;
            }
        }

        [Required(ErrorMessageResourceName = "researcher_name_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "researcher_name", ResourceType = typeof(ApsStrings))]
        public string ResearcherName { get; set; }

        [Required(ErrorMessageResourceName = "faculty_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "faculity", ResourceType = typeof(ApsStrings))]
        public int? CollegeId { get; set; }

        [Display(Name = "request_create_department", ResourceType = typeof(ApsStrings))]
        public int? DepartmentId { get; set; }

        [Display(Name = "major", ResourceType = typeof(ApsStrings))]
        public int? MajorId { get; set; }

        [Display(Name = "major", ResourceType = typeof(ApsStrings))]
        public string MajorOther { get; set; }

        [Required(ErrorMessageResourceName = "scientific_material_type_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "scientific_material_type", ResourceType = typeof(ApsStrings))]
        public string ScientificMaterialTypeId { get; set; }

        [Required(ErrorMessageResourceName = "publish_type_required", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "publish_type", ResourceType = typeof(ApsStrings))]
        public PublishTimeType PublishType { get; set; }

        [Display(Name = "related_publish_request", ResourceType = typeof(ApsStrings))]
        public long? PublishRequestId { get; set; }

        public SelectList PreviousPublishRequests { get; set; }

        public bool IsArabic { get; set; }

        [Display(Name = "TranslatedToId", ResourceType = typeof(ApsStrings))]
        public long? TranslatedToId { get; set; }

        [Display(Name = "TranslatedTo", ResourceType = typeof(ApsStrings))]
        public string TranslatedTo { get; set; }
    }
}