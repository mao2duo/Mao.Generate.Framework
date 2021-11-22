using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsEnumConverter))]
    public class CsEnum
    {
        public string Namespace { get; set; }
        public string Summary { get; set; }
        public string BaseTypeName { get; set; }
        public string Name { get; set; }
        public CsAttribute[] Attributes { get; set; }
        public CsEnumMember[] Members { get; set; }
    }
}
