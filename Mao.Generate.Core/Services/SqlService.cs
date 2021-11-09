using Dapper;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Services
{
    public class SqlService
    {
        ///// <summary>
        ///// 取得不包含系統資料庫的資料庫列表
        ///// </summary>
        //public SqlDatabase[] GetDatabases(string connectionString)
        //{
        //    using (var conn = new SqlConnection(connectionString))
        //    {
        //        return conn.Query<SqlDatabase>(@"
        //            SELECT database_id AS Id, 
        //                   [name], 
        //                   state 
        //            FROM   sys.databases 
        //            WHERE  [name] NOT IN ( 'master', 'tempdb', 'model', 'msdb' ) 
        //            ORDER  BY [name] ").ToArray();
        //    }
        //}
        /// <summary>
        /// 取得不包含系統資料庫的所有資料庫名稱
        /// </summary>
        public string[] GetDatabaseNames(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<string>(@"
                    SELECT [name] 
                    FROM   sys.databases 
                    WHERE  [name] NOT IN ( 'master', 'tempdb', 'model', 'msdb' ) 
                    ORDER  BY [name] ").ToArray();
            }
        }
        /// <summary>
        /// 取得資料庫的所有資料表名稱
        /// </summary>
        public string[] GetTableNames(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<string>(@"
                    SELECT [name]
                    FROM   sysobjects
                    WHERE  [type] = 'U'
                    ORDER BY [name]").ToArray();
            }
        }
        /// <summary>
        /// 取得資料表的所有欄位名稱
        /// </summary>
        public string[] GetColumnNames(string connectionString, string tableName)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<string>(@"
                    SELECT c.[name] 
                    FROM   sys.columns c 
                           INNER JOIN syscolumns sc ON c.object_id = sc.id AND c.column_id = sc.colid 
                           INNER JOIN sys.objects o ON c.object_id = o.object_id 
                    WHERE  o.type = 'U' AND o.[name] = @TableName 
                    ORDER  BY sc.colorder ",
                    new
                    {
                        TableName = tableName
                    }).ToArray();
            }
        }
        /// <summary>
        /// 取得資料庫的所有檢視的名稱
        /// </summary>
        public string[] GetViewNames(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<string>(@"
                    SELECT [name]
                    FROM   sysobjects
                    WHERE  type = 'V'
                           AND category = 0
                    ORDER  BY [name] ").ToArray();
            }
        }
        /// <summary>
        /// 取得資料庫的所有預存程序的名稱
        /// </summary>
        public string[] GetProcedureNames(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<string>(@"
                    SELECT [name]
                    FROM   sysobjects
                    WHERE  type = 'P'
                           AND category = 0
                    ORDER  BY [name] ").ToArray();
            }
        }
        /// <summary>
        /// 從資料庫讀出資料表的結構
        /// </summary>
        public SqlTable GetSqlTable(string connectionString, string tableName)
        {
            SqlTable sqlTable = new SqlTable();
            using (var conn = new SqlConnection(connectionString))
            {
                sqlTable.Name = tableName;
                sqlTable.Columns = conn.Query<SqlColumn>(@"
                    SELECT c.column_id                            AS Id,
                           CASE WHEN EXISTS (SELECT *
                                             FROM   sys.index_columns AS ic
                                                    LEFT JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                                             WHERE  ic.column_id = c.column_id AND i.object_id = c.object_id AND i.is_primary_key = 1) THEN 1 ELSE 0
                                END AS IsPrimaryKey,
                           c.[name]                               AS [Name],
                           t.[name]                               AS TypeName,
                           sc.prec                                AS [Length],
                           c.[precision]                          AS Prec,
                           c.scale                                AS Scale,
                           c.is_nullable                          AS IsNullable,
                           c.is_identity                          AS IsIdentity,
                           c.is_computed                          AS IsComputed,
                           Object_definition(c.default_object_id) AS DefaultDefine,
                           p_des.value                            AS Description
                    FROM   sys.columns c
                           INNER JOIN syscolumns sc ON c.object_id = sc.id AND c.column_id = sc.colid
                           INNER JOIN sys.objects o ON c.object_id = o.object_id
                           LEFT JOIN sys.types t ON t.user_type_id = c.user_type_id
                           LEFT JOIN sys.extended_properties p_des ON c.object_id = p_des.major_id AND c.column_id = p_des.minor_id AND p_des.[name] = 'MS_Description'
                    WHERE  o.type = 'U' AND o.[name] = @TableName
                    ORDER  BY sc.colorder ",
                    new { TableName = tableName }).ToArray();
            }
            return sqlTable;
        }
        /// <summary>
        /// 取得資料庫的關聯性
        /// </summary>
        public SqlForeignKey[] GetSqlForeignKeys(string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<SqlForeignKey>(@"
                    SELECT fk.[name]    AS Name, 
                           o.[name]     AS TableName, 
                           c.[name]     AS ColumnName, 
                           ref_o.[name] AS ReferencedTableName, 
                           ref_c.[name] AS ReferencedColumnName 
                    FROM   sys.objects o 
                           INNER JOIN sys.columns c ON o.object_id = c.object_id 
                           INNER JOIN sys.foreign_key_columns fkc ON fkc.parent_object_id = o.object_id AND fkc.parent_column_id = c.column_id 
                           INNER JOIN sys.foreign_keys fk ON fkc.constraint_object_id = fk.object_id 
                           INNER JOIN sys.objects ref_o ON ref_o.object_id = fkc.referenced_object_id AND ref_o.type = 'U' 
                           INNER JOIN sys.columns ref_c ON ref_c.object_id = ref_o.object_id AND ref_c.column_id = fkc.referenced_column_id 
                    WHERE  o.type = 'U' ").ToArray();
            }
        }

        /// <summary>
        /// 取得不包含括號與引號的預設值內容
        /// </summary>
        public string ConvertDefaultToDefaultValue(string @default)
        {
            if (!string.IsNullOrWhiteSpace(@default))
            {
                if (@default.StartsWith("(N'") && @default.EndsWith("')"))
                {
                    return @default.Substring(3, @default.Length - 5);
                }
                else if (@default.StartsWith("('") && @default.EndsWith("')"))
                {
                    return @default.Substring(2, @default.Length - 4);
                }
                else if (@default.StartsWith("((") && @default.EndsWith("))"))
                {
                    return @default.Substring(2, @default.Length - 4);
                }
                else if (@default.StartsWith("(") && @default.EndsWith(")"))
                {
                    return @default.Substring(1, @default.Length - 2);
                }
            }
            return @default;
        }

        /// <summary>
        /// 取得 SQL Server 新增資料表時所提供的所有完整資料類型名稱
        /// </summary>
        public IEnumerable<string> GetTypeFullNames()
        {
            yield return "bigint";
            yield return "binary(50)";
            yield return "bit";
            yield return "char(10)";
            yield return "date";
            yield return "datetime";
            yield return "datetime2(7)";
            yield return "datetimeoffset(7)";
            yield return "decimal(18, 0)";
            yield return "float";
            //yield return "geography";
            //yield return "geometry";
            //yield return "hierarchyid";
            yield return "image";
            yield return "int";
            yield return "money";
            yield return "nchar(10)";
            yield return "ntext";
            yield return "numeric(18, 0)";
            yield return "nvarchar(50)";
            yield return "nvarchar(max)";
            yield return "real";
            yield return "smalldatetime";
            yield return "smallint";
            yield return "smallmoney";
            yield return "sql_variant";
            yield return "text";
            yield return "time(7)";
            yield return "timestamp";
            yield return "tinyint";
            yield return "uniqueidentifier";
            yield return "varbinary(50)";
            yield return "varbinary(max)";
            yield return "varchar(50)";
            yield return "varchar(max)";
            yield return "xml";
        }

        ///// <summary>
        ///// 透過 Sql 驗證類型與值是否相符
        ///// </summary>
        //public bool SqlValidate(string connectionString, string typeName, object value)
        //{
        //    try
        //    {
        //        using (var conn = new SqlConnection(connectionString))
        //        {
        //            if (value is string s && Regex.IsMatch(s, @"^[_A-Za-z]+\([\S\s]*\)$"))
        //            {
        //                // 將 value 當作 Sql 的方法
        //                conn.QueryFirst($"SELECT CONVERT({typeName}, {value})");
        //            }
        //            else
        //            {
        //                conn.QueryFirst($"SELECT CONVERT({typeName}, {GenerateSql(new SqlConstant(typeName, false, value))})");
        //            }
        //        }
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// 為值加上引號
        /// </summary>
        public string AddQuotesIfNotFunction(object value)
        {
            var s = Convert.ToString(value);
            if (Regex.IsMatch(s, @"^[_A-Za-z]+\([\S\s]*\)$"))
            {
                return s;
            }
            return $"N'{s.Replace("'", "''").Replace("]", "]]")}'";
        }

        /// <summary>
        /// 新增或修改資料欄位的描述
        /// </summary>
        public void AddOrUpdateDescription(string connectionString, string tableName, string columnName, string description)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                if (!string.IsNullOrEmpty(description))
                {
                    conn.Execute(@"
                        IF EXISTS (
	                        SELECT 1 
	                        FROM   sys.columns c 
		                           INNER JOIN syscolumns sc ON c.object_id = sc.id AND c.column_id = sc.colid 
		                           INNER JOIN sys.objects o ON c.object_id = o.object_id 
		                           INNER JOIN sys.extended_properties p ON c.object_id = p.major_id AND c.column_id = p.minor_id AND p.[name] = 'MS_Description' 
	                        WHERE  o.type = 'U' AND o.[name] = @TableName AND c.[name] = @ColumnName 
                        ) 
	                        EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName, @level2type=N'COLUMN', @level2name=@ColumnName 
                        ELSE 
	                        EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName, @level2type=N'COLUMN', @level2name=@ColumnName ",
                        new
                        {
                            TableName = tableName,
                            ColumnName = columnName,
                            Description = description
                        });
                }
                else
                {
                    conn.Execute(@"
                        IF EXISTS (
	                        SELECT 1 
	                        FROM   sys.columns c 
		                           INNER JOIN syscolumns sc ON c.object_id = sc.id AND c.column_id = sc.colid 
		                           INNER JOIN sys.objects o ON c.object_id = o.object_id 
		                           INNER JOIN sys.extended_properties p ON c.object_id = p.major_id AND c.column_id = p.minor_id AND p.[name] = 'MS_Description' 
	                        WHERE  o.type = 'U' AND o.[name] = @TableName AND c.[name] = @ColumnName 
                        ) 
	                        EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName, @level2type=N'COLUMN', @level2name=@ColumnName ",
                        new
                        {
                            TableName = tableName,
                            ColumnName = columnName
                        });
                }
            }
        }
    }
}
