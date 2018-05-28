using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface IStateRepository : IBaseRepository
    {
        IEnumerable<State> States { get; }
        State Get(int StateID);
        State Detail(int StateID);
        bool Save(State state);
        bool Delete(int StateID);
    }
}
