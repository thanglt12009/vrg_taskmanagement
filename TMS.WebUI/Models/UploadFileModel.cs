namespace TMS.WebApp.Models
{
    public class UploadFileModel
    {
        public string TaskId { get; set; }

        public string AttachedId { get; set; }

        public string fileName { get; set; }

        public bool isUpdate { get; set; }
    }
}