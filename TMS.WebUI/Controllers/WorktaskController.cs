using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using TMS.Domain.Common;
using TMS.WebApp.Models;
using System.IO;
using TikaOnDotNet.TextExtraction;
using System.Collections.Generic;
using TMS.WebApp.HtmlHelpers;
using System.Web.Hosting;
using System.Web.Configuration;
using TMS.WebApp.Services;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Globalization;

namespace TMS.WebApp.Controllers
{
    [Authorize]
    public class WorktaskController : BaseController
    {
        private IWorktaskRepository repository;
        private ICategoryRepository catRepository;
        private IAccountRepository accRepository;
        private IBoardRepository boardRepository;
        public int PageSize = 10;
        private IList<KanbanInfoModel> userBoardList;
        // Constructor
        public WorktaskController(IWorktaskRepository worktaskRepository, ICategoryRepository catRepository, IAccountRepository accRepository, IBoardRepository boardRepository) : base()
        {
            this.repository = worktaskRepository;
            this.catRepository = catRepository;
            this.accRepository = accRepository;
            this.boardRepository = boardRepository;
        }
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);
            userBoardList = KanbanService.GetInstance().UserBoardList(User.Identity.Name);
        }

        // GET: Worktask
        [Authorize]
        public ActionResult Index(string boardcode)
        {
            int[] boardIdList = userBoardList.Select(b => b.Id).ToArray();
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
                var tasklist = repository.Worktasks.Where(m => m.BoardID == board.ID).OrderBy(w => w.WorktaskID);
                foreach (var task in tasklist)
                {
                    task.loadMetaInfo(repository, accRepository);
                }
                WorktaskListViewModel model = new WorktaskListViewModel
                {
                    Worktasks = tasklist,
                    Board = new KanbanInfoModel
                    {
                        Id = board.ID,
                        Title = board.Title,
                        Code = board.Code,
                        BoardMemberList = board.Members.Select(m => m.UID.ToString()).ToArray(),
                        Owner = board.Owner,
                        Workflow = board.Workflow
                    }
                };
                this.Response.Cookies.Remove(User.Identity.Name + "lastviewedboard");
                this.Response.Cookies.Add(new System.Web.HttpCookie(User.Identity.Name + "lastviewedboard", board.ID.ToString()));
                this.Response.Cookies.Get(User.Identity.Name + "lastviewedboard").Expires = DateTime.Now.AddDays(1);
                return View(model);
            }
            else
            {
                return this.RedirectToAction("Index", "Kanban");
            }

        }

        public ActionResult Create()
        {
            return PartialView("CreateModal", new Worktask());
        }

        public JsonResult GetAccount(int role = 0)
        {
            int[] allowedStatus ;
            if (role == 0)
                allowedStatus = new[] { 2, 3 };
            else
                allowedStatus = new[] { role };
            var result = repository.Accounts.Where(s => allowedStatus.Contains(s.Role)).ToList().Select(x => new AutoComplete
            {
                name = x.DisplayName2,
                code = x.UserName,
                id = x.UID
            });
            return Json(new { source = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTaskList(string boardcode, int taskid, string prompt)
        {
            prompt = prompt.ToLower();
            int[] boardIdList = userBoardList.Select(b => b.Id).ToArray();
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
                var result = repository.Worktasks.Where(t => t.BoardID == board.ID && t.WorktaskID != taskid && t.Title.ToLower().Contains(prompt)).Select(x => new AutoComplete
                {
                    name = x.Title,
                    code = x.Identify,
                    id = x.WorktaskID
                });
                return Json(new { source = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { source = new List<AutoComplete>() }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult InsertTask(Worktask worktask)
        {
            int currentUID = AccountHelper.GetCurrentUser(User.Identity.Name).UID;
            ElasticIndexService eService = new ElasticIndexService();
            Worktask oldObject = null;
            if (ModelState.IsValid)
            {
                var isNew = false;

                // Assign Current user AS Owner, Assignee, Reporter
                if (worktask.Owner == 0) worktask.Owner = currentUID;
                if (worktask.Assignee == 0) worktask.Assignee = currentUID;
                if (worktask.Reporter == null || worktask.Reporter == 0) worktask.Reporter = currentUID;
                if (worktask.TaskType == 0) worktask.TaskType = int.Parse(CategoryService.GetInstance().GetCategoryList(Contain.CatType.Category).ElementAt(1).Value);
                if (worktask.Priority == 0) worktask.Priority = int.Parse(CategoryService.GetInstance().GetCategoryList(Contain.CatType.Priority).ElementAt(1).Value);

                if (worktask.WorktaskID == 0)
                {
                    isNew = true;
                    worktask.BoardID = this.GetLastViewedBoardId(User.Identity.Name);
                    worktask.TaskMetas = new List<WorkTaskMetas>();
                    worktask = WorktaskService.GetInstance().loadMetaInfoFromRequest(worktask, Request);
                    worktask.loadMetaInfo(repository, accRepository);
                    // create workflow instance
                    Board board = this.boardRepository.Get(worktask.BoardID);
                    Workflow wf = board.Workflow;
                    State firstState = wf.States.Where(s => s.Type == (int)Contain.StateType.Init).FirstOrDefault();
                    WorkflowInstance wfi = new WorkflowInstance()
                    {
                        WorkflowID = wf.ID,
                        CurrentStateID = (firstState != null ? firstState.ID : 0)
                    };
                    worktask.WorkflowInstance = wfi;
                    worktask.Status = firstState.ID;
                }
                else
                {
                    worktask = WorktaskService.GetInstance().loadMetaInfoFromRequest(worktask, Request);
                    worktask.loadMetaInfo(repository, accRepository);
                    oldObject = repository.Worktasks.Where(x => x.WorktaskID == worktask.WorktaskID).FirstOrDefault();
                    oldObject = repository.Detail(oldObject.WorktaskID);
                    oldObject.loadMetaInfo(repository, accRepository);
                }
                var diff = new List<TaskHistory>();
                if (repository.SaveWorktask(worktask))
                {
                    //save history worktask when create new
                    string action = "Cập nhật";
                    if (isNew)
                    {
                        // Save data path for the new task
                        string dataPathRoot = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["DataDirectoryRoot"]);
                        string taskDataPath = Path.Combine(dataPathRoot, worktask.WorktaskID.ToString());
                        worktask.DataPath = taskDataPath;
                        Directory.CreateDirectory(taskDataPath);
                        action = "Tạo mới";
                    }
                    List<DiffInfo> diffInfos = worktask.Compare(oldObject);
                    diff = CompareObjExtensions.GenerateDiff(action, diffInfos, currentUID, worktask.WorktaskID);
                    repository.SaveHistory(diff);
                    
                    foreach (var item in worktask.Attachment)
                    {
                        var text = String.Empty;
                        if (System.IO.File.Exists(item.StoredPath))
                            text = new TextExtractor().Extract(item.StoredPath).Text;
                        item.Metadata = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
                    }
                    if (repository.SaveAttachmentFile(worktask))
                    {
                        //save history 
                        diff = CompareObjExtensions.DetailedCompare(null, worktask.Attachment, currentUID, worktask.WorktaskID, isNew);
                        if (repository.SaveHistory(diff))
                        {
                            // Save Index in Elastic

                            eService.CreateSingleIndex(repository.Detail(worktask.WorktaskID));

                            TempData["message"] = string.Format("Thông tin công việc {0} lưu thành công!", worktask.Identify);
                            return Json(new
                            {
                                taskId = worktask.WorktaskID,
                                attachment = worktask.Attachment.Select(x => new { AttachmentID = x.AttachmentID, Name = x.Name }),
                                success = true,
                                redirectUrl = Url.Action("Detail", new { taskcode = worktask.Identify })
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    // Save Index in Elastic
                    eService.CreateSingleIndex(repository.Detail(worktask.WorktaskID));

                    TempData["message"] = string.Format("Thông tin công việc {0} lưu thành công!", worktask.Identify);
                    return Json(new
                    {
                        taskId = worktask.WorktaskID,
                        success = true,
                        redirectUrl = Url.Action("Detail", new { taskcode = worktask.Identify })
                    }, JsonRequestBehavior.AllowGet);
                }
                TempData["message"] = string.Format("Lưu không thành công");
                return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                TempData["message"] = string.Format(GetModelErrorMessages(ModelState));
                return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult InsertTask2(Worktask worktask)
        {
            // NOT ModelState.IsValid
            if (!ModelState.IsValid)
            {
                TempData["message"] = string.Format(GetModelErrorMessages(ModelState));
                return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
            }

            // ModelState.IsValid
            int currentUID = AccountHelper.GetCurrentUser(User.Identity.Name).UID;
            ElasticIndexService eService = new ElasticIndexService();

            // Assign Current user AS Owner, Assignee, Reporter
            if (worktask.Owner == 0) worktask.Owner = currentUID;
            if (worktask.Assignee == 0) worktask.Assignee = currentUID;
            if (worktask.Reporter == 0) worktask.Reporter = currentUID;
            if (worktask.TaskType == 0) worktask.TaskType = int.Parse(CategoryService.GetInstance().GetCategoryList(Contain.CatType.Category).ElementAt(1).Value);
            if (worktask.Priority == 0) worktask.Priority = int.Parse(CategoryService.GetInstance().GetCategoryList(Contain.CatType.Priority).ElementAt(1).Value);

            string[] selectedtaskcodes = Request.Params.GetValues("RelatedTaskValue[]");
            int relatedtaskmetatype = catRepository.Categories.Where(c => c.CatType == (int)Contain.CatType.MetaType && c.CategoryName.Equals("metastring")).FirstOrDefault().CatValue;
            var isNew = false;

            // Set relate task
            if (worktask.WorktaskID == 0)
            {
                isNew = true;
                worktask.BoardID = this.GetLastViewedBoardId(User.Identity.Name);

                // create workflow instance
                Board board = this.boardRepository.Get(worktask.BoardID);
                Workflow wf = board.Workflow;
                State firstState = wf.States.Where(s => s.Type == (int)Contain.StateType.Init).FirstOrDefault();
                WorkflowInstance wfi = new WorkflowInstance()
                {
                    WorkflowID = wf.ID,
                    CurrentStateID = (firstState != null ? firstState.ID : 0)
                };
                worktask.WorkflowInstance = wfi;
                worktask.Status = firstState.ID;
            }

            worktask.TaskMetas = new List<WorkTaskMetas>();
            if (selectedtaskcodes != null && selectedtaskcodes.Length > 0)
            {
                foreach (string code in selectedtaskcodes)
                {
                    worktask.TaskMetas.Add(new WorkTaskMetas()
                    {
                        MetaKey = "RelatedTask",
                        MetaValue = code,
                        MetaType = relatedtaskmetatype
                    });
                }
            }
            else
            {
                if (!isNew)
                {
                    var oldObject = repository.Worktasks.Where(x => x.WorktaskID == worktask.WorktaskID).FirstOrDefault();
                    oldObject = repository.Detail(oldObject.WorktaskID);

                    worktask.TaskMetas = oldObject.TaskMetas;
                }
            }

            //Save data
            if (!repository.SaveWorktask(worktask))
            {
                TempData["message"] = string.Format("Lưu không thành công");
                return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
            }


            var diff = new List<TaskHistory>();

            //save history worktask when create new
            if (isNew)
            {
                // Save data path for the new task
                string dataPathRoot = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["DataDirectoryRoot"]);
                string taskDataPath = Path.Combine(dataPathRoot, worktask.WorktaskID.ToString());
                worktask.DataPath = taskDataPath;
                Directory.CreateDirectory(taskDataPath);


                diff = CompareObjExtensions.DetailedCompare(null, worktask, currentUID, worktask.WorktaskID, isNew);
            }

            repository.SaveHistory(diff);

            //Save attach files
            foreach (var item in worktask.Attachment)
            {
                var text = String.Empty;
                if (System.IO.File.Exists(item.StoredPath))
                    text = new TextExtractor().Extract(item.StoredPath).Text;
                item.Metadata = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
            }

            if (repository.SaveAttachmentFile(worktask)) // << Cái này không save được thì có rolback không?
            {
                //save history 
                diff = CompareObjExtensions.DetailedCompare(null, worktask.Attachment, currentUID, worktask.WorktaskID, isNew);
                if (!repository.SaveHistory(diff))
                {
                    TempData["message"] = string.Format("Lưu không thành công");
                    return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
                }
            }

            // Save Index in Elastic
            eService.CreateSingleIndex(repository.Detail(worktask.WorktaskID));

            TempData["message"] = string.Format("Thông tin công việc {0} lưu thành công!", worktask.Identify);
            return Json(new
            {
                taskId = worktask.WorktaskID,
                success = true,
                redirectUrl = Url.Action("Detail", new { taskcode = worktask.Identify })
            }, JsonRequestBehavior.AllowGet);
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public ActionResult MoveTask(MoveTaskViewModel movetask)
        {
            int currentUID = AccountHelper.GetCurrentUser(User.Identity.Name).UID;
            if (ModelState.IsValid)
            {
                Worktask newObject = repository.Worktasks.Where(x => x.WorktaskID == movetask.WorktaskID).FirstOrDefault();
                newObject = repository.Detail(newObject.WorktaskID);
                newObject.loadMetaInfo(repository, accRepository);
                Worktask oldObject = newObject.clone();
                if (newObject != null)
                {
                    newObject.BoardID = movetask.BoardID;
                    newObject.WorkflowInstance.CurrentStateID = movetask.StateID;
                    List<DiffInfo> diffInfos = newObject.Compare(oldObject);
                    var diff = CompareObjExtensions.GenerateDiff("Cập nhật", diffInfos, currentUID, oldObject.WorktaskID);
                    if (repository.SaveWorktask(newObject))
                    {
                        if (repository.SaveHistory(diff))
                        {
                            TempData["message"] = string.Format("Chuyển bảng công việc cho {0} thành công!", oldObject.Identify);
                            return RedirectToAction("Detail", new { taskcode = oldObject.Identify });
                        }
                    }
                }
                return View(movetask);
            }
            else
            {
                TempData["message"] = string.Format(GetModelErrorMessages(ModelState));
                return View(movetask);
            }
        }
        //Action result for ajax call
        [HttpPost]
        public ActionResult GetStateByBoardId(int boardid)
        {
            List<State> objState = new List<State>();
            Board board = boardRepository.Get(boardid);
            objState = board.Workflow.States.ToList();
            SelectList obgstate = new SelectList(objState, "ID", "Name", 0);
            return Json(obgstate);
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize]
        public ActionResult Edit(WorktaskViewModel worktask)
        {
            int currentUID = AccountHelper.GetCurrentUser(User.Identity.Name).UID;
            if (ModelState.IsValid)
            {
                //Compare data of new and old object for saving to history
                var oldObject = repository.Worktasks.Where(x => x.WorktaskID == worktask.WorktaskID).FirstOrDefault();
                oldObject = repository.Detail(oldObject.WorktaskID);
                if (oldObject != null)
                {
                    oldObject.loadMetaInfo(repository, accRepository);
                    worktask = (WorktaskViewModel)WorktaskService.GetInstance().loadMetaInfoFromRequest(worktask, Request);
                    worktask.loadMetaInfo(repository, accRepository);
                    List<DiffInfo> diffInfos = worktask.Compare(oldObject);
                    var diff = CompareObjExtensions.GenerateDiff("Cập nhật", diffInfos, currentUID, oldObject.WorktaskID);
                    if (repository.SaveWorktask(worktask))
                    {
                        if (repository.SaveHistory(diff))
                        {
                            TempData["message"] = string.Format("Thông tin công việc {0} lưu thành công!", worktask.Identify);
                            return Json(new
                            {
                                taskId = worktask.WorktaskID,
                                success = true,
                                message = TempData["message"],
                                redirectUrl = Url.Action("Detail", new { taskcode = worktask.Identify })
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                TempData["message"] = string.Format("Lưu không thành công");
                return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                TempData["message"] = string.Format(GetModelErrorMessages(ModelState));
                return Json(new { taskId = 0, success = false, message = TempData["message"] }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        // Main Detail
        public ViewResult Detail(string taskcode)
        {

            Worktask worktask = repository.Detail(taskcode);

            if (worktask == null)
            {
                return View("Index");
            }
            worktask.loadMetaInfo(repository, accRepository);
            foreach (var item in worktask.Comment)
            {
                //item.Username = repository.Accounts.Where(x => x.UID == item.AccountID).FirstOrDefault().UserName;
                item.RelativeTime = GetRelativeTime(item.CommentDate);
            }
            List<Event> eventlist = new List<Event>();
            foreach (var transition in worktask.WorkflowInstance.CurrentState.TransitionTo)
            {
                eventlist.Add(transition.Event);
            }
            ViewBag.NextStatus = HTMLHelperExtensions.GenerateStatusHTMLCode(worktask.WorkflowInstance.CurrentStateID, worktask.WorktaskID, (IEnumerable<Event>)eventlist);

            var attachmentModel = worktask.Attachment.ToList();

            return View(worktask);
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveComment(Comment comment)
        {
            comment.AccountID = AccountHelper.GetCurrentUser(User.Identity.Name).UID;
            comment.CommentDate = DateTime.Now;
            if (repository.SaveComment(comment))
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Quản lý, Quản trị")]
        public ActionResult Delete(int id)
        {
            Worktask deletedProduct = repository.DeleteWorktask(id);
            if (deletedProduct != null)
            {
                TempData["message"] = string.Format("{0} đã xóa.",
                deletedProduct.Identify);
            }
            return RedirectToAction("Index");
        }

        // Upload file
        public ActionResult SaveUploadedFile()
        {
            string pathFile = "";
            string fName = "";
            string guid = "";
            try
            {
                if (Request.Files.Count == 0)
                {
                    return Json(new { Message = "Không thấy tập tin được tải lên. Xin vui lòng thử lại!", success = false });
                }


                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    if (file != null && file.ContentLength > 0)
                    {
                        // START --- Get server root path ---
                        string dataPathRoot = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["DataDirectoryRoot"]);

                        if (!Directory.Exists(dataPathRoot)) Directory.CreateDirectory(dataPathRoot);
                        // END  --- Get server root path ---

                        guid = Guid.NewGuid().ToString();

                        string filePath = Path.Combine(dataPathRoot, string.Format("{0}.{1}", guid, file.FileName));

                        file.SaveAs(filePath);

                        //// START - EXTRACT PDF TO OCR FOLDER
                        //string ocrExtractPath = WebConfigurationManager.AppSettings["ocrExtractFolder"];
                        //if (ocrExtractPath != null)
                        //{
                        //    if (!Directory.Exists(ocrExtractPath)) Directory.CreateDirectory(ocrExtractPath);

                        //    file.SaveAs(ocrExtractPath);

                        //}
                        //// END - EXTRACT PDF TO OCR FOLDER

                        pathFile = filePath;
                        fName = file.FileName;



                    }
                }
                return Json(new { Message = pathFile, success = true, Name = fName, guid = guid });
            }
            catch
            {
                return Json(new { Message = "Có lỗi khi lưu tập tin được tải lên!", success = false });
            }
        }

        [HttpPost]
        public JsonResult DeleteUploaderFile(string path, int id, int taskId)
        {
            try
            {
                //insert page remove attachment
                if (id == 0)
                {
                    System.IO.File.Delete(path);
                    return Json(new { message = "Xóa tập tin thành công!", success = true }, JsonRequestBehavior.AllowGet);
                }
                if (repository.DeleteAttachment(id))
                {
                    var oldObj = repository.Attachments.Where(x => x.AttachmentID == id).FirstOrDefault();
                    var diff = CompareObjExtensions.DetailedCompare(oldObj, null, AccountHelper.GetCurrentUser(User.Identity.Name).UID, taskId, false);
                    if (repository.SaveHistory(diff))
                    {
                        System.IO.File.Delete(path);
                        return Json(new { message = "Xóa tập tin thành công!", success = true }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { message = "Có lỗi khi xóa tập tin!", success = false }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { message = "Có lỗi khi xóa tập tin!", success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public string GetRelativeTime(DateTime commentDate)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.Now.Ticks - commentDate.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "1 giây trước" : ts.Seconds + " giây trước";

            if (delta < 2 * MINUTE)
                return "1 phút trước";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " phút trước";

            if (delta < 90 * MINUTE)
                return "1 giờ trước";

            if (delta < 24 * HOUR)
                return ts.Hours + " giờ trước";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " ngày trước";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "một tháng trước" : months + " tháng trước";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "một năm trước" : years + " năm trước";
            }
        }

        public void DownloadFile(string path)
        {
            try
            {
                FileInfo file = new FileInfo(path);
                byte[] fileConent = System.IO.File.ReadAllBytes(path);
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.ClearHeaders();

                // START: Make correct file name
                var fileName = file.Name.Substring(file.Name.Split('.')[0].Length + 1);
                // END  : Make correct file name

                context.Response.AddHeader("Content-Disposition", string.Format("attachment; filename={0}", file.Name));
                context.Response.AddHeader("Content-Length", file.Length.ToString());
                context.Response.ContentType = "text/plain";
                context.Response.BinaryWrite(fileConent);
                context.Response.Flush();
                context.Response.End();
            }
            catch
            {

            }
        }

        public ActionResult UpdateStatus(int worktaskId, int nextEvent)
        {
            var worktask = repository.Worktasks.Where(x => x.WorktaskID == worktaskId).FirstOrDefault();
            worktask = repository.Detail(worktask.WorktaskID);
            State curState = worktask.WorkflowInstance.CurrentState;
            State nextState = null;
            int currentUID = AccountHelper.GetCurrentUser(User.Identity.Name).UID;
            Transition transition = curState.TransitionTo.Where(t => t.EventID == nextEvent).FirstOrDefault();
            if (transition != null)
            {
                nextState = transition.ToState;
            }
            if (curState.Type == (int)Contain.StateType.Init && nextState.Type != (int)Contain.StateType.Init)
            {
                worktask.ActualStartDate = DateTime.Now;
            }
            if (curState.Type != (int)Contain.StateType.End && nextState.Type == (int)Contain.StateType.End)
            {
                worktask.ActualEndDate = DateTime.Now;
            }
            var diff = new List<TaskHistory>();
            Worktask oldObject = worktask.clone();
            // go to next state
            worktask.WorkflowInstance.CurrentState = nextState;
            worktask.Status = nextState.ID;
            if (repository.SaveWorktask(worktask))
            {
                List<DiffInfo> diffInfos = worktask.Compare(oldObject);
                diff = CompareObjExtensions.GenerateDiff("Cập nhật", diffInfos, currentUID, oldObject.WorktaskID);
                repository.SaveHistory(diff);
                TempData["message"] = string.Format("Thông tin công việc {0} lưu thành công!", worktask.Identify);
                return RedirectToAction("Detail", new { taskcode = worktask.Identify });
            }
            return View();
        }


    }
}