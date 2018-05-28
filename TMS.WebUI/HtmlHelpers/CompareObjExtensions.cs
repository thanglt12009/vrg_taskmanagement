using TMS.Domain.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using TMS.WebApp.Services;
using TMS.Domain.Common;

namespace TMS.WebApp.HtmlHelpers
{
    public static class CompareObjExtensions
    {
        private static Dictionary<string, string> compareFieldTitle = new Dictionary<string, string>
        {
            { "Title", "Tiêu đề" },
            { "Description", "Mô tả" },
            { "Assignee", "Người thực hiện" },
            { "Reporter", "Người báo cáo" },
            { "PlannedStartDate", "Ngày dự định làm" },
            { "PlannedEndDate", "Ngày dự định xong" },
            { "Status", "Trạng thái" },
            { "Priority", "Độ ưu tiên" },
            { "TaskType", "Loại công việc" },
            { "BoardID", "Bảng công việc" }
        };
        //objA : old object
        //objB : new object
        public static List<TaskHistory> DetailedCompare<T>(this T objA , T objB, int updateUser, int taskID, bool isNew)
        {
            var list = new List<TaskHistory>();
            var action = new List<string> { "Cập nhật", "Tạo mới" };
            //insert
            if (objA == null)
            {
                //attachment
                if (IsList(objB))
                {
                    var collection = (IList)objB;
                    foreach (var item in collection)
                    {
                        //not default
                        if (!string.IsNullOrEmpty(GetValueProps(item, "AttachmentCode")))
                        {
                            var name = GetValueProps(item, "Name");
                            var updated = "Thêm mới tài liệu: " + name;
                            var newValue = name;
                            var originalValue = string.Empty;
                            var result = GenerateDiff(action[0], updated, newValue, originalValue, updateUser, taskID);
                            list.Add(result);
                        }
                    }
                }
                //worktask
                else
                {
                    //insert new worktask
                    if (isNew)
                    {
                        var identity = GetValueProps(objB, "Identify");
                        var updated = "Tạo mới task: " + identity;
                        var newValue = identity;
                        var originalValue = "";
                        var result = GenerateDiff(action[1], updated, newValue, originalValue, updateUser, taskID);
                        list.Add(result);
                    }
                }
            }
            //delete
            else if (objB == null)
            {
                var name = GetValueProps(objA, "Name");
                var updated = "Xóa tài liệu: " + name;
                var newValue = string.Empty;
                var originalValue = name;
                var result = GenerateDiff(action[0], updated, newValue, originalValue, updateUser, taskID);
                list.Add(result);
            }
            //edit
            else
            {
                var props = compareFieldTitle.Keys;
                foreach (var item in props)
                {
                    //get value by props name

                    var old = GetValueProps(objA, item);
                    var newObj = GetValueProps(objB, item);
                    if (!Equals(old, newObj))
                    {
                        String oldName = WorktaskService.GetInstance().GetTaskDisplayValue(taskID, old, item);
                        String newName = WorktaskService.GetInstance().GetTaskDisplayValue(taskID, newObj, item);
                        var result = GenerateDiff(action[0], item, newName, oldName, updateUser, taskID);
                        list.Add(result);
                    }
                }
            }
            return list;
        }
        public static bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }
        public static List<TaskHistory> GenerateDiff(string action, List<DiffInfo> diffInfo, int updateUser, int taskID)
        {
            List<TaskHistory> res = new List<TaskHistory>();
            Worktask task = WorktaskService.GetInstance().GetTask(taskID);
            foreach (var item in diffInfo)
            {
                string displayTitle = item.Title;
                switch (item.DisplayType)
                {
                    case DisplayTypeE.UserId:
                        item.OldValue = (item.OldValue == null ? String.Empty : AccountHelper.GetUserById((int)item.OldValue).DisplayName2);
                        item.NewValue = (item.NewValue == null ? String.Empty : AccountHelper.GetUserById((int)item.NewValue).DisplayName2);
                        break;
                    case DisplayTypeE.BoardId:
                        item.OldValue = (item.OldValue == null ? String.Empty : BoardService.GetInstance().GetBoard((int)item.OldValue).Title);
                        item.NewValue = (item.NewValue == null ? String.Empty : BoardService.GetInstance().GetBoard((int)item.NewValue).Title);
                        break;
                    case DisplayTypeE.WorkflowState:
                        int id = (int)(task.WorkflowInstance.WorkflowID == null ? 0 : task.WorkflowInstance.WorkflowID);
                        item.OldValue = (item.OldValue == null ? String.Empty : WorkflowService.GetInstance().GetState(id, (int)item.OldValue).Name);
                        item.NewValue = (item.NewValue == null ? String.Empty : WorkflowService.GetInstance().GetState(id, (int)item.NewValue).Name);
                        break;
                    case DisplayTypeE.CategoryPriority:
                        item.OldValue = (item.OldValue == null ? String.Empty : CategoryService.GetInstance().GetCategoryName((int)item.OldValue, Contain.CatType.Priority));
                        item.NewValue = (item.NewValue == null ? String.Empty : CategoryService.GetInstance().GetCategoryName((int)item.NewValue, Contain.CatType.Priority));
                        break;
                    case DisplayTypeE.CategoryTaskType:
                        item.OldValue = (item.OldValue == null ? String.Empty : CategoryService.GetInstance().GetCategoryName((int)item.OldValue, Contain.CatType.Category));
                        item.NewValue = (item.NewValue == null ? String.Empty : CategoryService.GetInstance().GetCategoryName((int)item.NewValue, Contain.CatType.Category));
                        break;
                    case DisplayTypeE.CategoryCompany:
                        item.OldValue = (item.OldValue == null ? String.Empty : CategoryService.GetInstance().GetCategoryName((int)item.OldValue, Contain.CatType.Company));
                        item.NewValue = (item.NewValue == null ? String.Empty : CategoryService.GetInstance().GetCategoryName((int)item.NewValue, Contain.CatType.Company));
                        break;
                    case DisplayTypeE.Date:
                        item.OldValue = (item.OldValue == null ? String.Empty : ((DateTime)item.OldValue).ToString("dd/MM/yyyy"));
                        item.NewValue = (item.NewValue == null ? String.Empty : ((DateTime)item.NewValue).ToString("dd/MM/yyyy"));
                        break;
                }
                res.Add(CompareObjExtensions.GenerateDiff(action, displayTitle, (item.NewValue == null ? String.Empty : item.NewValue.ToString()), (item.OldValue == null ? String.Empty : item.OldValue.ToString()), updateUser, taskID));
            }
            return res;
        }
        public static TaskHistory GenerateDiff(string action, string updatedField, string newVal, string origVal, int updateUser, int taskID)
        {
            var obj = new TaskHistory();
            obj.TaskID = taskID;
            obj.UpdatedDateTime = DateTime.Now;
            obj.UpdatedUser = updateUser;
            obj.Action = action;
            obj.UpdatedField = (compareFieldTitle.ContainsKey(updatedField) ? compareFieldTitle[updatedField] : updatedField) ;
            obj.NewValue = newVal;
            obj.OriginalValue = origVal;
            return obj;
        }

        public static string GetValueProps<T>(T obj, string name)
        {
            //OLD: return obj.GetType().GetProperty(name).GetValue(obj, null).ToString();

            var valueget = obj.GetType().GetProperty(name).GetValue(obj, null);

            if (valueget != null) return valueget.ToString();
            else return String.Empty;
        }
    } 
}

//if (isEdit)
//{
//    //list props need to compare
//    var props = new List<string> { "Title", "Description", "Assignee", "PlannedStartDate", "PlannedEndDate", "Status" };


//    foreach (var item in props)
//    {
//        //get value by props name
//        var old = objA.GetType().GetProperty(item).GetValue(objA, null);
//        var newObj = objB.GetType().GetProperty(item).GetValue(objB, null);
//        if (!Equals(old, newObj))
//        {

//        }
//    }
//}
////create new
//else
//{

//    //worktask
//    if (isWorktask)
//    {
//        var change = new TaskHistory();
//        change.TaskID = TaskID;
//        change.UpdatedDateTime = DateTime.Now;
//        change.UpdatedUser = UpdateUser;
//        change.Action = "Created Task";
//        change.UpdatedField = message + objA.GetType().GetProperty("Identify").GetValue(objA, null).ToString();
//        change.NewValue = objA.GetType().GetProperty("Identify").GetValue(objA, null).ToString();
//        change.OriginalValue = "";
//        list.Add(change);
//    }
//    else
//    {
//        var attachments = objA.GetType().GetProperty("At");
//        IList listAttachment = attachments.GetValue(objA, null) as IList;
//        foreach (var item in listAttachment)
//        {
//            //for attachments
//            var change = new TaskHistory();
//            change.TaskID = TaskID;
//            change.UpdatedDateTime = DateTime.Now;
//            change.UpdatedUser = UpdateUser;
//            change.Action = "Updated Task";
//            change.UpdatedField = message + item.GetType().GetProperty("Name").GetValue(item, null).ToString();
//            change.NewValue = item.GetType().GetProperty("Name").GetValue(item, null).ToString();
//            change.OriginalValue = string.Empty;
//            list.Add(change);
//        }
//    }
//}