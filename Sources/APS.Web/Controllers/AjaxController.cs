using APS.Core;
using APS.Core.Models;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using HeyRed.Mime;
using Humanizer;
using OfficeOpenXml;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize]
    public class AjaxController : BaseController
    {
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult LoadRequestSummaryPartial(long RequestId)
        {
            return PartialView("~/Views/Shared/_RequestSummaryPartial.cshtml", WebUtils.GetRequestSummary(RequestId));
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public void SavePreference(UserPreferenceModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Key) || string.IsNullOrWhiteSpace(model.Value))
                return;

            var key = model.Key.Trim().ToUpper();
            var value = model.Value.Trim().ToUpper();
            bool save = false;
            bool ignore = false;
            bool userAgentImportant = false;

            switch (key)
            {
                case "LANG":
                    switch (value)
                    {
                        case "AR":
                        case "EN":

                            save = true;
                            break;
                    }

                    break;

                case "MENU":

                    userAgentImportant = true;

                    switch (value)
                    {
                        case "COLLAPSED":
                        case "EXPANDED":
                            save = true;
                            break;

                        case "DONTCARE":
                            ignore = true;
                            break;
                    }
                    break;
            }

            if (ignore) return;

            if (save)
            {
                var prefs = DbContext.UserPreferences.Where(p => p.UserId == CurrentUser.Id && p.Key == key).ToList();

                if (prefs.Count == 0 && !ignore)
                {
                    var pref = new UserPreference
                    {
                        UserId = CurrentUser.Id,
                        Key = key,
                        UserAgent = Request.UserAgent,
                        Value = value
                    };

                    DbContext.UserPreferences.Add(pref);
                }
                else
                {
                    if (userAgentImportant)
                    {
                        var pref = prefs.FirstOrDefault(p => string.IsNullOrWhiteSpace(p.UserAgent) || p.UserAgent == Request.UserAgent);

                        if (pref == null && !ignore)
                        {
                            pref = new UserPreference
                            {
                                UserId = CurrentUser.Id,
                                Key = key,
                                UserAgent = Request.UserAgent,
                                Value = value
                            };

                            DbContext.UserPreferences.Add(pref);
                        }
                        else
                        {
                            pref.UserAgent = Request.UserAgent;
                            pref.Value = value;
                        }
                    }
                    else
                    {
                        prefs.First().Value = value;
                    }
                }

                DbContext.SaveChanges();
            }
        }

        [HttpPost]
        public JsonResult FindRefereeByEmail(string RefereeEmail)
        {
            var _response = new AjaxResponse();
            try
            {
                var referee = Utils.db.Referees.Where(x => x.Email == RefereeEmail && x.IsDeleted == false).FirstOrDefault();
                if (referee != null)
                {
                    _response.Data = new
                    {
                        Id = referee.Id,
                        FirstName = referee.FirstName,
                        SecondName = referee.SecondName,
                        ThirdName = referee.ThirdName,
                        LastName = referee.LastName,
                        MobileNumber = referee.MobileNo,
                        UserId = referee.UserId
                    };
                }
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.Message = ex.Message;
            }

            return Json(_response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveExternalReferee(ExternalRefereeViewModel _model)
        {
            var _response = new AjaxResponse();
            try
            {
                _response.Status = true;
                var util = PhoneNumberUtil.GetInstance();
                var number = (PhoneNumber)null;
                try
                {
                    number = util.Parse(_model.MobileNumber, "SA");
                }
                catch (NumberParseException)
                {
                    _response.Status = false;
                    ModelState.AddModelError("MobileNumber", LangHelpers.Instance["invalid_mobile_number"]);
                }
                if (!string.IsNullOrEmpty(_model.Email))
                {
                    var IsExistsInUser = Utils.db.Users.Any(x => x.Email.ToLower().TrimStart().TrimEnd() == _model.Email.ToLower().TrimStart().TrimEnd());
                    var IsExistsInReferee = Utils.db.Referees.Any(x => x.Email.ToLower().TrimStart().TrimEnd() == _model.Email.ToLower().TrimStart().TrimEnd());
                    var IsExistsInResearcher = Utils.db.Researchers.Any(x => x.Email.ToLower().TrimStart().TrimEnd() == _model.Email.ToLower().TrimStart().TrimEnd());
                    if (IsExistsInUser || IsExistsInReferee || IsExistsInResearcher)
                    {
                        _response.Status = false;
                        ModelState.AddModelError("Email", LangHelpers.Instance["email_exists"]);
                    }
                }
                if (ModelState.IsValid)
                {
                    var cmps = Utils.GetNameComponents(_model.FullName);
                    var referee = Utils.db.Referees.Where(x => x.Email == _model.Email && x.IsDeleted == false).FirstOrDefault();
                    if (referee == null)
                    {
                        referee = new Referee
                        {
                            FirstName = cmps.FirstName,
                            SecondName = cmps.SecondName,
                            ThirdName = cmps.ThirdName,
                            LastName = cmps.LastName,
                            Email = _model.Email,

                            UserId = null,
                            Created = DateTime.Now,
                            CreatedBy = CurrentUser.Id,
                            Updated = DateTime.Now,
                            UpdatedBy = CurrentUser.Id,
                        };

                        if (number != null)
                        {
                            referee.MobileNo = number.CountryCode + "" + number.NationalNumber;
                        }

                        DbContext.Referees.Add(referee);
                        DbContext.SaveChanges();
                    }
                    _response.Data = referee.Id;
                }
                else
                {
                    _response.Status = false;
                    _response.Message = string.Join("<br/>", ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                _response.Status = false;
                _response.Message = ex.Message;
            }

            return Json(_response);
        }

        [CustomAuthorize(Privilages.requests_view_requests)]
        public JsonResult ListRequests(RequestGridModel model)
        {
            DbContext.Configuration.LazyLoadingEnabled = false;

            if (IsCollegeUser) model.p = null;
            if (IsGroupRole) model.j = null;

            var nin = (string)null;
            if (Request["nin"] == null && IsValidNationalId(Request["nin"]))
            {
                nin = Request["nin"];
            }

            if (model == null) return Json(new
            {
                total = 0,
                rows = new List<RequestViewModel>(),
                current = model.Current,
                rowCount = model.RowCount
            });

            var sortBy = (string)null; ;
            var sortDirection = (string)null;
            if (model.Sort != null && model.Sort.Keys.Count > 0)
            {
                var key = model.Sort.Keys.First();
                var value = model.Sort[key];

                if (model.Sort != null && model.Sort.Count > 0)
                {
                    sortBy = key;
                    sortDirection = value;
                }
            }

            int take = 0;
            if (model.RowCount <= 0)
            {
                take = 20;
            }
            else
            {
                take = model.RowCount;
            }

            if (model.Current <= 0)
            {
                model.Current = 1;
            }

            int from = (model.Current - 1) * model.RowCount;
            long total = 0;

            var query = RequestUtils.GetRequests(
                model.p,
                model.j,
                model.u,
                null,
                model.Status,
                model.SearchPhrase,
                model.Status == RequestTab.Archive ? model.Current : 0,
                model.Status == RequestTab.Archive ? model.RowCount : 0,
                sortBy,
                sortDirection,
                LangHelpers.Instance,
                out total
            );

            var data = query
                .Select((r) => new RequestListModel
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
                })
                .OrderByDescending(r => r.DateUpdatedActualGreg)
                .ToList()
                .Select((r, i) => new RequestListModel
                {
                    Id = r.Id,
                    PublicationRequestId = r.PublicationRequestId,
                    PublicationRequestUpdated = r.PublicationRequestUpdated,
                    UniqueId = r.UniqueId,
                    Title = r.Title,
                    DepartmentName = r.DepartmentName,
                    CollegeName = r.CollegeName,
                    EmployeeId = r.EmployeeId,
                    Name = r.Name,
                    Type = LangHelpers.Instance[r.TypeEnum.ToString()],
                    DateCreated = r.DateCreatedActualGreg.Humanize(false, null, CultureInfo.GetCultureInfo("ar-SA")),
                    DateCreatedActual = r.DateCreatedActualGreg.FormatDate(),
                    DateUpdated = r.DateUpdatedActualGreg.Humanize(false, null, CultureInfo.GetCultureInfo("ar-SA")),
                    DateUpdatedActual = r.DateUpdatedActualGreg.FormatDate(),
                    Status = GetRequestStatus(r.StatusEnum),
                    Actionable = model.Status == RequestTab.Archive || r.StatusEnum == RequestStatus.Canceled || r.StatusEnum == RequestStatus.Completed ? false : RequestUtils.GetRequestActions(r).Count > 0,
                    Updated = r.Updated,
                    Index = i,
                });

            if (model.Status == RequestTab.Pending)
            {
                data = data.OrderBy(r => r.Actionable == true ? 0 : 1)
                            .ThenBy(r => r.Index)
                            .Skip((model.Current - 1) * model.RowCount)
                            .Take(model.RowCount)
                            .ToList();
            }
            else
            {
                data = data.OrderByDescending(r => r.DateUpdatedActualGreg)
                           .ThenByDescending(r => r.Id)
                           .ToList();
            }

            return Json(new
            {
                total = total,
                rows = data,
                current = model.Current,
                rowCount = model.RowCount
            });
        }

        [NonAction]
        private string GetRequestStatus(RequestStatus requestStatus)
        {
            string status = "";
            status = LangHelpers.Instance["state_" + ((requestStatus).ToString())];
            if (Utils.HasRolesAny(ApsUserRole.DeanOfLibraryAffairs) && requestStatus == RequestStatus.DistributionPendingCopyDistribution)
            {
                status = ApsStrings.entitylist_dla;
            }
            return status;
        }

        [CustomAuthorize(ApsUserRole.SystemAdmin, ApsUserRole.CommitteForPAEHead, ApsUserRole.DeanOfAcademicResearch, ApsUserRole.AcademicCouncilHead, ApsUserRole.ResearchUnitHead)]
        public JsonResult ListUsers(UsersGridModel model, ApsUserRole? role)
        {
            if (model == null) return Json(new
            {
                total = 0,
                rows = new List<User>(),
                current = model.Current,
                rowCount = model.RowCount
            });

            var sortBy = "userid";
            var sortDirection = SortDirection.Ascending;

            if (model.Sort != null && model.Sort.Count > 0)
            {
                var key = model.Sort.Keys.First();
                var value = model.Sort[key];
                sortBy = key;
                //  if (Enum.TryParse(key.ToPascalCase(), out sortBy))
                //   {
                sortDirection = value == "asc" ? SortDirection.Ascending : SortDirection.Descending;
                //   }
            }

            if (Utils.HasRolesAny(ApsUserRole.CommitteForPAEHead))
            {
                role = ApsUserRole.CommitteeforPAEMember;
            }
            else if (Utils.HasRolesAny(ApsUserRole.DeanOfAcademicResearch))
            {
                role = ApsUserRole.DeanshipOfAcademicResearchCouncilMember;
            }
            else if (Utils.HasRolesAny(ApsUserRole.AcademicCouncilHead))
            {
                role = ApsUserRole.AcademicCouncilMember;
            }
            else if (Utils.HasRolesAny(ApsUserRole.ResearchUnitHead))
            {
                role = ApsUserRole.ResearchUnitCouncilMember;
            }

            var collegeId = 0;
            if (Utils.IsCollegeUser)
            {
                if (CurrentUser.CollegeId == null)
                {
                    collegeId = -1;
                }
                else
                {
                    collegeId = CurrentUser.CollegeId.Value;
                }
            }

            string RoleId = String.Empty;
            if (role != null)
            {
                RoleId = Utils.db.Roles.FirstOrDefault(r => r.Name == role.ToString()).Id;
            }

            var roles = new List<string>();

            if (Request.QueryString["r"] != null)
            {
                var rolesStr = Request.QueryString["r"].Trim().Split('|');

                foreach (var rstr in rolesStr)
                {
                    ApsUserRole r = 0;
                    if (Enum.TryParse(rstr, true, out r) && r > 0)
                    {
                        roles.Add(r.ToString());
                    }
                }
            }

            sortBy = sortBy.ToLower();
            IQueryable<User> query = UserUtils.GetUsers(model.SearchPhrase, false, LangHelpers.Instance)
                .Where(u => roles.Count == 0 || u.Roles.Count(x => roles.Contains(x.Name)) > 0)
                .Where(u => role == null || u.Roles.Count(r => r.Id == RoleId) > 0 || u.UserExtraRoles.Count(l => l.RoleId == RoleId && l.IsActive == true) > 0)
                .Where(u => (collegeId == 0 || u.CollegeId == collegeId || u.UserExtraRoles.Count(l => l.CollegeId == collegeId) > 0));

            switch (sortBy)
            {
                case "userid":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.Id) : query.OrderByDescending(a => a.Id);
                    break;

                case "username":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.UserName) : query.OrderByDescending(a => a.UserName);
                    break;

                case "email":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.Email) : query.OrderByDescending(a => a.Email);
                    break;

                default:
                case "updated":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.Updated) : query.OrderByDescending(a => a.Updated);
                    break;
            }

            int take = 0;
            if (model.RowCount <= 0)
            {
                take = 20;
            }
            else
            {
                take = model.RowCount;
            }

            if (model.Current <= 0)
            {
                model.Current = 1;
            }
            int from = (model.Current - 1) * model.RowCount;

            var data = query.Skip(from).Take(take).ToList();
            var users = data.Select(a => new
            {
                UserId = a.Id,
                FullName = a.UserProfile.GetFullName(),
                RoleId = a.Roles.Count > 0 ? a.Roles.FirstOrDefault().Name : "",
                UserName = a.UserName,
                Email = a.Email,
                IsActivated = a.IsActivated ? ApsStrings.activated : ApsStrings.deactivated,
                CollegeName = a.CollegeId != null ? a.College.NameArabic : "",
                RoleName = a.Roles.Count() == 0 ? ApsStrings.none : WebUtils.GetUserRoleDisplayName(a),
                CvFileName = a.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault() == null ? "" : a.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault().FileName,
                CvKey = a.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault() == null ? "" : a.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault().Id.ToString()
            }).ToList()

            .Select(a => new
            {
                UserId = a.UserId,
                FullName = a.FullName,
                UserName = a.UserName,
                Email = a.Email,
                IsActivated = a.IsActivated,
                CollegeName = a.CollegeName,
                RoleName = LangHelpers.Instance[a.RoleName],
                CvFileName = a.CvFileName,
                CvKey = a.CvKey
            }).ToList();

            return Json(new
            {
                total = query.LongCount(),
                rows = users,
                current = model.Current,
                rowCount = model.RowCount
            });
        }

        [CustomAuthorize]
        public JsonResult ListReferees(UsersGridModel model)
        {
            if (model == null) return Json(new
            {
                total = 0,
                rows = new List<User>(),
                current = model.Current,
                rowCount = model.RowCount
            });

            var sortBy = "userid";
            var sortDirection = SortDirection.Ascending;

            if (model.Sort != null && model.Sort.Count > 0)
            {
                var key = model.Sort.Keys.First();
                var value = model.Sort[key];
                sortBy = key;
                sortDirection = value == "asc" ? SortDirection.Ascending : SortDirection.Descending;
            }

            var collegeId = 0;
            if (Utils.IsCollegeUser)
            {
                if (CurrentUser.CollegeId == null)
                {
                    collegeId = -1;
                }
                else
                {
                    collegeId = CurrentUser.CollegeId.Value;
                }
            }

            sortBy = sortBy.ToLower();
            IQueryable<Referee> query = UserUtils.GetReferees(model.SearchPhrase, false, LangHelpers.Instance, model.ShowAll);

            switch (sortBy)
            {
                case "userid":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.Id) : query.OrderByDescending(a => a.Id);
                    break;

                case "name":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.FirstName).ThenBy(a => a.SecondName).ThenBy(a => a.ThirdName).ThenBy(a => a.LastName) : query.OrderByDescending(a => a.LastName).ThenByDescending(a => a.ThirdName).ThenByDescending(a => a.SecondName).ThenByDescending(a => a.FirstName);
                    break;

                case "email":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.Email) : query.OrderByDescending(a => a.Email);
                    break;

                default:
                case "updated":
                    query = sortDirection == SortDirection.Ascending ? query.OrderBy(a => a.Updated) : query.OrderByDescending(a => a.Updated);
                    break;
            }

            int take = 0;
            if (model.RowCount <= 0)
            {
                take = 20;
            }
            else
            {
                take = model.RowCount;
            }

            if (model.Current <= 0)
            {
                model.Current = 1;
            }
            int from = (model.Current - 1) * model.RowCount;

            var data = query.Skip(from).Take(take).ToList();
            var users = data.Select(a => new
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = a.GetFullName(),
                UserName = a.User != null ? a.User.UserName : "",
                Email = a.Email,
                Mobile = a.MobileNo,
                IsActivated = a.User != null && a.User.IsActivated ? ApsStrings.activated : ApsStrings.deactivated,
                CollegeName = a.CollegeId != null ? a.College.NameArabic : "",
                RoleName = ApsStrings.referee.ToString(),
                CvFileName = a.User == null || a.User.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault() == null ? "" : a.User.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault().FileName,
                CvKey = a.User == null || a.User.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault() == null ? "" : a.User.UserFiles.Where(x => x.Type == (byte)FileType.Resume).OrderByDescending(x => x.Created).FirstOrDefault().Id.ToString(),
                MajorName = a.MajorId == null ? "" : a.Major.NameArabic,
                NumberOfVotes = (int?)a.RequestReferees.Where(r => r.RequestId == model.RequestId && r.CreatedByUser.Roles.FirstOrDefault().Name == model.RequestRefereeRole.ToString() && r.IsDeleted == false)
                .Sum(x => x.NumberOfVotes)
            }).ToList()

            .Select(a => new
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = a.FullName,
                UserName = a.UserName,
                Email = a.Email,
                Mobile = a.Mobile,
                IsActivated = a.IsActivated,
                CollegeName = a.CollegeName,
                MajorName = a.MajorName,
                RoleName = LangHelpers.Instance[a.RoleName],
                CvFileName = a.CvFileName,
                CvKey = a.CvKey,
                NumberOfVotes = a.NumberOfVotes >= 0 ? a.NumberOfVotes : null
            }).ToList();

            return Json(new
            {
                total = query.LongCount(),
                rows = users,
                current = model.Current,
                rowCount = model.RowCount
            });
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult GetUserDetailsByEmail(string Email)
        {
            var _response = new AjaxResponse();
            var userDetails = Utils.db.Users.Include("Roles").Include("UserProfile").SingleOrDefault(u => u.Email == Email);
            if (userDetails != null)
            {
                var userRoles = userDetails.Roles.Select(r => r.Name).ToList();
                var roles = new List<ApsUserRole>();
                foreach (var r in userRoles)
                {
                    var role = ApsUserRole.None;

                    if (Enum.TryParse(r, out role) && role != ApsUserRole.None)
                    {
                        roles.Add(role);
                    }
                }

                dynamic _userDetailsRetObj = new System.Dynamic.ExpandoObject();
                _userDetailsRetObj.FullName = userDetails.UserProfile.GetFullName();
                _userDetailsRetObj.PhoneNumber = userDetails.UserProfile.MobileNo;
                _userDetailsRetObj.Email = userDetails.Email;
                _userDetailsRetObj.UserName = userDetails.UserName;
                _userDetailsRetObj.Id = userDetails.Id;
                _userDetailsRetObj.NationalId = userDetails.UserProfile.NationalId;
                _userDetailsRetObj.Role = roles.Count > 0 ? LangHelpers.Instance[roles[0].ToString()] : "";
                _response.Data = _userDetailsRetObj;
            }
            else
            {
                _response.Status = false;
                _response.Message = LangHelpers.Instance["user_does_not_exists"];
            }
            return Json(_response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMajors(int? collegeId)
        {
            var college = (College)null;
            if (collegeId <= 0 || (college = DbContext.Colleges.SingleOrDefault(r => r.Id == collegeId)) == null)
            {
                return Json(new List<SelectListItem>());
            }

            return Json(college.Majors.OrderBy(c => IsArabic ? c.NameArabic : c.NameEnglish).Select(c => new { text = IsArabic ? c.NameArabic : c.NameEnglish, value = c.Id }).ToList());
        }

        [AllowAnonymous]
        public ActionResult GetDepartments(long? collegeId)
        {
            if (collegeId > 0)
            {
                var cat = (College)null;
                if (collegeId <= 0 || (cat = DbContext.Colleges.SingleOrDefault(r => r.Id == collegeId)) == null)
                {
                    return Json(new List<SelectListItem>());
                }

                var deps = cat.Departments.ToList();

                //var switchCulture = false;
                //if (Request["type"] != null)
                //{
                //    if (Request["dontFilter"] != "1")
                //    {
                //        deps = deps.Where(d => d.Field == null || d.Field == (byte)Enum.Parse(typeof(ReqFieldType), Request["type"])).ToList();
                //    }

                //    if ((ReqFieldType)(Enum.Parse(typeof(ReqFieldType), Request["type"])) == ReqFieldType.AppliedScienceField)
                //    {
                //        Utils.SwitchCulture("en");
                //        switchCulture = true;
                //    }
                //}

                var res = Json(
                            deps
                            .Select(c => new { text = c.LocalizedName(), value = c.Id })
                            .OrderBy(c => c.text)
                            .ToList()
                        );

                //if (switchCulture)
                //{
                //    Utils.SwitchCulture();
                //}

                return res;
            }
            else
            {
                var deps = DbContext.Departments.Where(f => false).ToList();

                //if (Request["type"] != null)
                //{
                //    deps = deps.Where(d => d.Field == null || d.Field == (byte)Enum.Parse(typeof(ReqFieldType), Request["type"])).ToList();
                //}

                return Json(
                           deps
                           .Select(c => new { text = c.LocalizedName(), value = c.Id })
                           .OrderBy(c => c.text)
                           .ToList()
                       );
            }
        }

        [AllowAnonymous]
        public ActionResult GetQualifications(ResearcherRole? type = null, ApsUserRole? role = null)
        {
            var qualifications = new List<QualificationType>();

            qualifications = Utils.QualificationTypesByResearcherType(type, role);

            return Json(
                           qualifications
                           .Select(c => new { text = LangHelpers.Instance[c.ToString()], value = c.ToString() })
                           .OrderBy(c => c.text)
                           .ToList()
                       );
        }

        [AllowAnonymous]
        public ActionResult GetCities(long? countryId, long? regionId)
        {
            if (countryId > 0)
            {
                var country = (Country)null;
                if (countryId <= 0 || (country = DbContext.Countries.SingleOrDefault(r => r.Id == countryId)) == null)
                {
                    return Json(new List<SelectListItem>());
                }

                return Json(country.Cities.OrderBy(c => c.NameEnglish).Select(c => new { text = c.NameEnglish, value = c.Id }).ToList());
            }
            else
            {
                var region = (Region)null;
                if (regionId <= 0 || (region = DbContext.Regions.SingleOrDefault(r => r.Id == regionId)) == null)
                {
                    return Json(new List<SelectListItem>());
                }

                return Json(region.Cities.OrderBy(c => c.NameEnglish).Select(c => new { text = c.NameEnglish, value = c.Id }).ToList());
            }
        }

        [AllowAnonymous]
        public ActionResult GetColleges(long? universityId)
        {
            if (universityId > 0)
            {
                var list = DbContext.Colleges
                               .Where(c => c.UniversityId == universityId)
                               .ToArray()
                               .Select(c => new SelectListItem
                               {
                                   Text = c.LocalizedName(),
                                   Value = c.Id.ToString()
                               })
                               .OrderBy(c => c.Text)
                               .ToList();

                return Json(list.Select(c => new { text = c.Text, value = c.Value }).ToList());
            }
            else
            {
                return Json(new List<SelectListItem>());
            }
        }

        [AllowAnonymous]
        public JsonResult SendVerificationCode(string to, CodeType? type, bool? imamOnly)
        {
            if (string.IsNullOrWhiteSpace(to) || !(type > 0)) return Json(new { success = false, msg = LangHelpers.Instance["INVALID_REQUEST"] });

            if (CurrentUser != null)
            {
                return Json(new { success = false, msg = LangHelpers.Instance["ALREADY_LOGGED_IN"] });
            }

            if (type == CodeType.SMS)
            {
                var validPhone = "";
                if (Utils.ValidateMobile(to, out validPhone) && UserSession.VerifiedMobile == validPhone)
                {
                    return Json(new { success = false, msg = LangHelpers.Instance["ALREADY_VERIFIED"], isVerified = true });
                }
                if (string.IsNullOrWhiteSpace(validPhone))
                {
                    return Json(new { success = false, msg = LangHelpers.Instance["invalid_mobile_number"] });
                }
                to = validPhone;
            }
            else if (type == CodeType.Email)
            {
                to = to.Trim().ToLower();
                if (Utils.ValidateEmail(to))
                {
                    if (UserSession.EmailVerified == to)
                    {
                        return Json(new { success = false, msg = LangHelpers.Instance["ALREADY_VERIFIED"], isVerified = true });
                    }

                    if (imamOnly == true && !to.EndsWith("@imamu.edu.sa") && !to.EndsWith(".imamu.edu.sa") && !to.EndsWith("kay-ict.com"))
                    {
                        return Json(new { success = false, msg = LangHelpers.Instance["email_must_be_imam_edu"], isVerified = true });
                    }
                }

                if (Utils.db.Users.Count(u => u.Email == to) > 0)
                {
                    return Json(new { success = false, msg = LangHelpers.Instance["EMAIL_EXISTS"] });
                }
            }

            if (!Utils.IsLocalhost)
            {
                if (type == CodeType.SMS && UserSession.LastSmsCodeGenerated != null && UserSession.LastSmsCodeGenerated.Value.AddMinutes(2) > DateTime.Now)
                {
                    return Json(new { success = false, msg = LangHelpers.Instance["PLEASE_WAIT_FOR_2_MINUTES"] });
                }

                if (type == CodeType.Email && UserSession.LastEmailCodeGenerated != null && UserSession.LastEmailCodeGenerated.Value.AddMinutes(2) > DateTime.Now)
                {
                    return Json(new { success = false, msg = LangHelpers.Instance["PLEASE_WAIT_FOR_2_MINUTES"] });
                }
            }

            var gKey = "";
            var err = "";
            if (!GenerateAndSendVerificationCode(type.Value, to, out err, out gKey))
            {
                return Json(new { success = false, msg = err });
            }

            return Json(new { success = true, key = gKey });
        }

        [AllowAnonymous]
        public JsonResult VerifyCode(string to, CodeType? type, string key, string code)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(to) || !(type > 0)) return Json(new { success = false, msg = LangHelpers.Instance["INVALID_REQUEST"] });

            if (CurrentUser != null)
            {
                return Json(new { success = false, msg = LangHelpers.Instance["ALREADY_LOGGED_IN"] });
            }
            if (type == CodeType.SMS)
            {
                var validPhone = "";
                if (Utils.ValidateMobile(to, out validPhone) && UserSession.VerifiedMobile == validPhone)
                {
                    return Json(new { success = true, msg = LangHelpers.Instance["ALREADY_VERIFIED"], isVerified = true });
                }

                to = validPhone;
            }
            else if (type == CodeType.Email)
            {
                to = to.Trim().ToLower();
                if (Utils.ValidateEmail(to) && UserSession.EmailVerified == to)
                {
                    return Json(new { success = true, msg = LangHelpers.Instance["ALREADY_VERIFIED"], isVerified = true });
                }
            }

            var db = Utils.db;
            var link = db.UserTemporaryUrls.FirstOrDefault(l => l.Key == key && l.SessionId == UserSession.Id);

            if (link == null)
            {
                return Json(new { success = false, msg = LangHelpers.Instance["NOT_FOUND"] });
            }

            if (link.IsUsed == true)
            {
                return Json(new { success = false, msg = LangHelpers.Instance["ALREADY_USED"] });
            }

            if (link.Expires < DateTime.Now)
            {
                return Json(new { success = false, msg = LangHelpers.Instance["EXPIRED"] });
            }

            var codeInt = (int)0;
            if (int.TryParse(code, out codeInt) && link.ShortCode != null && link.ShortCode == codeInt)
            {
                link.IsUsed = true;
                if (type == CodeType.SMS)
                    UserSession.VerifiedMobile = to;
                else
                    UserSession.EmailVerified = to;

                db.SaveChanges();

                return Json(new { success = true });
            }
            return Json(new { success = false, msg = LangHelpers.Instance["INVALID_CODE"] });
        }

        [AllowAnonymous]
        public dynamic Heartbeat()
        {
            var isLocked = UserSession != null && UserSession.Locked == true;
            var isExpired = UserSession == null || UserSession.Expires < DateTime.UtcNow || CurrentUser == null || Roles == null || Roles.Length == 0;

            var status = isExpired ? "EXPIRED" : (isLocked ? "LOCKED" : "VALID");
            return Json(new { status = status });
        }

        public FileResult ExportDocuments(string Type, int ReportNo = 0, int Status = 0, int rType = 0, int requestId = 0)
        {
            var path = Path.GetTempFileName();
            //switch (Type)
            //{
            //    case "financial":
            //        {
            //            int totalsubmitted = 0;
            //            int totalapproved = 0;
            //            int totalrejected = 0;
            //            int totalamount = 0;
            //            var documents = DbContext.Database.SqlQuery<ViewRequestsByFinancialTypesReport>("select * from ViewRequestsByFinancialTypesReport")
            //             .ToList()
            //             .Select(c => new ViewRequestsByFinancialTypesReport
            //             {
            //                 TotalAmount = c.TotalAmount,
            //                 UserRole = LangHelpers.Instance[c.UserRole],
            //                 ApprovedRequests = c.ApprovedRequests,
            //                 UserRoleId = c.UserRoleId,
            //                 RejectedRequests = c.RejectedRequests,
            //                 SubmittedRequests = c.SubmittedRequests
            //             })
            //             .ToList();

            //            using (ExcelPackage pkg = new ExcelPackage(new FileInfo(path), new FileInfo(Utils.MapPath("~/Content/templates/template_financial_report.xlsx"))))
            //            {
            //                var ws = pkg.Workbook.Worksheets.First();
            //                ws.Name = ApsStrings.name;
            //                ws.View.RightToLeft = Utils.IsUserLanguageArabic;
            //                var row = 1;
            //                var column = 1;
            //                ws.Cells[row, column++].Value = "نوع الشخص";
            //                ws.Cells[row, column++].Value = "عدد طلبات الصرف";
            //                ws.Cells[row, column++].Value = "عدد الطلبات المقبولة";
            //                ws.Cells[row, column++].Value = "عدد الطلبات المرفوضة";
            //                ws.Cells[row, column++].Value = "إجمالي المبلغ المصروف";

            //                // styling
            //                ws.Column(2).Width = 50;
            //                ws.Column(2).Style.Font.Bold = true;
            //                ws.Column(2).Style.WrapText = true;

            //                foreach (var drow in documents)
            //                {
            //                    column = 1;
            //                    row++;
            //                    if (drow.ApprovedRequests != null)
            //                    {
            //                        totalapproved += (int)drow.ApprovedRequests;
            //                    }
            //                    else
            //                    {
            //                        totalapproved += 0;
            //                    }
            //                    if (drow.SubmittedRequests != null)
            //                    {
            //                        totalsubmitted += (int)drow.SubmittedRequests;
            //                    }
            //                    else
            //                    {
            //                        totalsubmitted += 0;
            //                    }
            //                    if (drow.RejectedRequests != null)
            //                    {
            //                        totalrejected += (int)drow.RejectedRequests;
            //                    }
            //                    else
            //                    {
            //                        totalrejected += 0;
            //                    }
            //                    if (drow.TotalAmount != null)
            //                    {
            //                        totalamount += (int)drow.TotalAmount;
            //                    }
            //                    else
            //                    {
            //                        totalamount += 0;
            //                    }
            //                    ws.Cells[row, column++].Value = drow.UserRole;
            //                    ws.Cells[row, column++].Value = drow.SubmittedRequests == null ? 0 : drow.SubmittedRequests;
            //                    ws.Cells[row, column++].Value = drow.ApprovedRequests == null ? 0 : drow.ApprovedRequests;
            //                    ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.RejectedRequests;
            //                    ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.TotalAmount;
            //                }
            //                column = 1;
            //                row++;
            //                ws.Cells[row, column++].Value = @ApsStrings.totalA;
            //                ws.Cells[row, column++].Value = totalsubmitted;
            //                ws.Cells[row, column++].Value = totalapproved;
            //                ws.Cells[row, column++].Value = totalrejected;
            //                ws.Cells[row, column++].Value = totalamount;
            //                pkg.Save();
            //            }
            //        }
            //        break;
            //    case "types":
            //        {
            //            int totalsubmitted = 0;
            //            int totalapproved = 0;
            //            int totalrejected = 0;
            //            int totalprinted = 0;
            //            var documents = DbContext.Database.SqlQuery<ViewRequestsByTypeReport>("select * from ViewRequestsByTypeReport")
            //                                    .ToList()
            //                                    .Select(c => new ViewRequestsByTypeReport
            //                                    {
            //                                        PublicationTypeId = c.PublicationTypeId,
            //                                        PublicationType = LangHelpers.Instance[c.PublicationType],
            //                                        ApprovedRequests = c.ApprovedRequests,
            //                                        PrintedRequests = c.PrintedRequests,
            //                                        RejectedRequests = c.RejectedRequests,
            //                                        SubmittedRequests = c.SubmittedRequests
            //                                    })
            //                                    .ToList();

            //            using (ExcelPackage pkg = new ExcelPackage(new FileInfo(path), new FileInfo(Utils.MapPath("~/Content/templates/template_financial_report.xlsx"))))
            //            {
            //                var ws = pkg.Workbook.Worksheets.First();
            //                ws.Name = ApsStrings.name;
            //                ws.View.RightToLeft = Utils.IsUserLanguageArabic;
            //                var row = 1;
            //                var column = 1;
            //                ws.Cells[row, column++].Value = "نوع طلب النشر";
            //                ws.Cells[row, column++].Value = "عدد الطلبات المقدمة";
            //                ws.Cells[row, column++].Value = "عدد الطلبات المقبولة";
            //                ws.Cells[row, column++].Value = "عدد الطلبات المرفوضة";
            //                ws.Cells[row, column++].Value = "عدد الطلبات التي تم طباعتها";

            //                // styling
            //                ws.Column(2).Width = 50;
            //                ws.Column(2).Style.Font.Bold = true;
            //                ws.Column(2).Style.WrapText = true;

            //                foreach (var drow in documents)
            //                {
            //                    column = 1;
            //                    row++;
            //                    if (drow.ApprovedRequests != null)
            //                    {
            //                        totalapproved += (int)drow.ApprovedRequests;
            //                    }
            //                    else
            //                    {
            //                        totalapproved += 0;
            //                    }
            //                    if (drow.SubmittedRequests != null)
            //                    {
            //                        totalsubmitted += (int)drow.SubmittedRequests;
            //                    }
            //                    else
            //                    {
            //                        totalsubmitted += 0;
            //                    }
            //                    if (drow.RejectedRequests != null)
            //                    {
            //                        totalrejected += (int)drow.RejectedRequests;
            //                    }
            //                    else
            //                    {
            //                        totalrejected += 0;
            //                    }
            //                    if (drow.PrintedRequests != null)
            //                    {
            //                        totalprinted += (int)drow.PrintedRequests;
            //                    }
            //                    else
            //                    {
            //                        totalprinted += 0;
            //                    }
            //                    ws.Cells[row, column++].Value = drow.PublicationType;
            //                    ws.Cells[row, column++].Value = drow.SubmittedRequests == null ? 0 : drow.SubmittedRequests;
            //                    ws.Cells[row, column++].Value = drow.ApprovedRequests == null ? 0 : drow.ApprovedRequests;
            //                    ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.RejectedRequests;
            //                    ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.PrintedRequests;
            //                }
            //                column = 1;
            //                row++;
            //                ws.Cells[row, column++].Value = @ApsStrings.totalA;
            //                ws.Cells[row, column++].Value = totalsubmitted;
            //                ws.Cells[row, column++].Value = totalapproved;
            //                ws.Cells[row, column++].Value = totalrejected;
            //                ws.Cells[row, column++].Value = totalprinted;
            //                pkg.Save();
            //            }
            //        }
            //        break;
            //    case "detailed":
            //        {
            //            var documents = new List<DetailedRequestsViewModel>();
            //            if (ReportNo == 1)
            //            {
            //                documents = Utils.db.FuncRequestsByType((short)rType, (short)Status)
            //                    .ToList()
            //                    .Select(c => new DetailedRequestsViewModel
            //                    {
            //                        Id = c.Id,
            //                        Title = c.Title,
            //                        FirstName = c.FirstName,
            //                        LastName = c.LastName,
            //                        PublicationType = c.PublicationType,
            //                        Status = c.Status
            //                    }).ToList();
            //            }
            //            else
            //            {
            //                documents = Utils.db.FuncRequestsByFinancialTypes((short)rType, (short)Status)
            //                  .ToList()
            //                  .Select(c => new DetailedRequestsViewModel
            //                  {
            //                      Id = c.Id,
            //                      Title = c.Title,
            //                      FirstName = c.FirstName,
            //                      LastName = c.LastName,
            //                      PublicationType = c.PublicationType,
            //                      Status = c.Status
            //                  }).ToList();
            //            }
            //            using (ExcelPackage pkg = new ExcelPackage(new FileInfo(path), new FileInfo(Utils.MapPath("~/Content/templates/template_detailed_report.xlsx"))))
            //            {
            //                var ws = pkg.Workbook.Worksheets.First();
            //                ws.Name = ApsStrings.name;
            //                ws.View.RightToLeft = Utils.IsUserLanguageArabic;
            //                var row = 1;
            //                var column = 1;
            //                ws.Cells[row, column++].Value = "الرقم";
            //                ws.Cells[row, column++].Value = "العنوان";
            //                ws.Cells[row, column++].Value = "الاسم";
            //                ws.Cells[row, column++].Value = "نوع النشر";
            //                ws.Cells[row, column++].Value = "Status";

            //                // styling
            //                ws.Column(2).Width = 50;
            //                ws.Column(2).Style.Font.Bold = true;
            //                ws.Column(2).Style.WrapText = true;

            //                foreach (var drow in documents)
            //                {
            //                    column = 1;
            //                    row++;
            //                    ws.Cells[row, column++].Value = drow.Id;
            //                    ws.Cells[row, column++].Value = drow.Title;
            //                    ws.Cells[row, column++].Value = drow.FirstName + " " + drow.LastName;
            //                    ws.Cells[row, column++].Value = LangHelpers.Instance[((PublicationType)drow.PublicationType).ToString()];
            //                    ws.Cells[row, column++].Value = LangHelpers.Instance[((RequestStatus)drow.Status).ToString()];
            //                }
            //                pkg.Save();
            //            }
            //            break;
            //        }
            //}

            path = ExportData(path, Type, ReportNo, Status, rType, requestId);
            return File(path, MimeTypesMap.GetMimeType("xlsx"), "DocumentReport_" + DateTime.Now.ToString("yyyy_MM_dd") + ".xlsx");
        }

        public FileResult ExportDocumentsToPdf(string Type, int ReportNo = 0, int Status = 0, int rType = 0)
        {
            var path = Path.GetTempFileName();
            path = ExportData(path, Type, ReportNo, Status, rType);
            ExcelWorkbook workbook;
            //Load excel file
            //workbook.LoadFromFile(path.Name);
            ////Save excel file to pdf file.
            //workbook.SaveToFile("result.pdf", Spire.Xls.FileFormat.PDF);
            return File(path, MimeTypesMap.GetMimeType("pdf"), "DocumentPdfReport_" + DateTime.Now.ToString("yyyy_MM_dd") + ".pdf");
        }

        private string ExportData(string path, string Type, int ReportNo = 0, int Status = 0, int rType = 0, int requestId = 0)
        {
            switch (Type)
            {
                case "financial":
                    {
                        int totalsubmitted = 0;
                        int totalapproved = 0;
                        int totalrejected = 0;
                        int totalamount = 0;
                        var documents = DbContext.Database.SqlQuery<ViewRequestsByFinancialTypesReport>("select * from ViewRequestsByFinancialTypesReport")
                         .ToList()
                         .Select(c => new ViewRequestsByFinancialTypesReport
                         {
                             TotalAmount = c.TotalAmount,
                             //UserRole = LangHelpers.Instance[c.UserRole],
                             UserRoleAr = c.UserRoleAr,
                             ApprovedRequests = c.ApprovedRequests,
                             UserRoleId = c.UserRoleId,
                             RejectedRequests = c.RejectedRequests,
                             SubmittedRequests = c.SubmittedRequests
                         })
                         .ToList();

                        using (ExcelPackage pkg = new ExcelPackage(new FileInfo(path), new FileInfo(Utils.MapPath("~/Content/templates/template_financial_report.xlsx"))))
                        {
                            var ws = pkg.Workbook.Worksheets.First();
                            ws.Name = ApsStrings.name;
                            ws.View.RightToLeft = Utils.IsUserLanguageArabic;
                            var row = 1;
                            var column = 1;
                            ws.Cells[row, column++].Value = "نوع الشخص";
                            ws.Cells[row, column++].Value = "عدد طلبات الصرف";
                            ws.Cells[row, column++].Value = "عدد الطلبات المقبولة";
                            ws.Cells[row, column++].Value = "عدد الطلبات المرفوضة";
                            ws.Cells[row, column++].Value = "إجمالي المبلغ المصروف";

                            // styling
                            ws.Column(2).Width = 50;
                            ws.Column(2).Style.Font.Bold = true;
                            ws.Column(2).Style.WrapText = true;

                            foreach (var drow in documents)
                            {
                                column = 1;
                                row++;
                                if (drow.ApprovedRequests != null)
                                {
                                    totalapproved += (int)drow.ApprovedRequests;
                                }
                                else
                                {
                                    totalapproved += 0;
                                }
                                if (drow.SubmittedRequests != null)
                                {
                                    totalsubmitted += (int)drow.SubmittedRequests;
                                }
                                else
                                {
                                    totalsubmitted += 0;
                                }
                                if (drow.RejectedRequests != null)
                                {
                                    totalrejected += (int)drow.RejectedRequests;
                                }
                                else
                                {
                                    totalrejected += 0;
                                }
                                if (drow.TotalAmount != null)
                                {
                                    totalamount += (int)drow.TotalAmount;
                                }
                                else
                                {
                                    totalamount += 0;
                                }
                                ws.Cells[row, column++].Value = drow.UserRoleAr;
                                ws.Cells[row, column++].Value = drow.SubmittedRequests == null ? 0 : drow.SubmittedRequests;
                                ws.Cells[row, column++].Value = drow.ApprovedRequests == null ? 0 : drow.ApprovedRequests;
                                ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.RejectedRequests;
                                ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.TotalAmount;
                            }
                            column = 1;
                            row++;
                            ws.Cells[row, column++].Value = @ApsStrings.totalA;
                            ws.Cells[row, column++].Value = totalsubmitted;
                            ws.Cells[row, column++].Value = totalapproved;
                            ws.Cells[row, column++].Value = totalrejected;
                            ws.Cells[row, column++].Value = totalamount;
                            pkg.Save();
                        }
                    }
                    break;

                case "types":
                    {
                        int totalsubmitted = 0;
                        int totalapproved = 0;
                        int totalrejected = 0;
                        int totalprinted = 0;
                        var documents = DbContext.Database.SqlQuery<ViewRequestsByTypeReport>("select * from ViewRequestsByTypeReport")
                                                .ToList()
                                                .Select(c => new ViewRequestsByTypeReport
                                                {
                                                    PublicationTypeId = c.PublicationTypeId,
                                                    //PublicationType = LangHelpers.Instance[c.PublicationType],
                                                    PublicationTypeAr = c.PublicationTypeAr,
                                                    ApprovedRequests = c.ApprovedRequests,
                                                    PrintedRequests = c.PrintedRequests,
                                                    RejectedRequests = c.RejectedRequests,
                                                    SubmittedRequests = c.SubmittedRequests
                                                })
                                                .ToList();

                        using (ExcelPackage pkg = new ExcelPackage(new FileInfo(path), new FileInfo(Utils.MapPath("~/Content/templates/template_financial_report.xlsx"))))
                        {
                            var ws = pkg.Workbook.Worksheets.First();
                            ws.Name = ApsStrings.name;
                            ws.View.RightToLeft = Utils.IsUserLanguageArabic;
                            var row = 1;
                            var column = 1;
                            ws.Cells[row, column++].Value = "نوع طلب النشر";
                            ws.Cells[row, column++].Value = "عدد الطلبات المقدمة";
                            ws.Cells[row, column++].Value = "عدد الطلبات المقبولة";
                            ws.Cells[row, column++].Value = "عدد الطلبات المرفوضة";
                            ws.Cells[row, column++].Value = "عدد الطلبات التي تم طباعتها";

                            // styling
                            ws.Column(2).Width = 50;
                            ws.Column(2).Style.Font.Bold = true;
                            ws.Column(2).Style.WrapText = true;

                            foreach (var drow in documents)
                            {
                                column = 1;
                                row++;
                                if (drow.ApprovedRequests != null)
                                {
                                    totalapproved += (int)drow.ApprovedRequests;
                                }
                                else
                                {
                                    totalapproved += 0;
                                }
                                if (drow.SubmittedRequests != null)
                                {
                                    totalsubmitted += (int)drow.SubmittedRequests;
                                }
                                else
                                {
                                    totalsubmitted += 0;
                                }
                                if (drow.RejectedRequests != null)
                                {
                                    totalrejected += (int)drow.RejectedRequests;
                                }
                                else
                                {
                                    totalrejected += 0;
                                }
                                if (drow.PrintedRequests != null)
                                {
                                    totalprinted += (int)drow.PrintedRequests;
                                }
                                else
                                {
                                    totalprinted += 0;
                                }
                                ws.Cells[row, column++].Value = drow.PublicationTypeAr;
                                ws.Cells[row, column++].Value = drow.SubmittedRequests == null ? 0 : drow.SubmittedRequests;
                                ws.Cells[row, column++].Value = drow.ApprovedRequests == null ? 0 : drow.ApprovedRequests;
                                ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.RejectedRequests;
                                ws.Cells[row, column++].Value = drow.RejectedRequests == null ? 0 : drow.PrintedRequests;
                            }
                            column = 1;
                            row++;
                            ws.Cells[row, column++].Value = @ApsStrings.totalA;
                            ws.Cells[row, column++].Value = totalsubmitted;
                            ws.Cells[row, column++].Value = totalapproved;
                            ws.Cells[row, column++].Value = totalrejected;
                            ws.Cells[row, column++].Value = totalprinted;
                            pkg.Save();
                        }
                    }
                    break;

                case "detailed":
                    {
                        var documents = new List<DetailedRequestsViewModel>();
                        if (ReportNo == 1)
                        {
                            documents = Utils.db.FuncRequestsByType((short)rType, (short)Status)
                                .ToList()
                                .Select(c => new DetailedRequestsViewModel
                                {
                                    Id = c.Id,
                                    Title = c.Title,
                                    FirstName = c.FirstName,
                                    LastName = c.LastName,
                                    PublicationType = c.PublicationType,
                                    Status = c.Status
                                }).ToList();
                        }
                        else
                        {
                            documents = Utils.db.FuncRequestsByFinancialTypes((short)rType, (short)Status)
                              .ToList()
                              .Select(c => new DetailedRequestsViewModel
                              {
                                  Id = c.Id,
                                  Title = c.Title,
                                  FirstName = c.FirstName,
                                  LastName = c.LastName,
                                  PublicationType = c.PublicationType,
                                  Status = c.Status
                              }).ToList();
                        }
                        using (ExcelPackage pkg = new ExcelPackage(new FileInfo(path), new FileInfo(Utils.MapPath("~/Content/templates/template_detailed_report.xlsx"))))
                        {
                            var ws = pkg.Workbook.Worksheets.First();
                            ws.Name = ApsStrings.name;
                            ws.View.RightToLeft = Utils.IsUserLanguageArabic;
                            var row = 1;
                            var column = 1;
                            ws.Cells[row, column++].Value = "الرقم";
                            ws.Cells[row, column++].Value = "العنوان";
                            ws.Cells[row, column++].Value = "الاسم";
                            ws.Cells[row, column++].Value = "نوع النشر";
                            ws.Cells[row, column++].Value = "Status";

                            // styling
                            ws.Column(2).Width = 50;
                            ws.Column(2).Style.Font.Bold = true;
                            ws.Column(2).Style.WrapText = true;

                            foreach (var drow in documents)
                            {
                                column = 1;
                                row++;
                                ws.Cells[row, column++].Value = drow.Id;
                                ws.Cells[row, column++].Value = drow.Title;
                                ws.Cells[row, column++].Value = drow.FirstName + " " + drow.LastName;
                                ws.Cells[row, column++].Value = LangHelpers.Instance[((PublicationType)drow.PublicationType).ToString()];
                                ws.Cells[row, column++].Value = LangHelpers.Instance[((RequestStatus)drow.Status).ToString()];
                            }
                            pkg.Save();
                        }
                        break;
                    }

                case "entities":
                    {
                        var documents = new List<DistributionEntityViewModel>();

                        documents = Utils.db.RequestDistributionListAgencies
                            .Where(x => x.RequestId == requestId)
                            .ToList()
                            .Select(c => new DistributionEntityViewModel
                            {
                                Id = c.Id,
                                Name = c.Name,
                                NoOfCopies = c.NoOfCopies
                            }).ToList();

                        using (ExcelPackage pkg = new ExcelPackage(new FileInfo(path), new FileInfo(Utils.MapPath("~/Content/templates/template_distributionentities_print.xlsx"))))
                        {
                            var ws = pkg.Workbook.Worksheets.First();
                            ws.Name = ApsStrings.name;
                            ws.View.RightToLeft = Utils.IsUserLanguageArabic;
                            var row = 1;
                            var column = 1;
                            ws.Cells[row, column++].Value = "الرقم";
                            ws.Cells[row, column++].Value = "إسم الجهة";
                            ws.Cells[row, column++].Value = "عدد النسخ";

                            // styling
                            ws.Column(2).Width = 50;
                            ws.Column(2).Style.Font.Bold = true;
                            ws.Column(2).Style.WrapText = true;

                            foreach (var drow in documents)
                            {
                                column = 1;
                                row++;
                                ws.Cells[row, column++].Value = row;
                                ws.Cells[row, column++].Value = drow.Name;
                                ws.Cells[row, column++].Value = drow.NoOfCopies;
                            }
                            pkg.Save();
                        }
                        break;
                    }
            }
            return path;
        }
    }

    public class AjaxResponse
    {
        public AjaxResponse()
        {
            Status = true;
            Message = String.Empty;
        }

        public bool Status { set; get; }
        public string Message { set; get; }
        public object Data { set; get; }
    }
}