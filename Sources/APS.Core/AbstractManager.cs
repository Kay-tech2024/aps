using Microsoft.AspNet.Identity; // Maybe this one too
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using ECS.Auth;
using ECS.Data;
using ECS.Data.DbModels;

namespace ECS.Core
{
    public class AbstractManager
    {
        protected ApplicationUser _user;
        protected UserRole? _role;
        protected EcsEntities _db;
        protected ApplicationDbContext _authDbContext;
        protected ApplicationSignInManager _signInManager;
        protected ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public EcsEntities DbContext
        {
            get
            {

                if (_db != null) return _db;

                _db = HttpContext.Current.GetOwinContext().Get<EcsEntities>(typeof(EcsEntities).FullName);
                if (_db == null)
                {
                    _db = new EcsEntities();
                    HttpContext.Current.GetOwinContext().Set<EcsEntities>(typeof(EcsEntities).FullName, _db);
                }

                return _db;
            }
        }

        public ApplicationUser CurrentUser
        {
            get
            {
                if (_user == null)
                {
                    if (HttpContext.Current.User == null || HttpContext.Current.User.Identity == null) return null;
                    _user = UserManager.FindById(HttpContext.Current.User.Identity.GetUserId());
                }

                return _user;
            }
        }

        public UserRole Role
        {
            get
            {
                if (_role != null) return _role.Value;

                var roles = UserManager.GetRoles(CurrentUser.Id);
                if (roles.Contains("Global Admin"))
                {
                    _role = UserRole.GlobalAdmin;
                }
                else if (roles.Contains("Taqat Manager"))
                {
                    _role = UserRole.TaqatManager;
                }
                else if (roles.Contains("Taqat Officer"))
                {
                    _role = UserRole.TaqatOfficer;
                }
                else if (roles.Contains("Employment Manager"))
                {
                    _role = UserRole.EmploymentManager;
                }
                else if (roles.Contains("Employment Consultant"))
                {
                    _role = UserRole.EmploymentConsultant;
                }
                else if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    _role = UserRole.NormalUser;

                }
                else
                {
                    _role = UserRole.None;
                }
                return _role.Value;
            }
        }

        public static EcsEntities GetDatabaseContext()
        {
            var _db = HttpContext.Current.GetOwinContext().Get<EcsEntities>(typeof(EcsEntities).FullName);
            if (_db == null)
            {
                _db = new EcsEntities();
                HttpContext.Current.GetOwinContext().Set<EcsEntities>(typeof(EcsEntities).FullName, _db);
            }

            return _db;

        }
    }
}