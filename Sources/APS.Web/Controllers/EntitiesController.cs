using APS.Core;
using APS.Data;
using APS.Data.DbModels;
using APS.Web.App_Start.Code;
using APS.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace APS.Web.Controllers
{
    [CustomAuthorize(ApsUserRole.DeanOfAcademicResearch, ApsUserRole.OfficerOfPrintingUnit, ApsUserRole.SystemAdmin, ApsUserRole.ViceDeanOfAcademicResearch)]
    public class EntitiesController : BaseController
    {
        // GET: Entities
        public ActionResult Index()
        {
            var model = new DistributionEntitiesViewModel();
            model.DistributionEntities = DbContext.DistributionEntities
                                            .Where(x => x.IsDeleted != true)
                                            .Select(d => new DistributionEntityViewModel
                                            {
                                                Id = d.Id,
                                                Name = d.Name,
                                                NoOfCopies = d.NoOfCopies
                                            })
                                            .ToList();
            return View(model);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(DistributionEntitiesViewModel model)
        {
            var entityIds = new List<long>();
            entityIds = DbContext.DistributionEntities.Where(x => x.IsDeleted == false)
                            .Select(x => x.Id).ToList<long>();
            try
            {
                foreach (var m in model.DistributionEntities)
                {
                    var entity = new DistributionEntity();
                    if (m.Id == 0)
                    {
                        entity.Name = m.Name;
                        entity.NoOfCopies = (long)m.NoOfCopies;
                        entity.CreatedBy = CurrentUser.Id;
                        entity.Created = DateTime.Now;
                        entity.UpdatedBy = CurrentUser.Id;
                        entity.Updated = DateTime.Now;
                        DbContext.DistributionEntities.Add(entity);
                        DbContext.SaveChanges();
                        m.Id = entity.Id;
                    }
                    //else if (entityIds.IndexOf(m.Id) == -1)
                    //{
                    //    entity.IsDeleted = true;
                    //    DbContext.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                    //    DbContext.SaveChanges();
                    //}
                    else
                    {
                        entity = DbContext.DistributionEntities.Where(x => x.Id == m.Id).SingleOrDefault();
                        if (entity != null)
                        {
                            entity.Name = m.Name;
                            entity.NoOfCopies = (long)m.NoOfCopies;
                            entity.UpdatedBy = CurrentUser.Id;
                            entity.Updated = DateTime.Now;
                            DbContext.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                            DbContext.SaveChanges();
                        }
                        entityIds.Remove(m.Id);
                    }
                }
                foreach (var i in entityIds)
                {
                    var entity = new DistributionEntity();
                    entity = DbContext.DistributionEntities.Where(x => x.Id == i).SingleOrDefault();
                    entity.IsDeleted = true;
                    entity.UpdatedBy = CurrentUser.Id;
                    entity.Updated = DateTime.Now;
                    DbContext.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                    DbContext.SaveChanges();
                }

                TempData["message"] = "Entities are successfully added.";
                TempData["message_type"] = "success";
            }
            catch (Exception ex)
            {
                TempData["message"] = "Some of the records are failed to be added or modified.";
                TempData["message_type"] = "danger";
            }

            //return View(model);
            return RedirectToAction("Index");
        }

        [AjaxOnly]
        public dynamic AjaxDeleteEntity(int? id)
        {
            var ids = Guid.Empty;

            if (id == null || id <= 0)
                return new { success = false, msg = "INVALID_ID" };

            try
            {
                var entity = DbContext.DistributionEntities.SingleOrDefault(u => u.Id == id);

                if (entity == null)
                {
                    return new { success = false, msg = "NOT_FOUND" };
                }

                entity.IsDeleted = true;
                DbContext.Entry(entity).State = System.Data.Entity.EntityState.Modified;
                DbContext.SaveChanges();
                return new { success = true, msg = "DELETED" };
            }
            catch (Exception ex)
            {
                Logger.LogExceptionAsync(ex, id);
                return new { success = false, msg = ex.SanitizeException() };
            }
        }
    }
}