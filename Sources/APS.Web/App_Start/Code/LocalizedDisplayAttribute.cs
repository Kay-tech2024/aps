using System.ComponentModel;

namespace APS.Web.App_Start.Code
{
    public class LocalizedDisplayAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayAttribute(string resourceId)
            : base(GetMessageFromResource(resourceId))
        {
        }

        public static string GetMessageFromResource(string resourceId)
        {
            return LangHelpers.Instance[resourceId];
        }
    }
}