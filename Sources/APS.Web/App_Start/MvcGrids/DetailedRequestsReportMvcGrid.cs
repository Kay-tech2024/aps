using APS.Core;
using APS.Data;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using MVCGrid.Models;
using MVCGrid.Web;
using System.Linq;

namespace APS.Web.App_Start.MvcGrids
{
    public class DetailedRequestsReportMvcGrid
    {
        public static void RegisterGrids()
        {
            GridDefaults defaultSet1 = new GridDefaults()
            {
                Paging = true,
                ItemsPerPage = 20,
                Sorting = true,
                NoResultsMessage = ApsStrings.no_results
            };

            ColumnDefaults colDefaults = new ColumnDefaults()
            {
                EnableSorting = true
            };

            MVCGridDefinitionTable.Add("DetailedRequestsReportMvcGridArabic", new MVCGridBuilder<DetailedRequestsViewModel>(colDefaults)
                    .WithAuthorizationType(AuthorizationType.AllowAnonymous)
                    .AddColumns(cols =>
                    {
                        cols.Add("Id")
                            .WithColumnName("الرقم")
                            .WithValueExpression(b => b.Id.ToString());

                        cols.Add("Title")
                            .WithColumnName("العنوان")
                            .WithValueExpression(b => b.Title);

                        cols.Add("Name")
                           .WithColumnName("الاسم")
                           .WithValueExpression(b => b.FirstName + " " + b.LastName);

                        cols.Add("PublicationType")
                          .WithColumnName("نوع النشر")
                          .WithValueExpression(b => LangHelpers.Instance[((PublicationType)b.PublicationType).ToString()]);

                        cols.Add("Status")
                           .WithValueExpression(b => LangHelpers.Instance[((RequestStatus)b.Status).ToString()]);
                    })
                    .WithProcessingMessage("فضلًا انتظر..")
                    .WithNextButtonCaption("التالي").WithNoResultsMessage("لا توجد نتائج")
                    .WithPreviousButtonCaption("السابق")
                    .WithSummaryMessage("عرض  {0} الى {1} من {2} مدخلات")
                    .WithNoResultsMessage("لا توجد نتائج")
                    .WithPaging(true, itemsPerPage: 20, allowChangePageSize: true, maxItemsPerPage: 100)
                    .WithSorting(true, "Id")
                    .WithAdditionalQueryOptionNames("Search")
                    .WithAdditionalSetting("RenderLoadingDiv", false)
                    .WithRetrieveDataMethod((context) =>
                    {
                        var options = context.QueryOptions;
                        string globalSearch = options.GetAdditionalQueryOptionString("Search");

                        var typeNo = (short)0;
                        var statusNo = (short)0;
                        var reportNo = (short)0;

                        if (context.CurrentHttpContext.Request["ReportNo"] != null && short.TryParse(context.CurrentHttpContext.Request["ReportNo"], out reportNo))
                        {
                        }

                        if (context.CurrentHttpContext.Request["Status"] != null && short.TryParse(context.CurrentHttpContext.Request["Status"], out statusNo))
                        {
                        }

                        if (context.CurrentHttpContext.Request["Type"] != null && short.TryParse(context.CurrentHttpContext.Request["Type"], out typeNo))
                        {
                        }

                        if (reportNo != 1 && reportNo != 2)
                        {
                            reportNo = 1;
                        }

                        var query = (IQueryable<DetailedRequestsViewModel>)null;

                        if (reportNo == 1)
                        {
                            query = Utils.db.FuncRequestsByType(typeNo, statusNo)
                                .AsQueryable()
                                .Select(c => new DetailedRequestsViewModel
                                {
                                    Id = c.Id,
                                    Created = c.Created,
                                    Updated = c.Updated,
                                    CreatedBy = c.CreatedBy,
                                    UpdatedBy = c.UpdatedBy,
                                    FirstName = c.FirstName,
                                    IsPrinted = c.IsPrinted,
                                    LastName = c.LastName,
                                    PublicationType = c.PublicationType,
                                    SecondName = c.SecondName,
                                    Status = c.Status,
                                    ThirdName = c.ThirdName,
                                    Title = c.Title,
                                    Type = c.Type
                                });
                        }
                        else
                        {
                            query = Utils.db.FuncRequestsByFinancialTypes(typeNo, statusNo)
                              .AsQueryable()
                              .Select(c => new DetailedRequestsViewModel
                              {
                                  Id = c.Id,
                                  Created = c.Created,
                                  Updated = c.Updated,
                                  CreatedBy = c.CreatedBy,
                                  UpdatedBy = c.UpdatedBy,
                                  FirstName = c.FirstName,
                                  Amount = c.Amount == null ? 0 : (double)c.Amount.Value,
                                  LastName = c.LastName,
                                  PublicationType = c.PublicationType,
                                  SecondName = c.SecondName,
                                  Status = c.Status,
                                  ThirdName = c.ThirdName,
                                  Title = c.Title,
                                  Type = c.Type
                              });
                        }

                        if (options.SortColumnName != null)
                        {
                            switch (options.SortColumnName)
                            {
                                case "Id":
                                    query = options.SortDirection == MVCGrid.Models.SortDirection.Asc ? query.OrderBy(b => b.Id) : query.OrderByDescending(b => b.Id);
                                    break;

                                case "Name":
                                    query = options.SortDirection == MVCGrid.Models.SortDirection.Asc ? query.OrderBy(b => b.FirstName).ThenBy(b => b.LastName) : query.OrderByDescending(b => b.LastName).ThenByDescending(b => b.FirstName);
                                    break;

                                case "Title":
                                    query = options.SortDirection == MVCGrid.Models.SortDirection.Asc ? query.OrderBy(b => b.Title) : query.OrderByDescending(b => b.Title);
                                    break;

                                case "PublicationType":
                                    query = options.SortDirection == MVCGrid.Models.SortDirection.Asc ? query.OrderBy(b => b.PublicationType) : query.OrderByDescending(b => b.PublicationType);
                                    break;

                                case "Status":
                                    query = options.SortDirection == MVCGrid.Models.SortDirection.Asc ? query.OrderBy(b => b.Status) : query.OrderByDescending(b => b.Status);
                                    break;

                                case "Updated":
                                default:
                                    query = options.SortDirection == MVCGrid.Models.SortDirection.Asc ? query.OrderBy(b => b.Updated) : query.OrderByDescending(b => b.Updated);
                                    break;
                            }
                        }
                        else
                        {
                            query = query.OrderByDescending(c => c.Updated);
                        }

                        if (options.ItemsPerPage == null || options.ItemsPerPage < 1)
                        {
                            options.ItemsPerPage = 20;
                        }

                        if (options.PageIndex == null || options.PageIndex < 0)
                        {
                            options.PageIndex = 0;
                        }

                        var total = query.Count();
                        var data = query.Skip(options.PageIndex.Value * options.ItemsPerPage.Value).Take(options.ItemsPerPage.Value);

                        // Query your data here. Obey Ordering, paging and filtering paramters given in the context.QueryOptions.
                        // Use Entity Framwork, a module from your IoC Container, or any other method.
                        // Return QueryResult object containing IEnumerable<YouModelItem>
                        return new QueryResult<DetailedRequestsViewModel>()
                        {
                            Items = data,
                            TotalRecords = total
                        };
                    })
            );
        }
    }
}