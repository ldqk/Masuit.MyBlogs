using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace DALFactory
{
    /// <summary>
    /// 反射工厂
    /// </summary>
    public static class Factory
    {
        /// <summary>
        /// 根据配置文件中的方式进行反射创建对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullNamespace">程序集全命名空间</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T CreateInstance<T>(string fullNamespace)
        {
            try
            {
                string dalNameSpace = ConfigurationManager.AppSettings["DalNameSpace"] ?? "DAL";//根据命名空间创建实例对象
                Assembly ass = Assembly.Load(dalNameSpace);
                return (T)ass.CreateInstance(fullNamespace);
            }
            catch
            {
                try
                {
                    string dalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", ConfigurationManager.AppSettings["DalPath"] ?? "DAL.dll");//根据程序集路径创建实例对象
                    Assembly ass = Assembly.LoadFile(dalPath);
                    return (T)ass.CreateInstance(fullNamespace);
                }
                catch
                {
                    //都出错则往外抛异常
                    throw new Exception("找不到程序集");
                }
            }
        }
    }
}