using System;
using System.Collections.Generic;
using TMS.Domain.Entities;
using TMS.Domain.Common;

namespace TMS.Domain.Abstract
{
    public interface IAccountRepository : IBaseRepository
    {
        IEnumerable<Account> Accounts { get; }
        IEnumerable<Department> Departments { get; }
        int GetRoleForUser(string username);
        bool IsUserExist(string username);

        bool Save(Account account);

        bool SaveUserProfile(Account account);
        Account Find(int accountID);
        bool Delete(int accountID);
        Account Find(string username);

    }
}
