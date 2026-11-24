using APS.Auth;
using APS.Core;
using APS.Core.Managers;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PhoneNumbers;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(ApsUserRole.SystemAdmin, ApsUserRole.CommitteForPAEHead, ApsUserRole.DeanOfAcademicResearch, ApsUserRole.AcademicCouncilHead, ApsUserRole.ResearchUnitHead)]
    public class MembersController : BaseController
    {
        public ActionResult Index()
        {
            if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
            {
                ViewBag.RoleKeyName = ApsUserRole.DeanshipOfAcademicResearchCouncilMember.ToString();
            }
            else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
            {
                ViewBag.RoleKeyName = ApsUserRole.ResearchUnitCouncilMember.ToString();
            }
            else if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
            {
                ViewBag.RoleKeyName = ApsUserRole.CommitteeforPAEMember.ToString();
            }
            else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
            {
                ViewBag.RoleKeyName = ApsUserRole.AcademicCouncilMember.ToString();
            }

            return View();
        }

        public ActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = GetInstance<UsersManager>()
                .GetUsers()
                .Include("Roles")
                .Include("UserProfile")
                .Include("College")
                .Include("Major")
                .SingleOrDefault(u => u.Id == id);

            if (user == null)
            {
                TempData["message"] = "لم يتم العثور على المستخدم " + id;
                TempData["message_type"] = "warning";

                return RedirectToAction("Index");
            }

            var model = new EditUserViewModel
            {
                FullName = user.UserProfile.GetFullName(),
                UserId = user.Id,
                Password = "**********",
                ConfirmPassword = "**********",
                Email = user.Email,
                MobileNo = user.UserProfile.MobileNo,
                CollegeId = user.CollegeId,
                CollegeName = user.CollegeId == null ? null : (IsArabic ? user.College.NameArabic : user.College.NameEnglish),
                RoleId = user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Id,
                RoleName = LangHelpers.Instance[user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Name],
                IsActivated = user.IsActivated,
                ForcePasswordChange = user.ForcePasswordChange
            };
            return View(model);
        }

        // GET: Admin/Users/Create

        public ActionResult Create()
        {
            var model = new MemberCreateViewModel();
            var userid = User.Identity.EnhancedGetUserId();
            var user = AuthDbContext.Users.Find(userid);

            model.Colleges = GetColleges();
            model.Majors = GetMajors();
            model.Roles = GetRoles();

            if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.CommitteeforPAEMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.DeanshipOfAcademicResearchCouncilMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.AcademicCouncilMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.ResearchUnitCouncilMember.ToString()]).RoleId;
            }

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(MemberCreateViewModel model)
        {
            var roleId = Guid.Empty;
            var newUserRoleName = (IdentityRole)null;
            var newUserRole = ApsUserRole.None;

            model.Roles = GetRoles();

            if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.CommitteeforPAEMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.DeanshipOfAcademicResearchCouncilMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.AcademicCouncilMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.ResearchUnitCouncilMember.ToString()]).RoleId;
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

            if ((!model.Email.Trim().ToLower().EndsWith("@imamu.edu.sa") && !model.Email.Trim().ToLower().EndsWith("@kay-ict.com")))
            {
                ModelState.AddModelError("Email", LangHelpers.Instance.GetLangValueByString("email_must_be_imam_edu"));
            }

            model.UniversityId = CurrentUser.UniversityId;
            if (!string.IsNullOrWhiteSpace(model.RoleId) && Guid.TryParse(model.RoleId, out roleId))
            {
                newUserRoleName = AuthDbContext.Roles.SingleOrDefault(r => r.Id == model.RoleId);
                if (newUserRoleName == null || !Enum.TryParse(newUserRoleName.Name, out newUserRole) || newUserRole == ApsUserRole.None)
                {
                    ModelState.AddModelError("RoleID", "دور المستخدم مطلوب");
                }
                else
                {
                    if (IsCollegeUser)
                    {
                        model.CollegeId = CurrentUser.CollegeId;
                        if (model.CollegeId != null)
                        {
                            var college = Utils.GetCollegeDetails(model.CollegeId.Value);
                            model.UniversityId = college.UniversityId;
                        }
                    }
                    else
                    {
                        var needsCollege = Utils.IsCollegeRole(newUserRole);
                        var needsMajor = Utils.IsMajorRole(newUserRole);

                        if (needsCollege)
                        {
                            if (model.CollegeId == null)
                            {
                                ModelState.AddModelError("CollegeId", "حقل الكلية مطلوب");
                            }
                        }
                        else
                        {
                            model.CollegeId = null;
                        }

                        if (needsMajor)
                        {
                            if (model.MajorId == null)
                            {
                                ModelState.AddModelError("MajorId", "حقل التخصص مطلوب");
                            }
                        }
                        else
                        {
                            model.MajorId = null;
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(model.Password))
            {
                model.Password = System.Web.Security.Membership.GeneratePassword(8, 2);
            }

            if (String.IsNullOrEmpty(model.UserName))
            {
                model.UserName = model.Email;
            }

            if (!string.IsNullOrWhiteSpace(model.MobileNo))
            {
                string mob = "";
                if (!Utils.ValidateMobile(model.MobileNo, out mob))
                {
                    ModelState.AddModelError("MobileNo", ApsStrings.invalid_mobile_number);
                }
                else
                {
                    model.MobileNo = mob;
                }
            }
            else
            {
                model.MobileNo = null;
            }

            var appuser = new ApplicationUser();
            appuser = UserManager.FindByEmail(model.Email);
            if (appuser != null)
            {
                if (appuser.Id == CurrentUser.Id)
                {
                    ModelState.AddModelError("Email", LangHelpers.Instance["can_not_add_self_email"]);
                }
                else if (model.CollegeId != null && appuser.CollegeId != model.CollegeId)
                {
                    ModelState.AddModelError("Email", LangHelpers.Instance["user_belong_to_other_college"]);
                }
            }

            if (ModelState.IsValid)
            {
                bool userCreated = true;
                bool newUser = false;
                var result = new IdentityResult();
                if (appuser == null)
                {
                    appuser = new ApplicationUser
                    {
                        CollegeId = model.CollegeId,
                        UniversityId = model.UniversityId,
                        MajorId = model.MajorId,
                        FullName = model.FullName,
                        UserName = model.UserName,
                        Email = model.Email,
                        IsActivated = model.IsActivated,
                        PhoneNumber = model.MobileNo,
                        EmailConfirmed = model.IsActivated,
                        PhoneNumberConfirmed = model.IsActivated && model.MobileNo != null,
                        ForcePasswordChange = model.ForcePasswordChange,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        CreatedBy = CurrentUser.Id,
                        UpdatedBy = CurrentUser.Id,
                    };

                    result = UserManager.Create(appuser, model.Password);
                    userCreated = result.Succeeded;
                    newUser = true;
                }

                if ((!newUser || result.Succeeded) && newUserRoleName != null)
                {
                    var cmps = Utils.GetNameComponents(model.FullName);

                    if (newUser)
                    {
                        Template emailMessage = null;
                        Template smsText = null;
                        var link = Utils.GetApplicationUrl();

                        if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
                        {
                            emailMessage = TemplateUtils[TemplateKey.NewCommitteForPAEMemberWelcomeEmail];
                            smsText = TemplateUtils[TemplateKey.NewCommitteForPAEMemberWelcomeSMS];
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
                        {
                            emailMessage = TemplateUtils[TemplateKey.DeanOfAcademicResearchMemberWelcomeEmail];
                            smsText = TemplateUtils[TemplateKey.DeanOfAcademicResearchMemberWelcomeSMS];
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
                        {
                            emailMessage = TemplateUtils[TemplateKey.NewAcademicCouncilMemberWelcomeEmail];
                            smsText = TemplateUtils[TemplateKey.NewAcademicCouncilMemberWelcomeSMS];
                        }
                        else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
                        {
                            emailMessage = TemplateUtils[TemplateKey.ResearchUnitMemberWelcomeEmail];
                            smsText = TemplateUtils[TemplateKey.ResearchUnitMemberWelcomeSMS];
                        }

                        if (emailMessage != null)
                        {
                            var subject = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(emailMessage.SubjectEnglish) ? emailMessage.SubjectArabic : emailMessage.SubjectEnglish;
                            var body = !Utils.IsUserLanguageArabic || string.IsNullOrWhiteSpace(emailMessage.SubjectEnglish) ? emailMessage.BodyArabic : emailMessage.BodyEnglish;

                            body = body.Replace("{FullName}", model.FullName).Replace("{Email}", model.Email).Replace("{Password}", model.Password).Replace("{Link}", link);
                            EmailService.SendMessage(SmtpSettings, subject, body, new MailAddress(model.Email, model.FullName), null, null);
                        }

                        if (smsText != null)
                        {
                            var smsBody = smsText.BodyArabic.Replace("{FullName}", model.FullName);
                            SmsService.SendMessage(smsBody, model.MobileNo);
                        }

                        UserManager.AddToRole(appuser.Id, newUserRoleName.Name);

                        var userProfile = new UserProfile
                        {
                            UserId = appuser.Id,
                            Created = DateTime.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        if (number != null)
                        {
                            userProfile.MobileNo = "0" + number.NationalNumber;
                        }

                        userProfile.NationalId = model.NationalId;

                        userProfile.FirstName = cmps.FirstName;
                        userProfile.MiddleName = cmps.SecondName;
                        userProfile.ThirdName = cmps.ThirdName;
                        userProfile.LastName = cmps.LastName;

                        DbContext.UserProfiles.Add(userProfile);
                        DbContext.SaveChanges();
                    }
                    else
                    {
                        if (!Utils.HasRolesAny(appuser.Id, (ApsUserRole)Enum.Parse(typeof(ApsUserRole), newUserRoleName.Name)))
                        {
                            var userExtrsRole = DbContext.UserExtraRoles.Where(x => x.UserId == appuser.Id && x.RoleId == newUserRoleName.Id && x.IsActive == true).FirstOrDefault();
                            if (userExtrsRole == null)
                            {
                                var extraRole = new UserExtraRole
                                {
                                    UserId = appuser.Id,
                                    RoleId = newUserRoleName.Id,
                                    IsActive = true,
                                    CollegeId = model.CollegeId,
                                    Created = DateTime.Now,
                                    Updated = DateTime.Now,
                                    CreatedBy = Utils.SystemAdminUserId,
                                    UpdatedBy = Utils.SystemAdminUserId,
                                };

                                DbContext.UserExtraRoles.Add(extraRole);
                                DbContext.SaveChanges();
                            }
                        }
                    }

                    TempData["message"] = "تم إنشاء المستخدم بنجاح";
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index", "Members");
                }
                else
                {
                    var i = 1;
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError("CREATE_USER_ERR_" + i, err);
                        i++;
                    }
                }
            }

            TempData["message"] = "فشلت عملية إنشاء المستخدم";
            TempData["message_type"] = "danger";

            model.Colleges = GetColleges();
            model.Majors = GetMajors();

            return View(model);
        }

        public ActionResult Edit(string id)
        {
            var userId = Guid.Empty;
            if (id == null || !Guid.TryParse(id, out userId))
            {
                return RedirectToAction("Index");
            }

            if (id == CurrentUser.Id)
            {
                return RedirectToAction("Profile", "Account");
            }

            var user = UserUtils.GetUsers()
                .Include("Roles")
                .Include("UserProfile")
                .Include("College")
                .Include("Major")
                .SingleOrDefault(u => u.Id == id);

            if (user == null)
            {
                TempData["message"] = "لم يتم العثور على المستخدم " + id;
                TempData["message_type"] = "warning";

                return RedirectToAction("Index");
            }

            var model = new EditUserViewModel
            {
                UserId = user.Id,
                NationalId = user.UserProfile != null ? user.UserProfile.NationalId : "",
                UserName = user.UserName,
                FullName = user.UserProfile == null ? user.UserName : user.UserProfile.GetFullName(),
                Password = "**********",
                ConfirmPassword = "**********",
                Email = user.Email,
                MobileNo = user.UserProfile == null ? "" : user.UserProfile.MobileNo,
                CollegeId = user.CollegeId,
                CollegeName = user.CollegeId == null ? null : user.College.NameEnglish,
                MajorId = user.MajorId,
                MajorName = user.MajorId == null ? null : user.Major.NameEnglish,
                RoleId = user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Id,
                RoleName = LangHelpers.Instance[user.Roles.Count() == 0 ? null : user.Roles.FirstOrDefault().Name],
                IsActivated = user.IsActivated,
                ForcePasswordChange = user.ForcePasswordChange
            };

            model.Colleges = GetColleges();
            model.Majors = GetMajors();
            model.Roles = GetRoles();

            return View(model);
        }

        // POST: Admin/Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Edit(EditUserViewModel model)
        {
            var userId = null as string;
            var user = (User)null;

            if (!string.IsNullOrWhiteSpace(model.UserId) && model.UserId == CurrentUser.Id)
            {
                return RedirectToAction("Profile", "Account");
            }

            model.Roles = GetRoles();

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

            if (!string.IsNullOrWhiteSpace(model.UserId))
            {
                user = UserUtils.GetUsers()
                    .SingleOrDefault(u => u.Id == model.UserId);

                if (user == null)
                {
                    TempData["message"] = "لم يتم العثور على المستخدم " + model.UserId;
                    TempData["message_type"] = "warning";

                    return RedirectToAction("Index");
                }
            }

            if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.CommitteeforPAEMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.DeanshipOfAcademicResearchCouncilMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.AcademicCouncilMember.ToString()]).RoleId;
            }
            else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
            {
                model.RoleId = model.Roles.FirstOrDefault(r => r.RoleName == LangHelpers.Instance[ApsUserRole.ResearchUnitCouncilMember.ToString()]).RoleId;
            }
            var roleId = Guid.Empty;
            var userRoleName = (Role)null;
            var userRole = ApsUserRole.None;

            if (!string.IsNullOrWhiteSpace(model.RoleId) && Guid.TryParse(model.RoleId, out roleId))
            {
                userRoleName = DbContext.Roles.SingleOrDefault(r => r.Id == model.RoleId);
                if (userRoleName == null || !Enum.TryParse(userRoleName.Name, out userRole) || userRole == ApsUserRole.None)
                {
                    ModelState.AddModelError("RoleID", "دور المستخدم مطلوب");
                }
                else

                {
                    if (IsCollegeUser)
                    {
                        model.CollegeId = CurrentUser.CollegeId;
                    }
                    else
                    {
                        var needsCollege = Utils.IsCollegeRole(userRole);
                        var needsMajor = Utils.IsMajorRole(userRole);

                        if (needsCollege)
                        {
                            if (model.CollegeId == null)
                            {
                                ModelState.AddModelError("CollegeId", "حقل الكلية مطلوب");
                            }
                        }
                        else
                        {
                            model.CollegeId = null;
                        }

                        if (needsMajor)
                        {
                            if (model.MajorId == null)
                            {
                                ModelState.AddModelError("MajorId", "حقل التخصص مطلوب");
                            }
                        }
                        else
                        {
                            model.MajorId = null;
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "**********" && model.Password == model.ConfirmPassword)
            {
                var errorMsg = (string)null;
                if (!Utils.ValidatePassword(model.Password, Utils.IsAdmin(userRole), out errorMsg))
                {
                    ModelState.AddModelError("Password", errorMsg);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.NationalId))
            {
                if (!IsValidNationalId(model.NationalId))
                {
                    ModelState.AddModelError("NationalId", ApsStrings.invalid_national_id);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.MobileNo))
            {
                string mob = "";
                if (!Utils.ValidateMobile(model.MobileNo, out mob))
                {
                    ModelState.AddModelError("MobileNo", ApsStrings.invalid_mobile_number);
                }
                else
                {
                    model.MobileNo = mob;
                }
            }
            else
            {
                model.MobileNo = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.Roles.Clear();
                    user.Roles.Add(userRoleName);

                    if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "**********" && model.Password == model.ConfirmPassword)
                    {
                        user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    }

                    user.PhoneNumber = model.MobileNo;
                    user.Email = model.Email;
                    user.UserName = model.UserName;
                    user.IsActivated = model.IsActivated;

                    if (model.IsActivated == true)
                    {
                        user.EmailConfirmed = true;

                        if (user.PhoneNumber != null)
                            user.PhoneNumberConfirmed = true;

                        user.AccessFailedCount = 0;
                        user.LockoutEnabled = false;
                        user.LockoutEndDateUtc = null;

                        foreach (var u in user.UserSessions.Where(s => s.Locked != true))
                        {
                            u.Locked = true;
                            u.Lockdate = DateTime.Now;
                        }
                    }

                    user.ForcePasswordChange = model.ForcePasswordChange;
                    user.CollegeId = model.CollegeId;
                    user.MajorId = model.MajorId;
                    user.Updated = DateTime.Now;

                    if (user.UserProfile == null)
                    {
                        user.UserProfile = new UserProfile
                        {
                            UserId = user.Id,
                            Created = DateTime.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        DbContext.SaveChanges();
                    }

                    var cmps = Utils.GetNameComponents(model.FullName);

                    if (number != null)
                    {
                        user.UserProfile.MobileNo = "0" + number.NationalNumber;
                    }

                    user.UserProfile.NationalId = model.NationalId;

                    user.UserProfile.FirstName = cmps.FirstName;
                    user.UserProfile.MiddleName = cmps.SecondName;
                    user.UserProfile.ThirdName = cmps.ThirdName;
                    user.UserProfile.LastName = cmps.LastName;

                    DbContext.Entry(user).State = EntityState.Modified;
                    DbContext.Entry(user.UserProfile).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    TempData["message"] = "تم تحديث المستخدم بنجاح";
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Logger.LogExceptionAsync(ex, model);

                    ModelState.AddModelError("EXCEPTION", ex.SanitizeException());
                }
            }

            model.Colleges = GetColleges();
            model.Majors = GetMajors();
            model.Roles = GetRoles();

            return View(model);
        }

        // GET: Admin/Users/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (id == CurrentUser.Id)
            {
                Response.Write("You can't delete yourself.");
                Response.End();
                return null;
            }

            User aspNetUser = DbContext.Users.Find(id);
            if (aspNetUser == null)
            {
                return HttpNotFound();
            }
            return View(aspNetUser);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            if (id == CurrentUser.Id)
            {
                Response.Write("لا تستطيع حذف نفسك");
                Response.End();
                return RedirectToAction("Index");
            }
            var username = (string)null;
            try
            {
                User aspNetUser = DbContext.Users.Find(id);
                username = aspNetUser.UserName;

                DbContext.Users.Remove(aspNetUser);
                DbContext.SaveChanges();
                TempData["message"] = "تم حذف المستخدم بنجاح";
                TempData["message_type"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, id);
                TempData["message"] = "فشل حذف المستخدم" + " " + (string.IsNullOrWhiteSpace(username) ? id : username) + " (" + ex.SanitizeException() + ")";
                TempData["message_type"] = "danger";
            }
            return RedirectToAction("Index");
        }

        //
        // POST: /Manage/ChangePassword
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangePassword(ChangeUserPasswordViewModel model)
        {
            var userId = Guid.Empty;

            if (string.IsNullOrWhiteSpace(model.UserId) || !Guid.TryParse(model.UserId, out userId))
            {
                return RedirectToAction("Index", "Users");
            }

            var user = GetInstance<UsersManager>().GetUsers().SingleOrDefault(u => u.Id == userId.ToString());

            if (user == null)
            {
                TempData["message"] = "لم يتم العثور على المستخدم";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Members");
            }

            var roles = user.Roles.Select(r => r.Name).ToList();
            var userRole = ApsUserRole.None;

            foreach (var r in roles)
            {
                var role = ApsUserRole.None;

                if (Enum.TryParse<ApsUserRole>(r, out role) && role > userRole)
                {
                    userRole = role;
                }
            }

            var errorMsg = (string)null;
            if (!Utils.ValidatePassword(model.Password, Utils.IsAdmin(userRole), out errorMsg))
            {
                ModelState.AddModelError("Password", errorMsg);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    user.LockoutEnabled = false;
                    user.LockoutEndDateUtc = null;
                    user.AccessFailedCount = 0;

                    foreach (var u in user.UserSessions.Where(s => s.Locked != true))
                    {
                        u.Locked = true;
                        u.Lockdate = DateTime.Now;
                    }

                    DbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogExceptionAsync(ex, model);
                }

                return RedirectToAction("Edit", "Members", new { Id = model.UserId });
            }

            var model2 = (EditUserViewModel)((ViewResult)Edit(model.UserId)).Model;

            return View("Edit", model2);
        }

        [AjaxOnly]
        public dynamic AjaxDeleteUser(string id, string roleNameKey)
        {
            var ids = Guid.Empty;

            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out ids))
                return new { success = false, msg = "INVALID_APPLICAITON_ID" };

            if (id == CurrentUser.Id)
            {
                return new { success = false, msg = LangHelpers.Instance["you_can_delete_your_self"] };
            }
            try
            {
                var user = UserUtils.GetUsers(null, false, null, true).SingleOrDefault(u => u.Id == id);
                if (user == null)
                {
                    TempData["message"] = LangHelpers.Instance["could_not_find_any_user"] + " " + id;
                    TempData["message_type"] = "warning";

                    return RedirectToAction("Index");
                }
                var userRole = Utils.GetActualUserRole(null, id);
                if (userRole.ToString() == roleNameKey)
                {
                    user.IsDeleted = true;
                    DbContext.Entry(user).State = EntityState.Modified;
                    DbContext.SaveChanges();
                }
                else
                {
                    var role = DbContext.Roles.Where(r => r.Name == roleNameKey).FirstOrDefault();
                    if (role != null)
                    {
                        var userExtrsRole = DbContext.UserExtraRoles.Where(x => x.UserId == id && x.RoleId == role.Id && x.IsActive == true).FirstOrDefault();
                        if (userExtrsRole != null)
                        {
                            userExtrsRole.IsActive = false;
                            DbContext.Entry(userExtrsRole).State = EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                    }
                }
                return new { success = true, msg = LangHelpers.Instance["user_deleted_successfully"] };
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, id);
                return new { success = false, msg = ex.SanitizeException() };
            }
        }
    }
}