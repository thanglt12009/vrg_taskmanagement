using System;
using System.Collections.Generic;
using TMS.Domain.Abstract;
using TMS.Domain.Entities;
using System.Linq;
using System.Data.Entity.Validation;
using System.Web.Hosting;
using System.IO;
using System.Web.Configuration;
using System.Data.Entity;
using TMS.Domain.Common;

namespace TMS.Domain.Concrete
{
    public class EFWorktaskRepository : BaseRepository, IWorktaskRepository
    {
        public IEnumerable<Worktask> Worktasks
        {
            get
            {
                return context.Worktasks.Where(w=>w.DeleteFlag == false);
            }
        }



        public IEnumerable<Worktask> FullWorktasks
        {
            get
            {
                return context.Worktasks;
            }
        }

        public IEnumerable<Account> Accounts
        {
            get
            {
                return context.Accounts;
            }
        }

        public IEnumerable<Attachment> Attachments
        {
            get
            {
                return context.Attachments;
            }
        }

        public Worktask Detail(int taskID)
        {
            try
            {
                var worktask = context.Worktasks.Where(p => p.WorktaskID == taskID).FirstOrDefault();
                worktask.Attachment = context.Attachments.Where(p => p.WorktaskID == worktask.WorktaskID && p.DeleteFlag == false).ToList();
                worktask.Comment = context.Comments.Where(p => p.TaskID == worktask.WorktaskID).ToList();
                worktask.TaskHistories = context.Histories.Where(p => p.TaskID == worktask.WorktaskID).ToList();
                worktask.TaskMetas = context.WorkTaskMetas.Where(m => m.WorktaskID == worktask.WorktaskID).ToList();
                return worktask;
            }

            catch
            {

                throw;
            }

        }

        public Worktask Detail(string taskcode)
        {
            try
            {
                var worktask = context.Worktasks.Where(p => p.Identify == taskcode).FirstOrDefault();
                worktask.Attachment = context.Attachments.Where(p => p.WorktaskID == worktask.WorktaskID && p.DeleteFlag == false).ToList();
                worktask.Comment = context.Comments.Where(p => p.TaskID == worktask.WorktaskID).ToList();
                worktask.TaskHistories = context.Histories.Where(p => p.TaskID == worktask.WorktaskID).ToList();
                worktask.TaskMetas = context.WorkTaskMetas.Where(m => m.WorktaskID == worktask.WorktaskID).ToList();
                return worktask;
            }
            catch
            {

                return null;
            }
        }

        public Worktask DetailInfo(Worktask task)
        {
            try
            {
                task.AssigneeAcc = context.Accounts.Where(a => a.UID == task.Assignee).FirstOrDefault();
                task.OwnerAcc = context.Accounts.Where(a => a.UID == task.Owner).FirstOrDefault();
                task.ReporterAcc = context.Accounts.Where(a => a.UID == task.Reporter).FirstOrDefault();
                task.TaskMetas = context.WorkTaskMetas.Where(m => m.WorktaskID == task.WorktaskID).ToList();
                return task;
            }
            catch
            {

                throw;
            }

        }

        #region SAVE WORKTASK - CREATE BY TUNGNT
        public bool SaveWorktask(Worktask worktask)
        {
            var result = true;
            try
            {
                if (worktask.WorktaskID == 0)
                {
                    var board = context.Boards.Find(worktask.BoardID);
                    string boardcode = board.Code.ToLower();
                    var lastTask = context.Worktasks.Where(t => t.Identify.StartsWith(boardcode)).OrderByDescending(t => t.Identify).FirstOrDefault();
                    string taskcode = String.Empty;
                    if (lastTask != null)
                    {
                        int c = 0;
                        taskcode = lastTask.Identify.Replace(boardcode, "");
                        if (int.TryParse(taskcode, out c))
                        {
                            c += 1;
                            taskcode = board.Code.ToLower() + c.ToString().PadLeft(6, '0');
                        }
                        else
                        {
                            taskcode = String.Empty;
                        }
                    }
                    if (taskcode.Length == 0)
                    {
                        int c = 1;
                        taskcode = board.Code.ToLower() + c.ToString().PadLeft(6, '0');
                    }
                    worktask.CreationDate = DateTime.Now;
                    worktask.LastUpdateDateTime = DateTime.Now;
                    worktask.Identify = taskcode;
                    //worktask.Owner = "Current User";
                    context.Worktasks.Add(worktask);
                }
                else
                {
                    int relatedtaskmetatype = context.Categories.Where(c => c.CatType == (int)Contain.CatType.MetaType && c.CategoryName.Equals("metastring")).FirstOrDefault().CatValue;
                    Worktask dbEntry = context.Worktasks.Where(t => t.WorktaskID == worktask.WorktaskID).FirstOrDefault();
                    Worktask atask;
                    if (dbEntry != null)
                    {

                        dbEntry.TaskType = worktask.TaskType;
                        dbEntry.Title = worktask.Title;
                        dbEntry.Description = worktask.Description;
                        dbEntry.Assignee = worktask.Assignee;
                        dbEntry.PlannedStartDate = worktask.PlannedStartDate;
                        dbEntry.PlannedEndDate = worktask.PlannedEndDate;
                        dbEntry.Status = worktask.Status;
                        dbEntry.ActualStartDate = worktask.ActualStartDate;
                        dbEntry.ActualEndDate = worktask.ActualEndDate;
                        dbEntry.Priority = worktask.Priority;
                        dbEntry.LastUpdateDateTime = DateTime.Now;

                        List<WorkTaskMetas> updatedMeta = new List<WorkTaskMetas>();
                        updatedMeta.AddRange(worktask.TaskMetas.Select(m => m));
                        List<WorkTaskMetas> removedMeta = new List<WorkTaskMetas>();
                        foreach (WorkTaskMetas meta in dbEntry.TaskMetas)
                        {
                            if (updatedMeta.Count >= 0)
                            {
                                bool found = false;
                                foreach (var item in updatedMeta)
                                {
                                    if (item.MetaKey.Equals(meta.MetaKey))
                                    {
                                        found = true;
                                        meta.MetaKey = item.MetaKey;
                                        meta.MetaType = item.MetaType;
                                        meta.MetaValue = item.MetaValue;
                                        break;
                                    }
                                }
                                if (!found)
                                    removedMeta.Add(meta);
                            }
                        }
                        foreach (WorkTaskMetas removed in removedMeta)
                        {
                            dbEntry.TaskMetas.Remove(removed);
                        }
                        if (updatedMeta.Count > 0)
                        {
                            foreach (WorkTaskMetas item in updatedMeta)
                            {
                                bool found = false;
                                foreach (WorkTaskMetas meta in dbEntry.TaskMetas)
                                {
                                    if (item.MetaKey.Equals(meta.MetaKey))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found)
                                {
                                    dbEntry.TaskMetas.Add(new WorkTaskMetas()
                                    {
                                        MetaKey = item.MetaKey,
                                        MetaValue = item.MetaValue,
                                        MetaType = item.MetaType
                                    });
                                }
                            }
                        }
                        context.Entry(dbEntry).State = System.Data.Entity.EntityState.Modified;
                        foreach (var item in removedMeta)
                        {
                            if (item.MetaKey.Equals("RelatedTask"))
                            {
                                string code = item.MetaValue;
                                this.DeleteMeta(item.MetaID);
                                atask = this.Detail(code);
                                if (atask != null)
                                {
                                    var old = atask.TaskMetas.Where(m => m.MetaKey == "RelatedTask" && m.MetaValue == worktask.Identify).FirstOrDefault();
                                    if (old != null)
                                    {
                                        atask.TaskMetas.Remove(old);
                                        this.DeleteMeta(old.MetaID);
                                    }
                                }
                            } else
                            {
                                this.DeleteMeta(item.MetaID);
                            }
                        }
                        foreach (var ameta in worktask.TaskMetas)
                        {
                            if (ameta.MetaKey == "RelatedTask")
                            {
                                atask = this.Detail(ameta.MetaValue);
                                if (atask != null)
                                {
                                    var old = atask.TaskMetas.Where(m => m.MetaKey == "RelatedTask" && m.MetaValue == worktask.Identify).FirstOrDefault();
                                    if (old == null)
                                    {
                                        atask.TaskMetas.Add(new WorkTaskMetas()
                                        {
                                            MetaKey = "RelatedTask",
                                            MetaValue = worktask.Identify,
                                            MetaType = relatedtaskmetatype
                                        });
                                        context.Entry(atask).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                        }
                    }
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
        #endregion

        #region SAVE WORKTASK - EDIT BY THUTTD
        //public bool SaveWorktask(Worktask worktask)
        //{
        //    var result = true;
        //    try
        //    {
        //        if (worktask.WorktaskID == 0)
        //        {
        //            worktask.CreationDate = DateTime.Now;
        //            worktask.Identify = String.Format("{0,9}", new Random().Next(10, 99) + DateTime.Now.ToString("yyMMdd"));
        //            // Attach Owner
        //            Account owner = context.Accounts.Find(worktask.Owner);
        //            context.Accounts.Attach(owner);
        //            worktask.OwnerAcc

        //            context.Worktasks.Add(worktask);


        //            Department dpt = context.Departments.Find(account.DeptID);
        //            context.Departments.Attach(dpt);
        //            account.Department = dpt;
        //            context.Accounts.Add(account);

        //        }
        //        else
        //        {
        //            Worktask dbEntry = context.Worktasks.Find(worktask.WorktaskID);
        //            if (dbEntry != null)
        //            {
        //                dbEntry.Title = worktask.Title;
        //                dbEntry.Description = worktask.Description;
        //                dbEntry.Assignee = worktask.Assignee;
        //                dbEntry.PlannedStartDate = worktask.PlannedStartDate;
        //                dbEntry.PlannedEndDate = worktask.PlannedEndDate;
        //                dbEntry.Status = worktask.Status;
        //                dbEntry.ActualStartDate = worktask.ActualStartDate;
        //                dbEntry.ActualEndDate = worktask.ActualEndDate;
        //            }
        //        }
        //        context.SaveChanges();
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        result = false;
        //        foreach (var eve in e.EntityValidationErrors)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
        //                eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //            foreach (var ve in eve.ValidationErrors)
        //            {
        //                System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
        //                    ve.PropertyName, ve.ErrorMessage);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        result = false;
        //        //e.GetBaseException();
        //    };
        //    return result;
        //}
        #endregion

        public bool SaveComment(Comment comment)
        {
            var result = true;
            try
            {
                context.Comments.Add(comment);
                context.SaveChanges();
            }
            catch
            {
                result = false;
                //e.GetBaseException();
            };
            return result;
        }

        public bool SaveAttachmentFile(Worktask worktask) {
            var result = true;
            try
            {
                if (worktask.WorktaskID != 0)
                {
                    bool ocrEnabled = Convert.ToBoolean(WebConfigurationManager.AppSettings["ocrEnabled"]);
                    var dataPath = context.Worktasks.Where(w => w.WorktaskID == worktask.WorktaskID).Select(x => x.DataPath).FirstOrDefault();

                    foreach (var item in worktask.Attachment)
                    {
                        //CuongND: This should be do in Controller or Service
                        //But I need to place here in combine with old code
                        //Moving Files from root to correct folder
                        // START --- Move file to new Path ----

                        string oldFilePath = item.StoredPath;
                        string fileName = Path.GetFileName(item.StoredPath);
                        string filePath = Path.GetDirectoryName(oldFilePath);
                        //var entry = context.Attachments.Where(a => a.AttachmentID == item.AttachmentID).FirstOrDefault();


                        if (dataPath != filePath)
                        {
                            if (null == dataPath)
                            {
                                string taskDataPath = Path.Combine(filePath, worktask.WorktaskID.ToString());
                                dataPath = taskDataPath;
                            }
                            string newFilePath = Path.Combine(dataPath, fileName);

                            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);

                            File.Move(oldFilePath, newFilePath);
                            item.StoredPath = newFilePath;

                            item.WorktaskID = worktask.WorktaskID;
                            context.Attachments.Add(item);
                            context.SaveChanges();

                            // START - EXTRACT PDF TO OCR FOLDER
                            // Start if user upload a PDF file *** (please change logic later)

                            if (ocrEnabled)
                            {
                                var ocrExtension = WebConfigurationManager.AppSettings["ocrFileExtension"];
                                if (Path.GetExtension(fileName).ToUpper() == ocrExtension.ToUpper())
                                {
                                    string ocrExtractPath = WebConfigurationManager.AppSettings["ocrExtractFolder"];
                                    if (ocrExtractPath != null)
                                    {
                                        // Move to a folder with name OCR_FolderData\TaskID\
                                        var ocrExtractTaskPath = Path.Combine(ocrExtractPath, worktask.WorktaskID.ToString());

                                        if (!Directory.Exists(ocrExtractTaskPath)) Directory.CreateDirectory(ocrExtractTaskPath);

                                        // Chop the GUID prefix the file
                                        fileName = fileName.Substring(fileName.Split('.')[0].Length + 1);
                                        fileName = worktask.WorktaskID.ToString() + "_" + item.AttachmentID + "_" + fileName;

                                        var ocrExtractTaskFile = Path.Combine(ocrExtractTaskPath, fileName);

                                        // Copy to new location
                                        File.Copy(newFilePath, ocrExtractTaskFile);
                                    }
                                }
                                // END - EXTRACT PDF TO OCR FOLDER
                            }
                        }
                        // END   --- Move file to new Path ----
                    }

                    // Save change in Worktask again
                }
                context.SaveChanges();
            }
            catch
            {
                result = false;
            }

            return result;
        } 

        public bool DeleteAttachment(int attachmentID)
        {
            try
            {
                Attachment dbEntry = context.Attachments.Where(x => x.AttachmentID == attachmentID).FirstOrDefault();
                if (dbEntry != null)
                {
                    dbEntry.DeleteFlag = true;
                    context.SaveChanges();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool DeleteMeta(int metaID, bool commit = false)
        {
            try
            {
                WorkTaskMetas dbEntry = context.WorkTaskMetas.Where(x => x.MetaID == metaID).FirstOrDefault();
                if (dbEntry != null)
                {
                    context.Entry(dbEntry).State = System.Data.Entity.EntityState.Deleted;
                    if (commit)
                    {
                        context.SaveChanges();
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public Worktask DeleteWorktask(int id)
        {
            Worktask dbEntry = context.Worktasks.Find(id);
            if (dbEntry != null)
            {
                //remove worktasks
                //context.Worktasks.Remove(dbEntry);
                // Please set DelFlag = 1 in Production
                dbEntry.DeleteFlag = true;

                //remove attachments
                var attachments = context.Attachments.Where(x => x.WorktaskID == id).ToList();
                foreach (var item in attachments)
                {
                    // Hard delete
                    //context.Attachment.Remove(item);
                    // soft delete
                    item.DeleteFlag = true;
                }
                context.SaveChanges();
            }

            return dbEntry;
        }

        public bool SaveHistory(List<TaskHistory> history)
        {
            var result = true;
            try
            {
                foreach (var item in history)
                {
                    context.Histories.Add(item);
                    context.SaveChanges();
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Add attachment into Worktask. 
        /// Becareful to save Attachment to db BEFORE call this function
        /// </summary>
        /// <param name="worktaskId">Task ID of worktask</param>
        /// <param name="attachment">Attachment instance</param>
        /// <returns>True: Success, False: Incomplete</returns>
        public async System.Threading.Tasks.Task<string> SaveAttachmentIntoTask(int worktaskId, Attachment attachment)
        {
            string saveResult = "";

            Worktask worktask = context.Worktasks.Where(w => w.WorktaskID == worktaskId).FirstOrDefault();

            // Make sure task is get correctly
            if (worktask != null && worktask.WorktaskID > 0)
            {
               var list=  worktask.Attachment as List<Attachment>;
                list.Add(attachment);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // ex
                    saveResult += ex.Message;
                }
            }

            return saveResult;
        }
    }
}
