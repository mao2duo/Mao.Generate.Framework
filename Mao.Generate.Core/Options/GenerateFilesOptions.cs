using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Options
{
    public class GenerateFilesOptions
    {
        /// <summary>
        /// 取得預設檔案名稱的方法
        /// </summary>
        public Func<string> DefaultNameFactory { get; set; }

        /// <summary>
        /// 指定執行的方法名稱
        /// </summary>
        public string[] ExecuteMethodNames { get; set; }

        /// <summary>
        /// 指定不執行的方法名稱
        /// </summary>
        public string[] IgnoreMethodNames { get; set; }
    }
}
