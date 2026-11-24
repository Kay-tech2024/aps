using APS.Auth;
using APS.Core;
using APS.Core.Managers;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using APS.Services;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace APS.Web.Controllers
{
    [CustomAuthorize]
    public class BaseController : Controller
    {
        protected ApsDbEntities _db;
        protected SmtpSettings _smtpSettings;
        protected SmsSettings _malathSmsSettings;
        protected SmsSettings _mobilySmsSettings;
        protected UserPreferences _preferences;
        protected ApplicationUser _user;
        protected ApsUserRole[] _roles;
        protected Privilages? _privilages;
        protected bool? _isProviderRole;
        protected bool? _isAdmin;
        protected bool? _isJpcRole;
        protected bool? _isSystemAdmin;
        protected ApplicationDbContext _authDbContext;
        protected ApplicationSignInManager _signInManager;
        protected ApplicationUserManager _userManager;

        public new HttpContextBase HttpContext
        {
            get
            {
                return HttpContextManager.Current;
            }
        }

        public new HttpRequestBase Request
        {
            get
            {
                return HttpContext.Request;
            }
        }

        public bool IsDebug { get { return ((CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation")).Debug; } }

        public bool IsBackgroundJob { get { return HttpContext == null; } }

        public RequestListModel ToListModel(long requestId)
        {
            if (!(requestId > 0)) return null;

            return DbContext.Requests
                     .Where(r => r.Id == requestId)
                     .Select(r =>
                          new RequestListModel
                          {
                              Id = r.Id,
                              PublicationRequestId = (r.Type == (byte)RequestType.PublishRequest || r.Type == (byte)RequestType.RepublishRequest) ? r.Id : r.PublicationRequestId,
                              PublicationRequestUpdated = (r.Type == (byte)RequestType.PublishRequest || r.Type == (byte)RequestType.RepublishRequest) ? r.Updated : r.PublicationRequest.Updated,
                              UniqueId = r.UniqueId == null ? r.Id.ToString() : r.UniqueId,
                              Title = r.Title,
                              DepartmentName = r.Department != null ? r.Department.NameArabic : "",
                              CollegeName = r.College != null ? r.College.NameArabic : "",
                              EmployeeId = r.CreatedByUser.UserProfile == null ? "" : r.CreatedByUser.UserProfile.EmployeeId,
                              Name = r.CreatedByUser.UserProfile == null ? "" : r.CreatedByUser.FullName,
                              TypeEnum = (RequestType)r.Type,
                              DateCreatedActualGreg = r.Created,
                              DateUpdatedActualGreg = r.Updated,
                              StatusEnum = (RequestStatus)r.Status,
                              Updated = r.Updated,
                              Index = 0,

                              CreatedBy = r.CreatedBy,
                              NoOfParticipant = r.NoOfParticipant,
                              RequestCoresearchers = r.RequestCoresearchers.Select(rc => new RequestCoresearcherListModel
                              {
                                  ResearcherId = rc.ResearcherId,
                                  DidResearcherAgree = rc.DidResearcherAgree,
                              }).ToList(),
                              CollegeId = r.CollegeId,
                              RequestReferees = r.RequestReferees.Select(rr => new RequestRefereeListModel
                              {
                                  RefereeUserId = rr.Referee.UserId,
                                  Action = rr.Action,
                                  CreatedByUserRole = rr.CreatedByUser.Roles.FirstOrDefault().Name,
                                  DidRefereeAgree = rr.DidRefereeAgree,
                                  IsDeleted = rr.IsDeleted,
                                  RequestReviewSentOn = rr.RequestReviewSentOn,
                                  RoundNo = rr.RoundNo,
                                  SubmittedOn = rr.SubmittedOn,
                              }).ToList(),
                              RequestUsers = r.RequestUsers.Select(ru => new RequestUserListModel
                              {
                                  UserId = ru.UserId,
                                  Action = ru.Action,
                                  UserRoles = ru.User.Roles.Select(x => x.Name).ToList(),
                              }).ToList(),
                              PublicationRequest = new PublicationRequestListModel
                              {
                                  CreatedBy = r.PublicationRequest.CreatedBy
                              }
                          }
                    )
                    .FirstOrDefault();
        }

        public Request GetRequest(long? id = null)
        {
            var req = DbContext.Requests.Where(e => e.Id == id).ToList();
            if (req != null)
            {
                return req.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                if (_signInManager != null)
                    return _signInManager;

                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                if (HttpContext == null) return null;
                if (_userManager != null) return _userManager;

                _userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                if (_userManager == null)
                {
                    _userManager = ApplicationUserManager.Create(AuthDbContext);
                }

                return _userManager;
            }
        }

        public string SessionId
        {
            get
            {
                var sessionClaim = SignInManager.AuthenticationManager.User.Claims.FirstOrDefault(c => c.Type == "SESSION_ID");

                if (sessionClaim != null)
                {
                    return sessionClaim.Value;
                }

                return null;
            }
        }

        public UserSession UserSession
        {
            get
            {
                return UserUtils.UserSession;
            }
        }

        public string UserId
        {
            get
            {
                return CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id;
            }
        }

        public ApsDbEntities DbContext
        {
            get
            {
                if (_db != null && !_db.IsDisposed) return _db;

                _db = Utils.db;

                return _db;
            }
        }

        public ApplicationDbContext AuthDbContext
        {
            get
            {
                if (_authDbContext != null && !_authDbContext.IsDisposed) return _authDbContext;

                _authDbContext = Utils.auth_db;

                return _authDbContext;
            }
        }

        public bool IsArabic
        {
            get
            {
                return true;
            }
        }

        protected override void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            base.OnAuthenticationChallenge(filterContext);
        }

        public SmtpSettings SmtpSettings
        {
            get
            {
                if (_smtpSettings == null)
                {
                    _smtpSettings = Utils.GetSmtpSettings(DbContext);
                }

                return _smtpSettings;
            }
        }

        public UserPreferences Preferences
        {
            get
            {
                if (_preferences == null)
                {
                    _preferences = new UserPreferences { Language = LanguagePreference.DontCare, Menu = MenuPreference.DontCare };

                    if (CurrentUser != null && CurrentUser.Id != null)
                    {
                        var prefs = DbContext.UserPreferences.Where(s => s.UserId == CurrentUser.Id).ToList();

                        if (prefs.Count(s => s.Key.ToUpper() == "LANG") >= 1)
                        {
                            _preferences.Language = prefs.FirstOrDefault(s => s.Key.ToUpper() == "LANG").Value == "AR" ? LanguagePreference.Arabic : LanguagePreference.English;
                        }

                        if (prefs.Count(s => s.Key.ToUpper() == "MENU" && s.UserAgent == Request.UserAgent) >= 1)
                        {
                            _preferences.Menu = prefs.FirstOrDefault(s => s.Key.ToUpper() == "MENU" && s.UserAgent == Request.UserAgent).Value == "COLLAPSED" ? MenuPreference.Collapsed : MenuPreference.Expanded;
                        }
                        else if (prefs.Count(s => s.Key.ToUpper() == "MENU") >= 1)
                        {
                            _preferences.Menu = prefs.FirstOrDefault(s => s.Key.ToUpper() == "MENU").Value == "COLLAPSED" ? MenuPreference.Collapsed : MenuPreference.Expanded;
                        }
                    }
                }

                return _preferences;
            }
        }

        public IList<string> CurrentUserRoles
        {
            get
            {
                return UserManager.GetRoles(CurrentUser.Id);
            }
        }

        public ApplicationUser CurrentUser
        {
            get
            {
                return UserUtils.CurrentUser;
            }
        }

        public ApplicationUser ActualCurrentUser
        {
            get
            {
                return UserUtils.ActualCurrentUser;
            }
        }

        public void SetCurrentUser(string username)
        {
            _user = AuthDbContext.Users.FirstOrDefault(u => u.UserName == username);
            _roles = DbContext.Roles.Where(ru => ru.Users.Count(u => u.Id == _user.Id) > 0).ToList().Select(r => (ApsUserRole)Enum.Parse(typeof(ApsUserRole), r.Name)).ToArray();
        }

        public ApsUserRole[] Roles
        {
            get
            {
                if (_roles != null) return _roles;

                _roles = UserUtils.Roles;
                return _roles;
            }
        }

        public ApsUserRole[] ActualRoles
        {
            get
            {
                if (_roles != null) return _roles;

                if (CurrentUser == null) return new ApsUserRole[0];

                _roles = Utils.GetUserRoles(CurrentUser.Id);
                return _roles;
            }
        }

        public Privilages Privilages
        {
            get
            {
                if (_privilages != null) return _privilages.Value;

                _privilages = Utils.GetUserPrivilages(CurrentUser.Id);

                return _privilages.Value;
            }
        }

        public Privilages ActualPrivilages
        {
            get
            {
                if (_privilages != null) return _privilages.Value;

                _privilages = Utils.GetActualUserPrivilages(CurrentUser.Id);

                return _privilages.Value;
            }
        }

        public bool IsAdmin
        {
            get
            {
                if (_isAdmin != null) return _isAdmin.Value;
                _isAdmin = Utils.IsAdmin(Roles);
                return _isAdmin.Value;
            }
        }

        public bool IsCollegeUser
        {
            get
            {
                return CurrentUser.CollegeId != null;
            }
        }

        public bool IsGroupRole
        {
            get
            {
                return CurrentUser.CollegeId != null;
            }
        }

        public bool IsSystemAdmin
        {
            get
            {
                if (_isSystemAdmin != null) return _isSystemAdmin.Value;
                _isSystemAdmin = Utils.IsSystemAdmin(Roles);
                return _isSystemAdmin.Value;
            }
        }

        protected List<CollegeViewModel> GetDepartments(bool allowOtherDepartments = false)
        {
            return Utils.GetDepartments(allowOtherDepartments).Select(p => new CollegeViewModel
            {
                Id = p.Id,
                Name = p.Name
            }).OrderBy(p => p.NameArabic).ToList();
        }

        public ActionResult RedirectToLastIndexPage(string action, string controller)
        {
            if (UserSession != null && !string.IsNullOrWhiteSpace(UserSession.LastIndexUrl) && Request.Url.ToString() != UserSession.LastIndexUrl)
            {
                return Redirect(UserSession.LastIndexUrl);
            }
            else
            {
                return RedirectToAction(action, controller);
            }
        }

        protected List<CollegeViewModel> GetColleges()
        {
            return Utils.GetColleges().Select(j => new CollegeViewModel
            {
                Id = j.Id,
                Name = j.Name
            }).ToList();
        }

        protected List<MajorViewModel> GetMajors()
        {
            return Utils.GetMajors().Select(j => new MajorViewModel
            {
                Id = j.Id,
                Name = j.Name
            }).ToList();
        }

        protected List<RoleViewModel> GetRoles()
        {
            var roles = new List<RoleViewModel>();
            var EnumRole = Enum.GetValues(typeof(ApsUserRole));
            foreach (var R in EnumRole)
            {
                var role = DbContext.Roles.Where(x => x.Name == R.ToString()).Select(r => new RoleViewModel { RoleId = r.Id, RoleName = r.Name }).FirstOrDefault();
                if (role != null)
                    roles.Add(role);
            }
            roles = roles
                        .Where(r => r.RoleName != "PrincipalResearcher")
                        .Select(r => new RoleViewModel { RoleId = r.RoleId, RoleName = LangHelpers.Instance[r.RoleName] })
                        .ToList();

            return roles.OrderBy(r => r.RoleName).ToList();
        }

        public ApsUserRole ValidateProfileByRole()
        {
            if (CurrentUser == null) return ApsUserRole.None;
            var Role = ApsUserRole.None;
            var UserActiveRole = Utils.GetActiveUserRole();

            if (UserActiveRole == ApsUserRole.Referee)
            {
                var RefereeProfileValid = DbContext.Referees.Where(x => x.UserId == CurrentUser.Id).Select(x => x.ForceToUpdateProfile).FirstOrDefault();
                if (RefereeProfileValid == true)
                    Role = ApsUserRole.Referee;
            }
            else if (UserActiveRole == ApsUserRole.Researcher)
            {
                var ResearcherProfileValid = DbContext.Researchers.Where(x => x.UserId == CurrentUser.Id).Select(x => x.ForceToUpdateProfile).FirstOrDefault();
                if (ResearcherProfileValid == true)
                    Role = ApsUserRole.Researcher;
            }
            else if (UserActiveRole != ApsUserRole.Referee || UserActiveRole != ApsUserRole.Researcher)
            {
                if (CurrentUser.ForceToUpdateProfile == true)
                    Role = UserActiveRole;
            }
            return Role;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = CurrentUser;

            if (user == null && !string.IsNullOrWhiteSpace(filterContext.HttpContext.Request["UserName"]) && filterContext.ActionDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName != "Account")
            {
                var controller = new AccountController();
                var model = new LoginViewModel
                {
                    UserName = filterContext.HttpContext.Request["UserName"],
                    Password = filterContext.HttpContext.Request["Password"],
                    RememberMe = filterContext.HttpContext.Request["RememberMe"] == "true",
                };

                var result = controller.Login(model, null);
                filterContext.Result = result;
            }
            else if (user != null && filterContext != null && filterContext.ActionDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName != "Account")
            {
                if (!HttpContext.Request.IsAjaxRequest())
                {
                    var role = ValidateProfileByRole();
                    if (role != ApsUserRole.None)
                    {
                        TempData["message"] = LangHelpers.Instance[user.ForcePasswordChange ? "please_complete_your_profile_and_change_password" : "please_complete_your_profile"];
                        TempData["message_type"] = "danger";

                        filterContext.Result = RedirectToAction("Profile", "Account", new { Role = role });
                    }
                    else if (user.ForcePasswordChange && filterContext != null && filterContext.ActionDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName != "Account")
                    {
                        TempData["message"] = LangHelpers.Instance["you_must_change_password"];
                        TempData["message_type"] = "danger";

                        filterContext.Result = RedirectToAction("Profile", "Account");
                    }
                }
                //if (filterContext != null && filterContext.ActionDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor != null && filterContext.ActionDescriptor.ControllerDescriptor.ControllerName != "Account")
                //{
                //    TempData["message"] = ApsStrings.you_must_change_password;
                //    TempData["message_type"] = "danger";

                //    filterContext.Result = RedirectToAction("Profile", "Account");
                //}
            }

            base.OnActionExecuting(filterContext);
        }

        private static Regex _rx_national_id = new Regex(@"^([1-2][0-9]{9})$", RegexOptions.Compiled);
        private static Regex _rx_national_id_start = new Regex(@"^[1-2]\d{0,9}$", RegexOptions.Compiled);

        public static bool IsValidNationalId(string nin)
        {
            if (string.IsNullOrWhiteSpace(nin)) return false;
            nin = nin.Replace(" ", "");

            var newNin = nin;
            if (nin.Split('-').Length == 2)
            {
                newNin = nin.Split('-')[1];
            }

            var isValidSaudiId = _rx_national_id.Match(newNin).Success && Utils.CalculateLuhnAlgorithm(newNin);

            if (!isValidSaudiId && nin.StartsWith("GCC-"))
            {
                long value = 0;
                return long.TryParse(newNin, out value);
            }

            return isValidSaudiId;
        }

        public static bool IsValidExtensionForFileName(string fileName)
        {
            return AbstractManager.IsValidExtensionForFileName(fileName);
        }

        public static string[] ValidFileExtensions
        {
            get
            {
                return AbstractManager.ValidFileExtensions;
            }
        }

        public static bool IsValidNationalIdStart(string nin)
        {
            if (string.IsNullOrWhiteSpace(nin)) return false;
            nin = nin.Replace(" ", "");

            return _rx_national_id_start.Match(nin).Success;
        }

        public T GetInstance<T>() where T : new()
        {
            if (Utils.HasOwinContext)
            {
                var instance = HttpContext.GetOwinContext().Get<T>(typeof(T).FullName);
                if (instance == null)
                {
                    instance = new T();
                    HttpContext.GetOwinContext().Set<T>(typeof(T).FullName, instance);
                }

                return instance;
            }

            return new T();
        }

        public RequestsManager RequestUtils
        {
            get
            {
                return GetInstance<RequestsManager>();
            }
        }

        public ExportManager ExportUtils
        {
            get
            {
                return GetInstance<ExportManager>();
            }
        }

        public TemplatesManager TemplateUtils
        {
            get
            {
                return GetInstance<TemplatesManager>();
            }
        }

        public CommonManager CommonUtils
        {
            get
            {
                return GetInstance<CommonManager>();
            }
        }

        public UsersManager UserUtils
        {
            get
            {
                return GetInstance<UsersManager>();
            }
        }

        public Department GetDepartment(long? did = null)
        {
            var provider = (Department)null;

            if (!IsCollegeUser)
            {
                if (did != null && did > 0)
                    provider = DbContext.Departments.SingleOrDefault(p => p.Id == did.Value);
            }
            else
            {
                provider = DbContext.Departments.SingleOrDefault(p => p.Id == CurrentUser.CollegeId);
            }

            return provider;
        }

        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new CustomJsonResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        public SmsProvider PreferredSmsProvider
        {
            get
            {
                var setts = DbContext.Settings.Where(s => s.Key.StartsWith("sms")).ToList();

                if (setts.Count(s => s.Key.ToLower() == "sms:type") == 1)
                {
                    var type = SmsProvider.MalathSMS;

                    if (Enum.TryParse(setts.SingleOrDefault(s => s.Key.ToLower() == "sms:type").Value, true, out type))
                    {
                        return type;
                    }
                }
                else if (ConfigurationManager.AppSettings["sms:type"] != null)
                {
                    var type = SmsProvider.MalathSMS;

                    if (Enum.TryParse(ConfigurationManager.AppSettings["sms:type"], true, out type))
                    {
                        return type;
                    }
                }

                return SmsProvider.MobilyWS;
            }
        }

        public SmsSettings MalathSmsSettings
        {
            get
            {
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                        Logger.LogExceptionAsync(ex, _smtpSettings);
                    }
                }

                return _malathSmsSettings;
            }
        }

        public SmsSettings MobilySmsSettings
        {
            get
            {
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
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
                        Logger.LogExceptionAsync(ex, _smtpSettings);
                    }
                }

                return _mobilySmsSettings;
            }
        }

        public bool GenerateAndSendVerificationCode(CodeType type, string to, out string err, out string key)
        {
            key = (string)null;
            err = (string)null;

            var db = Utils.db;

            var email = (string)null;
            var mobile = (string)null;

            if (string.IsNullOrWhiteSpace(to))
            {
                err = LangHelpers.Instance["missing_address"];
                return false;
            }

            to = to.Trim().ToLower();

            if (type == CodeType.Email)
            {
                email = to;
            }
            else if (type == CodeType.SMS)
            {
                mobile = to;
            }

            try
            {
                var validMobileNumber = (string)null;

                if ((type == CodeType.SMS) && (string.IsNullOrWhiteSpace(mobile) || !Utils.ValidateMobile(mobile, out validMobileNumber)))
                {
                    err = LangHelpers.Instance["invalid_mobile_number"];
                    return false;
                }
                else if ((type == CodeType.Email) && (string.IsNullOrWhiteSpace(email) || !Utils.ValidateEmail(email)))
                {
                    err = LangHelpers.Instance["invalid_email"];
                    return false;
                }

                var uniqueHash = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + to;

                var tempUrl = null as UserTemporaryUrl;

                var strHash = Convert.ToBase64String(SHA256CryptoServiceProvider.Create().ComputeHash(Encoding.ASCII.GetBytes(uniqueHash))).Replace("+", "").Replace("/", "").Replace("\\", "");

                key = strHash;

                tempUrl = new UserTemporaryUrl
                {
                    Key = strHash,
                    IsUsed = false,
                    UserId = null,
                    SessionId = UserSession.Id,
                    Type = (byte)(type == CodeType.SMS ? TempUrlType.SmsActivationNoUser : TempUrlType.EmailActivationNoUser),
                    Expires = DateTime.Now.AddHours(24),
                    ShortCode = Utils.GenerateShortCode(),
                    Generated = DateTime.Now,
                    GeneratedBy = UserId,
                };

                db.UserTemporaryUrls.Add(tempUrl);
                db.SaveChanges();

                if (type == CodeType.SMS)
                {
                    var smsMessage = TemplateUtils[TemplateKey.SmsActivationNoUser];

                    var msg = !Utils.IsUserLanguageArabic ? smsMessage.BodyEnglish : smsMessage.BodyArabic;

                    msg = msg.Replace("{code}", tempUrl.ShortCode.ToString());

                    var isSent = SmsService.SendMessage(msg, new string[] { validMobileNumber }, out err);

                    if (!isSent)
                    {
                        err = LangHelpers.Instance["FAILED_TO_SEND_SMS_" + err];
                    }
                    else
                    {
                        UserSession.LastSmsCodeGenerated = DateTime.Now;
                        db.SaveChanges();
                    }

                    return isSent;
                }
                else
                {
                    var emailMessage = TemplateUtils[TemplateKey.EmailActivationNoUser];
                    var subject = Utils.IsUserLanguageArabic ? emailMessage.SubjectArabic : emailMessage.SubjectEnglish;

                    var msg = !Utils.IsUserLanguageArabic ? emailMessage.BodyEnglish : emailMessage.BodyArabic;

                    msg = msg.Replace("{code}", tempUrl.ShortCode.ToString());

                    var isSent = EmailService.SendMessage(SmtpSettings, subject, msg, new MailAddress(email), null, null);

                    if (!isSent)
                    {
                        err = LangHelpers.Instance["FAILED_TO_SEND_EMAIL_" + err];
                    }
                    else
                    {
                        UserSession.LastEmailCodeGenerated = DateTime.Now;
                        db.SaveChanges();
                    }

                    return isSent;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                Logger.LogExceptionAsync(ex);
                err = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex);

                err = ex.SanitizeException();

                return false;
            }
        }

        public bool CreateTemporaryLink(TempUrlType type, string userId, string email, string mobile, out string err, out string key)
        {
            key = (string)null;
            err = (string)null;

            var db = Utils.db;
            var user = (User)null;

            if (string.IsNullOrWhiteSpace(email))
                email = null;
            else
                email = email.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(mobile))
                mobile = null;

            try
            {
                user = db.Users.FirstOrDefault(u => (userId != null && u.Id == userId) || (email != null && u.Email == email));

                if (user != null)
                {
                    if (string.IsNullOrWhiteSpace(email))
                        email = user.Email;
                    if (string.IsNullOrWhiteSpace(mobile))
                        mobile = user.PhoneNumber;
                }

                var validMobileNumber = (string)null;

                if (type == TempUrlType.SmsActivation && (string.IsNullOrWhiteSpace(mobile) || !Utils.ValidateMobile(mobile, out validMobileNumber)))
                {
                    err = "invalid_mobile_number";
                    //err = LangHelpers.Instance[err];
                    return false;
                }
                else if (type == TempUrlType.EmailActivation && (string.IsNullOrWhiteSpace(email) || !Utils.ValidateEmail(email)))
                {
                    err = "invalid_email";
                    return false;
                }
                else if (type == TempUrlType.EmailActivation && user == null)
                {
                    err = "USER_NOT_FOUND";
                    return false;
                }

                var uniqueHash = DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    uniqueHash += userId;
                }
                else if (!string.IsNullOrWhiteSpace(email))
                {
                    uniqueHash += email;
                }
                else if (!string.IsNullOrWhiteSpace(mobile))
                {
                    uniqueHash += mobile;
                }

                var tempUrl = null as UserTemporaryUrl;

                var strHash = Convert.ToBase64String(SHA256CryptoServiceProvider.Create().ComputeHash(Encoding.ASCII.GetBytes(uniqueHash))).Replace("+", "").Replace("/", "").Replace("\\", "");

                key = strHash;

                tempUrl = new UserTemporaryUrl
                {
                    Key = strHash,
                    IsUsed = false,
                    UserId = user == null ? null : user.Id,
                    Type = (byte)type,
                    Expires = type == TempUrlType.SmsActivation ? DateTime.Now.AddMinutes(5) : DateTime.Now.AddHours(24),
                    ShortCode = Utils.GenerateShortCode(),
                    Generated = DateTime.Now,
                    GeneratedBy = CurrentUser == null ? Utils.SystemAdminUserId : CurrentUser.Id,
                };

                db.UserTemporaryUrls.Add(tempUrl);
                db.SaveChanges();

                if (type == TempUrlType.SmsActivation)
                {
                    var smsMessage = TemplateUtils[TemplateKey.NewUserSmsActivation];

                    var msg = !Utils.IsUserLanguageArabic ? smsMessage.BodyEnglish : smsMessage.BodyArabic;

                    msg = msg.Replace("{code}", tempUrl.ShortCode.ToString());

                    var isSent = SmsService.SendMessage(msg, new string[] { validMobileNumber }, out err);

                    if (!isSent)
                    {
                        err = "FAILD_TO_SEND_SMS";
                    }

                    return isSent;
                }
                else if (type == TempUrlType.EmailActivation)
                {
                    var emailMessage = TemplateUtils[TemplateKey.NewUserEmailActivation];

                    var subject = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(emailMessage.SubjectEnglish) ? emailMessage.SubjectArabic : emailMessage.SubjectEnglish;
                    var body = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(emailMessage.SubjectEnglish) ? emailMessage.BodyArabic : emailMessage.BodyEnglish;

                    var link = Utils.GetApplicationUrl() + "l/" + tempUrl.Key;
                    body = body.Replace("{username}", user.UserName).Replace("{link}", link);

                    var isSent = EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(user.Email, user.FullName), null, null);
                    if (!isSent)
                    {
                        err = "FAILD_TO_SEND_EMAIL";
                    }

                    return isSent;
                }
                else if (type == TempUrlType.ResetPassword)
                {
                    var emailMessage = TemplateUtils[TemplateKey.ResetPassword];

                    var subject = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(emailMessage.SubjectEnglish) ? emailMessage.SubjectArabic : emailMessage.SubjectEnglish;
                    var body = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(emailMessage.SubjectEnglish) ? emailMessage.BodyArabic : emailMessage.BodyEnglish;

                    var link = Utils.GetApplicationUrl() + "l/" + tempUrl.Key;
                    body = body.Replace("{username}", user.UserName).Replace("{link}", link);

                    var isSent = EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(user.Email, user.FullName), null, null);
                    if (!isSent)
                    {
                        err = "FAILD_TO_SEND_EMAIL";
                    }

                    return isSent;
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                Logger.LogExceptionAsync(ex);
                err = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;

                return false;
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex);

                err = ex.SanitizeException();

                return false;
            }

            return true;
        }

        public bool SendRefereeNotification(RequestReferee rr, string key = null)
        {
            if (rr == null || !(rr.Id > 0)) return false;

            var email = rr.Referee.Email;
            var mobile = rr.Referee.MobileNo;

            if (!Utils.ValidateEmail(email))
                email = null;

            if (!Utils.ValidateMobile(mobile, out mobile))
                mobile = null;

            if (email == null && mobile == null) return false;

            var fullName = rr.Referee.GetFullName();
            var subject = (string)null;
            var body = (string)null;
            var smsBody = (string)null;

            var tempUrl = (UserTemporaryUrl)null;

            if (rr.Referee.UserId == null)
            {
                if ((string.IsNullOrWhiteSpace(key) || (tempUrl = DbContext.UserTemporaryUrls.OrderByDescending(tu => tu.Generated).FirstOrDefault(tu => tu.Key == key && tu.Expires > DateTime.Now && !tu.IsUsed)) == null))
                {
                    var err = "";
                    if (!CreateTemporaryLink(TempUrlType.RegisterReferee, null, email, mobile, out err, out key))
                        return false;

                    tempUrl = DbContext.UserTemporaryUrls.OrderByDescending(tu => tu.Generated).FirstOrDefault(tu => tu.Key == key && tu.Expires > DateTime.Now && !tu.IsUsed);

                    if (tempUrl == null)
                        return false;
                }

                var template = TemplateUtils[TemplateKey.RegisterReferee];

                subject = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.SubjectArabic : template.SubjectEnglish;
                body = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.BodyArabic : template.BodyEnglish;
                var link = Utils.GetApplicationUrl() + "l/" + tempUrl.Key;
                body = body.Replace("{name}", fullName).Replace("{link}", link);
                body = body.Replace("{title}", rr.Request.Title).Replace("{type}", LangHelpers.Instance["type_" + ((PublicationType)rr.Request.PublicationType).ToString()]);

                template = TemplateUtils[TemplateKey.RegisterRefereeSMS];

                smsBody = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.BodyArabic : template.BodyEnglish;
                smsBody = smsBody.Replace("{name}", fullName).Replace("{link}", link);
                smsBody = smsBody.Replace("{title}", rr.Request.Title).Replace("{type}", LangHelpers.Instance["type_" + ((PublicationType)rr.Request.PublicationType).ToString()]);
            }
            else
            {
                var template = TemplateUtils[TemplateKey.InviteReferee];

                subject = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.SubjectArabic : template.SubjectEnglish;
                body = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.BodyArabic : template.BodyEnglish;

                var link = Utils.GetApplicationUrl();
                body = body.Replace("{name}", fullName).Replace("{link}", link);
                body = body.Replace("{title}", rr.Request.Title).Replace("{type}", LangHelpers.Instance["type_" + ((PublicationType)rr.Request.PublicationType).ToString()]);

                template = TemplateUtils[TemplateKey.InviteRefereeSMS];

                smsBody = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.BodyArabic : template.BodyEnglish;
                smsBody = smsBody.Replace("{name}", fullName).Replace("{link}", link);
                smsBody = smsBody.Replace("{title}", rr.Request.Title).Replace("{type}", LangHelpers.Instance["type_" + ((PublicationType)rr.Request.PublicationType).ToString()]);
            }

            var sent = false;

            if (email != null)
            {
                var isSent = EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(email, fullName), null, null);
                if (isSent)
                {
                    sent = true;
                }
            }

            if (mobile != null)
            {
                smsBody = Utils.StripHTML(smsBody);
                var isSent = SmsServiceMalath.SendMessage(MalathSmsSettings, smsBody, mobile);
                if (isSent)
                {
                    sent = true;
                }
            }

            return sent;
        }

        public bool SendNonOfficialEmailApprovalNotification(ApplicationUser appUser, string emails, string names)
        {
            if (appUser == null || (appUser.Id == null)) return false;
            var subject = (string)null;
            var body = (string)null;
            var emailId = appUser.Email;
            var fullName = appUser.FullName;
            var template = TemplateUtils[TemplateKey.NonOfficialEmailRequiresApproval];

            if (!Utils.ValidateEmail(emailId))
                emailId = null;
            if (emailId == null) return false;

            subject = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.SubjectArabic : template.SubjectEnglish;
            body = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(template.SubjectEnglish) ? template.BodyArabic : template.BodyEnglish;
            body = body.Replace("{name}", fullName).Replace("{emailId}", emailId);

            var sent = false;

            if (emailId != null)
            {
                try
                {
                    var isSent = EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(emails, names), null, null);
                    if (isSent)
                    {
                        sent = true;
                    }
                }
                catch (Exception ex)
                {
                    sent = false;
                    Logger.LogExceptionAsync(ex, _smtpSettings);
                }
            }
            return sent;
        }

        public bool SendContactUsMessage(ContactUsViewModel contact, string emails, string names)
        {
            if (contact == null || (contact.EmailAddress == null)) return false;
            var subject = (string)null;
            var body = (string)null;
            var emailId = contact.EmailAddress;
            var fullName = contact.FullName;
            var message = contact.Message;
            var subjectC = contact.Subject;
            var template = TemplateUtils[TemplateKey.ContactUsMessage];
            if (!Utils.ValidateEmail(emailId))
                emailId = null;
            if (emailId == null) return false;

            if (Utils.IsTextArabic(message))
            {
                subject = template.SubjectArabic;
                body = template.BodyArabic;
            }
            else
            {
                subject = template.SubjectEnglish;
                body = template.BodyEnglish;
            }
            subject = subject.Replace("{fullname}", fullName);
            body = body.Replace("{fullname}", fullName).Replace("{emailId}", emailId).Replace("{message}", message).Replace("{subject}", string.IsNullOrEmpty(subjectC) ? "" : subjectC);

            var sent = false;

            if (emailId != null)
            {
                try
                {
                    var isSent = EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(emails, names), null, null);
                    if (isSent)
                    {
                        sent = true;
                    }
                }
                catch (Exception ex)
                {
                    sent = false;
                    Logger.LogExceptionAsync(ex, _smtpSettings);
                }
            }
            return sent;
        }

        public bool NotificationOnFinalProcedureAction(long Id, bool isApprove)
        {
            if (Id == 0) return false;
            var subject = (string)null;
            var body = (string)null;
            var sent = false;
            Template template = null;

            var allUsers = Utils.db.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.ResearchUnitHead.ToString() || r.Name == ApsUserRole.ViceDeanOfAcademicResearch.ToString() || r.Name == ApsUserRole.Researcher.ToString()) > 0).ToList();
            var emails = string.Join(";", allUsers.Select(x => x.Email).ToArray());
            var names = string.Join(",", allUsers.Select(x => x.FullName).ToArray());
            if (isApprove)
            {
                template = TemplateUtils[TemplateKey.NotificationOnFinalApproval];
            }
            else
            {
                template = TemplateUtils[TemplateKey.NotificationOnFinalRejection];
            }
            subject = template.SubjectArabic.Replace("{Id}", Id.ToString());
            body = template.BodyArabic.Replace("{Id}", Id.ToString()); ;
            try
            {
                var isSent = EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(emails, names), null, null);
                if (isSent)
                {
                    sent = true;
                }
            }
            catch (Exception ex)
            {
                sent = false;
                Logger.LogExceptionAsync(ex, _smtpSettings);
            }
            return sent;
        }

        public bool NotificationOnGradeBasedRefereeAction(long Id, bool isApprove)
        {
            if (Id == 0) return false;
            var subject = (string)null;
            var body = (string)null;
            var smsBody = (string)null;
            var sent = false;
            Template template = null;

            var allSelectedUsers = Utils.db.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.DeanOfAcademicResearch.ToString() || r.Name == ApsUserRole.ViceDeanOfAcademicResearch.ToString() || r.Name == ApsUserRole.AcademicCouncilHead.ToString() || r.Name == ApsUserRole.AcademicCouncilMember.ToString() || r.Name == ApsUserRole.CommitteForPAEHead.ToString() || r.Name == ApsUserRole.CommitteeforPAEMember.ToString()) > 0 && !string.IsNullOrEmpty(u.Email)).ToList();
            //var emails = string.Join(";", allSelectedUsers.Select(x => x.Email).ToArray());
            //var names = string.Join(",", allSelectedUsers.Select(x => x.FullName).ToArray());
            string mobile = string.Join(",", allSelectedUsers.Where(x => string.IsNullOrEmpty(x.PhoneNumber)).Select(x => x.PhoneNumber).ToArray());
            MailAddress[] emails = new MailAddress[allSelectedUsers.Count];
            //foreach (var e in allSelectedUsers)
            //{
            //    var mailAddress = new MailAddress(e.Email, e.FullName);
            //    emails.
            //}
            for (var i = 0; i < allSelectedUsers.Count; i++)
            {
                emails[i] = new MailAddress(allSelectedUsers[i].Email, allSelectedUsers[i].FullName);
            }

            if (isApprove)
            {
                template = TemplateUtils[TemplateKey.NotificationOnGradeBasedRefereeApproval];
            }
            else
            {
                template = TemplateUtils[TemplateKey.NotificationOnGradeBasedRefereeRejection];
            }
            subject = template.SubjectArabic.Replace("{Id}", Id.ToString());
            body = template.BodyArabic.Replace("{Id}", Id.ToString()); ;
            try
            {
                var isSent = EmailService.SendMessage(SmtpSettings, subject, body, emails, null, null);
                if (isSent)
                {
                    sent = true;
                }
                if (mobile != null)
                {
                    smsBody = Utils.StripHTML(body);
                    var isSmsSent = SmsServiceMalath.SendMessage(MalathSmsSettings, smsBody, mobile);
                    if (isSent && isSmsSent)
                    {
                        sent = true;
                    }
                }
            }
            catch (Exception ex)
            {
                sent = false;
                Logger.LogExceptionAsync(ex, _smtpSettings);
            }
            return sent;
        }

        protected T GetKeyValue<T>(string key, List<UserSessionData> data)
        {
            var db = Utils.db;

            if (data == null)
            {
                data = db.UserSessionDatas.Where(x => x.SessionId == UserSession.Id).ToList();
            }

            var dv = db.UserSessionDatas.FirstOrDefault(x => x.SessionId == UserSession.Id && x.Key == key);
            if (dv != null)
            {
                if (dv.Type.StartsWith("System.Int32"))
                {
                    int a = 0;
                    if (int.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Nullable`1[[System.Int32"))
                {
                    int a = 0;
                    if (int.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Int64"))
                {
                    long a = 0;
                    if (long.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Nullable`1[[System.Int64"))
                {
                    long a = 0;
                    if (long.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Int16"))
                {
                    Int16 a = 0;
                    if (Int16.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Nullable`1[[System.Int16"))
                {
                    Int16 a = 0;
                    if (Int16.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Byte"))
                {
                    byte a = 0;
                    if (byte.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Nullable`1[[System.Byte"))
                {
                    byte a = 0;
                    if (byte.TryParse(dv.Value, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.DateTime"))
                {
                    DateTime a = default(DateTime);
                    if (DateTime.TryParseExact(dv.Value, "yyyy-MM-dd hh:mm:ss tt", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Nullable`1[[System.DateTime"))
                {
                    DateTime a = default(DateTime);
                    if (DateTime.TryParseExact(dv.Value, "yyyy-MM-dd hh:mm:ss tt", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("APS.Data.QualificationType"))
                {
                    QualificationType a = default(QualificationType);
                    if (Enum.TryParse(dv.Value, true, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else if (dv.Type.StartsWith("System.Nullable`1[[APS.Data.QualificationType"))
                {
                    QualificationType a = default(QualificationType);
                    if (Enum.TryParse(dv.Value, true, out a))
                    {
                        return (T)((object)a);
                    }

                    return default(T);
                }
                else
                {
                    return (T)((object)dv.Value);
                }
            }

            return default(T);
        }

        protected void SaveKeyValue<T>(string key, T valueObj, List<UserSessionData> data)
        {
            var db = Utils.db;

            if (data == null)
            {
                data = db.UserSessionDatas.Where(x => x.SessionId == UserSession.Id).ToList();
            }

            if (key != null)
            {
                var value = valueObj == null ? null : valueObj.SpecialToString();
                var type = typeof(T).FullName;
                var dv = data.FirstOrDefault(k => k.Key == key && k.Type == type);
                if (dv == null)
                {
                    db.UserSessionDatas.Add(new UserSessionData
                    {
                        Key = key,
                        Value = value,
                        Type = type,
                        SessionId = UserSession.Id,
                    });
                }
                else
                {
                    dv.Value = value;
                }

                db.SaveChanges();
            }
        }
    }

    public class CustomJsonResult : JsonResult
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;

            try
            {
                response.ContentType = !string.IsNullOrEmpty(this.ContentType) ? this.ContentType : "application/json";

                if (this.ContentEncoding != null)
                {
                    response.ContentEncoding = this.ContentEncoding;
                }

                if (this.Data == null)
                {
                    return;
                }

                response.Write(JsonConvert.SerializeObject(this.Data, Settings));
            }
            catch
            {
            }
        }
    }
}