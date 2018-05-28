using System.ComponentModel.DataAnnotations;

namespace TMS.WebApp.Models
{
    public class ElasticSearchQuery
    {
        [Required]
        public string RawSearch { get; set; }

        public string FileContent { get; set; }
    }
}