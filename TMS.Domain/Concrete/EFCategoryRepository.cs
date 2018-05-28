using System;
using System.Collections.Generic;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Validation;
using TMS.Domain.Common;
using System.Web.Mvc;

namespace TMS.Domain.Concrete
{
    public class EFCategoryRepository : BaseRepository, ICategoryRepository
    {
        public IEnumerable<Category> Categories
        {
            get
            {
                return context.Categories;
            }
        }

        public Category Get(int CatID)
        {
            return context.Categories.Find(CatID);
        }

        public Category GetByCatType(int CatValue, int CatType)
        {
            Category cat = context.Categories.Where(c => c.CatType == CatType && c.CatValue == CatValue).FirstOrDefault();
            if (null != cat)
            {
                return cat;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Category> GetCategoriesByType(int catType)
        {
            return context.Categories.Where<Category>(m => m.CatType == catType && m.DeleteFlag == false);
        }

        public IEnumerable<SelectListItem> GetCategoriesByTypeForView(int catType)
        {
            var catList = this.GetCategoriesByType(catType);
            IEnumerable<SelectListItem> selectList = catList.Select(c => new SelectListItem() { Text = c.CategoryName, Value = c.CatValue.ToString(), Selected = false }).ToList< SelectListItem>();
            ((List<SelectListItem>)selectList).Insert(0, new SelectListItem() { Value = String.Empty, Text = String.Empty, Selected = false });
            return selectList;
        }
        public bool Save(Category cat)
        {
            bool result = false;
            try
            {
                if (cat.CatID == 0)
                {
                    context.Categories.Add(cat);

                }
                else
                {
                    Category dbEntry = context.Categories.Where(a => a.CatID == cat.CatID).FirstOrDefault();
                    dbEntry.CategoryName = cat.CategoryName;
                    dbEntry.CatValue = cat.CatValue;
                    dbEntry.CatType = cat.CatType;
                    dbEntry.ParentCatID = cat.ParentCatID;

                    context.Entry(dbEntry).State = EntityState.Modified;
                }
                result = context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException e)
            {
                result = false;
                foreach (var eve in e.EntityValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
            }
            catch
            {
                result = false;
                //e.GetBaseException();
            };
            return result;
        }

        public bool Delete(int catID)
        {
            Category dbEntry = context.Categories.Find(catID);
            if (null != dbEntry)
            {
                context.Categories.Remove(dbEntry);
            }
            return context.SaveChanges() > 0;
        }
    }
}
