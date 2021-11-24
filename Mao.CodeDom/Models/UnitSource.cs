using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Models
{
    public class UnitSource
    {
        public UnitSource()
        {
            this.Assemblies.Add("Microsoft.CSharp.dll");
            this.Assemblies.Add("System.dll");
            this.Assemblies.Add("System.Data.dll");
        }

        /// <summary>
        /// 需要的組件
        /// </summary>
        public List<string> Assemblies { get; } = new List<string>();

        /// <summary>
        /// 原始程式碼
        /// </summary>
        public string SourceCode { get; set; }
    }

}
