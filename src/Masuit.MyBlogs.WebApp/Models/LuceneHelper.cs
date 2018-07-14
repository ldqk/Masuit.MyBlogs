using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using IBLL;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Logging;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Newtonsoft.Json;
using PanGu;
using PanGu.HighLight;
using Directory = System.IO.Directory;
using Version = Lucene.Net.Util.Version;

namespace Masuit.MyBlogs.WebApp.Models
{
    /// <summary>
    /// Lucene帮助类
    /// </summary>
    public static class LuceneHelper
    {
        #region Config

        /// <summary>
        /// 索引文件夹路径
        /// </summary>
        public static string IndexPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory + @"App_Data\lucenedir\";//设置Lucene索引目录

        public static object LockObj { get; } = new object();
        ///// <summary>
        ///// 分词API  //http://zhannei.baidu.com/api/customsearch/keywords?title=群主可撤回消息，QQ9.0去广告本地SVIP绿色精简优化版
        ///// http://api.pullword.com
        ///// </summary>
        //public static HttpClient ApiClient { get; set; } = new HttpClient() { BaseAddress = new Uri("http://api.pullword.com") };

        #endregion

        #region 创建索引

        /// <summary>
        /// 创建索引，如果索引库存在则刷新索引
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public static void CreateIndex(IQueryable<Post> dataSource)
        {
            if (string.IsNullOrEmpty(IndexPath))
            {
                throw new Exception("未设置索引文件夹路径，参数名：" + IndexPath);
            }
            string dir = IndexPath;

            #region 创建索引前尝试删除索引文件夹

            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception e)
            {
                LogManager.Debug("尝试删除索引文件夹失败！", e.Message);
            }

            #endregion

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (File.Exists(Path.Combine(dir, "segments.gen")))
            {
                IncreaseIndex();
                return;
            }
            string indexPath = dir; //注意和磁盘上文件夹的大小写一致，否则会报错。将创建的分词内容放在该目录下。
            using (FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory())) //指定索引文件(打开索引目录) FS指的是就是FileSystem
            {
                bool isUpdate = IndexReader.IndexExists(directory); //IndexReader:对索引进行读取的类。该语句的作用：判断索引库文件夹是否存在以及索引特征文件是否存在。
                lock (LockObj)
                {
                    using (var writer = new IndexWriter(directory, new PanGuAnalyzer(), !isUpdate, IndexWriter.MaxFieldLength.UNLIMITED)) //!向索引库中写索引。这时在这里加锁。
                    {
                        foreach (Post item in dataSource.AsParallel())
                        {
                            item.Content = item.Content.RemoveHtml();
                            try
                            {
                                writer.AddDocument(EntityToDocument(item));
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region 查询所有符合条件的内容

        /// <summary>
        /// 查询所有符合条件的内容
        /// </summary>
        /// <param name="kw">关键词</param>
        /// <param name="segment">提取长度</param>
        /// <returns></returns>
        public static IEnumerable<PostOutputDto> Search(string kw, int segment = 200)
        {
            if (string.IsNullOrEmpty(IndexPath))
            {
                throw new Exception("未设置索引文件夹路径，参数名：" + IndexPath);
            }
            string indexPath = IndexPath;
            using (var analyzer = new PanGuAnalyzer())
            {
                var list = CutKeywords(kw);
                var result = new ConcurrentQueue<PostOutputDto>();
                Parallel.ForEach(list, k =>
                {
                    if (k.Contains(new[] { @"\?", @"\*", @"\+", @"\-", @"\[", @"\]", @"\{", @"\}", @"\(", @"\)", "�" }))
                    {
                        return;
                    }
                    FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory());
                    IndexReader reader = IndexReader.Open(directory, true);
                    var searcher = new IndexSearcher(reader);
                    QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { nameof(Post.Id), nameof(Post.Title), nameof(Post.Content), nameof(Post.Author), nameof(Post.Label), nameof(Post.Email), nameof(Post.Keyword) }, analyzer); //多个字段查询  
                    Query query = parser.Parse(k);
                    int n = 100000;
                    TopDocs docs = searcher.Search(query, null, n);
                    if (docs?.TotalHits != 0 && docs?.ScoreDocs != null)
                    {
                        foreach (ScoreDoc sd in docs.ScoreDocs) //遍历搜索到的结果  
                        {
                            Document doc = searcher.Doc(sd.Doc);
                            if (result.Any(p => p.Id == doc.Get(nameof(Post.Id)).ToInt32()))
                            {
                                continue;
                            }
                            var simpleHtmlFormatter = new SimpleHTMLFormatter("<span style='color:red;background-color:yellow;font-size: 1.1em;font-weight:700;'>", "</span>");
                            var highlighter = new Highlighter(simpleHtmlFormatter, new Segment()) { FragmentSize = segment };
                            var content = doc.Get(nameof(Post.Content));
                            if (content.Length <= segment)
                            {
                                segment = content.Length;
                            }
                            result.Enqueue(new PostOutputDto()
                            {
                                Id = doc.Get(nameof(Post.Id)).ToInt32(),
                                Title = doc.Get(nameof(Post.Title)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Title))) : doc.Get(nameof(Post.Title)),
                                Content = content.ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, content) : content.Substring(0, segment),
                                Author = doc.Get(nameof(Post.Author)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Author))) : doc.Get(nameof(Post.Author)),
                                Label = doc.Get(nameof(Post.Label)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Label))) : doc.Get(nameof(Post.Label)),
                                Email = doc.Get(nameof(Post.Email)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Email))) : doc.Get(nameof(Post.Email)),
                                Keyword = doc.Get(nameof(Post.Keyword)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Keyword))) : doc.Get(nameof(Post.Keyword))
                            });
                        }
                    }
                });
                return result.Where(p => !string.IsNullOrEmpty(p.Title)).DistinctBy(p => p.Id);
            }
        }

        #endregion

        #region 多字段分页查询数据 

        /// <summary>
        /// 多字段分页查询数据
        /// </summary>
        /// <param name="keyword">关键词</param>
        /// <param name="total">总数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="segment">提取长度</param>
        /// <returns></returns>
        public static List<PostOutputDto> SearchPages(string keyword, out int total, int pageIndex = 1, int pageSize = 10, int segment = 200)
        {
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            int totalCount = 0;
            if (string.IsNullOrEmpty(IndexPath))
            {
                throw new Exception("未设置索引文件夹路径，参数名：" + IndexPath);
            }
            string indexPath = IndexPath;
            using (var analyzer = new PanGuAnalyzer())
            {
                var list = CutKeywords(keyword);
                var result = new ConcurrentQueue<PostOutputDto>();
                list.ForEach(k =>
                {
                    if (k.Contains(new[] { @"\?", @"\*", @"\+", @"\-", @"\[", @"\]", @"\{", @"\}", @"\(", @"\)", "�" }))
                    {
                        return;
                    }
                    FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory());
                    IndexReader reader = IndexReader.Open(directory, true);
                    var searcher = new IndexSearcher(reader);
                    var bq = new BooleanQuery();
                    //if (flag != "")
                    //{
                    //    var qpflag = new QueryParser(Version.LUCENE_30, "flag", analyzer);
                    //    Query qflag = qpflag.Parse(flag);
                    //    bq.Add(qflag, Occur.MUST); //与运算  
                    //}
                    if (k != "")
                    {
                        QueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30, new[] { nameof(Post.Id), nameof(Post.Title), nameof(Post.Content), nameof(Post.Author), nameof(Post.Label), nameof(Post.Email), nameof(Post.Keyword) }, analyzer); //多个字段查询
                        try
                        {
                            Query queryKeyword = parser.Parse(k);
                            bq.Add(queryKeyword, Occur.MUST); //与运算  
                        }
                        catch
                        {
                            return;
                        }
                    }
                    var collector = TopScoreDocCollector.Create(pageIndex * pageSize, true);
                    searcher.Search(bq, collector);
                    if (collector?.TotalHits == 0)
                    {
                        totalCount += 0;
                    }
                    else
                    {
                        int start = pageSize * (pageIndex - 1); //结束数  
                        int limit = pageSize;
                        ScoreDoc[] hits = collector?.TopDocs(start, limit).ScoreDocs;
                        totalCount += collector?.TotalHits ?? 0;
                        foreach (ScoreDoc sd in hits) //遍历搜索到的结果  
                        {
                            var doc = searcher.Doc(sd.Doc);
                            if (result.Any(p => p.Id == doc.Get(nameof(Post.Id)).ToInt32()))
                            {
                                continue;
                            }
                            var simpleHtmlFormatter = new SimpleHTMLFormatter("<span style='color:red;background-color:yellow;font-size: 1.1em;font-weight:700;'>", "</span>");
                            var highlighter = new Highlighter(simpleHtmlFormatter, new Segment())
                            {
                                FragmentSize = segment
                            };
                            var content = doc.Get(nameof(Post.Content));
                            if (content.Length <= segment)
                            {
                                segment = content.Length;
                            }
                            result.Enqueue(new PostOutputDto()
                            {
                                Id = doc.Get(nameof(Post.Id)).ToInt32(),
                                Title = doc.Get(nameof(Post.Title)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Title))) : doc.Get(nameof(Post.Title)),
                                Content = content.ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, content) : content.Substring(0, segment),
                                Author = doc.Get(nameof(Post.Author)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Author))) : doc.Get(nameof(Post.Author)),
                                Label = doc.Get(nameof(Post.Label)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Label))) : doc.Get(nameof(Post.Label)),
                                Email = doc.Get(nameof(Post.Email)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Email))) : doc.Get(nameof(Post.Email)),
                                Keyword = doc.Get(nameof(Post.Keyword)).ToLower().Contains(k.ToLower()) ? highlighter.GetBestFragment(k, doc.Get(nameof(Post.Keyword))) : doc.Get(nameof(Post.Keyword))
                            });
                        }
                    }
                });
                total = totalCount;
                return result.Where(p => !string.IsNullOrEmpty(p.Title)).DistinctBy(p => p.Id).ToList();
            }
        }

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public static List<string> CutKeywords(string keyword)
        {
            var list = new HashSet<string> { keyword };
            var mc = Regex.Matches(keyword, @"(([A-Z]*[a-z]*)[\d]*)([\u4E00-\u9FA5]+)*((?!\p{P}).)*");
            foreach (Match m in mc)
            {
                list.Add(m.Value);
                foreach (Group g in m.Groups)
                {
                    list.Add(g.Value);
                }
            }
            if (keyword.Length >= 6)
            {
                using (HttpClient client = new HttpClient() { BaseAddress = new Uri("http://zhannei.baidu.com") })
                {
                    try
                    {
                        var res = client.GetAsync($"/api/customsearch/keywords?title={keyword}").Result;
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            BaiduAnalysisModel model = JsonConvert.DeserializeObject<BaiduAnalysisModel>(res.Content.ReadAsStringAsync().Result, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                            if (model.Result.Res.KeywordList != null && model.Result.Res.KeywordList.Any())
                            {
                                list.AddRange(model.Result.Res.KeywordList.ToArray());
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            list.RemoveWhere(s => s.Length < 2 || Regex.IsMatch(s, @"^\p{P}.*"));
            return list.OrderByDescending(s => s.Length).ToList();
        }

        #endregion

        #region 删除索引

        /// <summary>
        /// 删除索引
        /// </summary>
        public static bool DeleteIndex()
        {
            if (string.IsNullOrEmpty(IndexPath))
            {
                throw new Exception("未设置索引文件夹路径，参数名：" + IndexPath);
            }
            string indexPath = IndexPath;
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory());
            using (var writer = new IndexWriter(directory, new PanGuAnalyzer(), false, IndexWriter.MaxFieldLength.LIMITED))
            {
                writer.DeleteAll();
                writer.Commit();
                writer.Optimize();
                return writer.HasDeletions();
            }
        }

        #endregion

        #region 文章的增量索引，全局刷新索引库

        /// <summary>
        /// 文章的增量索引，全局刷新索引库
        /// </summary>
        public static void IncreaseIndex()
        {
            if (string.IsNullOrEmpty(IndexPath))
            {
                throw new Exception("未设置索引文件夹路径，参数名：" + IndexPath);
            }
            using (FSDirectory directory = FSDirectory.Open(new DirectoryInfo(IndexPath), new NoLockFactory()))
            {
                using (IndexReader reader = IndexReader.Open(directory, true))
                {
                    //数据库中的所有数据
                    var dataAll = AutofacConfig.Container.Resolve<IPostBll>().LoadEntities(p => p.Status == Status.Pended);
                    var addData = new List<Post>(); //数据库中新来的数据，要进索引库的
                    var updData = new List<Post>(); // 数据库中修改的数据，也要修改索引库的
                    //判断有无字典
                    if (Directory.Exists(IndexPath))
                    {
                        foreach (Post item in dataAll)
                        {
                            bool addf = IsInIndex(item.Id.ToString().Trim(), reader); //索引数据库是否存在这条记录
                            if (addf)
                            {
                                addData.Add(item);
                            }
                            else
                            {
                                updData.Add(item);
                            }
                        }
                        if (updData.Count > 0)
                        {
                            UpdateListToIndex(updData, reader);
                        }
                        using (var writer = new IndexWriter(directory, new PanGuAnalyzer(), false, IndexWriter.MaxFieldLength.UNLIMITED))
                        {
                            try
                            {
                                if (addData.Count > 0 && updData.Count > 0)
                                {
                                    AddListToIndex(updData, writer); //更新索引
                                    AddListToIndex(addData, writer); //添加索引
                                }

                                if (addData.Count > 0 && updData.Count == 0)
                                {
                                    AddListToIndex(addData, writer);
                                }
                                writer.Optimize();
                            }
                            catch (IOException e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                    else
                    {
                        CreateIndex(dataAll); //否则就直接创建索引
                    }
                }
            }
        }

        #endregion

        #region 判断数据库中的某条数据是否已经索引了

        /// <summary>
        /// 功能描述：判断数据库中的某条数据是否已经索引了
        /// </summary>
        /// <param name="id">数据库记录的id字段值</param>
        /// <param name="pIndexReader">IndexReader的对象</param>
        /// <returns>true表示存在， false表示不存在</returns>
        private static bool IsInIndex(String id, IndexReader pIndexReader)
        {
            bool flag = true;
            for (int i = 0; i < pIndexReader.NumDocs(); i++)
            {
                Document doc = pIndexReader.Document(i);
                if (id.Equals(doc.Get(nameof(Post.Id))))
                {
                    flag = false;
                }
            }
            return flag;
        }

        #endregion

        #region 将实体对象转换成Lucene的文档对象

        /// <summary>
        /// 将实体对象转换成Lucene的文档对象
        /// </summary>
        /// <param name="item">文章实体</param>
        /// <returns></returns>
        private static Document EntityToDocument(Post item)
        {
            var doc = new Document();
            doc.Add(new Field(nameof(item.Id), item.Id.ToString(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field(nameof(item.Title), item.Title, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field(nameof(item.Content), item.Content.RemoveHtmlTag(), Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field(nameof(item.Author), item.Author, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field(nameof(item.Email), item.Email ?? "", Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field(nameof(item.Label), item.Label ?? "", Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            doc.Add(new Field(nameof(item.Keyword), item.Keyword ?? "", Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            return doc;
        }

        #endregion

        #region 增量索引，批量添加索引库

        /// <summary>
        /// 增量索引，批量添加索引库
        /// </summary>
        /// <param name="posts">文章集合</param>
        /// <param name="writer">索引读写器</param>
        public static void AddListToIndex(List<Post> posts, IndexWriter writer)
        {
            if (posts.Count > 0)
            {
                foreach (Post item in posts)
                {
                    Document document = EntityToDocument(item);
                    writer.AddDocument(document); //将文档写入索引库
                }
            }
        }

        #endregion

        #region 增量索引 第一步去索引库删除数据

        /// <summary>
        /// 增量索引 第一步去索引库删除数据
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="reader"></param>
        public static void UpdateListToIndex(List<Post> resultList, IndexReader reader)
        {
            if (resultList.Count > 0)
            {
                foreach (Post item in resultList)
                {
                    var term = new Term("id", item.Id.ToString().Trim());
                    //Query query = new TermQuery(term);
                    reader.DeleteDocuments(term);
                }
            }
        }

        #endregion
    }
}