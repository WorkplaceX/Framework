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
    using System.Reflection;
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
        private void RunSqlApplication()
        {
            UtilFramework.Log("### Start RunSqlApplication");
            string sql = "UPDATE FrameworkApplication SET IsActive = 0";
            UtilBuildTool.SqlCommand(sql, true);
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
            UtilBuildTool.SqlCommand(sqlUpsert, true, parameterList.ToArray());
            UtilFramework.Log("### Exit RunSqlApplication");
        }

        /// <summary>
        /// Add available App types to table FrameworkApplicationType.
        /// </summary>
        private void RunSqlApplicationType()
        {
            UtilFramework.Log("### Start RunSqlApplicationType");
            string sql = "UPDATE FrameworkApplicationType SET IsExist = 0";
            UtilBuildTool.SqlCommand(sql, true);
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
            UtilBuildTool.SqlCommand(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlApplicationType");
        }

        private void RunSqlTable()
        {
            UtilFramework.Log("### Start RunSqlTable");
            string sql = "UPDATE FrameworkTable SET IsExist = 0";
            UtilBuildTool.SqlCommand(sql, true);
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
                string tableName = UtilBuildToolInternal.UtilDataAccessLayer.TypeRowToNameCSharp(typeRow);
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                sqlSelect.Append(string.Format("SELECT '{0}' AS Name", tableName));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlCommand(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlTable");
        }

        private void RunSqlGrid()
        {
            UtilFramework.Log("### Start RunSqlGrid");
            string sql = "UPDATE FrameworkGrid SET IsExist = 0";
            UtilBuildTool.SqlCommand(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkGrid AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT FrameworkTable.Id AS TableId FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Name = Source.TableName) AS TableId, Source.GridName AS Name
                    EXCEPT
                    SELECT Target.TableId, Target.Name)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TableId, Name, IsExist)
	            VALUES ((SELECT FrameworkTable.Id AS TableId FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Name = Source.TableName), Source.GridName, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            foreach (Type typeRow in UtilBuildToolInternal.UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                string tableName = UtilBuildToolInternal.UtilDataAccessLayer.TypeRowToNameCSharp(typeRow);
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                sqlSelect.Append(string.Format("SELECT '{0}' AS TableName, NULL AS GridName", tableName));
            }
            //
            foreach (Type typeRow in UtilBuildToolInternal.UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                foreach (PropertyInfo propertyInfo in typeRow.GetProperties(BindingFlags.Static | BindingFlags.Public)) // Static declared GridName property on class Row.
                {
                    if (UtilFramework.IsSubclassOf(typeof(GridName), propertyInfo.PropertyType))
                    {
                        GridName gridName = (GridName)propertyInfo.GetValue(null);
                        //
                        Type gridTypeRow = (Type)gridName.GetType().GetField("TypeRowInternal", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gridName);
                        string gridNameExclusive = (string)gridName.GetType().GetTypeInfo().GetProperty("NameExclusive", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gridName);
                        string gridTableName = UtilBuildToolInternal.UtilDataAccessLayer.TypeRowToNameCSharp(gridTypeRow);
                        sqlSelect.Append(" UNION ALL\r\n");
                        sqlSelect.Append(string.Format("SELECT '{0}' AS TableName, '{1}' AS GridName", gridTableName, gridNameExclusive));
                    }
                }
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlCommand(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlTable");
        }

        private void RunSqlColumn()
        {
            UtilFramework.Log("### Start RunSqlColumn");
            string sql = "UPDATE FrameworkColumn SET IsExist = 0";
            UtilBuildTool.SqlCommand(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkColumn AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT TableX.Id AS TableId FROM FrameworkTable TableX WHERE TableX.Name = Source.TableName), Source.ColumnName
                    EXCEPT
                    SELECT Target.TableId, Target.Name)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TableId, Name, IsExist)
	            VALUES ((SELECT TableX.Id AS TableId FROM FrameworkTable TableX WHERE TableX.Name = Source.TableName), Source.ColumnName, 1);
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
                    string tableNameCSharp = UtilBuildToolInternal.UtilDataAccessLayer.TypeRowToNameCSharp(typeRow);
                    sqlSelect.Append(string.Format("(SELECT '{0}' AS TableName, '{1}' AS ColumnName)", tableNameCSharp, column.ColumnNameCSharp));
                }
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlCommand(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlColumn");
        }

        public override void Run()
        {
            RunSqlTable();
            RunSqlColumn();
            RunSqlGrid();
            RunSqlApplicationType();
            RunSqlApplication();
        }
    }
}
