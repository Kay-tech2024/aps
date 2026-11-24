using ECS.Data.DbModels;
using System.Collections.Generic;
using System.Linq;

namespace ECS.Core
{
    public class ClaimsManager : AbstractManager
    {
        public IQueryable<FinancialClaim> GetGeneralClaims(ClaimTab? tab = ClaimTab.Search, string query = null, ClaimFields sortBy = ClaimFields.Date,
            SortDirection sortDirection = SortDirection.Descending, bool includeHistory = false, bool includePending = true)
        {
            if (tab == null) return new List<FinancialClaim>().AsQueryable();

            var financialclaim = new FinancialClaim();

            var predicate = PredicateBuilder.BaseAnd<FinancialClaim>();

            switch (Role)
            {
                case UserRole.TaqatManager:
                case UserRole.TaqatOfficer:
                case UserRole.GlobalAdmin:

                    if (includeHistory && !includePending)
                    {
                        predicate = predicate.And(c => c.Approved != null);
                    }
                    else if (!includeHistory && includePending)
                    {
                        predicate = predicate.And(c => c.Approved == null);
                    }

                    switch (tab)
                    {
                        case ClaimTab.Fixed:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Fixed.ToString().ToUpper());
                            break;

                        case ClaimTab.Setup:
                            predicate = predicate.And(a => a.ClaimType == ClaimType.Setup.ToString().ToUpper());
                            break;

                        default:
                            return new List<FinancialClaim>().AsQueryable();
                    }

                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        query = query.ToLower().Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
                        var predicate2 = PredicateBuilder.BaseOr<FinancialClaim>();

                        predicate2 = predicate2.Or(a => a.Provider != null && (a.Provider.NameEnglish.Contains(query) || a.Provider.NameArabic.Contains(query)));

                        predicate = predicate.And(predicate2);
                    }

                    break;

                case UserRole.EmploymentManager:
                case UserRole.EmploymentConsultant:
                    if (CurrentUser.ProviderId == null) return new List<FinancialClaim>().AsQueryable();

                    predicate = predicate.And(c => c.ProviderId == CurrentUser.ProviderId);

                    if (includeHistory && !includePending)
                    {
                        predicate = predicate.And(c => c.Approved != null);
                    }
                    else if (!includeHistory && includePending)
                    {
                        predicate = predicate.And(c => c.Approved == null);
                    }

                    switch (tab)
                    {
                        case ClaimTab.Fixed:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Fixed.ToString().ToUpper());
                            break;

                        case ClaimTab.Setup:
                            predicate = predicate.And(a => a.ClaimType == ClaimType.Setup.ToString().ToUpper());
                            break;

                        default:
                            return new List<FinancialClaim>().AsQueryable();
                    }

                    break;

                default:
                    return new List<FinancialClaim>().AsQueryable();
            }

            var result = DbContext.FinancialClaims.Where(predicate);

            switch (sortBy)
            {
                case ClaimFields.Id:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.Id) : result.OrderByDescending(c => c.Id);
                    break;
                case ClaimFields.Amount:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.Amount) : result.OrderByDescending(c => c.Amount);
                    break;
                case ClaimFields.Status:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.IsApproved == null ? 0 : (c.IsApproved.Value ? 1 : 2)) : result.OrderByDescending(c => c.IsApproved == null ? 0 : (c.IsApproved.Value ? 1 : 2));
                    break;
                case ClaimFields.Date:
                default:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.Submitted) : result.OrderByDescending(c => c.Submitted);
                    break;
            }

            return result;
        }

        public IQueryable<JobSeekerClaim> GetJobSeekerClaims(ClaimTab? tab = ClaimTab.Search, string query = null, ClaimFields sortBy = ClaimFields.Date,
            SortDirection sortDirection = SortDirection.Descending, bool includeHistory = false, bool includePending = true)
        {
            if (tab == null) return new List<JobSeekerClaim>().AsQueryable();

            var JobSeekerclaim = new JobSeekerClaim();

            var predicate = PredicateBuilder.BaseAnd<JobSeekerClaim>();

            switch (Role)
            {
                case UserRole.TaqatManager:
                case UserRole.TaqatOfficer:
                case UserRole.GlobalAdmin:

                    if (includeHistory && !includePending)
                    {
                        predicate = predicate.And(c => c.Approved != null);
                    }
                    else if (!includeHistory && includePending)
                    {
                        predicate = predicate.And(c => c.Approved == null);
                    }

                    switch (tab)
                    {
                        case ClaimTab.Attachment:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Attachment.ToString().ToUpper());
                            break;

                        case ClaimTab.Placement:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Placement.ToString().ToUpper());
                            break;

                        case ClaimTab.Sustainability:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Sustainability.ToString().ToUpper());
                            break;

                        case ClaimTab.Coaching:
                            predicate = predicate.And(a => a.ClaimType == ClaimType.Coaching.ToString().ToUpper());
                            break;

                        default:
                            return new List<JobSeekerClaim>().AsQueryable();
                    }

                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        query = query.ToLower().Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
                        var predicate2 = PredicateBuilder.BaseOr<JobSeekerClaim>();

                        predicate2 = predicate2.Or(a => a.JobSeeker.FirstName.Contains(query) || a.JobSeeker.LastName.Contains(query) || (a.JobSeeker.FirstName + " " + a.JobSeeker.LastName).Contains(query) || (a.JobSeeker.FirstName + " " + a.JobSeeker.MiddleName + " " + a.JobSeeker.LastName).Contains(query) || (a.JobSeeker.FirstName + " " + a.JobSeeker.MiddleName + " " + a.JobSeeker.ThirdName + " " + a.JobSeeker.LastName).Contains(query));
                        predicate2 = predicate2.Or(a => a.JobSeeker.NIN.Contains(query));
                        predicate2 = predicate2.Or(a => a.JobSeeker.Region != null && (a.JobSeeker.Region.NameEnglish.Contains(query) || a.JobSeeker.Region.NameArabic.Contains(query)));
                        predicate2 = predicate2.Or(a => a.Provider != null && (a.Provider.NameEnglish.Contains(query) || a.Provider.NameArabic.Contains(query)));

                        predicate = predicate.And(predicate2);
                    }

                    break;

                case UserRole.EmploymentManager:
                case UserRole.EmploymentConsultant:
                    if (CurrentUser.ProviderId == null) return new List<JobSeekerClaim>().AsQueryable();

                    predicate = predicate.And(c => c.ProviderId == CurrentUser.ProviderId);

                    if (includeHistory && !includePending)
                    {
                        predicate = predicate.And(c => c.Approved != null);
                    }
                    else if (!includeHistory && includePending)
                    {
                        predicate = predicate.And(c => c.Approved == null);
                    }

                    switch (tab)
                    {
                        case ClaimTab.Attachment:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Attachment.ToString().ToUpper());
                            break;

                        case ClaimTab.Placement:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Placement.ToString().ToUpper());
                            break;

                        case ClaimTab.Sustainability:
                            predicate = predicate.And(c => c.ClaimType == ClaimType.Sustainability.ToString().ToUpper());
                            break;

                        case ClaimTab.Coaching:
                            predicate = predicate.And(a => a.ClaimType == ClaimType.Coaching.ToString().ToUpper());
                            break;

                        default:
                            return new List<JobSeekerClaim>().AsQueryable();
                    }

                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        query = query.ToLower().Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Trim();
                        var predicate2 = PredicateBuilder.BaseOr<JobSeekerClaim>();

                        predicate2 = predicate2.Or(a => a.JobSeeker.FirstName.Contains(query) || a.JobSeeker.LastName.Contains(query) || (a.JobSeeker.FirstName + " " + a.JobSeeker.LastName).Contains(query) || (a.JobSeeker.FirstName + " " + a.JobSeeker.MiddleName + " " + a.JobSeeker.LastName).Contains(query) || (a.JobSeeker.FirstName + " " + a.JobSeeker.MiddleName + " " + a.JobSeeker.ThirdName + " " + a.JobSeeker.LastName).Contains(query));
                        predicate2 = predicate2.Or(a => a.JobSeeker.NIN.Contains(query));
                        predicate2 = predicate2.Or(a => a.JobSeeker.Region != null && (a.JobSeeker.Region.NameEnglish.Contains(query) || a.JobSeeker.Region.NameArabic.Contains(query)));

                        predicate = predicate.And(predicate2);
                    }

                    break;

                default:
                    return new List<JobSeekerClaim>().AsQueryable();
            }

            var result = DbContext.JobSeekerClaims.Where(predicate);

            switch(sortBy)
            {
                case ClaimFields.Id:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.Id) : result.OrderByDescending(c => c.Id);
                    break;
                case ClaimFields.Amount:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.Amount) : result.OrderByDescending(c => c.Amount);
                    break;
                case ClaimFields.JobSeekerNIN:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.JobSeeker.NIN) : result.OrderByDescending(c => c.JobSeeker.NIN);
                    break;
                case ClaimFields.JobSeekerName:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.JobSeeker.FirstName + c.JobSeeker.LastName) : result.OrderByDescending(c => c.JobSeeker.FirstName + c.JobSeeker.LastName);
                    break;
                case ClaimFields.Status:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.IsApproved == null ? 0 : (c.IsApproved.Value ? 1 : 2)) : result.OrderByDescending(c => c.IsApproved == null ? 0 : (c.IsApproved.Value ? 1 : 2));
                    break;
                case ClaimFields.Date:
                default:
                    result = sortDirection == SortDirection.Ascending ? result.OrderBy(c => c.Submitted) : result.OrderByDescending(c => c.Submitted);
                    break;
            }

            return result;
        }
    }
}