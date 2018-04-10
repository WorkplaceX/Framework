namespace Framework.BuildTool
{
    using Database.dbo;
    using Framework.Application;
    using Framework.Application.Config;
    using Framework.Component;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
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
        /// Add AppConfig to sql table FrameworkApplication.
        /// </summary>
        private void RunSqlApplication()
        {
            UtilFramework.Log("### Start RunSqlApplication");
            string sql = "UPDATE FrameworkApplication SET IsActive = 0";
            UtilBuildTool.SqlExecute(sql, true);
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
            foreach (FrameworkApplicationDisplay frameworkApplication in AppBuildTool.DbFrameworkApplicationView())
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
                    "(SELECT {0} AS Text, {1} AS Path, (SELECT ApplicationType.Id FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.TypeName = {2}) AS ApplicationTypeId, {3} AS IsActive)",
                    UtilDataAccessLayer.ExecuteParameterAdd(frameworkApplication.Text, SqlDbType.NVarChar, parameterList),
                    UtilDataAccessLayer.ExecuteParameterAdd(frameworkApplication.Path, SqlDbType.NVarChar, parameterList),
                    UtilDataAccessLayer.ExecuteParameterAdd(frameworkApplication.TypeName, SqlDbType.NVarChar, parameterList, isUseParam: false),
                    UtilDataAccessLayer.ExecuteParameterAdd(frameworkApplication.IsActive, SqlDbType.Bit, parameterList, isUseParam: false)));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            UtilFramework.Log("### Exit RunSqlApplication");
        }

        /// <summary>
        /// Add available App types to table FrameworkApplicationType.
        /// </summary>
        private void RunSqlApplicationType()
        {
            UtilFramework.Log("### Start RunSqlApplicationType");
            string sql = "UPDATE FrameworkApplicationType SET IsExist = 0";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkApplicationType AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.TypeName
                    EXCEPT
                    SELECT Target.TypeName)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TypeName, IsExist)
	            VALUES (Source.TypeName, 1);
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
                sqlSelect.Append(string.Format("(SELECT '{0}' AS TypeName)", UtilFramework.TypeToName(type)));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlExecute(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlApplicationType");
        }

        /// <summary>
        /// Populate and update table FrameworkTable.
        /// </summary>
        private void RunSqlTable()
        {
            UtilFramework.Log("### Start RunSqlTable");
            string sql = "UPDATE FrameworkTable SET IsExist = 0";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkTable AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.TableNameCSharp
                    EXCEPT
                    SELECT Target.TableNameCSharp)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TableNameCSharp, TableNameSql, IsExist)
	            VALUES (Source.TableNameCSharp, Source.TableNameSql, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                string tableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow);
                SqlTableAttribute tableAttribute = (SqlTableAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlTableAttribute));
                string tableNameSql = "NULL";
                if (tableAttribute != null && (tableAttribute.SqlSchemaName != null || tableAttribute.SqlTableName != null))
                {
                    tableNameSql = string.Format("'[{0}].[{1}]'", tableAttribute.SqlSchemaName, tableAttribute.SqlTableName);
                }
                sqlSelect.Append(string.Format("SELECT '{0}' AS TableNameCSharp, {1} AS TableNameSql", tableNameCSharp, tableNameSql));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlExecute(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlTable");
        }

        /// <summary>
        /// Populate and update table FrameworkGrid.
        /// </summary>
        private void RunSqlGrid()
        {
            UtilFramework.Log("### Start RunSqlGrid");
            string sql = "UPDATE FrameworkGrid SET IsExist = 0";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkGrid AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT FrameworkTable.Id AS TableId FROM FrameworkTable FrameworkTable WHERE FrameworkTable.TableNameCSharp = Source.TableNameCSharp) AS TableId, Source.GridName
                    EXCEPT
                    SELECT Target.TableId, Target.GridName)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TableId, GridName, IsExist)
	            VALUES ((SELECT FrameworkTable.Id AS TableId FROM FrameworkTable FrameworkTable WHERE FrameworkTable.TableNameCSharp = Source.TableNameCSharp), Source.GridName, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                string tableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow);
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                sqlSelect.Append(string.Format("SELECT '{0}' AS TableNameCSharp, NULL AS GridName", tableNameCSharp));
            }
            //
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                // Property
                foreach (PropertyInfo propertyInfo in typeRow.GetProperties(BindingFlags.Static | BindingFlags.Public)) // Static declared GridName property on class Row.
                {
                    if (UtilFramework.IsSubclassOf(propertyInfo.PropertyType, typeof(GridName)))
                    {
                        GridName gridName = (GridName)propertyInfo.GetValue(null);
                        //
                        Type gridTypeRow = (Type)gridName.GetType().GetField("TypeRowInternal", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gridName);
                        string gridNameExclusive = (string)gridName.GetType().GetTypeInfo().GetProperty("NameExclusive", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gridName);
                        string gridTableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(gridTypeRow);
                        if (gridNameExclusive != null) // Gets added automatically anyway: "public static GridName<Flight> GridName { get { return new GridName<Flight>(); } }"
                        {
                            sqlSelect.Append(" UNION ALL\r\n");
                            sqlSelect.Append(string.Format("SELECT '{0}' AS TableNameCSharp, '{1}' AS GridName", gridTableNameCSharp, gridNameExclusive));
                        }
                    }
                }
                // Field
                foreach (FieldInfo fieldInfo in typeRow.GetFields(BindingFlags.Static | BindingFlags.Public)) // Static declared GridName property on class Row.
                {
                    if (UtilFramework.IsSubclassOf(fieldInfo.FieldType, typeof(GridName)))
                    {
                        GridName gridName = (GridName)fieldInfo.GetValue(null);
                        //
                        Type gridTypeRow = (Type)gridName.GetType().GetField("TypeRowInternal", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gridName);
                        string gridNameExclusive = (string)gridName.GetType().GetTypeInfo().GetProperty("NameExclusive", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gridName);
                        string gridTableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(gridTypeRow);
                        if (gridNameExclusive != null) // Gets added automatically anyway: "public static GridName<Flight> GridName { get { return new GridName<Flight>(); } }"
                        {
                            sqlSelect.Append(" UNION ALL\r\n");
                            sqlSelect.Append(string.Format("SELECT '{0}' AS TableNameCSharp, '{1}' AS GridName", gridTableNameCSharp, gridNameExclusive));
                        }
                    }
                }
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlExecute(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlGrid");
        }

        /// <summary>
        /// Populate and update table FrameworkComponent.
        /// </summary>
        private void RunSqlComponent()
        {
            UtilFramework.Log("### Start RunSqlPage");
            string sql = "UPDATE FrameworkComponent SET IsExist = 0";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkComponent AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.ComponentNameCSharp
                    EXCEPT
                    SELECT Target.ComponentNameCSharp)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsPage = Source.IsPage, Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (ComponentNameCSharp, IsPage, IsExist)
	            VALUES (Source.ComponentNameCSharp, Source.IsPage, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<Type> typeComponentList = UtilFramework.TypeList(AppBuildTool.App.GetType(), typeof(Component));
            foreach (Type typeComponent in typeComponentList)
            {
                string componentNameCSharp = UtilFramework.TypeToName(typeComponent);
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                bool isPage = UtilFramework.IsSubclassOf(typeComponent, typeof(Page));
                sqlSelect.Append(string.Format("SELECT '{0}' AS ComponentNameCSharp, {1} AS IsPage", componentNameCSharp, isPage ? 1 : 0));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlExecute(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlPage");
        }

        /// <summary>
        /// Populate and update table FrameworkColumn.
        /// </summary>
        private void RunSqlColumn()
        {
            UtilFramework.Log("### Start RunSqlColumn");
            string sql = "UPDATE FrameworkColumn SET IsExist = 0";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkColumn AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT TableX.Id AS TableId FROM FrameworkTable TableX WHERE TableX.TableNameCSharp = Source.TableNameCSharp), Source.ColumnNameCSharp
                    EXCEPT
                    SELECT Target.TableId, Target.ColumnNameCSharp)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TableId, ColumnNameCSharp, ColumnNameSql, IsExist)
	            VALUES ((SELECT TableX.Id AS TableId FROM FrameworkTable TableX WHERE TableX.TableNameCSharp = Source.TableNameCSharp), Source.ColumnNameCSharp, Source.ColumnNameSql, 1);
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
                    string tableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow);
                    string columnNameCSharp = column.ColumnNameCSharp;
                    string columnNameSql = column.ColumnNameSql == null ? "NULL" : string.Format("'[{0}]'", column.ColumnNameSql);
                    sqlSelect.Append(string.Format("(SELECT '{0}' AS TableNameCSharp, '{1}' AS ColumnNameCSharp, {2} AS ColumnNameSql)", tableNameCSharp, columnNameCSharp, columnNameSql));
                }
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlExecute(sqlUpsert, true);
            UtilFramework.Log("### Exit RunSqlColumn");
        }

        /// <summary>
        /// Populate and update table FrameworkConfigGrid.
        /// </summary>
        private void RunSqlConfigGrid()
        {
            UtilFramework.Log("### Start RunSqlConfigGrid");
            string sqlUpsert = @"
            MERGE INTO FrameworkConfigGrid AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.GridId
                    EXCEPT
                    SELECT Target.GridId)
            WHEN MATCHED THEN
	            UPDATE SET Target.PageRowCountDefault = Source.PageRowCount, Target.IsInsertDefault = Source.IsInsert
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (GridId, PageRowCountDefault, IsInsertDefault)
	            VALUES (Source.GridId, Source.PageRowCount, IsInsert);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<SqlParameter> parameterList = new List<SqlParameter>();
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                foreach (ConfigGridAttribute config in typeRow.GetTypeInfo().GetCustomAttributes(typeof(ConfigGridAttribute)))
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sqlSelect.Append(" UNION ALL\r\n");
                    }
                    string tableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow);
                    sqlSelect.Append(string.Format(
                        "SELECT (SELECT Config.GridId FROM FrameworkConfigGridView Config WHERE EXISTS(SELECT Config.GridName, Config.TableNameCSharp INTERSECT SELECT {0} AS GridName, {1} AS TableNameCSharp)) AS GridId, {2} AS PageRowCount, {3} AS IsInsert",
                        UtilDataAccessLayer.Parameter(config.GridName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(tableNameCSharp, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(config.PageRowCount, SqlDbType.Int, parameterList),
                        UtilDataAccessLayer.Parameter(config.IsInsert, SqlDbType.Bit, parameterList)));
                }
            }
            if (isFirst == false)
            {
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            }
            UtilFramework.Log("### Exit RunSqlConfigGrid");
        }

        /// <summary>
        /// Populate and update table FrameworkConfigColumn.
        /// </summary>
        private void RunSqlConfigColumn()
        {
            UtilFramework.Log("### Start RunSqlConfigColumn");
            string sqlUpsert = @"
            MERGE INTO FrameworkConfigColumn AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.GridId, Source.ColumnId
                    EXCEPT
                    SELECT Target.GridId, Target.ColumnId)
            WHEN MATCHED THEN
	            UPDATE SET 
                    Target.TextDefault = Source.Text, 
                    Target.DescriptionDefault = Source.Description,
                    Target.IsVisibleDefault = Source.IsVisible,
                    Target.IsReadOnlyDefault = Source.IsReadOnly,
                    Target.SortDefault = Source.Sort,
                    Target.WidthPercentDefault = Source.WidthPercent
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (GridId, ColumnId, 
                    TextDefault, 
                    DescriptionDefault, 
                    IsVisibleDefault, 
                    IsReadOnlyDefault, 
                    SortDefault, 
                    WidthPercentDefault)
	            VALUES (Source.GridId, Source.ColumnId, 
                    Source.Text, 
                    Source.Description, 
                    Source.IsVisible, 
                    Source.IsReadOnly, 
                    Source.Sort, 
                    Source.WidthPercent);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<SqlParameter> parameterList = new List<SqlParameter>();
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App))) // Row
            {
                foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow)) // Column
                {
                    foreach (ConfigColumnAttribute config in column.GetType().GetTypeInfo().GetCustomAttributes(typeof(ConfigColumnAttribute))) // Attribute
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            sqlSelect.Append(" UNION ALL\r\n");
                        }
                        string tableNameCSharp = UtilDataAccessLayer.TypeRowToTableNameCSharp(typeRow);
                        sqlSelect.Append(string.Format(
                            "SELECT " +
                            "(SELECT Config.GridId FROM FrameworkConfigColumnView Config WHERE EXISTS(SELECT Config.GridName, Config.TableNameCSharp, Config.ColumnNameCSharp INTERSECT SELECT {0} AS GridName, {1} AS TableNameCSharp, {2} AS ColumnNameCSharp)) AS GridId, " +
                            "(SELECT Config.ColumnId FROM FrameworkConfigColumnView Config WHERE EXISTS(SELECT Config.GridName, Config.TableNameCSharp, Config.ColumnNameCSharp INTERSECT SELECT {0} AS GridName, {1} AS TableNameCSharp, {2} AS ColumnNameCSharp)) AS ColumnId, " +
                            "{3} AS Text, " +
                            "{4} AS Description, " +
                            "{5} AS IsVisible, " +
                            "{6} AS IsReadOnly, " +
                            "{7} AS Sort, " +
                            "{8} AS WidthPercent",
                            UtilDataAccessLayer.Parameter(config.GridName, SqlDbType.NVarChar, parameterList),
                            UtilDataAccessLayer.Parameter(tableNameCSharp, SqlDbType.NVarChar, parameterList),
                            UtilDataAccessLayer.Parameter(column.ColumnNameCSharp, SqlDbType.NVarChar, parameterList),
                            UtilDataAccessLayer.Parameter(config.Text, SqlDbType.NVarChar, parameterList),
                            UtilDataAccessLayer.Parameter(config.Description, SqlDbType.NVarChar, parameterList),
                            UtilDataAccessLayer.Parameter(config.IsVisible, SqlDbType.Bit, parameterList),
                            UtilDataAccessLayer.Parameter(config.IsReadOnly, SqlDbType.Bit, parameterList),
                            UtilDataAccessLayer.Parameter(config.Sort, SqlDbType.Int, parameterList),
                            UtilDataAccessLayer.Parameter(config.WidthPercent, SqlDbType.Int, parameterList)));
                    }
                }
            }
            if (isFirst == false)
            {
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            }
            UtilFramework.Log("### Exit RunSqlConfigColumn");
        }

        /// <summary>
        /// BuiltIn User, Role, Permission structure.
        /// </summary>
        private class BuiltInRole
        {
            public string ApplicationTypeName;

            public string RoleName;

            public string PermissionName;

            public string PermissionDescription;

            public string UserName;
        }

        /// <summary>
        /// Populate BuiltIn User, Role, Permission structure as reflected in code.
        /// </summary>
        private void RunSqlBuiltInRole(List<BuiltInRole> roleList)
        {
            string sql = "UPDATE FrameworkLoginRole SET IsBuiltInExist = 0 WHERE IsBuiltIn = 1";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkLoginRole AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT ApplicationType.Id AS ApplicationTypeId FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.TypeName = Source.ApplicationTypeName) AS ApplicationTypeId, Source.RoleName, 1 AS IsBuiltIn
                    EXCEPT
                    SELECT Target.ApplicationTypeId, Target.RoleName, Target.IsBuiltIn)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsBuiltInExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (ApplicationTypeId, RoleName, IsBuiltIn, IsBuiltInExist)
	            VALUES ((SELECT ApplicationType.Id AS ApplicationTypeId FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.TypeName = Source.ApplicationTypeName), Source.RoleName, 1, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<SqlParameter> parameterList = new List<SqlParameter>();
            foreach (BuiltInRole role in roleList.GroupBy(item => new { item.ApplicationTypeName, item.RoleName }).Select(group => group.First()))
            {
                if (role.RoleName != null)
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
                        "SELECT {0} AS ApplicationTypeName, {1} AS RoleName",
                        UtilDataAccessLayer.Parameter(role.ApplicationTypeName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(role.RoleName, SqlDbType.NVarChar, parameterList)));
                }
            }
            if (isFirst == false)
            {
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            }
        }

        private void RunSqlBuiltInRolePermission(List<BuiltInRole> roleList)
        {
            string sql = "UPDATE FrameworkLoginRolePermission SET IsActive = 0 WHERE IsBuiltIn = 1";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkLoginRolePermission AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT 
                        (SELECT Data.Id FROM FrameworkLoginRoleDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.RoleName = Source.RoleName) AS RoleId,
                        (SELECT Data.Id AS PermissionId FROM FrameworkLoginPermissionDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.PermissionName = Source.PermissionName) AS PermissionId,
                        1 AS IsBuiltIn
                    EXCEPT
                    SELECT 
                        Target.RoleId, 
                        Target.PermissionId, 
                        Target.IsBuiltIn)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsActive = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (RoleId, PermissionId, IsBuiltIn, IsActive)
	            VALUES (
                    (SELECT Data.Id FROM FrameworkLoginRoleDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.RoleName = Source.RoleName), -- AS RoleId, 
                    (SELECT Data.Id AS PermissionId FROM FrameworkLoginPermissionDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.PermissionName = Source.PermissionName), -- AS PermissionId, 
                    1, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<SqlParameter> parameterList = new List<SqlParameter>();
            foreach (BuiltInRole rolePermission in roleList.GroupBy(item => new { item.ApplicationTypeName, item.RoleName, item.PermissionName }).Select(group => group.First()))
            {
                if (rolePermission.RoleName != null && rolePermission.PermissionName != null)
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
                        "SELECT {0} AS ApplicationTypeName, {1} AS PermissionName, {2} AS RoleName",
                        UtilDataAccessLayer.Parameter(rolePermission.ApplicationTypeName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(rolePermission.PermissionName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(rolePermission.RoleName, SqlDbType.NVarChar, parameterList)));
                }
            }
            if (isFirst == false)
            {
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            }
        }

        private void RunSqlBuiltInUserRole(List<BuiltInRole> roleList)
        {
            string sql = "UPDATE FrameworkLoginUserRole SET IsActive = 0 WHERE IsBuiltIn = 1";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkLoginUserRole AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT 
                        (SELECT Data.Id AS UserId FROM FrameworkLoginUserDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.UserName = Source.UserName) AS UserId,
                        (SELECT Data.Id AS RoleId FROM FrameworkLoginRoleDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.RoleName = Source.RoleName) AS RoleId,
                        1 AS IsBuiltIn
                    EXCEPT
                    SELECT 
                        Target.UserId, 
                        Target.RoleId, 
                        Target.IsBuiltIn)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsActive = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (UserId, RoleId, IsBuiltIn, IsActive)
	            VALUES (
                    (SELECT Data.Id AS UserId FROM FrameworkLoginUserDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.UserName = Source.UserName), -- AS UserId,
                    (SELECT Data.Id AS RoleId FROM FrameworkLoginRoleDisplay Data WHERE Data.ApplicationTypeName = Source.ApplicationTypeName AND Data.RoleName = Source.RoleName), -- AS RoleId,
                    1, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<SqlParameter> parameterList = new List<SqlParameter>();
            foreach (BuiltInRole userRole in roleList.GroupBy(item => new { item.ApplicationTypeName, item.UserName, item.RoleName }).Select(group => group.First()))
            {
                if (userRole.UserName != null && userRole.RoleName != null)
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
                        "SELECT {0} AS ApplicationTypeName, {1} AS UserName, {2} AS RoleName",
                        UtilDataAccessLayer.Parameter(userRole.ApplicationTypeName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(userRole.UserName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(userRole.RoleName, SqlDbType.NVarChar, parameterList)));
                }
            }
            if (isFirst == false)
            {
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            }
        }

        private void RunSqlBuiltIn(out List<BuiltInRole> roleList)
        {
            roleList = new List<BuiltInRole>();
            foreach (Type typeApp in UtilFramework.TypeList(AppBuildTool.App.GetType(), typeof(App)))
            {
                UtilFramework.TypeToObject(typeApp);
                foreach (MethodInfo methodInfo in typeApp.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                {
                    if (methodInfo.ReturnType == typeof(FrameworkLoginPermissionDisplay))
                    {
                        FrameworkLoginPermissionDisplay permission = (FrameworkLoginPermissionDisplay)methodInfo.Invoke(null, new object[] { });
                        var typeInAssemblyList = UtilFramework.TypeInAssemblyList(typeApp);
                        Type typeAppDeclaredOnPermission = UtilFramework.TypeFromName(permission.ApplicationTypeName, typeInAssemblyList); // ApplicationTypeName declared on BuiltIn Permission.
                        UtilFramework.Assert(UtilFramework.IsSubclassOf(typeApp, typeAppDeclaredOnPermission)); // Permission gets derived.
                        string applicationTypeName = UtilFramework.TypeToName(typeApp);
                        // RolePermission
                        foreach (string roleName in permission.BuiltInRoleList())
                        {
                            roleList.Add(new BuiltInRole() { ApplicationTypeName = applicationTypeName, PermissionName = permission.PermissionName, PermissionDescription = permission.Description, RoleName = roleName });
                        }
                    }
                }
            }
            foreach (Type typeApp in UtilFramework.TypeList(AppBuildTool.App.GetType(), typeof(App)))
            {
                foreach (MethodInfo methodInfo in typeApp.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                {
                    if (methodInfo.ReturnType == typeof(FrameworkLoginUser))
                    {
                        FrameworkLoginUser user = (FrameworkLoginUser)methodInfo.Invoke(null, new object[] { });
                        string applicationTypeName = UtilFramework.TypeToName(typeApp);
                        // UserRole
                        foreach (string roleName in user.BuiltInRoleList())
                        {
                            roleList.Add(new BuiltInRole() { ApplicationTypeName = applicationTypeName, UserName = user.UserName, RoleName = roleName });
                        }
                    }
                }
            }
        }

        private void RunSqlBuiltInPermission(List<BuiltInRole> roleList)
        {
            string sql = "UPDATE FrameworkLoginPermission SET IsExist = 0";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkLoginPermission AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT ApplicationType.Id AS ApplicationTypeId FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.TypeName = Source.ApplicationTypeName) AS ApplicationTypeId, Source.PermissionName
                    EXCEPT
                    SELECT Target.ApplicationTypeId, Target.PermissionName)
            WHEN MATCHED THEN
	            UPDATE SET Target.Description = Source.Description, Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (ApplicationTypeId, PermissionName, Description, IsExist)
	            VALUES ((SELECT ApplicationType.Id AS ApplicationTypeId FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.TypeName = Source.ApplicationTypeName), Source.PermissionName, Source.Description, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<SqlParameter> parameterList = new List<SqlParameter>();
            foreach (BuiltInRole permission in roleList.GroupBy(item => new { item.ApplicationTypeName, item.PermissionName }).Select(group => group.First()))
            {
                if (permission.PermissionName != null)
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
                        "SELECT {0} AS ApplicationTypeName, {1} AS PermissionName, {2} AS Description",
                        UtilDataAccessLayer.Parameter(permission.ApplicationTypeName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(permission.PermissionName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(permission.PermissionDescription, SqlDbType.NVarChar, parameterList)));
                }
            }
            if (isFirst == false)
            {
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            }
        }

        private void RunSqlBuiltInUser(List<BuiltInRole> roleList)
        {
            string sql = "UPDATE FrameworkLoginUser SET IsBuiltInExist = 0 WHERE IsBuiltIn = 1";
            UtilBuildTool.SqlExecute(sql, true);
            string sqlUpsert = @"
            MERGE INTO FrameworkLoginUser AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT (SELECT ApplicationType.Id AS ApplicationTypeId FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.TypeName = Source.ApplicationTypeName) AS ApplicationTypeId, Source.UserName, 1 AS IsBuiltIn
                    EXCEPT
                    SELECT Target.ApplicationTypeId, Target.UserName, Target.IsBuiltIn)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsBuiltInExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (ApplicationTypeId, UserName, IsBuiltIn, IsBuiltInExist)
	            VALUES ((SELECT ApplicationType.Id AS ApplicationTypeId FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.TypeName = Source.ApplicationTypeName), Source.UserName, 1, 1);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            List<SqlParameter> parameterList = new List<SqlParameter>();
            foreach (BuiltInRole user in roleList.GroupBy(item => new { item.ApplicationTypeName, item.UserName }).Select(group => group.First()))
            {
                if (user.UserName != null)
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
                        "SELECT {0} AS ApplicationTypeName, {1} AS UserName",
                        UtilDataAccessLayer.Parameter(user.ApplicationTypeName, SqlDbType.NVarChar, parameterList),
                        UtilDataAccessLayer.Parameter(user.UserName, SqlDbType.NVarChar, parameterList)));
                }
            }
            if (isFirst == false)
            {
                sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
                UtilBuildTool.SqlExecute(sqlUpsert, true, parameterList);
            }
        }

        /// <summary>
        /// Insert and update table FrameworkLoginPermission, FrameworkLoginRole, FrameworkLoginUser.
        /// </summary>
        private void RunSqlBuiltIn()
        {
            RunSqlBuiltIn(out var roleList);
            RunSqlBuiltInPermission(roleList);
            RunSqlBuiltInRole(roleList);
            RunSqlBuiltInUser(roleList);
            RunSqlBuiltInRolePermission(roleList);
            RunSqlBuiltInUserRole(roleList);
        }

        public override void Run()
        {
            RunSqlTable();
            RunSqlColumn();
            RunSqlGrid();
            RunSqlComponent();
            RunSqlConfigGrid();
            RunSqlConfigColumn();
            RunSqlApplicationType();
            RunSqlApplication();
            RunSqlBuiltIn();
        }
    }
}
