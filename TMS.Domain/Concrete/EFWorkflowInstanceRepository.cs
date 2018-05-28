using TMS.Domain.Abstract;
using System.Collections.Generic;
using System.Linq;
using TMS.Domain.Entities;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFWorkflowInstanceRepository : BaseRepository, IWorkflowInstanceRepository
    {
        public IEnumerable<WorkflowInstance> WorkflowInstances
        {
            get
            {
                return context.WorkflowInstances.Where(w => w.DeleteFlag == false);
            }
        }

        public WorkflowInstance Get(int WorkflowInstanceID)
        {
            return context.WorkflowInstances.Find(WorkflowInstanceID);
        }

        public WorkflowInstance Detail(int WorkflowInstanceID)
        {
            try
            {
                var WorkflowInstance = context.WorkflowInstances.Where(p => p.ID == WorkflowInstanceID).FirstOrDefault();
                if (WorkflowInstance == null) return null;
                WorkflowInstance.CurrentState = context.States.Where(p => p.ID == WorkflowInstance.CurrentStateID).FirstOrDefault();
                WorkflowInstance.Workflow = context.Workflows.Where(p => p.ID == WorkflowInstance.WorkflowID).FirstOrDefault();
                return WorkflowInstance;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(WorkflowInstance workflowInstance)
        {
            bool result = true;
            try
            {
                if (workflowInstance.ID <= 0)
                {
                    context.WorkflowInstances.Add(workflowInstance);
                }
                else
                {
                    context.Entry(workflowInstance).State = System.Data.Entity.EntityState.Modified;
                }

                int changedNo = context.SaveChanges();
                if (changedNo <= 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (DbEntityValidationException e)
            {
                result = false;
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch
            {
                result = false;
            };
            return result;
        }

        public bool Delete(int workflowInstanceID)
        {
            try
            {
                WorkflowInstance dbEntry = context.WorkflowInstances.Find(workflowInstanceID);
                if (dbEntry != null)
                {
                    context.WorkflowInstances.Remove(dbEntry);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
