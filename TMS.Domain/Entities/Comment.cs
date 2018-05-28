using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TMS.Domain.Entities
{
    public class Comment
    {
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int TaskID { get; set;}

        [DataType(DataType.MultilineText)]
        public string CommentContent { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int AccountID { get; set; }

        [NotMapped]
        public string Username { get; set; }

        [NotMapped]
        public string RelativeTime { get; set; }

        public DateTime CommentDate { get; set; }
        public bool DeleteFlag { get; set; }

        public virtual Account Account { get; set; }

    }
}
