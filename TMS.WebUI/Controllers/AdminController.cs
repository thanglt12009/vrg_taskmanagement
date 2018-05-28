using System.Linq;
using System.Web.Mvc;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using System.Collections.Generic;
using TMS.WebApp.Services;
using TMS.Domain.Common;

namespace TMS.WebApp.Controllers
{
    public class AdminController : BaseController
    {
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;
        private IDepartmentRepository dptRepository;

        private IWorktaskRepository wRepository;

        public AdminController(IAccountRepository accRepository, IDepartmentRepository dptRepository, IWorktaskRepository worktaskRepo, ICategoryRepository catRepository) : base()

        {

            this.dptRepository = dptRepository;
            this.catRepository = catRepository;
            this.accRepository = accRepository;

            this.wRepository = worktaskRepo;

        }

        // GET: Admin
        [Authorize(Roles = "Quản trị")]
        public ActionResult ListAccount()
        {
           
            Account[] lstAccount = accRepository.Accounts.ToArray();

            return View(lstAccount);

        }
        [Authorize(Roles = "Quản trị")]
        public ActionResult AddUser()
        {
            List<SelectListItem> departments = new List<SelectListItem>();
            var lstDept = accRepository.Departments.ToList() as List<Department>;
            foreach (var item in lstDept)
            {
                departments.Add(new SelectListItem { Text = item.DeptName, Value = item.DeptID.ToString() });
            }
            ViewBag.Departments = departments;
            var newacc = new Account();
            newacc.IsLocalUser = true;
            newacc.Password = "12345";
            return PartialView("AddUserModal", newacc);
        }

        [Authorize(Roles = "Quản trị")]
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
        [Authorize(Roles = "Quản trị")]
        public ActionResult Save(Account account)
        {
            if (ModelState.IsValid)
            {
              
                if (!accRepository.Save(account))

                {
                    TempData["message"] = string.Format("Lưu không thành công");
                }
            }
            else
            {

                TempData["message"] = string.Format(GetModelErrorMessages(ModelState));
            }
            return RedirectToAction("ListAccount");

        }
        [Authorize(Roles = "Quản trị")]
        public ActionResult Delete(int UID)
        {
            if (ModelState.IsValid)
            {
                if (accRepository.Delete(UID))

                {
                    return RedirectToAction("ListAccount");
                }
            }
            
            TempData["message"] = string.Format("Xóa không thành công");
            return RedirectToAction("ListAccount");
        }
        public JsonResult IsUserExisted (string username)
        {
            bool check = accRepository.IsUserExist(username);
            return Json(new { success = true,  check = check});
        }


        public ActionResult ReIndexElastic()
        {
            ElasticIndexService elasticService = new ElasticIndexService();

            var list = (wRepository.Worktasks).ToList();
            //list.IncludeAllAttachmentObject();
            // Include all Attachment to relevant task. This will take time so maybe you should use a Join statement
            //to save performance here!!!
            foreach (var task in list)
            {
                task.loadMetaInfo(wRepository, accRepository);
                List<TMS.Domain.Entities.Attachment> attachmentList = wRepository.Attachments.Where(att => att.WorktaskID == task.WorktaskID).ToList();
                task.Attachment = null;

                // Refer to new attachment
                task.Attachment = attachmentList;
            }

            elasticService.CreateIndex(list);

            return RedirectToAction("Index","Admin");
        }

        public ActionResult DeleteIndex()
        {
            ElasticIndexService elasticService = new ElasticIndexService();
            elasticService.DeleteIndex(ElasticConfig.IndexName);
            return RedirectToAction("Index", "Admin");
        }



        public ViewResult Index()
        {
            return View();
        }

        // GET: Admin
        [Authorize(Roles = "Quản trị")]
        public ActionResult ListCategory()
        {

            Category[] lstCategories = catRepository.Categories.OrderBy(c => c.CatType).ThenBy(c => c.CatValue).ToArray();

            return View(lstCategories);

        }
        [Authorize(Roles = "Quản trị")]
        public ActionResult AddCategory()
        {
            List<SelectListItem> categories = new List<SelectListItem>();
            var lstCats = catRepository.Categories.OrderBy(c => c.CatType).ThenBy(c => c.CatValue).ToList() as List<Category>;
            categories.Add(new SelectListItem { Text = string.Empty, Value = string.Empty });
            foreach (var item in lstCats)
            {
                categories.Add(new SelectListItem { Text = item.CategoryName, Value = item.CatID.ToString() });
            }

            ViewBag.Categories = categories;
            var newcat = new Category();
            return PartialView("AddCategoryModal", newcat);
        }

        [Authorize(Roles = "Quản trị")]
        public ActionResult EditCategory(int catid)
        {
            List<SelectListItem> categories = new List<SelectListItem>();
            var lstCats = catRepository.Categories.OrderBy(c => c.CatType).ThenBy(c => c.CatValue).ToList() as List<Category>;
            categories.Add(new SelectListItem { Text = string.Empty, Value = string.Empty });
            foreach (var item in lstCats)
            {
                categories.Add(new SelectListItem { Text = item.CategoryName, Value = item.CatID.ToString() });
            }

            ViewBag.Categories = categories;
            Category cat = catRepository.Get(catid);

            return PartialView("EditCategoryModal", cat);
        }
        [Authorize(Roles = "Quản trị")]
        public ActionResult SaveCategory(Category cat)
        {
            if (ModelState.IsValid)
            {

                if (!catRepository.Save(cat))

                {
                    TempData["message"] = string.Format("Lưu không thành công");
                } else
                {
                    CategoryService.GetInstance().ReloadCategoryList((Contain.CatType)cat.CatType);
                    catRepository.ReloadDBContext();
                }
            }
            else
            {

                TempData["message"] = string.Format(GetModelErrorMessages(ModelState));
            }
            return RedirectToAction("ListCategory");

        }
        [Authorize(Roles = "Quản trị")]
        public ActionResult DeleteCategory(int catid)
        {
            if (ModelState.IsValid)
            {
                if (catRepository.Delete(catid))

                {
                    return RedirectToAction("ListCategory");
                }
            }

            TempData["message"] = string.Format("Xóa không thành công");
            return RedirectToAction("ListAccount");
        }

    }
}
