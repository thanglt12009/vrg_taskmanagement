using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMS.Domain.Entities
{
    public class State
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }

        public virtual Workflow Workflow { get; set; }
        public int? WorkflowID { get; set; }

        public virtual ICollection<WorkflowInstance> WorkflowInstances { get; set; }
        public virtual ICollection<Transition> TransitionFrom { get; set; }
        public virtual ICollection<Transition> TransitionTo { get; set; }
        public virtual ICollection<Action> Actions { get; set; }
        public bool DeleteFlag { get; set; }

        public Int16 Type { get; set; }
    }
}
