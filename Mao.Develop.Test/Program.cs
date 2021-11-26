using Mao.CodeDom.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Develop.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            CodeDomService codeDomService = new CodeDomService();
            CodeDom.Models.UnitSource source = new CodeDom.Models.UnitSource();
            source.SourceCode = @"
using Mao.CodeDom.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Develop.Test
{
    public class Test1
    {
        public void Execute()
        {
            File.WriteAllText(@""D:\Temp\test.txt"", DateTime.Now.ToString(""HHmmss""), Encoding.UTF8);
        }
    }
}";

            var compiled = codeDomService.Compile(source);
            var executable = codeDomService.CreateInstance(compiled);
            executable.GetType().GetMethod("Execute").Invoke(executable, null);
        }
    }
}
