using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Services
{
    public class CsService
    {
        public string Stringify(CsType csType)
        {
            StringBuilder classBuilder = new StringBuilder();
            // 描述
            if (!string.IsNullOrWhiteSpace(csType.Summary))
            {
                classBuilder.AppendLine($@"
/// <summary>
{csType.Summary.Lines().Select(x => $"/// {x}").Join("\n")}
/// </summary>".TrimStart('\r', '\n'));
            }
            // 標籤
            if (csType.Attributes != null && csType.Attributes.Any())
            {
                foreach (var csAttribute in csType.Attributes)
                {
                    classBuilder.AppendLine(this.Stringify(csAttribute));
                }
            }
            // 類別名稱
            classBuilder.Append($"public class {csType.Name}");
            // 泛型參數
            if (csType.GenericArguments != null && csType.GenericArguments.Any())
            {
                classBuilder.Append("<");
                classBuilder.Append(string.Join(", ", csType.GenericArguments.Select(x => x.Name)));
                classBuilder.Append(">");
            }
            // 繼承的類別
            List<string> inherits = new List<string>();
            if (!string.IsNullOrEmpty(csType.BaseTypeName))
            {
                inherits.Add(csType.BaseTypeName);
            }
            if (csType.InterfaceNames != null && csType.InterfaceNames.Any())
            {
                inherits.AddRange(csType.InterfaceNames);
            }
            if (inherits.Any())
            {
                classBuilder.Append($" : {string.Join(", ", inherits)}");
            }
            classBuilder.AppendLine();
            // 泛型約束
            if (csType.GenericArguments != null && csType.GenericArguments.Any())
            {
                StringBuilder constraintBuilder = new StringBuilder();
                foreach (var csGenericArgument in csType.GenericArguments)
                {
                    if (csGenericArgument.Constraints != null && csGenericArgument.Constraints.Any())
                    {
                        constraintBuilder.AppendLine($"where {csGenericArgument.Name} : {csGenericArgument.Constraints.Join(", ")}");
                    }
                }
                if (constraintBuilder.Length > 0)
                {
                    classBuilder.AppendLine(constraintBuilder.ToString().Indent());
                }
            }
            // 主體
            classBuilder.AppendLine($"{{");
            // 屬性
            if (csType.Properties != null && csType.Properties.Any())
            {
                foreach (var csProperty in csType.Properties)
                {
                    classBuilder.AppendLine(this.Stringify(csProperty).Indent());
                    classBuilder.AppendLine();
                }
            }
            // 方法
            if (csType.Methods != null && csType.Methods.Any())
            {
                foreach (var csMethod in csType.Methods)
                {
                    classBuilder.AppendLine(this.Stringify(csMethod).Indent());
                    classBuilder.AppendLine();
                }
            }
            classBuilder.AppendLine($"}}");
            return classBuilder.ToString();
        }
        public string Stringify(CsProperty csProperty)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(csProperty.Summary))
            {
                sb.AppendLine($@"
/// <summary>
{string.Join("\n", csProperty.Summary.Replace("\r\n", "\n").Split('\n').Select(x => $"/// {x}"))}
/// </summary>".TrimStart('\r', '\n'));
            }
            if (csProperty.Attributes != null && csProperty.Attributes.Any())
            {
                foreach (var csAttribute in csProperty.Attributes)
                {
                    sb.AppendLine(this.Stringify(csAttribute));
                }
            }
            sb.Append($"public {csProperty.TypeName} {csProperty.Name} {{ get; set; }}");
            if (!string.IsNullOrEmpty(csProperty.DefaultDefine))
            {
                sb.Append($" = {csProperty.DefaultDefine};");
            }
            return sb.ToString();
        }

        public string Stringify(CsAttribute csAttribute)
        {
            if (csAttribute.Arguments != null && csAttribute.Arguments.Any())
            {
                return $"[{csAttribute.Name}({string.Join(", ", csAttribute.Arguments.Select(x => this.Stringify(x)))})]";
            }
            return $"[{csAttribute.Name}]";
        }
        public string Stringify(CsAttributeArgument csAttributeArgument)
        {
            string left = csAttributeArgument.Name;
            string right;
            if (csAttributeArgument.Value == null)
            {
                right = "null";
            }
            else if (csAttributeArgument.Value is string @string)
            {
                right = $"\"{@string}\"";
            }
            else if (csAttributeArgument.Value is Type type)
            {
                right = $"typeof({type.Name})";
            }
            else
            {
                throw new NotSupportedException();
            }
            if (string.IsNullOrEmpty(left))
            {
                return right;
            }
            return $"{left} = {right}";
        }

        public string Stringify(CsMethod csMethod)
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }
        public string Stringify(CsParameter csParameter)
        {
            StringBuilder sb = new StringBuilder();
            return sb.ToString();
        }
    }
}
