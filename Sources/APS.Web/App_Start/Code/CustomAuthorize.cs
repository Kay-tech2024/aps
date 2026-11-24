using APS.Auth;
using APS.Core;
using APS.Data;
using APS.Web.Controllers;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace APS.Web.App_Start.Code
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class CustomAuthorize : AuthorizeAttribute
    {
        private Privilages[] _privilages;
        private ApsUserRole[] _roles;

        public CustomAuthorize() : this(new Privilages[0], null)
        {
        }

        public CustomAuthorize(params Privilages[] privilages) : this(privilages, null)
        {
        }

        public CustomAuthorize(params ApsUserRole[] roles) : this(new Privilages[0], roles)
        {
        }

        public CustomAuthorize(Privilages[] privilages, params ApsUserRole[] roles)
        {
            if (roles != null && roles.Length > 0)
            {
                this.Roles = string.Join(",", roles.Select(r => Enum.GetName(r.GetType(), r)));
            }
            else
            {
                this.Roles = null;
            }

            this._privilages = privilages;
            this._roles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if ((_privilages == null || _privilages.Length == 0) && (_roles == null || _roles.Length == 0))
            {
                return httpContext != null && httpContext.User != null && httpContext.User.Identity.IsAuthenticated == true;
            }

            bool isAuthorized = false;

            if (_privilages != null && _privilages.Length > 0)
            {
                isAuthorized = Utils.HasPrivilagesAny(_privilages);
            }

            if (_roles != null && _roles.Length > 0)
            {
                isAuthorized = isAuthorized || Utils.HasRolesAny(_roles);
            }

            return isAuthorized;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            ApplicationUser user = null;
            var roles = Utils.GetUserRoles();

            if (filterContext.HttpContext != null && filterContext.HttpContext.User != null && filterContext.HttpContext.User.Identity != null && filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                user = ((BaseController)filterContext.Controller).CurrentUser;

                if (user != null && (!user.IsActivated || (Utils.IsCollegeRole(roles) && user.CollegeId == null)))
                {
                    new AccountController().LogOff();
                    user = null;
                    roles = new ApsUserRole[0];
                }
            }

            if (user == null)
            {
                if (string.IsNullOrWhiteSpace(filterContext.HttpContext.Request["UserName"]))
                {
                    var controller = new AccountController();

                    var result = (ViewResult)controller.Login(null);
                    var non = new NonceFilter();
                    non.OnActionExecuting2(filterContext);
                    filterContext.Result = result;
                }
            }
            else
            {
                var result = new ContentResult();
                result.Content = "Not authorized. <a href=\"" + Utils.GetApplicationUrl() + "\">Go to homepage</a>";
                filterContext.Result = result;
            }
        }
    }
}