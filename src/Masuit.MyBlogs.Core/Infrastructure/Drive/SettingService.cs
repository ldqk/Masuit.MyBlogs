using Masuit.MyBlogs.Core.Models.Drive;

namespace Masuit.MyBlogs.Core.Infrastructure.Drive;

public sealed class SettingService : IDisposable
{
	private readonly DriveContext _context;
	public SettingService(DriveContext context)
	{
		_context = context;
	}

	/// <summary>
	/// 获取设置
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public string Get(string key)
	{
		//判断是否存在
		if (!_context.Settings.Any(setting => setting.Key == key))
		{
			return null;
		}
		var result = _context.Settings.SingleOrDefault(setting => setting.Key == key).Value;
		return result;
	}

	/// <summary>
	/// 创建或更新设置
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public async Task Set(string key, string value)
	{
		if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(key))
		{
			throw new Exception("键值对为空");
		}
		if (string.IsNullOrEmpty(value))
			value = "";
		//已经存在
		if (_context.Settings.Any(setting => setting.Key == key))
		{
			Setting setting = _context.Settings.Single(setting => setting.Key == key);
			setting.Value = value;
			_context.Settings.Update(setting);
		}
		else
		{
			Setting setting = new Setting()
			{
				Key = key,
				Value = value
			};
			await _context.Settings.AddAsync(setting);
		}
		await _context.SaveChangesAsync();
	}

	public void Dispose()
	{
		_context.Dispose();
	}
}