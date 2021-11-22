using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    /// <summary>
    /// 輸入元件的資料來源
    /// </summary>
    public class UIDataSource<T>
    {
        public UIDataSourceType Type { get; set; }
        public IEnumerable<T> Values { get; set; }
        public HttpMethod ApiHttpMethod { get; set; }
        public string ApiUrl { get; set; }
        public Dictionary<string, object> ApiParameters { get; set; }
        /// <summary>
        /// 將 API 回傳結果轉換成 Values 的方法
        /// </summary>
        public Func<object, IEnumerable<T>> ApiResponseConverter { get; set; }
        /// <summary>
        /// 如何將選項的值轉換成文字
        /// </summary>
        public Func<T, string> Stringifier { get; set; }
    }

    public enum UIDataSourceType
    {
        Static,
        Api
    }
}
