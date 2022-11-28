using Masuit.Tools;
using System.Net;
using System.Text;

namespace Masuit.MyBlogs.Core.Common;

/// <summary>
/// QQWryIpSearch 请作为单例使用 数据库缓存在内存
/// </summary>
public sealed class QQWrySearcher : IDisposable
{
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    private object _versionLock = new object();

    private static readonly Encoding Gb2312Encoding;

    /// <summary>
    /// 数据库 缓存
    /// </summary>
    private byte[] _qqwryDbBytes;

    /// <summary>
    /// Ip索引 缓存
    /// </summary>
    private long[] _ipIndexCache;

    /// <summary>
    /// 起始定位
    /// </summary>
    private long _startPosition;

    /// <summary>
    /// 是否初始化
    /// </summary>
    private bool? _init;

    private readonly string _dbPath;

    private int? _ipCount;

    private string _version;

    /// <summary>
    /// 记录总数
    /// </summary>
    public int IpCount
    {
        get
        {
            _ipCount ??= _ipIndexCache.Length;
            return _ipCount.Value;
        }
    }

    /// <summary>
    /// 版本信息
    /// </summary>
    public string Version
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_version))
            {
                return _version;
            }
            lock (_versionLock)
            {
                if (!string.IsNullOrWhiteSpace(_version))
                {
                    return _version;
                }
                _version = GetIpLocation(IPAddress.Parse("255.255.255.255")).Network;
                return _version;
            }
        }
    }

    static QQWrySearcher()
    {
#if NET45

#else
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        Gb2312Encoding = Encoding.GetEncoding("gb2312");
    }

    public QQWrySearcher(string dbPath)
    {
        _dbPath = dbPath;
        Init();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    public bool Init(bool getNewDb = false)
    {
        if (_init != null && !getNewDb)
        {
            return _init.Value;
        }
        _initLock.Wait();
        try
        {
            if (_init != null && !getNewDb)
            {
                return _init.Value;
            }

            _qqwryDbBytes = FileToBytes(_dbPath);

            _ipIndexCache = BlockToArray(ReadIpBlock(_qqwryDbBytes, out _startPosition));

            _ipCount = null;
            _version = null;
            _init = true;
        }
        finally
        {
            _initLock.Release();
        }

        if (_qqwryDbBytes == null)
        {
            throw new InvalidOperationException("无法打开IP数据库" + _dbPath + "！");
        }

        return true;

    }

    /// <summary>
    ///  获取指定IP所在地理位置
    /// </summary>
    /// <param name="ip">要查询的IP地址</param>
    /// <returns></returns>
    public (string City, string Network) GetIpLocation(IPAddress ip)
    {
        if (ip.IsPrivateIP())
        {
            return ("内网", "内网");
        }
        var ipnum = IpToLong(ip);
        return ReadLocation(ipnum, _startPosition, _ipIndexCache, _qqwryDbBytes);
    }

    /// <inheritdoc />
    /// <summary>
    /// 释放
    /// </summary>
    public void Dispose()
    {
        _initLock?.Dispose();
        _versionLock = null;
        _qqwryDbBytes = null;
        _qqwryDbBytes = null;
        _ipIndexCache = null;
        _init = null;
    }

    ///<summary>
    /// 将字符串形式的IP转换位long
    ///</summary>
    ///<param name="ip"></param>
    ///<returns></returns>
    private static long IpToLong(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        var ipBytes = new byte[8];
        for (var i = 0; i < 4; i++)
        {
            ipBytes[i] = bytes[3 - i];
        }

        return BitConverter.ToInt64(ipBytes);
    }

    ///<summary>
    /// 将索引区字节块中的起始IP转换成Long数组
    ///</summary>
    ///<param name="ipBlock"></param>
    private static long[] BlockToArray(byte[] ipBlock)
    {
        var ipArray = new long[ipBlock.Length / 7];
        var ipIndex = 0;
        var temp = new byte[8];
        for (var i = 0; i < ipBlock.Length; i += 7)
        {
            Array.Copy(ipBlock, i, temp, 0, 4);
            ipArray[ipIndex] = BitConverter.ToInt64(temp, 0);
            ipIndex++;
        }
        return ipArray;
    }

    /// <summary>
    ///  从IP数组中搜索指定IP并返回其索引
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="ipArray">IP数组</param>
    /// <param name="start">指定搜索的起始位置</param>
    /// <param name="end">指定搜索的结束位置</param>
    /// <returns></returns>
    private static int SearchIp(long ip, long[] ipArray, int start, int end)
    {
        //二分法 https://baike.baidu.com/item/%E4%BA%8C%E5%88%86%E6%B3%95%E6%9F%A5%E6%89%BE
        while (true)
        {
            //计算中间索引
            var middle = (start + end) / 2;
            if (middle == start)
            {
                return middle;
            }
            else if (ip < ipArray[middle])
            {
                end = middle;
            }
            else
            {
                start = middle;
            }
        }
    }

    ///<summary>
    /// 读取IP文件中索引区块
    ///</summary>
    ///<returns></returns>
    private static byte[] ReadIpBlock(byte[] bytes, out long startPosition)
    {
        long offset = 0;
        startPosition = ReadLongX(bytes, offset, 4);
        offset += 4;
        var endPosition = ReadLongX(bytes, offset, 4);
        offset = startPosition;
        var count = (endPosition - startPosition) / 7 + 1;//总记录数

        var ipBlock = new byte[count * 7];
        for (var i = 0; i < ipBlock.Length; i++)
        {
            ipBlock[i] = bytes[offset + i];
        }
        return ipBlock;
    }

    /// <summary>
    ///  从IP文件中读取指定字节并转换位long
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="bytesCount">需要转换的字节数，主意不要超过8字节</param>
    /// <returns></returns>
    private static long ReadLongX(byte[] bytes, long offset, int bytesCount)
    {
        var cBytes = new byte[8];
        for (var i = 0; i < bytesCount; i++)
        {
            cBytes[i] = bytes[offset + i];
        }
        return BitConverter.ToInt64(cBytes, 0);
    }

    /// <summary>
    ///  从IP文件中读取字符串
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="flag">转向标志</param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private static string ReadString(byte[] bytes, int flag, ref long offset)
    {
        if (flag == 1 || flag == 2)//转向标志
        {
            offset = ReadLongX(bytes, offset, 3);
        }
        else
        {
            offset -= 1;
        }
        var list = new List<byte>();
        var b = bytes[offset];
        offset += 1;
        while (b > 0)
        {
            list.Add(b);
            b = bytes[offset];
            offset += 1;
        }
        return Gb2312Encoding.GetString(list.ToArray());
    }

    private static byte[] FileToBytes(string fileName)
    {
        using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            byte[] bytes = new byte[fileStream.Length];

            fileStream.Read(bytes, 0, bytes.Length);

            fileStream.Close();

            return bytes;
        }
    }

    private static (string City, string Network) ReadLocation(long ip, long startPosition, long[] ipIndex, byte[] qqwryDbBytes)
    {
        long offset = SearchIp(ip, ipIndex, 0, ipIndex.Length) * 7 + 4;

        //偏移
        var arrayOffset = startPosition + offset;
        //跳过结束IP
        arrayOffset = ReadLongX(qqwryDbBytes, arrayOffset, 3) + 4;
        //读取标志
        var flag = qqwryDbBytes[arrayOffset];
        arrayOffset += 1;
        //表示国家和地区被转向
        if (flag == 1)
        {
            arrayOffset = ReadLongX(qqwryDbBytes, arrayOffset, 3);
            //再读标志
            flag = qqwryDbBytes[arrayOffset];
            arrayOffset += 1;
        }
        var countryOffset = arrayOffset;
        var city = ReadString(qqwryDbBytes, flag, ref arrayOffset);

        if (flag == 2)
        {
            arrayOffset = countryOffset + 3;
        }

        flag = qqwryDbBytes[arrayOffset];
        arrayOffset += 1;
        var network = ReadString(qqwryDbBytes, flag, ref arrayOffset);

        return (city, network);
    }
}