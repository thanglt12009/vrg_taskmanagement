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
    public class BoardService
    {
        private IBoardRepository boardRepository;
        private static BoardService _instance = null;
        private IDictionary<int, IEnumerable<SelectListItem>> boardList = null;
        private object locked = new object();
        public BoardService(IBoardRepository boardRepository)
        {
            this.boardRepository = boardRepository;
            boardList = new Dictionary<int, IEnumerable<SelectListItem>>();
            if (_instance == null)
                _instance = this;
        }
        public static BoardService GetInstance()
        {
            return _instance;
        }
        public void ReloadData()
        {
            lock (this)
            {
                boardRepository.ReloadDBContext();
            }
        }
        public IEnumerable<SelectListItem> GetBoardList(string username, int? workflowID)
        {
            int id = (workflowID == null ? 0 : (int)workflowID);
            lock (locked)
            {
                if (id == 0 && !boardList.ContainsKey(id))
                {
                    Account user = AccountHelper.GetCurrentUser(username);
                    var bList = boardRepository.Boards.Where(b => b.OwnerID == user.UID || b.Members.Select(m => m.UID).Any(i => i == user.UID)).ToList();
                    IEnumerable<SelectListItem> selectList = bList.Select(c => new SelectListItem() { Text = c.Title, Value = c.ID.ToString(), Selected = false }).ToList<SelectListItem>();
                    boardList.Add(id, selectList);
                }
                else if (!boardList.ContainsKey(id))
                {
                    Account user = AccountHelper.GetCurrentUser(username);
                    var bList = boardRepository.Boards.Where(b => b.WorkflowID == id && (b.OwnerID == user.UID || b.Members.Select(m => m.UID).Any(i => i == user.UID))).ToList();
                    IEnumerable<SelectListItem> selectList = bList.Select(c => new SelectListItem() { Text = c.Title, Value = c.ID.ToString(), Selected = false }).ToList<SelectListItem>();
                    boardList.Add(id, selectList);
                }
            }
            return boardList[id];
        }
        public Board GetBoard(int boardid)
        {
            return boardRepository.Detail(boardid);
        }
    }
}