using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using TMS.Domain.Common;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Globalization;
using TMS.Domain.Abstract;

namespace TMS.Domain.Entities
{
    [ElasticsearchType(Name = "worktask")]
    public class Worktask : Comparable
    {
        [Key]
        [HiddenInput(DisplayValue = false)]
        [Text(Analyzer = "keyword", Similarity = "BM25")]
        public int WorktaskID { get; set; }

        [Text(Name = "task_identify")]
        [HiddenInput(DisplayValue = false)]
        public string Identify { get; set; }

        [Text(Name = "task_title")]
        [Required(ErrorMessage = "Xin nhập Trích yếu")]
        [Display(Name = "Trích yếu")]
        [Comparable(DisplayType = DisplayTypeE.Text, MetaInfo = MetaValueTypeE.None)]
        public string Title { get; set; }

        [Text(Name = "task_description")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        [Comparable(DisplayType = DisplayTypeE.Text, MetaInfo = MetaValueTypeE.None)]
        public string Description { get; set; }

        [Number(Ignore = true)]
        [Display(Name = "Người tạo")]
        public int Owner { get; set; }

        [Number(Ignore = true)]
        [Display(Name = "Người thực hiện")]
        [Required(ErrorMessage = "Xin điền Người thực hiện")]
        [Comparable(DisplayType = DisplayTypeE.UserId, MetaInfo = MetaValueTypeE.None)]
        public int Assignee { get; set; }

        //[NotMapped]
        //[Display(Name = "Asignee Name")]
        //public string AssigneeName { get; set; }

        [Date(Ignore = true)]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày tạo")]
        public DateTime CreationDate { get; set; }

        [Date(Ignore = true)]
        [Display(Name = "Dự định làm")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Comparable(DisplayType = DisplayTypeE.Date, MetaInfo = MetaValueTypeE.None)]
        public DateTime? PlannedStartDate { get; set; }

        [Date(Ignore = true)]
        [Display(Name = "Dự định xong")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [ValidationExtension("PlannedStartDate", true, ErrorMessage = "Ngày Bắt đầu phải nhỏ hơn Ngày kết thúc")]
        [Comparable(DisplayType = DisplayTypeE.Date, MetaInfo = MetaValueTypeE.None)]
        public DateTime? PlannedEndDate { get; set; }

        //object attachments
        [Object(Name = "attachments", Store = false)]
        public IEnumerable<Attachment> Attachment { get; set; }

        [Object(Ignore = true)]
        [Display(Name = "Trạng thái")]
        [Comparable(DisplayType = DisplayTypeE.WorkflowState, MetaInfo = MetaValueTypeE.None)]
        public int Status { get; set; }

        [Date(Ignore = true)]
        [Display(Name = "Ngày bắt đầu")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ActualStartDate { get; set; }
        [Date(Ignore = true)]
        [Display(Name = "Ngày kết thúc")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ActualEndDate { get; set; }
        [Date(Ignore = true)]
        [Display(Name = "Ngày cập nhật cuối")]
        public DateTime? LastUpdateDateTime { get; set; }

        [Display(Name = "Độ ưu tiên")]
        [Object(Ignore = true)]
        [Required(ErrorMessage = "Xin chọn Độ ưu tiên")]
        [Comparable(DisplayType = DisplayTypeE.CategoryPriority, MetaInfo = MetaValueTypeE.None)]
        public int Priority { get; set; }

        [Display(Name = "Loại Công việc")]
        [Object(Ignore = true)]
        [Required(ErrorMessage = "Xin chọn Loại Công việc")]
        [Comparable(DisplayType = DisplayTypeE.CategoryTaskType, MetaInfo = MetaValueTypeE.None)]
        public int TaskType { get; set; }

        [Object(Ignore = true)]
        public IEnumerable<Comment> Comment { get; set; }

        [Object(Ignore = true)]
        public IEnumerable<TaskHistory> TaskHistories { get; set; }

        [Object(Ignore = true)]
        public virtual Account OwnerAcc { get; set; }

        [Object(Ignore = true)]
        public virtual Account AssigneeAcc { get; set; }

        [Number(Ignore = true)]
        [Display(Name = "Người quản lý")]
        [Comparable(DisplayType = DisplayTypeE.UserId, MetaInfo = MetaValueTypeE.None)]
        public int? Reporter { get; set; }

        [Object(Ignore = true)]
        [ForeignKey("Reporter")]
        public virtual Account ReporterAcc { get; set; }

        [Text(Ignore = true)]
        public string DataPath { get; set; }

        [Boolean(Ignore = true)]
        public bool DeleteFlag { get; set; }

        [Display(Name = "Bảng công việc")]
        [Required(ErrorMessage = "Xin chọn Bảng Công Việc")]
        [Comparable(DisplayType = DisplayTypeE.BoardId, MetaInfo = MetaValueTypeE.None)]
        public int BoardID { get; set; }

        [Object(Ignore = true)]
        public virtual Board Board { get; set; }

        [Object(Name = "metainfos", Store = false, Ignore = true)]
        public virtual ICollection<WorkTaskMetas> TaskMetas { get; set; }
        [Object(Ignore = true)]
        public virtual WorkflowInstance WorkflowInstance { get; set; }
        [Number(Ignore = true)]
        public int? WorkflowInstanceID { get; set; }

        [NotMapped]
        [Display(Name = "Ngày công văn đến")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Comparable(DisplayType = DisplayTypeE.Date, MetaInfo = MetaValueTypeE.SingleValue)]
        public DateTime? MetaInfoToVRGDateTime { get; set; }
        [NotMapped]
        [Display(Name = "Ngày đến Ban")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Comparable(DisplayType = DisplayTypeE.Date, MetaInfo = MetaValueTypeE.SingleValue)]
        public DateTime? MetaInfoToDepartmentDateTime { get; set; }
        [NotMapped]
        [Display(Name = "Số công văn đến")]
        [Comparable(DisplayType = DisplayTypeE.Text, MetaInfo = MetaValueTypeE.SingleValue)]
        public String MetaInfoDocumentNo { get; set; }

        [NotMapped]
        [Display(Name = "Công ty")]
        [Number(Ignore = true)]
        [Comparable(DisplayType = DisplayTypeE.CategoryCompany, MetaInfo = MetaValueTypeE.SingleValue)]
        public int? MetaInfoCompany { get; set; }

        [NotMapped]
        [Display(Name = "Công việc liên quan")]
        [Object(Ignore = true)]
        [Comparable(DisplayType = DisplayTypeE.RelatedTask, MetaInfo = MetaValueTypeE.MultipleValue)]
        public List<Worktask> RelatedTask { get; set; }

        [NotMapped]
        [Number(Ignore = true)]
        [Display(Name = "PTGĐ/TGĐ")]
        [Comparable(DisplayType = DisplayTypeE.UserId, MetaInfo = MetaValueTypeE.SingleValue)]
        public int? Director { get; set; }
        [NotMapped]
        [Object(Ignore = true)]
        public virtual Account DirectorAcc { get; set; }

        [NotMapped]
        [Number(Ignore = true)]
        [Display(Name = "HĐTV")]
        [Comparable(DisplayType = DisplayTypeE.UserId, MetaInfo = MetaValueTypeE.SingleValue)]
        public int? CouncilMember { get; set; }
        [NotMapped]
        [Object(Ignore = true)]
        public virtual Account CouncilMemberAcc { get; set; }

        public Worktask()
        {
            Attachment = new List<Attachment> {
                new Attachment()
            };
            Comment = new List<Comment>
            {
                new Comment()
            };
            DeleteFlag = false;
        }
        public Worktask clone()
        {
            return (Worktask)this.MemberwiseClone();
        }
        public void loadMetaInfo(IWorktaskRepository repository, IAccountRepository accRepository)
        {
            Type t = this.GetType();
            List<PropertyInfo> props = t.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ComparableAttribute))).ToList();
            DisplayTypeE displayType;
            MetaValueTypeE metaInfo;
            foreach (var p in props)
            {
                displayType = p.GetCustomAttribute<ComparableAttribute>().DisplayType;
                metaInfo = p.GetCustomAttribute<ComparableAttribute>().MetaInfo;
                if (metaInfo != MetaValueTypeE.None)
                {
                    string key = p.Name;
                    WorkTaskMetas meta = this.TaskMetas.Where(m => m.MetaKey == key).FirstOrDefault();
                    if (meta != null)
                    {
                        PropertyInfo fp;
                        switch (displayType)
                        {
                            case DisplayTypeE.Date:
                                DateTime d;
                                if (DateTime.TryParseExact(meta.MetaValue, "yyyy-MM-dd", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d))
                                {
                                    p.SetValue(this, d);
                                }
                                break;
                            case DisplayTypeE.RelatedTask:
                                List<WorkTaskMetas> rlist = this.TaskMetas.Where(m => m.MetaKey == key).ToList();
                                foreach (var item in rlist)
                                {
                                    if (this.RelatedTask == null)
                                        this.RelatedTask = new List<Worktask>();
                                    Worktask task = repository.Detail(item.MetaValue);
                                    if (task != null)
                                    {
                                        this.RelatedTask.Add(task);
                                    }
                                }
                                break;
                            case DisplayTypeE.CategoryCompany:
                                int v;
                                if (int.TryParse(meta.MetaValue, out v))
                                {
                                    p.SetValue(this, v);
                                }
                                break;
                            case DisplayTypeE.UserId:
                                int uid;
                                Account acc;
                                if (int.TryParse(meta.MetaValue, out uid))
                                {
                                    p.SetValue(this, uid);
                                    fp = t.GetProperty(p.Name + "Acc");
                                    if (fp != null)
                                    {
                                        acc = accRepository.Accounts.Where(a => a.UID == uid).FirstOrDefault();
                                        fp.SetValue(this, acc);
                                    }
                                }
                                break;
                            default:
                                p.SetValue(this, meta.MetaValue);
                                break;
                        }
                    }
                }
            }
        }
    }
}




