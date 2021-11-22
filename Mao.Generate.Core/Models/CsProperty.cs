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

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(this.Summary))
            {
                stringBuilder.AppendLine($@"
/// <summary>
{string.Join("\n", this.Summary.Replace("\r\n", "\n").Split('\n').Select(x => $"/// {x}"))}
/// </summary>".TrimStart('\r', '\n'));
            }
            if (this.Attributes != null && this.Attributes.Any())
            {
                foreach (var csAttribute in this.Attributes)
                {
                    stringBuilder.AppendLine(csAttribute.ToString());
                }
            }
            stringBuilder.Append($"public {this.TypeName} {this.Name} {{ get; set; }}");
            if (!string.IsNullOrEmpty(this.DefaultDefine))
            {
                stringBuilder.Append($" = {this.DefaultDefine};");
            }
            return stringBuilder.ToString();
        }
    }
}
