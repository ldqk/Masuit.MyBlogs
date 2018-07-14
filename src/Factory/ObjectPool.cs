using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace DALFactory
{
    /// <summary>
    /// 对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : ConcurrentQueue<T> where T : class
    {
        private int Limit { get; set; }

        public ObjectPool(int limit = 32)
        {
            Limit = limit;
        }

        public ObjectPool(List<T> list) : base(list)
        {
            Limit = list.Count;
        }

        public new void Enqueue(T item)
        {
            if (Count >= Limit)
            {
                TryDequeue(out var _);
            }

            base.Enqueue(item);
        }
        public T Dequeue(string fullNamespace)
        {
            if (Count < 1 || IsEmpty)
            {
                for (int i = 0; i < Limit; i++)
                {
                    Enqueue(GetInstance(fullNamespace));
                }
            }
            TryDequeue(out var t);
            return t ?? GetInstance(fullNamespace);
        }

        /// <summary>
        /// 根据配置文件中的方式进行反射创建对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullNamespace">程序集全命名空间</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetInstance(string fullNamespace)
        {
            try
            {
                string DalNameSpace = ConfigurationManager.AppSettings["DalNameSpace"] ?? "DAL";//根据命名空间创建实例对象
                Assembly ass = Assembly.Load(DalNameSpace);
                return (T)ass.CreateInstance(fullNamespace);
            }
            catch (Exception)
            {
                try
                {
                    string DalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", ConfigurationManager.AppSettings["DalPath"] ?? "DAL.dll");//根据程序集路径创建实例对象
                    Assembly ass = Assembly.LoadFile(DalPath);
                    return (T)ass.CreateInstance(fullNamespace);
                }
                catch (Exception)
                {
                    //都出错则往外抛异常
                    throw new Exception("找不到程序集");
                }
            }
        }
    }
}