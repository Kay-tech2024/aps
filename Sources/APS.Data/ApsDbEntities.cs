using APS.Data.DbModels;

namespace APS.Data
{
    public class ApsDbEntities : ApsDbEntitiesPrivate
    {
        private bool _isDisposed = false;

        public bool IsDisposed { get { return _isDisposed; } }

        public ApsDbEntities() : base("ApsDb")
        {
            System.Data.Entity.Database.SetInitializer<ApsDbEntities>(null);

            Configuration.LazyLoadingEnabled = true;
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.ValidateOnSaveEnabled = false;

            Database.CommandTimeout = 0;
        }

        protected override void Dispose(bool disposing)
        {
            _isDisposed = true;
            base.Dispose(disposing);
        }
    }
}