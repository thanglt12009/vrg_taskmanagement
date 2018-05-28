using Nest;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using TMS.Domain.Common;

namespace TMS.Domain.Entities
{
    public class WorkflowInstance
    {
        [Key]
        public int ID { get; set; }

        public virtual Workflow Workflow { get; set; }
        public int? WorkflowID { get; set; }

        public virtual ICollection<Worktask> Tasks { get; set; }

        public virtual State CurrentState { get; set; }
        public int? CurrentStateID { get; set; }
        public bool DeleteFlag { get; set; }
    }
}
