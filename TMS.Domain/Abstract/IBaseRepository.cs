using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Domain.Abstract
{
    public interface IBaseRepository
    {
        void Detach<TEntity>(TEntity acc) where TEntity : class;
        void ReloadDBContext();
    }
}
