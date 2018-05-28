using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Domain.Entities;
using System.Data.Entity;

namespace TMS.Domain.Common
{
    public static class QueryHelper
    {
        public static IQueryable<Worktask> IncludeAllAttachmentObject(this IQueryable<Worktask> query)
        {
            return query
                .Include(w=>w.Attachment);

        }
    }
}
