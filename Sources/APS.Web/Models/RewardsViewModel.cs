using System.Collections.Generic;

namespace APS.Web.Models
{
    public class RewardsViewModel
    {
    }

    public class ReviewerRewardViewModel
    {
        public long Id { get; set; }
        public long Minpages { get; set; }
        public long MaxPages { get; set; }
        public long RewardValue { get; set; }
    }

    public class ReviewersRewardViewModel
    {
        //public ReviewersRewardViewModel()
        //{
        //    ReviewerRewards = new List<ReviewerRewardViewModel>();
        //}
        public List<ReviewerRewardViewModel> ReviewerRewards { get; set; }
    }

    public class ResearchersRewardViewModel
    {
        public long Id { get; set; }
        public long Master { get; set; }
        public long Phd { get; set; }
    }
}