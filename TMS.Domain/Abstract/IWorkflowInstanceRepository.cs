using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface IWorkflowInstanceRepository : IBaseRepository
    {
        IEnumerable<WorkflowInstance> WorkflowInstances { get; }
        WorkflowInstance Get(int AWorkflowInstanceID);
        WorkflowInstance Detail(int WorkflowInstanceID);
        bool Save(WorkflowInstance workflowInstance);
        bool Delete(int WorkflowInstanceID);
    }
}
