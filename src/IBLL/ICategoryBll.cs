using Models.Entity;

namespace IBLL
{
    public partial interface ICategoryBll : IBaseBll<Category>
    {
        /// <summary>
        /// 删除分类，并将该分类下的文章移动到新分类下
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mid"></param>
        /// <returns></returns>
        bool Delete(int id, int mid);
    }
}