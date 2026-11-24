using APS.Data;
using APS.Web.App_Start.Code;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(ApsUserRole.SystemAdmin)]
    public class ImpersonationController : BaseController
    {
        // GET: Impersonation
        public ActionResult Index()
        {
            return View();
        }
    }
}