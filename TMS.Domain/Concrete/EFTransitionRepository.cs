using TMS.Domain.Abstract;
using System.Collections.Generic;
using System.Linq;
using TMS.Domain.Entities;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFTransitionRepository : BaseRepository, ITransitionRepository
    {
        public IEnumerable<Transition> Transitions
        {
            get
            {
                return context.Transitions.Where(w => w.DeleteFlag == false);
            }
        }

        public Transition Get(int TransitionID)
        {
            return context.Transitions.Find(TransitionID);
        }

        public Transition Detail(int TransitionID)
        {
            try
            {
                var transition = context.Transitions.Where(p => p.ID == TransitionID).FirstOrDefault();
                if (transition == null) return null;
                transition.FromState = context.States.Where(p => p.ID == transition.FromStateID).FirstOrDefault();
                transition.ToState = context.States.Where(p => p.ID == transition.ToStateID).FirstOrDefault();
                transition.Event = context.Events.Where(p => p.ID == transition.EventID).FirstOrDefault();
                return transition;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(Transition transition)
        {
            bool result = true;
            try
            {
                if (transition.ID <= 0)
                {
                    context.Transitions.Add(transition);
                }
                else
                {
                    context.Entry(transition).State = System.Data.Entity.EntityState.Modified;
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

        public bool Delete(int transitionID)
        {
            try
            {
                Transition dbEntry = context.Transitions.Find(transitionID);
                if (dbEntry != null)
                {
                    context.Transitions.Remove(dbEntry);
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
