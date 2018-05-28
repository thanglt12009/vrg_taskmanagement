using System.Collections.Generic;
using P050916.Domain.Entities;

namespace P050916.Domain.Abstract
{
    public interface ITaskCategoryRepository
    {
        IEnumerable<TaskCategory> TaskCategories { get; }
    }
}
