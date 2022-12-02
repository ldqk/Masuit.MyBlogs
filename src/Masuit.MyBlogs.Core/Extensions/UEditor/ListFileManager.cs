namespace Masuit.MyBlogs.Core.Extensions.UEditor;

/// <summary>
/// FileManager 的摘要说明
/// </summary>
public class ListFileManager : Handler
{
	private enum ResultState
	{
		Success,
		InvalidParam,
		AuthorizError,
		IOError,
		PathNotFound
	}

	private int _start;
	private int _size;
	private int _total;
	private ResultState _state;
	private readonly string _pathToList;
	private string[] _fileList;
	private readonly string[] _searchExtensions;

	public ListFileManager(HttpContext context, string pathToList, string[] searchExtensions) : base(context)
	{
		_searchExtensions = searchExtensions.Select(x => x.ToLower()).ToArray();
		_pathToList = pathToList;
	}

	public override Task<string> Process()
	{
		try
		{
			_start = string.IsNullOrEmpty(Request.Query["start"]) ? 0 : Convert.ToInt32(Request.Query["start"]);
			_size = string.IsNullOrEmpty(Request.Query["size"]) ? UeditorConfig.GetInt("imageManagerListSize") : Convert.ToInt32(Request.Query["size"]);
		}
		catch (FormatException)
		{
			_state = ResultState.InvalidParam;
			return Task.FromResult(WriteResult());
		}
		var buildingList = new List<string>();
		try
		{
			var localPath = AppContext.BaseDirectory + "wwwroot" + _pathToList;
			buildingList.AddRange(Directory.GetFiles(localPath, "*", SearchOption.AllDirectories).Where(x => _searchExtensions.Contains(Path.GetExtension(x).ToLower())).Select(x => _pathToList + x.Substring(localPath.Length).Replace("\\", "/")));
			_total = buildingList.Count;
			_fileList = buildingList.OrderBy(x => x).Skip(_start).Take(_size).ToArray();
		}
		catch (UnauthorizedAccessException)
		{
			_state = ResultState.AuthorizError;
		}
		catch (DirectoryNotFoundException)
		{
			_state = ResultState.PathNotFound;
		}
		catch (IOException)
		{
			_state = ResultState.IOError;
		}
		return Task.FromResult(WriteResult());
	}

	private string WriteResult()
	{
		return WriteJson(new
		{
			state = GetStateString(),
			list = _fileList?.Select(x => new
			{
				url = x
			}),
			start = _start,
			size = _size,
			total = _total
		});
	}

	private string GetStateString()
	{
		return _state switch
		{
			ResultState.Success => "SUCCESS",
			ResultState.InvalidParam => "参数不正确",
			ResultState.PathNotFound => "路径不存在",
			ResultState.AuthorizError => "文件系统权限不足",
			ResultState.IOError => "文件系统读取错误",
			_ => "未知错误"
		};
	}
}