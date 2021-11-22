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

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            // 描述
            if (!string.IsNullOrWhiteSpace(this.Summary))
            {
                stringBuilder.AppendLine($@"
/// <summary>
{this.Summary.Lines().Select(x => $"/// {x}").Join("\n")}
/// </summary>".TrimStart('\r', '\n'));
            }
            // 標籤
            if (this.Attributes != null && this.Attributes.Any())
            {
                foreach (var csAttribute in this.Attributes)
                {
                    stringBuilder.AppendLine(csAttribute.ToString());
                }
            }
            // 類別名稱
            stringBuilder.Append($"public class {this.Name}");
            // 泛型參數
            if (this.GenericArguments != null && this.GenericArguments.Any())
            {
                stringBuilder.Append("<");
                stringBuilder.Append(string.Join(", ", this.GenericArguments.Select(x => x.Name)));
                stringBuilder.Append(">");
            }
            // 繼承的類別
            List<string> inherits = new List<string>();
            if (!string.IsNullOrEmpty(this.BaseTypeName))
            {
                inherits.Add(this.BaseTypeName);
            }
            if (this.InterfaceNames != null && this.InterfaceNames.Any())
            {
                inherits.AddRange(this.InterfaceNames);
            }
            if (inherits.Any())
            {
                stringBuilder.Append($" : {string.Join(", ", inherits)}");
            }
            stringBuilder.AppendLine();
            // 泛型約束
            if (this.GenericArguments != null && this.GenericArguments.Any())
            {
                StringBuilder constraintBuilder = new StringBuilder();
                foreach (var csGenericArgument in this.GenericArguments)
                {
                    if (csGenericArgument.Constraints != null && csGenericArgument.Constraints.Any())
                    {
                        constraintBuilder.AppendLine($"where {csGenericArgument.Name} : {csGenericArgument.Constraints.Join(", ")}");
                    }
                }
                if (constraintBuilder.Length > 0)
                {
                    stringBuilder.AppendLine(constraintBuilder.ToString().Indent());
                }
            }
            // 主體
            stringBuilder.AppendLine($"{{");
            // 屬性
            if (this.Properties != null && this.Properties.Any())
            {
                foreach (var csProperty in this.Properties)
                {
                    stringBuilder.AppendLine(csProperty.ToString().Indent());
                    stringBuilder.AppendLine();
                }
            }
            // 方法
            if (this.Methods != null && this.Methods.Any())
            {
                foreach (var csMethod in this.Methods)
                {
                    stringBuilder.AppendLine(csMethod.ToString().Indent());
                    stringBuilder.AppendLine();
                }
            }
            stringBuilder.AppendLine($"}}");
            return stringBuilder.ToString();
        }
    }
}
