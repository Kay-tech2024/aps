using System.Collections.Generic;

namespace APS.Web.Models
{
    public class UserRolesViewModel
    {
        public UserViewModel user { set; get; }
        public List<RoleViewModel> UserRoles { get; set; }
        public List<RoleViewModel> AllRoles { get; set; }
    }
}