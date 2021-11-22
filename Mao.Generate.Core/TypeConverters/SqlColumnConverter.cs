using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class SqlColumnConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => false;
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(x => x.Name == nameof(ConvertFrom)
                    && x.GetParameters().Length == 1
                    && x.GetParameters()[0].ParameterType.IsAssignableFrom(sourceType));
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                var convert = this.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.Name == nameof(ConvertFrom)
                        && x.GetParameters().Length == 1
                        && x.GetParameters()[0].ParameterType.IsAssignableFrom(value.GetType()));
                if (convert != null)
                {
                    convert.Invoke(this, new object[] { value });
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        protected SqlColumn ConvertFrom(CsProperty csProperty)
        {
            SqlColumn sqlColumn = new SqlColumn();

            var columnAttribute = csProperty.Attributes?.FirstOrDefault(x => x.Name == "Column" || x.Name == "ColumnAttribute");

            // 如果有 [Column] 則使用指定的字串當作欄位名稱
            if (columnAttribute != null && string.IsNullOrEmpty(columnAttribute.Arguments[0].Name))
            {
                sqlColumn.Name = columnAttribute.Arguments[0].Value as string;
            }
            else
            {
                sqlColumn.Name = csProperty.Name;
            }
            // 如果有 [Column(Order = N)] 則指定欄位的排序
            if (columnAttribute != null && columnAttribute.Arguments.Any(y => y.Name == "Order"))
            {
                sqlColumn.Order = (int)columnAttribute.Arguments.First(y => y.Name == "Order").Value;
            }

            // 如果有 [Key] 或名稱為 Id 就設為主鍵
            if (csProperty.Attributes != null && csProperty.Attributes.Any(x =>
                new[]
                {
                    "Key",
                    "KeyAttribute"
                }.Contains(x.Name)))
            {
                sqlColumn.IsPrimaryKey = true;
            }
            else
            {
                sqlColumn.IsPrimaryKey = sqlColumn.Name.Equals("Id", StringComparison.OrdinalIgnoreCase);
            }

            // 如果有 [DataType] 則使用指定的字串當作完整類型名稱
            if (csProperty.Attributes != null
                && csProperty.Attributes.Any(x => new[] { "DataType", "DataTypeAttribute" }.Contains(x.Name)
                    && x.Arguments[0].Value is string))
            {
                sqlColumn.TypeFullName = csProperty.Attributes.First(x => new[] { "DataType", "DataTypeAttribute" }.Contains(x.Name))
                    .Arguments[0].Value as string;
            }
            // 如果有 [Column(TypeName = "")] 則使用指定的字串當作完整類型名稱
            else if (columnAttribute != null && columnAttribute.Arguments.Any(y => y.Name == "TypeName"))
            {
                sqlColumn.TypeFullName = columnAttribute.Arguments.First(y => y.Name == "TypeName").Value as string;
            }
            else
            {
                sqlColumn.TypeName = this.GetTypeName(csProperty);
                if (!string.IsNullOrEmpty(sqlColumn.TypeName))
                {
                    // 如果有 [StringLength] 或 [MaxLength] 則指定字串的長度
                    if (sqlColumn.TypeName.EndsWith("char")
                        && csProperty.Attributes != null && csProperty.Attributes.Any(x =>
                            new[]
                            {
                                "StringLength",
                                "StringLengthAttribute",
                                "MaxLength",
                                "MaxLengthAttribute"
                            }.Contains(x.Name)))
                    {
                        sqlColumn.Length = Convert.ToInt32(csProperty.Attributes.First(x =>
                            new[]
                            {
                                "StringLength",
                                "StringLengthAttribute",
                                "MaxLength",
                                "MaxLengthAttribute"
                            }.Contains(x.Name)).Arguments[0].Value);
                    }
                }
            }

            // 如果有 [Required] 則不允許 NULL
            if (csProperty.Attributes != null && csProperty.Attributes.Any(x => x.Name == "Required" || x.Name == "RequiredAttribute"))
            {
                sqlColumn.IsNullable = false;
            }
            // 主索引鍵不為 NULL
            else if (sqlColumn.IsPrimaryKey)
            {
                sqlColumn.IsNullable = false;
            }
            else
            {
                sqlColumn.IsNullable = csProperty.TypeName.EndsWith("?")
                    || Regex.IsMatch(csProperty.TypeName, @"^\s*Nullable\s*\<\s*\S+\s*\>\s*$")
                    || csProperty.TypeName.Equals("string", StringComparison.OrdinalIgnoreCase);
            }

            // TODO: [DefaultValue]
            if (csProperty.DefaultValue != null)
            {
                if (csProperty.DefaultValue.Equals("DateTime.Now"))
                {
                    sqlColumn.DefaultDefine = "GETDATE()";
                }
                else if (csProperty.DefaultValue.Equals("string.Empty"))
                {
                    sqlColumn.DefaultDefine = "('')";
                }
                else if (csProperty.TypeName.Equals("string", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(csProperty.DefaultValue.ToString()))
                {
                    sqlColumn.DefaultDefine = $"('{csProperty.DefaultValue}')";
                }
            }

            // TODO: [Description]
            sqlColumn.Description = csProperty.Summary;
            return sqlColumn;
        }

        protected string GetVariableDefine(string typeName, object value)
        {
            throw new NotImplementedException();
        }

        protected string GetTypeName(CsProperty csProperty)
        {
            switch (csProperty.TypeName)
            {
                case "bool":
                case "bool?":
                    return "bit";
                case "byte":
                case "byte?":
                    return "tinyint";
                case "short":
                case "short?":
                    return "smallint";
                case "int":
                case "int?":
                    return "int";
                case "long":
                case "long?":
                    return "bigint";
                case "float":
                case "float?":
                    return "real";
                case "double":
                case "double?":
                    return "float";
                case "decimal":
                case "decimal?":
                    return "decimal";
                case "DateTime":
                case "DateTime?":
                    return "datetime";
                case "DateTimeOffset":
                case "DateTimeOffset?":
                    return "datetimeoffset";
                case "TimeSpan":
                case "TimeSpan?":
                    return "time";
                case "Guid":
                case "Guid?":
                    return "uniqueidentifier";
                case "byte[]":
                    return "varbinary";
                case "string":
                    return "nvarchar";
                case "object":
                    return "sql_variant";
                default:
                    throw new NotImplementedException(csProperty.TypeName);
            }
        }
    }
}
