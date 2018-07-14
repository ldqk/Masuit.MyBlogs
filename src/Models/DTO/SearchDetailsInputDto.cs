using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.DTO
{
    /// <summary>
    /// 搜索详细记录输入模型
    /// </summary>
    public class SearchDetailsInputDto
    {
        public SearchDetailsInputDto()
        {
            SearchTime = DateTime.Now;
        }

        /// <summary>
        /// 关键词
        /// </summary>
        [Required(ErrorMessage = "关键词不能为空"), MaxLength(64, ErrorMessage = "关键词最大允许64个字符")]
        public string KeyWords { get; set; }

        /// <summary>
        /// 搜索时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime SearchTime { get; set; }

        /// <summary>
        /// 访问者IP
        /// </summary>
        public string IP { get; set; }
    }
}