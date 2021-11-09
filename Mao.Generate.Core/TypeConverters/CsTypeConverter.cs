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
            sqlTable.Columns = csType.Properties
                .Where(x => x.Attributes == null
                    || !x.Attributes.Any(y => y.Name == "NotMapped" || y.Name == "NotMappedAttribute"))
                .Select(x => Convert.ChangeType(x, typeof(SqlColumn)) as SqlColumn)
                .ToArray();
            return sqlTable;
        }
    }
}
