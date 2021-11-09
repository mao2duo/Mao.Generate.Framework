using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    public class CsMethod
    {
        public string Summary { get; set; }
        public string ReturnTypeName { get; set; }
        public string Name { get; set; }
        public CsAttribute[] Attributes { get; set; }
        public CsParameter[] Parameters { get; set; }
        public string FullCode { get; set; }
    }
}
