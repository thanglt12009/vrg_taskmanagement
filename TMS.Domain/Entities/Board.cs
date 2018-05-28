using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace TMS.Domain.Entities
{
    public class Board
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        public int ID { get; set; }

        [Required(ErrorMessage = "Xin nhập Tên bảng")]
        [Display(Name = "Tên bảng")]
        public string Title { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string Code { get; set; }

        public int OwnerID { get; set; }

        public virtual Account Owner { get; set; }

        public virtual ICollection<Account> Members { get; set; }
        public virtual ICollection<Worktask> Tasks { get; set; }
        public virtual Workflow Workflow { get; set; }
        public int? WorkflowID { get; set; }

        public bool DeleteFlag { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }
}
