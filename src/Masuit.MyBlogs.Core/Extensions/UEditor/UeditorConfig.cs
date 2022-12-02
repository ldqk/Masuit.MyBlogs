using Newtonsoft.Json.Linq;

namespace Masuit.MyBlogs.Core.Extensions.UEditor;

/// <summary>
/// Config 的摘要说明
/// </summary>
public static class UeditorConfig
{
	public static JObject Items => JObject.Parse(File.ReadAllText(AppContext.BaseDirectory + "App_Data/ueconfig.json"));

	public static T GetValue<T>(string key)
	{
		return Items[key].Value<T>();
	}

	public static String[] GetStringList(string key)
	{
		return Items[key].Select(x => x.Value<String>()).ToArray();
	}

	public static String GetString(string key)
	{
		return GetValue<String>(key);
	}

	public static int GetInt(string key)
	{
		return GetValue<int>(key);
	}
}