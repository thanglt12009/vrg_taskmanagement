using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMS.Domain.Entities
{
    public class Workflow
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<WorkflowInstance> WorkflowInstances { get; set; }

        public virtual ICollection<State> States { get; set; }
        public virtual ICollection<Board> Boards { get; set; }
        public bool DeleteFlag { get; set; }
    }
}
