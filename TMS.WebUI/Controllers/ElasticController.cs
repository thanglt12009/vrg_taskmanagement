using Nest;
using System.Web.Mvc;
using TMS.WebApp.Models;
using System.Collections.Generic;
using TMS.Domain.Entities;

using TMS.WebApp.Services;
using TMS.Domain.Abstract;
using TMS.Domain.Common;

namespace TMS.WebApp.Controllers
{
    [Authorize]
    public class ElasticController : BaseController
    {
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;
        public ElasticController(ICategoryRepository catRepository, IAccountRepository accRepository) : base()
        {

            this.catRepository = catRepository;
            this.accRepository = accRepository;
        }
        // GET: Search
        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(ElasticSearchQuery searchQuery)
        {
            //
            ElasticSearchService elasticService = new ElasticSearchService();

            var result = elasticService
                .Search(User.Identity.Name, searchQuery.RawSearch, 1, 20);

            return PartialView("_SearchResult", result);
        }
    }
}