using TMS.Domain.Abstract;
using TMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Domain.Entities;
using System.Reflection;

namespace TMS.WebApp.Services
{
    public class WorktaskService
    {
        private IWorktaskRepository worktaskRepository;
        private ICategoryRepository catRepository;
        private static WorktaskService _instance = null;
        private object locked = new object();
        public WorktaskService(IWorktaskRepository worktaskRepository, ICategoryRepository catRepository)
        {
            this.worktaskRepository = worktaskRepository;
            this.catRepository = catRepository;
            if (_instance == null)
                _instance = this;
        }
        public static WorktaskService GetInstance()
        {
            return _instance;
        }
        public void ReloadData()
        {
            lock (this)
            {
                worktaskRepository.ReloadDBContext();
            }
        }
        public Worktask GetTask(int taskid)
        {
            return worktaskRepository.Detail(taskid);
        }
        public Worktask loadMetaInfoFromRequest(Worktask task, HttpRequestBase request)
        {
            int stringmetatype = catRepository.Categories.Where(c => c.CatType == (int)Contain.CatType.MetaType && c.CategoryName.Equals("metastring")).FirstOrDefault().CatValue;
            int numbermetatype = catRepository.Categories.Where(c => c.CatType == (int)Contain.CatType.MetaType && c.CategoryName.Equals("metanumber")).FirstOrDefault().CatValue;
            if (task.TaskMetas == null)
                task.TaskMetas = new List<WorkTaskMetas>();
            Worktask oldtask = null;
            if (task.WorktaskID > 0)
                oldtask = worktaskRepository.Detail(task.WorktaskID);

            Type t = task.GetType();
            List<PropertyInfo> props = t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ComparableAttribute))).ToList();
            DisplayTypeE displayType;
            MetaValueTypeE metaInfo;

            foreach (var p in props)
            {
                displayType = p.GetCustomAttribute<ComparableAttribute>().DisplayType;
                metaInfo = p.GetCustomAttribute<ComparableAttribute>().MetaInfo;
                int metaType = 0;
                switch (displayType)
                {
                    case DisplayTypeE.Text:
                        metaType = stringmetatype;
                        break;
                    case DisplayTypeE.Date:
                        metaType = stringmetatype;
                        break;
                    case DisplayTypeE.RelatedTask:
                        metaType = numbermetatype;
                        break;
                }
                switch (metaInfo)
                {
                    case MetaValueTypeE.MultipleValue:
                        string[] selectedmetavalues = request.Params.GetValues(p.Name+"Value[]");
                        if (selectedmetavalues != null && selectedmetavalues.Length > 0)
                        {
                            foreach (string code in selectedmetavalues)
                            {
                                task.TaskMetas.Add(new WorkTaskMetas()
                                {
                                    MetaKey = p.Name,
                                    MetaValue = code,
                                    MetaType = metaType
                                });
                            }
                        }
                        else
                        {
                            if (oldtask != null)
                            {
                                foreach (var item in oldtask.TaskMetas)
                                {
                                    if (item.MetaKey.Equals(p.Name))
                                    {
                                        task.TaskMetas.Add(item);
                                    }
                                }
                            }
                        }
                        break;
                    case MetaValueTypeE.SingleValue:
                        WorkTaskMetas oldmetainfo = null;
                        WorkTaskMetas metainfo = null;
                        if (oldtask != null)
                        {
                            oldmetainfo = oldtask.TaskMetas.Where(m => m.MetaKey == p.Name).FirstOrDefault();
                            
                        }
                        metainfo = task.TaskMetas.Where(m => m.MetaKey == p.Name).FirstOrDefault();
                        if (metainfo == null && oldmetainfo == null)
                            metainfo = new WorkTaskMetas();
                        else if (metainfo == null && oldmetainfo != null)
                            metainfo = oldmetainfo;
                        var reqvalue = p.GetValue(task);
                        string metavalue = null;
                        switch (displayType)
                        {
                            case DisplayTypeE.Text:
                                metavalue = (reqvalue == null ? null : reqvalue.ToString());
                                break;
                            case DisplayTypeE.Date:
                                metavalue = (reqvalue == null ? null : ((DateTime)reqvalue).ToString("yyyy-MM-dd"));
                                break;
                            case DisplayTypeE.RelatedTask:
                                metavalue = (reqvalue == null ? null : reqvalue.ToString());
                                break;
                            default:
                                metavalue = (reqvalue == null ? null : reqvalue.ToString());
                                break;
                        }

                        metainfo.MetaKey = p.Name;
                        metainfo.MetaValue = metavalue;
                        metainfo.MetaType = metaType;
                        task.TaskMetas.Add(metainfo);
                        break;
                }
            }
            return task;
        }
        public string GetTaskDisplayValue(int taskId, string value, string propname)
        {
            Worktask task = this.worktaskRepository.Detail(taskId);
            if (task != null)
            {
                if (propname.Equals("Status"))
                {
                    int id = (int)(task.WorkflowInstance.WorkflowID == null ? 0 : task.WorkflowInstance.WorkflowID);
                    return WorkflowService.GetInstance().GetState(id, int.Parse(value)).Name;
                } else if (propname.Equals("Priority"))
                {
                    return CategoryService.GetInstance().GetCategoryName(int.Parse(value), Contain.CatType.Priority);
                }
                else if (propname.Equals("TaskType"))
                {
                    return CategoryService.GetInstance().GetCategoryName(int.Parse(value), Contain.CatType.Category);
                }
                else if (propname.Equals("BoardID"))
                {
                    return task.Board.Title;
                }
                else if (propname.Equals("Assignee"))
                {
                    return AccountHelper.GetUserFullname(task.AssigneeAcc.UserName);
                }
                else if (propname.Equals("Reporter"))
                {
                    return AccountHelper.GetUserFullname(task.ReporterAcc.UserName);
                }
            }
            return value;
        }
    }
}