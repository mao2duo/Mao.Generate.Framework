using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsTypeConverter))]
    public class CsType
    {
        public string Summary { get; set; }
        public CsAttribute[] Attributes { get; set; }
        public string Name { get; set; }
        public CsGenericArgument[] GenericArguments { get; set; }
        public string BaseTypeName { get; set; }
        public string[] InterfaceNames { get; set; }
        public CsProperty[] Properties { get; set; }
        public CsMethod[] Methods { get; set; }
    }
}
