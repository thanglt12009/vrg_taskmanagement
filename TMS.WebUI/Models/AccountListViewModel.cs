using System;
using System.Collections.Generic;
using TMS.Domain.Entities;

namespace TMS.WebApp.Models
{
    public class AccountListViewModel
    {
        public IEnumerable<Account> Account { get; set; }
    }
}