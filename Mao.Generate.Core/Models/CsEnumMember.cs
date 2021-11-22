using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(CsEnumMemberConverter))]
    public class CsEnumMember
    {
        public string Summary { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public CsAttribute[] Attributes { get; set; }
    }
}
