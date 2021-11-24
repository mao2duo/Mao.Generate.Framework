using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsAttributeArgumentConverter))]
    public class CsAttributeArgument
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
