using TMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.WebApp.Models
{
    public class TaskReportRecordModel
    {
        public int TaskType { get; set; }
        public string Title { get; set; }
        public string AssigneeName { get; set; }

        public int Status { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdateDate { get; set; }
    }
}