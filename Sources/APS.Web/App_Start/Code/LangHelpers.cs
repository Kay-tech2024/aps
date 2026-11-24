using APS.Core;
using APS.Web.Resources;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace APS.Web.App_Start.Code
{
    public class LangHelpers : Translator
    {
        private static Translator _instance;

        private LangHelpers()
        {
        }

        public override string GetLangValueByString(string str, bool returnNullIfNotFound = false, string lang = null)
        {
            if (str == null) return null;
            str = Utils.PascalCaseToUnderscore(str);

            CultureInfo Culture = new CultureInfo(lang != "EN" ? "ar-SA" : "en-US");

            var oldCulture = Thread.CurrentThread.CurrentUICulture.Name;

            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;

            var t = typeof(ApsStrings);
            var fields = t.GetProperties(BindingFlags.Static | BindingFlags.Public)
                .Where(fi => fi.Name == str).ToList();

            var tstr = str;
            if (fields.Count > 0) tstr = fields[0].GetValue(null).ToString();
            else if (returnNullIfNotFound) tstr = null;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(oldCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(oldCulture);

            return tstr;
        }

        public string GetRequestHistoryStatus(string str)
        {
            return "";
        }

        public static Translator Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = new LangHelpers();
                return Instance;
            }
        }
    }
}