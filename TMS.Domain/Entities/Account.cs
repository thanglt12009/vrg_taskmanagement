using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TMS.Domain.Common;

namespace TMS.Domain.Entities
{
    public class Account
    {
        [Key]
        public int UID { get; set; }
        [Required]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Vai trò")]
        public int Role { get; set; }

        public int DeptID { get; set; }
        [Display(Name = "Người dùng nội bộ")]
        public bool IsLocalUser { get; set; }

        [Display(Name = "Tên đầy đủ")]
        public string DisplayName2 { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [ForeignKey("DeptID")]
        public virtual Department Department { get; set; }
        public bool DeleteFlag { get; set; }
        public string Password { get; set; }


        public Account(string username, int role, Department dept)
        {
            this.UserName = username;
            this.Role = role;
            this.Department = dept;
        }
        public Account()
        {
            this.WorkTasks = new HashSet<Worktask>();
            this.WorkTask1s = new HashSet<Worktask>();
            this.WorkTask2s = new HashSet<Worktask>();
            this.TaskHistory = new HashSet<TaskHistory>();
            this.Comment = new HashSet<Comment>();
        }
        public virtual ICollection<Worktask> WorkTasks { get; set; }

        public virtual ICollection<Worktask> WorkTask1s { get; set; }
        public virtual ICollection<Worktask> WorkTask2s { get; set; }

        public virtual ICollection<TaskHistory> TaskHistory { get; set; }

        public virtual ICollection<Comment> Comment { get; set; }

        public override string ToString()
        {
            return this.DisplayName2;
        }

        public virtual ICollection<Board> Boards { get; set; }
        public virtual ICollection<Board> OwnedBoards { get; set; }
    }
}
