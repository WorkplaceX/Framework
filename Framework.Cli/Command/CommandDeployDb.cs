namespace Framework.Cli.Command
{
    using Database.dbo;
    using Framework.DataAccessLayer;
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

        private void DeployDbExecute(string folderName, bool isFrameworkDb)
        {
            // SELECT FrameworkScript
            var task = Data.SelectAsync(Data.Query<FrameworkScript>());
            task.Wait();
            var rowList = task.Result;

            // FileNameList. For example "Framework/Framework.Cli/DeployDb/Config.sql"
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
                    string fileNameFull = UtilFramework.FolderName + fileName;
                    Console.WriteLine(string.Format("Execute {0}", fileNameFull));
                    string sql = UtilFramework.FileLoad(fileNameFull);
                    Data.ExecuteNonQueryAsync(sql, null, isFrameworkDb, commandTimeout: 0).Wait();
                    FrameworkScript row = new FrameworkScript() { FileName = fileName, Date = DateTime.UtcNow };
                    Data.InsertAsync(row).Wait();
                }
            }
        }

        /// <summary>
        /// Populate sql tables FrameworkTable, FrameworkField with assembly typeRow.
        /// </summary>
        private void Meta()
        {
            List<Type> typeRowList = UtilDalType.TypeRowList(AppCli.AssemblyList(isIncludeApp: true));
            // Table
            {
                List<FrameworkTable> rowList = new List<FrameworkTable>();
                foreach (Type typeRow in typeRowList)
                {
                    FrameworkTable table = new FrameworkTable();
                    rowList.Add(table);
                    table.TableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    if (UtilDalType.TypeRowIsTableNameSql(typeRow))
                    {
                        table.TableNameSql = UtilDalType.TypeRowToTableNameWithSchemaSql(typeRow);
                    }
                    table.IsExist = true;
                }
                UtilDalUpsert.UpsertIsExistAsync<FrameworkTable>().Wait();
                UtilDalUpsert.UpsertAsync(rowList, nameof(FrameworkTable.TableNameCSharp)).Wait();
            }

            // Field
            {
                List<FrameworkFieldBuiltIn> rowList = new List<FrameworkFieldBuiltIn>();
                foreach (Type typeRow in typeRowList)
                {
                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    var fieldList = UtilDalType.TypeRowToFieldList(typeRow);
                    foreach (var field in fieldList)
                    {
                        FrameworkFieldBuiltIn fieldBuiltIn = new FrameworkFieldBuiltIn();
                        rowList.Add(fieldBuiltIn);

                        fieldBuiltIn.TableIdName = tableNameCSharp;
                        fieldBuiltIn.FieldNameCSharp = field.PropertyInfo.Name;
                        fieldBuiltIn.FieldNameSql = field.FieldNameSql;
                        fieldBuiltIn.IsExist = true;
                    }
                    // break;
                }
                UtilDalUpsert.UpsertIsExistAsync<FrameworkFieldBuiltIn>().Wait();
                UtilDalUpsertBuiltIn.UpsertAsync<FrameworkFieldBuiltIn>(rowList, new string[] { nameof(FrameworkField.TableId), nameof(FrameworkField.FieldNameCSharp) }, "Framework", AppCli.AssemblyList()).Wait();
            }
        }

        /// <summary>
        /// Populate sql BuiltIn tables.
        /// </summary>
        private void BuiltIn()
        {
            List<Assembly> assemblyList = AppCli.AssemblyList(isIncludeApp: true, isIncludeFrameworkCli: true);
            var builtInList = AppCli.CommandDeployDbBuiltInListInternal();
            foreach (var item in builtInList)
            {
                if (item.RowList.Count > 0)
                {
                    Type typeRow = item.RowList.First().GetType();
                    UtilDalUpsertBuiltIn.UpsertAsync(typeRow, item.RowList, item.FieldNameKeyList, item.TableNameSqlReferencePrefex, assemblyList).Wait();
                }
            }
        }

        protected internal override void Execute()
        {
            CommandBuild.InitConfigWebServer(AppCli); // Copy ConnectionString from ConfigCli.json to ConfigWebServer.json. Command reads ConnectionString from ConfigWebServer.json.

            // FolderNameDeployDb
            string folderNameDeployDbFramework = UtilFramework.FolderName + "Framework/Framework.Cli/DeployDb/";
            string folderNameDeployDbApplication = UtilFramework.FolderName + "Application.Cli/DeployDb/";

            // SqlInit
            string fileNameInit = UtilFramework.FolderName + "Framework/Framework.Cli/DeployDbInit/Init.sql";
            string sqlInit = UtilFramework.FileLoad(fileNameInit);
            Data.ExecuteNonQueryAsync(sqlInit, null, isFrameworkDb: true).Wait();

            Console.WriteLine("DeployDb");
            DeployDbExecute(folderNameDeployDbFramework, isFrameworkDb: true); // Uses ConnectionString in ConfigWebServer.json
            DeployDbExecute(folderNameDeployDbApplication, isFrameworkDb: false);

            // Populate sql tables FrameworkTable, FrameworkField.
            Console.WriteLine("Update FrameworkTable, FrameworkField tables");
            Meta();

            // Populate sql BuiltIn tables.
            Console.WriteLine("Update BuiltIn tables");
            BuiltIn();

            Console.WriteLine("DeployDb successful!");
        }
    }
}
