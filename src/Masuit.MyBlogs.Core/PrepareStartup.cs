using JiebaNet.Segmenter;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.Tools;
using Masuit.Tools.Systems;
using Masuit.Tools.Win32;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core
{
    public static class PrepareStartup
    {
        /// <summary>
        /// 初始化系统设置参数
        /// </summary>
        /// <param name="app"></param>
        internal static void InitSettings(this IApplicationBuilder app)
        {
            var dic = app.ApplicationServices.GetRequiredService<DataContext>().SystemSetting.ToDictionary(s => s.Name, s => s.Value);
            CommonHelper.SystemSettings.AddOrUpdate(dic);
        }

        internal static void UseLuceneSearch(this IApplicationBuilder app, IHostEnvironment env, IHangfireBackJob hangfire, LuceneIndexerOptions luceneIndexerOptions)
        {
            Task.Run(() =>
            {
                Console.WriteLine("正在导入自定义词库...");
                double time = HiPerfTimer.Execute(() =>
                {
                    var set = app.ApplicationServices.GetRequiredService<DataContext>().Post.Select(p => $"{p.Title},{p.Label},{p.Keyword}").AsEnumerable().SelectMany(s => s.Split(',', '，', ' ', '+', '—', '(', ')', '：', '&', '（', '）', '-', '_', '[', ']')).Where(s => s.Length > 1).ToHashSet();
                    var lines = File.ReadAllLines(Path.Combine(env.ContentRootPath, "App_Data", "CustomKeywords.txt")).Union(set);
                    var segmenter = new JiebaSegmenter();
                    foreach (var word in lines)
                    {
                        segmenter.AddWord(word);
                    }
                });
                Console.WriteLine($"导入自定义词库完成，耗时{time}s");
                Windows.ClearMemorySilent();
            });

            string lucenePath = Path.Combine(env.ContentRootPath, luceneIndexerOptions.Path);
            if (!Directory.Exists(lucenePath) || Directory.GetFiles(lucenePath).Length < 1)
            {
                Console.WriteLine("索引库不存在，开始自动创建Lucene索引库...");
                hangfire.CreateLuceneIndex();
                Console.WriteLine("索引库创建完成！");
            }
        }
    }
}