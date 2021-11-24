using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.CodeDom.Models
{
    public class UnitCompiled
    {
        /// <summary>
        /// 編譯時載入的組件
        /// </summary>
        public string[] Assemblies { get; set; }

        /// <summary>
        /// 編譯時的程式碼
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// 編譯錯誤
        /// </summary>
        public List<CompilerError> CompilerErrors { get; } = new List<CompilerError>();

        /// <summary>
        /// 編譯是否成功
        /// </summary>
        public bool CompilerSuccess => CompilerErrors == null || !CompilerErrors.Any();

        /// <summary>
        /// 編譯出來的類型
        /// </summary>
        public Type ResultType { get; set; }
    }
}
