using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class SqlColumnConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => false;

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(x => x.Name == nameof(ConvertTo)
                    && destinationType.IsAssignableFrom(x.ReturnType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is SqlColumn sqlColumn)
            {
                if (destinationType == typeof(CsProperty))
                {
                    return ConvertTo(sqlColumn);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        protected CsProperty ConvertTo(SqlColumn sqlColumn)
        {
            CsProperty csProperty = new CsProperty();
            csProperty.Summary = sqlColumn.Description;
            csProperty.TypeName = this.GetTypeName(sqlColumn);
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
        protected string GetTypeName(SqlColumn sqlColumn)
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
    }
}
