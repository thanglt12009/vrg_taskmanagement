using System.Collections.Generic;
using P050916.Domain.Abstract;
using P050916.Domain.Entities;

namespace P050916.Domain.Concrete
{
    public class EFTaskCategoryRepository : ITaskCategoryRepository
    {
        private EFDbContext context = new EFDbContext();

        public IEnumerable<TaskCategory> TaskCategories
        {
            get
            {
                
                    return context.TaskCategories;
            }
        }
    }
}
