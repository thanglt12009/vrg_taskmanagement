using System.Web.Mvc;
using TMS.Domain.Abstract;
using TMS.WebApp.Services;
using System;
using TMS.Domain.Entities;
using TMS.Domain.Common;
using System.Collections;
using System.Collections.Generic;
using TMS.WebApp.Models;

namespace TMS.WebApp.Controllers
{
    public class BaseController : Controller
    {

        // Constructor
        public BaseController()
        {
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            ViewBag.IsEnglish = requestContext.RouteData.Values.ContainsKey("Culture") ? (requestContext.RouteData.Values["culture"].ToString().ToLower() == "en") : true;
        }

        protected string GetModelErrorMessages(ModelStateDictionary modelState)
        {
            string errormsg = String.Empty;

            foreach (var state in modelState)
            {
                if (state.Value.Errors.Count > 0)
                {
                    foreach (var err in state.Value.Errors)
                    {
                        errormsg += err.ErrorMessage + "<br>";
                    }
                }
            }
            return errormsg;
        }
        public int GetLastViewedBoardId(string username)
        {
            var lvb = this.Request.Cookies.Get(username + "lastviewedboard");
            if (lvb != null)
            {
                int id = 0;
                if (int.TryParse(lvb.Value, out id))
                {
                    return id;
                }
            }
            return 0;
        }
    }
}