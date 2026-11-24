using APS.Core;
using APS.Data.DbModels;
using MVCGrid.Models;
using MVCGrid.Web;
using System.Linq;

namespace APS.Web.App_Start.MvcGrids
{
    public class Financial_Report
    {
        public static void RegisterGrids()
        {
            // add your Grid definitions here, using the MVCGridDefinitionTable.Add method

            GridDefaults defaultSet1 = new GridDefaults()
            {
                Paging = true,
                ItemsPerPage = 20,
                Sorting = true,
                NoResultsMessage = "Sorry, no results were found"
            };

            ColumnDefaults colDefaults = new ColumnDefaults()
            {
                EnableSorting = true
            };

            MVCGridDefinitionTable.Add("General Report", new MVCGridBuilder<ViewPublishReport>(colDefaults)
    .WithAuthorizationType(AuthorizationType.AllowAnonymous)
    .AddColumns(cols =>
    {
        // Add your columns here

        cols.Add("Id")
            .WithValueExpression(b => b.Publicationtype.ToString()); // use the Value Expression to return the cell text for this column

        cols.Add("TotalCount")
           .WithValueExpression(b => b.TotalCount.ToString()); // use the Value Expression to return the cell text for this column

        cols.Add("ApprovedCount")
           .WithValueExpression(b => b.ApprovedCount.ToString()); // use the Value Expression to return the cell text for this column

        cols.Add("RejectedCount")
           .WithValueExpression(b => b.RejectedCount.ToString()); // use the Value Expression to return the cell text for this column

        cols.Add("NumberofOrdersThatHavebeenPrinted")
           .WithValueExpression(b => b.NumberofOrdersThatHavebeenPrinted.ToString()); // use the Value Expression to return the cell text for this column
    })
    .WithRetrieveDataMethod((context) =>
    {
        var data = Utils.db.ViewPublishReports.ToList();
        int sum = 0;
        // int sum = 0;
        data.Add(new ViewPublishReport
        {
            //sum+=TotalCount+ApprovedCount+RejectedCount+NumberofOrdersThatHavebeenPrinted;
        });

        // Query your data here. Obey Ordering, paging and filtering paramters given in the context.QueryOptions.
        // Use Entity Framwork, a module from your IoC Container, or any other method.
        // Return QueryResult object containing IEnumerable<YouModelItem>
        return new QueryResult<ViewPublishReport>()
        {
            Items = data,
            TotalRecords = 0 // if paging is enabled, return the total number of records of all pages
        };
    })
);
        }
    }
}