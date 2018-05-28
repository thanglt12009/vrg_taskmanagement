using System.Collections.Generic;
using System.Web.Http;
using TMS.Domain.Entities;
using TMS.Domain.Abstract;
using System.Threading.Tasks;
using System.IO;
using TMS.WebApp.Models;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using System.Linq;
using System.Web.Configuration;
using System;
using TMS.Domain.Common;

namespace TMS.WebApp.Controllers
{
    public class WorktaskApiController : ApiController
    {
        private IWorktaskRepository repository;
        private ICategoryRepository catRepository;

        // Constructor
        public WorktaskApiController(IWorktaskRepository worktaskRepository, ICategoryRepository catRepository)
        {
            this.repository = worktaskRepository;
            this.catRepository = catRepository;
        }

        // GET: api/WorktaskApi
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/WorktaskApi/5
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost]
        // POST: api/WorktaskApi
        public async Task<IHttpActionResult> Post([FromBody]WorktaskSimpleModel worktask)
        {
            var oldTask = repository.Detail(worktask.WorktaskID);

            if (oldTask != null)
            {
                // Also, we need to compare object to avoid conflict.
                // Please check logic Flow here//
                State curState = oldTask.WorkflowInstance.CurrentState;
                State nextState = null;
                Transition transition = curState.TransitionTo.Where(t => t.ToStateID == worktask.Status).FirstOrDefault();
                if (transition != null)
                {
                    nextState = transition.ToState;
                }
                if (curState.Type == (int)Contain.StateType.Init && nextState.Type != (int)Contain.StateType.Init)
                {
                    oldTask.ActualStartDate = DateTime.Now;
                }
                if (curState.Type != (int)Contain.StateType.End && nextState.Type == (int)Contain.StateType.End)
                {
                    oldTask.ActualEndDate = DateTime.Now;
                }
                // go to next state
                oldTask.WorkflowInstance.CurrentState = nextState;
                oldTask.Status = nextState.ID;

                var result = repository.SaveWorktask(oldTask);

                if (result) return Ok(new { isSuccess = true, msg = "Chuyển trạng thái thành công!" });
                else return Ok(new { isSuccess = false, msg = "Có lỗi xảy ra, không thể chuyển trạng thái!" });
            }
            else
            {
                return Ok(new { isSuccess = false, msg = "Có lỗi xảy ra khi xử lý cho " + worktask.Title });
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> Upload([FromBody] UploadFileModel incomingFile)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Ok(new { isSuccess = false, message = "Not a MIME MultiPart Content." });
            }

            // Identify requested worktask
            var intTaskId = 0;
            if (!int.TryParse(incomingFile.TaskId, out intTaskId))
            {
                return NotFound();
            }
            var intAttachedId = 0;
            if (!int.TryParse(incomingFile.AttachedId, out intAttachedId))
            {
                return NotFound();
            }

            // Get the appropiate worktask by ID (int)
            Worktask worktask = repository.Detail(intTaskId);

            if (worktask == null)
            {
                return NotFound();
            }

            Attachment attached = worktask.Attachment.Where(p => p.AttachmentID == intAttachedId && p.DeleteFlag == false).FirstOrDefault();

            if (attached == null)
            {
                return NotFound();
            }

            // Await task to get file
            Stream requestStream = await Request.Content.ReadAsStreamAsync();
            var mProvider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(mProvider);

            // START --- Get server root path ---
            string dataPathRoot = HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["DataDirectoryRoot"]);

            if (!Directory.Exists(dataPathRoot)) Directory.CreateDirectory(dataPathRoot);

            string taskPath = Path.Combine(dataPathRoot, intTaskId.ToString());

            if (!Directory.Exists(taskPath)) Directory.CreateDirectory(taskPath);
            // END  --- Get server root path ---

            foreach (var file in mProvider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var fieldname = file.Headers.ContentDisposition.Name;

                if (fieldname == "pdf_file")
                {
                    var buffer = await file.ReadAsByteArrayAsync();
                    string filePath = Path.Combine(taskPath, attached.Name);

                    var saveStream = new MemoryStream(buffer);
                    saveStream.Position = 0;

                    try
                    {
                        using (var fileStream = File.Create(filePath))
                        {
                            saveStream.CopyTo(fileStream);
                        }
                    }
                    catch
                    {

                        throw;
                    }
                }
                else if (fieldname == "txt_file")
                {
                    //TODO: update metadata of existing attachment
                    var filePath = Path.Combine(taskPath, filename);
                }
            }
            return Ok();
        }
    }
}
