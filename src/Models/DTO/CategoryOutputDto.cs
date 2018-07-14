using System.Collections.Generic;

namespace Models.DTO
{
    /// <summary>
    /// 文章分类输出模型
    /// </summary>
    public class CategoryOutputDto : BaseDto
    {
        public CategoryOutputDto()
        {
            Post = new HashSet<PostOutputDto>();
        }
        /// <summary>
        /// 分类名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分类描述
        /// </summary>
        public string Description { get; set; }

        public virtual ICollection<PostOutputDto> Post { get; set; }
    }
}