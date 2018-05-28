using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using TMS.Domain.Common;

namespace TMS.Domain.Entities
{
    public class Category
    {
        [Key]
        public int CatID { get; set; }
        [Required]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }
        public bool DeleteFlag { get; set; }

        public int? ParentCatID { get; set; }

        public virtual ICollection<Category> ChildCategories { get; set; }

        [Object(Ignore = true)]
        [ForeignKey("ParentCatID")]
        [Display(Name = "Danh mục cha")]
        public virtual Category ParentCategory { get; set; }

        [Required]
        [Display(Name = "Giá trị danh mục")]
        public int CatValue { get; set; }
        [Required]
        [Display(Name = "Phân loại danh mục")]
        public int CatType { get; set; }

        public Category()
        {
            CatID = 0;
            CategoryName = String.Empty;
            ParentCatID = null;
            CatValue = 0;
            CatType = -1;
        }
        public Category(int catID, string categoryName, int? parentCatID = null)
        {
            this.CatID = catID;
            this.CategoryName = categoryName;
            this.ParentCatID = parentCatID;
        }
    }
}
