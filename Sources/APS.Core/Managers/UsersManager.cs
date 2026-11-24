using APS.Data;
using APS.Data.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;

namespace APS.Core.Managers
{
    public class UsersManager : AbstractManager
    {
        public IQueryable<User> GetUsers(string query = null, bool includeDeleted = true, Translator translator = null, bool avoidCollegeIdFilter = false)
        {
            var predicate = PredicateBuilder.BaseAnd<User>();

            if (IsCollegeUser)
            {
                if (!avoidCollegeIdFilter)
                {
                    if (IsCollegeUser)
                    {
                        if (CurrentUser.CollegeId == null) return new List<User>().AsQueryable();

                        predicate = predicate.And(p => p.CollegeId == CurrentUser.CollegeId);
                    }
                    else
                    {
                        if (CurrentUser.CollegeId == null) return new List<User>().AsQueryable();

                        predicate = predicate.And(p => p.CollegeId == CurrentUser.CollegeId);
                    }
                }
            }
            else if (!IsSystemAdmin)
            {
                predicate = predicate.And(p => p.Roles.Count(r => r.Name == ApsUserRole.SystemAdmin.ToString()) == 0);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower().Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
                var predicate2 = PredicateBuilder.BaseOr<User>();

                predicate2 = predicate2.Or(a => a.FullName.Contains(query));
                predicate2 = predicate2.Or(a => a.UserName.Contains(query));
                predicate2 = predicate2.Or(a => a.Email.Contains(query));

                predicate2 = predicate2.Or(a => a.UserProfile != null && ((a.UserProfile.FirstName != null && a.UserProfile.FirstName.Contains(query)) || (a.UserProfile.LastName != null && a.UserProfile.LastName.Contains(query)) || (a.UserProfile.FirstName != null && a.UserProfile.LastName != null && (a.UserProfile.FirstName + " " + a.UserProfile.LastName).Contains(query))));
                predicate2 = predicate2.Or(a => a.Referees.Count(r => (r.FirstName != null && r.FirstName.Contains(query)) || (r.LastName != null && r.LastName.Contains(query)) || (r.FirstName != null && r.LastName != null && (r.FirstName + " " + r.LastName).Contains(query))) > 0);
                predicate2 = predicate2.Or(a => a.Researchers.Count(r => (r.FirstName != null && r.FirstName.Contains(query)) || (r.LastName != null && r.LastName.Contains(query)) || (r.FirstName != null && r.LastName != null && (r.FirstName + " " + r.LastName).Contains(query))) > 0);

                if (translator != null)
                {
                    var roles = Enum.GetValues(typeof(ApsUserRole));

                    var matchingRoles = new List<string>();

                    foreach (var r in roles)
                    {
                        var arabic = translator.GetLangValueByString(r.ToString(), true, "ar");
                        var english = translator.GetLangValueByString(r.ToString(), true, "en");

                        if (arabic != null && arabic.Contains(query))
                            matchingRoles.Add(r.ToString());
                        else if (english != null && english.Contains(query))
                            matchingRoles.Add(r.ToString());
                    }

                    if (matchingRoles.Count > 0)
                    {
                        foreach (var x in matchingRoles)
                        {
                            predicate2 = predicate2.Or(a => a.Roles.Count(r => r.Name == x) > 0);
                        }
                    }
                }

                predicate = predicate.And(predicate2);
            }

            if (!includeDeleted)
            {
                predicate = predicate.And(u => u.IsDeleted == false);
            }

            var result = DbContext.Users.Where(predicate);
            return result;
        }

        public IQueryable<Referee> GetReferees(string query = null, bool includeDeleted = true, Translator translator = null, bool showAll = false)
        {
            var predicate = PredicateBuilder.BaseAnd<Referee>();
            if (!showAll)
            {
                var validateReferee = PredicateBuilder.BaseOr<Referee>();
                validateReferee = validateReferee.Or(v => (!string.IsNullOrEmpty(v.FirstName)) &&
                  (!string.IsNullOrEmpty(v.Email)) &&
                  (!string.IsNullOrEmpty(v.MobileNo))
                  );
                predicate = predicate.And(validateReferee);
            }
            //if (IsCollegeUser)
            //{
            //    if (IsCollegeUser)
            //    {
            //        if (CurrentUser.CollegeId == null) return new List<Referee>().AsQueryable();

            //        predicate = predicate.And(p => p.CollegeId == CurrentUser.CollegeId);
            //    }
            //    else
            //    {
            //        if (CurrentUser.CollegeId == null) return new List<Referee>().AsQueryable();

            //        predicate = predicate.And(p => p.CollegeId == CurrentUser.CollegeId);
            //    }
            //}

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower().Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
                var predicate2 = PredicateBuilder.BaseOr<Referee>();

                predicate2 = predicate2.Or(r =>
                    (r.FirstName != null && r.FirstName.ToLower().Contains(query)) ||
                    (r.SecondName != null && r.SecondName.ToLower().Contains(query)) ||
                    (r.ThirdName != null && r.ThirdName.ToLower().Contains(query)) ||
                    (r.LastName != null && r.LastName.ToLower().Contains(query)) ||
                    (r.FirstName != null && r.SecondName != null && (r.FirstName + " " + r.SecondName).ToLower().Contains(query)) ||
                    (r.FirstName != null && r.SecondName != null && r.ThirdName != null && (r.FirstName + " " + r.SecondName + " " + r.ThirdName).ToLower().Contains(query)) ||
                    (r.FirstName != null && r.SecondName != null && r.ThirdName != null && r.LastName != null && (r.FirstName + " " + r.SecondName + " " + r.ThirdName + " " + r.LastName).ToLower().Contains(query)) ||
                    (r.FirstName != null && r.ThirdName != null && r.LastName != null && (r.FirstName + " " + r.ThirdName + " " + r.LastName).ToLower().Contains(query)) ||
                    (r.FirstName != null && r.LastName != null && (r.FirstName + " " + r.LastName).ToLower().Contains(query))

                );

                predicate2 = predicate2.Or(a => a.Major.NameArabic.Contains(query) || a.Major.NameEnglish.Contains(query));

                predicate2 = predicate2.Or(a => a.Email != null && a.Email.Contains(query));
                predicate2 = predicate2.Or(a => a.MobileNo != null && a.MobileNo.Contains(query));
                predicate2 = predicate2.Or(a => a.Major.NameArabic != null && a.Major.NameArabic.Contains(query));
                predicate = predicate.And(predicate2);
            }

            if (!includeDeleted)
            {
                predicate = predicate.And(u => u.IsDeleted == false);
            }

            var result = DbContext.Referees.Where(predicate);
            var count = result.Count();
            return result;
        }

        public HashSet<KeyValuePair<ApsUserRole, int?>> GetDelegations()
        {
            var v = Utils.GetDictVal("DELEGATIONS");
            if (v != null) return (HashSet<KeyValuePair<ApsUserRole, int?>>)v;

            var db = Utils.db;
            var delegations = Enumerable.Empty<object>()
                            .Select(r => new
                            {
                                RoleName = string.Empty,
                                CollegeId = (int?)null
                            }) // prototype of anonymous type
                            .ToList();
            if (CurrentUser != null && CurrentUser.Id != null)
            {
                delegations = db.UserDelegations
                        .Where(x => x.DelegateeId == CurrentUser.Id && x.EffectiveFrom <= DateTime.Now && (x.EffectiveTo == null || x.EffectiveTo >= DateTime.Now))
                        .Where(x => x.Id == db.UserDelegations.Where(x2 => x2.DelegatorId == x.DelegatorId).Max(x2 => x2.Id))
                        .Select(x => new
                        {
                            RoleName = x.Delegator.Roles.FirstOrDefault().Name,
                            CollegeId = x.Delegator.CollegeId
                        })
                        .ToList();
                var extraRoles = db.UserExtraRoles
                    .Where(x => x.UserId == CurrentUser.Id && x.IsActive == true && (x.EffectiveFrom == null || x.EffectiveFrom <= DateTime.Now) && (x.EffectiveTo == null || x.EffectiveTo >= DateTime.Now))
                    .Select(x => new
                    {
                        RoleName = x.Role.Name,
                        CollegeId = x.CollegeId
                    })
                    .ToList();

                delegations.AddRange(extraRoles.Where(r => !delegations.Contains(r)));
                //delegations.Distinct().ToList();
            }

            var listRoles = new HashSet<KeyValuePair<ApsUserRole, int?>>();

            foreach (var d in delegations)
            {
                var role = (ApsUserRole)Enum.Parse(typeof(ApsUserRole), d.RoleName, true);
                if (Utils.IsCollegeRole(role) && !(d.CollegeId > 0)) continue;

                var roleKey = new KeyValuePair<ApsUserRole, int?>(role, Utils.IsCollegeRole(role) && d.CollegeId > 0 ? d.CollegeId.Value : (int?)null);
                if (listRoles.Contains(roleKey)) continue;
                listRoles.Add(roleKey);
            }

            Utils.SetDictVal("DELEGATIONS", listRoles);

            return listRoles;
        }

        public bool CanChangeRoleTo(ApsUserRole role, int? collegeId)
        {
            if (ActualRoles == null || ActualRoles.Length == 0 || ActualRoles[0] == ApsUserRole.None) return false;

            if (ActualRoles[0] == ApsUserRole.SystemAdmin) return true;
            if (ActualRoles[0] == role) return true;
            var roleKey = new KeyValuePair<ApsUserRole, int?>(role, collegeId);
            var delegations = GetDelegations();

            if (delegations.Contains(roleKey))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}