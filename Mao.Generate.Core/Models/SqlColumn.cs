using Mao.Generate.Core.TypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Models
{
    [TypeConverter(typeof(SqlColumnConverter))]
    public class SqlColumn
    {
        public bool IsPrimaryKey { get; set; }

        public string Name { get; set; }
        public string TypeName { get; set; }
        public int Length { get; set; }
        public int Prec { get; set; }
        public int Scale { get; set; }

        public bool IsNullable { get; set; }
        #region
        /// <summary>
        /// 完整類型名稱
        /// </summary>
        public string TypeFullName
        {
            get
            {
                switch (TypeName)
                {
                    case "binary":
                    case "char":
                    case "nchar":
                    case "nvarchar":
                    case "varbinary":
                    case "varchar":
                        return $"{TypeName}({(Length == -1 ? "max" : Convert.ToString(Length))})";
                    case "datetime2":
                    case "datetimeoffset":
                    case "time":
                        return $"{TypeName}({Scale})";
                    case "decimal":
                    case "numeric":
                        return $"{TypeName}({Prec}, {Scale})";
                    default:
                        return TypeName;
                }
            }
            set
            {
                string typeName = value;
                int length = -1;
                int prec = 18;
                int scale = 2;
                if (!string.IsNullOrEmpty(value))
                {
                    Invoker.UsingIf(Regex.Match(value, @"^\s*(?<typeName>\w+)\s*\(\s*(?<length>(\d+|max))\s*\)\s*$"),
                        x => x.Success,
                        match =>
                        {
                            typeName = match.Groups["typeName"].Value;
                            Invoker.Using(match.Groups["length"].Value,
                                lengthString =>
                                {
                                    if (lengthString.ToLower() == "max")
                                    {
                                        length = -1;
                                        return;
                                    }
                                    if (new[] { "datetime2", "datetimeoffset", "time" }.Contains(typeName.ToLower()))
                                    {
                                        scale = Convert.ToInt32(lengthString);
                                        return;
                                    }
                                    length = Convert.ToInt32(lengthString);
                                });
                        },
                        () =>
                        {
                            Invoker.UsingIf(Regex.Match(value, @"^\s*(?<typeName>\w+)\s*\(\s*(?<prec>\d+)\s*,\s*(?<scale>\d+)\s*\)\s*$"),
                                x => x.Success,
                                match =>
                                {
                                    typeName = match.Groups["typeName"].Value;
                                    prec = Convert.ToInt32(match.Groups["prec"].Value);
                                    scale = Convert.ToInt32(match.Groups["scale"].Value);
                                });
                        });
                }
                TypeName = typeName;
                Length = length;
                Prec = prec;
                Scale = scale;
            }
        }
        #endregion

        public bool IsIdentity { get; set; }
        public bool IsComputed { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        public string DefaultDefine { get; set; }
        //public object DefaultConstant { get; set; }

        public int Order { get; set; }

        /// <summary>
        /// 資料欄位允許的類型名稱
        /// </summary>
        public static string[] TypeNames = new []
        {
            "bigint",
            "binary",
            "bit",
            "char",
            "date",
            "datetime",
            "datetime2",
            "datetimeoffset",
            "decimal",
            "float",
            //"geography",
            //"geometry",
            //"hierarchyid",
            "image",
            "int",
            "money",
            "nchar",
            "ntext",
            "numeric",
            "nvarchar",
            "real",
            "smalldatetime",
            "smallint",
            "smallmoney",
            "sql_variant",
            "text",
            "time",
            "timestamp",
            "tinyint",
            "uniqueidentifier",
            "varbinary",
            "varchar",
            "xml",
        };
    }
}
