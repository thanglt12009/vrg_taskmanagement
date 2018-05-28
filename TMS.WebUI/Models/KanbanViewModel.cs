using System.Collections.Generic;
using TMS.Domain.Entities;
using TMS.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System;

namespace TMS.WebApp.Models
{
    public class KanbanViewModel
    {
        public KanbanInfoModel Board { get; set; }
        public IDictionary<int, IList<WorktaskSimpleModel>> TaskLists { get; set; }

        public string AsigneeName { get; set; }

        public int? TaskType { get; set; }

        public int? TaskPriority { get; set; }
        public string BoardCode { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/M/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PlanStartMin { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PlanStartMax { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PlanEndMin { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? PlanEndMax { get; set; }
    }
}