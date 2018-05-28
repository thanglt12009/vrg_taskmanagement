using System.ComponentModel.DataAnnotations;

namespace TMS.Domain.Entities
{
    public class Action
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual State State { get; set; }
        public int? StateID { get; set; }
        public int Type { get; set; }
        public string Detail { get; set; }
        public bool DeleteFlag { get; set; }
    }
}
