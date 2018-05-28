using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace P050916.WebApp.Models
{
    public class KanbanFilterModel
    {
        public string  AsigneeName { get; set; }

        public int? TaskCategory { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PlanStartMin { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PlanStartMax { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PlanEndMin { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/mm/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PlanEndMax { get; set; }
    }
}