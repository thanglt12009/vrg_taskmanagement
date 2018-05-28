using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TMS.Domain.Entities;

namespace TMS.WebApp.Models
{
    public class DashboardViewModel
    {
        public int WorkflowID { get; set; }
        public List<TaskGroupByStatusModel> TaskGroupByStatusModel { get; set; }

        public List<State> StateList { get; set; }
    }

    public class TaskGroupByStatusModel
    {
        public int State { get; set; }
        public int Count { get; set; }
    }

    public class PivotRecordModel
    {
        public string UserName { get; set; }
        public ArrayList Data { get; set; }
    }
}