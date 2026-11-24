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
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(Privilages.users_view_users_list)]
    public class UsersController : BaseController
    {
        public ActionResult Index()
        {
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
            var model = new EditUserViewModel();
            var userid = User.Identity.EnhancedGetUserId();
            var user = AuthDbContext.Users.Find(userid);

            model.Colleges = GetColleges();
            model.Majors = GetMajors();
            model.Roles = GetRoles();

            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Create(EditUserViewModel model)
        {
            var roleId = Guid.Empty;
            var newUserRoleName = (IdentityRole)null;
            var newUserRole = ApsUserRole.None;

            var number = (PhoneNumber)null;

            if (!string.IsNullOrWhiteSpace(model.NationalId))
            {
                if (!IsValidNationalId(model.NationalId))
                {
                    ModelState.AddModelError("NationalId", ApsStrings.invalid_national_id);
                }
            }

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
                        if (IsGroupRole)
                        {
                            model.CollegeId = CurrentUser.CollegeId;
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

            var errorMsg = (string)null;
            if (!Utils.ValidatePassword(model.Password, Utils.IsAdmin(newUserRole), out errorMsg))
            {
                ModelState.AddModelError("Password", errorMsg);
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

            model.ExtraRoles = model.ExtraRoles.Where(r => r.RoleId != null).ToList();
            model.Roles = GetRoles();

            {
                var roles = Utils.db.Roles.ToList();
                var i = 0;

                var dupes = model.ExtraRoles.GroupBy(x => new { x.RoleId }).Where(x => x.Skip(1).Any());
                if (dupes.Count() > 0)
                {
                    ModelState.AddModelError("ExtraRoles_0_", string.Format(LangHelpers.Instance["duplicate_additional_roles"]));
                }
                foreach (var t in model.ExtraRoles)
                {
                    if (model.Roles.Count(r => r.RoleId == t.RoleId) == 0)
                    {
                        ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["role_not_found"]));
                    }
                    else if (t.RoleId == model.RoleId)
                    {
                        ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["extra_role_and_main_role_are_same"]));
                    }
                    else
                    {
                        t.Role = (ApsUserRole)Enum.Parse(typeof(ApsUserRole), roles.FirstOrDefault(r => r.Id == t.RoleId).Name);
                        t.EffectiveFromDate = Utils.StringToDate(t.EffectiveFrom);
                        t.EffectiveToDate = Utils.StringToDate(t.EffectiveTo);

                        if (Utils.IsCollegeRole(t.Role))
                        {
                            if (!(t.CollegeId > 0))
                            {
                                ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["college_required"]));
                            }
                        }
                        else
                        {
                            t.CollegeId = null;
                        }
                        if (t.EffectiveToDate < t.EffectiveFromDate)
                        {
                            ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["effective_to_before_effective_from"]));
                        }
                    }
                    i++;
                }
            }

            if (ModelState.IsValid)
            {
                var appuser = new ApplicationUser
                {
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

                var result = UserManager.Create(appuser, model.Password);

                if (result.Succeeded && newUserRoleName != null)
                {
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
                    var cmps = Utils.GetNameComponents(model.FullName);

                    userProfile.FirstName = cmps.FirstName;
                    userProfile.MiddleName = cmps.SecondName;
                    userProfile.ThirdName = cmps.ThirdName;
                    userProfile.LastName = cmps.LastName;

                    DbContext.UserProfiles.Add(userProfile);

                    switch (newUserRole)
                    {
                        case ApsUserRole.Researcher:
                            {
                                var researcher = new Researcher
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
                                    ForceToUpdateProfile = true
                                };

                                if (number != null)
                                {
                                    researcher.MobileNo = "0" + number.NationalNumber;
                                }

                                DbContext.Researchers.Add(researcher);
                            }
                            break;

                        case ApsUserRole.Referee:
                            {
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
                                    ForceToUpdateProfile = true
                                };

                                if (number != null)
                                {
                                    referee.MobileNo = "0" + number.NationalNumber;
                                }

                                DbContext.Referees.Add(referee);
                            }
                            break;
                    }

                    try
                    {
                        DbContext.SaveChanges();

                        {
                            var encounteredIds = new HashSet<long>();
                            var user = DbContext.Users.Where(u => u.Id == appuser.Id).FirstOrDefault();
                            // Extra Roles
                            var extraRoles = user.CreatedByUser25.ToList();

                            for (var i = 0; i < model.ExtraRoles.Count; i++)
                            {
                                var rmodel = model.ExtraRoles[i];

                                var exRole = DbContext.UserExtraRoles
                                    .OrderByDescending(r => r.Id)
                                    .Where(r => r.UserId == appuser.Id)
                                    .FirstOrDefault(r => rmodel.Id > 0 && rmodel.Id == r.Id);

                                if (exRole == null)
                                {
                                    exRole = new UserExtraRole
                                    {
                                        UserId = appuser.Id,
                                        Created = DateTime.Now,
                                        CreatedBy = CurrentUser.Id,
                                    };
                                    DbContext.UserExtraRoles.Add(exRole);
                                }
                                exRole.RoleId = rmodel.RoleId;
                                exRole.CollegeId = rmodel.CollegeId;
                                exRole.EffectiveFrom = rmodel.EffectiveFromDate;
                                exRole.EffectiveTo = rmodel.EffectiveToDate;
                                exRole.IsActive = rmodel.IsActive;
                                exRole.Updated = DateTime.Now;
                                exRole.UpdatedBy = CurrentUser.Id;

                                DbContext.SaveChanges();
                                encounteredIds.Add(exRole.Id);
                                var userExtraRole = DbContext.Roles.Where(x => x.Id == rmodel.RoleId).Select(x => x.Name).FirstOrDefault();
                                var userRole = Enum.Parse(typeof(ApsUserRole), userExtraRole);
                                switch (userRole)
                                {
                                    case ApsUserRole.Researcher:
                                        if (rmodel.Role == ApsUserRole.Referee)
                                        {
                                            var researcher = DbContext.Researchers.Where(x => x.UserId == user.Id).FirstOrDefault();
                                            if (researcher != null)
                                            {
                                                Referee referee = null;
                                                if (user.Referees.Count() == 0)
                                                {
                                                    referee = new Referee
                                                    {
                                                        UserId = user.Id,
                                                        Email = researcher.Email,
                                                        MobileNo = researcher.MobileNo,
                                                        FirstName = researcher.FirstName,
                                                        SecondName = researcher.SecondName,
                                                        ThirdName = researcher.ThirdName,
                                                        LastName = researcher.LastName,
                                                        UniversityId = researcher.UniversityId,
                                                        CollegeId = researcher.CollegeId,
                                                        Address = researcher.Address,
                                                        CityId = researcher.CityId,
                                                        CountryId = researcher.CountryId,
                                                        NationalityId = researcher.NationalityId,
                                                        MajorId = researcher.MajorId,
                                                        Qualification = researcher.Qualification == null ? (byte)QualificationType.Unspecified : (byte)researcher.Qualification,

                                                        Specialization = researcher.Specialization,
                                                        IsDeleted = false,

                                                        Created = DateTime.Now,
                                                        Updated = DateTime.Now,
                                                        CreatedBy = CurrentUser.Id,
                                                        UpdatedBy = CurrentUser.Id,
                                                        DepartmentId = researcher.DepartmentId,
                                                        DepartmentOther = researcher.DepartmentOther,
                                                    };
                                                }

                                                DbContext.Referees.Add(referee);
                                                DbContext.SaveChanges();
                                            }
                                        }
                                        break;

                                    case ApsUserRole.Referee:
                                        if (rmodel.Role == ApsUserRole.Researcher)
                                        {
                                            var referee = DbContext.Referees.Where(x => x.UserId == user.Id).FirstOrDefault();
                                            if (referee != null)
                                            {
                                                Researcher researcher = null;
                                                if (user.Researchers.Count() == 0)
                                                {
                                                    researcher = new Researcher
                                                    {
                                                        UserId = referee.UserId,
                                                        FirstName = referee.FirstName,
                                                        SecondName = referee.SecondName,
                                                        ThirdName = referee.ThirdName,
                                                        LastName = referee.LastName,
                                                        NationalId = user.NationalId,
                                                        AccountOwner = user.UserProfile.BankAccountHolder,

                                                        NationalityId = referee.NationalityId,
                                                        Email = referee.Email,
                                                        MobileNo = referee.MobileNo,
                                                        CollegeId = referee.CollegeId,
                                                        DateOfBirth = referee.DateOfBirth,
                                                        UniversityId = referee.UniversityId,

                                                        MajorId = referee.MajorId,
                                                        Address = referee.Address,
                                                        Created = DateTime.Now,
                                                        Updated = DateTime.Now,
                                                        CreatedBy = CurrentUser.Id,
                                                        UpdatedBy = CurrentUser.Id,
                                                        CountryId = referee.CountryId,
                                                        CityId = referee.CityId,
                                                        DepartmentId = referee.DepartmentId,
                                                        Qualification = referee.Qualification == null ? (byte)QualificationType.Unspecified : (byte)referee.Qualification,
                                                        UniversityOther = user.UniversityOther,
                                                        CollegeOther = user.CollegeOther,
                                                        DepartmentOther = referee.DepartmentOther,
                                                        Specialization = referee.Specialization
                                                    };
                                                }

                                                DbContext.Researchers.Add(researcher);
                                                DbContext.SaveChanges();
                                            }
                                        }
                                        break;
                                }
                            }
                            DbContext.SaveChanges();
                        }

                        TempData["message"] = "تم إنشاء المستخدم بنجاح";
                        TempData["message_type"] = "success";

                        return RedirectToAction("Index", "Users");
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                        string error = ex.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage;

                        TempData["message"] = "فشلت عملية إنشاء المستخدم " + error;
                        TempData["message_type"] = "danger";

                        Logger.LogExceptionAsync(ex, new { error = error, model = model });
                    }
                    catch (Exception ex)
                    {
                        var error = ex.Message;
                        TempData["message"] = "فشلت عملية إنشاء المستخدم " + error;
                        TempData["message_type"] = "danger";

                        Logger.LogExceptionAsync(ex, new { error = error, model = model });
                    }
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

            model.Colleges = GetColleges();
            model.Majors = GetMajors();
            //model.Roles = GetRoles();

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

            {
                var extraRoles = Utils.db.UserExtraRoles
                        .Where(x => x.UserId == user.Id)
                        .ToList();

                if (extraRoles.Count > 0)
                {
                    var eRoles = extraRoles
                        .OrderBy(r => r.Id)
                        .Select(r =>
                            new ExtraRoleViewModel
                            {
                                Id = r.Id,
                                RoleId = r.RoleId,
                                Role = (ApsUserRole)Enum.Parse(typeof(ApsUserRole), r.Role.Name),
                                CollegeId = r.CollegeId,
                                EffectiveFrom = r.EffectiveFrom == null ? null : r.EffectiveTo.Value.ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("en-US")),
                                EffectiveTo = r.EffectiveTo == null ? null : r.EffectiveTo.Value.ToString("yyyy-MM-dd", CultureInfo.GetCultureInfo("en-US")),
                                IsActive = r.IsActive
                            }
                        ).ToList();

                    model.ExtraRoles.AddRange(eRoles);
                }
            }

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
                    .Include("Roles")
                    .Include("UserProfile")
                    .Include("Researchers")
                    .Include("Referees")
                    .SingleOrDefault(u => u.Id == model.UserId);

                if (user == null)
                {
                    TempData["message"] = "لم يتم العثور على المستخدم " + model.UserId;
                    TempData["message_type"] = "warning";

                    return RedirectToAction("Index");
                }
            }

            var roleId = Guid.Empty;
            var userRoleName = (Role)null;
            var userRole = ApsUserRole.None;
            var IsAddRole = false;
            if (!string.IsNullOrWhiteSpace(model.RoleId) && Guid.TryParse(model.RoleId, out roleId))
            {
                var IsAdmin = user.Roles.Any(x => x.Name == ApsUserRole.SystemAdmin.ToString());
                if (!IsAdmin)
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
                            model.CollegeId = model.CollegeId == null ? CurrentUser.CollegeId : model.CollegeId;
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
                        IsAddRole = true;
                    }
                }
            }
            else
            {
                ModelState.AddModelError("RoleID", "دور المستخدم مطلوب");
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
            //var oldUserName = Utils.db.Users.Where(x => x.Id == model.UserId).Select(x => x.UserName).FirstOrDefault();
            var isUniqueUserName = Utils.db.Users.Any(x => x.UserName == model.UserName && model.UserId != x.Id);
            var isUniqueEmail = Utils.db.Users.Any(x => x.UserName == model.Email && model.UserId != x.Id);
            if (isUniqueUserName)
            {
                ModelState.AddModelError("UserName", ApsStrings.username + " " + ApsStrings.must_be_unique);
            }
            if (isUniqueEmail)
            {
                ModelState.AddModelError("Email", ApsStrings.email + " " + ApsStrings.must_be_unique);
            }

            model.ExtraRoles = model.ExtraRoles.Where(r => r.RoleId != null).ToList();
            model.Roles = GetRoles();

            {
                var roles = Utils.db.Roles.ToList();
                var i = 0;

                var dupes = model.ExtraRoles.GroupBy(x => new { x.RoleId }).Where(x => x.Skip(1).Any());
                if (dupes.Count() > 0)
                {
                    ModelState.AddModelError("ExtraRoles_0_", string.Format(LangHelpers.Instance["duplicate_additional_roles"]));
                }
                foreach (var t in model.ExtraRoles)
                {
                    if (model.Roles.Count(r => r.RoleId == t.RoleId) == 0)
                    {
                        ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["role_not_found"]));
                    }
                    else if (t.RoleId == model.RoleId)
                    {
                        ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["extra_role_and_main_role_are_same"]));
                    }
                    else
                    {
                        t.Role = (ApsUserRole)Enum.Parse(typeof(ApsUserRole), roles.FirstOrDefault(r => r.Id == t.RoleId).Name);
                        t.EffectiveFromDate = Utils.StringToDate(t.EffectiveFrom);
                        t.EffectiveToDate = Utils.StringToDate(t.EffectiveTo);
                        if (Utils.IsCollegeRole(t.Role))
                        {
                            if (!(t.CollegeId > 0))
                            {
                                ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["college_required"]));
                            }
                        }
                        else
                        {
                            t.CollegeId = null;
                        }

                        if (t.EffectiveToDate < t.EffectiveFromDate)
                        {
                            ModelState.AddModelError("ExtraRoles_" + i + "_", string.Format(LangHelpers.Instance["effective_to_before_effective_from"]));
                        }
                    }
                    i++;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cmps = Utils.GetNameComponents(model.FullName);
                    var oldRole = user.Roles.Select(x => x.Id).FirstOrDefault();
                    if (oldRole != model.RoleId)
                    {
                        user.ForceToUpdateProfile = true;
                    }
                    if (IsAddRole)
                    {
                        user.Roles.Clear();
                        user.Roles.Add(userRoleName);
                    }

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

                    {
                        var encounteredIds = new HashSet<long>();
                        //var extraRoles = user.UserDependents.ToList();
                        var extraRoles = Utils.db.UserExtraRoles
                                        .Where(x => x.UserId == user.Id)
                                        .ToList();
                        for (var i = 0; i < model.ExtraRoles.Count; i++)
                        {
                            var rmodel = model.ExtraRoles[i];
                            var exRole = DbContext.UserExtraRoles
                                .OrderByDescending(r => r.Id)
                                .Where(r => r.UserId == user.Id)
                                .FirstOrDefault(r => rmodel.Id > 0 && rmodel.Id == r.Id);
                            if (exRole == null)
                            {
                                exRole = new UserExtraRole
                                {
                                    UserId = user.Id,
                                    Created = DateTime.Now,
                                    CreatedBy = CurrentUser.Id,
                                };
                                user.ForceToUpdateProfile = true;
                                DbContext.UserExtraRoles.Add(exRole);
                            }

                            exRole.RoleId = rmodel.RoleId;
                            exRole.CollegeId = rmodel.CollegeId;
                            exRole.EffectiveFrom = rmodel.EffectiveFromDate;
                            exRole.EffectiveTo = rmodel.EffectiveToDate;
                            exRole.IsActive = rmodel.IsActive;
                            exRole.Updated = DateTime.Now;
                            exRole.UpdatedBy = CurrentUser.Id;
                            DbContext.SaveChanges();
                            encounteredIds.Add(exRole.Id);
                            switch (userRole)
                            {
                                case ApsUserRole.Researcher:
                                    if (rmodel.Role == ApsUserRole.Referee)
                                    {
                                        var researcher = DbContext.Researchers.Where(x => x.UserId == user.Id).FirstOrDefault();
                                        if (researcher != null)
                                        {
                                            Referee referee = null;
                                            if (user.Referees.Count() == 0)
                                            {
                                                referee = new Referee
                                                {
                                                    UserId = user.Id,
                                                    Email = researcher.Email,
                                                    MobileNo = researcher.MobileNo,
                                                    FirstName = researcher.FirstName,
                                                    SecondName = researcher.SecondName,
                                                    ThirdName = researcher.ThirdName,
                                                    LastName = researcher.LastName,
                                                    UniversityId = researcher.UniversityId,
                                                    CollegeId = researcher.CollegeId,
                                                    Address = researcher.Address,
                                                    CityId = researcher.CityId,
                                                    CountryId = researcher.CountryId,
                                                    NationalityId = researcher.NationalityId,
                                                    MajorId = researcher.MajorId,
                                                    Qualification = researcher.Qualification == null ? (byte)QualificationType.Unspecified : (byte)researcher.Qualification,

                                                    Specialization = researcher.Specialization,
                                                    IsDeleted = false,

                                                    Created = DateTime.Now,
                                                    Updated = DateTime.Now,
                                                    CreatedBy = CurrentUser.Id,
                                                    UpdatedBy = CurrentUser.Id,
                                                    DepartmentId = researcher.DepartmentId,
                                                    DepartmentOther = researcher.DepartmentOther,
                                                    ForceToUpdateProfile = true
                                                };
                                                DbContext.Referees.Add(referee);
                                                DbContext.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            Referee referee = null;
                                            if (user.Referees.Count() == 0)
                                            {
                                                referee = new Referee
                                                {
                                                    UserId = user.Id,
                                                    Email = user.Email,
                                                    MobileNo = user.PhoneNumber,
                                                    FirstName = cmps.FirstName,
                                                    SecondName = cmps.SecondName,
                                                    ThirdName = cmps.ThirdName,
                                                    LastName = cmps.LastName,
                                                    UniversityId = user.UniversityId,
                                                    CollegeId = user.CollegeId,
                                                    Address = user.Address,
                                                    CountryId = user.CountryId,
                                                    MajorId = user.MajorId,
                                                    IsDeleted = false,
                                                    Created = DateTime.Now,
                                                    Updated = DateTime.Now,
                                                    CreatedBy = CurrentUser.Id,
                                                    UpdatedBy = CurrentUser.Id,
                                                    DepartmentId = user.DepartmentId,
                                                    DepartmentOther = user.DepartmentOther,
                                                    ForceToUpdateProfile = true
                                                };

                                                DbContext.Referees.Add(referee);
                                                DbContext.SaveChanges();
                                            }
                                        }
                                    }
                                    break;

                                case ApsUserRole.Referee:
                                    if (rmodel.Role == ApsUserRole.Researcher)
                                    {
                                        var referee = DbContext.Referees.Where(x => x.UserId == user.Id).FirstOrDefault();
                                        if (referee != null)
                                        {
                                            Researcher researcher = null;
                                            if (user.Researchers.Count() == 0)
                                            {
                                                researcher = new Researcher
                                                {
                                                    UserId = referee.UserId,
                                                    FirstName = referee.FirstName,
                                                    SecondName = referee.SecondName,
                                                    ThirdName = referee.ThirdName,
                                                    LastName = referee.LastName,
                                                    NationalId = user.NationalId,
                                                    AccountOwner = user.UserProfile.BankAccountHolder,

                                                    NationalityId = referee.NationalityId,
                                                    Email = referee.Email,
                                                    MobileNo = referee.MobileNo,
                                                    CollegeId = referee.CollegeId,
                                                    DateOfBirth = referee.DateOfBirth,
                                                    UniversityId = referee.UniversityId,

                                                    MajorId = referee.MajorId,
                                                    Address = referee.Address,
                                                    Created = DateTime.Now,
                                                    Updated = DateTime.Now,
                                                    CreatedBy = CurrentUser.Id,
                                                    UpdatedBy = CurrentUser.Id,
                                                    CountryId = referee.CountryId,
                                                    CityId = referee.CityId,
                                                    DepartmentId = referee.DepartmentId,
                                                    Qualification = referee.Qualification == null ? (byte)QualificationType.Unspecified : (byte)referee.Qualification,
                                                    UniversityOther = user.UniversityOther,
                                                    CollegeOther = user.CollegeOther,
                                                    DepartmentOther = referee.DepartmentOther,
                                                    Specialization = referee.Specialization,
                                                    ForceToUpdateProfile = true
                                                };
                                                DbContext.Researchers.Add(researcher);
                                                DbContext.SaveChanges();
                                            }
                                        }
                                        else
                                        {
                                            Researcher researcher = null;
                                            if (user.Researchers.Count() == 0)
                                            {
                                                researcher = new Researcher
                                                {
                                                    UserId = user.Id,
                                                    FirstName = cmps.FirstName,
                                                    SecondName = cmps.SecondName,
                                                    ThirdName = cmps.ThirdName,
                                                    LastName = cmps.LastName,
                                                    NationalId = user.NationalId,
                                                    AccountOwner = user.UserProfile.BankAccountHolder,
                                                    Email = user.Email,
                                                    MobileNo = user.PhoneNumber,
                                                    CollegeId = user.CollegeId,
                                                    UniversityId = user.UniversityId,

                                                    MajorId = user.MajorId,
                                                    Address = user.Address,
                                                    Created = DateTime.Now,
                                                    Updated = DateTime.Now,
                                                    CreatedBy = CurrentUser.Id,
                                                    UpdatedBy = CurrentUser.Id,
                                                    CountryId = user.CountryId,
                                                    DepartmentId = user.DepartmentId,
                                                    UniversityOther = user.UniversityOther,
                                                    CollegeOther = user.CollegeOther,
                                                    DepartmentOther = user.DepartmentOther,
                                                    ForceToUpdateProfile = true
                                                };
                                                DbContext.Researchers.Add(researcher);
                                                DbContext.SaveChanges();
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        foreach (var cr in extraRoles)
                        {
                            if (cr.Role.Name == "Researcher")
                            {
                                var researcher = DbContext.Researchers.Where(x => x.UserId == user.Id).FirstOrDefault();
                                if (researcher != null)
                                {
                                    DbContext.Entry(researcher).State = EntityState.Deleted;
                                }
                            }
                            else if (cr.Role.Name == "Referee")
                            {
                                var referee = DbContext.Referees.Where(x => x.UserId == user.Id).FirstOrDefault();
                                if (referee != null)
                                {
                                    DbContext.Entry(referee).State = EntityState.Deleted;
                                }
                            }
                            if (!encounteredIds.Contains(cr.Id))
                            {
                                DbContext.Entry(cr).State = EntityState.Deleted;
                            }
                        }
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
                                    DbContext.SaveChanges();
                                }
                            }
                            break;

                        case ApsUserRole.Referee:
                            Referee referee = null;
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
                                    DbContext.SaveChanges();
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

                return RedirectToAction("Index", "Users");
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

                return RedirectToAction("Edit", "Users", new { Id = model.UserId });
            }

            var model2 = (EditUserViewModel)((ViewResult)Edit(model.UserId)).Model;

            return View("Edit", model2);
        }

        [AjaxOnly]
        public dynamic AjaxDeleteUser(string id)
        {
            var ids = Guid.Empty;

            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out ids))
                return new { success = false, msg = "INVALID_APPLICAITON_ID" };

            if (id == CurrentUser.Id)
            {
                return new { success = false, msg = "لا تستطيع حذف نفسك" };
            }
            try
            {
                var user = UserUtils.GetUsers().SingleOrDefault(u => u.Id == id);

                if (user == null)
                {
                    TempData["message"] = "لم يتم العثور على المستخدم " + id;
                    TempData["message_type"] = "warning";

                    return RedirectToAction("Index");
                }

                user.IsDeleted = true;
                user.UserName = user.UserName + DateTime.Now.Date.ToString() + "_Deleted";
                user.Email = user.Email + DateTime.Now.Date.ToString() + "_Deleted";
                user.PhoneNumber = user.PhoneNumber + DateTime.Now.Date.ToString() + "_Deleted";
                DbContext.Entry(user).State = EntityState.Modified;
                DbContext.SaveChanges();
                return new { success = true, msg = "تم حذف المستخدم" };
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, id);
                return new { success = false, msg = ex.SanitizeException() };
            }
        }
    }
}