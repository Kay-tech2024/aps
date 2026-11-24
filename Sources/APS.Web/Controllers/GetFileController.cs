using APS.Web.App_Start.Code;
using HeyRed.Mime;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize]
    [AllowAnonymous]
    public class GetFileController : BaseController
    {
        public ActionResult Index(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Response.Write("ملف غير صالح");
            }

            var file = (FileToBeDownloaded)null;
            file = DbContext.UserSessionFiles.Where(c => c.Id.ToString() == id && c.SessionId == UserSession.Id).Where(a => a.Id.ToString() == id).Select(c => new FileToBeDownloaded
            {
                FilePath = c.FilePath,
                FileName = c.FileName
            }).FirstOrDefault();
            if (file == null)
            {
                file = DbContext.RequestFiles.Where(a => a.Id.ToString() == id).Select(c => new FileToBeDownloaded
                {
                    FilePath = c.FilePath,
                    FileName = c.FileName
                }).FirstOrDefault();
                if (file == null)
                {
                    //file = DbContext.UserFiles.Where(c => !IsCollegeUser || (CurrentUser.CollegeId != null && c.User.CollegeId == CurrentUser.CollegeId)).Where(a => a.Id.ToString() == id).Select(c => new FileToBeDownloaded
                    //{
                    //    FilePath = c.FilePath,
                    //    FileName = c.FileName
                    //}).FirstOrDefault();
                    file = DbContext.UserFiles.Where(a => a.Id.ToString() == id).Select(c => new FileToBeDownloaded
                    {
                        FilePath = c.FilePath,
                        FileName = c.FileName
                    }).FirstOrDefault();
                }
            }

            if (file == null)
            {
                Response.Write("ملف غير صالح");
                Response.End();
                return null;
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                Response.Write("ملف غير موجود");
                Response.End();
                return null;
            }

            return File(file.FilePath, MimeTypesMap.GetMimeType(Path.GetExtension(file.FilePath)), file.FileName);
        }
    }

    public class FileToBeDownloaded
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public bool Isdownloaded { get; set; }
    }
}