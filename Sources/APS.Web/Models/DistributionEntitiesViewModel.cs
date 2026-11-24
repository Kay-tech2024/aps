using System.Collections.Generic;

namespace APS.Web.Models
{
    public class DistributionEntityViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public long? NoOfCopies { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DistributionEntitiesViewModel
    {
        public List<DistributionEntityViewModel> DistributionEntities { get; set; }
    }
}