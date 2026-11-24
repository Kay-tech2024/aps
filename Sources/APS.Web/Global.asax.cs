using APS.Core;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace APS.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest()
        {
        }

        protected void Application_EndRequest()
        {
            if (Request.Cookies.Count > 0)
            {
                foreach (string s in Request.Cookies.AllKeys)
                {
                    if (s == "id")
                    {
                        Request.Cookies["id"].HttpOnly = true;
                    }
                    //if (!Utils.IsLocalhost)
                    //{
                    //    Response.Cookies[s].Secure = true;
                    //}
                }
            }

            if (this.Context != null && this.Context.AllErrors != null && this.Context.AllErrors.Length > 0)
            {
                foreach (var ex in this.Context.AllErrors)
                {
                    Logger.LogExceptionAsync(ex);
                }
            }
        }

        private bool IsUserAllowedToSeeMiniProfilerUI(HttpRequest httpRequest)
        {
            return true;
        }

        protected void Application_PreSendRequestHeaders(object source, EventArgs e)
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
        }

        protected void Application_Start()
        {
            //HangfireBootstrapper.Instance.Start();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            MVCGridConfig.RegisterGrids();

            using (var db = new ApsDbEntities())
            {
                try
                {
                    {
                        var rolesEnums = Enum.GetValues(typeof(ApsUserRole));
                        var roles = db.Roles.ToList();

                        foreach (var r in rolesEnums)
                        {
                            if (roles.Count(x => x.Name == r.ToString()) > 0) continue;

                            var role = new Role
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = r.ToString()
                            };

                            db.Roles.Add(role);
                        }

                        db.SaveChanges();
                    }
                    //{
                    //    var rolesEnums = Enum.GetNames(typeof(ApsUserRole));
                    //    var roles = db.Roles.ToList();

                    //    foreach (var r in roles)
                    //    {
                    //        if (rolesEnums.Count(x => x ==r.Name) > 0) continue;

                    //        db.Roles.Remove(r);
                    //    }

                    //    db.SaveChanges();
                    //}

                    {
                        var oldStatusEnum = Enum.GetValues(typeof(OldRequestStatus));

                        foreach (var r in oldStatusEnum)
                        {
                            var reqs = db.Requests.Where(x => x.Status == (long)r).ToList();

                            foreach (var req in reqs)
                            {
                                var newStatus = RequestStatus.Unspecified;

                                Enum.TryParse<RequestStatus>(r.ToString(), true, out newStatus);

                                if (newStatus > 0)
                                {
                                    req.Status = (long)newStatus;
                                }
                            }

                            db.SaveChanges();
                            var reqsH = db.RequestHistoryLogs.Where(x => x.Status == (long)r).ToList();

                            foreach (var req in reqsH)
                            {
                                var newStatus = RequestStatus.Unspecified;

                                Enum.TryParse<RequestStatus>(r.ToString(), true, out newStatus);

                                if (newStatus > 0)
                                {
                                    req.Status = (long)newStatus;
                                }
                            }

                            db.SaveChanges();
                        }

                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void Application_AcquireRequestState(object sender, EventArgs e)
        {
            var path = Request.RawUrl;

            if (path.ToLower().StartsWith(Request.ApplicationPath.ToLower() + "/content") || path.ToLower().StartsWith(Request.ApplicationPath.ToLower() + "/bundles"))
                return;

            Utils.SetUserLocale(Context);
        }

        private void Session_Start(object sender, EventArgs e)
        {
            string CookieHeaders = HttpContext.Current.Request.Headers["Cookie"];

            if ((null != CookieHeaders) && (CookieHeaders.IndexOf("ASP.NET_SessionId") >= 0))
            {
                // It is existing visitor, but ASP.NET session is expired
            }
            else
            {
                // It is a new visitor, session was not created before
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            //HangfireBootstrapper.Instance.Stop();
        }
    }
}