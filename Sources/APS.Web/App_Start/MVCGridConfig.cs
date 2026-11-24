namespace APS.Web
{
    using APS.Web.App_Start.MvcGrids;

    public static class MVCGridConfig
    {
        public static void RegisterGrids()
        {
            DetailedRequestsReportMvcGrid.RegisterGrids();
            General_Report_PublishingSystem.RegisterGrids();
        }
    }
}