using TMS.Domain.Abstract;
using TMS.Domain.Common;
using TMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using TMS.WebApp.Models;

namespace TMS.WebApp.Services
{
    public class KanbanService
    {
        private IAccountRepository accRepository;
        private IBoardRepository boardRepository;

        private static KanbanService _instance = null;

        public KanbanService(IBoardRepository boardRepository, IAccountRepository accRepository)
        {
            this.boardRepository = boardRepository;
            this.accRepository = accRepository;
            if (_instance == null)
                _instance = this;
        }
        public static KanbanService GetInstance()
        {
            return _instance;
        }
        public void ReloadData()
        {
            lock(this)
            {
                accRepository.ReloadDBContext();
                boardRepository.ReloadDBContext();
            }
        }
        public IList<KanbanInfoModel> UserBoardList(string username)
        {
            lock(this)
            {
                Account user = AccountHelper.GetCurrentUser(username);
                return boardRepository.Boards.Where(b => b.OwnerID == user.UID || b.Members.Select(m => m.UID).Any(i => i == user.UID))
                    .Select(
                        s => new KanbanInfoModel
                        {
                            Id = s.ID,
                            Title = s.Title,
                            Code = s.Code,
                            Owner = s.Owner,
                            Workflow = s.Workflow
                        }
                    ).ToList();
            }
        }
        public string GenerateBoardCode(string title)
        {
            title = title.Trim().ToLower();
            title = System.Text.RegularExpressions.Regex.Replace(title, @"\s+", " ");
            title = title.Replace(' ', '-');
            title = title.Substring(0, (title.Length > 10 ? 10 : title.Length));
            var board = boardRepository.Boards.Where(b => b.Code == title).FirstOrDefault();
            if (board != null)
            {
                title = title + "-" + DateTime.Now.ToString("ddMMyyyy");
            }
            return title;
        }
        public bool CheckBoardValid(Board board, string username)
        {
            if (board != null)
            {
                Account user = AccountHelper.GetCurrentUser(username);
                if (!(board.OwnerID == user.UID || board.Members.Select(m => m.UID).ToList().Contains(user.UID)))
                {
                    return false;
                }
            }
            return true;
        }
        public string GetBoardCode(int id)
        {
            var board = boardRepository.Get(id);
            if (board != null)
                return board.Code;
            return String.Empty;
        }
    }
}