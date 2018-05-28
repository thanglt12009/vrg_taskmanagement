using System;
using System.Collections.Generic;
using TMS.Domain.Entities;
using TMS.Domain.Common;
using System.Web.Mvc;

namespace TMS.Domain.Abstract
{
    public interface ICategoryRepository : IBaseRepository
    {
        IEnumerable<Category> Categories { get; }
        Category GetByCatType(int CatValue, int CatType);
        IEnumerable<Category> GetCategoriesByType(int catType);
        IEnumerable<SelectListItem> GetCategoriesByTypeForView(int catType);
        Category Get(int CatID);
        bool Save(Category cat);
        bool Delete(int catID);
    }
}
