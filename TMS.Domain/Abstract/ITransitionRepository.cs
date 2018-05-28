using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface ITransitionRepository : IBaseRepository
    {
        IEnumerable<Transition> Transitions { get; }
        Transition Get(int TransitionID);
        Transition Detail(int TransitionID);
        bool Save(Transition transition);
        bool Delete(int TransitionID);
    }
}
