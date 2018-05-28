using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Http;
using System.Web.Optimization;
using System.Data.Entity;
using TMS.Domain.Concrete;
using TMS.WebApp.Infrastructure;
using TMS.WebApp.Services;

namespace TMS.WebApp
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            App_Start.BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Disable EF
            Database.SetInitializer<EFDbContext>(null);
            ServiceBuilder.Initialize();
        }
    }
}