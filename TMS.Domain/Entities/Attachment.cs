using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nest;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;

namespace TMS.Domain.Entities
{
    [ElasticsearchType(Name = "attachment")]
    public class Attachment
    {
        [Number(Ignore = true)]
        [HiddenInput(DisplayValue = false)]
        [Key]
        public int AttachmentID { get; set; }

        [Text(Name = "attachment_number")]
        //[Required(ErrorMessage = "Vui lòng nhập số văn bản")]
        [DisplayName("Số văn bản")]
        [DisplayFormat(NullDisplayText = "Never connected")]
        public string AttachmentCode { get; set; }

        [Text(Name = "attachment_type")]
        [Required(ErrorMessage = "Vui lòng nhập loại tài liệu")]
        [DisplayName("Loại")]
        public string ArchiveType { get; set; }

        [Date(Name = "attachment_date")]
        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        [DisplayName("Ngày văn bản")]
        public DateTime? Date { get; set; }

        [Text(Name = "attachment_receiver", Ignore = true)]
        //[Required(ErrorMessage = "Vui lòng nhập người nhận")]
        [DisplayName("Người nhận")]
        public string Receiver { get; set; }

        [Text(Name = "attachment_storenumber")]
        [DisplayName("Số hồ sơ lưu")]
        public string ArchiveNo { get; set; }

        [Text(Name = "attachment_locationname")]
        [DisplayName("Vị trí lưu trữ")]
        public string StorageLocation { get; set; }

        [Text(Name = "attachment_content")]
        public string Metadata { get; set; }

        [Text(Name = "attachment_name")]
        public string Name { get; set; }

        [Text(Ignore = true)]
        public string StoredPath { get; set; }

        [Number(Ignore = true)]
        public int WorktaskID { get; set; }

        //[Object(Ignore =true)]
        //[ForeignKey("WorktaskID")]
        //[ScriptIgnore]
        //public virtual Worktask Worktask { get;set;}

        [Boolean(Ignore =true)]
        public bool DeleteFlag { get; set; }

        //public virtual Worktask Worktask { get; set; }
        public Attachment()
        {
            AttachmentID = 0;
            AttachmentCode = string.Empty;
            ArchiveType = string.Empty;
            Receiver = string.Empty;
            StorageLocation = string.Empty;
            ArchiveNo = string.Empty;
            StoredPath = string.Empty;
            Name = string.Empty;
        }
    }
}
