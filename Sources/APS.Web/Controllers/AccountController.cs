using APS.Auth;
using APS.Core;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using APS.TimeMachine;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using EntityFramework.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PhoneNumbers;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(Privilages.account_login, Privilages.account_profile)]
    public class AccountController : BaseController
    {
        public AccountController()
        {
        }

        public ActionResult Index()
        {
            return RedirectToAction("Profile");
        }

        [AllowAnonymous]
        public ActionResult ExternalReferee(string _h)
        {
            if (!String.IsNullOrEmpty(_h))
            {
                string _publicHashLink = Utils.Decrypt(_h);
                if (!String.IsNullOrEmpty(_publicHashLink))
                {
                    string[] _publicHashLinkParts = _publicHashLink.Split(Core.Constants.ExternalRefereeParamsConnector);
                    if (_publicHashLinkParts.Count() >= 3)
                    {
                        int refereeId = -1;
                        Int64 requestId = -1;
                        string email = String.Empty;
                        try
                        {
                            requestId = Convert.ToInt64(_publicHashLinkParts[0]);
                            refereeId = Convert.ToInt32(_publicHashLinkParts[1]);
                            email = _publicHashLinkParts[2];
                        }
                        catch (Exception ex) { }

                        if (requestId != -1 && refereeId != -1 && !String.IsNullOrEmpty(email))
                        {
                            var request = Utils.db.Requests.Where(r => (r.Id == requestId) && r.IsDeleted == false).FirstOrDefault();
                            if (request != null)
                            {
                                var referee = Utils.db.RequestReferees.Include("Referee").Where(r => r.RequestId == requestId && r.CreatedByUser.Roles.FirstOrDefault().Name == ApsUserRole.AcademicCouncilHead.ToString() && r.RefereeId == refereeId && r.IsDeleted == false).FirstOrDefault();
                                if (referee != null)
                                {
                                    int externalRefereeResponseTimeDays = (new RefereeUtil()).GetExtrnalRefereeResponseTimeDays();
                                    int totalHrs = externalRefereeResponseTimeDays * 24;
                                    if (TM.Now.Subtract(referee.RequestReviewSentOn.Value).TotalHours < totalHrs)
                                    {
                                        string welcomeMessage = LangHelpers.Instance["external_referee_welcome_page_message"];
                                        welcomeMessage = welcomeMessage.Replace("{FullName}", referee.Referee.GetFullName());

                                        ViewBag.RequestId = requestId;
                                        ViewBag.RefereeId = refereeId;
                                        ViewBag.Email = email;
                                        ViewBag.ParamsHash = _h;
                                        ViewBag.FullName = referee.Referee.GetFullName();
                                        ViewBag.WelcomeMessage = welcomeMessage;
                                        if (email.EndsWith("@imamu.edu.sa"))
                                        {
                                            ViewBag.RoleCode = 1;
                                        }
                                        else
                                        {
                                            ViewBag.RoleCode = 5;
                                        }

                                        if (referee.Action == null)
                                        {
                                            return View("~/Views/Requests/ExternalRefereeView.cshtml");
                                        }
                                        else if (referee.Action == (byte)RequestAction.AcceptToReview && (referee.Referee != null && referee.Referee.UserId == null))
                                        {
                                            var session = UserSession;
                                            var db = Utils.db;
                                            var data = db.UserSessionDatas.Where(x => x.SessionId == UserSession.Id).ToList();
                                            SaveKeyValue<string>("Email", referee.Referee.Email, data);
                                            SaveKeyValue<string>("ExternalRefereeHashParams", _h, data);

                                            return RedirectToAction("Register", new { role = ViewBag.RoleCode });
                                        }
                                    }
                                    else
                                    {
                                        TempData["message"] = LangHelpers.Instance["external_referee_welcome_page_link_expire"];
                                        TempData["message_type"] = "warning";
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Account/Login.cshtml");
        }

        //
        // POST: /Account/Login

        [ValidateAntiForgeryToken]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            if (model == null || string.IsNullOrWhiteSpace(model.UserName) || ModelState == null || !ModelState.IsValid)
            {
                return View("~/Views/Account/Login.cshtml", model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true

            model.UserName = model.UserName.Trim().ToLower();
            var user = (ApplicationUser)null;
            if (model.UserName.Contains("@"))
            {
                user = UserManager.FindByEmail(model.UserName);
            }
            else
            {
                user = UserManager.FindByName(model.UserName);
            }
            if (!Utils.IsLocalhost)
            {
                RecaptchaVerificationHelper recaptchaHelper = this.GetRecaptchaVerificationHelper();

                if (String.IsNullOrEmpty(recaptchaHelper.Response))
                {
                    TempData["message"] = LangHelpers.Instance["robot_required"];
                    TempData["message_type"] = "warning";
                    return View("~/Views/Account/Login.cshtml", model);
                }
                else
                {
                    RecaptchaVerificationResult recaptchaResult = recaptchaHelper.VerifyRecaptchaResponse();
                    if (recaptchaResult != RecaptchaVerificationResult.Success)
                    {
                        TempData["message"] = LangHelpers.Instance["Incorrect captcha answer."];
                        TempData["message_type"] = "warning";
                        return View("~/Views/Account/Login.cshtml", model);
                    }
                    else
                    {
                        ViewBag.VerifyCaptcha = false;
                    }
                }
            }
            if (user != null && user.IsDeleted)
            {
                TempData["message"] = "هذا الحساب محذوف";
                TempData["message_type"] = "warning";
                return View("~/Views/Account/Login.cshtml", model);
            }
            else if (user != null && user.IsActivated != true)
            {
                TempData["message"] = "هذا الحساب لم يتم تفعيله";
                TempData["message_type"] = "warning";
                return View("~/Views/Account/Login.cshtml", model);
            }
            else if (user != null && user.EmailConfirmed != true)
            {
                TempData["message"] = "لم يتم تفعيل البريد الإلكتروني. يرجى الضغط <a href=\"" + Utils.GetApplicationUrl() + "Account/ResendEmailActivation?username=" + user.UserName + "\">هنا</a> لإعادة إرسال رسالة تفعيل البريد الإلكتروني";
                TempData["message_type"] = "warning";
                return View("~/Views/Account/Login.cshtml", model);
            }
            else if (user != null && user.PhoneNumberConfirmed != true)
            {
                TempData["message"] = "لم يتم تفعيل رقم الجوال. يرجى الضغط <a href=\"" + Utils.GetApplicationUrl() + "Account/SendMobileActivation?username=" + user.UserName + "\">هنا</a> لإعادة إرسال رسالة تفعيل رقم الجوال";
                TempData["message_type"] = "warning";

                return View("~/Views/Account/Login.cshtml", model);
            }

            var isSuccess = !string.IsNullOrWhiteSpace(model.Password) && UserManager.CheckPassword(user, model.Password);

            if (!isSuccess && !string.IsNullOrWhiteSpace(model.Password))
            {
                model.Password = model.Password.Trim();
                isSuccess = UserManager.CheckPassword(user, model.Password);
            }

            if (Utils.IsLocalhost)
            {
                if (user != null)
                {
                    isSuccess = true;
                }
            }

            if (isSuccess)
            {
                user.AccessFailedCount = 0;
                user.LockoutEnabled = false;
                user.LockoutEndDateUtc = null;

                AuthDbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                AuthDbContext.SaveChanges();

                var user2 = DbContext.Users.Include("Roles").Include("Researchers").SingleOrDefault(u => u.Id == user.Id);

                var roles = user2.Roles.ToArray();

                if (!user.IsActivated)
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    ModelState.AddModelError("", "هذا الحساب معطل");
                    return View("~/Views/Account/Login.cshtml", model);
                }
                Session.Abandon();
                Response.Cookies.Add(new HttpCookie("id", Guid.NewGuid().ToString()));
                var identity = SignInManager.CreateUserIdentity(user);

                var sessionId = Guid.NewGuid().ToString();

                var cr = identity.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

                if (user2.Researchers != null && user2.Researchers.Count > 0)
                {
                    identity.AddClaim(new Claim("RESEARCHER_ID", user2.Researchers.First().Id.ToString()));
                }

                identity.AddClaim(new Claim("SESSION_ID", sessionId));

                AuthenticationManager.SignIn(identity);

                var userSessions = DbContext.UserSessions.Where(s => s.UserId == user.Id && s.Lockdate == null).ToList();

                foreach (var s in userSessions)
                {
                    s.Locked = true;
                    s.Lockdate = DateTime.Now;
                }

                if (userSessions.Count > 0)
                {
                }

                var session = new UserSession
                {
                    UserId = user.Id,
                    IpAddress = Utils.GetUserIP(),
                    Expires = model.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(60),
                    Locked = false,
                    Sessionid = sessionId,
                    Updated = DateTime.Now,
                    Created = DateTime.Now,
                };

                DbContext.UserSessions.Add(session);
                DbContext.SaveChanges();

                var currentUrl = Utils.GetFullUrl();

                if (currentUrl.ToLower().EndsWith("account/login") || currentUrl.ToLower().EndsWith("account/logoff"))
                    return RedirectToAction("Index", "Home");

                return Redirect(currentUrl);
            }
            else
            {
                if (user != null)
                {
                    user.AccessFailedCount++;

                    if (user.AccessFailedCount >= 3)
                    {
                        user.LockoutEnabled = true;
                        user.LockoutEndDateUtc = DateTime.UtcNow.AddHours(1);
                    }

                    AuthDbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    AuthDbContext.SaveChanges();

                    if (user.AccessFailedCount >= 3)
                    {
                        ModelState.AddModelError("", string.Format("هذا الحساب معطل لمدة {0} دقيقة", Math.Round((user.LockoutEndDateUtc.Value - DateTime.UtcNow).TotalMinutes)));
                    }
                    else
                    {
                        ModelState.AddModelError("", string.Format("معلومات الدخول غير صحيحة.", 3 - user.AccessFailedCount));
                    }
                }
                else
                {
                    ModelState.AddModelError("", "معلومات الدخول غير صحيحة");
                }

                return View("~/Views/Account/Login.cshtml", model);
            }
        }

        public new ActionResult Profile(ApsUserRole Role = ApsUserRole.None)
        {
            if (CurrentUser == null) return RedirectToAction("LogOff");
            var fileType = (byte)FileType.IbanPhotocopy;
            User user = DbContext.Users
                .Include("UserProfile")
                .Include("Department")
                .Include("College")
                .Include("Roles")
                .Where(u => u.Id == CurrentUser.Id)
                .FirstOrDefault();
            var UserActiveRole = ApsUserRole.None;
            if (Role == ApsUserRole.None)
            {
                UserActiveRole = Utils.GetActiveUserRole();
            }
            else
            {
                UserActiveRole = Role;
            }
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new EditUserViewModel
            {
                FullName = user.UserProfile == null ? user.UserName : user.UserProfile.GetFullName(),
                NationalId = user.UserProfile != null ? user.UserProfile.NationalId : "",
                UserId = user.Id,
                Password = "**********",
                ConfirmPassword = "**********",
                Email = user.Email,
                MobileNo = user.UserProfile == null ? "" : user.UserProfile.MobileNo,
                UserName = user.UserName,
                CollegeId = user.CollegeId ?? 0,
                CollegeName = user.CollegeId == null ? null : (IsArabic ? user.College.NameArabic : user.College.NameEnglish),
                RoleId = user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Id,
                RoleName = LangHelpers.Instance[user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Name],
                IsActivated = user.IsActivated,
                NationalityId = user.UserProfile == null ? null : user.UserProfile.NationalityId,
                BirthDate = user.UserProfile == null ? "" : user.UserProfile.BirthDate == null ? null : user.UserProfile.BirthDate.Value.FormatDate(false, false, false),
                CountryId = user.CountryId ?? 0,
                CityId = user.UserProfile == null ? 0 : user.UserProfile.CityId ?? 0,
                Address = user.Address ?? "",
                UniversityId = user.UniversityId ?? 0,
                UniversityOther = user.UniversityOther ?? "",
                CollegeOther = user.CollegeOther ?? "",
                DepartmentID = user.DepartmentId ?? 0,
                Bank = user.UserProfile == null ? 0 : user.UserProfile.BankId ?? 0,
                BankBranch = user.UserProfile == null ? "" : user.UserProfile.BankBranch,
                BankAccountHolderName = user.UserProfile == null ? "" : user.UserProfile.BankAccountHolder,
                BankAccountIBAN = user.UserProfile == null ? "" : user.UserProfile.BankIban,
                BankOther = user.UserProfile == null ? "" : user.UserProfile.BankName,
                BankAccountTypeId = user.UserProfile == null ? null : user.UserProfile.BankAccountTypeId,
                BankAccountNumberTypeId = user.UserProfile == null ? null : user.UserProfile.BankAccountNumberTypeId,
                SwiftCode = user.UserProfile == null ? "" : user.UserProfile.SwiftCode,
                RoutingNumber = user.UserProfile == null ? "" : user.UserProfile.RoutingNumber,
                Specialization = user.UserProfile == null ? "" : user.UserProfile.Specialization,
                MajorId = user.MajorId
            };
            if (user.UserProfile != null && user.UserProfile.Qualification != null)
            {
                model.Qualification = (QualificationType)user.UserProfile.Qualification > 0 ? (QualificationType)user.UserProfile.Qualification : QualificationType.Unspecified;
            }
            if (user.UserProfile != null && user.UserProfile.BankAccountNumberTypeId == (byte)BankAccountNumberType.Iban)
            {
                fileType = (byte)FileType.IbanPhotocopy;
            }
            else if (user.UserProfile != null && user.UserProfile.BankAccountNumberTypeId == (byte)BankAccountNumberType.Swift)
            {
                fileType = (byte)FileType.SwiftPhotocopy;
            }

            if (UserActiveRole == ApsUserRole.Researcher)
            {
                var researcher = user.Researchers.Where(x => x.UserId == user.Id).FirstOrDefault();
                //model.NationalId = researcher.NationalId;
                model.UniversityId = researcher.UniversityId ?? 0;
                model.UniversityOther = researcher.UniversityOther;
                model.CollegeId = researcher.CollegeId == null ? user.CollegeId ?? 0 : researcher.CountryId ?? 0;

                model.CollegeOther = researcher.CollegeOther;
                model.DepartmentID = researcher.DepartmentId == null ? user.DepartmentId ?? 0 : researcher.DepartmentId ?? 0;
                model.DepartmentOther = researcher.DepartmentOther ?? "";

                if (researcher.Qualification != null)
                {
                    model.Qualification = (QualificationType)researcher.Qualification > 0 ? (QualificationType)researcher.Qualification : QualificationType.Unspecified;
                }
                model.Specialization = researcher.Specialization ?? "";
                model.CountryId = researcher.CountryId == null ? user.CountryId ?? 0 : researcher.CountryId ?? 0;
                model.CityId = researcher.CityId == null ? user.UserProfile == null ? 0 : user.UserProfile.CityId ?? 0 : researcher.CityId ?? 0;
                model.BirthDate = researcher.DateOfBirth == null ? user.UserProfile == null ? "" : user.UserProfile.BirthDate == null ?
                    string.Empty : user.UserProfile.BirthDate.Value.FormatDate(false, false, false) : researcher.DateOfBirth == null ? string.Empty : researcher.DateOfBirth.Value.FormatDate(false, false, false);
                model.NationalityId = researcher.NationalityId == null ? user.UserProfile.NationalityId ?? 0 : researcher.NationalityId ?? 0;

                //model.UniversityId = 1;
            }
            else if (UserActiveRole == ApsUserRole.Referee)
            {
                var referee = user.Referees.Where(x => x.Email == user.Email).FirstOrDefault();
                if (referee != null)
                {
                    if (model.UniversityId == null)
                        model.UniversityId = referee.UniversityId ?? 0;
                    if (model.CollegeId == null)
                        model.CollegeId = referee.CollegeId ?? 0;
                    if (model.DepartmentID == null)
                        model.DepartmentID = referee.DepartmentId ?? 0;
                    if (model.DepartmentOther == null)
                        model.DepartmentOther = referee.DepartmentOther ?? "";

                    if (referee.Qualification != null)
                    {
                        model.Qualification = (QualificationType)referee.Qualification > 0 ? (QualificationType)referee.Qualification : QualificationType.Unspecified;
                    }
                    if (model.CountryId == null)
                        model.CountryId = referee.CountryId ?? 0;
                    if (model.CityId == null)
                        model.CityId = referee.CityId ?? 0;
                    if (model.NationalityId == null)
                        model.NationalityId = referee.NationalityId ?? 0;
                    if (model.MajorId == null)
                        model.MajorId = referee.MajorId;
                    model.RoleName = ApsUserRole.Referee.ToString();
                    model.Specialization = referee.Specialization ?? "";
                }
            }
            model.BankIbanPhotocopyReadonly = user.UserFiles
                                                .Where(f => f.Type == fileType)
                                                .OrderByDescending(f => f.Created)
                                                .Select(c => new AttachedFile
                                                {
                                                    FileId = c.Id.ToString(),
                                                    Type = (FileType)c.Type,
                                                    FileName = Path.GetFileName(c.FileName)
                                                })
                                                .FirstOrDefault();
            FillOutLookups(model);
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public new ActionResult Profile(EditUserViewModel model)
        {
            var userId = CurrentUser.Id;
            var user = (User)null;

            var fileType = (byte)FileType.IbanPhotocopy;

            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                user = DbContext.Users
                        .Include("Roles")
                        .Include("UserProfile")
                        .Include("College")
                        .SingleOrDefault(u => u.Id == model.UserId);
                if (user == null)
                {
                    ModelState.AddModelError("UserId", "هذا المستخدم غير موجود");
                }
            }
            if (model.UserName != user.UserName)
            {
                var isExists = DbContext.Users.Where(x => x.UserName == model.UserName).Count() > 0;
                if (isExists)
                {
                    ModelState.AddModelError("UserName", LangHelpers.Instance["username_must_be_unique"]);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "**********" && model.Password == model.ConfirmPassword)
            {
                if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "يجب إدخال كلمة المرور الحالية");
                }
                else if (!UserManager.CheckPassword(CurrentUser, model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "كلمة المرور الحالية لا تطابق المسجلة");
                }
                else
                {
                    var errorMsg = (string)null;
                    if (!Utils.ValidatePassword(model.Password, IsAdmin, out errorMsg))
                    {
                        ModelState.AddModelError("Password", errorMsg);
                    }
                }
            }

            var birthDate = DateTime.MinValue;

            //model.CollegeId = user.CollegeId;
            //model.CollegeName = user.CollegeId == null ? null : user.College.NameEnglish;
            //model.RoleId = user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Id;
            //model.RoleName = LangHelpers.Instance[user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Name];

            if (!string.IsNullOrWhiteSpace(model.NationalId))
            {
                if (!IsValidNationalId(model.NationalId))
                {
                    ModelState.AddModelError("NationalId", ApsStrings.invalid_national_id);
                }
            }
            else
            {
                ModelState.AddModelError("NationalId", ApsStrings.national_id_required);
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", LangHelpers.Instance["fullname_required"]);
            }
            else
            {
                var comps = Utils.GetNameComponents(model.FullName);

                if (comps.FirstName == null || comps.LastName == null || comps.ThirdName == null || comps.SecondName == null)
                {
                    ModelState.AddModelError("FullName", LangHelpers.Instance["quadname_required"]);
                }
            }

            var number = (PhoneNumber)null;

            if (!string.IsNullOrWhiteSpace(model.MobileNo))
            {
                var util = PhoneNumberUtil.GetInstance();

                try
                {
                    number = util.Parse(model.MobileNo, "SA");
                }
                catch (NumberParseException)
                {
                    ModelState.AddModelError("MobileNo", ApsStrings.invalid_mobile_number);
                }
            }
            else
            {
                ModelState.AddModelError("MobileNo", ApsStrings.mobile_number_required);
            }

            if (Utils.HasRolesAny(ApsUserRole.Researcher, ApsUserRole.Referee))
            {
                //model.UniversityId = 1;

                //if (!string.IsNullOrWhiteSpace(model.Email) && (!model.Email.Trim().ToLower().EndsWith("@imamu.edu.sa") && !model.Email.Trim().ToLower().EndsWith(".imamu.edu.sa") && !model.Email.Trim().ToLower().EndsWith("@kay-ict.com")))
                //{
                // ModelState.AddModelError("Email", LangHelpers.Instance.GetLangValueByString("email_must_be_imam_edu"));
                //}

                if (!(model.NationalityId > 0))
                {
                    ModelState.AddModelError("NationalityId", LangHelpers.Instance["nationality_required"]);
                }
                if (!(model.CountryId > 0))
                {
                    ModelState.AddModelError("CountryId", LangHelpers.Instance["country_required"]);
                }

                if ((model.CityId == null))
                {
                    ModelState.AddModelError("CityId", LangHelpers.Instance["country_required"]);
                }
                if (string.IsNullOrWhiteSpace(model.BirthDate) || !DateTime.TryParseExact(model.BirthDate.Trim(), "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out birthDate))
                {
                    ModelState.AddModelError("BirthDate", LangHelpers.Instance["birthdate_required"]);
                }
                else
                {
                    var age = Utils.CalculateAge(birthDate);
                    if (age < 17)
                    {
                        ModelState.AddModelError("BirthDate", LangHelpers.Instance["age_is_too_young"]);
                    }
                }

                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    ModelState.AddModelError("Address", LangHelpers.Instance["address_required"]);
                }
                if (model.UniversityId == 0 && string.IsNullOrWhiteSpace(model.UniversityOther))
                {
                    ModelState.AddModelError("UniversityOther", LangHelpers.Instance["university_required"]);
                }
                else if (!(model.UniversityId >= 0))
                {
                    ModelState.AddModelError("UniversityId", LangHelpers.Instance["university_required"]);
                }

                if (model.CollegeId == 0 && string.IsNullOrWhiteSpace(model.CollegeOther))
                {
                    ModelState.AddModelError("CollegeOther", LangHelpers.Instance["college_required"]);
                }
                else if (!(model.CollegeId >= 0))
                {
                    ModelState.AddModelError("CollegeId", LangHelpers.Instance["college_required"]);
                }
                if (model.DepartmentID == 0 && string.IsNullOrWhiteSpace(model.DepartmentOther))
                {
                    ModelState.AddModelError("DepartmentOther", LangHelpers.Instance["department_required"]);
                }
                else if (!(model.DepartmentID >= 0))
                {
                    ModelState.AddModelError("DepartmentID", LangHelpers.Instance["department_required"]);
                }
                if (string.IsNullOrWhiteSpace(model.Specialization))
                {
                    ModelState.AddModelError("Specialization", LangHelpers.Instance["specialization_required"]);
                }
                if (!(model.Qualification > 0))
                {
                    ModelState.AddModelError("Qualification", LangHelpers.Instance["qualification_required"]);
                }
                if (model.Bank == 0 && string.IsNullOrWhiteSpace(model.BankOther))
                {
                    ModelState.AddModelError("BankOther", LangHelpers.Instance["bank_required"]);
                }
                else if (!(model.Bank >= 0))
                {
                    ModelState.AddModelError("Bank", LangHelpers.Instance["bank_required"]);
                }

                if (string.IsNullOrWhiteSpace(model.BankAccountHolderName))
                {
                    ModelState.AddModelError("BankAccountHolderName", LangHelpers.Instance["account_holder_name_required"]);
                }
            }

            // Bank Details
            if (Utils.HasRolesAny(ApsUserRole.Researcher, ApsUserRole.CoResearcher, ApsUserRole.Referee))
            {
                var lang = (string)null;

                if (model.BankAccountNumberTypeId == (byte)BankAccountNumberType.Iban)
                {
                    fileType = (byte)FileType.IbanPhotocopy;
                    if (string.IsNullOrWhiteSpace(model.BankAccountIBAN))
                    {
                        ModelState.AddModelError("BankAccountIBAN", LangHelpers.Instance["iban_required"]);
                    }
                    else if (!Utils.ValidateIBAN(model.BankAccountIBAN))
                    {
                        ModelState.AddModelError("BankAccountIBAN", LangHelpers.Instance["invalid_iban"]);
                    }
                }
                else if (model.BankAccountNumberTypeId == (byte)BankAccountNumberType.Swift)
                {
                    fileType = (byte)FileType.SwiftPhotocopy;
                    if (string.IsNullOrWhiteSpace(model.SwiftCode))
                    {
                        ModelState.AddModelError("SwiftCode", LangHelpers.Instance["swift_required"]);
                    }
                    else if (!Utils.ValidateSWIFT(model.SwiftCode))
                    {
                        ModelState.AddModelError("SwiftCode", LangHelpers.Instance["invalid_swift"]);
                    }
                }

                if (model.BankIbanPhotocopy == null || model.BankIbanPhotocopy.ContentLength == 0)
                {
                    var oldFile = user.UserFiles
                                                .Where(f => f.Type == fileType)
                                                .OrderByDescending(f => f.Created)
                                                .Select(c => new AttachedFile
                                                {
                                                    FileId = c.Id.ToString(),
                                                    Type = (FileType)c.Type,
                                                    FileName = Path.GetFileName(c.FileName)
                                                })
                                                .FirstOrDefault();

                    if (oldFile == null)
                    {
                        ModelState.AddModelError("BankIbanPhotocopy", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("BankIbanPhotocopy", false, lang)));
                    }
                }
                else
                {
                    var extension = model.BankIbanPhotocopy.FileName == null ? null : Path.GetExtension(model.BankIbanPhotocopy.FileName).ToLower();
                    if (extension == null || (extension != ".pdf" && extension != ".doc" && extension != ".docx" && extension != ".jpeg" && extension != ".jpg" && extension != ".png"))
                    {
                        ModelState.AddModelError("BankIbanPhotocopy", LangHelpers.Instance["file_type_pdf_doc"]);
                    }
                }
            }
            if (Utils.HasRolesAny(ApsUserRole.Referee))
            {
                if ((model.MajorId ?? 0) == 0)
                {
                    ModelState.AddModelError("MajorId", LangHelpers.Instance["major_required"]);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (user.UserProfile == null)
                    {
                        user.UserProfile = new UserProfile
                        {
                            UserId = user.Id,
                            Created = DateTime.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
                            MobileNo = model.MobileNo
                        };

                        DbContext.UserProfiles.Add(user.UserProfile);
                        DbContext.SaveChanges();
                    }
                    if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "**********" && model.Password == model.ConfirmPassword)
                    {
                        user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                        user.ForcePasswordChange = false;
                    }
                    else if (CurrentUser.ForcePasswordChange)
                    {
                        TempData["message"] = ApsStrings.must_change_password;
                        TempData["message_type"] = "danger";

                        FillOutLookups(model);

                        return View(model);
                    }
                    // User Profile Data
                    if (number != null)
                    {
                        user.UserProfile.MobileNo = "0" + number.NationalNumber;
                    }

                    var cmps = Utils.GetNameComponents(model.FullName);

                    user.UserProfile.FirstName = cmps.FirstName;
                    user.UserProfile.MiddleName = cmps.SecondName;
                    user.UserProfile.ThirdName = cmps.ThirdName;
                    user.UserProfile.LastName = cmps.LastName;
                    user.UserProfile.NationalId = model.NationalId;
                    user.UserProfile.Updated = TM.Now;
                    user.UserProfile.NationalityId = model.NationalityId;
                    user.UserProfile.CityId = model.CityId == 0 ? null : model.CityId;
                    user.UserProfile.BirthDate = birthDate;
                    user.UserProfile.BankId = model.Bank == 0 ? null : model.Bank;

                    user.UserProfile.BankAccountHolder = model.BankAccountHolderName;
                    user.UserProfile.BankBranch = model.BankBranch;
                    user.UserProfile.BankIban = model.BankAccountIBAN;
                    user.UserProfile.BankAccountNumberTypeId = model.BankAccountNumberTypeId;
                    user.UserProfile.BankAccountTypeId = model.BankAccountTypeId;
                    user.UserProfile.SwiftCode = model.SwiftCode;
                    user.UserProfile.RoutingNumber = model.RoutingNumber;
                    user.UserProfile.BankName = model.BankOther;
                    user.UserProfile.Specialization = model.Specialization;
                    user.UserProfile.Qualification = (byte?)model.Qualification;

                    // User Data
                    user.UserName = model.UserName;
                    user.CountryId = model.CountryId;
                    user.UniversityId = model.UniversityId == 0 ? null : model.UniversityId;
                    user.CollegeId = model.CollegeId == 0 ? null : model.CollegeId;
                    user.DepartmentId = model.DepartmentID == 0 ? null : model.DepartmentID;
                    user.MajorId = model.MajorId == 0 ? null : model.MajorId;
                    user.Email = model.Email;

                    //user.IdentityNumber = model.NationalId;
                    user.Address = model.Address;
                    user.PhoneNumber = model.MobileNo;
                    user.Updated = TM.Now;
                    user.NationalId = model.NationalId;
                    user.FullName = model.FullName;
                    user.UniversityOther = model.UniversityOther;
                    user.CollegeOther = model.CollegeOther;
                    user.MajorOther = model.MajorName;
                    user.DepartmentOther = model.DepartmentOther;

                    if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "**********" && model.Password == model.ConfirmPassword)
                    {
                        user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                        user.ForcePasswordChange = false;
                    }
                    else if (CurrentUser.ForcePasswordChange)
                    {
                        TempData["message"] = "يجب تغيير كلمة المرور";
                        TempData["message_type"] = "danger";

                        return View(model);
                    }

                    if (number != null)
                    {
                        user.UserProfile.MobileNo = "0" + number.NationalNumber;
                    }

                    user.Updated = DateTime.Now;
                    user.UserProfile.Updated = DateTime.Now;

                    if (user.PhoneNumber != user.UserProfile.MobileNo)
                    {
                        user.PhoneNumber = user.UserProfile.MobileNo;
                    }

                    if (user.Created == null)
                    {
                        user.Created = DbContext.UserLogs.Count(u => u.UserId == user.Id) == 0 ? DateTime.Now : DbContext.UserLogs.OrderByDescending(u => u.Created).FirstOrDefault(u => u.UserId == user.Id).Created;
                    }
                    user.ForceToUpdateProfile = false;
                    DbContext.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    DbContext.Entry(user.UserProfile).State = System.Data.Entity.EntityState.Modified;

                    DbContext.SaveChanges();

                    var researcher = DbContext.Researchers.Where(x => x.UserId == user.Id).FirstOrDefault();
                    if (researcher != null)
                    {
                        researcher.NationalId = model.NationalId;
                        researcher.Qualification = (byte)model.Qualification;
                        researcher.DepartmentOther = model.DepartmentOther;
                        researcher.Specialization = model.Specialization;
                        researcher.CollegeOther = model.CollegeOther;
                        researcher.CollegeId = model.CollegeId == 0 ? null : model.CollegeId;
                        researcher.FirstName = cmps.FirstName;
                        researcher.LastName = cmps.LastName;
                        researcher.SecondName = cmps.SecondName;
                        researcher.ThirdName = cmps.ThirdName;
                        researcher.NationalityId = model.NationalityId;
                        researcher.NationalId = model.NationalId;
                        researcher.CountryId = model.CountryId;
                        researcher.CityId = model.CityId == 0 ? null : model.CityId;
                        researcher.UniversityId = model.UniversityId == 0 ? null : model.UniversityId;
                        researcher.DepartmentId = model.DepartmentID == 0 ? null : model.DepartmentID;
                        researcher.DepartmentOther = model.DepartmentOther;
                        researcher.Address = model.Address;
                        researcher.Email = model.Email;
                        researcher.MobileNo = model.MobileNo;
                        researcher.Updated = TM.Now;
                        researcher.UpdatedBy = CurrentUser.Id;
                        researcher.DateOfBirth = birthDate;
                        researcher.MajorId = model.MajorId == 0 ? null : model.MajorId;
                        researcher.ForceToUpdateProfile = false;
                        // CVs
                        DbContext.Entry(researcher).State = System.Data.Entity.EntityState.Modified;
                    }

                    DbContext.SaveChanges();

                    // Referees
                    var referee = DbContext.Referees.Where(x => x.UserId == user.Id).FirstOrDefault();
                    if (referee != null)
                    {
                        referee.NationalityId = model.NationalityId;
                        referee.Qualification = (byte)model.Qualification;
                        referee.DepartmentOther = model.DepartmentOther;
                        referee.Specialization = model.Specialization;
                        referee.CollegeId = model.CollegeId == 0 ? null : model.CollegeId;
                        referee.FirstName = cmps.FirstName;
                        referee.LastName = cmps.LastName;
                        referee.SecondName = cmps.SecondName;
                        referee.ThirdName = cmps.ThirdName;
                        referee.CountryId = model.CountryId;
                        referee.CityId = model.CityId == 0 ? null : model.CityId;
                        referee.UniversityId = model.UniversityId == 0 ? null : model.UniversityId;
                        referee.DepartmentId = model.DepartmentID == 0 ? null : model.DepartmentID;
                        referee.DepartmentOther = model.DepartmentOther;
                        referee.Address = model.Address;
                        referee.Email = model.Email;
                        referee.MobileNo = model.MobileNo;
                        referee.Updated = TM.Now;
                        referee.UpdatedBy = CurrentUser.Id;
                        referee.DateOfBirth = birthDate;
                        referee.MajorId = model.MajorId == 0 ? null : model.MajorId;
                        referee.ForceToUpdateProfile = false;

                        // CVs
                        DbContext.Entry(referee).State = System.Data.Entity.EntityState.Modified;
                    }
                    if (model.BankIbanPhotocopy != null && model.BankIbanPhotocopy.ContentLength > 0)
                    {
                        var file = model.BankIbanPhotocopy;
                        var oldFile = user.UserFiles
                                    .Where(f => f.Type == fileType)
                                   .OrderByDescending(f => f.Created)
                                   .FirstOrDefault();

                        var fileId = oldFile == null ? Guid.NewGuid() : oldFile.Id;
                        var fileName = Path.GetFileName(file.FileName);
                        var extension = Path.GetExtension(fileName);
                        var path = Path.Combine(Server.MapPath("~/Content/files/"), fileId + extension);

                        file.EnhancedSaveAs(path);

                        if (oldFile == null)
                        {
                            oldFile = new UserFile
                            {
                                Id = fileId,
                                UserId = user.Id,
                                Type = (byte)fileType,
                                CreatedBy = Utils.CurrentUser.Id,
                                Updated = TM.Now,
                                UpdatedBy = CurrentUser.Id,

                                FileName = Path.GetFileName(file.FileName),
                                Extension = extension,
                                Created = TM.Now,
                                FilePath = path
                            };

                            DbContext.UserFiles.Add(oldFile);
                        }
                        else
                        {
                            oldFile.FilePath = path;
                            oldFile.Extension = extension;
                            oldFile.FileName = Path.GetFileName(file.FileName);
                            oldFile.Updated = TM.Now;
                            oldFile.UpdatedBy = CurrentUser.Id;
                        }
                    }
                    DbContext.SaveChanges();

                    TempData["message"] = "تم تحديث الملف بنجاح";
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index", "Account");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    Logger.LogExceptionAsync(ex, model);
                    TempData["message"] = ApsStrings.failed + " " + ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                    TempData["message_type"] = "danger";
                }
                catch (Exception ex)
                {
                    Logger.LogExceptionAsync(ex, model);

                    var ex2 = ex;
                    while (ex2.InnerException != null)
                    {
                        ex2 = ex2.InnerException;
                    }

                    TempData["message"] = ApsStrings.failed + " " + ex2.Message;
                    TempData["message_type"] = "danger";
                }
            }

            model.IsActivated = user == null ? false : user.IsActivated;
            FillOutLookups(model);
            return View(model);
        }

        public ActionResult LogOff()
        {
            if (User != null && User.Identity != null && CurrentUser != null && CurrentUser.Id != null)
            {
                var sessionClaim = SignInManager.AuthenticationManager.User.Claims.FirstOrDefault(c => c.Type == "SESSION_ID");

                if (sessionClaim != null)
                {
                    var session = DbContext.UserSessions.OrderByDescending(v => v.Created).FirstOrDefault(s => s.Sessionid == sessionClaim.Value);
                    if (session != null)
                    {
                        session.Locked = true;
                        session.Lockdate = DateTime.Now;
                    }
                }

                DbContext.UserLogs.Add(new UserLog
                {
                    Created = DateTime.Now,
                    UserId = CurrentUser.Id,
                    OperationCode = "LOGOUT"
                });

                DbContext.SaveChanges();
            }
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Abandon();
            Response.Cookies.Add(new HttpCookie("id", Guid.NewGuid().ToString()));
            return RedirectToAction("Index", "Home");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register(ApsUserRole? role, int? step)
        {
            var model = new RegistrationViewModel()
            {
                Step = step
            };

            if (role == null)
            {
                role = ApsUserRole.Researcher;
            }

            if (!(model.Step >= 1))
            {
                model.Step = 1;
            }

            GetDataFromSession(ref model, true);

            if (!String.IsNullOrEmpty(model.ExternalRefereeHashParams) && !String.IsNullOrEmpty(model.Email))
            {
                UserSession.EmailVerified = model.Email;
            }

            if (!(model.LastSavedStep >= 1))
            {
                model.LastSavedStep = 0;
            }

            if (model.Step > 5 || model.Step > model.LastSavedStep + 1)
            {
                return Redirect(Utils.GetRegisterLink(role.Value, model.LastSavedStep + 1));
            }

            ViewBag.Countries = new SelectList(DbContext.Countries
                .OrderBy(c => c.NameArabic == "المملكة العربية السعودية" ? 1 : 2)
                .ThenBy(c => c.NameArabic), "Id", "NameArabic");

            ViewBag.Roles = new SelectList(
                            new List<SelectListItem>
                            {
                                new SelectListItem { Selected = true, Text = LangHelpers.Instance["researcher"], Value = "1" },
                                new SelectListItem { Selected = false, Text = LangHelpers.Instance["referee"], Value = "5" }
                            }, "Value", "Text", 1);

            model.CountryId = 10;

            model.Role = role;

            FillOutLookups(model);
            ViewBag.IncludeCKEditor = true;

            if (model.Step == 4)
            {
                ViewBag.IsMobileVerified = UserSession != null && UserSession.VerifiedMobile != null && UserSession.VerifiedMobile == model.MobileNumber;
                ViewBag.IsEmailVerified = UserSession != null && UserSession.EmailVerified != null && UserSession.EmailVerified == model.Email;
            }

            if (role == null || (role != ApsUserRole.Researcher && role != ApsUserRole.Referee))
            {
                return View();
            }
            else if (role == ApsUserRole.Researcher)
            {
                return View("RegisterResearcherSteps", model);
            }
            else
            {
                return View("RegisterResearcherSteps", model);
            }
        }

        private void GetDataFromSession(ref RegistrationViewModel model, bool overwriteCurrentStep = false)
        {
            if (model == null) model = new RegistrationViewModel();

            var db = Utils.db;
            var session = UserSession;
            var data = db.UserSessionDatas.Where(x => x.SessionId == UserSession.Id).ToList();

            model.LastSavedStep = GetKeyValue<int?>("LastSavedStep", data);
            model.ExternalRefereeHashParams = GetKeyValue<string>("ExternalRefereeHashParams", data);

            if (overwriteCurrentStep || model.Step != 1)
            {
                model.NationalId = GetKeyValue<string>("NationalId", data);
                model.FullName = GetKeyValue<string>("FullName", data);
                model.UniversityOther = GetKeyValue<string>("UniversityOther", data);
                model.CollegeOther = GetKeyValue<string>("CollegeOther", data);
                model.DepartmentOther = GetKeyValue<string>("DepartmentOther", data);
                model.MajorOther = GetKeyValue<string>("MajorOther", data);
                model.BirthDate = GetKeyValue<string>("BirthDate", data);
                model.UniversityId = GetKeyValue<int?>("UniversityId", data);
                model.UniversityName = GetKeyValue<string>("UniversityName", data);
                model.MajorId = GetKeyValue<int?>("MajorId", data);
                model.NationalityId = GetKeyValue<int?>("NationalityId", data);
                model.CountryId = GetKeyValue<int?>("CountryId", data);
                model.CollegeId = GetKeyValue<int?>("CollegeId", data);
                model.Address = GetKeyValue<string>("Address", data);
                model.CityId = GetKeyValue<int?>("CityId", data);
                model.Qualification = GetKeyValue<QualificationType?>("Qualification", data);
                model.DepartmentID = GetKeyValue<int?>("DepartmentID", data);
                model.Specialization = GetKeyValue<string>("Specialization", data);
            }

            if (overwriteCurrentStep || model.Step != 2)
            {
                model.Bank = GetKeyValue<int?>("Bank", data);
                var fileId = GetKeyValue<string>("BankIbanPhotocopyReadonly", data);
                if (fileId != null)
                {
                    model.BankIbanPhotocopyReadonly = db.UserSessionFiles
                                                    .Where(f => f.Id.ToString() == fileId && f.SessionId == UserSession.Id)
                                                    .ToList()
                                                    .Select(c => new AttachedFile
                                                    {
                                                        FileId = c.Id.ToString(),
                                                        Type = (FileType)c.Type,
                                                        FileName = Path.GetFileName(c.FileName)
                                                    })
                                                    .FirstOrDefault();
                }

                model.BankOther = GetKeyValue<string>("BankOther", data);
                model.BankName = GetKeyValue<string>("BankName", data);
                model.BankBranch = GetKeyValue<string>("BankBranch", data);
                model.BankAccountHolderName = GetKeyValue<string>("BankAccountHolderName", data);
                model.BankAccountIBAN = GetKeyValue<string>("BankAccountIBAN", data);
                model.BankAccountTypeId = GetKeyValue<int?>("BankAccountTypeId", data);
                model.BankAccountNumberTypeId = GetKeyValue<int?>("BankAccountNumberTypeId", data);
                model.SwiftCode = GetKeyValue<string>("SwiftCode", data);
                model.RoutingNumber = GetKeyValue<string>("RoutingNumber", data);
            }

            if (overwriteCurrentStep || model.Step != 3)
            {
                var fileId = GetKeyValue<string>("CvFileReadonly", data);
                if (fileId != null)
                {
                    model.CvFileReadonly = db.UserSessionFiles
                                                    .Where(f => f.Id.ToString() == fileId && f.SessionId == UserSession.Id)
                                                    .ToList()
                                                    .Select(c => new AttachedFile
                                                    {
                                                        FileId = c.Id.ToString(),
                                                        Type = (FileType)c.Type,
                                                        FileName = Path.GetFileName(c.FileName)
                                                    })
                                                    .FirstOrDefault();
                }
                //model.SectionPublishedPapers = GetKeyValue<string>("SectionPublishedPapers", data);
                //model.SectionInterests = GetKeyValue<string>("SectionInterests", data);
                //model.SectionCareerHistory = GetKeyValue<string>("SectionCareerHistory", data);
                //model.SectionAcademicHistory = GetKeyValue<string>("SectionAcademicHistory", data);
                //model.SectionTrainingCourses = GetKeyValue<string>("SectionTrainingCourses", data);
                //model.SectionAwards = GetKeyValue<string>("SectionAwards", data);
                //model.SectionMemberships = GetKeyValue<string>("SectionMemberships", data);
                //model.CvText = GetKeyValue<string>("CvText", data);
            }

            if (overwriteCurrentStep || model.Step != 4)
            {
                model.Email = GetKeyValue<string>("Email", data);
                model.MobileNumber = GetKeyValue<string>("MobileNumber", data);
            }
        }

        private void SaveDataToSession(RegistrationViewModel model)
        {
            if (model == null) return;

            var session = UserSession;

            var db = Utils.db;
            var data = db.UserSessionDatas.Where(x => x.SessionId == UserSession.Id).ToList();

            if (model.Step == 1)
            {
                SaveKeyValue<string>("NationalId", model.NationalId, data);
                SaveKeyValue<string>("FullName", model.FullName, data);
                SaveKeyValue<string>("UniversityOther", model.UniversityOther, data);
                SaveKeyValue<string>("CollegeOther", model.CollegeOther, data);
                SaveKeyValue<string>("DepartmentOther", model.DepartmentOther, data);
                SaveKeyValue<string>("MajorOther", model.MajorOther, data);
                SaveKeyValue<string>("BirthDate", model.BirthDate, data);
                SaveKeyValue<int?>("UniversityId", model.UniversityId, data);
                SaveKeyValue<string>("UniversityName", model.UniversityName, data);
                SaveKeyValue<int?>("MajorId", model.MajorId, data);
                SaveKeyValue<int?>("NationalityId", model.NationalityId, data);
                SaveKeyValue<int?>("CountryId", model.CountryId, data);
                SaveKeyValue<int?>("CollegeId", model.CollegeId, data);
                SaveKeyValue<string>("Address", model.Address, data);
                SaveKeyValue<int?>("CityId", model.CityId, data);
                SaveKeyValue<QualificationType?>("Qualification", model.Qualification, data);
                SaveKeyValue<int?>("DepartmentID", model.DepartmentID, data);
                SaveKeyValue<string>("Specialization", model.Specialization, data);
            }
            else if (model.Step == 2)
            {
                SaveKeyValue<int?>("Bank", model.Bank, data);

                var file = (UserSessionFile)null;
                var files = Request.Files;
                for (var i = 0; i < files.AllKeys.Count(); i++)
                {
                    var ff = Request.Files[i];

                    if (!(ff.ContentLength > 0)) continue;

                    var fileName = Path.GetFileName(ff.FileName);
                    var extension = Path.GetExtension(fileName);
                    var fileType = (byte)FileType.IbanPhotocopy;
                    if (model.BankAccountNumberTypeId == (byte)BankAccountNumberType.Swift)
                    {
                        fileType = (byte)FileType.SwiftPhotocopy;
                    }

                    file = new Data.DbModels.UserSessionFile
                    {
                        Id = Guid.NewGuid(),
                        Type = fileType,
                        CreatedBy = UserId,
                        FileName = Path.GetFileName(ff.FileName),
                        Extension = extension,
                        SessionId = UserSession.Id,
                        Created = DateTime.Now
                    };

                    file.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), file.Id + extension);
                    ff.EnhancedSaveAs(file.FilePath);

                    db.UserSessionFiles.Add(file);

                    db.SaveChanges();

                    break;
                }

                if (file != null)
                {
                    SaveKeyValue<string>("BankIbanPhotocopyReadonly", file.Id.ToString(), data);
                }

                SaveKeyValue<string>("BankOther", model.BankOther, data);
                SaveKeyValue<string>("BankName", model.BankName, data);
                SaveKeyValue<string>("BankBranch", model.BankBranch, data);
                SaveKeyValue<string>("BankAccountHolderName", model.BankAccountHolderName, data);
                SaveKeyValue<string>("BankAccountIBAN", model.BankAccountIBAN, data);

                SaveKeyValue<int?>("BankAccountTypeId", model.BankAccountTypeId, data);
                SaveKeyValue<int?>("BankAccountNumberTypeId", model.BankAccountNumberTypeId, data);

                SaveKeyValue<string>("SwiftCode", model.SwiftCode, data);
                SaveKeyValue<string>("RoutingNumber", model.RoutingNumber, data);
            }
            else if (model.Step == 3)
            {
                var file = (UserSessionFile)null;
                var files = Request.Files;
                for (var i = 0; i < files.AllKeys.Count(); i++)
                {
                    var ff = Request.Files[i];

                    if (!(ff.ContentLength > 0)) continue;

                    var fileName = Path.GetFileName(ff.FileName);
                    var extension = Path.GetExtension(fileName);
                    var fileType = (byte)FileType.Resume;

                    file = new Data.DbModels.UserSessionFile
                    {
                        Id = Guid.NewGuid(),
                        Type = fileType,
                        CreatedBy = UserId,
                        FileName = Path.GetFileName(ff.FileName),
                        Extension = extension,
                        SessionId = UserSession.Id,
                        Created = DateTime.Now
                    };

                    file.FilePath = Path.Combine(Server.MapPath("~/Content/files/"), file.Id + extension);
                    ff.EnhancedSaveAs(file.FilePath);

                    db.UserSessionFiles.Add(file);

                    db.SaveChanges();

                    break;
                }

                if (file != null)
                {
                    SaveKeyValue<string>("CvFileReadonly", file.Id.ToString(), data);
                }
                //SaveKeyValue<string>("SectionPublishedPapers", model.SectionPublishedPapers, data);
                //SaveKeyValue<string>("SectionInterests", model.SectionInterests, data);
                //SaveKeyValue<string>("SectionCareerHistory", model.SectionCareerHistory, data);
                //SaveKeyValue<string>("SectionAcademicHistory", model.SectionAcademicHistory, data);
                //SaveKeyValue<string>("SectionTrainingCourses", model.SectionTrainingCourses, data);
                //SaveKeyValue<string>("SectionAwards", model.SectionAwards, data);
                //SaveKeyValue<string>("SectionMemberships", model.SectionMemberships, data);
                //SaveKeyValue<string>("CvText", model.CvText, data);
            }
            else if (model.Step == 4)
            {
                SaveKeyValue<string>("Email", model.Email, data);
                SaveKeyValue<string>("MobileNumber", model.MobileNumber, data);
            }

            SaveKeyValue<int?>("LastSavedStep", model.Step, data);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(RegistrationViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Register", "Account");
            }

            var res = this.Register(model.Role, model.Step);
            if (res is RedirectResult || res is RedirectToRouteResult)
            {
                return res;
            }

            var model2 = (RegistrationViewModel)((ViewResult)res).Model;

            model.Step = model2.Step;
            model.LastSavedStep = model2.LastSavedStep;

            //if (model.Role == ApsUserRole.Researcher)
            //    model.Role = ApsUserRole.PrincipalResearcher;

            if (model.Role == null || (model.Role != ApsUserRole.Researcher && model.Role != ApsUserRole.Referee))
            {
                return RedirectToAction("Register", "Account");
            }

            var db = Utils.db;

            var birthDate = DateTime.MinValue;

            if (!string.IsNullOrWhiteSpace(model2.BirthDate) && DateTime.TryParseExact(model2.BirthDate.Trim(), "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out birthDate))
            {
            }

            switch (model.Step)
            {
                case 1:
                    {
                        if (!(model.CountryId > 0))
                        {
                            ModelState.AddModelError("CountryId", LangHelpers.Instance["residence_country_required"]);
                        }

                        if (!(model.CityId > 0))
                        {
                            ModelState.AddModelError("CityId", LangHelpers.Instance["residence_city_required"]);
                        }

                        if (!(model.Qualification > 0))
                        {
                            ModelState.AddModelError("Qualification", LangHelpers.Instance["qualification_required"]);
                        }

                        if (string.IsNullOrWhiteSpace(model.Specialization))
                        {
                            ModelState.AddModelError("Specialization", LangHelpers.Instance["specialization_required"]);
                        }

                        if (string.IsNullOrWhiteSpace(model.FullName))
                        {
                            ModelState.AddModelError("FullName", LangHelpers.Instance["quadname_required"]);
                        }
                        else
                        {
                            var comps = Utils.GetNameComponents(model.FullName);

                            if (comps.FirstName == null || comps.LastName == null || comps.ThirdName == null || comps.SecondName == null)
                            {
                                ModelState.AddModelError("FullName", LangHelpers.Instance["quadname_required"]);
                            }
                        }

                        birthDate = DateTime.MinValue;

                        if (string.IsNullOrWhiteSpace(model.BirthDate) || !DateTime.TryParseExact(model.BirthDate.Trim(), "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out birthDate))
                        {
                            ModelState.AddModelError("BirthDate", LangHelpers.Instance["birthdate_required"]);
                        }
                        else
                        {
                            var age = Utils.CalculateAge(birthDate);
                            if (age < 17)
                            {
                                ModelState.AddModelError("BirthDate", LangHelpers.Instance["age_is_too_young"]);
                            }
                        }

                        if (string.IsNullOrWhiteSpace(model.NationalId))
                        {
                            ModelState.AddModelError("NationalId", LangHelpers.Instance["national_id_required"]);
                        }
                        else if (!IsValidNationalId(model.NationalId))
                        {
                            ModelState.AddModelError("NationalId", LangHelpers.Instance["invalid_national_id"]);
                        }

                        if (model.UniversityId == 0 && string.IsNullOrWhiteSpace(model.UniversityOther))
                        {
                            ModelState.Remove("UniversityId");
                            ModelState.Remove("CollegeId");
                            ModelState.AddModelError("UniversityOther", LangHelpers.Instance["university_required"]);
                        }
                        else if (!(model.UniversityId >= 0))
                        {
                            ModelState.AddModelError("UniversityId", LangHelpers.Instance["university_required"]);
                        }

                        if (model.CollegeId == 0 && string.IsNullOrWhiteSpace(model.CollegeOther))
                        {
                            ModelState.Remove("CollegeId");
                            ModelState.AddModelError("CollegeOther", LangHelpers.Instance["college_required"]);
                        }
                        else if (!(model.CollegeId >= 0))
                        {
                            ModelState.AddModelError("CollegeId", LangHelpers.Instance["college_required"]);
                        }

                        if (model.DepartmentID == 0 && string.IsNullOrWhiteSpace(model.DepartmentOther))
                        {
                            ModelState.Remove("DepartmentID");
                            ModelState.AddModelError("DepartmentOther", LangHelpers.Instance["department_required"]);
                        }
                        else if (!(model.DepartmentID >= 0))
                        {
                            ModelState.AddModelError("DepartmentID", LangHelpers.Instance["department_required"]);
                        }

                        if (!(model.NationalityId > 0))
                        {
                            ModelState.AddModelError("NationalityId", LangHelpers.Instance["nationality_required"]);
                        }

                        //if (model.Role == ApsUserRole.Referee)
                        //{
                        //    if (string.IsNullOrWhiteSpace(model.MobileNumber))
                        //    {
                        //        ModelState.AddModelError("MobileNumber", LangHelpers.Instance["mobile_required"]);
                        //    }
                        //    else
                        //    {
                        //        model.MobileNumber = model.MobileNumber.Trim().ToLower();
                        //        var phone = "";
                        //        if (!Utils.ValidateMobile(model.MobileNumber, out phone))
                        //        {
                        //            ModelState.AddModelError("MobileNumber", LangHelpers.Instance["invalid_mobile"]);
                        //        }
                        //        else
                        //        {
                        //            model.MobileNumber = phone;
                        //            ViewBag.IsMobileVerified = UserSession != null && UserSession.VerifiedMobile != null && UserSession.VerifiedMobile == model.MobileNumber;

                        //            if (ViewBag.IsMobileVerified != true)
                        //            {
                        //                ModelState.AddModelError("MobileNumber", LangHelpers.Instance["mobile_not_verified"]);
                        //            }
                        //        }
                        //    }
                        //    if (string.IsNullOrWhiteSpace(model.Password))
                        //    {
                        //        ModelState.AddModelError("Password", LangHelpers.Instance["password_required"]);
                        //    }
                        //}
                    }
                    break;

                case 2:
                    {
                        if (model.Bank == 0 && string.IsNullOrWhiteSpace(model.BankOther))
                        {
                            ModelState.AddModelError("BankOther", LangHelpers.Instance["bank_required"]);
                        }
                        else if (!(model.Bank >= 0))
                        {
                            ModelState.AddModelError("Bank", LangHelpers.Instance["bank_required"]);
                        }

                        if (string.IsNullOrWhiteSpace(model.BankAccountHolderName))
                        {
                            ModelState.AddModelError("BankAccountHolderName", LangHelpers.Instance["account_holder_name_required"]);
                        }

                        if (model.BankAccountNumberTypeId == (byte)BankAccountNumberType.Iban)
                        {
                            if (string.IsNullOrWhiteSpace(model.BankAccountIBAN))
                            {
                                ModelState.AddModelError("BankAccountIBAN", LangHelpers.Instance["iban_required"]);
                            }
                            else if (!Utils.ValidateIBAN(model.BankAccountIBAN))
                            {
                                ModelState.AddModelError("BankAccountIBAN", LangHelpers.Instance["invalid_iban"]);
                            }
                        }
                        else if (model.BankAccountNumberTypeId == (byte)BankAccountNumberType.Swift)
                        {
                            if (string.IsNullOrWhiteSpace(model.SwiftCode))
                            {
                                ModelState.AddModelError("SwiftCode", LangHelpers.Instance["swift_required"]);
                            }
                            else if (!Utils.ValidateSWIFT(model.SwiftCode))
                            {
                                ModelState.AddModelError("SwiftCode", LangHelpers.Instance["invalid_swift"]);
                            }
                            //if (string.IsNullOrWhiteSpace(model.RoutingNumber))
                            //{
                            //    ModelState.AddModelError("RoutingNumber", LangHelpers.Instance["routing_number_required"]);
                            //}
                            //else if (!Utils.ValidateSWIFT(model.RoutingNumber))
                            //{
                            //    ModelState.AddModelError("RoutingNumber", LangHelpers.Instance["invalid_routing_number"]);
                            //}
                        }

                        {
                            var foundIban = false;

                            var uploadedIban = (HttpPostedFileBase)null;

                            uploadedIban = Request.Files["BankIbanPhotocopy"];

                            if (uploadedIban != null && uploadedIban.ContentLength > 0)
                            {
                                foundIban = true;
                            }

                            if (!foundIban)
                            {
                                if (model2.BankIbanPhotocopyReadonly == null)
                                    ModelState.AddModelError("BankIbanPhotocopy", LangHelpers.Instance["iban_photo_required"]);
                            }
                            else
                            {
                                var extension = Path.GetExtension(uploadedIban.FileName).ToLower();
                                if (extension == null || (extension != ".pdf" && extension != ".doc" && extension != ".docx" && extension != ".jpeg" && extension != ".jpg" && extension != ".png"))
                                {
                                    ModelState.AddModelError("BankIbanPhotocopy", LangHelpers.Instance["file_type_pdf_doc"]);
                                }
                            }
                        }
                    }
                    break;

                case 3:
                    {
                        {
                            var foundCv = false;
                            var uploadedCv = (HttpPostedFileBase)null;

                            uploadedCv = Request.Files["CvFile"];

                            if (uploadedCv != null && uploadedCv.ContentLength > 0)
                            {
                                foundCv = true;
                            }

                            if (!foundCv)
                            {
                                if (model2.CvFileReadonly == null)
                                    ModelState.AddModelError("CvFile", LangHelpers.Instance["resume_required"]);
                            }
                            else
                            {
                                var extension = Path.GetExtension(uploadedCv.FileName).ToLower();
                                if (extension == null || (extension != ".pdf" && extension != ".doc" && extension != ".docx"))
                                {
                                    ModelState.AddModelError("CvFile", LangHelpers.Instance["file_type_pdf_doc"]);
                                }
                            }
                        }
                        //if (model.Role == ApsUserRole.PrincipalResearcher)
                        //{
                        //    var lang = (string)null;
                        //    if (string.IsNullOrWhiteSpace(model.SectionPublishedPapers) || Utils.StripHTML(model.SectionPublishedPapers).Trim().Length == 0)
                        //    {
                        //        ModelState.AddModelError("SectionPublishedPapers", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("SectionPublishedPapers", false, lang)));
                        //    }

                        //    if (string.IsNullOrWhiteSpace(model.SectionInterests) || Utils.StripHTML(model.SectionInterests).Trim().Length == 0)
                        //    {
                        //        ModelState.AddModelError("SectionInterests", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("SectionInterests", false, lang)));
                        //    }

                        //    if (string.IsNullOrWhiteSpace(model.SectionCareerHistory) || Utils.StripHTML(model.SectionCareerHistory).Trim().Length == 0)
                        //    {
                        //        ModelState.AddModelError("SectionCareerHistory", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("SectionCareerHistory", false, lang)));
                        //    }

                        //    if (string.IsNullOrWhiteSpace(model.SectionAcademicHistory) || Utils.StripHTML(model.SectionAcademicHistory).Trim().Length == 0)
                        //    {
                        //        ModelState.AddModelError("SectionAcademicHistory", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("SectionAcademicHistory", false, lang)));
                        //    }

                        //    if (string.IsNullOrWhiteSpace(model.SectionTrainingCourses) || Utils.StripHTML(model.SectionTrainingCourses).Trim().Length == 0)
                        //    {
                        //        ModelState.AddModelError("SectionTrainingCourses", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("SectionTrainingCourses", false, lang)));
                        //    }

                        //    if (string.IsNullOrWhiteSpace(model.SectionAwards) || Utils.StripHTML(model.SectionAwards).Trim().Length == 0)
                        //    {
                        //        ModelState.AddModelError("SectionAwards", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("SectionAwards", false, lang)));
                        //    }

                        //    if (string.IsNullOrWhiteSpace(model.SectionMemberships) || Utils.StripHTML(model.SectionMemberships).Trim().Length == 0)
                        //    {
                        //        ModelState.AddModelError("SectionMemberships", string.Format(LangHelpers.Instance.GetLangValueByString("field_is_required", false, lang), LangHelpers.Instance.GetLangValueByString("SectionMemberships", false, lang)));
                        //    }
                        //}
                    }
                    break;

                case 4:
                    {
                        if (string.IsNullOrWhiteSpace(model.Email))
                        {
                            ModelState.AddModelError("Email", LangHelpers.Instance["email_required"]);
                        }
                        else
                        {
                            model.Email = model.Email.Trim().ToLower();

                            if (!Utils.ValidateEmail(model.Email))
                            {
                                ModelState.AddModelError("Email", LangHelpers.Instance["invalid_email"]);
                            }
                            else
                            {
                                //if (model2.UniversityId == 1 && (!model.Email.Trim().ToLower().EndsWith("@imamu.edu.sa") && !model.Email.Trim().ToLower().EndsWith("@kay-ict.com")))
                                //{
                                //    ModelState.AddModelError("Email", LangHelpers.Instance.GetLangValueByString("email_must_be_imam_edu"));
                                //}

                                if (model.EmailConfirmation.Trim().ToLower() != model.Email)
                                {
                                    ModelState.AddModelError("EmailConfirmation", LangHelpers.Instance["email_confirmation_mismatch"]);
                                }
                                else if (db.Users.FirstOrDefault(u => u.Email == model.Email) != null)
                                {
                                    ModelState.AddModelError("Email", LangHelpers.Instance["email_exists"]);
                                }
                                else
                                {
                                    ViewBag.IsEmailVerified = UserSession != null && UserSession.EmailVerified != null && UserSession.EmailVerified == model.Email;

                                    if (ViewBag.IsEmailVerified != true)
                                    {
                                        ModelState.AddModelError("Email", LangHelpers.Instance["email_not_verified"]);
                                    }
                                }
                            }
                        }

                        if (string.IsNullOrWhiteSpace(model.MobileNumber))
                        {
                            ModelState.AddModelError("MobileNumber", LangHelpers.Instance["mobile_required"]);
                        }
                        else
                        {
                            model.MobileNumber = model.MobileNumber.Trim().ToLower();
                            var phone = "";
                            if (!Utils.ValidateMobile(model.MobileNumber, out phone))
                            {
                                ModelState.AddModelError("MobileNumber", LangHelpers.Instance["invalid_mobile"]);
                            }
                            else
                            {
                                model.MobileNumber = phone;

                                ViewBag.IsMobileVerified = UserSession != null && UserSession.VerifiedMobile != null && UserSession.VerifiedMobile == model.MobileNumber;

                                if (ViewBag.IsMobileVerified != true)
                                {
                                    ModelState.AddModelError("MobileNumber", LangHelpers.Instance["mobile_not_verified"]);
                                }
                            }
                        }
                    }

                    break;

                case 5:
                    {
                        if (model.Password == null)
                        {
                            ModelState.AddModelError("Password", LangHelpers.Instance["password_required"]);
                        }
                    }
                    break;
            }

            if (ModelState.IsValid)
            {
                if (model.Step == 1)
                {
                    if (model.UniversityId == 0) model.UniversityId = null;
                    if (model.CollegeId == 0) model.CollegeId = null;
                    if (model.DepartmentID == 0) model.DepartmentID = null;
                    if (model.Bank == 0) model.Bank = null;
                }

                if (model.Step < 5)
                {
                    SaveDataToSession(model);
                    TempData["message"] = LangHelpers.Instance["data_saved_successfully"];
                    TempData["message_type"] = "success";
                    return Redirect(Utils.GetRegisterLink(model.Role.Value, model.Step.Value + 1));
                }
                else
                {
                    using (var trans2 = DbContext.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            var hasher = new PasswordHasher();

                            var appUser = new User
                            {
                                Id = Guid.NewGuid().ToString(),
                                FullName = model2.FullName,
                                UserName = model2.Email,
                                Email = model2.Email,
                                PhoneNumber = model2.MobileNumber,
                                CountryId = model2.CountryId == 0 ? null : model2.CountryId,
                                CollegeId = model2.CollegeId == 0 ? null : model2.CollegeId,
                                CollegeOther = model2.CollegeOther,
                                UniversityOther = model2.UniversityOther,
                                MajorOther = model2.MajorOther,
                                Address = model2.Address,
                                PasswordHash = hasher.HashPassword(model.Password),
                                SecurityStamp = "a14ee95e-c73e-466e-a786-aaf4378237d3",
                                AccessFailedCount = 0,
                                CreatedBy = UserId,
                                UpdatedBy = UserId,
                                DepartmentId = model2.DepartmentID,
                                UniversityId = model2.UniversityId,
                                MajorId = model2.MajorId,
                                Roles = db.Roles.Where(r => r.Name == model.Role.Value.ToString()).ToList(),
                                Created = DateTime.Now,
                                Updated = DateTime.Now,
                                IsActivated = (model2.Role == ApsUserRole.Researcher || model2.Role == ApsUserRole.Referee),
                                EmailConfirmed = UserSession.EmailVerified == model2.Email,
                                PhoneNumberConfirmed = UserSession.VerifiedMobile == model2.MobileNumber,
                                DepartmentOther = model2.DepartmentOther,
                                NationalId = model2.NationalId
                            };

                            db.Users.Add(appUser);
                            db.SaveChanges();

                            var gKey = "";
                            var err = "";

                            {
                                var comps = Utils.GetNameComponents(model2.FullName);

                                var profile = new UserProfile
                                {
                                    UserId = appUser.Id,
                                    FirstName = comps.FirstName,
                                    MiddleName = comps.SecondName,
                                    ThirdName = comps.ThirdName,
                                    LastName = comps.LastName,
                                    AddressLine1 = model2.Address,
                                    BirthDate = birthDate,
                                    NationalityId = model2.NationalityId,
                                    BankName = model2.BankOther,
                                    BankBranch = model2.BankBranch,
                                    BankIban = model2.BankAccountIBAN,
                                    BankAccountHolder = model2.BankAccountHolderName,
                                    NationalId = model2.NationalId,
                                    MobileNo = model2.MobileNumber,
                                    MobileNumber = model2.MobileNumber,
                                    BankId = model2.Bank == 0 ? null : model2.Bank,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    CreatedBy = appUser.Id,
                                    UpdatedBy = appUser.Id,
                                    BankAccountTypeId = model2.BankAccountTypeId,
                                    BankAccountNumberTypeId = model2.BankAccountNumberTypeId,
                                    SwiftCode = model2.SwiftCode,
                                    RoutingNumber = model2.RoutingNumber,
                                    Specialization = model2.Specialization,
                                    Qualification = model2.Qualification == null ? (byte)QualificationType.Unspecified : (byte)model2.Qualification,

                                    CityId = model2.CityId
                                    //CvSectionPublishedPapers = model2.SectionPublishedPapers,
                                    //CvSectionInterests = model2.SectionInterests,
                                    //CvSectionCareerHistory = model2.SectionCareerHistory,
                                    //CvSectionTrainingCourses = model2.SectionTrainingCourses,
                                    //CvSectionAwards = model2.SectionAwards,
                                    //CvSectionMemberships = model2.SectionMemberships,
                                    //CvSectionAcademicHistory = model2.SectionAcademicHistory
                                };

                                db.UserProfiles.Add(profile);

                                db.SaveChanges();
                                if (model2.Role == ApsUserRole.Researcher)
                                {
                                    var researcher = new Researcher
                                    {
                                        UserId = appUser.Id,
                                        FirstName = comps.FirstName,
                                        SecondName = comps.SecondName,
                                        ThirdName = comps.ThirdName,
                                        LastName = comps.LastName,
                                        NationalId = model2.NationalId,
                                        AccountOwner = model2.BankAccountHolderName,

                                        NationalityId = model2.NationalityId,
                                        Email = model2.Email,
                                        MobileNo = model2.MobileNumber,
                                        CollegeId = model2.CollegeId,
                                        DateOfBirth = birthDate,
                                        UniversityId = model2.UniversityId,

                                        MajorId = model2.MajorId,
                                        Address = model2.Address,
                                        Created = DateTime.Now,
                                        Updated = DateTime.Now,
                                        CreatedBy = appUser.Id,
                                        UpdatedBy = appUser.Id,
                                        CountryId = model2.CountryId,
                                        CityId = model2.CityId,
                                        DepartmentId = model2.DepartmentID,
                                        Qualification = model2.Qualification == null ? (byte)QualificationType.Unspecified : (byte)model2.Qualification,
                                        UniversityOther = model2.UniversityOther,
                                        CollegeOther = model2.CollegeOther,
                                        DepartmentOther = model2.DepartmentOther,
                                        Specialization = model2.Specialization
                                    };

                                    db.Researchers.Add(researcher);

                                    db.SaveChanges();
                                }
                                else if (model2.Role == ApsUserRole.Referee)
                                {
                                    var _referee = db.Referees.Where(r => r.Email == model2.Email).FirstOrDefault();
                                    if (_referee != null)
                                    {
                                        _referee.UserId = appUser.Id;
                                        _referee.Email = model2.Email;
                                        _referee.MobileNo = model2.MobileNumber;
                                        _referee.FirstName = comps.FirstName;
                                        _referee.SecondName = comps.SecondName;
                                        _referee.ThirdName = comps.ThirdName;
                                        _referee.LastName = comps.LastName;
                                        _referee.UniversityId = model2.UniversityId;
                                        _referee.CollegeId = model2.CollegeId;
                                        _referee.Address = model2.Address;
                                        _referee.CityId = model2.CityId;
                                        _referee.CountryId = model2.CountryId;
                                        _referee.NationalityId = model2.NationalityId;
                                        _referee.MajorId = model2.MajorId;
                                        _referee.Qualification = model2.Qualification == null ? (byte)QualificationType.Unspecified : (byte)model2.Qualification;

                                        _referee.Specialization = model2.Specialization;
                                        _referee.IsDeleted = false;
                                        _referee.Updated = DateTime.Now;
                                        _referee.UpdatedBy = appUser.Id;
                                        _referee.DepartmentId = model2.DepartmentID;
                                        _referee.DepartmentOther = model2.DepartmentOther;
                                        db.Entry(_referee).State = System.Data.Entity.EntityState.Modified;
                                    }
                                    else
                                    {
                                        var userReferee = new Referee
                                        {
                                            UserId = appUser.Id,
                                            Email = model2.Email,
                                            MobileNo = model2.MobileNumber,
                                            FirstName = comps.FirstName,
                                            SecondName = comps.SecondName,
                                            ThirdName = comps.ThirdName,
                                            LastName = comps.LastName,
                                            UniversityId = model2.UniversityId,
                                            CollegeId = model2.CollegeId,
                                            Address = model2.Address,
                                            CityId = model2.CityId,
                                            CountryId = model2.CountryId,
                                            NationalityId = model2.NationalityId,
                                            MajorId = model2.MajorId,
                                            Qualification = model2.Qualification == null ? (byte)QualificationType.Unspecified : (byte)model2.Qualification,

                                            Specialization = model2.Specialization,
                                            IsDeleted = false,

                                            Created = DateTime.Now,
                                            Updated = DateTime.Now,
                                            CreatedBy = appUser.Id,
                                            UpdatedBy = appUser.Id,
                                            DepartmentId = model2.DepartmentID,
                                            DepartmentOther = model2.DepartmentOther,
                                        };
                                        db.Referees.Add(userReferee);

                                        db.SaveChanges();
                                    }
                                }

                                if (model2.BankIbanPhotocopyReadonly != null)
                                {
                                    var sessFile = db.UserSessionFiles.FirstOrDefault(f => f.Id.ToString() == model2.BankIbanPhotocopyReadonly.FileId);

                                    if (sessFile != null)
                                    {
                                        var file = DbContext.UserFiles.FirstOrDefault(f => f.Id == sessFile.Id);

                                        var id = file == null ? sessFile.Id : Guid.NewGuid();

                                        file = new Data.DbModels.UserFile
                                        {
                                            Id = id,
                                            Type = sessFile.Type,
                                            CreatedBy = appUser.Id,
                                            FileName = sessFile.FileName,
                                            Extension = sessFile.Extension,
                                            UserId = appUser.Id,
                                            Created = DateTime.Now,
                                            Updated = DateTime.Now,
                                            UpdatedBy = appUser.Id,
                                            FilePath = sessFile.FilePath
                                        };

                                        db.UserFiles.Add(file);
                                    }
                                }

                                if (model2.CvFileReadonly != null)
                                {
                                    var sessFile = db.UserSessionFiles.FirstOrDefault(f => f.Id.ToString() == model2.CvFileReadonly.FileId);

                                    if (sessFile != null)
                                    {
                                        var file = DbContext.UserFiles.FirstOrDefault(f => f.Id == sessFile.Id);

                                        var id = file == null ? sessFile.Id : Guid.NewGuid();

                                        file = new Data.DbModels.UserFile
                                        {
                                            Id = id,
                                            Type = sessFile.Type,
                                            CreatedBy = appUser.Id,
                                            FileName = sessFile.FileName,
                                            Extension = sessFile.Extension,
                                            UserId = appUser.Id,
                                            Created = DateTime.Now,
                                            Updated = DateTime.Now,
                                            UpdatedBy = appUser.Id,
                                            FilePath = sessFile.FilePath
                                        };

                                        db.UserFiles.Add(file);
                                    }
                                }

                                db.SaveChanges();
                                bool linkCreated = true;
                                if (model2.Role == ApsUserRole.Researcher || (model2.Role == ApsUserRole.Referee && !appUser.EmailConfirmed))
                                {
                                    linkCreated = CreateTemporaryLink(model2.Role == ApsUserRole.Referee ? TempUrlType.RefereeEmailActivation : TempUrlType.WelcomeMessageResearcher, appUser.Id, model2.Email, appUser.PhoneNumber, out err, out gKey);
                                }

                                if (!linkCreated)
                                {
                                    TempData["message"] = LangHelpers.Instance["failed_to_send_activation_email"] + " " + err;
                                    TempData["message_type"] = "danger";
                                }
                                else
                                {
                                    try
                                    {
                                        db.UserSessionDatas.Where(x => x.SessionId == UserSession.Id).Delete();
                                        db.UserSessionFiles.Where(x => x.SessionId == UserSession.Id).Delete();
                                    }
                                    catch
                                    {
                                    }

                                    trans2.Commit();

                                    if (model2.Role == ApsUserRole.Referee && !appUser.EmailConfirmed)
                                    {
                                        TempData["message"] = LangHelpers.Instance["registration_successful_pending_approval"];
                                    }
                                    else
                                    {
                                        TempData["message"] = LangHelpers.Instance["registration_successul"];
                                    }

                                    TempData["message_type"] = "success";

                                    return RedirectToAction("Login", "Account");
                                }
                            }
                        }
                        catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                        {
                            try { trans2.Rollback(); } catch { }

                            model2.SmsCode = null;

                            Logger.LogExceptionAsync(ex, model2);
                            TempData["message"] = LangHelpers.Instance["failed"] + " " + ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                            TempData["message_type"] = "danger";
                        }
                        catch (Exception ex)
                        {
                            try { trans2.Rollback(); } catch { }

                            model2.SmsCode = null;

                            Logger.LogExceptionAsync(ex, model2);

                            TempData["message"] = LangHelpers.Instance["failed"] + " " + ex.SanitizeException();
                            TempData["message_type"] = "danger";
                        }
                    }
                }
            }

            ViewBag.Countries = new SelectList(DbContext.Countries.OrderBy(c => c.NameArabic), "Id", "NameArabic");
            ViewBag.Roles = new SelectList(
                           new List<SelectListItem>
                           {
                                new SelectListItem { Selected = true, Text = LangHelpers.Instance["researcher"], Value = "1" },
                                new SelectListItem { Selected = false, Text = LangHelpers.Instance["referee"], Value = "5" }
                           }, "Value", "Text", 1);
            FillOutLookups(model);
            ViewBag.IncludeCKEditor = true;

            var role = model.Role;

            if (role == ApsUserRole.Researcher)
            {
                return View("RegisterResearcherSteps", model);
            }
            else
            {
                return View("RegisterResearcherSteps", model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResendEmailActivation(string username)
        {
            var user = (User)null;
            if (string.IsNullOrWhiteSpace(username) || (user = DbContext.Users.FirstOrDefault(u => u.Email == username || u.UserName == username)) == null)
                return RedirectToAction("Login");

            var gKey = "";
            var err = "";

            if (!CreateTemporaryLink(TempUrlType.EmailActivation, user.Id, user.Email, user.PhoneNumber, out err, out gKey))
            {
                TempData["message"] = "فشلت عملية إرسال رسالة التفعي " + err;
                TempData["message_type"] = "danger";

                return RedirectToAction("Login");
            }
            else
            {
                TempData["message"] = "تم إرسال رسالة التفعيل إلى بريدك الإلكتروني";
                TempData["message_type"] = "success";

                return RedirectToAction("Login");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult SendResetPasswordEmail()
        {
            var model = new ForgotPasswordViewModel { };
            return View(model);
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SendResetPasswordEmail(ForgotPasswordViewModel model)
        {
            var username = model == null ? "" : model.Email;
            var user = (User)null;
            if (string.IsNullOrWhiteSpace(username) || (user = DbContext.Users.FirstOrDefault(u => u.Email == username || u.UserName == username)) == null)
                return RedirectToAction("Login");

            var gKey = "";
            var err = "";

            if (!CreateTemporaryLink(TempUrlType.ResetPassword, user.Id, user.Email, user.PhoneNumber, out err, out gKey))
            {
                TempData["message"] = "فشلت عملية إرسال رسالة التفعيل " + err;
                TempData["message_type"] = "danger";

                return RedirectToAction("Login");
            }
            else
            {
                TempData["message"] = "تم إرسال رسالة التفعيل إلى بريدك الإلكتروني";
                TempData["message_type"] = "success";

                return RedirectToAction("Login");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult SendMobileActivation(string username)
        {
            var user = (User)null;
            if (string.IsNullOrWhiteSpace(username) || (user = DbContext.Users.FirstOrDefault(u => u.Email == username || u.UserName == username)) == null)
                return RedirectToAction("Login");

            var model = new ResetPasswordViewModel
            {
                Email = user.Email,
                MobileNumber = user.PhoneNumber
            };

            return View(model);
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult SendMobileActivation(ResetPasswordViewModel model)
        {
            var username = model == null ? "" : model.Email;
            var user = (User)null;
            if (string.IsNullOrWhiteSpace(username) || (user = DbContext.Users.FirstOrDefault(u => u.Email == username || u.UserName == username)) == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", ApsStrings.mobile_no_required);
            }

            var db = Utils.db;

            var key = model.SmsKey == null ? null : db.UserTemporaryUrls.FirstOrDefault(u => model.SmsKey == null ? false : u.Key == model.SmsKey);

            if (key != null)
            {
                if (model.SmsCode != key.ShortCode.ToString())
                {
                    ModelState.AddModelError("ShortCode", ApsStrings.sms_code_mismatch);
                }
                else if (ModelState.IsValid)
                {
                    UserSession.VerifiedMobile = model.MobileNumber;
                    key.IsUsed = true;
                    user.PhoneNumber = model.MobileNumber;
                    user.PhoneNumberConfirmed = true;
                    db.SaveChanges();

                    TempData["message"] = "تم تفعيل رقم جوالك بنجاح!";
                    TempData["message_type"] = "success";

                    return RedirectToAction("Login");
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var gKey = "";
                    var err = "";

                    if (!CreateTemporaryLink(TempUrlType.SmsActivation, user.Id, user.Email, model.MobileNumber, out err, out gKey))
                    {
                        TempData["message"] = "فشلت عملية إرسال رسالة التفعيل " + err;
                        TempData["message_type"] = "danger";

                        return View(model);
                    }
                    else
                    {
                        TempData["message"] = "تم إرسال رسالة التفعيل إلى جوالك";
                        TempData["message_type"] = "success";

                        model.SmsKey = gKey;

                        return View(model);
                    }
                }
            }

            model.MobileNumber = user.PhoneNumber;
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult ContactUs()
        {
            var model = new ContactUsViewModel();
            return View(model);
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ContactUs(ContactUsViewModel model)
        {
            var contactUs = new ContactedU();
            var db = Utils.db;
            try
            {
                contactUs.FullName = model.FullName;
                contactUs.EmailAddress = model.EmailAddress;
                contactUs.Subject = model.Subject;
                contactUs.Message = model.Message;
                contactUs.Created = DateTime.Now;
                db.ContactedUs.Add(contactUs);
                db.SaveChanges();

                var allSystemAdmins = db.Users.Where(u => u.Roles.Count(r => r.Name == ApsUserRole.SystemAdmin.ToString()) > 0).ToList();
                var emails = string.Join(";", allSystemAdmins.Select(x => x.Email).ToArray());
                var names = string.Join(",", allSystemAdmins.Select(x => x.FullName).ToArray());
                var isSent = SendContactUsMessage(model, emails, names);
                if (isSent)
                {
                    TempData["message"] = "تم التسجيل بنجاح";
                    TempData["message_type"] = "success";
                    model.Message = "";
                }
                else
                {
                    TempData["message"] = "لم يتم إرسال رسالتك. يرجى الاتصال بعد بعض الوقت.";
                    TempData["message_type"] = "danger";
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                Logger.LogExceptionAsync(ex, model);
                TempData["message"] = ApsStrings.failed + " " + ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;
                TempData["message_type"] = "danger";
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, model);

                var ex2 = ex;
                while (ex2.InnerException != null)
                {
                    ex2 = ex2.InnerException;
                }
                TempData["message"] = ApsStrings.failed + " " + ex2.Message;
                TempData["message_type"] = "danger";
            }
            return View(model);
        }

        [NonAction]
        private void FillOutLookups(RegistrationViewModel model)
        {
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("iban", false, null), Value = "1" });
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("swift", false, null), Value = "2" });
                //list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });
                model.BankAccountNumberTypes = list.ToArray();
            }

            {
                var list = DbContext.Departments
                            .Where(c => c.CollegeId == model.CollegeId)
                            .ToArray()
                            .Select(c => new SelectListItem
                            {
                                Text = c.LocalizedName(),
                                Value = c.Id.ToString()
                            })
                            .OrderBy(c => c.Text)
                            .ToList();
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });
                model.Departments = list.ToArray();
            }

            if (model.Role == ApsUserRole.Researcher || model.Role == ApsUserRole.Researcher)
            {
                var list = DbContext.Universities
                               //.Where(c => c.Id == 1)
                               .ToArray()
                               .OrderBy(c => c.Id == 1 ? 0 : 1)
                               .ThenBy(c => c.LocalizedName())
                               .Select(c => new SelectListItem
                               {
                                   Text = c.LocalizedName(),
                                   Value = c.Id.ToString()
                               })
                               .ToList();
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });

                model.Universities = list.ToArray();
                if (model.Step == 1)
                {
                    model.UniversityId = 1;
                }
                model.UniversityName = list.FirstOrDefault().Text;
            }
            else
            {
                var list = DbContext.Universities
                               .ToArray()
                               .OrderBy(c => c.Id == 1 ? 0 : 1)
                               .ThenBy(c => c.LocalizedName())
                               .Select(c => new SelectListItem
                               {
                                   Text = c.LocalizedName(),
                                   Value = c.Id.ToString()
                               })
                               .ToList();

                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });

                model.Universities = list.ToArray();

                model.UniversityId = 1;
                model.UniversityName = list.FirstOrDefault().Text;
            }

            {
                ViewBag.IncludeCKEditor = true;
                ViewBag.LargerWidth = true;

                var list = DbContext.Colleges
                               .Where(c => c.UniversityId == model.UniversityId)
                               .ToArray()
                               .Select(c => new SelectListItem
                               {
                                   Text = c.LocalizedName(),
                                   Value = c.Id.ToString()
                               })
                               .OrderBy(c => c.Text)
                               .ToList();

                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });

                model.Colleges = list.ToArray();
            }

            {
                var values = Utils.QualificationTypesByResearcherType();
                var list = new List<SelectListItem>();

                var i = 0;
                foreach (var v in values)
                {
                    if (v.ToString() == "Unspecified") continue;

                    list.Add(new SelectListItem
                    {
                        Value = v.ToString(),
                        Text = LangHelpers.Instance[v.ToString()]
                    });

                    i++;
                }
                model.QualificationTypes = list.ToArray();
            }

            {
                var list = DbContext.Countries
                    .ToArray()
                    .OrderBy(c => c.Id == 10 ? 0 : 1)
                    .ThenBy(c => c.LocalizedName())
                     .Select(c => new SelectListItem
                     {
                         Text = c.LocalizedName(),
                         Value = c.Id.ToString()
                     })
                    .ToList();

                model.Countries = list.ToArray();
            }

            {
                var list = DbContext.Cities

                    .ToArray()
                    .Select(c => new CityViewModel
                    {
                        Id = c.Id,
                        CountryId = c.CountryId,
                        Name = c.LocalizedName(),
                    })
                    .OrderBy(c => c.Name)
                    .ToList();

                list.Add(new CityViewModel { Name = LangHelpers.Instance.GetLangValueByString("other", false, null), Id = 0 });

                model.Cities = list;
            }

            {
                var list = DbContext.Banks
                    .ToArray()
                    .Select(c => new SelectListItem
                    {
                        Text = c.LocalizedName(),
                        Value = c.Id.ToString()
                    })
                    .OrderBy(c => c.Text)
                    .ToList();

                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });

                model.Banks = list.ToArray();
            }

            {
                var list = DbContext.Majors
                    .ToArray()
                    .Select(c => new SelectListItem
                    {
                        Text = c.LocalizedName(),
                        Value = c.Id.ToString()
                    })
                    .OrderBy(c => c.Text)
                    .ToList();

                model.Majors = list.ToArray();
            }

            Utils.SwitchCulture(null);
        }

        [NonAction]
        private void FillOutLookups(EditUserViewModel model)
        {
            var isResearcher = Utils.HasRolesAny(ApsUserRole.Researcher);

            {
                var list = DbContext.Departments
                            .Where(c => c.CollegeId == model.CollegeId)
                            .ToArray()
                            .Select(c => new SelectListItem
                            {
                                Text = c.LocalizedName(),
                                Value = c.Id.ToString()
                            })
                            .OrderBy(c => c.Text)
                            .ToList();

                //if (!isResearcher)
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });
                model.Departments = list.ToArray();
            }

            if (isResearcher)
            {
                var list = DbContext.Universities
                               //.Where(c => c.Id == 1)
                               .ToArray()
                               .OrderBy(c => c.Id == 1 ? 0 : 1)
                               .ThenBy(c => c.LocalizedName())
                               .Select(c => new SelectListItem
                               {
                                   Text = c.LocalizedName(),
                                   Value = c.Id.ToString()
                               })
                               .ToList();
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });
                model.Universities = list.ToArray();

                //model.UniversityId = 1;
                model.UniversityName = list.FirstOrDefault().Text;
            }
            else
            {
                var list = DbContext.Universities
                               .ToArray()
                               .OrderBy(c => c.Id == 1 ? 0 : 1)
                               .ThenBy(c => c.LocalizedName())
                               .Select(c => new SelectListItem
                               {
                                   Text = c.LocalizedName(),
                                   Value = c.Id.ToString()
                               })
                               .ToList();

                //if (!isResearcher)
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });

                model.Universities = list.ToArray();
            }

            {
                ViewBag.IncludeCKEditor = true;
                ViewBag.LargerWidth = true;

                var list = DbContext.Colleges
                               .Where(c => c.UniversityId == model.UniversityId)
                               .ToArray()
                               .Select(c => new CollegeViewModel
                               {
                                   Name = c.LocalizedName(),
                                   Id = c.Id,
                               })
                               .OrderBy(c => c.Name)
                               .ToList();

                //if (!isResearcher)
                list.Add(new CollegeViewModel { Name = LangHelpers.Instance.GetLangValueByString("other", false, null), Id = 0 });

                model.Colleges = list;
            }

            {
                var values = Utils.QualificationTypesByResearcherType();
                var list = new List<SelectListItem>();

                var i = 0;
                foreach (var v in values)
                {
                    if (v.ToString() == "Unspecified") continue;

                    list.Add(new SelectListItem
                    {
                        Value = v.ToString(),
                        Text = LangHelpers.Instance[v.ToString()]
                    });

                    i++;
                }
                model.QualificationTypes = list.ToArray();
            }

            {
                var list = DbContext.Countries
                    .ToArray()
                    .OrderBy(c => c.Id == 1 ? 0 : 1)
                    .ThenBy(c => c.LocalizedName())
                     .Select(c => new SelectListItem
                     {
                         Text = c.LocalizedName(),
                         Value = c.Id.ToString()
                     })
                    .ToList();

                model.Countries = list.ToArray();
            }

            {
                var list = DbContext.Cities

                    .ToArray()
                    .Select(c => new CityViewModel
                    {
                        Id = c.Id,
                        CountryId = c.CountryId,
                        Name = c.LocalizedName(),
                    })
                    .OrderBy(c => c.Name)
                    .ToList();

                list.Add(new CityViewModel { Name = LangHelpers.Instance.GetLangValueByString("other", false, null), Id = 0 });

                model.Cities = list;
            }

            {
                var list = DbContext.Banks
                    .ToArray()
                    .Select(c => new SelectListItem
                    {
                        Text = c.LocalizedName(),
                        Value = c.Id.ToString()
                    })
                    .OrderBy(c => c.Text)
                    .ToList();

                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });

                model.Banks = list.ToArray();
            }

            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("iban", false, null), Value = "1" });
                list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("swift", false, null), Value = "2" });
                //list.Add(new SelectListItem { Text = LangHelpers.Instance.GetLangValueByString("other", false, null), Value = "0" });
                model.BankAccountNumberTypes = list.ToArray();
            }

            {
                var list = DbContext.Majors
                    .ToArray()
                    .Select(c => new MajorViewModel
                    {
                        Name = c.LocalizedName(),
                        Id = c.Id
                    })
                    .OrderBy(c => c.Name)
                    .ToList();

                list.Add(new MajorViewModel { Name = LangHelpers.Instance.GetLangValueByString("other", false, null), Id = 0 });

                model.Majors = list;
            }
            Utils.SwitchCulture(null);
        }
    }
}