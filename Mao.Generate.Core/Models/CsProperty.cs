using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsPropertyConverter))]
    public class CsProperty
    {
        public string Summary { get; set; }
        public CsAttribute[] Attributes { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public object DefaultValue { get; set; }
        public string DefaultDefine { get; set; }
    }
}
