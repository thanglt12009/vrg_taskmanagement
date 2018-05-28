using Nest;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using TMS.Domain.Common;

namespace TMS.Domain.Entities
{
    public class Transition
    {
        [Key]
        public int ID { get; set; }
        public virtual State FromState { get; set; }
        public int? FromStateID { get; set; }
        public virtual State ToState { get; set; }
        public int? ToStateID { get; set; }
        public virtual Event Event { get; set; }
        public int? EventID { get; set; }
        public bool DeleteFlag { get; set; }
    }
}
