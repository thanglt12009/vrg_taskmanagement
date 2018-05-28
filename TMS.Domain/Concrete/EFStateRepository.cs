using TMS.Domain.Abstract;
using System.Collections.Generic;
using System.Linq;
using TMS.Domain.Entities;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFStateRepository : BaseRepository, IStateRepository
    {
        public IEnumerable<State> States
        {
            get
            {
                return context.States.Where(w => w.DeleteFlag == false);
            }
        }

        public State Get(int StateID)
        {
            return context.States.Find(StateID);
        }

        public State Detail(int StateID)
        {
            try
            {
                var state = context.States.Where(p => p.ID == StateID).FirstOrDefault();
                if (state == null) return null;
                state.Workflow = context.Workflows.Where(p => p.ID == state.WorkflowID && p.DeleteFlag == false).FirstOrDefault();
                state.WorkflowInstances = context.WorkflowInstances.Where(p => p.CurrentStateID == state.ID).ToList();
                state.TransitionFrom = context.Transitions.Where(p => p.ToStateID == state.ID).ToList();
                state.TransitionTo = context.Transitions.Where(p => p.FromStateID == state.ID).ToList();
                state.Actions = context.Actions.Where(p => p.StateID == state.ID).ToList();
                return state;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(State state)
        {
            bool result = true;
            try
            {
                if (state.WorkflowInstances != null)
                {
                    var mList = state.WorkflowInstances.Select(m => m.ID).ToArray();
                    var aList = context.WorkflowInstances.Where(a => mList.Any(s => s == a.ID)).ToList();
                    state.WorkflowInstances.Clear();
                    state.WorkflowInstances = aList;
                }
                if (state.TransitionFrom != null)
                {
                    var mList = state.TransitionFrom.Select(m => m.ID).ToArray();
                    var aList = context.Transitions.Where(a => mList.Any(s => s == a.ID)).ToList();
                    state.TransitionFrom.Clear();
                    state.TransitionFrom = aList;
                }
                if (state.TransitionTo != null)
                {
                    var mList = state.TransitionTo.Select(m => m.ID).ToArray();
                    var aList = context.Transitions.Where(a => mList.Any(s => s == a.ID)).ToList();
                    state.TransitionTo.Clear();
                    state.TransitionTo = aList;
                }
                if (state.Actions != null)
                {
                    var mList = state.Actions.Select(m => m.ID).ToArray();
                    var aList = context.Actions.Where(a => mList.Any(s => s == a.ID)).ToList();
                    state.Actions.Clear();
                    state.Actions = aList;
                }
                if (state.ID <= 0)
                {
                    context.States.Add(state);
                }
                else
                {
                    context.Entry(state).State = System.Data.Entity.EntityState.Modified;
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

        public bool Delete(int stateID)
        {
            try
            {
                State dbEntry = context.States.Find(stateID);
                if (dbEntry != null)
                {
                    context.States.Remove(dbEntry);
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
