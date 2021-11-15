using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Generate.Core.Services
{
    /// <summary>
    /// 當環境無法連接資料庫的替代方案
    /// </summary>
    public class RemoteSqlService : SqlService
    {
        /// <summary>
        /// 用來產出語法與讀取查詢結果的目錄位置
        /// </summary>
        public string TempPath { get; set; }

        /// <summary>
        /// 等候遠端查詢結果的方法
        /// <para>parameters: 提示訊息</para>
        /// <para>return: 是否成功取得查詢結果</para>
        /// </summary>
        public Func<string, bool> Awaiter { get; set; }

        protected virtual string GetRemoteData(string sqlCommand, string commandFileName, string dataFileName)
        {
            if (string.IsNullOrEmpty(TempPath))
            {
                throw new ArgumentNullException(nameof(TempPath));
            }

            string commandFilePath = Path.Combine(TempPath, commandFileName.ToFileName());
            string dataFilePath = Path.Combine(TempPath, dataFileName.ToFileName());
            Directory.CreateDirectory(TempPath);
            File.WriteAllText(commandFilePath, sqlCommand, Encoding.UTF8);

            do
            {
                if (!Awaiter($"請將 {commandFilePath} 的內容做為查詢指令放至目的資料庫執行，並將結果儲存於 {dataFilePath}"))
                {
                    return null;
                }
            } while (!File.Exists(dataFilePath));

            return File.ReadAllText(dataFilePath, Encoding.UTF8);
        }

        public override SqlTable[] GetSqlTables(string connectionString, params string[] tableNames)
        {
            string sql = $@"
                SELECT o.[name]                       AS [Name],
                       o_des.[value]                  AS [Description],
                       (SELECT c.column_id                            AS Id,
                               CASE
                                 WHEN EXISTS (SELECT *
                                              FROM   sys.index_columns AS ic
                                                     LEFT JOIN sys.indexes i ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                                              WHERE  ic.column_id = c.column_id AND i.object_id = c.object_id AND i.is_primary_key = 1) THEN 1
                                 ELSE 0
                               END                                    AS IsPrimaryKey,
                               c.[name]                               AS [Name],
                               t.[name]                               AS TypeName,
                               sc.prec                                AS [Length],
                               c.[precision]                          AS Prec,
                               c.scale                                AS Scale,
                               c.is_nullable                          AS IsNullable,
                               c.is_identity                          AS IsIdentity,
                               c.is_computed                          AS IsComputed,
                               Object_definition(c.default_object_id) AS DefaultDefine,
                               p_des.value                            AS [Description],
                               sc.colorder                            AS [Order]
                        FROM   sys.columns c
                               INNER JOIN syscolumns sc ON c.object_id = sc.id AND c.column_id = sc.colid
                               LEFT JOIN sys.types t ON t.user_type_id = c.user_type_id
                               LEFT JOIN sys.extended_properties p_des ON c.object_id = p_des.major_id AND c.column_id = p_des.minor_id AND p_des.[name] = 'MS_Description'
                        WHERE  c.object_id = o.object_id
                        ORDER  BY sc.colorder
                        FOR XML PATH('column'), TYPE) AS [columns]
                FROM   sys.objects o
                       LEFT JOIN sys.extended_properties o_des ON o_des.major_id = o.object_id AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
                WHERE  o.type = 'U'
                       {(tableNames != null && tableNames.Any() ? $"AND o.[name] IN ({string.Join(", ", tableNames.Select(x => AddQuotes(x)))})" : "")}
                ORDER  BY o.[name]
                FOR XML PATH('table'), ROOT('tables') ".Unindent(16).TrimStart('\r', '\n');
            string xml = GetRemoteData(sql, "GetSqlTables.sql", "GetSqlTables.xml");
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            return ReadSqlTablesFromXml(xml);
        }

        public override SqlView[] GetSqlViews(string connectionString, params string[] viewNames)
        {
            string sql = $@"
                SELECT o.[name]      AS [Name],
                       o_des.[value] AS [Description]
                FROM   sys.objects o
                       LEFT JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
                WHERE  o.[type] = 'V'
                       {(viewNames != null && viewNames.Any() ? $"AND o.[name] IN ({string.Join(", ", viewNames.Select(x => AddQuotes(x)))})" : "")}
                ORDER  BY o.[name]
                FOR xml path('view'), root('views') ".Unindent(16).TrimStart('\r', '\n');
            string xml = GetRemoteData(sql, "GetSqlViews.sql", "GetSqlViews.xml");
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            return ReadSqlViewsFromXml(xml);
        }

        public override SqlProcedure[] GetSqlProcedures(string connectionString, params string[] procedureNames)
        {
            string sql = $@"
                SELECT o.[name]      AS [Name],
                       o_des.[value] AS [Description]
                FROM   sys.objects o
                       LEFT JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
                WHERE  o.[type] = 'P'
                       {(procedureNames != null && procedureNames.Any() ? $"AND o.[name] IN ({string.Join(", ", procedureNames.Select(x => AddQuotes(x)))})" : "")}
                ORDER  BY o.[name]
                FOR xml path('procedure'), root('procedures') ".Unindent(16).TrimStart('\r', '\n');
            string xml = GetRemoteData(sql, "GetSqlProcedures.sql", "GetSqlProcedures.xml");
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            return ReadSqlProceduresFromXml(xml);
        }

        public override void UpdateTableDescription(string connectionString, string tableName, string description)
        {
            string path = Path.Combine(TempPath, "UpdateTableDescription.sql");
            string sql = $@"
                IF NOT EXISTS (
	                SELECT * 
                    FROM   sys.objects o
                           INNER JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
	                WHERE  o.type = 'U' AND o.[name] = {AddQuotes(tableName)} 
                ) 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
                            EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name={AddQuotes(tableName)} 
                    END
                ELSE 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
	                        EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name={AddQuotes(tableName)} 
                        ELSE
	                        EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name={AddQuotes(tableName)}
                    END ".Unindent(16).TrimStart('\r', '\n');
            Directory.CreateDirectory(TempPath);
            File.AppendAllText(path, $@"{sql}{"\n"}", Encoding.UTF8);
        }
        public override void UpdateColumnDescription(string connectionString, string tableName, string columnName, string description)
        {
            string path = Path.Combine(TempPath, "UpdateColumnDescription.sql");
            string sql = $@"
                IF NOT EXISTS (
	                SELECT * 
	                FROM   sys.columns c 
		                   INNER JOIN syscolumns sc ON c.object_id = sc.id AND c.column_id = sc.colid 
		                   INNER JOIN sys.objects o ON c.object_id = o.object_id 
		                   INNER JOIN sys.extended_properties p ON c.object_id = p.major_id AND c.column_id = p.minor_id AND p.[name] = 'MS_Description' 
	                WHERE  o.type = 'U' AND o.[name] = {AddQuotes(tableName)} AND c.[name] = {AddQuotes(columnName)} 
                ) 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
                            EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name={AddQuotes(tableName)}, @level2type=N'COLUMN', @level2name={AddQuotes(columnName)} 
                    END
                ELSE 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
	                        EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name={AddQuotes(tableName)}, @level2type=N'COLUMN', @level2name={AddQuotes(columnName)} 
                        ELSE
	                        EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name={AddQuotes(tableName)}, @level2type=N'COLUMN', @level2name={AddQuotes(columnName)}
                    END ".Unindent(16).TrimStart('\r', '\n');
            Directory.CreateDirectory(TempPath);
            File.AppendAllText(path, $@"{sql}{"\n"}", Encoding.UTF8);
        }
        public override void UpdateViewDescription(string connectionString, string viewName, string description)
        {
            string path = Path.Combine(TempPath, "UpdateViewDescription.sql");
            string sql = $@"
                IF NOT EXISTS (
	                SELECT * 
                    FROM   sys.objects o
                           INNER JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
	                WHERE  o.type = 'V' AND o.[name] = {AddQuotes(viewName)} 
                ) 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
                            EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'VIEW', @level1name={AddQuotes(viewName)} 
                    END
                ELSE 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
	                        EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'VIEW', @level1name={AddQuotes(viewName)} 
                        ELSE
	                        EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'VIEW', @level1name={AddQuotes(viewName)}
                    END ".Unindent(16).TrimStart('\r', '\n');
            Directory.CreateDirectory(TempPath);
            File.AppendAllText(path, $@"{sql}{"\n"}", Encoding.UTF8);
        }
        public override void UpdateProcedureDescription(string connectionString, string procedureName, string description)
        {
            string path = Path.Combine(TempPath, "UpdateProcedureDescription.sql");
            string sql = $@"
                IF NOT EXISTS (
	                SELECT * 
                    FROM   sys.objects o
                           INNER JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
	                WHERE  o.type = 'P' AND o.[name] = {AddQuotes(procedureName)} 
                ) 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
                            EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'PROCEDURE', @level1name={AddQuotes(procedureName)} 
                    END
                ELSE 
                    BEGIN
                        IF NULLIF({AddQuotes(description)}, '') IS NOT NULL
	                        EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value={AddQuotes(description)}, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'PROCEDURE', @level1name={AddQuotes(procedureName)} 
                        ELSE
	                        EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'PROCEDURE', @level1name={AddQuotes(procedureName)}
                    END ".Unindent(16).TrimStart('\r', '\n');
            Directory.CreateDirectory(TempPath);
            File.AppendAllText(path, $@"{sql}{"\n"}", Encoding.UTF8);
        }

    }
}
