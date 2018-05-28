using P050916.Domain.Concrete;
using P050916.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace P050916.WebApp.App_Start
{
    public class TaskCategoriesCollectActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            //IEnumerable<TaskCategory> list;
            //using (EFDbContext dbcontext = new EFDbContext())
            //{
            //    list = dbcontext.TaskCategories.ToList();
            //}
            //filterContext.Controller.ViewBag.CategoryList = list.Select(c => new SelectListItem
            //{
            //    Text = c.TaskCategoryName,
            //    Value = c.TaskCatID.ToString()
            //}).ToList();
        }
    }
}