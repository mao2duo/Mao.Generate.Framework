using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class CsTypeConverter : TypeConverter
    {
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
            if (value is Type type)
            {
                return ConvertFrom(type);
            }
            return base.ConvertFrom(context, culture, value);
        }

        protected CsType ConvertFrom(Type type)
        {
            CsType csType = new CsType();
            csType.Name = type.Name;
            csType.BaseTypeName = type.BaseType?.Name;
            csType.InterfaceNames = type.GetInterfaces()?.Select(x => x.Name).ToArray();
            // TODO: GenericArguments
            // 描述
            var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttribute != null)
            {
                csType.Summary = descriptionAttribute.Description;
            }
            // 標籤
            csType.Attributes = type.GetCustomAttributes()?
                .Select(x => ObjectResolver.TypeConvert<CsAttribute>(x))
                .ToArray();
            // 成員
            csType.Properties = type.GetProperties()
                .Select(x => ObjectResolver.TypeConvert<CsProperty>(x))
                .ToArray();
            // 方法
            var methods = type.GetMethods()
                .Where(x => x.DeclaringType == type)
                .ToArray();
            if (methods != null && methods.Any())
            {
                var csMethods = new List<CsMethod>();
                foreach (var method in methods)
                {
                    var csMethod = new CsMethod();
                    csMethod.Name = method.Name;
                    // 執行階段無法取得 FullCode
                    csMethod.FullCode = null;
                    csMethods.Add(csMethod);
                }
                csType.Methods = csMethods.ToArray();
            }
            return csType;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return this.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(x => x.Name == nameof(ConvertTo)
                    && destinationType.IsAssignableFrom(x.ReturnType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is CsType csType)
            {
                if (destinationType == typeof(SqlTable))
                {
                    return ConvertTo(csType);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// 轉換成 SqlTable
        /// </summary>
        protected SqlTable ConvertTo(CsType csType)
        {
            SqlTable sqlTable = new SqlTable();
            if (csType.Attributes != null && csType.Attributes.Any(x => x.Name == "Table" || x.Name == "TableAttribute"))
            {
                sqlTable.Name = csType.Attributes.First(x => x.Name == "Table" || x.Name == "TableAttribute")
                    .Arguments[0].Value as string;
            }
            else
            {
                sqlTable.Name = csType.Name;
            }
            sqlTable.Description = csType.Summary;
            sqlTable.Columns = csType.Properties
                .Where(x => x.Attributes == null
                    || !x.Attributes.Any(y => y.Name == "NotMapped" || y.Name == "NotMappedAttribute"))
                .Select(x => ObjectResolver.TypeConvert<SqlColumn>(x))
                .OrderBy(x => x.Order)
                .ToArray();
            return sqlTable;
        }
    }
}
