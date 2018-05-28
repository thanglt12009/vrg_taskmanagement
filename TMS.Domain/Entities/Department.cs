using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace TMS.Domain.Entities
{
    public class Department
    {
        
        public Department()
        {
            //this.Accounts = new List<Account>();
            this.Accounts = new HashSet<Account>();
        }
        [Key]
        public int DeptID { get; set; }
        [Display(Name ="Phòng ban")]
        public string DeptName { get; set; }
        public bool DeleteFlag { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }

    }
}
