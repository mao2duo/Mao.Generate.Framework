using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.TypeConverters
{
    public class SqlTableConverter : BaseTypeConverter
    {
        /// <summary>
        /// CsType To SqlTable
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
                .Select(x => ObjectResolver.TypeConvert<CsProperty, SqlColumn>(x))
                .OrderBy(x => x.Order)
                .ToArray();
            return sqlTable;
        }
    }
}
