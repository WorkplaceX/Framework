namespace Framework.BuildTool
{
    using Framework.Application;
    using Framework.BuildTool.DataAccessLayer;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public class CommandRunSqlColumn : Command
    {
        public CommandRunSqlColumn(AppBuildTool appBuildTool) 
            : base("runSqlColumn", "Run sql table FrameworkColumn update for all in source code defined columns.")
        {
            this.AppBuildTool = appBuildTool;
        }

        public readonly AppBuildTool AppBuildTool;

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
                    SELECT Source.TableNameSql, Source.FieldNameSql, Source.FieldNameCsharp
                    EXCEPT
                    SELECT Target.TableNameSql, Target.FieldNameSql, Target.FieldNameCsharp)
            WHEN MATCHED THEN
	            UPDATE SET	Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (TableNameSql, FieldNameSql, FieldNameCsharp, IsExist)
	            VALUES (Source.TableNameSql, Source.FieldNameSql, Source.FieldNameCsharp, 1);
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
                    sqlSelect.Append(string.Format("(SELECT '{0}' AS TableNameSql, CASE WHEN '{1}' = '' THEN NULL ELSE '{1}' END AS FieldNameSql, '{2}' AS FieldNameCsharp)", column.TableNameSql, column.FieldNameSql, column.FieldNameCSharp));
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
            RunSqlColumn(Server.ConnectionManager.ConnectionString);
        }
    }
}
