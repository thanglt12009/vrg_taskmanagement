using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Domain.Entities
{
    public class WorkTaskMetas
    {
        [Key]
        [Number(Ignore = true)]
        [HiddenInput(DisplayValue = false)]
        public int MetaID { get; set; }

        [Number(Ignore = true)]
        public int? WorktaskID { get; set; }

        public Worktask Task { get; set; }

        [Text(Ignore = true)]
        public string MetaValue { get; set; }

        [Text(Ignore = true)]
        public string MetaKey { get; set; }

        [Number(Ignore = true)]
        public int MetaType { get; set; }

        [Boolean(Ignore = true)]
        public bool DeleteFlag { get; set; }

        public WorkTaskMetas clone()
        {
            return (WorkTaskMetas)this.MemberwiseClone();
        }
    }
}
