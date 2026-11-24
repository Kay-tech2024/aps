namespace APS.Web.Models
{
    public class DetailedRequestsViewModel
    {
        public System.Int64 Id { get; set; }
        public System.Byte Type { get; set; }
        public System.String Title { get; set; }
        public System.String FirstName { get; set; }
        public System.String SecondName { get; set; }
        public System.String ThirdName { get; set; }
        public System.String LastName { get; set; }
        public System.Int64 Status { get; set; }
        public System.Byte PublicationType { get; set; }
        public System.DateTime Created { get; set; }
        public System.String CreatedBy { get; set; }
        public System.DateTime Updated { get; set; }
        public System.String UpdatedBy { get; set; }
        public System.Int32? IsPrinted { get; set; }
        public double Amount { get; set; }
    }
}