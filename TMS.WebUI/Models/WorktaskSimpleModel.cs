using System;
using TMS.Domain.Entities;
using TMS.Domain.Common;

namespace TMS.WebApp.Models
{
    public class WorktaskSimpleModel
    {
        public int WorktaskID { get; set; }

        public string Title { get; set; }

        public int Status { get; set; }

        public string Description { get; set; }

        public string Identify { get; set; }

        public Account Assignee { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int Priority { get; set; }

        public int TaskType { get; set; }

    }
}