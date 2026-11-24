using APS.Core;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using APS.Web.Resources;
using CrystalDecisions.CrystalReports.Engine;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(ApsUserRole.DeanOfAcademicResearch, ApsUserRole.ViceDeanOfAcademicResearch)]
    public class ReportsController : BaseController
    {
        public ActionResult Types()
        {
            var model = DbContext.Database.SqlQuery<ViewRequestsByTypeReport>("select * from ViewRequestsByTypeReport")
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

            ViewBag.Breadcrumb = new BreadcrumbModel
            {
                Name = ApsStrings.reports,
                Link = Utils.GetApplicationUrl() + "Reports/",
                Next = new BreadcrumbModel
                {
                    Name = ApsStrings.requests_report,
                }
            };
            return View(model);
        }

        public ActionResult Financial()
        {
            var model = DbContext.Database.SqlQuery<ViewRequestsByFinancialTypesReport>("select * from ViewRequestsByFinancialTypesReport")
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

            ViewBag.Breadcrumb = new BreadcrumbModel
            {
                Name = ApsStrings.reports,
                Link = Utils.GetApplicationUrl() + "Reports/",
                Next = new BreadcrumbModel
                {
                    Name = ApsStrings.financial_report,
                }
            };
            return View(model);
        }

        public ActionResult List(int ReportNo, short? Status, short? Type)
        {
            if (Type == null || Type < 0) Type = 0;
            if (Status == null || Status < 0) Status = 0;

            var type = ReportNo == 2 ? LangHelpers.Instance[((Report1UserRole)Type).ToString()] : LangHelpers.Instance[((PublicationType)Type).ToString()];

            if (type == "Unspecificed" || type == "unspecified")
            {
                type = "All";
            }
            else if (type == "غير محدد")
            {
                type = "الإجمالي";
            }

            if (ReportNo == 1)
            {
                ViewBag.Heading = ApsStrings.requests_report + " (" + type + ")";
                ViewBag.Breadcrumb = new BreadcrumbModel
                {
                    Name = ApsStrings.reports,
                    Link = Utils.GetApplicationUrl() + "Reports/",
                    Next = new BreadcrumbModel
                    {
                        Name = ApsStrings.requests_report,
                        Link = Utils.GetApplicationUrl() + "Reports/Types/",
                        Next = new BreadcrumbModel
                        {
                            Name = ApsStrings.list,
                        }
                    }
                };
            }
            else if (ReportNo == 2)
            {
                ViewBag.Heading = ApsStrings.financial_report + " (" + type + ")";
                ViewBag.Breadcrumb = new BreadcrumbModel
                {
                    Name = ApsStrings.reports,
                    Link = Utils.GetApplicationUrl() + "Reports/",
                    Next = new BreadcrumbModel
                    {
                        Name = ApsStrings.financial_report,
                        Link = Utils.GetApplicationUrl() + "Reports/Financial/",
                        Next = new BreadcrumbModel
                        {
                            Name = ApsStrings.list,
                        }
                    }
                };
            }

            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReportAsPdf(string Type, int ReportNo = 0, int Status = 0, int rType = 0)
        {
            var reportName = "Report";
            ReportDocument rd = new ReportDocument();
            DataSet data = new DataSet("NewDataSet");
            var table = new DataTable();

            switch (Type)
            {
                case "types":
                    var typesDb = DbContext.Database.SqlQuery<ViewRequestsByTypeReport>("select * from ViewRequestsByTypeReport")
                                        .ToList()
                                        .Select(c => new ViewRequestsByTypeReport
                                        {
                                            PublicationTypeId = c.PublicationTypeId,
                                            //PublicationType = LangHelpers.Instance[c.PublicationType],
                                            PublicationTypeAr = c.PublicationTypeAr,
                                            ApprovedRequests = c.ApprovedRequests == null ? 0 : c.ApprovedRequests,
                                            PrintedRequests = c.PrintedRequests == null ? 0 : c.PrintedRequests,
                                            RejectedRequests = c.RejectedRequests == null ? 0 : c.RejectedRequests,
                                            SubmittedRequests = c.SubmittedRequests == null ? 0 : c.SubmittedRequests
                                        }).ToList();

                    table = Utils.ToDataTable(typesDb);
                    reportName = "RequestsByTypeReport";

                    rd.Load(Path.Combine(Server.MapPath("~/CRReports"), "RequestsByTypeReportNew.rpt"));
                    rd.SetDataSource(table);
                    break;

                case "financial":
                    var financialDb = DbContext.Database.SqlQuery<ViewRequestsByFinancialTypesReport>("select * from ViewRequestsByFinancialTypesReport")
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
                    data.Tables.Add(Utils.ToDataTable(financialDb));
                    reportName = "RequestsByFinancialTypesReport";
                    rd.Load(Path.Combine(Server.MapPath("~/CRReports"), "RequestsByFinancialTypesReportNew.rpt"));
                    break;

                case "detailed":
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

                        table = Utils.ToDataTable(documents);
                        reportName = "RequestsByTypeDetailedReport";
                        rd.Load(Path.Combine(Server.MapPath("~/CRReports"), "RequestsByTypeDetailedReport.rpt"));
                        rd.SetParameterValue("@PublicationType", (short)rType);
                        rd.SetParameterValue("@RequestStatus", (short)Status);
                        rd.SetDataSource(table);
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
                        data.Tables.Add(Utils.ToDataTable(documents));
                        reportName = "RequestsByFinancialTypesDetailedReport";
                        rd.Load(Path.Combine(Server.MapPath("~/CRReports"), "RequestsByFinancialTypesDetailedReport.rpt"));
                        rd.SetParameterValue("@UserRole", (short)rType);
                        rd.SetParameterValue("@RequestStatus", (short)Status);
                        rd.SetDataSource(table);
                    }
                    break;
            }
            //rd.SetDataSource(data);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf", reportName + ".pdf");
        }
    }
}