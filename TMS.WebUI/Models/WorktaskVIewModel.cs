using TMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.WebApp.Models
{
    public class WorktaskViewModel : Worktask
    {
        public string[] RelatedTaskValue { get; set; }
    }

    public class MoveTaskViewModel
    {
        public int BoardID { get; set; }
        public int StateID { get; set; }

        public int WorktaskID { get; set; }
    }
}