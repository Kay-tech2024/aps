using APS.Core;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using System;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Return()
        {
            return RedirectToLastIndexPage("Index", "Home");
        }

        public ActionResult ChangeUserRole(string role, int? collegeId, string username)
        {
            var roleEnum = ApsUserRole.None;

            if (!string.IsNullOrWhiteSpace(username))
            {
                var user = DbContext.Users.FirstOrDefault(u => u.UserName == username && u.IsDeleted == false);
                if (user != null)
                {
                    role = user.Roles.Select(r => r.Name).FirstOrDefault();
                    collegeId = user.CollegeId;
                }
            }

            Enum.TryParse(role, true, out roleEnum);

            var isValid = false;
            if (roleEnum != ApsUserRole.None)
            {
                if (Utils.IsCollegeRole(roleEnum))
                {
                    var college = (College)null;
                    if (collegeId != null && (college = DbContext.Colleges.Find(collegeId.Value)) != null)
                    {
                        isValid = true;
                    }
                    else
                    {
                    }
                }
                else
                {
                    isValid = true;
                }
            }

            if (isValid)
            {
                if (UserUtils.CanChangeRoleTo(roleEnum, collegeId))
                {
                    UserSession.CurrentRole = (byte)roleEnum;
                    if (Utils.IsCollegeRole(roleEnum))
                        UserSession.CurrentRoleDepartmentId = collegeId;
                    else
                        UserSession.CurrentRoleDepartmentId = null;
                    DbContext.SaveChanges();
                }
                else
                {
                    isValid = false;
                }
            }

            var referrer = Utils.GetReferrerUrl();

            if (!string.IsNullOrWhiteSpace(referrer) && referrer != "No referrer")
            {
                return Redirect(referrer);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
    }
}