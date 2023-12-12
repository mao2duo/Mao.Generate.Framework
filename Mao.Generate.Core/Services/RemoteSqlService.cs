using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

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
            // 如果目的檔案不存在，建立一個空的
            if (!File.Exists(dataFilePath))
            {
                File.WriteAllText(dataFilePath, string.Empty, Encoding.UTF8);
            }

            do
            {
                if (!Awaiter($"請於目的資料庫查詢該檔案內容：{commandFilePath}{Environment.NewLine}並將查詢結果儲存於該檔案內容：{dataFilePath}。"))
                {
                    return null;
                }
            } while (!File.Exists(dataFilePath));

            return File.ReadAllText(dataFilePath, Encoding.UTF8);
        }

        public override SqlTable[] GetSqlTables(string connectionString, params string[] tableNames)
        {
            string sql = $@"
                SELECT SCHEMA_NAME(o.[schema_id])     AS [Schema],
                       o.[name]                       AS [Name],
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
                SELECT SCHEMA_NAME(o.[schema_id]) AS [Schema],
                       o.[name]                   AS [Name],
                       o_des.[value]              AS [Description]
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
                SELECT SCHEMA_NAME(o.[schema_id]) AS [Schema],
                       o.[name]                   AS [Name],
                       o_des.[value]              AS [Description]
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

        public override SqlObject[] GetSqlObjectReference(string connectionString)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;

            int n = 1;
            Dictionary<SqlObject, SqlObject> referencingResolver = new Dictionary<SqlObject, SqlObject>();
            Dictionary<string, SqlObjectDependency[]> dependenciesCache = new Dictionary<string, SqlObjectDependency[]>(StringComparer.OrdinalIgnoreCase);

            void QueryReferences(SqlObject[] sqlObjects)
            {
                foreach (var dbReferences in sqlObjects
                    .SelectMany(x => x.Reference)
                    .GroupBy(x => new
                    {
                        x.SchemaName,
                        x.DatabaseName
                    }))
                {
                    var objectNames = dbReferences
                        .Where(x => !dependenciesCache.ContainsKey($"{x.DatabaseName}.{x.SchemaName}.{x.ObjectName}"))
                        .Select(x => x.ObjectName);
                    if (objectNames.Any())
                    {
                        var sqlBuilder = new StringBuilder();
                        sqlBuilder.AppendLine($"USE [{dbReferences.Key.DatabaseName}]");
                        sqlBuilder.AppendLine($"GO");
                        sqlBuilder.AppendLine($@"
                            SELECT DISTINCT {AddQuotes(dbReferences.Key.DatabaseName)}          AS [ObjectDatabase],
                                            SCHEMA_NAME(o.[schema_id])                          AS [ObjectSchemaName],
                                            o.[name]                                            AS [ObjectName],
                                            o.[type]                                            AS [ObjectType],
                                            o.[type_desc]                                       AS [ObjectTypeDesc],
                                            ref.[referenced_database_name]                      AS [ReferencedDatabase],
                                            ISNULL(ref.[referenced_schema_name], SCHEMA_NAME()) AS [ReferencedSchemaName],
                                            ref.[referenced_entity_name]                        AS [ReferencedName],
                                            ref.[referenced_class]                              AS [ReferencedClass],
                                            ref.[referenced_class_desc]                         AS [ReferencedClassDesc]
                            FROM   sys.objects o
                                   LEFT JOIN sys.sql_expression_dependencies ref ON ref.referencing_id = o.[object_id]
                            WHERE  o.[schema_id] = SCHEMA_ID({AddQuotes(dbReferences.Key.SchemaName)})
                                   AND o.[name] IN ({string.Join(", ", objectNames.Select(x => AddQuotes(x)))}) 
                            FOR xml path('dependency'), root('dependencies') ".Unindent(28).TrimStart('\r', '\n'));
                        sqlBuilder.AppendLine($"GO");

                        string xml = GetRemoteData(sqlBuilder.ToString(), $"GetSqlObjectDependencies{n:000}.sql", $"GetSqlObjectDependencies{n:000}.xml");
                        if (!string.IsNullOrEmpty(xml))
                        {
                            var dependencies = ReadSqlObjectDependenciesFromXml(xml);
                            foreach (var group in dependencies.GroupBy(x => $"{x.ObjectDatabase}.{x.ObjectSchemaName}.{x.ObjectName}"))
                            {
                                dependenciesCache.Add(group.Key, group.ToArray());
                            }
                        }
                        n++;
                    }
                }
            }

            void FillReferences(SqlObject[] sqlObjects)
            {
                if (sqlObjects.Any())
                {
                    QueryReferences(sqlObjects);
                    List<SqlObject> references = new List<SqlObject>();
                    foreach (var sqlObject in sqlObjects)
                    {
                        foreach (var dbReference in sqlObject.Reference)
                        {
                            var dependenciesKey = $"{dbReference.DatabaseName}.{dbReference.SchemaName}.{dbReference.ObjectName}";
                            if (dependenciesCache.ContainsKey(dependenciesKey))
                            {
                                bool loop;
                                var loopObject = sqlObject;
                                while (true)
                                {
                                    loop = $"{loopObject.DatabaseName}.{loopObject.SchemaName}.{loopObject.ObjectName}".Equals(dependenciesKey, StringComparison.OrdinalIgnoreCase);
                                    if (loop || !referencingResolver.ContainsKey(loopObject))
                                    {
                                        break;
                                    }
                                    loopObject = referencingResolver[loopObject];
                                }
                                if (!loop)
                                {
                                    referencingResolver.Add(dbReference, sqlObject);
                                    var dependencies = dependenciesCache[dependenciesKey];
                                    var dependency = dependencies.First();
                                    dbReference.ObjectType = dependency.ObjectType?.TrimEnd();
                                    dbReference.ObjectTypeDesc = dependency.ObjectTypeDesc;
                                    dbReference.Reference = dependencies
                                        .Where(x => !string.IsNullOrEmpty(x.ReferencedName))
                                        .Select(x => new SqlObject()
                                        {
                                            DatabaseName = x.ReferencedDatabase ?? dbReference.DatabaseName,
                                            SchemaName = x.ReferencedSchemaName,
                                            ObjectName = x.ReferencedName,
                                            Class = x.ReferencedClass,
                                            ClassDesc = x.ReferencedClassDesc
                                        })
                                        .ToList();
                                    references.Add(dbReference);
                                }
                            }
                        }
                    }
                    FillReferences(references.ToArray());
                }
            }

            //connectionStringBuilder.TrustServerCertificate = true;
            using (var conn = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                string sql = @"
                    SELECT DISTINCT DB_NAME()                                           AS [ObjectDatabase],
                                    SCHEMA_NAME(o.[schema_id])                          AS [ObjectSchemaName],
                                    o.[name]                                            AS [ObjectName],
                                    o.[type]                                            AS [ObjectType],
                                    o.[type_desc]                                       AS [ObjectTypeDesc],
                                    ref.[referenced_database_name]                      AS [ReferencedDatabase],
                                    ISNULL(ref.[referenced_schema_name], SCHEMA_NAME()) AS [ReferencedSchemaName],
                                    ref.[referenced_entity_name]                        AS [ReferencedName],
                                    ref.[referenced_class]                              AS [ReferencedClass],
                                    ref.[referenced_class_desc]                         AS [ReferencedClassDesc]
                    FROM   sys.objects o
                           LEFT JOIN sys.sql_expression_dependencies ref ON ref.referencing_id = o.[object_id]
                    WHERE  o.[type] IN ( 'V', 'P', 'IF', 'FN' ) 
                    FOR xml path('dependency'), root('dependencies') ".Unindent(16).TrimStart('\r', '\n');
                string xml = GetRemoteData(sql, "GetSqlObjectDependencies.sql", "GetSqlObjectDependencies.xml");
                if (!string.IsNullOrEmpty(xml))
                {
                    var sqlObjects = ReadSqlObjectDependenciesFromXml(xml)
                        .GroupBy(x => new
                        {
                            ObjectDatabase = x.ObjectDatabase,
                            ObjectSchemaName = x.ObjectSchemaName,
                            ObjectName = x.ObjectName,
                            ObjectType = x.ObjectType,
                            ObjectTypeDesc = x.ObjectTypeDesc
                        })
                        .Select(group => new SqlObject()
                        {
                            DatabaseName = group.Key.ObjectDatabase,
                            SchemaName = group.Key.ObjectSchemaName,
                            ObjectName = group.Key.ObjectName,
                            ObjectType = group.Key.ObjectType?.TrimEnd(),
                            ObjectTypeDesc = group.Key.ObjectTypeDesc?.TrimEnd(),
                            Reference = group
                                .Where(x => !string.IsNullOrEmpty(x.ReferencedName))
                                .Select(x => new SqlObject()
                                {
                                    DatabaseName = x.ReferencedDatabase ?? group.Key.ObjectDatabase,
                                    SchemaName = x.ReferencedSchemaName,
                                    ObjectName = x.ReferencedName,
                                    Class = x.ReferencedClass,
                                    ClassDesc = x.ReferencedClassDesc
                                }).ToList()
                        })
                        .OrderBy(x =>
                        {
                            switch (x.ObjectType)
                            {
                                case "V":
                                    return "0";
                                case "P":
                                    return "1";
                                case "IF":
                                    return "2";
                                case "FN":
                                    return "3";
                            }
                            return x.ObjectType;
                        })
                        .ThenBy(x => x.ObjectName)
                        .ToArray();
                    FillReferences(sqlObjects);
                    return sqlObjects;
                }
            }
            return null;
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
