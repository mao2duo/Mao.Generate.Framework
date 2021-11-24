using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsAttributeConverter))]
    public class CsAttribute
    {
        public string Name { get; set; }
        public CsAttributeArgument[] Arguments { get; set; }
    }
}
