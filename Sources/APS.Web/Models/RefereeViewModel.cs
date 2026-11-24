using System;

namespace APS.Web.Models
{
    public class RefereeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string MajorName { get; set; }
        public string Action { get; set; }
        public string UserId { get; set; }
        public string Comments { get; set; }

        public DateTime? SubmittedOn { get; set; }

        public string FileId { get; set; }
        public int RequestReviewOrderNumber { get; set; }
        public string FileName { get; set; }
        public bool? IsPrime { get; set; }
        public int NumberOfVotes { get; internal set; }
        public string ReviewSentOnStr { get; set; }
        public int Grade { get; internal set; }
        public string CreatedUserRole { get; internal set; }
    }
}