using TMS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Domain.Concrete
{
    public class BaseRepository : IBaseRepository
    {
        protected EFDbContext context = EFDbContext.GetInstance();
        public void Detach<TEntity>(TEntity entity) where TEntity : class
        {
            context.Entry(entity).State = EntityState.Detached;
        }

        public void ReloadDBContext()
        {
            context.Dispose();
            context = EFDbContext.GetInstance();
        }
    }
}
