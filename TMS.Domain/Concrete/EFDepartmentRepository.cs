using System;
using System.Collections.Generic;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using System.Linq;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFDepartmentRepository : BaseRepository, IDepartmentRepository
    {
        public IEnumerable<Department> Departments
        {
            get
            {
                return context.Departments;
            }
        }


        public Department Detail(int deptId)
        {
            return context.Departments.Find(deptId); ;
        }

        public bool Save(Department dept)
        {
            bool result = true;
            try
            {
                if (dept.DeptID <= 0)
                {
                    context.Departments.Add(dept);
                }
                else
                {
                    context.Entry(dept).State = System.Data.Entity.EntityState.Modified;
                }

                int changedNo = context.SaveChanges();
                if (changedNo <= 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
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



        public bool Delete(int deptId)
        {
            try
            {
                Department dbEntry = context.Departments.Find(deptId);
                if (dbEntry != null)
                {
                    context.Departments.Remove(dbEntry);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }


    }
}
