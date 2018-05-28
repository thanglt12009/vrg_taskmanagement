using TMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TMS.WebApp.Models
{
    public class KanbanInfoModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Xin nhập Tên bảng")]
        [Display(Name = "Tên bảng")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Xin nhập Mã bảng")]
        [Display(Name = "Mã bảng")]
        public string Code { get; set; }
        public Account Owner { get; set; }
        public Account Assignee { get; set; }
        public Workflow Workflow { get; set; }
        [Required(ErrorMessage = "Xin chọn Quy trình")]
        [Display(Name = "Quy trình")]
        public int WorkflowID { get; set; }
        public string[] BoardMemberList { get; set; }
    }
}