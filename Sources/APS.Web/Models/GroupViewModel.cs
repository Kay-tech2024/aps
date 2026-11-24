using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class GroupViewModel
    {
        public long Id { get; set; }
        public int CollegeId { get; set; }
        public string Name { get; set; }
        public string CollegeName { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "name_english")]
        public string NameEnglish { get; set; }

        [Display(ResourceType = typeof(Resources.ApsStrings), Name = "name_arabic")]
        public string NameArabic { get; set; }

        public List<CollegeViewModel> Departments { get; set; } = new List<CollegeViewModel>();
    }
}