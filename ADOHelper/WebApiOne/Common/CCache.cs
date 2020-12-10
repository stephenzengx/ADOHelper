using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

namespace Common.Helper
{
    public class CCache
    {
        public static void Clear()
        {
            IDictionaryEnumerator enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                HttpRuntime.Cache.Remove( ((DictionaryEntry)enumerator.Current).Key.ToString());
            }
        }

        /// <summary>
        /// 是否存在某key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ExitsKey(string key)
        {
            return (HttpRuntime.Cache.Get(key) == null) ? false : true;
        }

        /// <summary>
        /// 通过key获取value值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetCache(string key)
        {
            return HttpRuntime.Cache.Get(key);
        }

        public static void Remove(string key)
        {
            if (HttpRuntime.Cache.Get(key) != null)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        /// <summary>
        /// 这是key-value 已存在进行覆盖
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static bool SetCache(string key, object value, int seconds)
        {
            if (value == null)
            {
                return false;
            }

            if (HttpRuntime.Cache.Get(key) != null)
            {
                HttpRuntime.Cache.Remove(key);
            }
            if (seconds <= 0)
            {
                HttpRuntime.Cache.Insert(key, value);
            }
            else
            {
                HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddSeconds((double)seconds), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
            }

            return true;
        }

        public static string ToString(object obj)
        {
            return ((obj != null) ? obj.ToString() : ((string)""));
        }
    }
}
