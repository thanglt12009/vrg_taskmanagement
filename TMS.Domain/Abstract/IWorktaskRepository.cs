using TMS.Domain.Entities;
using System.Collections.Generic;

namespace TMS.Domain.Abstract
{
    public interface IWorktaskRepository : IBaseRepository
    {
        IEnumerable<Worktask> Worktasks { get; }

        IEnumerable<Account> Accounts { get; }

        IEnumerable<Attachment> Attachments { get; }    

        bool SaveComment(Comment comment);
        Worktask Detail(int taskID);

        Worktask Detail(string taskcode);

        Worktask DetailInfo(Worktask task);

        bool SaveWorktask(Worktask worktask);

        Worktask DeleteWorktask(int worktaskId);

        bool SaveAttachmentFile(Worktask worktask);

        bool SaveHistory(List<TaskHistory> history);

        bool DeleteAttachment(int attachmentID);
        bool DeleteMeta(int metaID, bool commit = false);
    }
}
