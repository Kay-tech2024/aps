using APS.Core.Models;
using APS.Data;
using APS.Web.App_Start.Code;
using APS.Web.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class PublicationViewModel
    {
        public RequestType Type { get; set; }
        public PublicationType PublicationType { get; set; }

        public long? RequestId { get; set; }

        public IEnumerable<DropDown> FacultyOptions
        {
            get
            {
                return Lookups.FacultyOptions;
            }
        }

        public IEnumerable<DropDown> DepartmentOptions
        {
            get
            {
                return Lookups.DepartmentOptions;
            }
        }

        public IEnumerable<DropDown> SciDepartmentOptions
        {
            get
            {
                return Lookups.SciDepartmentOptions;
            }
        }

        [Display(Name = "college", ResourceType = typeof(ApsStrings))]
        public string CollegeOther { get; set; }

        [Display(Name = "university", ResourceType = typeof(ApsStrings))]
        public string UniversityOther { get; set; }

        [Display(Name = "thesis_title", ResourceType = typeof(ApsStrings))]
        public string ThesisTitle { get; set; }

        [Display(Name = "degree", ResourceType = typeof(ApsStrings))]
        public Degree? Degree { get; set; }

        [Display(Name = "supervisor", ResourceType = typeof(ApsStrings))]
        public string Supervisor { get; set; }

        [Display(Name = "assistantSupervisor", ResourceType = typeof(ApsStrings))]
        public string AsstSupervisor { get; set; }

        [Display(Name = "discussion_committeemembers", ResourceType = typeof(ApsStrings))]
        public List<string> CommitteeMembers { get; set; } = new List<string>();

        [Display(Name = "scientific_department", ResourceType = typeof(ApsStrings))]
        public int SciDepartmentId { get; set; }

        [Display(Name = "gpa", ResourceType = typeof(ApsStrings))]
        public string GPA { get; set; }

        [Display(Name = "discussion_date", ResourceType = typeof(ApsStrings))]
        public string DateOfDiscussion { get; set; }

        [Display(Name = "publish_type", ResourceType = typeof(ApsStrings))]
        public PublishType PublishType { get; set; }

        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string Publisher { get; set; }

        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string PublisherPlace { get; set; }

        [Display(Name = "publisher_date", ResourceType = typeof(ApsStrings))]
        public string DateOfPublish { get; set; }

        public List<AttachedFile> Files { get; internal set; }

        public List<string> FileNames { get; set; } = new List<string>();

        public List<int> PageCounts { get; set; } = new List<int>();

        [Display(Name = "book_type", ResourceType = typeof(ApsStrings))]
        public BookType? BookType { get; set; }

        [Display(Name = "book_lang", ResourceType = typeof(ApsStrings))]
        public BookLanguage? BookLanguage { get; set; }

        [Display(Name = "arabic", ResourceType = typeof(ApsStrings))]
        public bool isArabic { get; set; }

        public bool IsTranslated { get; set; }

        public string RequestType { get; set; }

        [Display(Name = "transulated_to", ResourceType = typeof(ApsStrings))]
        public string TranslatedTo { get; set; }

        [Display(Name = "transulated_to", ResourceType = typeof(ApsStrings))]
        public long? TranslatedToId { get; set; }

        [Display(Name = "book_title", ResourceType = typeof(ApsStrings))]
        public string BookTitle { get; set; }

        [Display(Name = "original_book_title", ResourceType = typeof(ApsStrings))]
        public string OriginalBookTitle { get; set; }

        [Display(Name = "author_major", ResourceType = typeof(ApsStrings))]
        public int? AuthorMajorId { get; set; }

        [Display(Name = "recommendation_number", ResourceType = typeof(ApsStrings))]
        public int? RecommmendationNumber { get; set; }

        [Display(Name = "recommendation_date", ResourceType = typeof(ApsStrings))]
        public string RecommmendationDate { get; set; }

        [Display(Name = "handbook_type", ResourceType = typeof(ApsStrings))]
        public HandBookType? HandBookType { get; set; }

        [Display(Name = "handbook_language", ResourceType = typeof(ApsStrings))]
        public HandBookLanguage? HandBookLanguage { get; set; }

        [Display(Name = "handBook_title", ResourceType = typeof(ApsStrings))]
        public string HandBookTitle { get; set; }

        [Display(Name = "original_handbook_title", ResourceType = typeof(ApsStrings))]
        public string OriginalHandBookTitle { get; set; }

        [Display(Name = "majorof_dictionary", ResourceType = typeof(ApsStrings))]
        public int MajorOfDictionaryID { get; set; }

        [Display(Name = "dictionary_language", ResourceType = typeof(ApsStrings))]
        public DictionaryLanguage DictionaryLanguage { get; set; }

        [Display(Name = "dictionary_title", ResourceType = typeof(ApsStrings))]
        public string DictionaryTitle { get; set; }

        [Display(Name = "dictionary_title_original", ResourceType = typeof(ApsStrings))]
        public string OriginalDictionaryTitle { get; set; }

        [Display(Name = "majorof_encyclopedia", ResourceType = typeof(ApsStrings))]
        public int MajorOfEncyclopediaId { get; set; }

        [Display(Name = "encyclopedia_language", ResourceType = typeof(ApsStrings))]
        public EncyclopediaLanguage EncyclopediaLanguage { get; set; }

        [Display(Name = "encyclopedia_title", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaTitle { get; set; }

        [Display(Name = "encyclopedia_original_title", ResourceType = typeof(ApsStrings))]
        public string OriginalEncyclopediaTitle { get; set; }

        [Display(Name = "probe_type", ResourceType = typeof(ApsStrings))]
        public ProbeType ProbeType { get; set; }

        [Display(Name = "author_name", ResourceType = typeof(ApsStrings))]
        public string AuthorName { get; set; }

        [Display(Name = "scope_manuscript", ResourceType = typeof(ApsStrings))]
        public string ScopeOfManusript { get; set; }

        [Display(Name = "prober_major", ResourceType = typeof(ApsStrings))]
        public int ProberMajorId { get; set; }

        public string Title { get; set; }
    }
}