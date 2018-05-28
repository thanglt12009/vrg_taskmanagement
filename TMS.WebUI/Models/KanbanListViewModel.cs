using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Domain.Entities;

namespace TMS.WebApp.Models
{
    public class KanbanListViewModel
    {
        public IList<KanbanInfoModel> BoardList { get; set; }
        public IList<Workflow> WorkflowList { get; set; }
    }
}