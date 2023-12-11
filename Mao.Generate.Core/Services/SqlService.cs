using Dapper;
using Mao.Generate.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Mao.Generate.Core.Services
{
    public class SqlService
    {
        /// <summary>
        /// 取得連接字串預設連接的資料庫名稱
        /// </summary>
        public virtual string GetDefaultDatabaseName(string connectionString)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;
            return connectionStringBuilder.InitialCatalog;
        }
        /// <summary>
        /// 取得不包含系統資料庫的所有資料庫名稱
        /// </summary>
        public virtual string[] GetDatabaseNames(string connectionString)
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
        public virtual string[] GetTableNames(string connectionString)
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
        public virtual string[] GetColumnNames(string connectionString, string tableName)
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
        public virtual string[] GetViewNames(string connectionString)
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
        public virtual string[] GetProcedureNames(string connectionString)
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
        /// 取得不包含系統資料庫的資料庫列表
        /// </summary>
        public virtual SqlDatabase[] GetSqlDatabases(string connectionString, params string[] databaseNames)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<SqlDatabase>($@"
                    SELECT database_id AS Id, 
                           [name], 
                           state 
                    FROM   sys.databases 
                    WHERE  [name] NOT IN ( 'master', 'tempdb', 'model', 'msdb' ) 
                           {(databaseNames != null && databaseNames.Any() ? "AND [name] IN @DatabaseNames" : "")}
                    ORDER  BY [name] ",
                    new
                    {
                        DatabaseNames = databaseNames
                    }).ToArray();
            }
        }
        /// <summary>
        /// 取得資料表的結構
        /// </summary>
        public virtual SqlTable[] GetSqlTables(string connectionString, params string[] tableNames)
        {
            SqlTable[] sqlTables;
            using (var conn = new SqlConnection(connectionString))
            {
                sqlTables = conn.Query<SqlTable>($@"
                    SELECT SCHEMA_NAME(o.[schema_id]) AS [Schema],
                           o.[name]                   AS [Name],
                           o_des.[value]              AS [Description]
                    FROM   sys.objects o
                           LEFT JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
                    WHERE  o.[type] = 'U'
                           {(tableNames != null && tableNames.Any() ? "AND o.[name] IN @TableNames" : "")}
                    ORDER BY o.[name]",
                    new
                    {
                        TableNames = tableNames
                    }).ToArray();
            }
            foreach (var sqlTable in sqlTables)
            {
                sqlTable.Columns = GetSqlColumns(connectionString, sqlTable.Name);
            }
            return sqlTables.ToArray();
        }
        /// <summary>
        /// 取得資料表欄位的資訊
        /// </summary>
        public virtual SqlColumn[] GetSqlColumns(string connectionString, string tableName, params string[] columnNames)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<SqlColumn>($@"
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
                    WHERE  o.type = 'U'
                           AND o.[name] = @TableName
                           {(columnNames != null && columnNames.Any() ? "AND c.[name] IN @ColumnNames" : "")}
                    ORDER  BY sc.colorder ",
                    new
                    {
                        TableName = tableName,
                        ColumnNames = columnNames
                    }).ToArray();
            }
        }
        /// <summary>
        /// 取得檢視的資訊
        /// </summary>
        public virtual SqlView[] GetSqlViews(string connectionString, params string[] viewNames)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<SqlView>($@"
                    SELECT SCHEMA_NAME(o.[schema_id]) AS [Schema],
                           o.[name]                   AS [Name],
                           o_des.[value]              AS [Description]
                    FROM   sys.objects o
                           LEFT JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
                    WHERE  o.[type] = 'V'
                           {(viewNames != null && viewNames.Any() ? "AND o.[name] IN @ViewNames" : "")}
                    ORDER BY o.[name]",
                    new
                    {
                        VableNames = viewNames
                    }).ToArray();
            }
        }
        /// <summary>
        /// 取得預存程序的資訊
        /// </summary>
        public virtual SqlProcedure[] GetSqlProcedures(string connectionString, params string[] procedureNames)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                return conn.Query<SqlProcedure>($@"
                    SELECT SCHEMA_NAME(o.[schema_id]) AS [Schema],
                           o.[name]                   AS [Name],
                           o_des.[value]              AS [Description]
                    FROM   sys.objects o
                           LEFT JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
                    WHERE  o.[type] = 'P'
                           {(procedureNames != null && procedureNames.Any() ? "AND o.[name] IN @ProcedureNames" : "")}
                    ORDER BY o.[name]",
                    new
                    {
                        ProcedureNames = procedureNames
                    }).ToArray();
            }
        }
        /// <summary>
        /// 取得資料表 Foreign Key 的關聯性
        /// </summary>
        public virtual SqlForeignKey[] GetSqlForeignKeys(string connectionString)
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
        /// 取得資料庫物件的關聯性
        /// </summary>
        public virtual SqlObject[] GetSqlObjectReference(string connectionString)
        {
            SqlObject[] sqlObjects;
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;

            HashSet<string> fillStack = new HashSet<string>();
            void FillReferences(SqlObject sqlObject)
            {
                var fillKey = $"{sqlObject.DatabaseName}.{sqlObject.SchemaName}.{sqlObject.ObjectName}";
                if (fillStack.Add(fillKey))
                {
                    foreach (var dbReferences in sqlObject.Reference
                        .GroupBy(x => new
                        {
                            x.SchemaName,
                            x.DatabaseName
                        }))
                    {
                        connectionStringBuilder.InitialCatalog = dbReferences.Key.DatabaseName;
                        try
                        {
                            DataTable dt;
                            using (var conn = new SqlConnection(connectionStringBuilder.ConnectionString))
                            {
                                var reader = conn.ExecuteReader(@"
                                    SELECT DISTINCT SCHEMA_NAME(o.[schema_id])                  AS [ObjectSchemaName],
                                                    o.[name]                                    AS [ObjectName],
                                                    o.[type]                                    AS [ObjectType],
                                                    o.[type_desc]                               AS [ObjectTypeDesc],
                                                    ref.[referenced_database_name]              AS [ReferencedDatabase],
                                                    ISNULL(ref.[referenced_schema_name], 'dbo') AS [ReferencedSchemaName],
                                                    ref.[referenced_entity_name]                AS [ReferencedName],
                                                    ref.[referenced_class]                      AS [ReferencedClass],
                                                    ref.[referenced_class_desc]                 AS [ReferencedClassDesc]
                                    FROM   sys.objects o
                                           LEFT JOIN sys.sql_expression_dependencies ref ON ref.referencing_id = o.[object_id]
                                    WHERE  o.[schema_id] = SCHEMA_ID(@SchemaName)
                                           AND o.[name] IN @ObjectNames ",
                                    new
                                    {
                                        SchemaName = dbReferences.Key.SchemaName,
                                        ObjectNames = dbReferences.Select(x => x.ObjectName).ToList()
                                    });
                                dt = new DataTable();
                                dt.Load(reader);
                            }
                            foreach (var group in dt.Rows.Cast<DataRow>()
                                .GroupBy(row => new
                                {
                                    ObjectSchemaName = row["ObjectSchemaName"] as string,
                                    ObjectName = row["ObjectName"] as string,
                                    ObjectType = row["ObjectType"] as string,
                                    ObjectTypeDesc = row["ObjectTypeDesc"] as string
                                }))
                            {
                                var dbReference = dbReferences.First(x =>
                                    x.SchemaName.Equals(group.Key.ObjectSchemaName, StringComparison.OrdinalIgnoreCase)
                                    && x.ObjectName.Equals(group.Key.ObjectName, StringComparison.OrdinalIgnoreCase));
                                dbReference.ObjectType = group.Key.ObjectType?.TrimEnd();
                                dbReference.ObjectTypeDesc = group.Key.ObjectTypeDesc;
                                dbReference.Reference = group
                                    .Where(row => row["ReferencedName"] != DBNull.Value)
                                    .Select(row => new SqlObject()
                                    {
                                        DatabaseName = row["ReferencedDatabase"] as string ?? dbReference.DatabaseName,
                                        SchemaName = row["ReferencedSchemaName"] as string,
                                        ObjectName = row["ReferencedName"] as string,
                                        Class = (byte)row["ReferencedClass"],
                                        ClassDesc = row["ReferencedClassDesc"] as string
                                    })
                                    .ToList();
                                FillReferences(dbReference);
                            }
                        }
                        catch (SqlException ex)
                        {
                            foreach (var dbReference in dbReferences)
                            {
                                dbReference.ErrorMessage = ex.Message;
                            }
                        }
                    }
                    fillStack.Remove(fillKey);
                }
                else
                {
                    sqlObject.Reference?.Clear();
                }
            }

            //connectionStringBuilder.TrustServerCertificate = true;
            using (var conn = new SqlConnection(connectionStringBuilder.ConnectionString))
            {
                var reader = conn.ExecuteReader(@"
                    SELECT DISTINCT SCHEMA_NAME(o.[schema_id])                  AS [ObjectSchemaName],
                                    o.[name]                                    AS [ObjectName],
                                    o.[type]                                    AS [ObjectType],
                                    o.[type_desc]                               AS [ObjectTypeDesc],
                                    ref.[referenced_database_name]              AS [ReferencedDatabase],
                                    ISNULL(ref.[referenced_schema_name], 'dbo') AS [ReferencedSchemaName],
                                    ref.[referenced_entity_name]                AS [ReferencedName],
                                    ref.[referenced_class]                      AS [ReferencedClass],
                                    ref.[referenced_class_desc]                 AS [ReferencedClassDesc]
                    FROM   sys.objects o
                           LEFT JOIN sys.sql_expression_dependencies ref ON ref.referencing_id = o.[object_id]
                    WHERE  o.[type] IN ( 'V', 'P', 'IF', 'FN' ) ");
                var dt = new DataTable();
                dt.Load(reader);
                sqlObjects = dt.Rows.Cast<DataRow>()
                    .GroupBy(row => new
                    {
                        ObjectSchemaName = row["ObjectSchemaName"] as string,
                        ObjectName = row["ObjectName"] as string,
                        ObjectType = row["ObjectType"] as string,
                        ObjectTypeDesc = row["ObjectTypeDesc"] as string
                    })
                    .Select(group => new SqlObject()
                    {
                        DatabaseName = connectionStringBuilder.InitialCatalog,
                        SchemaName = group.Key.ObjectSchemaName,
                        ObjectName = group.Key.ObjectName,
                        ObjectType = group.Key.ObjectType?.TrimEnd(),
                        ObjectTypeDesc = group.Key.ObjectTypeDesc?.TrimEnd(),
                        Reference = group
                            .Where(row => row["ReferencedName"] != DBNull.Value)
                            .Select(row => new SqlObject()
                            {
                                DatabaseName = row["ReferencedDatabase"] as string ?? connectionStringBuilder.InitialCatalog,
                                SchemaName = row["ReferencedSchemaName"] as string,
                                ObjectName = row["ReferencedName"] as string,
                                Class = (byte)row["ReferencedClass"],
                                ClassDesc = row["ReferencedClassDesc"] as string
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
            }
            foreach (var sqlObject in sqlObjects)
            {
                FillReferences(sqlObject);
            }
            return sqlObjects;
        }

        /// <summary>
        /// 更新資料表的描述
        /// </summary>
        public virtual void UpdateTableDescription(string connectionString, string tableName, string description)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(@"
                    IF NOT EXISTS (
	                    SELECT * 
                        FROM   sys.objects o
                               INNER JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
	                    WHERE  o.type = 'U' AND o.[name] = @TableName 
                    ) 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
                                EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName 
                        END
                    ELSE 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
	                            EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName 
                            ELSE
	                            EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName
                        END ",
                    new
                    {
                        TableName = tableName,
                        Description = description
                    });
            }
        }
        /// <summary>
        /// 更新資料表欄位的描述
        /// </summary>
        public virtual void UpdateColumnDescription(string connectionString, string tableName, string columnName, string description)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(@"
                    IF NOT EXISTS (
	                    SELECT * 
	                    FROM   sys.columns c 
		                       INNER JOIN syscolumns sc ON c.object_id = sc.id AND c.column_id = sc.colid 
		                       INNER JOIN sys.objects o ON c.object_id = o.object_id 
		                       INNER JOIN sys.extended_properties p ON c.object_id = p.major_id AND c.column_id = p.minor_id AND p.[name] = 'MS_Description' 
	                    WHERE  o.type = 'U' AND o.[name] = @TableName AND c.[name] = @ColumnName 
                    ) 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
                                EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName, @level2type=N'COLUMN', @level2name=@ColumnName 
                        END
                    ELSE 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
	                            EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName, @level2type=N'COLUMN', @level2name=@ColumnName 
                            ELSE
	                            EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'TABLE', @level1name=@TableName, @level2type=N'COLUMN', @level2name=@ColumnName
                        END ",
                    new
                    {
                        TableName = tableName,
                        ColumnName = columnName,
                        Description = description
                    });
            }
        }
        /// <summary>
        /// 更新檢視的描述
        /// </summary>
        public virtual void UpdateViewDescription(string connectionString, string viewName, string description)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(@"
                    IF NOT EXISTS (
	                    SELECT * 
                        FROM   sys.objects o
                               INNER JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
	                    WHERE  o.type = 'V' AND o.[name] = @ViewName 
                    ) 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
                                EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'VIEW', @level1name=@ViewName 
                        END
                    ELSE 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
	                            EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'VIEW', @level1name=@ViewName 
                            ELSE
	                            EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'VIEW', @level1name=@ViewName
                        END ",
                    new
                    {
                        ViewName = viewName,
                        Description = description
                    });
            }
        }
        /// <summary>
        /// 更新預存程序的描述
        /// </summary>
        public virtual void UpdateProcedureDescription(string connectionString, string procedureName, string description)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Execute(@"
                    IF NOT EXISTS (
	                    SELECT * 
                        FROM   sys.objects o
                               INNER JOIN sys.extended_properties o_des ON o_des.major_id = o.[object_id] AND o_des.minor_id = 0 AND o_des.[name] = 'MS_Description'
	                    WHERE  o.type = 'P' AND o.[name] = @ProcedureName 
                    ) 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
                                EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'PROCEDURE', @level1name=@ProcedureName 
                        END
                    ELSE 
                        BEGIN
                            IF NULLIF(@Description, '') IS NOT NULL
	                            EXEC sys.sp_updateextendedproperty @name=N'MS_Description', @value=@Description, @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'PROCEDURE', @level1name=@ProcedureName 
                            ELSE
	                            EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name=N'dbo', @level1type=N'PROCEDURE', @level1name=@ProcedureName
                        END ",
                    new
                    {
                        ProcedureName = procedureName,
                        Description = description
                    });
            }
        }
        /// <summary>
        /// 產生對應資料表的查詢結果的 INSERT 語法
        /// </summary>
        public virtual string GenerateInsertCommand(string connectionString, string tableName, string sqlCommand)
        {
            SqlTable sqlTable = GetSqlTables(connectionString, tableName).First();
            var dt = new DataTable();
            using (var conn = new SqlConnection(connectionString))
            {
                var reader = conn.ExecuteReader(sqlCommand);
                dt.Load(reader);
            }
            return GenerateInsertCommand(sqlTable, dt);
        }
        /// <summary>
        /// 產生對應資料表的查詢結果的 INSERT 語法
        /// </summary>
        public virtual string GenerateInsertCommand(SqlTable sqlTable, DataTable dt)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (dt != null && dt.Rows.Count != 0)
            {
                SqlColumn[] sqlColumns = sqlTable.Columns
                    .Where(x => !(x.IsNullable || x.IsIdentity || !string.IsNullOrEmpty(x.DefaultDefine)) || dt.Columns.Contains(x.Name))
                    .ToArray();
                SqlColumn identityColumn = sqlTable.Columns
                    .Where(x => x.IsIdentity)
                    .FirstOrDefault();
                bool insertIdentity = identityColumn != null && sqlColumns.Contains(identityColumn);
                stringBuilder.AppendLine($"INSERT INTO [{sqlTable.Name}] ({string.Join(", ", sqlColumns.Select(x => $"[{x.Name}]"))})");
                stringBuilder.AppendLine($"VALUES");
                foreach (DataRow row in dt.Rows)
                {
                    stringBuilder.AppendLine($"({string.Join(", ", sqlColumns.Select(x => GenerateValueExpression(x.TypeName, x.IsNullable, dt.Columns.Contains(x.Name) ? row[x.Name] : null)))}),");
                }
            }
            return stringBuilder.ToString().TrimEnd(' ', '\r', '\n', ',');
        }
        /// <summary>
        /// 將物件轉成 SQL 的表達式
        /// </summary>
        public virtual string GenerateValueExpression(string typeName, bool isNullable, object value)
        {
            if (isNullable && value == null)
            {
                return "NULL";
            }
            if (value is SqlParameter parameter)
            {
                return $"@{parameter.ParameterName.TrimStart('@')}";
            }
            switch (typeName)
            {
                case "bit":
                    if (value is bool b)
                    {
                        return b ? "1" : "0";
                    }
                    string lower = Convert.ToString(value).ToLower();
                    if (lower == "true" || lower == "t" || lower == "1")
                    {
                        return "1";
                    }
                    return "0";
                case "date":
                    if (value is DateTime d1)
                    {
                        return $"'{d1:yyyy-MM-dd}'";
                    }
                    return AddQuotesIfNotFunction(value);
                case "smalldatetime":
                    if (value is DateTime d2)
                    {
                        return $"'{d2:yyyy-MM-dd HH:mm:ss}'";
                    }
                    return AddQuotesIfNotFunction(value);
                case "datetime":
                case "datetime2":
                    if (value is DateTime d3)
                    {
                        return $"'{d3:yyyy-MM-dd HH:mm:ss.fff}'";
                    }
                    return AddQuotesIfNotFunction(value);
                case "time":
                case "timestamp":
                case "datetimeoffset":
                case "binary":
                case "varbinary":
                case "image":
                    throw new NotImplementedException();
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "sql_variant":
                case "text":
                case "uniqueidentifier":
                case "varchar":
                case "xml":
                    return AddQuotesIfNotFunction(value);
                case "bigint":
                case "decimal":
                case "float":
                case "int":
                case "money":
                case "numeric":
                case "real":
                case "smallint":
                case "smallmoney":
                case "tinyint":
                    string s = Convert.ToString(value);
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        return s;
                    }
                    return "0";
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 將 XML 的 InnerText 轉換成布林值
        /// </summary>
        protected virtual bool XmlInnerTextToBoolean(string innerText)
        {
            return innerText == "1" || innerText?.ToLower() == "true";
        }
        /// <summary>
        /// 從 XML 取得資料表的結構
        /// </summary>
        public virtual SqlTable[] ReadSqlTablesFromXml(string xml)
        {
            List<SqlTable> sqlTables = new List<SqlTable>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var tables = doc.DocumentElement.SelectNodes("table");
            foreach (XmlNode table in tables)
            {
                SqlTable sqlTable = new SqlTable();
                Invoker.UsingIf(table.SelectSingleNode("Name"),
                    node => node != null,
                    node => sqlTable.Name = node.InnerText);
                Invoker.UsingIf(table.SelectSingleNode("Description"),
                    node => node != null,
                    node => sqlTable.Description = node.InnerText);
                var columnsNode = table.SelectSingleNode("columns");
                if (columnsNode != null)
                {
                    sqlTable.Columns = ReadSqlColumnsFromXml(columnsNode.OuterXml);
                }
                sqlTables.Add(sqlTable);
            }
            return sqlTables.ToArray();
        }
        /// <summary>
        /// 從 XML 取得資料表欄位的資訊
        /// </summary>
        public virtual SqlColumn[] ReadSqlColumnsFromXml(string xml)
        {
            List<SqlColumn> sqlColumns = new List<SqlColumn>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var columns = doc.DocumentElement.SelectNodes("column");
            foreach (XmlNode column in columns)
            {
                SqlColumn sqlColumn = new SqlColumn();
                Invoker.UsingIf(column.SelectSingleNode("Name"),
                    node => node != null,
                    node => sqlColumn.Name = node.InnerText);
                Invoker.UsingIf(column.SelectSingleNode("TypeName"),
                    node => node != null,
                    node => sqlColumn.TypeName = node.InnerText);
                Invoker.UsingIf(column.SelectSingleNode("IsNullable"),
                    node => node != null,
                    node => sqlColumn.IsNullable = XmlInnerTextToBoolean(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("Length"),
                    node => node != null,
                    node => sqlColumn.Length =
                        node.InnerText?.ToLower() == "max" ? -1 : Convert.ToInt32(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("Prec"),
                    node => node != null,
                    node => sqlColumn.Prec = Convert.ToInt32(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("Scale"),
                    node => node != null,
                    node => sqlColumn.Scale = Convert.ToInt32(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("DefaultDefine"),
                    node => node != null,
                    node => sqlColumn.DefaultDefine = node.InnerText);
                Invoker.UsingIf(column.SelectSingleNode("IsPrimaryKey"),
                    node => node != null,
                    node => sqlColumn.IsPrimaryKey = XmlInnerTextToBoolean(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("IsIdentity"),
                    node => node != null,
                    node => sqlColumn.IsIdentity = XmlInnerTextToBoolean(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("IsComputed"),
                    node => node != null,
                    node => sqlColumn.IsComputed = XmlInnerTextToBoolean(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("Description"),
                    node => node != null,
                    node => sqlColumn.Description = node.InnerText);
                Invoker.UsingIf(column.SelectSingleNode("Order"),
                    node => node != null,
                    node => sqlColumn.Order = Convert.ToInt32(node.InnerText));
                Invoker.UsingIf(column.SelectSingleNode("TypeFullName"),
                    node => node != null,
                    node => sqlColumn.TypeFullName = node.InnerText);
                sqlColumns.Add(sqlColumn);
            }
            return sqlColumns.ToArray();
        }
        /// <summary>
        /// 從 XML 取得檢視的資訊
        /// </summary>
        public virtual SqlView[] ReadSqlViewsFromXml(string xml)
        {
            List<SqlView> sqlViews = new List<SqlView>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var views = doc.DocumentElement.SelectNodes("view");
            foreach (XmlNode view in views)
            {
                SqlView sqlView = new SqlView();
                Invoker.UsingIf(view.SelectSingleNode("Name"),
                    node => node != null,
                    node => sqlView.Name = node.InnerText);
                Invoker.UsingIf(view.SelectSingleNode("Description"),
                    node => node != null,
                    node => sqlView.Description = node.InnerText);
                sqlViews.Add(sqlView);
            }
            return sqlViews.ToArray();
        }
        /// <summary>
        /// 從 XML 取得預存程序的資訊
        /// </summary>
        public virtual SqlProcedure[] ReadSqlProceduresFromXml(string xml)
        {
            List<SqlProcedure> sqlProcedures = new List<SqlProcedure>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var procedures = doc.DocumentElement.SelectNodes("procedure");
            foreach (XmlNode procedure in procedures)
            {
                SqlProcedure sqlProcedure = new SqlProcedure();
                Invoker.UsingIf(procedure.SelectSingleNode("Name"),
                    node => node != null,
                    node => sqlProcedure.Name = node.InnerText);
                Invoker.UsingIf(procedure.SelectSingleNode("Description"),
                    node => node != null,
                    node => sqlProcedure.Description = node.InnerText);
                sqlProcedures.Add(sqlProcedure);
            }
            return sqlProcedures.ToArray();
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
        /// 為值加上引號 (如果不是呼叫方法的形式)
        /// </summary>
        public string AddQuotesIfNotFunction(object value)
        {
            var s = Convert.ToString(value);
            if (Regex.IsMatch(s, @"^[_A-Za-z]+\([\S\s]*\)$"))
            {
                return s;
            }
            return AddQuotes(s);
        }
        /// <summary>
        /// 為值加上引號
        /// </summary>
        public string AddQuotes(object value)
        {
            return $"N'{value?.ToString().Replace("'", "''")}'";
        }
    }
}
