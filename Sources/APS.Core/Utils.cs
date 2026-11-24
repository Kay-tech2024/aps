using APS.Auth;
using APS.Core.Managers;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using APS.Services;
using APS.TimeMachine;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;

namespace APS.Core
{
    public static class Utils
    {
        [ThreadStaticAttribute]
        private static Dictionary<string, dynamic> _staticObjs = new Dictionary<string, dynamic>();

        public static string StripHTML(string HTMLText, bool decode = true)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var stripped = reg.Replace(HTMLText, "");
            return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
        }

        public static bool IsUserLanguageArabic
        {
            get
            {
                return CultureInfo.CurrentUICulture.Name.StartsWith("ar");
            }
        }

        public static string SystemAdminUserId
        {
            get
            {
                return "00000000-0000-0000-0000-000000000000";
            }
        }

        public static ApsDbEntities db
        {
            get
            {
                var _db = (ApsDbEntities)null;

                if (Utils.HasOwinContext)
                {
                    _db = HttpContext.Current.GetOwinContext().Get<ApsDbEntities>(typeof(ApsDbEntities).FullName);
                }
                else
                {
                    if (_staticObjs == null) _staticObjs = new Dictionary<string, dynamic>();

                    if (_staticObjs.ContainsKey(typeof(ApsDbEntities).FullName))
                        _db = (ApsDbEntities)_staticObjs[typeof(ApsDbEntities).FullName];
                }

                if (_db == null || _db.IsDisposed)
                {
                    _db = new ApsDbEntities();

                    _db.Configuration.AutoDetectChangesEnabled = false;
                    _db.Configuration.LazyLoadingEnabled = true;
                    _db.Configuration.ValidateOnSaveEnabled = false;

                    if (Utils.HasOwinContext)
                    {
                        HttpContext.Current.GetOwinContext().Set<ApsDbEntities>(typeof(ApsDbEntities).FullName, _db);
                    }
                    else
                    {
                        _staticObjs[typeof(ApsDbEntities).FullName] = _db;
                    }
                }

                return _db;
            }

            set
            {
                value.Configuration.AutoDetectChangesEnabled = false;
                value.Configuration.LazyLoadingEnabled = false;
                value.Configuration.ValidateOnSaveEnabled = false;

                if (Utils.HasOwinContext)
                    HttpContext.Current.GetOwinContext().Set<ApsDbEntities>(typeof(ApsDbEntities).FullName, value);
                else
                {
                    if (_staticObjs == null) _staticObjs = new Dictionary<string, dynamic>();

                    if (_staticObjs.ContainsKey(typeof(ApsDbEntities).FullName))
                        _staticObjs[typeof(ApsDbEntities).FullName] = value;
                }
            }
        }

        public static ApplicationDbContext auth_db
        {
            get
            {
                var _db = Utils.HasOwinContext ? HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>() : null;
                if (_db == null)
                {
                    if (Utils.HasOwinContext)
                    {
                        _db = new ApplicationDbContext();

                        _db.Configuration.AutoDetectChangesEnabled = false;
                        _db.Configuration.LazyLoadingEnabled = false;
                        _db.Configuration.ValidateOnSaveEnabled = false;

                        HttpContext.Current.GetOwinContext().Set<ApplicationDbContext>(_db);
                    }
                    else
                    {
                        if (_staticObjs == null) _staticObjs = new Dictionary<string, dynamic>();

                        if (_staticObjs.ContainsKey(typeof(ApplicationDbContext).FullName))
                            return (ApplicationDbContext)_staticObjs[typeof(ApplicationDbContext).FullName];

                        _db = new ApplicationDbContext();

                        _db.Configuration.AutoDetectChangesEnabled = false;
                        _db.Configuration.LazyLoadingEnabled = false;
                        _db.Configuration.ValidateOnSaveEnabled = false;

                        _staticObjs[typeof(ApplicationDbContext).FullName] = _db;
                    }
                }

                return _db;
            }

            set
            {
                value.Configuration.AutoDetectChangesEnabled = false;
                value.Configuration.LazyLoadingEnabled = false;
                value.Configuration.ValidateOnSaveEnabled = false;

                if (Utils.HasOwinContext)
                    HttpContext.Current.GetOwinContext().Set<ApplicationDbContext>(value);
                else
                {
                    if (_staticObjs == null) _staticObjs = new Dictionary<string, dynamic>();

                    if (_staticObjs.ContainsKey(typeof(ApplicationDbContext).FullName))
                        _staticObjs[typeof(ApplicationDbContext).FullName] = value;
                }
            }
        }

        public static void DapperAppend(this StringBuilder builder, string str, StringBuilder builder2)
        {
            builder.Append(str);
            builder2.Append(str);
        }

        public static void ClearFolder(string FolderName)
        {
            if (string.IsNullOrWhiteSpace(FolderName)) return;

            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles("*.*", SearchOption.AllDirectories))
            {
                fi.IsReadOnly = false;
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                di.Delete();
            }
        }

        private static Regex rx_email = new Regex(@"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.Compiled);

        private static Regex rx_Officialemail = new Regex(@"^(.+)(edu|edu.sa|gov|gov.sa)$", RegexOptions.Compiled);
        private static Regex rx_Arabic_Chars = new Regex(@"[\u0600-\u06ff]|[\u0750-\u077f]|[\ufb50-\ufc3f]|[\ufe70-\ufefc]", RegexOptions.Compiled);

        public static bool IsTextArabic(string text)
        {
            return rx_Arabic_Chars.Match(text).Success;
        }

        public static void SwitchCulture(string lang = null)
        {
            if (lang == null)
            {
                var c = (CultureInfo)GetDictVal("OldCulture");

                if (c != null)
                {
                    Thread.CurrentThread.CurrentCulture = c;
                    Thread.CurrentThread.CurrentUICulture = c;
                }
                return;
            }

            lang = lang.Trim().ToUpper();
            if (lang != "AR" && lang != "EN") return;

            var oldCulture = Thread.CurrentThread.CurrentUICulture;
            SetDictVal("OldCulture", oldCulture);
            CultureInfo Culture = new CultureInfo(lang != "EN" ? "ar-SA" : "en-US");
            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = Culture;
        }

        public static bool ValidateEmail(string email)
        {
            return rx_email.Match(email).Success;
        }

        public static bool ValidateMobile(string mobile)
        {
            string m = "";
            return ValidateMobile(mobile, out m);
        }

        public static bool ValidateMobile(string mobile, out string validMobileNumber)
        {
            validMobileNumber = null;
            if (string.IsNullOrWhiteSpace(mobile)) return false;

            var number = (PhoneNumber)null;

            var util = PhoneNumberUtil.GetInstance();

            try
            {
                number = util.Parse(mobile, "SA");
                validMobileNumber = number.CountryCode.ToString() + number.NationalNumber;

                if (!util.IsValidNumber(number) || util.GetNumberType(number) != PhoneNumberType.MOBILE)
                {
                    return false;
                }
                return true;
            }
            catch (NumberParseException)
            {
                return false;
            }
        }

        public static int CalculateAge(DateTime date)
        {
            return (int)Math.Floor((DateTime.Now - date).TotalDays / 365.25);
        }

        public static bool ValidateIBAN(string iban)
        {
            if (string.IsNullOrWhiteSpace(iban)) return false;

            iban = iban.ToUpper().Trim(); //IN ORDER TO COPE WITH THE REGEX BELOW

            if (System.Text.RegularExpressions.Regex.IsMatch(iban, "^[A-Z0-9]"))
            {
                iban = iban.Replace(" ", String.Empty);
                string bank =
                iban.Substring(4, iban.Length - 4) + iban.Substring(0, 4);
                int asciiShift = 55;
                StringBuilder sb = new StringBuilder();
                foreach (char c in bank)
                {
                    int v;
                    if (Char.IsLetter(c)) v = c - asciiShift;
                    else v = int.Parse(c.ToString());
                    sb.Append(v);
                }
                string checkSumString = sb.ToString();
                int checksum = int.Parse(checkSumString.Substring(0, 1));
                for (int i = 1; i < checkSumString.Length; i++)
                {
                    int v = int.Parse(checkSumString.Substring(i, 1));
                    checksum *= 10;
                    checksum += v;
                    checksum %= 97;
                }
                return checksum == 1;
            }
            else
                return false;
        }

        public static bool ValidateSWIFT(string swift)
        {
            if (string.IsNullOrWhiteSpace(swift)) return false;

            return true;
            //swift = swift.ToUpper().Trim(); //IN ORDER TO COPE WITH THE REGEX BELOW

            //if (Regex.IsMatch(swift, "^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$]"))
            //{
            //    swift = swift.Replace(" ", String.Empty);
            //    string bank =
            //    swift.Substring(4, swift.Length - 4) + swift.Substring(0, 4);
            //    int asciiShift = 55;
            //    StringBuilder sb = new StringBuilder();
            //    foreach (char c in bank)
            //    {
            //        int v;
            //        if (Char.IsLetter(c)) v = c - asciiShift;
            //        else v = int.Parse(c.ToString());
            //        sb.Append(v);
            //    }
            //    string checkSumString = sb.ToString();
            //    int checksum = int.Parse(checkSumString.Substring(0, 1));
            //    for (int i = 1; i < checkSumString.Length; i++)
            //    {
            //        int v = int.Parse(checkSumString.Substring(i, 1));
            //        checksum *= 10;
            //        checksum += v;
            //        checksum %= 97;
            //    }
            //    return checksum == 1;
            //}
            //else
            //    return false;
        }

        public static bool ValidateRoutingNumber(string routingNumber)
        {
            return true;
        }

        public static DateTime GetNistTime()
        {
            DateTime dateTime = DateTime.MinValue;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader stream = new StreamReader(response.GetResponseStream());
                string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
                string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
                double milliseconds = Convert.ToInt64(time) / 1000.0;
                dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds).ToLocalTime();
            }

            return dateTime;
        }

        public static bool HasOwinContext
        {
            get
            {
                if (HttpContextManager.Current == null || HttpContextManager.Current.Request == null) return false;

                try
                {
                    var owinContext = HttpContextManager.Current.GetOwinContext();
                    if (owinContext == null) return false;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static T GetInstance<T>() where T : new()
        {
            if (!HasOwinContext)
            {
                return new T();
            }

            var instance = HttpContextManager.Current.GetOwinContext().Get<T>(typeof(T).FullName);
            if (instance == null)
            {
                instance = new T();
                HttpContextManager.Current.GetOwinContext().Set<T>(typeof(T).FullName, instance);
            }

            return instance;
        }

        public static void SetUserLocale(HttpContext context)
        {
            var lang = (string)null;
            if (context != null && context.Request != null && context.Request.QueryString["lang"] != null)
            {
                var userId = GetUserId();

                var tlang = context.Request.QueryString["lang"].ToUpper().Trim();

                if (tlang == "AR" || tlang == "EN")
                {
                    lang = tlang;
                    HttpRequest Request = context.Request;

                    if (context.User != null && context.User.Identity.IsAuthenticated)
                    {
                        var db = Utils.db;

                        var pref = db.UserPreferences.Where(p => p.UserId == userId).SingleOrDefault(p => p.Key == "LANG");
                        if (pref == null)
                        {
                            pref = new UserPreference
                            {
                                Key = "LANG",
                                UserId = userId,
                                UserAgent = Request.UserAgent
                            };

                            db.UserPreferences.Add(pref);
                        }
                        else
                        {
                            pref.Value = context.Request.QueryString["lang"].ToUpper();
                            db.Entry(pref).State = System.Data.Entity.EntityState.Modified;
                        }

                        db.SaveChanges();
                    }
                    else if (context.Session != null)
                    {
                        context.Session["lang"] = lang;
                    }
                }
            }
            else if (context != null && context.Session != null && context.Session["lang"] != null)
            {
                var tlang = ((string)context.Session["lang"]).ToUpper().Trim();

                if (tlang == "AR" || tlang == "EN")
                {
                    lang = tlang;
                }
            }
            else
            {
                var db = Utils.db;
                var userId = GetUserId();
                var pref = db.UserPreferences.Where(p => p.UserId == userId).SingleOrDefault(p => p.Key == "LANG");
                if (pref != null)
                {
                    lang = pref.Value;
                }
            }

            try
            {
                CultureInfo Culture = new CultureInfo(lang != "EN" ? "ar-SA" : "en-US");
                Thread.CurrentThread.CurrentCulture = Culture;
                var UICulture = new CultureInfo(lang != "EN" ? "ar-SA" : "en-US");
                Thread.CurrentThread.CurrentUICulture = UICulture;
            }
            catch (Exception ex) { Logger.LogExceptionAsync(ex); }
        }

        public static string GetUserId()
        {
            if (HttpContextManager.Current == null || HttpContextManager.Current.User == null) return null;

            return HttpContextManager.Current.User.Identity.EnhancedGetUserId();
        }

        public static bool HasPrivilagesAll(string userId, params Privilages[] privilages)
        {
            return HasPrivilagesAll(null, userId, privilages);
        }

        public static bool HasPrivilagesAll(params Privilages[] privilages)
        {
            return HasPrivilagesAll(HttpContextManager.Current, null, privilages);
        }

        public static bool HasPrivilagesAll(HttpContextBase context, string userId, params Privilages[] privilages)
        {
            if (privilages == null || privilages.Count(p => p > 0) == 0) return false;
            var userPrivilages = GetUserPrivilages(context, userId);
            if (userPrivilages == 0) return false;

            foreach (var p in privilages)
            {
                if (p == 0) continue;

                if ((userPrivilages & p) != p)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HasPrivilagesAny(string userId, params Privilages[] privilages)
        {
            return HasPrivilagesAny(null, userId, privilages);
        }

        public static bool HasPrivilagesAny(params Privilages[] privilages)
        {
            return HasPrivilagesAny(HttpContextManager.Current, null, privilages);
        }

        public static bool HasPrivilagesAny(HttpContextBase context, string userId, params Privilages[] privilages)
        {
            if (privilages == null || privilages.Count(p => p > 0) == 0) return false;

            var userPrivilages = GetUserPrivilages(context, userId);

            foreach (var p in privilages)
            {
                if (p == 0) continue;

                if ((userPrivilages & p) == p)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasRolesAny(params ApsUserRole[] roles)
        {
            return HasRolesAny(HttpContextManager.Current, null, roles);
        }

        public static bool HasRolesAny(string userId, params ApsUserRole[] roles)
        {
            return HasRolesAny(HttpContextManager.Current, userId, roles);
        }

        public static bool HasActualRolesAny(params ApsUserRole[] roles)
        {
            return HasActualRolesAny(HttpContextManager.Current, null, roles);
        }

        public static bool HasRolesAny(HttpContextBase context, string userId, params ApsUserRole[] roles)
        {
            if (roles == null || roles.Length == 0) return false;

            var userRoles = GetUserRoles(context, userId);

            foreach (var r in roles)
            {
                if (userRoles.Count(r2 => r2 == r) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasActualRolesAny(HttpContextBase context, string userId, params ApsUserRole[] roles)
        {
            if (roles == null || roles.Length == 0) return false;

            var userRoles = GetActualUserRoles(context, userId);

            foreach (var r in roles)
            {
                if (userRoles.Count(r2 => r2 == r) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static Privilages GetUserPrivilages()
        {
            return GetUserPrivilages(HttpContextManager.Current, null);
        }

        public static Privilages GetActualUserPrivilages()
        {
            return GetActualUserPrivilages(HttpContextManager.Current, null);
        }

        public static Privilages GetUserPrivilages(string userID)
        {
            return GetUserPrivilages(null, userID);
        }

        public static Privilages GetActualUserPrivilages(string userID)
        {
            return GetActualUserPrivilages(null, userID);
        }

        public static Privilages GetUserPrivilages(HttpContextBase context = null, string userID = null)
        {
            if (context == null) context = HttpContextManager.Current;

            var roles = GetUserRoles(context, userID);

            if (roles != null && roles.Length > 0)
            {
                if (userID != null && context != null && context.Session != null && context.Session["CURRENT_ROLE"] != null)
                {
                    var role = ApsUserRole.None;

                    if (Enum.TryParse(context.Session["CURRENT_ROLE"].ToString(), out role) && role != ApsUserRole.None)
                    {
                        return AccessPrivilagesMatrix.GetByUserRole(role);
                    }
                }

                var privilages = AccessPrivilagesMatrix.GetByUserRole(roles[0]);
                for (var i = 1; i < roles.Length; i++)
                {
                    privilages |= AccessPrivilagesMatrix.GetByUserRole(roles[i]);
                }

                return privilages;
            }

            return 0;
        }

        public static Privilages GetActualUserPrivilages(HttpContextBase context = null, string userID = null)
        {
            if (context == null) context = HttpContextManager.Current;

            var roles = GetUserRoles(context, userID);

            if (roles != null && roles.Length > 0)
            {
                if (userID != null && context != null && context.Session != null && context.Session["CURRENT_ROLE"] != null)
                {
                    var role = ApsUserRole.None;

                    if (Enum.TryParse(context.Session["CURRENT_ROLE"].ToString(), out role) && role != ApsUserRole.None)
                    {
                        return AccessPrivilagesMatrix.GetByUserRole(role);
                    }
                }

                var privilages = AccessPrivilagesMatrix.GetByUserRole(roles[0]);
                for (var i = 1; i < roles.Length; i++)
                {
                    privilages |= AccessPrivilagesMatrix.GetByUserRole(roles[i]);
                }

                return privilages;
            }

            return 0;
        }

        public static ApsUserRole GetUserRole(HttpContextBase context = null, string userID = null)
        {
            var roles = GetUserRoles(context, userID);
            if (roles == null || roles.Length == 0) return ApsUserRole.None;
            if (roles.Length == 1) return roles[0];

            var role = roles[0];
            for (var i = 1; i < roles.Length; i++)
            {
                if (roles[i] > role)
                {
                    role = roles[i];
                }
            }

            return role;
        }

        public static ApsUserRole GetActualUserRole(HttpContextBase context = null, string userID = null)
        {
            var roles = GetActualUserRoles(context, userID);
            if (roles == null || roles.Length == 0) return ApsUserRole.None;
            if (roles.Length == 1) return roles[0];

            var role = roles[0];
            for (var i = 1; i < roles.Length; i++)
            {
                if (roles[i] > role)
                {
                    role = roles[i];
                }
            }

            return role;
        }

        public static ApsUserRole[] GetUserRoles()
        {
            return GetUserRoles(HttpContextManager.Current, null);
        }

        public static ApsUserRole[] GetActualUserRoles()
        {
            return GetActualUserRoles(HttpContextManager.Current, null);
        }

        public static ApsUserRole[] GetUserRoles(string userId)
        {
            return GetUserRoles(null, userId);
        }

        public static ApsUserRole[] GetActualUserRoles(string userId)
        {
            return GetActualUserRoles(null, userId);
        }

        public static ApsUserRole[] GetActualUserRoles(HttpContextBase context = null, string userId = null)
        {
            if (userId == null)
            {
                if (context == null)
                {
                    context = HttpContextManager.Current;
                }

                if (context != null && context.User != null && context.User.Identity != null)
                {
                    if (context.User.Identity is ClaimsIdentity)
                    {
                        var identity = (ClaimsIdentity)context.User.Identity;
                        var roleClaims = identity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

                        var roles = new List<ApsUserRole>();

                        foreach (var c in roleClaims)
                        {
                            var role = ApsUserRole.None;

                            if (Enum.TryParse(c.Value, out role) && role != ApsUserRole.None)
                            {
                                roles.Add(role);
                            }
                        }

                        return roles.ToArray();
                    }
                    else
                    {
                        var username = context.User.Identity.Name;
                        var user = Utils.db.Users.FirstOrDefault(u => u.UserName == username);

                        if (user != null)
                        {
                            var roles = new List<ApsUserRole>();

                            foreach (var c in user.Roles)
                            {
                                var role = ApsUserRole.None;

                                if (Enum.TryParse(c.Name, out role) && role != ApsUserRole.None)
                                {
                                    roles.Add(role);
                                }
                            }

                            return roles.ToArray();
                        }
                    }
                }
            }
            else
            {
                var user = Utils.db.Users.SingleOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    var userRoles = user.Roles.Select(r => r.Name).ToList();

                    var roles = new List<ApsUserRole>();

                    foreach (var r in userRoles)
                    {
                        var role = ApsUserRole.None;

                        if (Enum.TryParse(r, out role) && role != ApsUserRole.None)
                        {
                            roles.Add(role);
                        }
                    }

                    return roles.ToArray();
                }
            }

            return new ApsUserRole[0];
        }

        public static ApsUserRole[] GetUserRoles(HttpContextBase context = null, string userId = null)
        {
            if (userId == null)
            {
                if (context == null)
                {
                    context = HttpContextManager.Current;
                }

                if (context != null && context.User != null && context.User.Identity != null)
                {
                    if (context.User.Identity is ClaimsIdentity)
                    {
                        var identity = (ClaimsIdentity)context.User.Identity;
                        var roleClaims = identity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

                        var roles = new List<ApsUserRole>();

                        var UserSession = GetCurrentUserSession();
                        if (UserSession != null && UserSession.CurrentRole != null && UserUtils.CanChangeRoleTo((ApsUserRole)UserSession.CurrentRole, (int?)UserSession.CurrentRoleDepartmentId))
                        {
                            roles = new List<ApsUserRole>() { (ApsUserRole)UserSession.CurrentRole };
                        }
                        else
                        {
                            foreach (var c in roleClaims)
                            {
                                var role = ApsUserRole.None;

                                if (Enum.TryParse(c.Value, out role) && role != ApsUserRole.None)
                                {
                                    roles.Add(role);
                                }
                            }
                        }

                        return roles.ToArray();
                    }
                    else
                    {
                        var username = context.User.Identity.Name;
                        var user = Utils.db.Users.FirstOrDefault(u => u.UserName == username);

                        if (user != null)
                        {
                            var roles = new List<ApsUserRole>();

                            foreach (var c in user.Roles)
                            {
                                var role = ApsUserRole.None;

                                if (Enum.TryParse(c.Name, out role) && role != ApsUserRole.None)
                                {
                                    roles.Add(role);
                                }
                            }

                            return roles.ToArray();
                        }
                    }
                }
            }
            else
            {
                var user = Utils.db.Users.Include("Roles").SingleOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    var userRoles = user.Roles.Select(r => r.Name).ToList();

                    var roles = new List<ApsUserRole>();

                    foreach (var r in userRoles)
                    {
                        var role = ApsUserRole.None;

                        if (Enum.TryParse(r, out role) && role != ApsUserRole.None)
                        {
                            roles.Add(role);
                        }
                    }

                    return roles.ToArray();
                }
            }

            return new ApsUserRole[0];
        }

        public static bool IsAdmin(params ApsUserRole[] roles)
        {
            if (roles == null || roles.Length == 0)
            {
                roles = GetUserRoles();
            }

            foreach (var role in roles)
            {
                if (role == ApsUserRole.SystemAdmin)
                    return true;
            }

            return false;
        }

        public static bool IsSystemAdmin(params ApsUserRole[] roles)
        {
            if (roles == null || roles.Length == 0)
            {
                roles = GetUserRoles();
            }

            foreach (var role in roles)
            {
                if (role == ApsUserRole.SystemAdmin)
                    return true;
            }

            return false;
        }

        public static string SerializeObject(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string SerializeObjectBaseTypes(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                ContractResolver = new MyContractResolver()
            };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string CleanObjectNoPunctuation(object obj)
        {
            if (obj == null) return null;

            var str = obj is string ? (string)obj : obj.ToString();

            return CleanString(str, true, true, true, true, true, true, true);
        }

        public static string CleanObject(object obj, bool removeDiacritics = true, bool neutralizeArabic = true, bool removeLeadingSpaces = true, bool removeTrailingSpaces = true, bool removeAllPunctation = false, bool removeRepeatedSpaces = true, bool ifEmptyReturnNull = true)
        {
            if (obj == null) return null;

            var str = obj is string ? (string)obj : obj.ToString();

            return CleanString(str, removeDiacritics, neutralizeArabic, removeLeadingSpaces, removeTrailingSpaces, removeAllPunctation, removeRepeatedSpaces, ifEmptyReturnNull);
        }

        public static string CleanStringNoPunctuation(string str)
        {
            return CleanString(str, true, true, true, true, true, true, true);
        }

        public static string CleanString(string str, bool removeDiacritics = true, bool neutralizeArabic = true, bool removeLeadingSpaces = true, bool removeTrailingSpaces = true, bool removeAllPunctation = false, bool removeRepeatedSpaces = true, bool ifEmptyReturnNull = true)
        {
            if (str == null)
                return null;

            if (removeAllPunctation)
            {
                str = new string(str.Where(c => !char.IsPunctuation(c) && c != ' ' && c != '\t').ToArray());
            }

            if (removeLeadingSpaces)
            {
                str = str.TrimStart();
            }

            if (removeTrailingSpaces)
            {
                str = str.TrimEnd();
            }

            if (removeRepeatedSpaces)
            {
                str = str.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            }

            if (removeDiacritics)
            {
                str = RemoveDiacritics(str);
            }

            if (neutralizeArabic)
            {
                str = str.Replace("أ", "ا").Replace("آ", "ا").Replace("إ", "ا").Replace("ي", "ى").Replace("ؤ", "و").Replace("ءا", "ا");
            }

            if (string.IsNullOrWhiteSpace(str))
            {
                if (ifEmptyReturnNull) return null;
                return "";
            }

            return str;
        }

        public static ApplicationUser CurrentUser
        {
            get
            {
                return UserUtils.CurrentUser;
            }
        }

        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        public static bool GetGosiStatus(ApsDbEntities db)
        {
            var sett = db.Settings.SingleOrDefault(s => s.Key == "IS_GOSI_AVAILABLE");

            return sett != null && sett.Value == "Y";
        }

        public static UserDetails GetCurrentUserDetails(ApsDbEntities db = null)
        {
            if (HttpContextManager.Current == null || HttpContextManager.Current.User == null)
            {
                return new UserDetails
                {
                    Id = "00000000-0000-0000-0000-000000000000",
                    CurrentRole = new RoleDetails { RoleName = ApsUserRole.SystemAdmin.ToString(), Role = ApsUserRole.SystemAdmin },
                    CurrentSession = new UserSession { },
                    UserName = "system",
                    Email = "ecs-info@hrdf.org.sa",
                    FullName = "System Administrator",
                    Roles = new List<RoleDetails>
                    {
                        new RoleDetails
                        {
                            RoleName = ApsUserRole.SystemAdmin.ToString(), Role = ApsUserRole.SystemAdmin
                        }
                    }
                };
            }

            if (HttpContextManager.Current.User == null || HttpContextManager.Current.User.Identity == null) return new UserDetails
            {
                Id = "00000000-0000-0000-0000-000000000000",
                CurrentRole = new RoleDetails { RoleName = ApsUserRole.None.ToString() },
                CurrentSession = new UserSession { },
                UserName = "system",
                Email = "ecs-info@hrdf.org.sa",
                FullName = "System Administrator",
                Roles = new List<RoleDetails>
                    {
                        new RoleDetails
                        {
                            RoleName = ApsUserRole.None.ToString(), Role = ApsUserRole.None
                        }
                    }
            };

            if (db == null)
            {
                db = Utils.db;
            }

            var userId = HttpContextManager.Current.User.Identity.EnhancedGetUserId();
            var _user = (UserDetails)null;
            var tmpUser = db.Users
                            .Include("UserProfile")
                            .Include("Roles")
                            .Where(u => u.Id == userId)
                            .FirstOrDefault();

            if (tmpUser != null)
            {
                if (tmpUser.UserProfile == null)
                {
                    var up = new UserProfile
                    {
                        UserId = tmpUser.Id,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id
                    };

                    db.UserProfiles.Add(up);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                    }

                    tmpUser.UserProfile = up;
                }
                _user = new UserDetails
                {
                    DbUser = tmpUser,
                    Id = tmpUser.Id,
                    FullName = tmpUser.UserProfile != null ? tmpUser.UserProfile.GetFullName() : tmpUser.UserName,
                    Email = tmpUser.Email,
                    UserName = tmpUser.UserName,
                    ForcePasswordChange = tmpUser.ForcePasswordChange,
                    CurrentSession = GetCurrentUserSession(db),
                    Roles = tmpUser.Roles.Select(r => new RoleDetails
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        CollegeId = tmpUser.CollegeId,
                    }).ToList()
                };

                for (var i = 0; i < _user.Roles.Count; i++)
                {
                    var role = ApsUserRole.None;
                    if (!Enum.TryParse<ApsUserRole>(_user.Roles[i].RoleName, true, out role) || role == ApsUserRole.None)
                    {
                        _user.Roles.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        _user.Roles[i].Role = role;

                        _user.CurrentRole = _user.Roles[i];
                    }
                }

                if (_user.CurrentRole == null && _user.Roles.Count > 0)
                {
                    _user.CurrentRole = _user.Roles.OrderByDescending(r => r.Role).First();
                }
            }

            return _user;
        }

        public static long? ResearcherId
        {
            get
            {
                var context = HttpContext.Current;
                var identity = (ClaimsIdentity)context.User.Identity;
                var claim = identity.Claims.FirstOrDefault(r => r.Type == "RESEARCHER_ID");
                if (claim == null) return null;

                return long.Parse(claim.Value);
            }
        }

        public static string Key
        {
            get
            {
                var context = HttpContext.Current;
                var identity = (ClaimsIdentity)context.User.Identity;
                var claim = identity.Claims.FirstOrDefault(r => r.Type == "KEY");
                if (claim == null) return null;

                return claim.Value;
            }
        }

        public static UserDetails GetActualCurrentUserDetails(ApsDbEntities db = null)
        {
            if (HttpContextManager.Current == null || HttpContextManager.Current.User == null)
            {
                return new UserDetails
                {
                    Id = "00000000-0000-0000-0000-000000000000",
                    CurrentRole = new RoleDetails { RoleName = ApsUserRole.SystemAdmin.ToString(), Role = ApsUserRole.SystemAdmin },
                    CurrentSession = new UserSession { },
                    UserName = "system",
                    Email = "ecs-info@hrdf.org.sa",
                    FullName = "System Administrator",
                    Roles = new List<RoleDetails>
                    {
                        new RoleDetails
                        {
                            RoleName = ApsUserRole.SystemAdmin.ToString(), Role = ApsUserRole.SystemAdmin
                        }
                    }
                };
            }

            if (HttpContextManager.Current.User == null || HttpContextManager.Current.User.Identity == null) return new UserDetails
            {
                Id = "00000000-0000-0000-0000-000000000000",
                CurrentRole = new RoleDetails { RoleName = ApsUserRole.None.ToString() },
                CurrentSession = new UserSession { },
                UserName = "system",
                Email = "ecs-info@hrdf.org.sa",
                FullName = "System Administrator",
                Roles = new List<RoleDetails>
                    {
                        new RoleDetails
                        {
                            RoleName = ApsUserRole.None.ToString(), Role = ApsUserRole.None
                        }
                    }
            };

            if (db == null)
            {
                db = Utils.db;
            }

            var _user = (UserDetails)null;
            var tmpUser = db.Users.Find(HttpContextManager.Current.User.Identity.EnhancedGetUserId());

            if (tmpUser != null)
            {
                _user = new UserDetails
                {
                    DbUser = tmpUser,
                    Id = tmpUser.Id,
                    FullName = tmpUser.UserProfile != null ? tmpUser.UserProfile.GetFullName() : tmpUser.UserName,
                    Email = tmpUser.Email,
                    UserName = tmpUser.UserName,
                    ForcePasswordChange = tmpUser.ForcePasswordChange,
                    CurrentSession = GetCurrentUserSession(db),
                    Roles = tmpUser.Roles.Select(r => new RoleDetails
                    {
                        RoleId = r.Id,
                        RoleName = r.Name,
                        CollegeId = tmpUser.CollegeId,
                    }).ToList()
                };

                for (var i = 0; i < _user.Roles.Count; i++)
                {
                    var role = ApsUserRole.None;
                    if (!Enum.TryParse<ApsUserRole>(_user.Roles[i].RoleName, true, out role) || role == ApsUserRole.None)
                    {
                        _user.Roles.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        _user.Roles[i].Role = role;

                        _user.CurrentRole = _user.Roles[i];
                    }
                }

                if (_user.CurrentRole == null && _user.Roles.Count > 0)
                {
                    _user.CurrentRole = _user.Roles.OrderByDescending(r => r.Role).First();
                }
            }

            return _user;
        }

        public static UserSession GetCurrentUserSession(ApsDbEntities db = null)
        {
            if (HttpContextManager.Current == null) return null;

            if (db == null)
            {
                db = Utils.db;
            }

            var _session = HttpContextManager.Current.GetOwinContext().Get<UserSession>(typeof(UserSession).FullName);

            if (_session == null)
            {
                var sessionId = EnhancedGetSessionId();
                _session = db.UserSessions.OrderByDescending(s => s.Created).FirstOrDefault(s => s.Sessionid == sessionId);

                HttpContextManager.Current.GetOwinContext().Set<UserSession>(typeof(UserSession).FullName, _session);
            }

            return _session;
        }

        public static ApplicationSignInManager GetApplicationSignInManager()
        {
            return HttpContextManager.Current.GetOwinContext().Get<ApplicationSignInManager>();
        }

        public static string EnhancedGetSessionId()
        {
            if (HttpContextManager.Current.Items.Contains("is_unit_testing"))
            {
                var db = Utils.db;
                var username = Utils.GetUsername();
                var user = db.Users.FirstOrDefault(u => u.UserName == username);

                if (user == null) return null;

                var sessionClaim = db.UserSessions.OrderBy(c => c.Created).FirstOrDefault(e => e.UserId == user.Id);

                if (sessionClaim != null)
                {
                    return sessionClaim.Sessionid;
                }

                return null;
            }
            else
            {
                return GetSessionId();
            }
        }

        public static string GetSessionId()
        {
            //Get the current claims principal
            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;

            // Get the claims values
            var name = identity.Claims.Where(c => c.Type == "SESSION_ID")
                               .Select(c => c.Value).SingleOrDefault();

            if (name != null) return name;
            return HttpContext.Current.Session.SessionID;
        }

        public static int GenerateShortCode()
        {
            var rand = new Random();

            return rand.Next(1000, 10000);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static NameComponents GetNameComponents(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            str = str.Trim();

            var components = new NameComponents();

            var titles = new string[] { "د.", "د ", "أ.د ", "أ.د/", "دكتور", "الدكتور", "الأستاذ الدكتور", "الأستاذ" };

            foreach (var t in titles)
            {
                if (str.StartsWith(t))
                {
                    components.Title = t.Trim();
                    str = str.Substring(t.Length).Trim();
                    break;
                }
            }

            var tabs = str.Trim().Split(' ').Where(s => s != "" && s != "بن" && s != "بنت").ToArray();

            components.FirstName = tabs[0];

            var startIndex = 1;

            if (tabs.Length > 1 && components.FirstName == "عبد")
            {
                components.FirstName += tabs[1];
                startIndex = 2;
            }
            else if (tabs.Length > 1 && tabs[1] == "الله")
            {
                components.FirstName += " " + tabs[1];
                startIndex = 2;
            }

            if (startIndex < tabs.Length)
            {
                components.SecondName = tabs[startIndex];
                startIndex++;

                if (components.SecondName == "عبد" && tabs.Length > startIndex)
                {
                    components.SecondName += tabs[startIndex];
                    startIndex++;
                }
            }

            if (startIndex < tabs.Length)
            {
                components.ThirdName = tabs[startIndex];
                startIndex++;

                if (components.ThirdName == "عبد" && tabs.Length > startIndex)
                {
                    components.ThirdName += tabs[startIndex];
                    startIndex++;
                }
            }

            if (startIndex < tabs.Length - 1)
            {
                components.LastName = tabs[tabs.Length - 1];

                if (components.LastName == "الله")
                {
                    var prev = tabs[tabs.Length - 2];
                    if (prev == components.ThirdName)
                    {
                        components.LastName = prev + " " + components.LastName;
                        components.ThirdName = null;
                    }
                    else
                    {
                        components.LastName = prev + " " + components.LastName;
                    }
                }
                startIndex = tabs.Length;
            }
            else if (startIndex < tabs.Length)
            {
                components.LastName = tabs[startIndex];
                startIndex++;

                if (components.LastName == "عبد" && tabs.Length > startIndex)
                {
                    components.LastName += tabs[startIndex];
                    startIndex++;
                }
            }

            if (components.LastName == null)
            {
                if (components.ThirdName != null)
                {
                    components.LastName = components.ThirdName;
                    components.ThirdName = null;
                }
                else if (components.SecondName != null)
                {
                    components.LastName = components.SecondName;
                    components.SecondName = null;
                }
            }

            return components;
        }

        public static bool EqualIgnoreDash(string str1, string str2)
        {
            if (str1 == null && str2 == null) return true;
            if (str1 == null || str2 == null) return false;

            str1 = str1.Replace(" ", "").Replace("-", "");
            str2 = str2.Replace(" ", "").Replace("-", "");

            if (str1 == str2) return true;

            if (str1.StartsWith("al"))
                str1 = str1.Substring(2);

            if (str2.StartsWith("al"))
                str2 = str2.Substring(2);

            if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
                return true;
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return false;

            return str1 == str2;
        }

        private static Regex rx_names_arabic = new Regex(@"^[\u0621-\u064A\s]+[\u0621-\u064A\s\-]*[\u0621-\u064A\s]+$", RegexOptions.Compiled);
        private static Regex rx_names_english = new Regex(@"^[a-zA-Z]+[a-zA-Z\-]*[a-zA-Z]+$", RegexOptions.Compiled);

        public static bool IsValidName(string str, bool emptyIsValid = true)
        {
            if (string.IsNullOrWhiteSpace(str)) return emptyIsValid;

            return rx_names_arabic.Match(str).Success || rx_names_english.Match(str).Success;
        }

        public static string GetSpecificExceptionData(Exception ex)
        {
            if (ex == null) return "";

            var str = "";

            if (ex is System.Data.Entity.Validation.DbEntityValidationException)
            {
                var dbex = (System.Data.Entity.Validation.DbEntityValidationException)ex;
                foreach (var er in dbex.EntityValidationErrors)
                {
                    foreach (var er2 in er.ValidationErrors)
                    {
                        str += "\r\n\t" + er2.PropertyName + ": " + er2.ErrorMessage;
                    }
                }
            }
            else if (ex.InnerException != null && ex.InnerException is System.Data.Entity.Validation.DbEntityValidationException)
            {
                var dbex = (System.Data.Entity.Validation.DbEntityValidationException)ex.InnerException;
                foreach (var er in dbex.EntityValidationErrors)
                {
                    foreach (var er2 in er.ValidationErrors)
                    {
                        str += "\r\n\t" + er2.PropertyName + ": " + er2.ErrorMessage;
                    }
                }
            }
            else if (ex.InnerException != null)
            {
                str += ex.InnerException.InnerException.Message;
            }

            str += "\r\n\tSource: " + ex.Source;

            return str;
        }

        public static int GetExceptionLineNumber(Exception ex)
        {
            var lineNumber = 0;
            const string lineSearch = ":line ";
            var index = ex.StackTrace.LastIndexOf(lineSearch);
            if (index != -1)
            {
                var lineNumberText = ex.StackTrace.Substring(index + lineSearch.Length);
                if (int.TryParse(lineNumberText, out lineNumber))
                {
                }
            }
            return lineNumber;
        }

        public static SmtpSettings GetSmtpSettings(ApsDbEntities db = null)
        {
            if (db == null) db = new ApsDbEntities();

            var _smtpSettings = new SmtpSettings();

            var setts = db.Settings.Where(s => s.Key.StartsWith("smtp")).ToList();
            if (ConfigurationManager.AppSettings["smtp:Host"] != null)
            {
                _smtpSettings.Host = ConfigurationManager.AppSettings["smtp:Host"];

                if (setts.Count(s => s.Key.ToLower() == "smtp:host") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:Host",
                        Value = _smtpSettings.Host
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:host") == 1)
            {
                _smtpSettings.Host = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:host").Value;
            }
            else
            {
                _smtpSettings.Host = "10.10.9.28";

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:Host",
                    Value = _smtpSettings.Host
                });
            }

            if (ConfigurationManager.AppSettings["smtp:Port"] != null)
            {
                int port = 0;
                if (int.TryParse(ConfigurationManager.AppSettings["smtp:Port"], out port))
                {
                    _smtpSettings.Port = port;
                }
                else
                {
                    _smtpSettings.Port = 25;
                }

                if (setts.Count(s => s.Key.ToLower() == "smtp:port") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:Port",
                        Value = _smtpSettings.Port.ToString()
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:port") == 1)
            {
                int port = 0;
                if (int.TryParse(setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:port").Value, out port))
                {
                    _smtpSettings.Port = port;
                }
                else
                {
                    _smtpSettings.Port = 25;

                    setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:port").Value = _smtpSettings.Port.ToString();
                }
            }
            else
            {
                _smtpSettings.Port = 25;

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:Port",
                    Value = _smtpSettings.Port.ToString()
                });
            }

            if (ConfigurationManager.AppSettings["smtp:Username"] != null)
            {
                _smtpSettings.Username = ConfigurationManager.AppSettings["smtp:Username"];
                if (setts.Count(s => s.Key.ToLower() == "smtp:username") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:Username",
                        Value = _smtpSettings.Username
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:username") == 1)
            {
                _smtpSettings.Username = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:username").Value;
            }
            else
            {
                _smtpSettings.Username = "ecs-info@hrdf.org.sa";

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:Username",
                    Value = _smtpSettings.Username
                });
            }

            if (ConfigurationManager.AppSettings["smtp:FromEmail"] != null)
            {
                _smtpSettings.FromEmail = ConfigurationManager.AppSettings["smtp:FromEmail"];

                if (setts.Count(s => s.Key.ToLower() == "smtp:fromemail") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:FromEmail",
                        Value = _smtpSettings.FromEmail
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:fromemail") == 1)
            {
                _smtpSettings.FromEmail = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:fromemail").Value;
            }
            else
            {
                _smtpSettings.FromEmail = "ecs-info@hrdf.org.sa";

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:FromEmail",
                    Value = _smtpSettings.FromEmail
                });
            }

            if (ConfigurationManager.AppSettings["smtp:FromName"] != null)
            {
                _smtpSettings.FromName = ConfigurationManager.AppSettings["smtp:FromName"];
                if (setts.Count(s => s.Key.ToLower() == "smtp:fromname") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:FromName",
                        Value = _smtpSettings.FromName
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:fromname") == 1)
            {
                _smtpSettings.FromName = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:fromname").Value;
            }
            else
            {
                _smtpSettings.FromName = "APS";

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:FromName",
                    Value = _smtpSettings.FromName
                });
            }

            if (ConfigurationManager.AppSettings["smtp:Password"] != null)
            {
                _smtpSettings.Password = ConfigurationManager.AppSettings["smtp:Password"];

                if (setts.Count(s => s.Key.ToLower() == "smtp:password") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:Password",
                        Value = _smtpSettings.Password
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:password") == 1)
            {
                _smtpSettings.Password = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:password").Value;
            }
            else
            {
                _smtpSettings.Password = "__SET_ME__";

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:Password",
                    Value = _smtpSettings.Password
                });
            }

            if (ConfigurationManager.AppSettings["smtp:SSL"] != null)
            {
                bool isEncrypted = false;
                if (bool.TryParse(ConfigurationManager.AppSettings["smtp:SSL"], out isEncrypted))
                {
                    _smtpSettings.SSL = isEncrypted;
                }
                else
                {
                    _smtpSettings.SSL = false;
                }

                if (setts.Count(s => s.Key.ToLower() == "smtp:ssl") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:SSL",
                        Value = _smtpSettings.SSL.ToString()
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:ssl") == 1)
            {
                bool isEncrypted = false;
                if (bool.TryParse(setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:ssl").Value, out isEncrypted))
                {
                    _smtpSettings.SSL = isEncrypted;
                }
                else
                {
                    _smtpSettings.SSL = false;

                    setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:ssl").Value = _smtpSettings.SSL.ToString();
                }
            }
            else
            {
                _smtpSettings.SSL = false;

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:SSL",
                    Value = _smtpSettings.SSL.ToString()
                });
            }

            if (ConfigurationManager.AppSettings["smtp:EmailCC"] != null)
            {
                _smtpSettings.EmailCC = ConfigurationManager.AppSettings["smtp:EmailCC"];

                if (setts.Count(s => s.Key.ToLower() == "smtp:emailcc") == 0)
                {
                    db.Settings.Add(new Setting
                    {
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                        Key = "smtp:EmailCC",
                        Value = _smtpSettings.EmailCC
                    });
                }
            }
            else if (setts.Count(s => s.Key.ToLower() == "smtp:emailcc") == 1)
            {
                _smtpSettings.EmailCC = setts.SingleOrDefault(s => s.Key.ToLower() == "smtp:emailcc").Value;
            }
            else
            {
                _smtpSettings.EmailCC = "j.abuasab-c@hrdf.org.sa";

                db.Settings.Add(new Setting
                {
                    Created = DateTime.Now,
                    CreatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Updated = DateTime.Now,
                    UpdatedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                    Key = "smtp:EmailCC",
                    Value = _smtpSettings.EmailCC
                });
            }

            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                Logger.LogExceptionAsync(ex, _smtpSettings);
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, _smtpSettings);
            }

            return _smtpSettings;
        }

        public static bool GetIsDebug()
        {
            return ((CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
        }

        public static bool HasQueryString(string key)
        {
            if (HttpContextManager.Current == null || HttpContextManager.Current.Request == null) return false;

            return HttpContextManager.Current.Request.QueryString[key] != null;
        }

        public static string GetReferrerUrl()
        {
            var context = HttpContextManager.Current;

            //Checking the current context content
            if (context != null && context.Request.UrlReferrer != null)
            {
                return context.Request.UrlReferrer.ToString();
            }
            return "No referrer";
        }

        public static string GetActionName(string fullUrl, out string controller)
        {
            controller = null;
            // Split the url to url + query string
            var questionMarkIndex = fullUrl.IndexOf('?');
            string queryString = null;
            if (questionMarkIndex != -1) // There is a QueryString
            {
                fullUrl = fullUrl.Substring(0, questionMarkIndex);

                queryString = fullUrl.Length <= questionMarkIndex ? "" : fullUrl.Substring(questionMarkIndex + 1);
            }

            // Arranges
            var request = new HttpRequest(null, fullUrl, queryString);
            var response = new HttpResponse(new StringWriter());
            var httpContext = new HttpContext(request, response);

            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));

            // Extract the data
            var values = routeData.Values;
            var controllerName = values["controller"];
            var actionName = values["action"];
            var areaName = values["area"];

            if (actionName != null)
                return actionName.ToString();

            return null;
        }

        public static string GetFullUrl()
        {
            //Return variable declaration
            var appPath = string.Empty;

            //Getting the current context of HTTP request
            var context = HttpContextManager.Current;

            //Checking the current context content
            if (context != null)
            {
                //Formatting the fully qualified website url/name
                appPath = string.Format("{0}://{1}{2}{3}",
                                        context.Request.Url.Scheme,
                                        context.Request.Url.Host,
                                        context.Request.Url.Port == 80 || context.Request.Url.Port == 443
                                            ? string.Empty
                                            : ":" + context.Request.Url.Port,
                                        context.Request.RawUrl);
            }

            return appPath;
        }

        public static string GetLocalizedFullUrl(string lang)
        {
            if (lang == null) return GetFullUrl();

            lang = lang.Trim().ToUpper();

            if (lang != "AR" && lang != "EN") return GetFullUrl();

            //Return variable declaration
            var appPath = string.Empty;

            //Getting the current context of HTTP request
            var context = HttpContextManager.Current;

            //Checking the current context content
            if (context != null)
            {
                var uri = context.Request.Url.AbsolutePath + (context.Request.QueryString.Count > 0 ? "?" : "");
                bool qsStarted = false;
                bool qsLangFound = false;
                foreach (var k in context.Request.QueryString.Keys)
                {
                    if (k == null || !(k is string)) continue;

                    var key = (string)k;
                    var value = (string)null;

                    if (k.ToString().ToUpper() == "LANG")
                    {
                        qsLangFound = true;
                        key = "lang";
                        value = lang.ToLower();
                    }
                    else
                    {
                        value = context.Request.QueryString[key];
                    }

                    if (!qsStarted)
                    {
                        uri += key + "=" + value;
                        qsStarted = true;
                    }
                    else
                    {
                        uri += "&" + key + "=" + value;
                    }
                }

                if (!qsLangFound)
                {
                    if (!qsStarted)
                    {
                        uri += "?lang=" + lang.ToLower();
                        qsStarted = true;
                    }
                    else
                    {
                        uri += "&lang=" + lang.ToLower();
                    }
                }

                //Formatting the fully qualified website url/name
                appPath = string.Format("{0}://{1}{2}{3}",
                                        context.Request.Url.Scheme,
                                        context.Request.Url.Host,
                                        context.Request.Url.Port == 80 || context.Request.Url.Port == 443
                                            ? string.Empty
                                            : ":" + context.Request.Url.Port,
                                        uri);
            }

            return appPath;
        }

        public static string GetApplicationUrl()
        {
            //Return variable declaration
            var appPath = string.Empty;

            if (ConfigurationManager.AppSettings["ApplicationUrl"] != null && ConfigurationManager.AppSettings["ApplicationUrl"].ToLower().StartsWith("http"))
            {
                appPath = ConfigurationManager.AppSettings["ApplicationUrl"];
            }
            else
            {
                //Getting the current context of HTTP request
                var context = HttpContextManager.Current;

                //Checking the current context content
                if (context != null)
                {
                    //Formatting the fully qualified website url/name
                    appPath = string.Format("{0}://{1}{2}{3}",
                                            context.Request.Url.Scheme,
                                            context.Request.Url.Host,
                                            context.Request.Url.Port == 80 || context.Request.Url.Port == 443
                                                ? string.Empty
                                                : ":" + context.Request.Url.Port,
                                            context.Request.ApplicationPath);
                }
                else
                {
                    var isProduction = ConfigurationManager.ConnectionStrings["ApsDB"] != null && ConfigurationManager.ConnectionStrings["ApsDB"].ConnectionString.ToLower().Contains("ecs-prod");

                    if (isProduction)
                    {
                        appPath = "https://ecs.hrdf.org.sa";
                    }
                    else
                    {
                        appPath = "https://ecs.hrdf.org.sa/testenv";
                    }
                }
            }

            if (!appPath.EndsWith("/"))
                appPath += "/";

            return appPath;
        }

        public static string FormatDateTime(DateTime? datetime)
        {
            if (datetime == null) return "";

            return datetime.Value.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture);
        }

        public static DateTime? ToDateTime(this string datetime)
        {
            if (string.IsNullOrEmpty(datetime)) return null;

            var date = DateTime.MaxValue;
            if (DateTime.TryParseExact(datetime, "yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out date))
                return date;
            return null;
        }

        public static string FormatDate(DateTime? datetime)
        {
            if (datetime == null) return "";

            return datetime.Value.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("en-US"));
        }

        public static string FormatTime(DateTime? datetime)
        {
            if (datetime == null) return "";

            return datetime.Value.ToLocalTime().ToString("hh:mm tt", CultureInfo.InvariantCulture);
        }

        public static string GetUsername()
        {
            if (HttpContextManager.Current != null && HttpContextManager.Current.Request != null && HttpContextManager.Current.User != null && HttpContextManager.Current.User.Identity != null && HttpContextManager.Current.User.Identity.IsAuthenticated)
            {
                return HttpContextManager.Current.User.Identity.Name;
            }

            return "NOT_AUTHENTICATED";
        }

        private static Regex rx_digits = new Regex("\\d", RegexOptions.Compiled);
        private static Regex rx_letters = new Regex("[a-zA-Z]", RegexOptions.Compiled);
        private static Regex rx_special_characters = new Regex("\\W", RegexOptions.Compiled);

        public static bool ValidatePassword(string password, bool isAdmin, out string errorMsg)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                errorMsg = "Password is empty";
                return false;
            }

            var missingRequirements = new List<string>();

            password = password.Trim();
            if (password.Contains(" ") || password.Contains("\t"))
            {
                missingRequirements.Add("Cannot contain spaces");
            }

            if (isAdmin)
            {
                if (password.Length < 12)
                {
                    missingRequirements.Add("Must be at least 12 characters");
                }
            }
            else
            {
                if (password.Length < 8)
                {
                    missingRequirements.Add("Must be at least 8 characters");
                }
            }

            if (password.Length > 30)
            {
                missingRequirements.Add("Must not exeed 30 characters");
            }

            if (!rx_digits.Match(password).Success)
            {
                missingRequirements.Add("Must contain digits");
            }

            if (!rx_letters.Match(password).Success)
            {
                missingRequirements.Add("Must contain letters");
            }

            if (!rx_special_characters.Match(password).Success)
            {
                missingRequirements.Add("Must contain special character");
            }

            if (missingRequirements.Count > 0)
            {
                errorMsg = "Password requirements: ";
                foreach (var r in missingRequirements)
                {
                    errorMsg += " - " + r;
                }

                return false;
            }

            errorMsg = null;

            return true;
        }

        public static DateTime? ToHijriDate(long num)
        {
            return ToHijriDate(num.ToString());
        }

        public static DateTime? ToHijriDate(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            str = new string(str.Where(c => char.IsDigit(c)).ToArray());

            if (str.Length != 8) return null;
            var num = (long)0;

            if (!long.TryParse(str, out num) || num <= 0) return null;

            var year = int.Parse(str.Substring(0, 4));
            var month = int.Parse(str.Substring(4, 2));
            var day = int.Parse(str.Substring(6, 2));

            var calendar = new UmAlQuraCalendar();

            try
            {
                if (year > 2000)
                {
                    return new DateTime(year, month, day, 0, 0, 0, 0);
                }
                else
                {
                    return calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
                }
            }
            catch
            {
                if (year < 2000)
                {
                    if (day >= 30)
                    {
                        day = 1;
                        month++;
                        if (month > 12)
                        {
                            month = 1;
                            year++;
                        }
                    }

                    if (day <= 0)
                    {
                        day = 1;
                    }

                    try
                    {
                        return calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
                    }
                    catch
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        public static string GetUserIP()
        {
            string strIP = String.Empty;
            if (HttpContextManager.Current == null || HttpContextManager.Current.Request == null) return null;

            var httpReq = HttpContextManager.Current.Request;

            //test for non-standard proxy server designations of client's IP
            if (httpReq.ServerVariables["HTTP_CLIENT_IP"] != null)
            {
                strIP = httpReq.ServerVariables["HTTP_CLIENT_IP"].ToString();
            }
            else if (httpReq.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            {
                strIP = httpReq.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }
            //test for host address reported by the server
            else if
            (
                //if exists
                (httpReq.UserHostAddress.Length != 0)
                &&
                //and if not localhost IPV6 or localhost name
                ((httpReq.UserHostAddress != "::1") || (httpReq.UserHostAddress != "localhost"))
            )
            {
                strIP = httpReq.UserHostAddress;
            }
            //finally, if all else fails, get the IP from a web scrape of another server
            else
            {
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    strIP = sr.ReadToEnd();
                }
                //scrape ip from the html
                int i1 = strIP.IndexOf("Address: ") + 9;
                int i2 = strIP.LastIndexOf("</body>");
                strIP = strIP.Substring(i1, i2 - i1);
            }
            return strIP;
        }

        public static UsersManager UserUtils
        {
            get
            {
                return Utils.GetInstance<UsersManager>();
            }
        }

        public static RequestsManager RequestUtils
        {
            get
            {
                return Utils.GetInstance<RequestsManager>();
            }
        }

        public static ExportManager ExportUtils
        {
            get
            {
                return Utils.GetInstance<ExportManager>();
            }
        }

        public static TemplatesManager TemplateUtils
        {
            get
            {
                return Utils.GetInstance<TemplatesManager>();
            }
        }

        public static CommonManager CommonUtils
        {
            get
            {
                return Utils.GetInstance<CommonManager>();
            }
        }

        public static bool IsLocalhost
        {
            get
            {
                return ConfigurationManager.AppSettings["IsLocalhost"] != null && ConfigurationManager.AppSettings["IsLocalhost"].ToLower().Trim() == "true";
            }
        }

        public static bool CalculateLuhnAlgorithm(string number)
        {
            if (number.Length % 2 != 0)
                return false; // Or maybe throw an exception?

            int sumOfAllValues = 0;
            //odd numbers minus the check Digit.
            for (int i = 0; i < number.Length; i++)
            {
                if (i % 2 != 0) // because i is 0 based instead of 1 based.
                    sumOfAllValues += (int)char.GetNumericValue(number[i]);
                else
                {
                    int number2 = (int)char.GetNumericValue(number[i]) * 2;
                    if (number2 >= 10)
                    {
                        number2 = (number2 % 10) + 1;
                    }
                    sumOfAllValues += number2;
                }
            }

            // get the luhn validity
            return (sumOfAllValues % 10) == 0;
        }

        public static double? ConvertStringToDouble(string value)
        {
            var strTrimmed = value.Trim().Split(' ');
            var strFloat = (double)0;
            foreach (var t in strTrimmed)
            {
                if (double.TryParse(t, out strFloat)) return strFloat;
            }
            return null;
        }

        public static bool UseFakeServices
        {
            get
            {
                return ConfigurationManager.AppSettings["UseFakeServices"] != null && ConfigurationManager.AppSettings["UseFakeServices"].ToLower() == "true";
            }
        }

        public static DateTime? ObjectToDate(object obj)
        {
            if (obj == null) return null;
            if (obj is DateTime) return ((DateTime)obj);
            if (obj is DateTime?) return (DateTime?)obj;

            return StringToDate(obj.ToString());
        }

        public static DateTime? SheetCellToDate(ExcelRange cell)
        {
            if (cell == null) return null;

            cell.Style.Numberformat.Format = "yyyy-mm-dd";

            if (cell.Value is DateTime)
            {
                return (DateTime)cell.Value;
            }
            else
            {
                return StringToDate(cell.Text);
            }
        }

        public static DateTime? StringToDate(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            var hijriDate = ToHijriDate(str);
            if (hijriDate != null) return hijriDate;

            str = str.Trim();

            var date = DateTime.MinValue;

            if (!DateTime.TryParseExact(str, "yyyy-M-d", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
            {
                if (!DateTime.TryParseExact(str, "yyyy-MM-dd", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                {
                    if (!DateTime.TryParseExact(str, "M/d/yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                    {
                        if (!DateTime.TryParseExact(str, "MM/dd/yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                        {
                            if (!DateTime.TryParseExact(str, "dd/MM/yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                            {
                                if (!DateTime.TryParseExact(str, "d/M/yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                                {
                                    if (!DateTime.TryParseExact(str, "d-M-yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                                    {
                                        if (!DateTime.TryParseExact(str, "dd-MM-yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out date))
                                        {
                                            return null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (date != DateTime.MinValue)
                return date;

            return null;
        }

        public static bool? StringToBoolean(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            str = str.Trim().ToUpper();

            switch (str)
            {
                case "Y":
                case "YES":
                case "1":
                case "JWD":
                    return true;

                case "N":
                case "NO":
                case "0":
                case "NON-JWD":
                case "NONJWD":
                    return false;
            }

            return null;
        }

        public static string StringToGender(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;

            str = str.Trim().ToUpper();

            switch (str)
            {
                case "M":
                case "MALE":
                case "MEN":
                case "MAN":
                    return "M";

                case "F":
                case "FEMALE":
                case "WOMAN":
                case "WOMEN":
                    return "F";
            }

            return null;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string RootPath
        {
            get
            {
                var _rootPath = (string)null;
                for (var i = 1; i < 5; i++)
                {
                    var append = "";
                    for (var j = 0; j < i; j++)
                        append += "\\..";

                    var candFile = AssemblyDirectory + append + "\\APS.Web\\";
                    if (Directory.Exists(candFile))
                    {
                        _rootPath = AssemblyDirectory + append + "\\APS.Web";
                        break;
                    }
                }

                return _rootPath;
            }
        }

        public static string MapPath(string relPath)
        {
            if (HttpContextManager.Current != null && HttpContextManager.Current.Server != null)
                return HttpContextManager.Current.Server.MapPath(relPath);
            else
            {
                var p = System.Web.Hosting.HostingEnvironment.MapPath(relPath);

                if (p != null) return p;

                if (relPath.StartsWith("~")) relPath = relPath.Substring(1).Replace("/", "\\");

                return RootPath + relPath;
            }
        }

        public static dynamic GetImpersonationRoles()
        {
            if (HasActualRolesAny(ApsUserRole.SystemAdmin))
            {
                return Enum.GetValues(typeof(ApsUserRole)).Cast<ApsUserRole>().Where(r => r != ApsUserRole.None).OrderBy(r => r.ToString()).ToList();
            }

            return new List<ApsUserRole>();
        }

        public static bool IsCollegeRole(params ApsUserRole[] roles)
        {
            if (roles == null || roles.Length == 0) roles = GetUserRoles();

            if (roles.Count(role => role.ToString().StartsWith("ResearchUnit")) > 0) return true;

            return false;
        }

        public static List<string> GetCollegeRoles()
        {
            var roles = Utils.db.Roles
                .Where(r => r.Name.StartsWith("ResearchUnit"))
                .Select(r => r.Id)
                .ToList();

            return roles;
        }

        public static bool IsMajorRole(params ApsUserRole[] roles)
        {
            if (roles == null || roles.Length == 0) roles = GetUserRoles();

            if (roles.Count(role => role.ToString().Contains("Referee")) > 0) return true;

            return false;
        }

        public static SmsProvider PreferredSmsProvider
        {
            get
            {
                var pref = GetDictVal("PreferredSmsProvider");

                if (pref != null) return pref;
                var setts = Utils.db.Settings.Where(s => s.Key.StartsWith("sms")).ToList();

                if (setts.Count(s => s.Key.ToLower() == "sms:type") == 1)
                {
                    var type = SmsProvider.MalathSMS;

                    if (Enum.TryParse(setts.SingleOrDefault(s => s.Key.ToLower() == "sms:type").Value, true, out type))
                    {
                        SetDictVal("PreferredSmsProvider", type);
                        return type;
                    }
                }
                else if (ConfigurationManager.AppSettings["sms:type"] != null)
                {
                    var type = SmsProvider.MalathSMS;

                    if (Enum.TryParse(ConfigurationManager.AppSettings["sms:type"], true, out type))
                    {
                        SetDictVal("PreferredSmsProvider", type);
                        return type;
                    }
                }
                SetDictVal("PreferredSmsProvider", SmsProvider.MobilyWS);

                return SmsProvider.MobilyWS;
            }
        }

        public static College GetCollegeDetails(int collegeId)
        {
            var v = Utils.GetDictVal("ALL_COLLEGES");
            var _colleges = new List<College>();
            if (v != null)
            {
                _colleges = (List<College>)v;
            }
            else
            {
                _colleges = Utils.db.Colleges.ToList();
            }

            return _colleges.Where(x => x.Id == collegeId).FirstOrDefault();
        }

        public static SmsSettings MalathSmsSettings
        {
            get
            {
                var userId = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id;

                var DbContext = Utils.db;
                var _malathSmsSettings = GetDictVal("MalathSmsSettings");

                if (_malathSmsSettings == null)
                {
                    _malathSmsSettings = new SmsSettings();

                    var setts = DbContext.Settings.Where(s => s.Key.StartsWith("sms")).ToList();

                    if (setts.Count(s => s.Key.ToLower() == "sms:type") == 1)
                    {
                        var type = SmsProvider.MalathSMS;

                        if (Enum.TryParse(setts.SingleOrDefault(s => s.Key.ToLower() == "sms:type").Value, true, out type))
                        {
                            _malathSmsSettings.Type = type;
                        }
                    }
                    else if (ConfigurationManager.AppSettings["sms:type"] != null)
                    {
                        var type = SmsProvider.MalathSMS;

                        if (Enum.TryParse(ConfigurationManager.AppSettings["sms:type"], true, out type))
                        {
                            _malathSmsSettings.Type = type;
                        }

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:type",
                            Value = _malathSmsSettings.Type.ToString()
                        });
                    }
                    else
                    {
                        _malathSmsSettings.Type = SmsProvider.MalathSMS;

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:type",
                            Value = _malathSmsSettings.Type.ToString(),
                        });
                    }

                    if (setts.Count(s => s.Key.ToLower() == "sms:sender:malath") == 1)
                    {
                        _malathSmsSettings.Sender = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:sender:malath").Value;
                    }
                    else if (ConfigurationManager.AppSettings["sms:sender:malath"] != null)
                    {
                        _malathSmsSettings.Sender = ConfigurationManager.AppSettings["sms:sender:malath"];

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:sender:malath",
                            Value = _malathSmsSettings.Sender
                        });
                    }
                    else
                    {
                        //_malathSmsSettings.Sender = "";
                        _malathSmsSettings.Sender = "omarss";

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:sender:malath",
                            Value = _malathSmsSettings.Sender
                        });
                    }

                    if (setts.Count(s => s.Key.ToLower() == "sms:username:malath") == 1)
                    {
                        _malathSmsSettings.Username = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:username:malath").Value;
                    }
                    else if (ConfigurationManager.AppSettings["sms:username:malath"] != null)
                    {
                        _malathSmsSettings.Username = ConfigurationManager.AppSettings["sms:username:malath"];

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:username:malath",
                            Value = _malathSmsSettings.Username
                        });
                    }
                    else
                    {
                        _malathSmsSettings.Username = "omarshaaban";

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:username:malath",
                            Value = _malathSmsSettings.Username
                        });
                    }

                    if (setts.Count(s => s.Key.ToLower() == "sms:password:malath") == 1)
                    {
                        _malathSmsSettings.Password = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:password:malath").Value;
                    }
                    else if (ConfigurationManager.AppSettings["sms:password:malath"] != null)
                    {
                        _malathSmsSettings.Password = ConfigurationManager.AppSettings["sms:password:malath"];

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:password:malath",
                            Value = _malathSmsSettings.Password
                        });
                    }
                    else
                    {
                        _malathSmsSettings.Password = "__SET_ME__";

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:password:malath",
                            Value = _malathSmsSettings.Password
                        });
                    }

                    try
                    {
                        DbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogExceptionAsync(ex, _malathSmsSettings);
                    }
                }

                SetDictVal("MalathSmsSettings", _malathSmsSettings);

                return _malathSmsSettings;
            }
        }

        public static SmsSettings MobilySmsSettings
        {
            get
            {
                var userId = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id;
                var DbContext = Utils.db;
                var _mobilySmsSettings = GetDictVal("MobilySmsSettings");

                if (_mobilySmsSettings == null)
                {
                    _mobilySmsSettings = new SmsSettings();

                    var setts = DbContext.Settings.Where(s => s.Key.StartsWith("sms")).ToList();

                    if (setts.Count(s => s.Key.ToLower() == "sms:type") == 1)
                    {
                        var type = SmsProvider.MalathSMS;

                        if (Enum.TryParse(setts.SingleOrDefault(s => s.Key.ToLower() == "sms:type").Value, true, out type))
                        {
                            _mobilySmsSettings.Type = type;
                        }
                    }
                    else if (ConfigurationManager.AppSettings["sms:type"] != null)
                    {
                        var type = SmsProvider.MalathSMS;

                        if (Enum.TryParse(ConfigurationManager.AppSettings["sms:type"], true, out type))
                        {
                            _mobilySmsSettings.Type = type;
                        }

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:type",
                            Value = _mobilySmsSettings.Type.ToString()
                        });
                    }
                    else
                    {
                        _mobilySmsSettings.Type = SmsProvider.MalathSMS;

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:type",
                            Value = _mobilySmsSettings.Type.ToString()
                        });
                    }

                    if (setts.Count(s => s.Key.ToLower() == "sms:sender:mobily") == 1)
                    {
                        _mobilySmsSettings.Sender = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:sender:mobily").Value;
                    }
                    else if (ConfigurationManager.AppSettings["sms:sender:mobily"] != null)
                    {
                        _mobilySmsSettings.Sender = ConfigurationManager.AppSettings["sms:sender:mobily"];

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:sender:mobily",
                            Value = _mobilySmsSettings.Sender
                        });
                    }
                    else
                    {
                        _mobilySmsSettings.Sender = "ECS";

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:sender:mobily",
                            Value = _mobilySmsSettings.Sender
                        });
                    }

                    if (setts.Count(s => s.Key.ToLower() == "sms:username:mobily") == 1)
                    {
                        _mobilySmsSettings.Username = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:username:mobily").Value;
                    }
                    else if (ConfigurationManager.AppSettings["sms:username:mobily"] != null)
                    {
                        _mobilySmsSettings.Username = ConfigurationManager.AppSettings["sms:username:mobily"];

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:username:mobily",
                            Value = _mobilySmsSettings.Username
                        });
                    }
                    else
                    {
                        _mobilySmsSettings.Username = "tmakeen-new";

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:username:mobily",
                            Value = _mobilySmsSettings.Username
                        });
                    }

                    if (setts.Count(s => s.Key.ToLower() == "sms:password:mobily") == 1)
                    {
                        _mobilySmsSettings.Password = setts.SingleOrDefault(s => s.Key.ToLower() == "sms:password:mobily").Value;
                    }
                    else if (ConfigurationManager.AppSettings["sms:password:mobily"] != null)
                    {
                        _mobilySmsSettings.Password = ConfigurationManager.AppSettings["sms:password:mobily"];

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:password:mobily",
                            Value = _mobilySmsSettings.Password
                        });
                    }
                    else
                    {
                        _mobilySmsSettings.Password = "__SET_ME__";

                        DbContext.Settings.Add(new Setting
                        {
                            Created = DateTime.Now,
                            CreatedBy = userId,
                            Updated = DateTime.Now,
                            UpdatedBy = userId,
                            Key = "sms:password:mobily",
                            Value = _mobilySmsSettings.Password
                        });
                    }

                    try
                    {
                        DbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogExceptionAsync(ex, _mobilySmsSettings);
                    }
                }

                SetDictVal("MobilySmsSettings", _mobilySmsSettings);

                return _mobilySmsSettings;
            }
        }

        public static bool IsDepartmentRole(ApsUserRole role)
        {
            return false;
        }

        public static bool IsCollegeUser
        {
            get
            {
                return Utils.HasRolesAny(ApsUserRole.ResearchUnitCouncilMember, ApsUserRole.ResearchUnitHead, ApsUserRole.ResearchUnitScretary) && RequestUtils.CurrentUser.CollegeId != null;
            }
        }

        public static bool IsMajorUser
        {
            get
            {
                return Utils.HasRolesAny(ApsUserRole.Referee) && RequestUtils.CurrentUser.MajorId != null;
            }
        }

        public static bool TimeMachineEnabled
        {
            get
            {
                return ConfigurationManager.AppSettings["TimeMachineEnabled"] != null && ConfigurationManager.AppSettings["TimeMachineEnabled"].ToLower().Trim() == "true";
            }
        }

        public static bool TimeMachineEffective
        {
            get
            {
                var isEnabled = TimeMachineEnabled;

                if (!isEnabled) return false;

                var realTime = DateTime.Now;
                var tmTime = TM.Now;
                if (realTime.Year != tmTime.Year || realTime.Month != tmTime.Month || realTime.Day != tmTime.Day || realTime.Hour != tmTime.Hour || realTime.Minute != tmTime.Minute)
                    return true;

                return false;
            }
        }

        public static List<NameModel> GetColleges()
        {
            var majors = new List<NameModel>();
            var IsArabic = RequestUtils.Preferences.Language != LanguagePreference.English;

            majors = RequestUtils.DbContext.Colleges.Select(r => new NameModel
            {
                Id = r.Id,
                Name = (IsArabic ? r.NameArabic : r.NameEnglish)
            }).OrderBy(p => p.Name).ToList();

            return majors;
        }

        public static List<NameModel> GetMajors()
        {
            var majors = new List<NameModel>();
            var IsArabic = RequestUtils.Preferences.Language != LanguagePreference.English;

            majors = RequestUtils.DbContext.Majors.Select(r => new NameModel
            {
                Id = r.Id,
                Name = (IsArabic ? r.NameArabic : r.NameEnglish)
            }).OrderBy(p => p.Name).ToList();

            return majors;
        }

        public static List<NameModel> GetDepartments(bool allowOtherDepartments = false)
        {
            var channels = new List<NameModel>();
            var IsArabic = RequestUtils.Preferences.Language != LanguagePreference.English;
            if (!IsCollegeUser || allowOtherDepartments)
            {
                channels = RequestUtils.DbContext.Departments.Select(r => new NameModel
                {
                    Id = r.Id,
                    Name = (IsArabic ? r.NameArabic : r.NameEnglish)
                }).OrderBy(p => p.Name).ToList();
            }

            return channels;
        }

        public static List<NameModel> GetDepartments(long? collegeId = null, bool allowOtherColleges = false)
        {
            var jpcs = new List<NameModel>();

            if (!RequestUtils.IsCollegeUser)
            {
                jpcs = RequestUtils.DbContext.Departments.Where(j => (collegeId == null || j.CollegeId == collegeId.Value)).Select(r => new NameModel
                {
                    Id = r.Id,
                    Name = RequestUtils.IsArabic ? r.NameArabic : r.NameEnglish
                }).OrderBy(p => p.Name).ToList();
            }
            else
            {
                allowOtherColleges = collegeId != null;

                jpcs = RequestUtils.DbContext.Departments.Where(j => ((allowOtherColleges && j.CollegeId == collegeId) || (!allowOtherColleges && j.CollegeId == RequestUtils.CurrentUser.CollegeId))).Select(r => new NameModel
                {
                    Id = r.Id,
                    Name = RequestUtils.IsArabic ? r.NameArabic : r.NameEnglish
                }).OrderBy(p => p.Name).ToList();
            }

            return jpcs;
        }

        public static string PascalCaseToUnderscore(string str)
        {
            var result = Regex.Replace(str, "(?<=[a-z0-9])[A-Z]", m => "_" + m.Value);
            result = result.ToLowerInvariant();

            return result;
        }

        public static string NormalizeString(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                str = str.Replace('أ', 'ا');
                str = str.Replace('إ', 'ا');
                str = str.Replace('آ', 'ا');
                str = str.Replace('ة', 'ه');
                str = str.Replace('ي', 'ى');
                str = str.Replace(" ", string.Empty);
                str = str.Replace("\t", string.Empty);
            }
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(str))
            {
                foreach (char c in str)
                {
                    if (!char.IsPunctuation(c))
                        sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static bool ValidateIsOfficialEmail(string email)
        {
            var isOfficial = rx_Officialemail.Match(email).Success;
            return isOfficial;
        }

        /// <summary>
        /// Converts a List&lt;T&gt; to a DataTable.
        /// </summary>
        /// <typeparam name="T">The type of the list collection.</typeparam>
        /// <param name="list">List instance reference.</param>
        /// <returns>A DataTable of the converted list collection.</returns>
        public static DataTable ToDataTable<T>(this List<T> list)
        {
            var entityType = typeof(T);

            // Lists of type System.String and System.Enum (which includes enumerations and structs) must be handled differently
            // than primitives and custom objects (e.g. an object that is not type System.Object).
            if (entityType == typeof(String))
            {
                var dataTable = new DataTable(entityType.Name);
                dataTable.Columns.Add(entityType.Name);

                // Iterate through each item in the list. There is only one cell, so use index 0 to set the value.
                foreach (T item in list)
                {
                    var row = dataTable.NewRow();
                    row[0] = item;
                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }
            else if (entityType.BaseType == typeof(Enum))
            {
                var dataTable = new DataTable(entityType.Name);
                dataTable.Columns.Add(entityType.Name);

                // Iterate through each item in the list. There is only one cell, so use index 0 to set the value.
                foreach (string namedConstant in Enum.GetNames(entityType))
                {
                    var row = dataTable.NewRow();
                    row[0] = namedConstant;
                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }

            // Check if the type of the list is a primitive type or not. Note that if the type of the list is a custom
            // object (e.g. an object that is not type System.Object), the underlying type will be null.
            var underlyingType = Nullable.GetUnderlyingType(entityType);
            var primitiveTypes = new List<Type>
        {
            typeof (Byte),
            typeof (Char),
            typeof (Decimal),
            typeof (Double),
            typeof (Int16),
            typeof (Int32),
            typeof (Int64),
            typeof (SByte),
            typeof (Single),
            typeof (UInt16),
            typeof (UInt32),
            typeof (UInt64),
        };

            var typeIsPrimitive = primitiveTypes.Contains(underlyingType);

            // If the type of the list is a primitive, perform a simple conversion.
            // Otherwise, map the object's properties to columns and fill the cells with the properties' values.
            if (typeIsPrimitive)
            {
                var dataTable = new DataTable(underlyingType.Name);
                dataTable.Columns.Add(underlyingType.Name);

                // Iterate through each item in the list. There is only one cell, so use index 0 to set the value.
                foreach (T item in list)
                {
                    var row = dataTable.NewRow();
                    row[0] = item;
                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }
            else
            {
                // TODO:
                // 1. Convert lists of type System.Object to a data table.
                // 2. Handle objects with nested objects (make the column name the name of the object and print "system.object" as the value).

                var dataTable = new DataTable(entityType.Name);
                var propertyDescriptorCollection = TypeDescriptor.GetProperties(entityType);

                // Iterate through each property in the object and add that property name as a new column in the data table.
                foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
                {
                    // Data tables cannot have nullable columns. The cells can have null values, but the actual columns themselves cannot be nullable.
                    // Therefore, if the current property type is nullable, use the underlying type (e.g. if the type is a nullable int, use int).
                    var propertyType = Nullable.GetUnderlyingType(propertyDescriptor.PropertyType) ?? propertyDescriptor.PropertyType;
                    dataTable.Columns.Add(propertyDescriptor.Name, propertyType);
                }

                // Iterate through each object in the list adn add a new row in the data table.
                // Then iterate through each property in the object and add the property's value to the current cell.
                // Once all properties in the current object have been used, add the row to the data table.
                foreach (T item in list)
                {
                    var row = dataTable.NewRow();

                    foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
                    {
                        var value = propertyDescriptor.GetValue(item);
                        row[propertyDescriptor.Name] = value ?? DBNull.Value;
                    }

                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }
        }

        public static string Encrypt(string source)
        {
            var key = Constants.EncryptionSecurityKey;

            var desCryptoProvider = new TripleDESCryptoServiceProvider();
            var hashMD5Provider = new MD5CryptoServiceProvider();

            byte[] byteHash;
            byte[] byteBuff;

            byteHash = hashMD5Provider.ComputeHash(Encoding.UTF8.GetBytes(key));
            desCryptoProvider.Key = byteHash;
            desCryptoProvider.Mode = CipherMode.ECB; //CBC, CFB
            byteBuff = Encoding.UTF8.GetBytes(source);
            var encoded = Convert.ToBase64String(desCryptoProvider.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            return encoded;
        }

        public static string Decrypt(string encodedText)
        {
            var key = Constants.EncryptionSecurityKey;

            var desCryptoProvider = new TripleDESCryptoServiceProvider();
            var hashMD5Provider = new MD5CryptoServiceProvider();

            byte[] byteHash;
            byte[] byteBuff;

            byteHash = hashMD5Provider.ComputeHash(Encoding.UTF8.GetBytes(key));
            desCryptoProvider.Key = byteHash;
            desCryptoProvider.Mode = CipherMode.ECB; //CBC, CFB
            byteBuff = Convert.FromBase64String(encodedText);

            var plaintext = Encoding.UTF8.GetString(desCryptoProvider.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));

            return plaintext;
        }

        public static dynamic GetDictVal(string key)
        {
            if (HasOwinContext)
            {
                return HttpContextManager.Current.GetOwinContext().Get<dynamic>(key);
            }
            else
            {
                if (_staticObjs == null) _staticObjs = new Dictionary<string, dynamic>();

                if (_staticObjs.ContainsKey(key))
                {
                    return _staticObjs[key];
                }
            }

            return null;
        }

        public static void SetDictVal(string key, dynamic value)
        {
            if (HasOwinContext)
            {
                HttpContext.Current.GetOwinContext().Set<dynamic>(key, value);
            }
            else
            {
                if (_staticObjs == null) _staticObjs = new Dictionary<string, dynamic>();

                if (_staticObjs.ContainsKey(key))
                {
                    _staticObjs[key] = value;
                }
                else
                {
                    _staticObjs.Add(key, value);
                }
            }
        }

        public static string GetRegisterLink(ApsUserRole role, int? step = null)
        {
            if (role != ApsUserRole.Referee && role != ApsUserRole.Researcher)
            {
                role = ApsUserRole.Researcher;
            }
            return GetApplicationUrl() + "Account/Register?role=" + role.ToString() + "&step=" + step;
        }

        public static List<QualificationType> QualificationTypesByResearcherType(ResearcherRole? type = null, ApsUserRole? role = null)
        {
            if (type != null)
            {
                switch (type)
                {
                    case ResearcherRole.MainResearcher:
                    case ResearcherRole.AssociateResearcher:
                    case ResearcherRole.Consultant:
                        {
                            return new List<QualificationType>
                            {
                                QualificationType.Professor,
                                QualificationType.AssociateProfessor,
                                QualificationType.AssistantProfessor,
                            };
                        }
                    case ResearcherRole.AssistantResearcher:
                        {
                            return new List<QualificationType>
                                {
                                        QualificationType.Masters,
                                        QualificationType.Bachelors,
                                        QualificationType.UndergraduateStudent,
                                        QualificationType.MasterStudent,
                                        QualificationType.PhdStudent,
                                        QualificationType.Technician,
                                        QualificationType.Vocational
                                };
                        }
                }
            }
            else if (role == ApsUserRole.Referee)
            {
                return new List<QualificationType>
                            {
                                QualificationType.Professor,
                                QualificationType.AssociateProfessor,
                                QualificationType.AssistantProfessor,
                            };
            }

            return new List<QualificationType>
                            {
                                QualificationType.Professor,
                                QualificationType.AssociateProfessor,
                                QualificationType.AssistantProfessor,
                                QualificationType.Masters,
                                QualificationType.Bachelors,
                                QualificationType.UndergraduateStudent,
                                QualificationType.MasterStudent,
                                QualificationType.PhdStudent,
                                QualificationType.Diploma,
                            };
        }

        public static string GetCollegeName(string userId = null, int? collegeId = null)
        {
            string collegeName = "";
            if (collegeId == null)
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return collegeName;
                }
                else
                {
                    collegeId = db.Users.Where(u => u.Id == userId).First().CollegeId;
                }
            }

            if (collegeId > 0)
            {
                collegeName = db.Colleges.FirstOrDefault(c => c.Id == collegeId).NameArabic;
            }

            return collegeName;
        }

        public static DateTime ConvertDate(bool Hijri, bool Gregorian, string DateTimeToConvert, string format = "yyyy-MM-dd")
        {
            DateTime Date = DateTime.Now;
            if (Hijri)
            {
                CultureInfo enCI = new CultureInfo("en-US");
                Date = DateTime.ParseExact(DateTimeToConvert, format, enCI.DateTimeFormat, DateTimeStyles.AllowInnerWhite);
            }
            if (Gregorian)
            {
                CultureInfo arCI = new CultureInfo("ar-SA");
                Date = DateTime.ParseExact(DateTimeToConvert, format, arCI.DateTimeFormat, DateTimeStyles.AllowInnerWhite);
            }
            return Date;
        }

        public static SendGrid.Helpers.Mail.Attachment GetSendGridAttachment(this System.Net.Mail.Attachment attachment)
        {
            using (var stream = new MemoryStream())
            {
                try
                {
                    attachment.ContentStream.CopyTo(stream);
                    return new SendGrid.Helpers.Mail.Attachment()
                    {
                        Disposition = "attachment",
                        Type = attachment.ContentType.MediaType,
                        Filename = attachment.Name,
                        ContentId = attachment.ContentId,
                        Content = Convert.ToBase64String(stream.ToArray())
                    };
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        public static ApsUserRole GetActiveUserRole()
        {
            var UserActiveRole = GetCurrentUserSession().CurrentRole == null ? ApsUserRole.None : (ApsUserRole)GetCurrentUserSession().CurrentRole;
            if (UserActiveRole == ApsUserRole.None)
                UserActiveRole = GetUserRole();
            return UserActiveRole;
        }
    }
}