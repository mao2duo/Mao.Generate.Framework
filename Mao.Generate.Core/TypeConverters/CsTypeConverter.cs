using Mao.Core.Models;
using Mao.Generate.Core.Models;
using Mao.Generate.Core.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsTypeConverter : BaseTypeConverter
    {
        /// <summary>
        /// ClassDeclarationSyntax To CsType
        /// </summary>
        protected CsType ConvertFrom(ClassDeclarationSyntax classSyntax, TypeDescriptorContext<ClassDeclarationSyntax, CsType> context)
        {
            CsService csService = context.GetService(typeof(CsService)) as CsService;

            CsType csType = new CsType();
            csType.Name = classSyntax.Identifier.Text;

            // TODO: Namespace, BaseTypeName

            #region Summary
            var classSummarySyntax = classSyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            csType.Summary = csService.GetSummary(classSummarySyntax);
            #endregion
            #region Attribute
            csType.Attributes = classSyntax.DescendantNodes()
                .OfType<AttributeSyntax>()?
                .Select(x => ObjectResolver.TypeConvert<AttributeSyntax, CsAttribute>(x))
                .ToArray();
            #endregion
            #region Properties
            csType.Properties = classSyntax.DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Where(x => x.Parent == classSyntax)
                .Select(x => ObjectResolver.TypeConvert<PropertyDeclarationSyntax, CsProperty>(x))
                .ToArray();
            #endregion
            #region Methods
            csType.Methods = classSyntax.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(x => x.Parent == classSyntax)
                .Select(x => ObjectResolver.TypeConvert<MethodDeclarationSyntax, CsMethod>(x))
                .ToArray();
            #endregion

            return csType;
        }
        /// <summary>
        /// Type To CsType
        /// </summary>
        protected CsType ConvertFrom(Type type)
        {
            CsType csType = new CsType();
            csType.Name = type.Name;
            csType.BaseTypeName = type.BaseType?.Name;
            csType.InterfaceNames = type.GetInterfaces()?
                .Select(x => x.Name)
                .ToArray();
            // TODO: GenericArguments
            // 描述
            var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                csType.Summary = descriptionAttribute.Description;
            }
            // 標籤
            csType.Attributes = type.GetCustomAttributes()?
                .Select(x => ObjectResolver.TypeConvert<Attribute, CsAttribute>(x))
                .ToArray();
            // 成員
            csType.Properties = type.GetProperties()
                .Select(x => ObjectResolver.TypeConvert<PropertyInfo, CsProperty>(x))
                .ToArray();
            // 方法
            csType.Methods = type.GetMethods()
                .Where(x => x.DeclaringType == type)
                .Select(x => ObjectResolver.TypeConvert<MethodInfo, CsMethod>(x))
                .ToArray();
            return csType;
        }
        /// <summary>
        /// SqlTable To CsType
        /// </summary>
        protected CsType ConvertFrom(SqlTable sqlTable)
        {
            CsType csType = new CsType();
            csType.Name = sqlTable.Name;
            csType.Summary = sqlTable.Description;
            csType.Properties = sqlTable.Columns?
                .OrderBy(x => x.Order)
                .Select(x => ObjectResolver.TypeConvert<SqlColumn, CsProperty>(x))
                .ToArray();
            return csType;
        }
        /// <summary>
        /// CsType To String
        /// </summary>
        public string ConvertToString(CsType csType)
        {
            StringBuilder stringBuilder = new StringBuilder();
            // 描述
            if (!string.IsNullOrWhiteSpace(csType.Summary))
            {
                stringBuilder.AppendLine($@"
/// <summary>
{csType.Summary.Lines().Select(x => $"/// {x}").Join("\n")}
/// </summary>".TrimStart('\r', '\n'));
            }
            // 標籤
            if (csType.Attributes != null && csType.Attributes.Any())
            {
                foreach (var csAttribute in csType.Attributes)
                {
                    stringBuilder.AppendLine(csAttribute.ToString());
                }
            }
            // 類別名稱
            stringBuilder.Append($"public class {csType.Name}");
            // 泛型參數
            if (csType.GenericArguments != null && csType.GenericArguments.Any())
            {
                stringBuilder.Append("<");
                stringBuilder.Append(string.Join(", ", csType.GenericArguments.Select(x => x.Name)));
                stringBuilder.Append(">");
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
                stringBuilder.Append($" : {string.Join(", ", inherits)}");
            }
            stringBuilder.AppendLine();
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
                    stringBuilder.AppendLine(constraintBuilder.ToString().Indent());
                }
            }
            // 主體
            stringBuilder.AppendLine($"{{");
            // 屬性
            if (csType.Properties != null && csType.Properties.Any())
            {
                foreach (var csProperty in csType.Properties)
                {
                    stringBuilder.AppendLine(ObjectResolver.TypeConvert<CsProperty, string>(csProperty).Indent());
                    stringBuilder.AppendLine();
                }
            }
            // 方法
            if (csType.Methods != null && csType.Methods.Any())
            {
                foreach (var csMethod in csType.Methods)
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
