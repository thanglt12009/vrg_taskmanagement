using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface IDepartmentRepository : IBaseRepository
    {
        IEnumerable<Department> Departments { get; }

        Department Detail(int deptId);

        bool Save(Department dept);

        bool Delete(int deptId);
    }
}
