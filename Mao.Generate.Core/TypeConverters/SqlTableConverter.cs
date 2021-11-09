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
    public class SqlTableConverter : TypeConverter
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
            if (value is SqlTable sqlTable)
            {
                if (destinationType == typeof(CsType))
                {
                    return ConvertTo(sqlTable);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        protected CsType ConvertTo(SqlTable sqlTable)
        {
            CsType csType = new CsType();
            csType.Name = sqlTable.Name;
            csType.Summary = sqlTable.Description;
            csType.Properties = sqlTable.Columns?
                .OrderBy(x => x.Order)
                .Select(x => ObjectResolver.TypeConvert<CsProperty>(x))
                .ToArray();
            return csType;
        }
    }
}
