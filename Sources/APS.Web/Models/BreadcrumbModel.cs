namespace APS.Web.Models
{
    public class BreadcrumbModel
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public BreadcrumbModel Next { get; set; }
    }
}