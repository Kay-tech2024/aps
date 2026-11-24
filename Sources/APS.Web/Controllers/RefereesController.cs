using APS.Auth;
using APS.Core;
using APS.Core.Managers;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using Microsoft.AspNet.Identity;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(ApsUserRole.SystemAdmin, ApsUserRole.CommitteForPAEHead)]
    public class RefereesController : BaseController
    {
        private string _roleId;

        public ActionResult Index()
        {
            return View();
        }

        private string CurrentRoleId
        {
            get
            {
                if (_roleId != null) return _roleId;

                _roleId = DbContext.Roles.FirstOrDefault(r => r.Name == ApsUserRole.Referee.ToString()).Id;

                return _roleId;
            }
        }

        private string CurrentRoleName
        {
            get
            {
                return LangHelpers.Instance[ApsUserRole.Referee.ToString()];
            }
        }

        public ActionResult Details(long? id)
        {
            if (!(id > 0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var referee = GetInstance<UsersManager>()
                .GetReferees()
                .SingleOrDefault(u => u.Id == id);

            if (referee == null)
            {
                TempData["message"] = "لم يتم العثور على المحكم " + id;
                TempData["message_type"] = "warning";

                return RedirectToAction("Index");
            }

            var model = new EditUserViewModel
            {
                FullName = referee.GetFullName(),
                Id = referee.Id.ToString(),
                UserId = referee.UserId,
                Password = "**********",
                ConfirmPassword = "**********",
                Email = referee.Email,
                MobileNo = referee.MobileNo,
                CollegeId = referee.CollegeId,
                CollegeName = referee.CollegeId == null ? null : (IsArabic ? referee.College.NameArabic : referee.College.NameEnglish),
                RoleId = CurrentRoleId,
                RoleName = CurrentRoleName,
                IsActivated = referee.User != null && referee.User.IsActivated,
                ForcePasswordChange = referee.User != null && referee.User.ForcePasswordChange
            };
            return View(model);
        }

        // GET: Admin/Users/Create

        public ActionResult Create()
        {
            var model = new RefereeRegisterViewModel();
            var userid = User.Identity.EnhancedGetUserId();
            var user = AuthDbContext.Users.Find(userid);

            ViewBag.Countries = new SelectList(DbContext.Countries
               .OrderBy(c => c.NameArabic == "المملكة العربية السعودية" ? 1 : 2)
               .ThenBy(c => c.NameArabic), "Id", "NameArabic");

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(RefereeRegisterViewModel model)
        {
            var db = Utils.db;
            /***** New Code ****/
            if (string.IsNullOrWhiteSpace(model.MobileNumber))
            {
                ModelState.AddModelError("MobileNumber", ApsStrings.mobile_no_required);
            }
            else
            {
                var validNumber = (string)null;
                if (!Utils.ValidateMobile(model.MobileNumber, out validNumber))
                {
                    ModelState.AddModelError("MobileNumber", ApsStrings.invalid_mobile_number);
                }
                else
                {
                    model.MobileNumber = validNumber;
                }
            }
            var birthDate = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(model.BirthDate) || !DateTime.TryParseExact(model.BirthDate.Trim(), "yyyy-MM-dd", new CultureInfo("en-US"), DateTimeStyles.None, out birthDate))
            {
                ModelState.AddModelError("BirthDate", ApsStrings.birthdate_required);
            }
            else
            {
                var age = Utils.CalculateAge(birthDate);
                if (age < 17)
                {
                    ModelState.AddModelError("BirthDate", ApsStrings.age_is_too_young);
                }
            }

            if (string.IsNullOrWhiteSpace(model.NationalId))
            {
                ModelState.AddModelError("NationalId", ApsStrings.national_id_required);
            }
            else if (!IsValidNationalId(model.NationalId))
            {
                ModelState.AddModelError("NationalId", ApsStrings.invalid_national_id);
            }
            model.Email = model.Email.Trim().ToLower();
            model.EmailConfirmation = model.EmailConfirmation.Trim().ToLower();

            if (!Utils.ValidateEmail(model.Email))
            {
                ModelState.AddModelError("Email", ApsStrings.invalid_email);
            }
            else if (model.EmailConfirmation != model.Email)
            {
                ModelState.AddModelError("EmailConfirmation", ApsStrings.email_confirmation_mismatch);
            }
            else if (db.Users.FirstOrDefault(u => u.Email == model.Email) != null)
            {
                ModelState.AddModelError("Email", ApsStrings.email_exists);
            }
            if (!string.IsNullOrEmpty(model.BankAccountIBAN) && !Utils.ValidateIBAN(model.BankAccountIBAN))
            {
                ModelState.AddModelError("BankAccountIBAN", ApsStrings.invalid_iban);
            }
            if (ModelState.IsValid)
            {
                var appuser = new ApplicationUser
                {
                    MajorId = model.MajorId,
                    IsActivated = model.IsActivated,
                    EmailConfirmed = model.IsActivated,
                    PhoneNumberConfirmed = false && model.MobileNumber != null,
                    ForcePasswordChange = model.ForcePasswordChange,
                    FullName = model.FullName,
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.MobileNumber,
                    CountryId = model.CountryId,
                    CollegeId = model.CollegeId,
                    CollegeOther = model.CollegeOther,
                    UniversityOther = model.UniversityOther,
                    MajorOther = model.MajorOther,
                    Address = model.Address,
                    Created = DateTime.Now,
                    Updated = DateTime.Now,
                    ForceToUpdateProfile = true
                };

                var result = UserManager.Create(appuser, model.Password);

                if (result.Succeeded)
                {
                    UserManager.AddToRole(appuser.Id, ApsUserRole.Referee.ToString());

                    var userProfile = new UserProfile
                    {
                        UserId = appuser.Id,
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser.Id,
                        MobileNo = model.MobileNumber
                    };

                    userProfile.NationalId = model.NationalId;
                    var cmps = Utils.GetNameComponents(model.FullName);

                    userProfile.FirstName = cmps.FirstName;
                    userProfile.MiddleName = cmps.SecondName;
                    userProfile.ThirdName = cmps.ThirdName;
                    userProfile.LastName = cmps.LastName;

                    DbContext.UserProfiles.Add(userProfile);

                    var referee = new Referee
                    {
                        FirstName = cmps.FirstName,
                        SecondName = cmps.SecondName,
                        ThirdName = cmps.ThirdName,
                        LastName = cmps.LastName,
                        Email = model.Email,

                        UserId = appuser.Id,
                        Created = DateTime.Now,
                        CreatedBy = CurrentUser.Id,
                        Updated = DateTime.Now,
                        UpdatedBy = CurrentUser.Id,
                        MobileNo = model.MobileNumber,
                        ForceToUpdateProfile = true
                    };

                    DbContext.Referees.Add(referee);

                    DbContext.SaveChanges();

                    TempData["message"] = "تم إنشاء المحكم بنجاح";
                    TempData["message_type"] = "success";

                    return RedirectToAction("Index", "Referees");
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

            TempData["message"] = "فشلت عملية إنشاء المحكم";
            TempData["message_type"] = "danger";

            ViewBag.Countries = new SelectList(DbContext.Countries.OrderBy(c => c.NameArabic), "Id", "NameArabic");
            return View(model);
        }

        public ActionResult Edit(long id)
        {
            var userId = Guid.Empty;
            if (!(id > 0))
            {
                return RedirectToAction("Index");
            }

            var referee = UserUtils.GetReferees()
                .SingleOrDefault(u => u.Id == id);

            if (referee == null)
            {
                TempData["message"] = "لم يتم العثور على المحكم " + id;
                TempData["message_type"] = "warning";

                return RedirectToAction("Index");
            }

            var model = new EditUserViewModel
            {
                Id = referee.Id.ToString(),
                UserId = referee.UserId,
                NationalId = referee.User != null && referee.User.UserProfile != null ? referee.User.UserProfile.NationalId : "",
                UserName = referee.User != null ? referee.User.UserName : "",
                FullName = referee.GetFullName(),
                Password = "**********",
                ConfirmPassword = "**********",
                Email = referee.Email,
                MobileNo = referee.MobileNo,
                CollegeId = referee.CollegeId,
                CollegeName = referee.CollegeId == null ? null : referee.College.NameEnglish,
                MajorId = referee.MajorId,
                MajorName = referee.MajorId == null ? null : referee.Major.NameEnglish,
                RoleId = CurrentRoleId,
                RoleName = CurrentRoleName,
                IsActivated = referee.User != null && referee.User.IsActivated,
                ForcePasswordChange = referee.User != null && referee.User.ForcePasswordChange
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
            var referee = (Referee)null;

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

            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                model.UserId = null;
            }

            if (string.IsNullOrWhiteSpace(model.Id))
                model.Id = null;

            referee = UserUtils.GetReferees()
                    .SingleOrDefault(u => (model.UserId != null && u.UserId == model.UserId) || (model.Id != null && u.Id.ToString() == model.Id));

            if (referee == null)
            {
                TempData["message"] = "لم يتم العثور على المحكم " + model.Id;
                TempData["message_type"] = "warning";

                return RedirectToAction("Index");
            }

            model.RoleId = CurrentRoleId;

            var roleId = Guid.Empty;
            var userRoleName = (Role)null;
            var userRole = ApsUserRole.None;

            if (!string.IsNullOrWhiteSpace(model.RoleId) && Guid.TryParse(model.RoleId, out roleId))
            {
                userRoleName = DbContext.Roles.SingleOrDefault(r => r.Id == model.RoleId);
                if (userRoleName == null || !Enum.TryParse(userRoleName.Name, out userRole) || userRole == ApsUserRole.None)
                {
                    ModelState.AddModelError("RoleID", "دور المحكم مطلوب");
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
                    var cmps = Utils.GetNameComponents(model.FullName);

                    var user = referee.User;

                    if (user == null)
                    {
                        if (model.UserName == null)
                        {
                            model.UserName = model.Email;
                        }

                        var appuser = new ApplicationUser
                        {
                            FullName = model.FullName,
                            CollegeId = model.CollegeId,
                            MajorId = model.MajorId,
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
                            ForceToUpdateProfile = true
                        };

                        if (string.IsNullOrWhiteSpace(model.Password))
                        {
                            model.Password = "__SET_ME__" + Utils.GenerateShortCode() + "-" + Utils.GenerateShortCode();
                        }

                        var result = UserManager.Create(appuser, model.Password);

                        if (result.Succeeded && userRoleName != null)
                        {
                            UserManager.AddToRole(appuser.Id, userRoleName.Name);

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

                            referee.UserId = appuser.Id;
                            DbContext.SaveChanges();

                            user = DbContext.Users.FirstOrDefault(u => u.Id == appuser.Id);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "**********" && model.Password == model.ConfirmPassword)
                    {
                        user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    }

                    user.PhoneNumber = model.MobileNo;
                    referee.MobileNo = model.MobileNo;
                    referee.Email = model.Email;
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
                    referee.CollegeId = model.CollegeId;
                    user.CollegeId = model.CollegeId;
                    referee.MajorId = model.MajorId;
                    user.MajorId = model.MajorId;
                    referee.Updated = DateTime.Now;
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

                    switch (userRole)
                    {
                        case ApsUserRole.Researcher:
                            Researcher researcher = null;
                            if (user.Researchers.Count == 0)
                            {
                                researcher = new Researcher
                                {
                                    FirstName = cmps.FirstName,
                                    SecondName = cmps.SecondName,
                                    ThirdName = cmps.ThirdName,
                                    LastName = cmps.LastName,
                                    Email = model.Email,
                                    CollegeId = model.CollegeId,
                                    MajorId = model.MajorId,
                                    UserId = user.Id,
                                    Created = DateTime.Now,
                                    CreatedBy = CurrentUser.Id,
                                    Updated = DateTime.Now,
                                    UpdatedBy = CurrentUser.Id,
                                    ForceToUpdateProfile = true
                                };

                                if (number != null)
                                {
                                    researcher.MobileNo = "0" + number.NationalNumber;
                                }

                                DbContext.Researchers.Add(researcher);
                                DbContext.SaveChanges();

                                user.Researchers = new List<Researcher> { researcher };
                            }

                            if (researcher == null)
                            {
                                researcher = user.Researchers.FirstOrDefault();
                                if (researcher != null)
                                {
                                    DbContext.Entry(researcher).State = EntityState.Modified;
                                }
                            }
                            break;

                        case ApsUserRole.Referee:

                            if (user.Referees.Count == 0)
                            {
                                referee = new Referee
                                {
                                    FirstName = cmps.FirstName,
                                    SecondName = cmps.SecondName,
                                    ThirdName = cmps.ThirdName,
                                    LastName = cmps.LastName,
                                    Email = model.Email,
                                    CollegeId = model.CollegeId,
                                    MajorId = model.MajorId,
                                    UserId = user.Id,
                                    Created = DateTime.Now,
                                    CreatedBy = CurrentUser.Id,
                                    Updated = DateTime.Now,
                                    UpdatedBy = CurrentUser.Id,
                                    ForceToUpdateProfile = true
                                };

                                if (number != null)
                                {
                                    referee.MobileNo = "0" + number.NationalNumber;
                                }

                                DbContext.Referees.Add(referee);
                                DbContext.SaveChanges();

                                user.Referees = new List<Referee> { referee };
                            }

                            if (referee == null)
                            {
                                referee = user.Referees.FirstOrDefault();
                                if (referee != null)
                                {
                                    DbContext.Entry(referee).State = EntityState.Modified;
                                }
                            }
                            break;
                    }

                    if (number != null)
                    {
                        user.UserProfile.MobileNo = "0" + number.NationalNumber;
                    }

                    user.UserProfile.NationalId = model.NationalId;

                    user.UserProfile.FirstName = cmps.FirstName;
                    user.UserProfile.MiddleName = cmps.SecondName;
                    user.UserProfile.ThirdName = cmps.ThirdName;
                    user.UserProfile.LastName = cmps.LastName;
                    referee.FirstName = cmps.FirstName;
                    referee.SecondName = cmps.SecondName;
                    referee.ThirdName = cmps.ThirdName;
                    referee.LastName = cmps.LastName;
                    switch (userRole)
                    {
                        case ApsUserRole.Researcher:
                            {
                                if (model.CollegeId != null)
                                    user.Researchers.First().MajorId = model.MajorId;

                                user.Researchers.First().MajorId = model.MajorId;
                            }
                            break;

                        case ApsUserRole.Referee:
                            {
                                if (model.CollegeId != null)
                                    user.Referees.First().CollegeId = model.CollegeId;

                                user.Referees.First().MajorId = model.MajorId;
                            }
                            break;
                    }

                    DbContext.SaveChanges();

                    DbContext.SaveChanges();

                    TempData["message"] = "تم تحديث المحكم بنجاح";
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
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var referee = DbContext.Referees.Find(id);
            if (referee == null)
            {
                return HttpNotFound();
            }
            return View(referee);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long? id)
        {
            var username = (string)null;
            try
            {
                var referee = DbContext.Referees.Find(id);
                username = referee.GetFullName();

                DbContext.Referees.Remove(referee);
                DbContext.SaveChanges();
                TempData["message"] = "تم حذف المحكم بنجاح";
                TempData["message_type"] = "success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, id);
                TempData["message"] = "فشل حذف المحكم" + " " + (string.IsNullOrWhiteSpace(username) ? id + "" : username) + " (" + ex.SanitizeException() + ")";
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

            if (!(model.Id > 0) || string.IsNullOrWhiteSpace(model.UserId) || !Guid.TryParse(model.UserId, out userId))
            {
                return RedirectToAction("Index", "Users");
            }

            var user = GetInstance<UsersManager>().GetUsers().SingleOrDefault(u => u.Id == userId.ToString());

            if (user == null)
            {
                TempData["message"] = "لم يتم العثور على المحكم";
                TempData["message_type"] = "danger";

                return RedirectToAction("Index", "Referees");
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

                return RedirectToAction("Edit", "Referees", new { Id = model.UserId });
            }

            var model2 = (EditUserViewModel)((ViewResult)Edit(model.Id.Value)).Model;

            return View("Edit", model2);
        }

        [AjaxOnly]
        public dynamic AjaxDeleteUser(long? id)
        {
            var ids = Guid.Empty;

            if (!(id > 0))
                return new { success = false, msg = "INVALID_APPLICAITON_ID" };

            try
            {
                var referee = UserUtils.GetReferees().SingleOrDefault(u => u.Id == id);

                if (referee == null)
                {
                    TempData["message"] = "لم يتم العثور على المحكم " + id;
                    TempData["message_type"] = "warning";

                    return RedirectToAction("Index");
                }

                referee.IsDeleted = true;
                DbContext.Entry(referee).State = EntityState.Modified;
                DbContext.SaveChanges();
                return new { success = true, msg = "تم حذف المحكم" };
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, id);
                return new { success = false, msg = ex.SanitizeException() };
            }
        }
    }
}