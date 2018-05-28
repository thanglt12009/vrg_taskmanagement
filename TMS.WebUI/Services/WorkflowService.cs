using TMS.Domain.Abstract;
using TMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TMS.Domain.Entities;

namespace TMS.WebApp.Services
{
    public class WorkflowService
    {
        private IWorkflowRepository workflowRepository;
        private static WorkflowService _instance = null;
        private IDictionary<int, IEnumerable<SelectListItem>> stateList = null;
        private object locked = new object();
        public WorkflowService(IWorkflowRepository wfRepository)
        {
            this.workflowRepository = wfRepository;
            stateList = new Dictionary<int, IEnumerable<SelectListItem>>();
            if (_instance == null)
                _instance = this;
        }
        public static WorkflowService GetInstance()
        {
            return _instance;
        }
        public void ReloadData()
        {
            lock (this)
            {
                workflowRepository.ReloadDBContext();
            }
        }
        public IEnumerable<SelectListItem> GetStateList(int? workflowID)
        {
            int id = (workflowID == null ? 0 : (int)workflowID);
            lock (locked)
            {
                if (!stateList.ContainsKey(id))
                {
                    Workflow wf = workflowRepository.Get(id);
                    if (wf != null)
                    {
                        var stList = wf.States.ToList<State>();
                        IEnumerable<SelectListItem> selectList = stList.Select(c => new SelectListItem() { Text = c.Name, Value = c.Value.ToString(), Selected = false }).ToList<SelectListItem>();
                        ((List<SelectListItem>)selectList).Insert(0, new SelectListItem() { Value = String.Empty, Text = String.Empty, Selected = false });
                        stateList.Add(id, selectList);
                    }
                }
            }
            return stateList[id];
        }public State GetState(int workflowID, int value)
        {
            lock (locked)
            {
                Workflow wf = workflowRepository.Get(workflowID);
                if (wf != null)
                {
                    return wf.States.Where(s => s.Value == value).FirstOrDefault();
                }
            }
            return null;
        }
        public IEnumerable<SelectListItem> GetWorkflowList()
        {
            IEnumerable<SelectListItem> selectList = workflowRepository.Workflows.Select(w => new SelectListItem() { Text = w.Name, Value = w.ID.ToString(), Selected = false }).ToList<SelectListItem>();
            return selectList;
        }
    }
}