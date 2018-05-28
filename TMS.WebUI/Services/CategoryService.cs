using TMS.Domain.Abstract;
using TMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TMS.WebApp.Services
{
    public class CategoryService
    {
        private ICategoryRepository catRepository;
        private static CategoryService _instance = null;
        private IDictionary<Contain.CatType, IEnumerable<SelectListItem>> categories = null;
        private object locked = new object();
        public CategoryService(ICategoryRepository catRepository)
        {
            this.catRepository = catRepository;
            categories = new Dictionary<Contain.CatType, IEnumerable<SelectListItem>>();
            if (_instance == null)
                _instance = this;
        }
        public static CategoryService GetInstance()
        {
            return _instance;
        }
        public void ReloadData()
        {
            lock (this)
            {
                catRepository.ReloadDBContext();
            }
        }
        public IEnumerable<SelectListItem> GetCategoryList(Contain.CatType catType)
        {
            lock (locked)
            {
                if (!categories.ContainsKey(catType))
                {
                    categories.Add(catType, catRepository.GetCategoriesByTypeForView((int)catType));
                }
            }
            return categories[catType];
        }
        public void ReloadCategoryList(Contain.CatType catType)
        {
            if (categories.ContainsKey(catType))
            {
                categories.Remove(catType);
                this.ReloadData();
                this.GetCategoryList(catType);
            }
        }
        public string GetCategoryName(int cateValue, Contain.CatType catType)
        {
            lock (this)
            {
                IEnumerable<SelectListItem> list = this.GetCategoryList(catType);
                var cat = list.Where<SelectListItem>(c => c.Value == cateValue.ToString()).FirstOrDefault();
                if (cat == null)
                {
                    return String.Empty;
                }
                return cat.Text;
            }
        }
    }
}