using Aspose.Words;
using System.IO;

namespace Masuit.MyBlogs.Core.Common
{
    /// <summary>
    /// 文档转换操作
    /// </summary>
    public static class DocumentConvert
    {
        /// <summary>
        /// doc转html
        /// </summary>
        /// <param name="docPath">doc文件路径</param>
        /// <param name="htmlDir">生成的html所在目录，由于生成html后会将图片都放到同级的目录下，所以用文件夹保存，默认的html文件名为index.html</param>
        /// <param name="index">默认文档名为index.html</param>
        public static void Doc2Html(string docPath, string htmlDir, string index = "index.html")
        {
            Document doc = new Document(docPath);
            doc.Save(Path.Combine(htmlDir, index), SaveFormat.Html);
        }
    }
}
