using System.Collections.Generic;
using TMS.Domain.Entities;
using System.ComponentModel.DataAnnotations;


namespace TMS.WebApp.Models
{
    public class AccountViewModel
    {

        // public  Account Account { get; set; }
        [Required(ErrorMessage ="Xin nhập tài khoản")]
        public string UserName { get; set; }

        //public int Role { get; set; }

        [Required(ErrorMessage = "Xin nhập mật khẩu")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}