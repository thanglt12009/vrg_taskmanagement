using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Moq;
using Ninject;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using TMS.Domain.Concrete;

namespace TMS.WebApp.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        // Declare Ninject Kernel
        private IKernel kernel;
        // Constructor with Param
        public NinjectDependencyResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }

        public void AddBindings()
        {
            //Bind data with real connection. Replace when you can access it
            kernel.Bind<IWorktaskRepository>().To<EFWorktaskRepository>();
            kernel.Bind<IAccountRepository>().To<EFAccountRepository>();
            kernel.Bind<IDepartmentRepository>().To<EFDepartmentRepository>();
            kernel.Bind<ICategoryRepository>().To<EFCategoryRepository>();
            kernel.Bind<IBoardRepository>().To<EFBoardRepository>();
            kernel.Bind<IWorkflowRepository>().To<EFWorkflowRepository>();
        }
    }
}