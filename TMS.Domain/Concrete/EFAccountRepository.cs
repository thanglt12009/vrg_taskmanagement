using System;
using System.Collections.Generic;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Validation;
using TMS.Domain.Common;

namespace TMS.Domain.Concrete
{
    public class EFAccountRepository : BaseRepository, IAccountRepository
    {

        public IEnumerable<Account> Accounts
        {
            get
            {
                return context.Accounts;
            }
        }
        public IEnumerable<Department> Departments
        {
            get
            {
                return context.Departments;
            }
        }
        
        public int GetRoleForUser(string username)
        {
            var acc = context.Accounts.Where(a => a.UserName == username).FirstOrDefault();
            if (null != acc)
            {
                return acc.Role;
            }
            else
            {
                return 0;
            }
        }
        public bool IsUserExist(string username)
        {
            Account acc = context.Accounts.Where(a => a.UserName == username).FirstOrDefault();
            if (null != acc)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public Account Find(string username)
        {
            Account acc = context.Accounts.Where(a => a.UserName == username).FirstOrDefault();
            if (null != acc)
            {
                return acc;
            }
            else
            {
                return null;
            }
        }
        public Account Find(int UID)
        {

            return context.Accounts.Find(UID);
        }
        public bool Save(Account account)
        {
            bool result = false;
            try
            {
                if (account.UID == 0)
                {
                    context.Accounts.Add(account);

                }
                else
                {
                    Account dbEntry = context.Accounts.Where(a => a.UserName == account.UserName).FirstOrDefault();
                    dbEntry.UserName = account.UserName;
                    dbEntry.Role = account.Role;
                    dbEntry.DisplayName2 = account.DisplayName2;
                    dbEntry.Email = account.Email;
                    dbEntry.IsLocalUser = account.IsLocalUser;
                    dbEntry.DeptID = account.DeptID;
                    if (account.Password != null && account.Password.Trim().Length > 0)
                    {
                        dbEntry.Password = account.Password.Trim();
                    }

                    context.Entry(dbEntry).State = EntityState.Modified;
                }
                result = context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException e)
            {
                result = false;
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch
            {
                result = false;
                //e.GetBaseException();
            };
            return result;
        }
        public bool SaveUserProfile(Account account)
        {
            bool result = false;
            try
            {
                if (account.UID == 0)
                {
                    context.Accounts.Add(account);

                }
                else
                {
                    Account dbEntry = context.Accounts.Where(a => a.UserName == account.UserName).FirstOrDefault();
                    dbEntry.UserName = account.UserName;
                    dbEntry.DisplayName2 = account.DisplayName2;
                    dbEntry.Email = account.Email;
                    if (account.Password != null && account.Password.Length > 0)
                    {
                        dbEntry.Password = account.Password;
                    }

                    context.Entry(dbEntry).State = EntityState.Modified;
                }
                result = context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException e)
            {
                result = false;
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch
            {
                result = false;
                //e.GetBaseException();
            };
            return result;
        }

        public bool Delete(int accountID)
        {
            Account dbEntry = context.Accounts.Find(accountID);
            if (null != dbEntry)
            {
                context.Accounts.Remove(dbEntry);
            }
            return context.SaveChanges() > 0;
        }
    }
}
