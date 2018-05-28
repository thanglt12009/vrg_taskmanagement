using System.Data.Entity;
using TMS.Domain.Entities;

namespace TMS.Domain.Concrete
{
    public class EFDbContext : DbContext
    {
        //private static EFDbContext _instance = null;
        public static EFDbContext GetInstance()
        {
            return new EFDbContext();
            /*if (_instance == null)
                _instance = new EFDbContext();
            return _instance;*/
        }
        ~EFDbContext()
        {
            /*if (this == _instance)
                _instance = null;*/
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>()
                        .HasOptional(m => m.ParentCategory)
                        .WithMany(t => t.ChildCategories)
                        .HasForeignKey(m => m.ParentCatID)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Worktask>()
                        .HasRequired(m => m.OwnerAcc)
                        .WithMany(t => t.WorkTasks)
                        .HasForeignKey(m => m.Owner)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Worktask>()
                        .HasRequired(m => m.AssigneeAcc)
                        .WithMany(t => t.WorkTask1s)
                        .HasForeignKey(m => m.Assignee)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Worktask>()
                        .HasOptional(m => m.ReporterAcc)
                        .WithMany(t => t.WorkTask2s)
                        .HasForeignKey(m => m.Reporter)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Worktask>()
                        .HasOptional(w => w.WorkflowInstance)
                        .WithMany(t => t.Tasks)
                        .HasForeignKey(m => m.WorkflowInstanceID)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaskHistory>().HasRequired(x => x.UpdatedUserAcc)
                .WithMany(t => t.TaskHistory)
                .HasForeignKey(m => m.UpdatedUser)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Comment>().HasRequired(x => x.Account)
                .WithMany(t => t.Comment)
                .HasForeignKey(m => m.AccountID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Board>()
                .HasMany<Account>(s => s.Members)
                .WithMany(c => c.Boards)
                .Map(cs =>
                {
                    cs.MapLeftKey("BoardId");
                    cs.MapRightKey("AccountId");
                    cs.ToTable("BoardMembers");
                });
            modelBuilder.Entity<Board>()
                        .HasRequired(m => m.Owner)
                        .WithMany(t => t.OwnedBoards)
                        .HasForeignKey(m => m.OwnerID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Board>()
                        .HasOptional(m => m.Workflow)
                        .WithMany(t => t.Boards)
                        .HasForeignKey(m => m.WorkflowID)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Worktask>()
                        .HasRequired(m => m.Board)
                        .WithMany(t => t.Tasks)
                        .HasForeignKey(m => m.BoardID)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<WorkTaskMetas>()
                        .HasRequired(m => m.Task)
                        .WithMany(t => t.TaskMetas)
                        .HasForeignKey(m => m.WorktaskID)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<WorkflowInstance>()
                        .HasOptional(w => w.Workflow)
                        .WithMany(t => t.WorkflowInstances)
                        .HasForeignKey(m => m.WorkflowID)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<WorkflowInstance>()
                        .HasOptional(w => w.CurrentState)
                        .WithMany(t => t.WorkflowInstances)
                        .HasForeignKey(m => m.CurrentStateID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<State>()
                        .HasOptional(w => w.Workflow)
                        .WithMany(t => t.States)
                        .HasForeignKey(m => m.WorkflowID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Transition>()
                        .HasOptional(w => w.FromState)
                        .WithMany(t => t.TransitionTo)
                        .HasForeignKey(m => m.FromStateID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Transition>()
                        .HasOptional(w => w.ToState)
                        .WithMany(t => t.TransitionFrom)
                        .HasForeignKey(m => m.ToStateID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Transition>()
                        .HasOptional(w => w.Event)
                        .WithMany(t => t.Transitions)
                        .HasForeignKey(m => m.EventID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Action>()
                        .HasOptional(w => w.State)
                        .WithMany(t => t.Actions)
                        .HasForeignKey(m => m.StateID)
                        .WillCascadeOnDelete(false);

        }
        public virtual DbSet<Worktask> Worktasks { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Account> Accounts { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<TaskHistory> Histories { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Board> Boards { get; set; }
        public virtual DbSet<WorkTaskMetas> WorkTaskMetas { get; set; }
        public virtual DbSet<Workflow> Workflows { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<Transition> Transitions { get; set; }
        public virtual DbSet<WorkflowInstance> WorkflowInstances { get; set; }
    }
}
