namespace Framework.Cli.Command
{
    using Database.dbo;
    using Framework.Cli.Config;
    using Framework.Dal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class CommandDeployDb : CommandBase
    {
        public CommandDeployDb(AppCli appCli)
            : base(appCli, "deployDb", "Deploy database by running sql scripts")
        {

        }

        private void SqlScriptExecute(string folderName, bool isFrameworkDb)
        {
            // SELECT FrameworkScript
            var task = UtilDal.SelectAsync(UtilDal.Query<FrameworkScript>());
            task.Wait();
            var rowList = task.Result;

            // FileNameList. For example "Framework/Framework.Cli/SqlScript/Config.sql"
            List<string> fileNameList = new List<string>();
            foreach (string fileName in UtilFramework.FileNameList(folderName, "*.sql"))
            {
                UtilFramework.Assert(fileName.ToLower().StartsWith(UtilFramework.FolderName.ToLower()));
                fileNameList.Add(fileName.Substring(UtilFramework.FolderName.Length));
            }

            fileNameList = fileNameList.OrderBy(item => item).ToList();
            foreach (string fileName in fileNameList)
            {
                if (rowList.Select(item => item.FileName.ToLower()).Contains(fileName.ToLower()) == false)
                {
                    string sql = UtilFramework.FileLoad(UtilFramework.FolderName + fileName);
                    UtilDal.ExecuteAsync(sql, isFrameworkDb).Wait();
                    FrameworkScript row = new FrameworkScript() { FileName = fileName, Date = DateTime.UtcNow };
                    UtilDal.InsertAsync(row).Wait();
                }
            }
        }

        private void Meta()
        {
            List<FrameworkTable> tableList = new List<FrameworkTable>();
            foreach (Type typeRow in AppCli.TypeRowList())
            {
                FrameworkTable table = new FrameworkTable();
                tableList.Add(table);
                string tableNameSql = null;
                SqlTableAttribute tableAttribute = (SqlTableAttribute)typeRow.GetTypeInfo().GetCustomAttribute(typeof(SqlTableAttribute));
                if (tableAttribute != null && (tableAttribute.SqlSchemaName != null || tableAttribute.SqlTableName != null))
                {
                    tableNameSql = string.Format("'[{0}].[{1}]'", tableAttribute.SqlSchemaName, tableAttribute.SqlTableName);
                }

                table.TableNameCSharp = UtilDal.TypeRowToTableNameCSharp(typeRow);
                table.TableNameSql = tableNameSql;
                table.IsExist = true;
            }
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();

            // ConnectionString
            string connectionStringFramework = configCli.ConnectionStringFramework;
            string connectionStringApplication = configCli.ConnectionStringApplication;

            // FolderNameSqlScript
            string folderNameSqlScriptFramework = UtilFramework.FolderName + "Framework/Framework.Cli/SqlScript/";
            string folderNameSqlScriptApplication = UtilFramework.FolderName + "Application.Cli/SqlScript/";

            // SqlInit
            string fileNameInit = UtilFramework.FolderName + "Framework/Framework.Cli/Sql/Init.sql";
            string sqlInit = UtilFramework.FileLoad(fileNameInit);
            UtilDal.ExecuteAsync(sqlInit).Wait();

            SqlScriptExecute(folderNameSqlScriptFramework, true);
            SqlScriptExecute(folderNameSqlScriptApplication, false);

            Meta();
        }
    }
}
