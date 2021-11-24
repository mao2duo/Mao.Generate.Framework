using Mao.Core.Models;
using Mao.Generate.Core.Models;
using Mao.Generate.Core.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsPropertyConverter : BaseTypeConverter
    {
        /// <summary>
        /// PropertyDeclarationSyntax To CsProperty
        /// </summary>
        protected CsProperty ConvertFrom(PropertyDeclarationSyntax propertySyntax, TypeDescriptorContext<PropertyDeclarationSyntax, CsProperty> context)
        {
            CsService csService = context.GetService(typeof(CsService)) as CsService;

            CsProperty csProperty = new CsProperty();
            csProperty.Name = propertySyntax.Identifier.Text;

            #region Summary
            var propertySummarySyntax = propertySyntax.GetLeadingTrivia()
                .FirstOrDefault(x =>
                    x.RawKind == (int)SyntaxKind.SingleLineDocumentationCommentTrivia
                    || x.RawKind == (int)SyntaxKind.MultiLineDocumentationCommentTrivia);
            csProperty.Summary = csService.GetSummary(propertySummarySyntax);
            #endregion
            #region Attribute
            csProperty.Attributes = propertySyntax.DescendantNodes()
                .OfType<AttributeSyntax>()?
                .Select(x => ObjectResolver.TypeConvert<AttributeSyntax, CsAttribute>(x))
                .ToArray();
            #endregion
            #region Type
            csProperty.TypeName = propertySyntax.DescendantNodes()
                .OfType<TypeSyntax>()
                .First(x => x.Parent == propertySyntax)
                .ToString();
            #endregion
            #region DefaultValue
            if (propertySyntax.Initializer != null)
            {
                if (propertySyntax.Initializer.Value is LiteralExpressionSyntax defaultValueSyntax)
                {
                    csProperty.DefaultValue = defaultValueSyntax.Token.Value;
                }
                else
                {
                    csProperty.DefaultValue = propertySyntax.Initializer.Value.ToString();
                }
            }
            #endregion

            return csProperty;
        }
        /// <summary>
        /// PropertyInfo To CsProperty
        /// </summary>
        protected CsProperty ConvertFrom(PropertyInfo property)
        {
            CsProperty csProperty = new CsProperty();
            csProperty.TypeName = property.PropertyType.Name;
            csProperty.Name = property.Name;
            // 描述
            var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                csProperty.Summary = descriptionAttribute.Description;
            }
            // 標籤
            csProperty.Attributes = property.GetCustomAttributes()?
                .Select(x => ObjectResolver.TypeConvert<Attribute, CsAttribute>(x))
                .ToArray();
            return csProperty;
        }
        /// <summary>
        /// SqlColumn To CsProperty
        /// </summary>
        protected CsProperty ConvertFrom(SqlColumn sqlColumn)
        {
            CsProperty csProperty = new CsProperty();
            csProperty.Summary = sqlColumn.Description;
            csProperty.TypeName = this.GetTypeNameFrom(sqlColumn);
            csProperty.Name = sqlColumn.Name;
            var attributes = new List<CsAttribute>();
            if (sqlColumn.IsPrimaryKey)
            {
                attributes.Add(new CsAttribute()
                {
                    Name = "Key"
                });
            }
            if (sqlColumn.IsIdentity)
            {
                attributes.Add(new CsAttribute()
                {
                    Name = "DatabaseGenerated",
                    Arguments = new CsAttributeArgument[]
                    {
                        new CsAttributeArgument()
                        {
                            Value = DatabaseGeneratedOption.Identity
                        }
                    }
                });
            }
            if (sqlColumn.IsComputed)
            {
                attributes.Add(new CsAttribute()
                {
                    Name = "DatabaseGenerated",
                    Arguments = new CsAttributeArgument[]
                    {
                        new CsAttributeArgument()
                        {
                            Value = DatabaseGeneratedOption.Computed
                        }
                    }
                });
            }
            return csProperty;
        }
        /// <summary>
        /// 從資料庫的類型取得 C# 類型的名稱
        /// </summary>
        private string GetTypeNameFrom(SqlColumn sqlColumn)
        {
            switch (sqlColumn.TypeName)
            {
                case "bigint":
                    return $"long{(sqlColumn.IsNullable ? "?" : "")}";
                case "binary":
                    return "byte[]";
                case "bit":
                    return $"bool{(sqlColumn.IsNullable ? "?" : "")}";
                case "char":
                    return "string";
                case "date":
                case "datetime":
                case "datetime2":
                    return $"DateTime{(sqlColumn.IsNullable ? "?" : "")}";
                case "datetimeoffset":
                    return $"DateTimeOffset{(sqlColumn.IsNullable ? "?" : "")}";
                case "decimal":
                    return $"decimal{(sqlColumn.IsNullable ? "?" : "")}";
                case "float":
                    return $"double{(sqlColumn.IsNullable ? "?" : "")}";
                case "image":
                    return "byte[]";
                case "int":
                    return $"int{(sqlColumn.IsNullable ? "?" : "")}";
                case "money":
                    return $"decimal{(sqlColumn.IsNullable ? "?" : "")}";
                case "nchar":
                    return "string";
                case "ntext":
                    return "string";
                case "numeric":
                    return $"decimal{(sqlColumn.IsNullable ? "?" : "")}";
                case "nvarchar":
                    return "string";
                case "real":
                    return $"float{(sqlColumn.IsNullable ? "?" : "")}";
                case "smalldatetime":
                    return $"DateTime{(sqlColumn.IsNullable ? "?" : "")}";
                case "smallint":
                    return $"short{(sqlColumn.IsNullable ? "?" : "")}";
                case "smallmoney":
                    return $"decimal{(sqlColumn.IsNullable ? "?" : "")}";
                case "sql_variant":
                    return "object";
                case "text":
                    return "string";
                case "time":
                    return $"TimeSpan{(sqlColumn.IsNullable ? "?" : "")}";
                case "timestamp":
                    return "byte[]";
                case "tinyint":
                    return $"byte{(sqlColumn.IsNullable ? "?" : "")}";
                case "uniqueidentifier":
                    return $"Guid{(sqlColumn.IsNullable ? "?" : "")}";
                case "varbinary":
                    return "byte[]";
                case "varchar":
                    return "string";
                case "xml":
                    return "string";
            }
            return null;
        }
        /// <summary>
        /// CsProperty To String
        /// </summary>
        protected string ConvertTo(CsProperty csProperty)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(csProperty.Summary))
            {
                stringBuilder.AppendLine($@"
/// <summary>
{string.Join("\n", csProperty.Summary.Replace("\r\n", "\n").Split('\n').Select(x => $"/// {x}"))}
/// </summary>".TrimStart('\r', '\n'));
            }
            if (csProperty.Attributes != null && csProperty.Attributes.Any())
            {
                foreach (var csAttribute in csProperty.Attributes)
                {
                    stringBuilder.AppendLine(ObjectResolver.TypeConvert<CsAttribute, string>(csAttribute));
                }
            }
            stringBuilder.Append($"public {csProperty.TypeName} {csProperty.Name} {{ get; set; }}");
            if (!string.IsNullOrEmpty(csProperty.DefaultDefine))
            {
                stringBuilder.Append($" = {csProperty.DefaultDefine};");
            }
            return stringBuilder.ToString();
        }
    }
}
