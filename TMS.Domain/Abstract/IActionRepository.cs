using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface IActionRepository : IBaseRepository
    {
        IEnumerable<Action> Actions { get; }
        Action Get(int ActionID);
        Action Detail(int ActionID);
        bool Save(Action action);
        bool Delete(int ActionID);
    }
}
