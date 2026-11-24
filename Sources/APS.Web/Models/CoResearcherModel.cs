using APS.Web.Resources;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class CoResearcherEncyclopediaModel
    {
        [Required(ErrorMessageResourceName = "encyclopedia_major", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "encyclopedia_major", ResourceType = typeof(ApsStrings))]
        public int MajorId { get; set; }

        [Required(ErrorMessageResourceName = "encyclopedia_lang", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "encyclopedia_lang", ResourceType = typeof(ApsStrings))]
        public int languageId { get; set; }

        [Required(ErrorMessageResourceName = "encyclopedia_title", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "encyclopedia_title", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaTitle { get; set; }

        [Required(ErrorMessageResourceName = "encyclopedia_original_title", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "encyclopedia_original_title", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaOriginalTitle { get; set; }

        [Required(ErrorMessageResourceName = "author_major", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "author_Major", ResourceType = typeof(ApsStrings))]
        public int MajorAuthorId { get; set; }

        [Required(ErrorMessageResourceName = "recommendation", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "recommendation", ResourceType = typeof(ApsStrings))]
        public string Recommendation { get; set; }

        public bool IsGovernmental { get; set; }

        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string PublisherName { get; set; }

        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string PublisherPlace { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "date", ResourceType = typeof(ApsStrings))]
        public string date { get; set; }

        [Display(Name = "reason_for_reject", ResourceType = typeof(ApsStrings))]
        public string ReasonOfReject { get; set; }

        public bool ISApproved { get; set; }
        public string SelectedAction { get; set; }
    }

    public class CoResearcherDictionaryModel
    {
        public int RequestId { get; set; }

        [Required(ErrorMessageResourceName = "majorof_dictionary", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "majorof_dictionary", ResourceType = typeof(ApsStrings))]
        public int MajorId { get; set; }

        [Required(ErrorMessageResourceName = "dictionary_language", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "dictionary_language", ResourceType = typeof(ApsStrings))]
        public int languageId { get; set; }

        [Required(ErrorMessageResourceName = "dictionary_title", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "dictionary_title", ResourceType = typeof(ApsStrings))]
        public string DictionaryTitle { get; set; }

        [Required(ErrorMessageResourceName = "dictionary_title_original", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "dictionary_title_original", ResourceType = typeof(ApsStrings))]
        public string OriginalDictionaryTitle { get; set; }

        [Required(ErrorMessageResourceName = "author_major", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "author_Major", ResourceType = typeof(ApsStrings))]
        public int MajorAuthorId { get; set; }

        [Required(ErrorMessageResourceName = "recommendation_number", ErrorMessageResourceType = typeof(ApsStrings))]
        [Display(Name = "recommendation_number", ResourceType = typeof(ApsStrings))]
        public long NumberOfRecommendations { get; set; }

        public bool IsGovernmental { get; set; }

        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string PublisherName { get; set; }

        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string PublisherPlace { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "date", ResourceType = typeof(ApsStrings))]
        public string date { get; set; }

        [Display(Name = "reason_for_reject", ResourceType = typeof(ApsStrings))]
        public string ReasonOfReject { get; set; }

        public bool ISApproved { get; set; }

        public string SelectedAction { get; set; }
    }
}