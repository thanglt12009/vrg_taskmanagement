using TMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Domain.Abstract
{
    public interface IBoardRepository : IBaseRepository
    {
        IEnumerable<Board> Boards { get; }
        Board Get(int BoardID);
        Board Detail(int BoardID);
        bool Save(Board board);
        bool Delete(int BoardID);
    }
}
