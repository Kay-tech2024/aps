using APS.Core;
using APS.Data.DbModels;
using APS.Web.Models;
using APS.Web.Resources;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    public class DelegationController : BaseController
    {
        // GET: Delegation
        public ActionResult Index()
        {
            var model = new DelegationViewModel();
            model.Delegations = DbContext.UserDelegations.OrderByDescending(x => x.Id).Where(x => x.DelegatorId == CurrentUser.Id).Select(
                d => new DelegationDetailsViewModel
                {
                    DelegateeEmail = d.Delegatee.Email,
                    DelegateeName = d.Delegatee.UserName,
                    EffectiveFrom = d.EffectiveFrom.ToString(),
                    EffectiveTo = d.EffectiveTo.ToString()
                }).ToList();
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(DelegationViewModel model)
        {
            // Verify Email Id
            if (!Utils.ValidateEmail(model.Email))
            {
                TempData["message"] = ApsStrings.invalid_email;
                TempData["message_type"] = "danger";
                return View(model);
            }
            var toUser = DbContext.Users.Where(x => x.Email == model.Email).FirstOrDefault();
            if (toUser == null)
            {
                TempData["message"] = ApsStrings.invalid_return_user;
                TempData["message_type"] = "danger";
                return View(model);
            }
            if (toUser.Id == CurrentUser.Id)
            {
                TempData["message"] = "You cannot delegate yourself..";
                TempData["message_type"] = "danger";
                return View(model);
            }

            try
            {
                var userFrom = DbContext.UserDelegations.OrderByDescending(x => x.Id).Where(x => x.DelegatorId == CurrentUser.Id).FirstOrDefault();
                DateTime effectFrom = DateTime.MinValue;
                DateTime effectTo = DateTime.MaxValue;
                DateTime.TryParseExact(model.EffectiveFrom.Trim(), "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out effectFrom);
                if (!string.IsNullOrWhiteSpace(model.EffectiveTo))
                {
                    DateTime.TryParseExact(model.EffectiveTo.Trim(), "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out effectTo);
                }

                var delegation = new UserDelegation();
                if (userFrom == null)
                {
                    delegation.DelegatorId = CurrentUser.Id;
                    delegation.DelegateeId = toUser.Id;
                    delegation.EffectiveFrom = effectFrom;
                    if (effectTo != DateTime.MaxValue)
                    {
                        delegation.EffectiveTo = effectTo;
                    }
                    DbContext.UserDelegations.Add(delegation);
                    DbContext.SaveChanges();
                }
                else
                {
                    //update previous entry
                    userFrom.EffectiveTo = effectFrom;
                    DbContext.Entry(userFrom).State = EntityState.Modified;

                    //insert new entry
                    delegation.DelegatorId = CurrentUser.Id;
                    delegation.DelegateeId = toUser.Id;
                    delegation.EffectiveFrom = effectFrom;
                    if (effectTo != DateTime.MaxValue)
                    {
                        delegation.EffectiveTo = effectTo;
                    }
                    DbContext.UserDelegations.Add(delegation);
                    DbContext.SaveChanges();
                }
                TempData["message"] = "تم التفويض بنجاح";
                TempData["message_type"] = "success";
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, model);
                var ex2 = ex;
                while (ex2.InnerException != null)
                {
                    ex2 = ex2.InnerException;
                }
                TempData["message"] = ApsStrings.failed + " " + ex2.Message;
                TempData["message_type"] = "danger";
            }

            return View(model);
        }
    }
}