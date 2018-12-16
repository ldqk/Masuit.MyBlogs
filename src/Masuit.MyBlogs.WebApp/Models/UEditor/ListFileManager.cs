using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Masuit.MyBlogs.WebApp.Models.UEditor
{
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

        public override void Process()
        {
            try
            {
                _start = string.IsNullOrEmpty(Request["start"]) ? 0 : Convert.ToInt32(Request["start"]);
                _size = string.IsNullOrEmpty(Request["size"]) ? UeditorConfig.GetInt("imageManagerListSize") : Convert.ToInt32(Request["size"]);
            }
            catch (FormatException)
            {
                _state = ResultState.InvalidParam;
                WriteResult();
                return;
            }
            var buildingList = new List<string>();
            try
            {
                var localPath = Server.MapPath(_pathToList);
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
            finally
            {
                WriteResult();
            }
        }

        private void WriteResult()
        {
            WriteJson(new
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
            switch (_state)
            {
                case ResultState.Success:
                    return "SUCCESS";
                case ResultState.InvalidParam:
                    return "参数不正确";
                case ResultState.PathNotFound:
                    return "路径不存在";
                case ResultState.AuthorizError:
                    return "文件系统权限不足";
                case ResultState.IOError:
                    return "文件系统读取错误";
            }
            return "未知错误";
        }
    }
}