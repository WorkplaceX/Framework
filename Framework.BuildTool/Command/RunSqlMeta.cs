namespace Framework.BuildTool
{
    using Database.dbo;
    using Framework.Application;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Text;

    /// <summary>
    /// Executed as part of RunSqlCreate command.
    /// </summary>
    public class CommandRunSqlMeta : Command
    {
        public CommandRunSqlMeta(AppBuildTool appBuildTool) 
            : base("runSqlMeta", "Copy all in code declared Apps, tables and columns to database table FrameworkTable.")
        {
            this.AppBuildTool = appBuildTool;
        }

        public readonly AppBuildTool AppBuildTool;

        /// <summary>
        /// Add AppSetup to table FrameworkApplication.
        /// </summary>
        private void RunSqlApplication(string connectionString)
        {
            UtilFramework.Log("### Start RunSqlApplication");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE FrameworkApplication SET IsActive = 0";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
                string sqlUpsert = @"
                MERGE INTO FrameworkApplication AS Target
                USING ({0}) AS Source
	                ON NOT EXISTS(
                        SELECT Source.Path
                        EXCEPT
                        SELECT Target.Path)
                WHEN MATCHED THEN
	                UPDATE SET Target.Text = Source.Text, Target.Path = Source.Path, Target.ApplicationTypeId = Source.ApplicationTypeId, Target.IsActive = Source.IsActive
                WHEN NOT MATCHED BY TARGET THEN
	                INSERT (Text, Path, ApplicationTypeId, IsActive)
	                VALUES (Source.Text, Source.Path, Source.ApplicationTypeId, Source.IsActive);
                ";
                StringBuilder sqlSelect = new StringBuilder();
                bool isFirst = true;
                List<SqlParameter> parameterList = new List<SqlParameter>();
                foreach (FrameworkApplicationView frameworkApplication in AppBuildTool.DbFrameworkApplicationView())
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sqlSelect.Append(" UNION ALL\r\n");
                    }
                    sqlSelect.Append(string.Format(
                        "(SELECT {0} AS Text, {1} AS Path, (SELECT ApplicationType.Id FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Name = {2}) AS ApplicationTypeId, {3} AS IsActive)",
                        UtilBuildToolInternal.UtilDataAccessLayer.Parameter(frameworkApplication.Text, SqlDbType.NVarChar, parameterList),
                        UtilBuildToolInternal.UtilDataAccessLayer.Parameter(frameworkApplication.Path, SqlDbType.NVarChar, parameterList),
                        UtilBuildToolInternal.UtilDataAccessLayer.Parameter(frameworkApplication.Type, SqlDbType.NVarChar, parameterList),
                        UtilBuildToolInternal.UtilDataAccessLayer.Parameter(frameworkApplication.IsActive, SqlDbType.Bit, parameterList)));
                }
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                using (SqlCommand command = new SqlCommand(sqlUpsert, connection))
                {
                    UtilBuildToolInternal.UtilDataAccessLayer.Parameter(command, parameterList);
                    command.ExecuteNonQuery();
                }
            }
            UtilFramework.Log("### Exit RunSqlApplication");
        }

        /// <summary>
        /// Add available App types to table FrameworkApplicationType.
        /// </summary>
        private void RunSqlApplicationType(string connectionString)
        {
            UtilFramework.Log("### Start RunSqlApplicationType");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "UPDATE FrameworkApplicationType SET IsExist = 0";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
                string sqlUpsert = @"
                MERGE INTO FrameworkApplicationType AS Target
                USING ({0}) AS Source
	                ON NOT EXISTS(
                        SELECT Source.Name
                        EXCEPT
                        SELECT Target.Name)
                WHEN MATCHED THEN
	                UPDATE SET Target.IsExist = 1
                WHEN NOT MATCHED BY TARGET THEN
	                INSERT (Name, IsExist)
	                VALUES (Source.Name, 1);
                ";
                StringBuilder sqlSelect = new StringBuilder();
                bool isFirst = true;
                foreach (Type type in UtilApplication.ApplicationTypeList(AppBuildTool.App.GetType()))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sqlSelect.Append(" UNION ALL\r\n");
                    }
                    sqlSelect.Append(string.Format("(SELECT '{0}' AS Name)", UtilFramework.TypeToName(type)));
                }
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                using (SqlCommand command = new SqlCommand(sqlUpsert, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            UtilFramework.Log("### Exit RunSqlApplicationType");
        }

        private void RunSqlTable(string connectionString)
        {
            UtilFramework.Log("### Start RunSqlTable");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string sql = "UPDATE FrameworkTable SET IsExist = 0";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
            string sqlUpsert = @"
            MERGE INTO FrameworkTable AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.Name
                    EXCEPT
                    SELECT Target.Name)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (Name, IsExist)
	            VALUES (Source.Name, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            foreach (Type typeRow in UtilBuildToolInternal.UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                string tableName = UtilBuildToolInternal.UtilDataAccessLayer.TypeRowToName(typeRow);
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                sqlSelect.Append(string.Format("(SELECT '{0}' AS Name)", tableName));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            using (SqlCommand command = new SqlCommand(sqlUpsert, connection))
            {
                command.ExecuteNonQuery();
            }
            UtilFramework.Log("### Exit RunSqlTable");
        }

        private void RunSqlColumn(string connectionString)
        {
            UtilFramework.Log("### Start RunSqlColumn");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string sql = "UPDATE FrameworkColumn SET IsExist = 0";
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
            string sqlUpsert = @"
            MERGE INTO FrameworkColumn AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT TableX.Id AS TableId FROM FrameworkTable TableX WHERE TableX.Name = Source.TableNameSql), Source.FieldNameSql, Source.FieldNameCSharp
                    EXCEPT
                    SELECT Target.TableId, Target.FieldNameSql, Target.FieldNameCSharp)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TableId, FieldNameSql, FieldNameCSharp, IsExist)
	            VALUES ((SELECT TableX.Id AS TableId FROM FrameworkTable TableX WHERE TableX.Name = Source.TableNameSql), Source.FieldNameSql, Source.FieldNameCSharp, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            foreach (Type typeRow in UtilBuildToolInternal.UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                foreach (Cell column in UtilBuildToolInternal.UtilDataAccessLayer.ColumnList(typeRow))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sqlSelect.Append(" UNION ALL\r\n");
                    }
                    string tableName = UtilBuildToolInternal.UtilDataAccessLayer.TypeRowToName(typeRow);
                    sqlSelect.Append(string.Format("(SELECT '{0}' AS TableNameSql, CASE WHEN '{1}' = '' THEN NULL ELSE '{1}' END AS FieldNameSql, '{2}' AS FieldNameCSharp)", tableName, column.FieldNameSql, column.FieldNameCSharp));
                }
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            using (SqlCommand command = new SqlCommand(sqlUpsert, connection))
            {
                command.ExecuteNonQuery();
            }
            UtilFramework.Log("### Exit RunSqlColumn");
        }

        public override void Run()
        {
            RunSqlTable(ConnectionManagerServer.ConnectionString);
            RunSqlColumn(ConnectionManagerServer.ConnectionString);
            RunSqlApplicationType(ConnectionManagerServer.ConnectionString);
            RunSqlApplication(ConnectionManagerServer.ConnectionString);
        }
    }
}
