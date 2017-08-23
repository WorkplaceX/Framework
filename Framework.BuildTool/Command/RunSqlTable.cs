﻿namespace Framework.BuildTool
{
    using Database.dbo;
    using Framework.Application;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using System;
    using System.Data.SqlClient;
    using System.Text;

    public class CommandRunSqlTable : Command
    {
        public CommandRunSqlTable(AppBuildTool appBuildTool) 
            : base("runSqlTable", "Copy all in code defined tables and columns to database table FrameworkTable.")
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
                        SELECT Source.Name
                        EXCEPT
                        SELECT Target.Name)
                WHEN MATCHED THEN
	                UPDATE SET Target.Path = Source.Path, Target.IsActive = Source.IsActive
                WHEN NOT MATCHED BY TARGET THEN
	                INSERT (Name, Path, ApplicationTypeId, IsActive)
	                VALUES (Source.Name, Source.Path, Source.ApplicationTypeId, Source.IsActive);
                ";
                StringBuilder sqlSelect = new StringBuilder();
                bool isFirst = true;
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
                        "(SELECT '{0}' AS Name, '{1}' AS Path, (SELECT ApplicationType.Id FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Name = '{2}') AS ApplicationTypeId, CASE WHEN '{3}' = 'True' THEN 1 ELSE 0 END AS IsActive )",
                        frameworkApplication.Name, frameworkApplication.Path, frameworkApplication.Type, frameworkApplication.IsActive));
                }
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                using (SqlCommand command = new SqlCommand(sqlUpsert, connection))
                {
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
                foreach (Type type in UtilFramework.ApplicationTypeList(AppBuildTool.App.GetType()))
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
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                string tableName = UtilDataAccessLayer.TypeRowToName(typeRow);
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
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sqlSelect.Append(" UNION ALL\r\n");
                    }
                    string tableName = UtilDataAccessLayer.TypeRowToName(typeRow);
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