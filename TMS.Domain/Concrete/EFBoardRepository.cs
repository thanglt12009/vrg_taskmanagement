using TMS.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Domain.Entities;
using System.Data.Entity.Validation;

namespace TMS.Domain.Concrete
{
    public class EFBoardRepository : BaseRepository, IBoardRepository
    {
        public IEnumerable<Board> Boards
        {
            get
            {
                return context.Boards.Where(w => w.DeleteFlag == false);
            }
        }

        public Board Get(int BoardID)
        {
            return context.Boards.Find(BoardID);
        }
        public Board Get(string code)
        {
            return context.Boards.Where(b => b.Code == code).FirstOrDefault();
        }

        public Board Detail(int BoardID)
        {
            try
            {
                var board = context.Boards.Where(p => p.ID == BoardID).FirstOrDefault();
                if (board == null) return null;
                board.Owner = context.Accounts.Where(p => p.UID == board.OwnerID && p.DeleteFlag == false).FirstOrDefault();
                board.Workflow = context.Workflows.Where(p => p.ID == board.WorkflowID && p.DeleteFlag == false).FirstOrDefault();
                board.Members = context.Accounts.Where(p => p.Boards.Select(b => b.ID).ToList().Contains(board.ID)).ToList();
                board.Tasks = context.Worktasks.Where(p => p.BoardID == board.ID).ToList();
                return board;
            }
            catch
            {
                throw;
            }
        }

        public bool Save(Board board)
        {
            bool result = true;
            try
            {
                if (board.Members != null)
                {
                    var mList = board.Members.Select(m => m.UID).ToArray();
                    var aList = context.Accounts.Where(a => mList.Any(s => s == a.UID)).ToList();
                    board.Members.Clear();
                    board.Members = aList;
                }
                if (board.ID <= 0)
                {
                    context.Boards.Add(board);
                }
                else
                {
                    context.Entry(board).State = System.Data.Entity.EntityState.Modified;
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
            };
            return result;
        }

        public bool Delete(int BoardID)
        {
            try
            {
                Board dbEntry = context.Boards.Find(BoardID);
                if (dbEntry != null)
                {
                    context.Boards.Remove(dbEntry);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
