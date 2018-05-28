using TMS.Domain.Abstract;
using System.Collections.Generic;
using System.Linq;
using TMS.Domain.Entities;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFWorkflowRepository : BaseRepository, IWorkflowRepository
    {
        public IEnumerable<Workflow> Workflows
        {
            get
            {
                return context.Workflows.Where(w => w.DeleteFlag == false);
            }
        }

        public Workflow Get(int WorkflowID)
        {
            return context.Workflows.Find(WorkflowID);
        }

        public Workflow Detail(int WorkflowID)
        {
            try
            {
                var workflow = context.Workflows.Where(p => p.ID == WorkflowID).FirstOrDefault();
                if (workflow == null) return null;
                workflow.WorkflowInstances = context.WorkflowInstances.Where(p => p.WorkflowID == workflow.ID).ToList();
                workflow.States = context.States.Where(p => p.WorkflowID == workflow.ID).ToList();
                workflow.Boards = context.Boards.Where(p => p.WorkflowID == workflow.ID).ToList();
                return workflow;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(Workflow workflow)
        {
            bool result = true;
            try
            {
                if (workflow.WorkflowInstances != null)
                {
                    var mList = workflow.WorkflowInstances.Select(m => m.ID).ToArray();
                    var aList = context.WorkflowInstances.Where(a => mList.Any(s => s == a.ID)).ToList();
                    workflow.WorkflowInstances.Clear();
                    workflow.WorkflowInstances = aList;
                }
                if (workflow.States != null)
                {
                    var mList = workflow.States.Select(m => m.ID).ToArray();
                    var aList = context.States.Where(a => mList.Any(s => s == a.ID)).ToList();
                    workflow.States.Clear();
                    workflow.States = aList;
                }
                if (workflow.Boards != null)
                {
                    var mList = workflow.Boards.Select(m => m.ID).ToArray();
                    var aList = context.Boards.Where(a => mList.Any(s => s == a.ID)).ToList();
                    workflow.Boards.Clear();
                    workflow.Boards = aList;
                }
                if (workflow.ID <= 0)
                {
                    context.Workflows.Add(workflow);
                }
                else
                {
                    context.Entry(workflow).State = System.Data.Entity.EntityState.Modified;
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

        public bool Delete(int WorkflowID)
        {
            try
            {
                Workflow dbEntry = context.Workflows.Find(WorkflowID);
                if (dbEntry != null)
                {
                    context.Workflows.Remove(dbEntry);
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
