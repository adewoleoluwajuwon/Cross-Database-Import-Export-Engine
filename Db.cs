using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Linq; 



namespace Eximp
{
    #region Enums 
    public enum DuplicateStrategy
    {
        Error,          // current behavior
        Skip,           // MySQL: INSERT IGNORE, Postgres: ON CONFLICT DO NOTHING
        Upsert          // MySQL: ON DUPLICATE KEY UPDATE, Postgres: ON CONFLICT (PK) DO UPDATE
    }
    #endregion
    public static class Db
    {
        #region ConfigDir
        private static readonly string ConfigDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Eximp");

        public static string ConfigFilePath { get; } =
            Path.Combine(ConfigDir, "dbconfig.txt");
        #endregion


        #region Helpers
        // Helper method to get database configuration
        // Helper to pick correct parameter prefix (@ for most, : for Oracle)
        private static string P(string provider, string name) =>
            provider == "Oracle.ManagedDataAccess.Client" ? $":{name}" : $"@{name}";

        private static (string provider, string server, string database, string username, string password) GetDbConfig()
        {
            if (!File.Exists(ConfigFilePath))
                throw new FileNotFoundException("Database configuration file not found.", ConfigFilePath);

            var lines = File.ReadAllLines(ConfigFilePath);
            if (lines.Length < 5)
                throw new InvalidOperationException("dbconfig.txt must have exactly 5 lines: Provider, Server, Database, Username, Password.");

            return (
                provider: lines[0].Trim(),
                server: lines[1].Trim(),
                database: lines[2].Trim(),
                username: lines[3].Trim(),
                password: lines[4].Trim()
            );
        }
        #endregion
        #region ConnectionString
        private static string BuildOracleDataSource(string server, string serviceName)
        {
            // If no port was typed, default to 1521
            var hostPort = server.Contains(":") ? server : $"{server}:1521";
            return $"{hostPort}/{serviceName}";   // service name, e.g., XEPDB1 / ORCLPDB1 / FREEPDB1
        }

        // Build connection string based on provider
        private static string GetConnectionString()
        {
            var config = GetDbConfig();

            return config.provider switch
            {
                "System.Data.SqlClient" or "Microsoft.Data.SqlClient" =>
                    $"Server={config.server};Database={config.database};User Id={config.username};Password={config.password};TrustServerCertificate=True;",
                "MySqlConnector" or "MySql.Data.MySqlClient" =>
                    $"Server={config.server};Database={config.database};User Id={config.username};Password={config.password};SslMode=None;AllowPublicKeyRetrieval=True",
                "Npgsql" =>
                    $"Host={config.server};Database={config.database};Username={config.username};Password={config.password};SSL Mode=Disable;",
                "Oracle.ManagedDataAccess.Client" =>                      // ✅ updated
                    $"User Id={config.username};Password={config.password};Data Source={BuildOracleDataSource(config.server, config.database)};",
                _ => throw new Exception($"Unsupported provider: {config.provider}")
            };
        }
        #endregion

        #region ConnectionString TestConString QeuryAsync ExecAsync ListTablesAsync ListColumnsAsync 
        // Get database connection using provider factory
        public static DbConnection GetConn()
        {
            var config = GetDbConfig();
            var factory = DbProviderFactories.GetFactory(config.provider);
            var connection = factory.CreateConnection();
            connection.ConnectionString = GetConnectionString();
            return connection;
        }

        public static async Task<bool> TestConnectionAsync()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                var cfg = GetDbConfig();
                await using var cn = GetConn();
                await cn.OpenAsync();

                await using var cmd = cn.CreateCommand();
                cmd.CommandText = cfg.provider switch
                {
                    "Oracle.ManagedDataAccess.Client" => "SELECT 1 FROM dual",
                    _ => "SELECT 1"
                };

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) == 1;
            }
            catch
            {
                return false;
            }
        }

        //public static async Task<bool> TestConnectionAsync()
        //{
        //    try
        //    {
        //        await using var cn = GetConn();
        //        await cn.OpenAsync();
        //        await using var cmd = cn.CreateCommand();
        //        cmd.CommandText = "SELECT 1";
        //        var result = await cmd.ExecuteScalarAsync();
        //        return Convert.ToInt32(result) == 1;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public static async Task<DataTable> QueryAsync(string sql, params DbParameter[] parameters)
        {
            await using var cn = GetConn();
            await cn.OpenAsync();
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }

        public static async Task<int> ExecAsync(string sql, params DbParameter[] parameters)
        {
            await using var cn = GetConn();
            await cn.OpenAsync();
            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var param in parameters)
                    cmd.Parameters.Add(param);
            }
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<DataTable> ListTablesAsync()
        {
            var config = GetDbConfig();

            string sql = config.provider switch
            {
                "System.Data.SqlClient" => @"
                    SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS FullName
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_TYPE = 'BASE TABLE'
                    ORDER BY TABLE_SCHEMA, TABLE_NAME",

                "MySqlConnector" or "MySql.Data.MySqlClient" => @"
                    SELECT CONCAT(TABLE_SCHEMA, '.', TABLE_NAME) AS FullName
                    FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_TYPE = 'BASE TABLE' 
                    AND TABLE_SCHEMA NOT IN ('information_schema','performance_schema','mysql','sys')
                    ORDER BY TABLE_SCHEMA, TABLE_NAME",


                "Npgsql" => @"
                    SELECT schemaname || '.' || tablename AS FullName
                    FROM pg_tables
                    WHERE schemaname NOT IN ('information_schema', 'pg_catalog')
                    ORDER BY schemaname, tablename",

                "Oracle.ManagedDataAccess.Client" => @"
                    SELECT OWNER || '.' || TABLE_NAME AS FullName
                    FROM ALL_TABLES
                    WHERE OWNER NOT IN ('SYS', 'SYSTEM')
                    ORDER BY OWNER, TABLE_NAME",

                _ => throw new Exception($"Unsupported provider for ListTables: {config.provider}")
            };

            return await QueryAsync(sql);
        }

        public static async Task<DataTable> ListColumnsAsync(string fullTableName)
        {
            var config = GetDbConfig();
            var parts = fullTableName.Split('.');
            var schema = parts.Length > 1 ? parts[0] : (config.provider == "System.Data.SqlClient" ? "dbo" : "public");
            var table = parts.Length > 1 ? parts[1] : parts[0];

            string sql = config.provider switch
            {
                "System.Data.SqlClient" => @"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table
                    ORDER BY ORDINAL_POSITION",

                "MySqlConnector" or "MySql.Data.MySqlClient" => @"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table
                    ORDER BY ORDINAL_POSITION",

                "Npgsql" => @"
                    SELECT column_name AS COLUMN_NAME
                    FROM information_schema.columns
                    WHERE table_schema = @schema AND table_name = @table
                    ORDER BY ordinal_position",

                _ => throw new Exception($"Unsupported provider for ListColumns: {config.provider}")
            };

            var factory = DbProviderFactories.GetFactory(config.provider);
            var schemaParam = factory.CreateParameter();
            schemaParam.ParameterName = P(config.provider, "schema");     
          
            //schemaParam.ParameterName = "@schema";
            schemaParam.Value = schema;

            var tableParam = factory.CreateParameter();
            tableParam.ParameterName = P(config.provider, "table");
            //tableParam.ParameterName = "@table";
            tableParam.Value = table;

            return await QueryAsync(sql, schemaParam, tableParam);
        }
        #endregion

        #region Current Distinct
        public static async Task<DataTable> DistinctValuesAsync(string fullTableName, string column)
        {
            var config = GetDbConfig();
            var qcol = QuoteId(config.provider, column);
            var sql = $"SELECT DISTINCT {qcol} AS Val FROM {fullTableName} ORDER BY {qcol}";
            return await QueryAsync(sql);
        }
        #endregion

        #region Distinic
        //public static async Task<DataTable> DistinctValuesAsync(string fullTableName, string column)
        //{
        //    var config = GetDbConfig();                                    
        //    var qcol = QuoteId(config.provider, column);
        //    var sql = $"SELECT DISTINCT \"{column}\" AS Val FROM {fullTableName} ORDER BY \"{column}\"";
        //    return await QueryAsync(sql);
        //}
        #endregion
        #region FIlter
        //public static async Task<DataTable> FilterAsync(string fullTableName, string filterColumn, object value)
        //{
        //    var config = GetDbConfig();
        //    var factory = DbProviderFactories.GetFactory(config.provider);
        //    var param = factory.CreateParameter();
        //    param.ParameterName = "@v";
        //    param.Value = value ?? DBNull.Value;

        //    var qcol = QuoteId(config.provider, filterColumn);

        //    var sql = $"SELECT * FROM {fullTableName} WHERE \"{filterColumn}\" = @v ORDER BY 1";
        //    return await QueryAsync(sql, param);
        //}
        #endregion
        #region CurrentFilter
        public static async Task<DataTable> FilterAsync(string fullTableName, string filterColumn, object value)
        {
            var config = GetDbConfig();
            var factory = DbProviderFactories.GetFactory(config.provider);
            var param = factory.CreateParameter();
            param.ParameterName = P(config.provider, "v");
            param.Value = value ?? DBNull.Value;

            var qcol = QuoteId(config.provider, filterColumn);
            var sql = $"SELECT * FROM {fullTableName} WHERE {qcol} = {param.ParameterName} ORDER BY 1";
            return await QueryAsync(sql, param);
        }

        #endregion


        #region Bulk Insert (batched + progress + cancel + duplicate strategy)
        public static async Task BulkInsertAsync(
        string fullTableName,
        DataTable data,
        int batchSize = 1000,
        DuplicateStrategy duplicateStrategy = DuplicateStrategy.Error,
        IProgress<int>? progress = null,
        CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fullTableName))
                throw new ArgumentException("Table name must be provided.", nameof(fullTableName));

            var cfg = GetDbConfig();

            // ---------- SQL Server fast path (SqlBulkCopy) ----------
            if (cfg.provider == "System.Data.SqlClient")
            {
                await using var cn = (System.Data.SqlClient.SqlConnection)GetConn();
                await cn.OpenAsync(ct);

                var sqlParts = fullTableName.Split('.', 2);
                var ssSchema = sqlParts.Length == 2 ? sqlParts[0] : "dbo";
                var ssTable = sqlParts.Length == 2 ? sqlParts[1] : sqlParts[0];

                // Skip IDENTITY columns
                var identityCols = await GetAutoGeneratedColumnsAsync(cfg.provider, ssSchema, ssTable);

                using var bulk = new System.Data.SqlClient.SqlBulkCopy(cn)
                {
                    DestinationTableName = fullTableName,
                    BatchSize = Math.Max(1, batchSize),
                    BulkCopyTimeout = 0
                };
                bulk.NotifyAfter = Math.Min(500, Math.Max(1, batchSize));
                if (progress != null)
                    bulk.SqlRowsCopied += (_, e) => progress.Report((int)e.RowsCopied);

                var destCols = await ListColumnsAsync(fullTableName);
                var destSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow r in destCols.Rows)
                    destSet.Add(Convert.ToString(r["COLUMN_NAME"])!);

                foreach (DataColumn c in data.Columns)
                    if (destSet.Contains(c.ColumnName) && !identityCols.Contains(c.ColumnName))
                        bulk.ColumnMappings.Add(c.ColumnName, c.ColumnName);

                
                await bulk.WriteToServerAsync(data, ct);
                return;
            }

            // ---------- Universal path: MySQL / Postgres / Oracle ----------
            await using var conn = GetConn();
            await conn.OpenAsync(ct);

            var nameParts = fullTableName.Split('.', 2);
            string? schema = nameParts.Length == 2 ? nameParts[0] : null;
            string table = nameParts.Length == 2 ? nameParts[1] : nameParts[0];

            if (string.IsNullOrEmpty(schema))
            {
                schema = cfg.provider switch
                {
                    "Npgsql" => "public",
                    "MySqlConnector" or "MySql.Data.MySqlClient" => cfg.database,
                    "Oracle.ManagedDataAccess.Client" => cfg.username,
                    _ => null
                };
            }

            // Destination columns that exist and are present in DataTable
            var destCols2 = await ListColumnsAsync(fullTableName);
            var validCols = new List<string>();
            foreach (DataRow r in destCols2.Rows)
            {
                string col = Convert.ToString(r["COLUMN_NAME"])!;
                if (data.Columns.Contains(col))
                    validCols.Add(col);
            }

            // Drop auto-generated columns (identity/auto_increment/identity)
            var autoGen = await GetAutoGeneratedColumnsAsync(cfg.provider, schema, table);
            validCols.RemoveAll(c => autoGen.Contains(c));
            if (validCols.Count == 0) return;

            // Target identifier and base SQL
            string target = string.IsNullOrEmpty(schema)
                ? QuoteId(cfg.provider, table)
                : $"{QuoteId(cfg.provider, schema!)}.{QuoteId(cfg.provider, table)}";

            string colList = string.Join(", ", validCols.Select(c => QuoteId(cfg.provider, c)));
            string paramList = string.Join(", ", validCols.Select((c, i) => $"@p{i}"));
            string insertVerb = "INSERT INTO";
            string trailing = "";

            // Duplicate handling
            if (duplicateStrategy == DuplicateStrategy.Skip)
            {
                if (cfg.provider is "MySqlConnector" or "MySql.Data.MySqlClient")
                    insertVerb = "INSERT IGNORE INTO";
                else if (cfg.provider == "Npgsql")
                    trailing = " ON CONFLICT DO NOTHING";
            }
            else if (duplicateStrategy == DuplicateStrategy.Upsert)
            {
                if (cfg.provider is "MySqlConnector" or "MySql.Data.MySqlClient")
                {
                    var updateSet = string.Join(", ",
                        validCols.Select(c => $"{QuoteId(cfg.provider, c)}=VALUES({QuoteId(cfg.provider, c)})"));
                    trailing = $" ON DUPLICATE KEY UPDATE {updateSet}";
                }
                else if (cfg.provider == "Npgsql")
                {
                    var pk = await GetPrimaryKeyColumnsAsync(cfg.provider, schema!, table);
                    if (pk.Count == 0)
                        throw new InvalidOperationException("Upsert requires a primary key (PostgreSQL).");

                    var conflict = string.Join(", ", pk.Select(c => QuoteId(cfg.provider, c)));
                    var updateSet = string.Join(", ",
                        validCols.Where(c => !pk.Contains(c, StringComparer.OrdinalIgnoreCase))
                                 .Select(c => $"{QuoteId(cfg.provider, c)}=EXCLUDED.{QuoteId(cfg.provider, c)}"));
                    trailing = $" ON CONFLICT ({conflict}) DO UPDATE SET {updateSet}";
                }
            }

            string sqlBase = $"{insertVerb} {target} ({colList}) VALUES ({paramList}){trailing}";

            var factory = DbProviderFactories.GetFactory(cfg.provider);
            int total = data.Rows.Count;
            int size = Math.Max(1, batchSize);

            for (int offset = 0; offset < total; offset += size)
            {
                ct.ThrowIfCancellationRequested();

                int take = Math.Min(size, total - offset);
                using var tx = conn.BeginTransaction();
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = sqlBase;
                cmd.CommandTimeout = 0;

                // Create parameters once per batch
                var parms = new DbParameter[validCols.Count];
                for (int i = 0; i < validCols.Count; i++)
                {
                    var p = factory.CreateParameter();
                    p.ParameterName = $"@p{i}";
                    cmd.Parameters.Add(p);
                    parms[i] = p;
                }

                for (int i = 0; i < take; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var row = data.Rows[offset + i];
                    for (int c = 0; c < validCols.Count; c++)
                        parms[c].Value = row[validCols[c]] ?? DBNull.Value;

                    await cmd.ExecuteNonQueryAsync(ct);
                    progress?.Report(offset + i + 1);
                }

                await tx.CommitAsync(ct);
            }
        }
        #endregion


        #region Export to Excel
        public static async Task ExportToExcelAsync(string filePath, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must be provided.", nameof(tableName));

            var dt = await QueryAsync($"SELECT * FROM {tableName}");

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Export");
            ws.Cell(1, 1).InsertTable(dt, "ExportTable", true);
            workbook.SaveAs(filePath);
        }
        #endregion

        #region Export to PDF
        public static async Task ExportToPdfAsync(string filePath, string tableName, string title = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name must be provided.", nameof(tableName));

            var dt = await QueryAsync($"SELECT * FROM {tableName}");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());
                    page.Margin(25);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text(title ?? $"Export - {tableName}")
                        .SemiBold().FontSize(14).AlignCenter();

                    page.Content().Element(content =>
                    {
                        content.Table(table =>
                        {
                            // Calculate flexible widths: 
                            // Give short columns a fixed min width, long columns more space.
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (DataColumn col in dt.Columns)
                                {
                                    if (col.DataType == typeof(string) && col.MaxLength > 0 && col.MaxLength > 30)
                                        columns.RelativeColumn(2); // wider for long text
                                    else if (col.DataType == typeof(string))
                                        columns.RelativeColumn(1.5f);
                                    else
                                        columns.ConstantColumn(60); // fixed width for numbers/dates
                                }
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                foreach (DataColumn col in dt.Columns)
                                {
                                    header.Cell().Element(HeaderCellStyle)
                                          .Text(col.ColumnName).SemiBold();
                                }
                            });

                            // Table Body
                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (var value in row.ItemArray)
                                {
                                    table.Cell().Element(BodyCellStyle)
                                         .Text(value?.ToString() ?? string.Empty)
                                         .WrapAnywhere(); // ensures no overflow
                                }
                            }

                            // Styles
                            static IContainer HeaderCellStyle(IContainer c) =>
                                c.PaddingVertical(4).PaddingHorizontal(6)
                                 .Background(Colors.Grey.Lighten3)
                                 .BorderBottom(1).BorderColor(Colors.Grey.Medium);

                            static IContainer BodyCellStyle(IContainer c) =>
                                c.PaddingVertical(3).PaddingHorizontal(6)
                                 .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);
                        });
                    });

                    // Footer with page numbers
                    page.Footer().AlignRight().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            document.GeneratePdf(filePath);
        }
        #endregion

        #region column Helpers
        private static string QuoteId(string provider, string name) =>
        provider switch
        {
            "MySqlConnector" or "MySql.Data.MySqlClient" => $"`{name}`",
            "System.Data.SqlClient" => $"[{name}]",
            "Npgsql" or "Oracle.ManagedDataAccess.Client" => $"\"{name}\"",
            _ => name
        };
        #endregion

        #region Help AutoGenerated Columns

        private static async Task<HashSet<string>> GetAutoGeneratedColumnsAsync(
            string provider, string? schema, string table)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(table)) return names;

            // Normalise for Oracle (catalog is uppercase by default)
            var schemaForQuery = (provider == "Oracle.ManagedDataAccess.Client" && schema != null)
                ? schema.ToUpperInvariant()
                : schema;

            string? sql = provider switch
            {
                // SQL Server: identity columns
                "System.Data.SqlClient" => @"
            SELECT c.name AS COLUMN_NAME
            FROM sys.columns c
            JOIN sys.tables t  ON c.object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE s.name = @schema AND t.name = @table AND c.is_identity = 1",

                // MySQL: auto_increment
                "MySqlConnector" or "MySql.Data.MySqlClient" => @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table
              AND EXTRA LIKE '%auto_increment%'",

                // PostgreSQL: identity or serial (nextval default)
                "Npgsql" => @"
            SELECT column_name AS COLUMN_NAME
            FROM information_schema.columns
            WHERE table_schema = @schema AND table_name = @table
              AND (is_identity = 'YES' OR column_default LIKE 'nextval(%')",

                // Oracle 12c+: identity columns
                "Oracle.ManagedDataAccess.Client" => @"
            SELECT COLUMN_NAME
            FROM ALL_TAB_IDENTITY_COLS
            WHERE OWNER = :schema AND TABLE_NAME = :table",

                _ => null
            };

            if (sql is null || string.IsNullOrEmpty(schemaForQuery))
                return names;

            var factory = DbProviderFactories.GetFactory(provider);
            await using var cn = factory.CreateConnection();
            cn.ConnectionString = GetConnectionString();
            await cn.OpenAsync();

            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;

            // Parameter names / tokens per provider
            string P(string name) => provider switch
            {
                "Oracle.ManagedDataAccess.Client" => ":" + name,
                _ => "@" + name
            };

            var p1 = cmd.CreateParameter(); p1.ParameterName = P("schema"); p1.Value = schemaForQuery!;
            var p2 = cmd.CreateParameter(); p2.ParameterName = P("table"); p2.Value = table;
            cmd.Parameters.Add(p1); cmd.Parameters.Add(p2);

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                names.Add(Convert.ToString(r[0])!);

            return names;
        }


        #endregion

        #region Get Primary Key Columns
        private static async Task<HashSet<string>> GetPrimaryKeyColumnsAsync(string provider, string? schema, string table)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(table)) return set;

            // Normalize for Oracle (unquoted identifiers are stored upper-case)
            string qSchema = schema ?? "";
            string qTable = table;
            if (provider == "Oracle.ManagedDataAccess.Client")
            {
                if (!string.IsNullOrEmpty(qSchema)) qSchema = qSchema.ToUpperInvariant();
                qTable = qTable.ToUpperInvariant();
            }

            string? sql = provider switch
            {
                // SQL Server
                "System.Data.SqlClient" => @"
            SELECT kcu.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
              ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
             AND kcu.TABLE_SCHEMA    = tc.TABLE_SCHEMA
             AND kcu.TABLE_NAME      = tc.TABLE_NAME
            WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
              AND tc.TABLE_SCHEMA = @schema
              AND tc.TABLE_NAME   = @table
            ORDER BY kcu.ORDINAL_POSITION;",

                // MySQL
                "MySqlConnector" or "MySql.Data.MySqlClient" => @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_SCHEMA = @schema
              AND TABLE_NAME   = @table
              AND CONSTRAINT_NAME = 'PRIMARY'
            ORDER BY ORDINAL_POSITION;",

                // PostgreSQL
                "Npgsql" => @"
            SELECT kcu.column_name AS COLUMN_NAME
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu
              ON kcu.constraint_name = tc.constraint_name
             AND kcu.table_schema    = tc.table_schema
             AND kcu.table_name      = tc.table_name
            WHERE tc.constraint_type = 'PRIMARY KEY'
              AND tc.table_schema = @schema
              AND tc.table_name   = @table
            ORDER BY kcu.ordinal_position;",

                // Oracle
                "Oracle.ManagedDataAccess.Client" => @"
            SELECT cols.COLUMN_NAME
            FROM ALL_CONSTRAINTS cons
            JOIN ALL_CONS_COLUMNS cols
              ON cons.OWNER           = cols.OWNER
             AND cons.CONSTRAINT_NAME = cols.CONSTRAINT_NAME
            WHERE cons.CONSTRAINT_TYPE = 'P'
              AND cons.OWNER      = :schema
              AND cons.TABLE_NAME = :table
            ORDER BY cols.POSITION;",

                _ => null
            };

            if (sql is null) return set;

            var factory = DbProviderFactories.GetFactory(provider);
            await using var cn = factory.CreateConnection();
            cn.ConnectionString = GetConnectionString();
            await cn.OpenAsync();

            await using var cmd = cn.CreateCommand();
            cmd.CommandText = sql;

            // Parameter token differs on Oracle (":"), others use "@"
            string pSchema = provider == "Oracle.ManagedDataAccess.Client" ? ":schema" : "@schema";
            string pTable = provider == "Oracle.ManagedDataAccess.Client" ? ":table" : "@table";

            var ps = cmd.CreateParameter(); ps.ParameterName = pSchema; ps.Value = qSchema;
            var pt = cmd.CreateParameter(); pt.ParameterName = pTable; pt.Value = qTable;
            cmd.Parameters.Add(ps); cmd.Parameters.Add(pt);

            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                set.Add(Convert.ToString(r[0])!);

            return set;
        }

        #endregion
    }
}

