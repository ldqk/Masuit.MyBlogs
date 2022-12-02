using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CacheManager.Core;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.Tools.Html;
using Microsoft.EntityFrameworkCore;
using PanGu;
using PanGu.HighLight;
using System.Reflection;
using System.Text.RegularExpressions;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed class PostService : BaseService<Post>, IPostService
{
	private readonly ICacheManager<SearchResult<PostDto>> _cacheManager;
	private readonly ICategoryRepository _categoryRepository;
	private readonly IMapper _mapper;
	private readonly IPostTagsRepository _postTagsRepository;

	public PostService(IPostRepository repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher, ICacheManager<SearchResult<PostDto>> cacheManager, ICategoryRepository categoryRepository, IMapper mapper, IPostTagsRepository postTagsRepository) : base(repository, searchEngine, searcher)
	{
		_cacheManager = cacheManager;
		_categoryRepository = categoryRepository;
		_mapper = mapper;
		_postTagsRepository = postTagsRepository;
	}

	/// <summary>
	/// 文章高亮关键词处理
	/// </summary>
	/// <param name="p"></param>
	/// <param name="keyword"></param>
	public async Task Highlight(Post p, string keyword)
	{
		try
		{
			var simpleHtmlFormatter = new SimpleHTMLFormatter("<span style='color:red;background-color:yellow;font-size: 1.1em;font-weight:700;'>", "</span>");
			var highlighter = new Highlighter(simpleHtmlFormatter, new Segment()) { FragmentSize = int.MaxValue };
			keyword = Regex.Replace(keyword, @"<|>|\(|\)|\{|\}|\[|\]", " ");
			var keywords = Searcher.CutKeywords(keyword);
			var context = BrowsingContext.New(Configuration.Default);
			var document = await context.OpenAsync(req => req.Content(p.Content));
			var elements = document.DocumentElement.GetElementsByTagName("p");
			foreach (var e in elements)
			{
				for (var index = 0; index < e.ChildNodes.Length; index++)
				{
					var node = e.ChildNodes[index];
					bool handled = false;
					foreach (var s in keywords)
					{
						string frag;
						if (handled == false && node.TextContent.Contains(s, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(frag = highlighter.GetBestFragment(s, node.TextContent)))
						{
							switch (node)
							{
								case IElement el:
									el.InnerHtml = frag;
									handled = true;
									break;

								case IText t:
									var parser = new HtmlParser();
									var parseDoc = parser.ParseDocument(frag).Body;
									e.ReplaceChild(parseDoc, t);
									handled = true;
									break;
							}
						}
					}
				}
			}
			p.Content = document.Body.InnerHtml;
		}
		catch
		{
			// ignored
		}
	}

	public SearchResult<PostDto> SearchPage(Expression<Func<Post, bool>> whereBase, int page, int size, string keyword)
	{
		var cacheKey = $"search:{keyword}:{page}:{size}";
		var result = _cacheManager.GetOrAdd(cacheKey, _ =>
		{
			var searchResult = SearchEngine.ScoredSearch<Post>(BuildSearchOptions(page, size, keyword));
			var entities = searchResult.Results.Where(s => s.Entity.Status == Status.Published).DistinctBy(s => s.Entity.Id).ToList();
			var ids = entities.Select(s => s.Entity.Id).ToArray();
			var dic = GetQuery(whereBase.And(p => ids.Contains(p.Id) && p.LimitMode != RegionLimitMode.OnlyForSearchEngine)).ProjectTo<PostDto>(_mapper.ConfigurationProvider).ToDictionary(p => p.Id);
			var posts = entities.Where(s => dic.ContainsKey(s.Entity.Id)).Select(s => dic[s.Entity.Id]).ToList();
			var simpleHtmlFormatter = new SimpleHTMLFormatter("<span style='color:red;background-color:yellow;font-size: 1.1em;font-weight:700;'>", "</span>");
			var highlighter = new Highlighter(simpleHtmlFormatter, new Segment()) { FragmentSize = 200 };
			var keywords = Searcher.CutKeywords(keyword);
			HighlightSegment(posts, keywords, highlighter);
			SolvePostsCategory(posts);
			return new SearchResult<PostDto>()
			{
				Results = posts,
				Elapsed = searchResult.Elapsed,
				Total = searchResult.TotalHits
			};
		});
		return result;
	}

	public void SolvePostsCategory(IList<PostDto> posts)
	{
		var cids = posts.Select(p => p.CategoryId).Distinct().ToArray();
		var categories = _categoryRepository.GetQuery(c => cids.Contains(c.Id)).Include(c => c.Parent).ToDictionary(c => c.Id);
		posts.ForEach(p => p.Category = _mapper.Map<CategoryDto_P>(categories[p.CategoryId]));
	}

	/// <summary>
	/// 高亮截取处理
	/// </summary>
	/// <param name="posts"></param>
	/// <param name="keywords"></param>
	/// <param name="highlighter"></param>
	private static void HighlightSegment(IList<PostDto> posts, List<string> keywords, Highlighter highlighter)
	{
		foreach (var p in posts)
		{
			p.Content = p.Content.RemoveHtmlTag();
			foreach (var s in keywords)
			{
				string frag;
				if (p.Title.Contains(s) && !string.IsNullOrEmpty(frag = highlighter.GetBestFragment(s, p.Title)))
				{
					p.Title = frag;
					break;
				}
			}

			bool handled = false;
			foreach (var s in keywords)
			{
				string frag;
				if (p.Content.Contains(s) && !string.IsNullOrEmpty(frag = highlighter.GetBestFragment(s, p.Content)))
				{
					p.Content = frag;
					handled = true;
					break;
				}
			}

			if (p.Content.Length > 200 && !handled)
			{
				p.Content = p.Content[..200];
			}
		}
	}

	private static SearchOptions BuildSearchOptions(int page, int size, string keyword)
	{
		keyword = Regex.Replace(keyword, @":\s+", ":");
		var fields = new List<string>();
		var newkeywords = new List<string>();
		foreach (var item in keyword.Split(' ', '　').Where(s => s.Contains(new[] { ":", "：" })))
		{
			var part = item.Split(':', '：');
			var field = typeof(Post).GetProperty(part[0], BindingFlags.IgnoreCase)?.Name;
			if (!string.IsNullOrEmpty(field))
			{
				fields.Add(field);
			}

			newkeywords.Add(part[1]);
		}

		var searchOptions = fields.Any() ? new SearchOptions(newkeywords.Join(" "), page, size, fields.Join(",")) : new SearchOptions(keyword, page, size, typeof(Post));
		if (keyword.Contains(new[] { " ", ",", ";" }))
		{
			searchOptions.Score = 0.3f;
		}

		return searchOptions;
	}

	/// <summary>
	/// 文章所有tag
	/// </summary>
	/// <returns></returns>
	public Dictionary<string, int> GetTags()
	{
		return _postTagsRepository.GetAll(t => t.Count, false).FromCache().ToDictionary(g => g.Name, g => g.Count);
	}

	/// <summary>
	/// 添加实体并保存
	/// </summary>
	/// <param name="t">需要添加的实体</param>
	/// <returns>添加成功</returns>
	public override Post AddEntitySaved(Post t)
	{
		t = base.AddEntity(t);
		SearchEngine.SaveChanges(t.Status == Status.Published);
		return t;
	}

	/// <summary>
	/// 添加实体并保存（异步）
	/// </summary>
	/// <param name="t">需要添加的实体</param>
	/// <returns>添加成功</returns>
	public override Task<int> AddEntitySavedAsync(Post t)
	{
		base.AddEntity(t);
		return SearchEngine.SaveChangesAsync(t.Status == Status.Published);
	}

	/// <summary>
	/// 根据ID删除实体并保存
	/// </summary>
	/// <param name="id">实体id</param>
	/// <returns>删除成功</returns>
	public override bool DeleteById(int id)
	{
		DeleteEntity(GetById(id));
		return SearchEngine.SaveChanges() > 0;
	}

	/// <summary>
	/// 根据ID删除实体并保存（异步）
	/// </summary>
	/// <param name="id">实体id</param>
	/// <returns>删除成功</returns>
	public override Task<int> DeleteByIdAsync(int id)
	{
		base.DeleteById(id);
		return SearchEngine.SaveChangesAsync();
	}

	/// <summary>
	/// 删除多个实体并保存（异步）
	/// </summary>
	/// <param name="list">实体集合</param>
	/// <returns>删除成功</returns>
	public override Task<int> DeleteEntitiesSavedAsync(IEnumerable<Post> list)
	{
		base.DeleteEntities(list);
		return SearchEngine.SaveChangesAsync();
	}

	/// <summary>
	/// 根据条件删除实体
	/// </summary>
	/// <param name="where">查询条件</param>
	/// <returns>删除成功</returns>
	public override int DeleteEntitySaved(Expression<Func<Post, bool>> where)
	{
		base.DeleteEntity(where);
		return SearchEngine.SaveChanges();
	}

	/// <summary>
	/// 删除实体并保存
	/// </summary>
	/// <param name="t">需要删除的实体</param>
	/// <returns>删除成功</returns>
	public override bool DeleteEntitySaved(Post t)
	{
		base.DeleteEntity(t);
		return SearchEngine.SaveChanges() > 0;
	}

	/// <summary>
	/// 根据条件删除实体
	/// </summary>
	/// <param name="where">查询条件</param>
	/// <returns>删除成功</returns>
	public override Task<int> DeleteEntitySavedAsync(Expression<Func<Post, bool>> where)
	{
		base.DeleteEntity(where);
		return SearchEngine.SaveChangesAsync();
	}

	/// <summary>
	/// 统一保存的方法
	/// </summary>
	/// <returns>受影响的行数</returns>
	public int SaveChanges(bool flushIndex)
	{
		return flushIndex ? SearchEngine.SaveChanges() : base.SaveChanges();
	}

	/// <summary>
	/// 统一保存数据
	/// </summary>
	/// <returns>受影响的行数</returns>
	public async Task<int> SaveChangesAsync(bool flushIndex)
	{
		return flushIndex ? await SearchEngine.SaveChangesAsync() : await base.SaveChangesAsync();
	}
}