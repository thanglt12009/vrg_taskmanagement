using Nest;
using System;
using System.ComponentModel.DataAnnotations;

namespace TMS.Domain.Entities
{
    public class TaskHistory
    {
        [Key]
        public int HistoryID { get; set; }

        public int TaskID { get; set; }
        public string UpdatedField { get; set; }
        public string OriginalValue { get; set; }
        public string NewValue { get; set; }
        public DateTime UpdatedDateTime { get; set; }

        public string Action { get; set;}

        //using in temp
        public int UpdatedUser { get; set; }

        //Implementing 
        [Object(Ignore = true)]
        public virtual Account UpdatedUserAcc { get; set; }
      //  public virtual Worktask Worktask { get; set; }
        

    }
}
