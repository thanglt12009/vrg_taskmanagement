using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface IEventRepository : IBaseRepository
    {
        IEnumerable<Event> Events { get; }
        Event Get(int EventID);
        Event Detail(int EventID);
        bool Save(Event Event);
        bool Delete(int EventID);
    }
}
