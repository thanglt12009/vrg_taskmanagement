using TMS.Domain.Abstract;
using System.Collections.Generic;
using System.Linq;
using TMS.Domain.Entities;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFActionRepository : BaseRepository, IActionRepository
    {
        public IEnumerable<Action> Actions
        {
            get
            {
                return context.Actions.Where(w => w.DeleteFlag == false);
            }
        }

        public Action Get(int ActionID)
        {
            return context.Actions.Find(ActionID);
        }

        public Action Detail(int ActionID)
        {
            try
            {
                var action = context.Actions.Where(p => p.ID == ActionID).FirstOrDefault();
                if (action == null) return null;
                action.State = context.States.Where(p => p.ID == action.StateID).FirstOrDefault();
                return action;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(Action action)
        {
            bool result = true;
            try
            {
                if (action.ID <= 0)
                {
                    context.Actions.Add(action);
                }
                else
                {
                    context.Entry(action).State = System.Data.Entity.EntityState.Modified;
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

        public bool Delete(int actionID)
        {
            try
            {
                Action dbEntry = context.Actions.Find(actionID);
                if (dbEntry != null)
                {
                    context.Actions.Remove(dbEntry);
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
