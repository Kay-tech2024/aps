using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace APS.Auth
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private bool _isDisposed = false;

        public bool IsDisposed { get { return _isDisposed; } }

        public ApplicationDbContext()
            : base("name=ApsDB", throwIfV1Schema: true)
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole>().ToTable("UserRoles");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("UserLogins");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("UserClaims");
        }

        protected override void Dispose(bool disposing)
        {
            _isDisposed = true;
            base.Dispose(disposing);
        }
    }
}