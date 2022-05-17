using Masuit.LuceneEFCore.SearchEngine;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    [Table("Variables")]
    public class Variables : LuceneIndexableBaseEntity
    {
        [Required(ErrorMessage = "变量名不能为空"), RegularExpression(@"^[a-zA-Z][a-zA-Z\d]*$", ErrorMessage = "变量名不规范")]
        public string Key { get; set; }

        [Required(ErrorMessage = "变量值不能为空")]
        public string Value { get; set; }
    }
}
