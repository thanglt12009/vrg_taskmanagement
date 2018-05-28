using System;
using System.Linq;
using System.Web.Mvc;
using TMS.Domain.Abstract;
using TMS.WebApp.Models;
using TMS.Domain.Common;
using System.Collections.Generic;
using TMS.Domain.Entities;
using TMS.WebApp.Services;

namespace TMS.WebApp.Controllers
{
    [Authorize]
    public class KanbanController : BaseController
    {
        private IWorktaskRepository wRepository;
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;
        private IBoardRepository boardRepository;
        private IWorkflowRepository wfRepository;
        private IList<KanbanInfoModel> userBoardList;
        // Constructor
        public KanbanController(IBoardRepository boardRepository, IWorktaskRepository worktaskRepository, ICategoryRepository catRepository, IAccountRepository accRepository, IWorkflowRepository wfRepository) : base()
        {
            this.wRepository = worktaskRepository;
            this.catRepository = catRepository;
            this.accRepository = accRepository;
            this.boardRepository = boardRepository;
            this.wfRepository = wfRepository;
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            userBoardList = KanbanService.GetInstance().UserBoardList(User.Identity.Name);
        }

        // GET: Kanban
        public ActionResult ViewBoard(string boardcode)
        {
            Board board;
            if (boardcode == null)
            {
                int boardId = this.GetLastViewedBoardId(User.Identity.Name);
                board = boardRepository.Detail(boardId);
            }
            else
            {
                board = boardRepository.Boards.Where(b => b.Code == boardcode).FirstOrDefault();
                if (board != null)
                {
                    board = boardRepository.Detail(board.ID);
                }
            }

            if (!KanbanService.GetInstance().CheckBoardValid(board, User.Identity.Name))
            {
                board = null;
            }

            if (board != null)
            {
                // Build the model for VIEW
                var listTask = wRepository.Worktasks
                    .Where(t => ((t.BoardID == board.ID) && (t.ActualEndDate != null && t.ActualEndDate.Value.AddDays(7) > DateTime.Now || t.ActualEndDate == null)))
                    .GroupBy(w => w.Status)
                    .Select(g =>
                    new {
                        Status = g.Key,
                        List = g
                        .Select(gr =>
                            new WorktaskSimpleModel
                            {
                                WorktaskID = gr.WorktaskID,
                                Title = gr.Title,
                                Description = gr.Description,
                                Identify = gr.Identify,
                                StartDate = gr.PlannedStartDate,
                                EndDate = gr.PlannedEndDate,
                                Assignee = gr.AssigneeAcc,
                                Priority = gr.Priority,
                                TaskType = gr.TaskType,
                                Status = gr.Status
                            })
                    }).ToList();
                Workflow wf = board.Workflow;
                var kanbanModel = new KanbanViewModel();

                kanbanModel.Board = new KanbanInfoModel {
                    Id = board.ID,
                    Title = board.Title,
                    Code = board.Code,
                    BoardMemberList = board.Members.Select(m => m.UID.ToString()).ToArray(),
                    Owner = board.Owner,
                    Workflow = wf
                };
                kanbanModel.BoardCode = board.Code;
                kanbanModel.TaskLists = new Dictionary<int, IList<WorktaskSimpleModel>>();
                foreach (State state in wf.States)
                {
                    var taskListByState = listTask.Where(t => t.Status == state.ID).Select(p => p.List).FirstOrDefault();
                    kanbanModel.TaskLists.Remove(state.ID);
                    kanbanModel.TaskLists.Add(state.ID, (taskListByState != null) ? taskListByState.ToList() : new List<WorktaskSimpleModel>());
                }
                this.Response.Cookies.Remove(User.Identity.Name + "lastviewedboard");
                this.Response.Cookies.Add(new System.Web.HttpCookie(User.Identity.Name + "lastviewedboard", board.ID.ToString()));
                this.Response.Cookies.Get(User.Identity.Name + "lastviewedboard").Expires = DateTime.Now.AddDays(1);
                return View(kanbanModel);
            }
            else
            {
                return this.RedirectToAction("Index");
            }
            
        }

        public ActionResult Index()
        {
            KanbanListViewModel kanbanListModel = new KanbanListViewModel();
            kanbanListModel.BoardList = userBoardList;
            kanbanListModel.WorkflowList = wfRepository.Workflows.ToList();
            return View(kanbanListModel);
        }
        [HttpPost]
        public PartialViewResult List(KanbanViewModel viewModel)
        {
            Board board;
            string boardcode = viewModel.BoardCode;
            if (boardcode == null)
            {
                int boardId = this.GetLastViewedBoardId(User.Identity.Name);
                board = boardRepository.Detail(boardId);
            }
            else
            {
                board = boardRepository.Boards.Where(b => b.Code == boardcode).FirstOrDefault();
                if (board != null)
                {
                    board = boardRepository.Detail(board.ID);
                }
            }

            if (!KanbanService.GetInstance().CheckBoardValid(board, User.Identity.Name))
            {
                board = null;
            }
            IEnumerable<Worktask> filteredListTask;
            if (board != null)
            {
                filteredListTask = wRepository.Worktasks.Where(t => t.BoardID == board.ID);
            }
            else
            {
                filteredListTask = wRepository.Worktasks.Where(w => w.BoardID == 0);
            }
            if (viewModel.TaskType != null)
            {
                filteredListTask = filteredListTask.Where(t => t.TaskType == viewModel.TaskType);
            }

            if (viewModel.TaskPriority != null)
            {
                filteredListTask = filteredListTask.Where(t => t.Priority == viewModel.TaskPriority);
            }

            if (!String.IsNullOrEmpty(viewModel.AsigneeName))
            {
                filteredListTask = filteredListTask.Where(t => t.AssigneeAcc.UserName.ToUpper().Contains(viewModel.AsigneeName.ToUpper())
                || t.AssigneeAcc.DisplayName2.ToUpper().Contains(viewModel.AsigneeName.ToUpper()));
            }
            if (viewModel.PlanStartMax != null)
            {
                filteredListTask = filteredListTask.Where(t => t.PlannedStartDate <= viewModel.PlanStartMax);
            }
            if (viewModel.PlanStartMin != null)
            {
                filteredListTask = filteredListTask.Where(t => t.PlannedStartDate >= viewModel.PlanStartMin);
            }
            if (viewModel.PlanEndMax != null)
            {
                filteredListTask = filteredListTask.Where(t => t.PlannedEndDate <= viewModel.PlanEndMax);
            }
            if (viewModel.PlanEndMin != null)
            {
                filteredListTask = filteredListTask.Where(t => t.PlannedEndDate >= viewModel.PlanEndMin);
            }

            Workflow wf = board.Workflow;
            var kanbanModel = new KanbanViewModel();
            var listTask = filteredListTask
                .GroupBy(w => w.Status)
                .Select(g =>
                new {
                    Status = g.Key,
                    List = g
                    .Select(gr =>
                        new WorktaskSimpleModel
                        {
                            WorktaskID = gr.WorktaskID,
                            Title = gr.Title,
                            Description = gr.Description,
                            Identify = gr.Identify,
                            StartDate = gr.PlannedStartDate,
                            EndDate = gr.PlannedEndDate,
                            Assignee = gr.AssigneeAcc,
                            Priority = gr.Priority,
                            TaskType = gr.TaskType,
                            Status = gr.Status
                        })
                }).ToList();

            kanbanModel.Board = new KanbanInfoModel
            {
                Id = board.ID,
                Title = board.Title,
                Code = board.Code,
                BoardMemberList = board.Members.Select(m => m.UID.ToString()).ToArray(),
                Owner = board.Owner,
                Workflow = wf
            };
            kanbanModel.TaskLists = new Dictionary<int, IList<WorktaskSimpleModel>>();
            foreach (State state in wf.States)
            {
                var taskListByState = listTask.Where(t => t.Status == state.ID).Select(p => p.List).FirstOrDefault();
                kanbanModel.TaskLists.Remove(state.ID);
                kanbanModel.TaskLists.Add(state.ID, (taskListByState != null) ? taskListByState.ToList() : new List<WorktaskSimpleModel>());
            }

            return PartialView("_Kanban", kanbanModel);
            
        }

        [HttpPost]
        public ActionResult SaveBoard(KanbanInfoModel model)
        {
            if (ModelState.IsValid)
            {
                bool isNew = false;
                Board board = null;

                if (model.Id > 0)
                {
                    board = boardRepository.Detail(model.Id);
                    if (board != null)
                    {
                        board.Title = model.Title.Trim();
                        board.Members.Clear();
                    }
                }
                if (board == null)
                {
                    board = new Board
                    {
                        ID = model.Id,
                        Title = model.Title.Trim(),
                        Code = (model.Code.Trim().Length == 0 ? KanbanService.GetInstance().GenerateBoardCode(model.Title.Trim()) : model.Code.Trim()),
                        OwnerID = AccountHelper.GetCurrentUser(User.Identity.Name).UID,
                        WorkflowID = model.WorkflowID,
                        Members = new List<Account>()
                    };
                    isNew = true;
                }
                
                foreach(string id in model.BoardMemberList)
                {
                    int uid = 0;
                    int.TryParse(id, out uid);
                    var user = accRepository.Find(uid);
                    if (user != null && uid != board.OwnerID && (!board.Members.Select(m => m.UID).Contains(uid)))
                    {
                        board.Members.Add(user);
                    }
                }
                if (boardRepository.Save(board))
                {
                    KanbanService.GetInstance().ReloadData();
                    TempData["message"] = string.Format("Lưu Bảng Công Việc thành công");
                    var url = String.Empty;
                    if (isNew)
                    {
                        url = Url.Action("ViewBoard", new { boardcode = board.Code });
                    }
                    else
                    {
                        url = Url.Action("Index");
                    }
                    return Json(new { boardid = board.ID, success = true, message = TempData["message"], redirectUrl = url }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    TempData["message"] = string.Format("Lưu không thành công");
                    return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                TempData["message"] = string.Format("Lưu không thành công");
                return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult EditBoard(int id)
        {
            var board = boardRepository.Detail(id);
            KanbanInfoModel boardInfo = new KanbanInfoModel
            {
                Id = board.ID,
                Title = board.Title,
                Code = board.Code,
                BoardMemberList = board.Members.Select(m => m.UID.ToString()).ToArray()
            };

            return PartialView("EditBoardModal", boardInfo);
        }

        [HttpGet]
        public ActionResult GetUserBoard()
        {
            var data = userBoardList.Select(b => new { Id = b.Id, Title = b.Title, Code = b.Code }).ToArray();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetUserWorkflow()
        {
            var data = userBoardList.Select(b => new { Id = b.Workflow.ID, Title = b.Workflow.Name }).Distinct().ToArray();
            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }
    }
}