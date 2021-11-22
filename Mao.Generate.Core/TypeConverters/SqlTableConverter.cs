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

        /// <summary>
        /// 轉換成 SqlTable
        /// </summary>
        protected SqlTable ConvertFrom(CsType csType)
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
