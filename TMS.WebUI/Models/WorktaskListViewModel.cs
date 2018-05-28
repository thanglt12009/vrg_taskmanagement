using System.Collections.Generic;
using TMS.Domain.Entities;
using TMS.Domain.Common;

namespace TMS.WebApp.Models
{
    public class WorktaskListViewModel
    {
        public KanbanInfoModel Board { get; set; }
        public IEnumerable<Worktask> Worktasks { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string CurrentWorktaskCategory { get; set; }

        public int? TaskPriorityEnum { get; set; }

        public int? TaskStatusEnum { get; set; }

        public int? TaskTypeEnum { get; set; }

        public int? Company { get; set; }
    }
}