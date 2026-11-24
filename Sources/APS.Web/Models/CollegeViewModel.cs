using APS.Core.Models;
using APS.Web.App_Start.Code;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class CollegeViewModel
    {
        [Editable(false)]
        public long? Id { get; set; }

        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "name_english_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "name_english")]
        [Unique(TableName = "Colleges")]
        public string NameEnglish { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "name_arabic")]
        [Unique(TableName = "Colleges")]
        public string NameArabic { get; set; }

        public List<AttachedFile> Files { get; set; }
    }
}