using System;
using System.Linq;
using System.Web.Mvc;
using TMS.Domain.Abstract;
using TMS.Domain.Common;
using TMS.Domain.Entities;
using System.Collections.Generic;
using TMS.WebApp.Models;
using TMS.WebApp.Services;
using System.Collections;

namespace TMS.WebApp.Controllers
{
    [Authorize]
    public class DashboardsController : BaseController
    {
        private IWorktaskRepository repository;
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;
        private IWorkflowRepository wfRepository;
        private IList<KanbanInfoModel> userBoardList;

        // Constructor
        public DashboardsController(IWorktaskRepository worktaskRepository, ICategoryRepository catRepository, IAccountRepository accRepository, IWorkflowRepository wfRepository) : base()
        {
            this.repository = worktaskRepository;
            this.catRepository = catRepository;
            this.accRepository = accRepository;
            this.wfRepository = wfRepository;
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            userBoardList = KanbanService.GetInstance().UserBoardList(User.Identity.Name);
        }

        public ActionResult Index(int workflow)
        {
            DashboardViewModel data = new DashboardViewModel();
            data.WorkflowID = workflow;
            data.StateList = wfRepository.Detail(workflow).States.OrderBy(s => s.ID).ToList<State>();
            return View(data);
        }

        public ActionResult TaskGroupByStatus(int workflow)
        {
            Workflow wf = wfRepository.Detail(workflow);
            int[] boardIdList = userBoardList.Where(b => b.Workflow.ID == workflow).Select(b => b.Id).ToArray();
            var statusList = wf.States.OrderBy(s => s.ID).Select(b => b.ID).ToList();
            var statusSummary = repository.Worktasks.Where(m => boardIdList.Any(s => s == m.BoardID)).OrderBy(g => g.Status).GroupBy(m => m.Status).Select(g => new { status = g.Key, number = g.Count() }).ToArray();
            List<int> data = new List<int>();
            foreach (var item in statusList)
            {
                if (item > 0)
                {
                    bool found = false;
                    foreach (var d in statusSummary)
                    {
                        if (d.status == item)
                        {
                            data.Add(d.number);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        data.Add(0);
                    }
                }
            }

            var dataTaskByStatusChart = new
            {
                labels = wf.States.OrderBy(s => s.ID).Select(b => b.Name).ToArray<string>(),
                datasets = new[] {
                    new {
                        data = data.ToArray(),
                        backgroundColor = new []
                        {
                            "#a3e1d4",
                            "#dedede",
                            "#b5b8cf",
                            "#4dffb8",
                            "#d9b38c",
                            "#fffd99"
                        },
                        hoverBackgroundColor = new []
                        {
                            "#c5ece4",
                            "#e6e6e6",
                            "#c1c4d7",
                            "#80ffcc",
                            "#e6ccb3",
                            "#ffff16"
                        }
                    }
                }
            };

            var result = Json(dataTaskByStatusChart, JsonRequestBehavior.AllowGet);
            return result;
        }

        public ActionResult TaskGroupByPriority()
        {
            int[] boardIdList = userBoardList.Select(b => b.Id).ToArray();
            var priorityList = CategoryService.GetInstance().GetCategoryList(Contain.CatType.Priority).OrderBy(p => p.Value).Select(p => p.Value).ToList();
            var prioritySummary = repository.Worktasks.Where(m => boardIdList.Any(s => s == m.BoardID)).OrderBy(g => g.Priority).GroupBy(m => m.Priority).Select(g => new { priority = g.Key, number = g.Count() }).ToArray();
            List<int> data = new List<int>();
            foreach(var item in priorityList)
            {
                if (item.Length > 0)
                {
                    bool found = false;
                    foreach (var d in prioritySummary)
                    {
                        if (d.priority == int.Parse(item))
                        {
                            data.Add(d.number);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        data.Add(0);
                    }
                }
            }
            var dataTaskByStatusChart = new
            {
                labels = CategoryService.GetInstance().GetCategoryList(Contain.CatType.Priority).Where(s => s.Text != String.Empty).OrderBy(s => s.Value).Select(s => s.Text).ToArray<string>(),
                datasets = new[] {
                    new {
                        data = data.ToArray(),
                        backgroundColor = new []
                        {
                            "#a3e1d4",
                            "#dedede",
                            "#b5b8cf",
                            "#4dffb8",
                            "#d2a679"
                        },
                        hoverBackgroundColor = new []
                        {
                            "#c5ece4",
                            "#e6e6e6",
                            "#c1c4d7",
                            "#80ffcc",
                            "#e6ccb3"
                        }
                    }
                }
            };

            var result = Json(dataTaskByStatusChart, JsonRequestBehavior.AllowGet);
            return result;
        }

        [HttpPost]
        public ActionResult TaskPivot(int workflow)
        {
            JsonResult result = new JsonResult();

            try
            {
                // Initialization.   
                string search = Request.Form.GetValues("search[value]")[0];
                string draw = Request.Form.GetValues("draw")[0];
                string order = Request.Form.GetValues("order[0][column]")[0];
                string orderDir = Request.Form.GetValues("order[0][dir]")[0];
                int startRec = Convert.ToInt32(Request.Form.GetValues("start")[0]);
                int pageSize = Convert.ToInt32(Request.Form.GetValues("length")[0]);
                // Loading
                List<Dictionary<string, string>> data = this.LoadData(workflow);
                // Total record count.   
                int totalRecords = data.Count;
                // Verification.   
                if (!string.IsNullOrEmpty(search) &&
                    !string.IsNullOrWhiteSpace(search))
                {
                    // Apply search   
                    data = data.Where(p => p["0"].ToString().ToLower().Contains(search.ToLower())).ToList();
                }
                // Sorting.   
                data = this.SortByColumnWithOrder(order, orderDir, data);
                // Filter record count.   
                int recFilter = data.Count;
                // Apply pagination.   
                data = data.Skip(startRec).Take(pageSize).ToList();
                // Loading drop down lists.   
                result = this.Json(new
                {
                    draw = Convert.ToInt32(draw),
                    recordsTotal = totalRecords,
                    recordsFiltered = recFilter,
                    data = data
                }, JsonRequestBehavior.AllowGet);

            }
            catch
            {

                throw;
            }

            return result;
        }

        #region Load Data  
        /// <summary>  
        /// Load data method.   
        /// </summary>  
        /// <returns>Returns - Data</returns>  
        private List<Dictionary<string, string>> LoadData(int workflow)
        {
            // Initialization.   
            List<Dictionary<string, string>> lst = new List<Dictionary<string, string>>();
            int[] boardIdList = userBoardList.Where(b => b.Workflow.ID == workflow).Select(b => b.Id).ToArray();
            try
            {
                var dataList = repository.Worktasks.Where(m => boardIdList.Any(s => s == m.BoardID)).GroupBy(x => x.AssigneeAcc.UserName)
                    .Select(g => new
                    {
                        Name = g.Key,
                        Count = g.Count(),
                        StatusList = g.GroupBy(x => x.Status).Select(xg => new { Status = xg.Key, Count = xg.Count() })
                    }).ToList();
                var statuslist = userBoardList[0].Workflow.States.OrderBy(s => s.ID).Select(s => s.ID).Distinct().ToList();

                foreach (var record in dataList)
                {
                    // Initialization.
                    Dictionary<string, string> item = new Dictionary<string, string>();
                    item.Add("0", record.Name);
                    foreach (var status in statuslist)
                    {
                        var task = record.StatusList.Where(x => x.Status == status).FirstOrDefault();
                        item.Add(status.ToString(), task != null ? task.Count.ToString() : "0");
                    }
                    lst.Add(item);
                }
            }
            catch (Exception ex)
            {
                // info.   
                Console.Write(ex);
            }
            // info.   
            return lst;
        }
        #endregion

        #region Get data method
        /// <summary>  
        /// Sort by column with order method.   
        /// </summary>  
        /// <param name="order">Order parameter</param>  
        /// <param name="orderDir">Order direction parameter</param>  
        /// <param name="data">Data parameter</param>  
        /// <returns>Returns - Data</returns>  
        private List<Dictionary<string, string>> SortByColumnWithOrder(string order, string orderDir, List<Dictionary<string, string>> data)
        {
            // Initialization.   
            List<Dictionary<string, string>> lst = new List<Dictionary<string, string>>();
            try
            {
                lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ContainsKey(order) ? p[order] : string.Empty).ToList() : data.OrderBy(p => p.ContainsKey(order) ? p[order] : string.Empty).ToList();
            }
            catch (Exception ex)
            {
                // info.   
                Console.Write(ex);
            }
            // info.   
            return lst;
        }
        #endregion  
    }
}