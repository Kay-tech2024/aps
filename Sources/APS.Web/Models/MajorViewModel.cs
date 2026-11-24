using APS.Web.App_Start.Code;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class MajorViewModel
    {
        [Editable(false)]
        public long? Id { get; set; }

        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.ApsStrings), ErrorMessageResourceName = "name_english_required")]
        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "name_english")]
        [Unique(TableName = "Majors")]
        public string NameEnglish { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "name_arabic")]
        [Unique(TableName = "Majors")]
        public string NameArabic { get; set; }
    }
}