using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface IWorkflowRepository : IBaseRepository
    {
        IEnumerable<Workflow> Workflows { get; }
        Workflow Get(int WorkflowID);
        Workflow Detail(int WorkflowID);
        bool Save(Workflow workflow);
        bool Delete(int WorkflowID);
    }
}
