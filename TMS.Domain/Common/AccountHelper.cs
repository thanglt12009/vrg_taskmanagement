using TMS.Domain.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Domain.Entities;
using System.Web.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Security.Cryptography;

namespace TMS.Domain.Common
{
    public static class AccountHelper
    {

        // TODO: Return user fullname from AD
        public static string GetUserFullname(string username)
        {
            EFDbContext db = EFDbContext.GetInstance();
            var domain = WebConfigurationManager.AppSettings["ADConnectionString"];
            try
            {
                PrincipalContext pc = new PrincipalContext(ContextType.Domain, domain);

                var principal = UserPrincipal.FindByIdentity(pc, username);
                if (null != principal)
                {
                    return principal.DisplayName;
                }
                else
                {
                    
                    return db.Accounts.Where(a => a.UserName == username).FirstOrDefault().DisplayName2;
                }
            }
            catch
            {
                return db.Accounts.Where(a => a.UserName == username).FirstOrDefault().DisplayName2;
            }

        }

        public static Account GetCurrentUser(string username)
        {
            EFDbContext db = EFDbContext.GetInstance();
            Account acc = null;
            lock (db)
            {
                acc = db.Accounts.Where(a => a.UserName == username).FirstOrDefault();
                if (acc == null)
                    acc = new Account();
            }

            return acc;
        }
        public static Account GetUserById(int userid)
        {
            EFDbContext db = EFDbContext.GetInstance();
            Account acc = null;
            lock (db)
            {
                acc = db.Accounts.Where(a => a.UID == userid).FirstOrDefault();
                if (acc == null)
                    acc = new Account();
            }

            return acc;
        }

        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }
    }
}
