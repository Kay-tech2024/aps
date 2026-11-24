using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    public class DocumentsController : BaseController
    {
        // GET: Documents
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DocumentsPdf(string name)
        {
            //switch(name)
            //{
            //    case "TermsAndConditions":

            //        break;
            //    case "Rules":

            //        break;
            //    case "Others":

            //        break;

            //}
            var file = Path.Combine(Server.MapPath("~/DocumentsPdf"), name + ".PDF");
            file = Path.GetFullPath(file);
            return File(file, "application/pdf");
        }

        //[AjaxOnly]
        public ActionResult DownloadTemplate(string Id)
        {
            var template = DbContext.Templates.Where(x => x.TemplateKey.ToString() == Id).FirstOrDefault();
            var fileName = template.FileName;
            var extension = template.Extension;
            var file = Path.Combine(Server.MapPath("~/DocumentsPdf"), fileName + extension);
            file = Path.GetFullPath(file);
            return File(file, "application/pdf");
        }
    }
}