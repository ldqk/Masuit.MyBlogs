using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Masuit.MyBlogs.Core.Extensions.UEditor
{
    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public static class UeditorConfig
    {
        private static bool noCache = true;
        private static JObject BuildItems()
        {
            var json = File.ReadAllText(AppContext.BaseDirectory + "ueconfig.json");
            return JObject.Parse(json);
        }

        public static JObject Items
        {
            get
            {
                if (noCache || _items == null)
                {
                    _items = BuildItems();
                }
                return _items;
            }
        }

        private static JObject _items;

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
}