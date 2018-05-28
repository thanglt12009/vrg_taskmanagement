using Nest;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using TMS.Domain.Common;

namespace TMS.Domain.Entities
{
    public class Event
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Transition> Transitions { get; set; }
        public bool DeleteFlag { get; set; }
    }
}
