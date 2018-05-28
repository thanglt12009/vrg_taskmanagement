using TMS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TMS.WebApp.Services
{
    public class ServiceBuilder
    {
        public static void Initialize()
        {
            ICategoryRepository catRepository = (ICategoryRepository)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(ICategoryRepository));
            IAccountRepository accRepository = (IAccountRepository)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IAccountRepository));
            IBoardRepository boardRepository = (IBoardRepository)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IBoardRepository));
            IWorkflowRepository workflowRepository = (IWorkflowRepository)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IWorkflowRepository));
            IWorktaskRepository worktaskRepository = (IWorktaskRepository)System.Web.Mvc.DependencyResolver.Current.GetService(typeof(IWorktaskRepository));
            CategoryService catService;
            UserPermissionService userService;
            KanbanService kanbanService;
            WorkflowService workflowService;
            BoardService boardService;
            WorktaskService worktaskService;
            if (CategoryService.GetInstance() == null)
            {
                catService = new CategoryService(catRepository);
            }
            else
            {
                catService = CategoryService.GetInstance();
            }
            if (UserPermissionService.GetInstance() == null)
            {
                userService = new UserPermissionService(accRepository);
            }
            else
            {
                userService = UserPermissionService.GetInstance();
            }
            if (KanbanService.GetInstance() == null)
            {
                kanbanService = new KanbanService(boardRepository, accRepository);
            }
            else
            {
                kanbanService = KanbanService.GetInstance();
            }
            if (WorkflowService.GetInstance() == null)
            {
                workflowService = new WorkflowService(workflowRepository);
            }
            else
            {
                workflowService = WorkflowService.GetInstance();
            }
            if (BoardService.GetInstance() == null)
            {
                boardService = new BoardService(boardRepository);
            }
            else
            {
                boardService = BoardService.GetInstance();
            }
            if (WorktaskService.GetInstance() == null)
            {
                worktaskService = new WorktaskService(worktaskRepository, catRepository);
            }
            else
            {
                worktaskService = WorktaskService.GetInstance();
            }
        }
    }
}