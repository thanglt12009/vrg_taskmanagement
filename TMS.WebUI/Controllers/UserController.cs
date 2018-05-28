using System.Web.Mvc;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using TMS.WebApp.Models;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Web.Security;
using System.Web.Configuration;
using System;
using TMS.Domain.Common;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace TMS.WebApp.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private IDepartmentRepository dptRepository;
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;

        public UserController(IAccountRepository repository, IDepartmentRepository dptRepository, ICategoryRepository catRepository) : base()
        {
            this.dptRepository = dptRepository;
            this.catRepository = catRepository;
            this.accRepository = repository;
        }
        [Authorize]
        public ActionResult EditUser(int UID)
        {
            List<SelectListItem> departments = new List<SelectListItem>();
            var lstDept = dptRepository.Departments.ToList() as List<Department>;
            foreach (var item in lstDept)
            {
                departments.Add(new SelectListItem { Text = item.DeptName, Value = item.DeptID.ToString() });
            }
            ViewBag.Departments = departments;

            Account acc = accRepository.Accounts.Where(a => a.UID == UID).FirstOrDefault();

            return PartialView("EditUserModal", acc);
        }
        [Authorize]
        public JsonResult Save(Account account)
        {
            string message = String.Empty;
            if (ModelState.IsValid)
            {

                if (!accRepository.SaveUserProfile(account))

                {
                    message = string.Format("Lưu không thành công");
                }
            }
            else
            {
                message = string.Format(GetModelErrorMessages(ModelState));
            }
            return Json(new {errMsg = message });

        }
    }
}