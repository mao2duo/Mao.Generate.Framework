using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace System.Collections.Generic
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// 加入多個 Key/Value
        /// </summary>
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> keyValues)
        {
            if (keyValues != null)
            {
                foreach (var keyValue in keyValues)
                {
                    dictionary.Add(keyValue);
                }
            }
        }
        /// <summary>
        /// 嘗試加入 Key/Value, 如果已經有存在相同的 Key 則不加入並傳回 false
        /// </summary>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 嘗試加入多個 Key/Value，傳回 Key 不重複有成功加入的數量
        /// </summary>
        public static int TryAddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> keyValues)
        {
            int count = 0;
            if (keyValues != null)
            {
                foreach (var keyValue in keyValues)
                {
                    if (dictionary.TryAdd(keyValue.Key, keyValue.Value))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        /// <summary>
        /// 轉換成 Url.Query 的 name/value 形式
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> ToUrlQueries(this IDictionary<string, object> dictionary, string prefix = null)
        {
            List<KeyValuePair<string, string>> queries = new List<KeyValuePair<string, string>>();
            foreach (var keyValue in dictionary)
            {
                var key = $"{prefix}.{keyValue.Key}".TrimStart('.');
                var value = keyValue.Value;
                if (value is IEnumerable<IDictionary<string, object>> subDictionaries)
                {
                    foreach (var subDictionary in subDictionaries)
                    {
                        queries.AddRange(subDictionary.ToUrlQueries(key));
                    }
                    continue;
                }
                if (value is IDictionary<string, object> _subDictionary)
                {
                    queries.AddRange(_subDictionary.ToUrlQueries(key));
                    continue;
                }
                if (value is IEnumerable enumerable)
                {
                    foreach (var single in enumerable)
                    {
                        queries.Add(new KeyValuePair<string, string>(key, ToUrlQueryValueString(single)));
                    }
                    continue;
                }
                queries.Add(new KeyValuePair<string, string>(key, ToUrlQueryValueString(value)));
            }
            return queries;
        }
        private static string ToUrlQueryValueString(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToLongTimeString();
            }
            return value?.ToString();
        }
        /// <summary>
        /// 轉換成 Url.QueryString
        /// </summary>
        public static string ToUrlQueryString(this IDictionary<string, object> dictionary, string prefix = null)
        {
            IEnumerable<KeyValuePair<string, string>> queries = dictionary.ToUrlQueries(prefix);
            return string.Join("&", queries.Select(x => $"{x.Key}={WebUtility.UrlEncode(x.Value)}"));
        }
    }
}
