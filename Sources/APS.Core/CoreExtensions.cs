using APS.Data;
using APS.Data.DbModels;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace APS.Core
{
    public static class CoreExtensions
    {
        public static string FormatDate(this DateTime date, bool includeTime = false, bool ArabicFormat = false, bool includeSuffix = false)
        {
            if (date == default(DateTime)) return "";

            if (!includeTime)
                return date.ToString("yyyy-MM-dd'" + (!includeSuffix ? "" : (ArabicFormat ? (Utils.IsUserLanguageArabic ? "هـ" : "H") : (Utils.IsUserLanguageArabic ? "م" : "AD"))) + "'", CultureInfo.GetCultureInfo(ArabicFormat ? "ar-SA" : "en-US"));
            else
                return date.ToString("yyyy-MM-dd'" + (!includeSuffix ? "" : (ArabicFormat ? (Utils.IsUserLanguageArabic ? "هـ" : "H") : (Utils.IsUserLanguageArabic ? "م" : "AD"))) + "' hh:MM tt", CultureInfo.GetCultureInfo(ArabicFormat ? "ar-SA" : "en-US"));
        }

        public static void EnhancedSaveAs(this HttpPostedFileBase file, string newFilePath)
        {
            using (FileStream fileToSave = new FileStream(newFilePath, FileMode.Create))
            {
                file.InputStream.CopyTo(fileToSave);
            }
        }

        public static string SpecialToString(this object obj)
        {
            if (obj == null) return "";

            if (obj is DateTime || obj is DateTime?)
                return ((DateTime)obj).ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.GetCultureInfo("en-US"));
            return obj.ToString();
        }

        public static string EnhancedGetUserId(this IIdentity p)
        {
            if (HttpContextManager.Current.Items.Contains("is_unit_testing"))
            {
                var db = Utils.db;
                var username = Utils.GetUsername();
                var user = db.Users.FirstOrDefault(u => u.UserName == username);

                if (user == null) return null;

                return user.Id;
            }
            else
            {
                return p.GetUserId();
            }
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }

        public static IList<T> ChainAdd<T>(this IList<T> list, T value)
        {
            list.Add(value);
            return list;
        }

        public static void DiscardChanges(this DbContext dbContext)
        {
            foreach (var entry in dbContext.ChangeTracker.Entries())
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified; //Revert changes made to deleted entity.
                        entry.State = EntityState.Unchanged;
                        break;

                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                }
            }
        }

        public static string GetFullName(this UserProfile user)
        {
            if (user == null) return "";

            if (user.FirstName == null && user.LastName == null)
            {
                if (string.IsNullOrWhiteSpace(user.User.FullName))
                {
                    return user.User.UserName;
                }
                else
                {
                    return user.User.FullName;
                }
            }

            if (user.MiddleName == null) user.MiddleName = "";
            if (user.ThirdName == null) user.ThirdName = "";

            return user.FirstName + (string.IsNullOrWhiteSpace(user.MiddleName) ? "" : " " + user.MiddleName.Trim()) + (string.IsNullOrWhiteSpace(user.ThirdName) ? "" : " " + user.ThirdName.Trim()) + (string.IsNullOrWhiteSpace(user.LastName) ? "" : " " + user.LastName.Trim());
        }

        public static string GetFullName(this Researcher user)
        {
            if (user == null) return "";

            if (user.FirstName == null || user.LastName == null)
            {
                if (user.FirstName != null) return user.FirstName;
                if (user.LastName != null) return user.LastName;

                if (user.User != null) return user.User.UserName;

                return "";
            }

            if (user.SecondName == null) user.SecondName = "";
            if (user.ThirdName == null) user.ThirdName = "";

            return user.FirstName + (string.IsNullOrWhiteSpace(user.SecondName) ? "" : " " + user.SecondName.Trim()) + (string.IsNullOrWhiteSpace(user.ThirdName) ? "" : " " + user.ThirdName.Trim()) + (string.IsNullOrWhiteSpace(user.LastName) ? "" : " " + user.LastName.Trim());
        }

        public static string GetFullName(this Referee referee)
        {
            if (referee == null) return "";

            if (referee.FirstName == null || referee.LastName == null)
            {
                if (referee.FirstName != null) return referee.FirstName;
                if (referee.LastName != null) return referee.LastName;

                if (referee.User != null) return referee.User.UserName;

                return "";
            }

            if (referee.SecondName == null) referee.SecondName = "";
            if (referee.ThirdName == null) referee.ThirdName = "";
            if (referee.LastName == null) referee.LastName = "";

            return referee.FirstName + (string.IsNullOrWhiteSpace(referee.SecondName) ? "" : " " + referee.SecondName.Trim()) + (string.IsNullOrWhiteSpace(referee.ThirdName) ? "" : " " + referee.ThirdName.Trim()) + (string.IsNullOrWhiteSpace(referee.LastName) ? "" : " " + referee.LastName.Trim());
        }

        public static bool Exists<TEntity>(this ApsDbEntities context, TEntity entity)
             where TEntity : class
        {
            return context.Set<TEntity>().Local.Any(e => e == entity);
        }

        public static T GetProperty<T>(this object src, string name)
        {
            if (src.HasProperty(name) && src.GetPropValue(name) is T)
                return (T)src.GetPropValue(name);

            return default(T);
        }

        public static T AddIfNotExists<T>(this DbSet<T> dbSet, T entity, Expression<Func<T, bool>> predicate = null) where T : class, new()
        {
            var exists = predicate != null ? dbSet.Any(predicate) : dbSet.Any();
            return !exists ? dbSet.Add(entity) : null;
        }

        /// <summary>
        /// Use the current thread's culture info for conversion
        /// </summary>
        ///

        public static string ToTitleCase(this string str)
        {
            var cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        public static string ToPascalCase(this string str)
        {
            // Find word parts using the following rules:
            // 1. all lowercase starting at the beginning is a word
            // 2. all caps is a word.
            // 3. first letter caps, followed by all lowercase is a word
            // 4. the entire string must decompose into words according to 1,2,3.
            // Note that 2&3 together ensure MPSUser is parsed as "MPS" + "User".

            var m = Regex.Match(str, "^(?<word>^[a-z]+|[A-Z]+|[A-Z][a-z]+)+$");
            var g = m.Groups["word"];

            // Take each word and convert individually to TitleCase
            // to generate the final output.  Note the use of ToLower
            // before ToTitleCase because all caps is treated as an abbreviation.
            var t = Thread.CurrentThread.CurrentCulture.TextInfo;
            var sb = new StringBuilder();
            foreach (var c in g.Captures.Cast<Capture>())
                sb.Append(t.ToTitleCase(c.Value.ToLower()));
            return sb.ToString();
        }

        /// <summary>
        /// Overload which uses the culture info with the specified name
        /// </summary>
        public static string ToTitleCase(this string str, string cultureInfoName)
        {
            var cultureInfo = new CultureInfo(cultureInfoName);
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        /// <summary>
        /// Overload which uses the specified culture info
        /// </summary>
        public static string ToTitleCase(this string str, CultureInfo cultureInfo)
        {
            return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
        }

        public static object GetPropValue(this object src, string propName)
        {
            if (src.HasProperty(propName))
                return src.GetType().GetProperty(propName).GetValue(src, null);
            return null;
        }

        public static void SetPropValue(this object src, string propName, object value)
        {
            if (src.HasProperty(propName))
                src.GetType().GetProperty(propName).SetValue(src, value);
        }

        public static bool HasProperty(this object settings, string name)
        {
            return settings.GetType().GetProperty(name) != null;
        }

        public static string SanitizeException(this Exception ex)
        {
            if (ex.InnerException != null) return SanitizeException(ex.InnerException);

            if (ex is System.Data.SqlClient.SqlException)
            {
                var rex = (System.Data.SqlClient.SqlException)ex;
                return rex.Message + "<br/>" + rex.Errors.SerializeObject();
            }
            else if (ex is System.Data.Entity.Validation.DbEntityValidationException)
            {
                var rex = (System.Data.Entity.Validation.DbEntityValidationException)ex;

                var errors = new List<string>();
                foreach (var e in rex.EntityValidationErrors.ToList())
                {
                    foreach (var e2 in e.ValidationErrors.ToList())
                    {
                        errors.Add(e2.PropertyName + "|" + e2.ErrorMessage);
                    }
                }

                return rex.Message + "<br/>" + errors.SerializeObject();
            }
            else
            {
                return ex.Message;
            }
        }

        public static string SerializeObject(this object obj)
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

        //public static IDictionary<string, object> ToDictionary(this object source)
        //{
        //    return source.ToDictionary<object>();
        //}

        //public static IDictionary<string, T> ToDictionary<T>(this object source)
        //{
        //    if (source == null)
        //        return null;

        //    var dictionary = new Dictionary<string, T>();
        //    foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
        //    {
        //        dictionary.Add(property.Name, (T)property.GetValue(source));
        //    }

        //    return dictionary;
        //}

        public static void SafeAdd<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, value);
            }
        }

        public static void SafeAddNin(this Dictionary<string, string> dict, string key, string value, ExcelRange cell = null)
        {
            if (dict.ContainsKey(key))
            {
                if (Array.IndexOf(dict[key].Split('|'), value) < 0)
                    dict[key] += " | " + value;
            }
            else
            {
                dict.Add(key, value);
            }

            if (cell != null)
            {
                cell.Value = value;
            }
        }

        public static string SpecialTrim(this string str, bool removeCashida = true, bool removeDiacritics = true, bool removeTabsAndDuplicateSpaces = true)
        {
            if (str == null) return null;
            var builder = new StringBuilder(str.Length);
            var flag = false;

            for (var i = 0; i < str.Length; i++)
            {
                if (flag == false && !char.IsLetterOrDigit(str[i]))
                {
                    continue;
                }

                flag = true;

                if (removeCashida && str[i] == 'ـ') continue;
                if (removeDiacritics && CharUnicodeInfo.GetUnicodeCategory(str[i]) == UnicodeCategory.NonSpacingMark) continue;

                if (removeTabsAndDuplicateSpaces && (str[i] == ' ' || str[i] == '\t' || str[i] == '\r' || str[i] == '\n'))
                {
                    if (i + 1 < str.Length && (str[i + 1] != ' ' && str[i + 1] != '\t' && str[i + 1] != '\r' && str[i + 1] != '\n'))
                        builder.Append(' ');

                    continue;
                }

                builder.Append(str[i]);
            }
            var str2 = builder.ToString();

            flag = false;
            var lastDigitOrLetterIndex = -1;

            for (var i = str2.Length - 1; i >= 0; i--)
            {
                if (flag == false && !char.IsLetterOrDigit(str2[i]) && str2[i] != ')')
                {
                    continue;
                }

                lastDigitOrLetterIndex = i;
                break;
            }

            if (lastDigitOrLetterIndex > 0)
                return str2.Substring(0, lastDigitOrLetterIndex + 1);

            return str2;
        }

        public static string LocalizedName(this object obj, Translator translator = null)
        {
            if (obj == null) return null;
            var val = (object)null;

            var isArabic = Utils.IsUserLanguageArabic;

            if (!isArabic)
            {
                val = obj.GetPropValue("NameEnglish");

                if (val != null)
                {
                    var str = val.ToString();

                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        return str;
                    }
                }
            }

            val = obj.GetPropValue("NameArabic");

            if (val != null)
            {
                var str = val.ToString();

                if (!string.IsNullOrWhiteSpace(str))
                {
                    return str;
                }
            }

            val = obj.GetPropValue("Name");

            if (val != null)
            {
                var str = val.ToString();

                if (!string.IsNullOrWhiteSpace(str))
                {
                    if (translator != null)
                        translator.GetLangValueByString(str);

                    return str;
                }
            }

            return "";
        }

        public static string GetAuthorName(object author, bool lastNameBeforeFirstName = true, bool shortHand = false)
        {
            if (author == null) return null;

            lastNameBeforeFirstName = true;

            var builder = new StringBuilder(500);

            string firstName = (string)author.GetPropValue("FirstNameArabic");
            string secondName = (string)author.GetPropValue("SecondNameArabic");
            string thirdName = (string)author.GetPropValue("ThirdNameArabic");
            string lastName = (string)author.GetPropValue("LastNameArabic");

            if (firstName != null)
                firstName = firstName.SpecialTrim();

            if (firstName == "د" || firstName == "د." || firstName == "د/")
            {
                if (!string.IsNullOrWhiteSpace(secondName))
                {
                    firstName = secondName;
                    secondName = null;
                }
                else if (!string.IsNullOrWhiteSpace(thirdName))
                {
                    firstName = thirdName;
                    thirdName = null;
                }
            }

            if (secondName != null)
                secondName = secondName.SpecialTrim();

            if (thirdName != null)
                thirdName = thirdName.SpecialTrim();

            if (lastName != null)
                lastName = lastName.SpecialTrim();

            if (lastNameBeforeFirstName)
            {
                if (!string.IsNullOrWhiteSpace(lastName))
                {
                    builder.Append(lastName);

                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        builder.Append("، ");
                        builder.Append(firstName);

                        if (shortHand) return builder.ToString();

                        if (!string.IsNullOrWhiteSpace(secondName))
                        {
                            builder.Append(" ");
                            builder.Append(secondName);
                        }

                        if (!string.IsNullOrWhiteSpace(thirdName))
                        {
                            builder.Append(" ");
                            builder.Append(thirdName);
                        }
                    }

                    return builder.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(thirdName))
                {
                    builder.Append(thirdName);

                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        builder.Append("، ");
                        builder.Append(firstName);

                        if (shortHand) return builder.ToString();

                        if (!string.IsNullOrWhiteSpace(secondName))
                        {
                            builder.Append(" ");
                            builder.Append(secondName);
                        }
                    }

                    return builder.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(secondName))
                {
                    builder.Append(secondName);

                    if (!string.IsNullOrWhiteSpace(firstName))
                    {
                        builder.Append("، ");
                        builder.Append(firstName);
                    }

                    return builder.ToString();
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(firstName))
                {
                    builder.Append(firstName);

                    if (shortHand)
                    {
                        if (!string.IsNullOrWhiteSpace(lastName))
                        {
                            builder.Append(" ");
                            builder.Append(lastName);
                        }

                        return builder.ToString();
                    }

                    if (!string.IsNullOrWhiteSpace(secondName))
                    {
                        builder.Append(" ");
                        builder.Append(secondName);
                    }

                    if (!string.IsNullOrWhiteSpace(thirdName))
                    {
                        builder.Append(" ");
                        builder.Append(thirdName);
                    }

                    if (!string.IsNullOrWhiteSpace(lastName))
                    {
                        builder.Append(" ");
                        builder.Append(lastName);
                    }

                    return builder.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(secondName))
                {
                    builder.Append(secondName);

                    if (!string.IsNullOrWhiteSpace(thirdName))
                    {
                        builder.Append(" ");
                        builder.Append(thirdName);
                    }

                    if (!string.IsNullOrWhiteSpace(lastName))
                    {
                        builder.Append(" ");
                        builder.Append(lastName);
                    }

                    return builder.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(thirdName))
                {
                    builder.Append(thirdName);

                    if (!string.IsNullOrWhiteSpace(lastName))
                    {
                        builder.Append(" ");
                        builder.Append(lastName);
                    }

                    return builder.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(lastName))
                {
                    builder.Append(lastName);

                    return builder.ToString();
                }
            }

            if (!string.IsNullOrWhiteSpace(firstName))
                builder.Append(firstName);

            return builder.ToString();
        }
    }
}