using TMS.Domain.Abstract;
using System.Collections.Generic;
using System.Linq;
using TMS.Domain.Entities;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFEventRepository : BaseRepository, IEventRepository
    {
        public IEnumerable<Event> Events
        {
            get
            {
                return context.Events.Where(w => w.DeleteFlag == false);
            }
        }

        public Event Get(int EventID)
        {
            return context.Events.Find(EventID);
        }

        public Event Detail(int EventID)
        {
            try
            {
                var Event = context.Events.Where(p => p.ID == EventID).FirstOrDefault();
                if (Event == null) return null;
                Event.Transitions = context.Transitions.Where(p => p.EventID == Event.ID).ToList();
                return Event;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(Event Event)
        {
            bool result = true;
            try
            {
                if (Event.Transitions != null)
                {
                    var mList = Event.Transitions.Select(m => m.ID).ToArray();
                    var aList = context.Transitions.Where(a => mList.Any(s => s == a.ID)).ToList();
                    Event.Transitions.Clear();
                    Event.Transitions = aList;
                }
                if (Event.ID <= 0)
                {
                    context.Events.Add(Event);
                }
                else
                {
                    context.Entry(Event).State = System.Data.Entity.EntityState.Modified;
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

        public bool Delete(int EventID)
        {
            try
            {
                Event dbEntry = context.Events.Find(EventID);
                if (dbEntry != null)
                {
                    context.Events.Remove(dbEntry);
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
