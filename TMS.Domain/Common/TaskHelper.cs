using P050916.Domain.Concrete;
using P050916.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace P050916.Domain.Common
{
    public static class TaskHelper
    {
        public static IEnumerable<TaskCategory> getListForTaskCategory()
        {
            List<TaskCategory> listTaskCate = new List<TaskCategory>();

            using (EFDbContext context = new EFDbContext())
            {
                listTaskCate = context.TaskCategories.ToList();
            }

            var list = listTaskCate.Select(h => new SelectListItem
            {
                Text = h.TaskCategoryName,
                Value = h.TaskCatID.ToString()
            });

            return listTaskCate;
        }
    }
}
