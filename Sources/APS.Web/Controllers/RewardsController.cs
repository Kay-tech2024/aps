using APS.Core;
using APS.Data;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(ApsUserRole.SystemAdmin)]
    public class RewardsController : BaseController
    {
        // GET: Rewards
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Researchers()
        {
            var model = new ResearchersRewardViewModel();
            var rewards = DbContext.ResearcherRewards.FirstOrDefault();

            model.Id = rewards.Id;
            model.Master = rewards.Master;
            model.Phd = rewards.Phd;
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Researchers(ResearchersRewardViewModel model)
        {
            var rewards = DbContext.ResearcherRewards.FirstOrDefault();
            if (rewards.Id == model.Id)
            {
                rewards.Master = model.Master;
                rewards.Phd = model.Phd;
                DbContext.Entry(rewards).State = EntityState.Modified;
                DbContext.SaveChanges();
                TempData["message"] = "يتم تحديث البيانات بنجاح";
                TempData["message_type"] = "success";
            }
            else
            {
                TempData["message"] = "لم يتم تحديث البيانات بنجاح";
                TempData["message_type"] = "danger";
            }

            return View("Index");
        }

        public ActionResult Reviewers()
        {
            var model = new ReviewersRewardViewModel();
            var rewards = DbContext.ReviewerRewards.ToList();
            model.ReviewerRewards = new List<ReviewerRewardViewModel>();

            foreach (var r in rewards)
            {
                var m = new ReviewerRewardViewModel();
                m.Id = r.Id;
                m.Minpages = r.Minpages;
                m.MaxPages = r.MaxPages;
                m.RewardValue = r.RewardValue;
                model.ReviewerRewards.Add(m);
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Reviewers(ReviewersRewardViewModel model)
        {
            var rewards = Utils.db.ReviewerRewards;
            if (model.ReviewerRewards != null && model.ReviewerRewards.Count > 0)
            {
                foreach (var m in model.ReviewerRewards)
                {
                    var r = rewards.Where(x => x.Id == m.Id).FirstOrDefault();
                    if (r.Id != 0)
                    {
                        r.RewardValue = m.RewardValue;
                        DbContext.Entry(r).State = EntityState.Modified;
                    }
                }
                DbContext.SaveChanges();
                TempData["message"] = "يتم تحديث البيانات بنجاح";
                TempData["message_type"] = "success";
            }
            else
            {
                TempData["message"] = "لم يتم تحديث البيانات بنجاح";
                TempData["message_type"] = "danger";
            }

            return View("Index");
        }
    }
}