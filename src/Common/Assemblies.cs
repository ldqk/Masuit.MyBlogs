using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Common
{
    /// <summary>
    /// 程序集缓存
    /// </summary>
    public static class Assemblies
    {
        public static Assembly ServiceAssembly { get; set; } = Assembly.LoadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", ConfigurationManager.AppSettings["BllPath"] ?? "BLL.dll"));
        public static Assembly RepositoryAssembly { get; set; } = Assembly.LoadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", ConfigurationManager.AppSettings["DalPath"] ?? "DAL.dll"));
    }
}