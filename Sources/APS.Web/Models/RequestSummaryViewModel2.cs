using APS.Core.Models;
using APS.Web.Resources;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace APS.Web.Models
{
    public class RequestSummaryViewModel2
    {
        [Display(Name = "request_id", ResourceType = typeof(ApsStrings))]
        public string RequestsId { get; set; }

        [Display(Name = "RequestsIsScientificProduction", ResourceType = typeof(ApsStrings))]
        public string RequestsIsScientificProduction { get; set; }

        [Display(Name = "number_of_participants", ResourceType = typeof(ApsStrings))]
        public string RequestsNoOfParticipant { get; set; }

        [Display(Name = "publish_type", ResourceType = typeof(ApsStrings))]
        public string RequestsPublicationType { get; set; }

        [Display(Name = "title", ResourceType = typeof(ApsStrings))]
        public string RequestsTitle { get; set; }

        [Display(Name = "request_type", ResourceType = typeof(ApsStrings))]
        public string RequestsType { get; set; }

        [Display(Name = "Requests_CollegeId", ResourceType = typeof(ApsStrings))]
        public string RequestsCollegeId { get; set; }

        [Display(Name = "request_create_department", ResourceType = typeof(ApsStrings))]
        public string RequestDepartmentId { get; set; }

        [Display(Name = "researcher_name", ResourceType = typeof(ApsStrings))]
        public string ResearchersFirstName { get; set; }

        [Display(Name = "email", ResourceType = typeof(ApsStrings))]
        public string ResearchersEmail { get; set; }

        [Display(Name = "mobile", ResourceType = typeof(ApsStrings))]
        public string ResearchersMobileNo { get; set; }

        [Display(Name = "birthdate", ResourceType = typeof(ApsStrings))]
        public string ResearchersDateOfBirth { get; set; }

        [Display(Name = "mobile", ResourceType = typeof(ApsStrings))]
        public string UserProfilesMobileNo { get; set; }

        [Display(Name = "gender", ResourceType = typeof(ApsStrings))]
        public string UserProfilesGender { get; set; }

        [Display(Name = "national_id", ResourceType = typeof(ApsStrings))]
        public string UserProfilesNationalId { get; set; }

        [Display(Name = "Bank_Id", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankId { get; set; }

        [Display(Name = "bank_account_type", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankAccountType { get; set; }

        [Display(Name = "bank_account_number_type_id", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankAccountNumberType { get; set; }

        [Display(Name = "bank_account_number_type_id", ResourceType = typeof(ApsStrings))]
        public int? UserProfilesBankAccountNumberTypeId { get; set; }

        [Display(Name = "bank_name", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankName { get; set; }

        [Display(Name = "bank_branch", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankBranch { get; set; }

        [Display(Name = "account_holder_name_inter", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankAccountHolder { get; set; }

        [Display(Name = "iban", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankIban { get; set; }

        //[Display(Name = "bank_account_type_id", ResourceType = typeof(ApsStrings))]
        //public int? UserProfilesBankAccountIdType { get; set; }

        [Display(Name = "swift_code", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankSwift { get; set; }

        [Display(Name = "routing_number", ResourceType = typeof(ApsStrings))]
        public string UserProfilesBankRouting { get; set; }

        [Display(Name = "book_type", ResourceType = typeof(ApsStrings))]
        public string BookRequestsBookType { get; set; }

        [Display(Name = "is_language_arabic", ResourceType = typeof(ApsStrings))]
        public string BookRequestsIsArabic { get; set; }

        [Display(Name = "is_translated", ResourceType = typeof(ApsStrings))]
        public string BookRequestsIsTranslated { get; set; }

        [Display(Name = "translated_to", ResourceType = typeof(ApsStrings))]
        public string BookRequestsTranslatedTo { get; set; }

        [Display(Name = "title_of_the_book", ResourceType = typeof(ApsStrings))]
        public string BookRequestsBookTitle { get; set; }

        [Display(Name = "title_origianal_book", ResourceType = typeof(ApsStrings))]
        public string BookRequestsOriginalTitleBook { get; set; }

        [Display(Name = "major", ResourceType = typeof(ApsStrings))]
        public string BookRequestsMajorId { get; set; }

        [Display(Name = "Recommendations", ResourceType = typeof(ApsStrings))]
        public string BookRequestsNoOfRecommendations { get; set; }

        [Display(Name = "Recommendations_date", ResourceType = typeof(ApsStrings))]
        public string BookRequestsRecommendationDate { get; set; }

        [Display(Name = "Recommendations_type", ResourceType = typeof(ApsStrings))]
        public string BookRequestsPublishType { get; set; }

        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string BookRequestsPublisher { get; set; }

        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string BookRequestsPublisherPlace { get; set; }

        [Display(Name = "publisher_date", ResourceType = typeof(ApsStrings))]
        public string BookRequestsPublishingDate { get; set; }

        [Display(Name = "ReviewType", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsReviewType { get; set; }

        [Display(Name = "author_name", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsAuthersName { get; set; }

        [Display(Name = "title_of_the_book", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsBookTitle { get; set; }

        [Display(Name = "scope_manuscript_required", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsScopOfManuscript { get; set; }

        [Display(Name = "prober_major", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsReviewMajor { get; set; }

        [Display(Name = "Publish_Type", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsPublishType { get; set; }

        [Display(Name = "Review_Requests_Publisher", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsPublisher { get; set; }

        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsPublisherPlace { get; set; }

        [Display(Name = "publisher_date", ResourceType = typeof(ApsStrings))]
        public string ReviewRequestsPublisherDate { get; set; }

        [Display(Name = "majorof_dictionary", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsDictionaryMajor { get; set; }

        [Display(Name = "is_language_arabic", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsIsArabic { get; set; }

        //review
        [Display(Name = "is_translated", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsIsTranslated { get; set; }

        [Display(Name = "translated_to", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsTranslatedTo { get; set; }

        [Display(Name = "dictionary_title", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsDictionaryTitle { get; set; }

        [Display(Name = "dictionary_title_original", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsOriginalDictionaryTitle { get; set; }

        [Display(Name = "author_major", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsMajorOfAuther { get; set; }

        [Display(Name = "Recommendations", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsNoOfRecommendations { get; set; }

        [Display(Name = "Recommendations_date", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsRecommendationDate { get; set; }

        [Display(Name = "r_major", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsPublishType { get; set; }

        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsPublisher { get; set; }

        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsPublisherPlace { get; set; }

        [Display(Name = "publisher_date", ResourceType = typeof(ApsStrings))]
        public string DictionaryRequestsPublisherDate { get; set; }

        [Display(Name = "majorof_encyclopedia", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsMajorOfEncyclopedia { get; set; }

        [Display(Name = "is_language_arabic", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsIsArabic { get; set; }

        [Display(Name = "is_translated", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsIsTranslated { get; set; }

        [Display(Name = "translated_to", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsTranslatedTo { get; set; }

        [Display(Name = "encyclopedia_title", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsEncyclopediaTitle { get; set; }

        [Display(Name = "encyclopedia_original_title", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsOriginalEncyclopediaTitle { get; set; }

        [Display(Name = "author_major", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsMajorOfAuther { get; set; }

        [Display(Name = "Recommendations", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsNoOfRecommendations { get; set; }

        [Display(Name = "Recommendations_date", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsRecommendationDate { get; set; }

        [Display(Name = "Recommendations_type", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsPublishType { get; set; }

        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsPublisher { get; set; }

        [Display(Name = "publisher_place", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsPublisherPlace { get; set; }

        [Display(Name = "publisher_date", ResourceType = typeof(ApsStrings))]
        public string EncyclopediaRequestsPublisherDate { get; set; }

        [Display(Name = "handbook_type", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsHandbookType { get; set; }

        [Display(Name = "is_language_arabic", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsIsArabic { get; set; }

        [Display(Name = "is_translated", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsIsTranslated { get; set; }

        [Display(Name = "translated_to", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsTranslatedTo { get; set; }

        [Display(Name = "handBook_title", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsHandbookTitle { get; set; }

        [Display(Name = "original_handbook_title", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsOriginalHandbookTitle { get; set; }

        [Display(Name = "major_name", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsMajorId { get; set; }

        [Display(Name = "handbook_type", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsPublishType { get; set; }

        [Display(Name = "", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsPublisher { get; set; }

        [Display(Name = "publish_place", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsPublisherPlace { get; set; }

        [Display(Name = "publish_date", ResourceType = typeof(ApsStrings))]
        public string HandbookRequestsPublisherDate { get; set; }

        [Display(Name = "scientific_department", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsSciDepartmentId { get; set; }

        [Display(Name = "supervisor", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsSupervisor { get; set; }

        [Display(Name = "assistantSupervisor", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsAsstSupervisor { get; set; }

        [Display(Name = "thesis_title", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsThesisTitle { get; set; }

        [Display(Name = "discussion_committeemembers", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsCommitteeMembers { get; set; }

        [Display(Name = "degree", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsDegree { get; set; }

        [Display(Name = "ThesisRequestsGPA", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsGPA { get; set; }

        [Display(Name = "discussion_date", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsDateOfDiscussion { get; set; }

        [Display(Name = "publish_type", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsPublishType { get; set; }

        [Display(Name = "publisher", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsPublisher { get; set; }

        [Display(Name = "publish_place", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsPublisherPlace { get; set; }

        [Display(Name = "publish_date", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsPublisherDate { get; set; }

        [Display(Name = "ThesisRequestsMajorId", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsMajorId { get; set; }

        [Display(Name = "ThesisRequestsCollegeId", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsCollegeId { get; set; }

        [Display(Name = "university", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsUniversityOther { get; set; }

        [Display(Name = "author_major", ResourceType = typeof(ApsStrings))]
        public string ThesisRequestsMajorOther { get; set; }

        [Display(Name = "Files", ResourceType = typeof(ApsStrings))]
        public List<AttachedFile> Files { get; set; } = new List<AttachedFile>();

        [Display(Name = "Participants", ResourceType = typeof(ApsStrings))]
        public List<string> Participants { get; set; } = new List<string>();
    }
}