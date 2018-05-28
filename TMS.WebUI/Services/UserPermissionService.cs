using TMS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.WebApp.Services
{
    public class UserPermissionService
    {
        private IAccountRepository accRepository;
        private static UserPermissionService _instance = null;
        public UserPermissionService(IAccountRepository accRepository)
        {
            this.accRepository = accRepository;
            if (_instance == null)
                _instance = this;
        }
        public static UserPermissionService GetInstance()
        {
            return _instance;
        }

        public bool isAdmin(int role)
        {
            return (role == 1);
        }
        public bool isManager(int role)
        {
            return (role == 2);
        }
        public bool isUser(int role)
        {
            return (role == 3);
        }
        public bool isDirector(int role)
        {
            return (role == 4);
        }
        public bool isCouncilMember(int role)
        {
            return (role == 5);
        }
    }
}