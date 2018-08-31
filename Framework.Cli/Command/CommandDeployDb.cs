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
                    UtilDal.ExecuteNonQueryAsync(sql, null, isFrameworkDb).Wait();
                    FrameworkScript row = new FrameworkScript() { FileName = fileName, Date = DateTime.UtcNow };
                    UtilDal.InsertAsync(row).Wait();
                }
            }
        }

        public class FrameworkFieldBuiltIn : FrameworkField
        {
            public FrameworkFieldBuiltIn()
            {

            }

            public string TableIdName { get; set; }
        }

        private void Meta()
        {
            // Table
            {
                List<FrameworkTable> rowList = new List<FrameworkTable>();
                foreach (Type typeRow in AppCli.TypeRowList())
                {
                    FrameworkTable table = new FrameworkTable();
                    rowList.Add(table);
                    table.TableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    table.TableNameSql = UtilDalType.TypeRowToTableNameSql(typeRow);
                    table.IsExist = true;
                }
                UtilDalUpsert.UpsertAsync(rowList, nameof(FrameworkTable.TableNameCSharp)).Wait();
            }

            // Field
            {
                List<FrameworkFieldBuiltIn> rowList = new List<FrameworkFieldBuiltIn>();
                foreach (Type typeRow in AppCli.TypeRowList())
                {
                    string tableName = UtilDalType.TypeRowToTableNameSql(typeRow);
                    var fieldList = UtilDalType.TypeRowToFieldList(typeRow);
                    foreach (var field in fieldList)
                    {
                        FrameworkFieldBuiltIn fieldBuiltIn = new FrameworkFieldBuiltIn();
                        rowList.Add(fieldBuiltIn);

                        fieldBuiltIn.TableIdName = tableName;
                        fieldBuiltIn.FieldNameCSharp = field.PropertyInfo.Name;
                        fieldBuiltIn.FieldNameSql = field.FieldNameSql;
                        fieldBuiltIn.IsExist = true;
                    }
                }
                // UtilDalUpsertBuiltIn.UpsertAsync<FrameworkFieldBuiltIn>(rowList, new string[] { nameof(FrameworkField.TableId), nameof(FrameworkField.FieldNameCSharp) }).Wait();
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
            UtilDal.ExecuteNonQueryAsync(sqlInit).Wait();

            SqlScriptExecute(folderNameSqlScriptFramework, true);
            SqlScriptExecute(folderNameSqlScriptApplication, false);

            Meta();
        }
    }
}
