namespace Framework.Cli.Command
{
    using Database.dbo;
    using Framework.Cli.Config;
    using Framework.DataAccessLayer;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using static Framework.Cli.AppCli;

    /// <summary>
    /// Cli deployDb command.
    /// </summary>
    internal class CommandDeployDb : CommandBase
    {
        public CommandDeployDb(AppCli appCli)
            : base(appCli, "deployDb", "Deploy database by running sql scripts")
        {

        }

        private CommandOption optionDrop;

        private CommandOption optionSilent;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionDrop = configuration.Option("-d|--drop", "Drop sql tables and views.", CommandOptionType.NoValue);
            optionSilent = configuration.Option("-s|--silent", "No command line user interaction.", CommandOptionType.NoValue);
        }

        /// <summary>
        /// Returns true if fileName ends with Drop.sql
        /// </summary>
        private bool IsFileNameDrop(string fileName)
        {
            return fileName.ToLower().EndsWith("drop.sql");
        }

        /// <summary>
        /// Execute (*.sql) scripts.
        /// </summary>
        private void DeployDbExecute(string folderName, bool isFrameworkDb)
        {
            // SELECT FrameworkDeployDb
            var rowList = Data.Query<FrameworkDeployDb>().QueryExecute();

            // FileNameList. For example "Framework/Framework.Cli/DeployDb/Config.sql"
            List<string> fileNameList = new List<string>();
            foreach (string fileName in UtilFramework.FileNameList(folderName, "*.sql"))
            {
                UtilFramework.Assert(fileName.ToLower().StartsWith(UtilFramework.FolderName.ToLower()));
                if (!IsFileNameDrop(fileName))
                {
                    fileNameList.Add(fileName.Substring(UtilFramework.FolderName.Length));
                }
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
                    FrameworkDeployDb row = new FrameworkDeployDb() { FileName = fileName, Date = DateTime.UtcNow };
                    Data.InsertAsync(row).Wait();
                }
            }
        }

        /// <summary>
        /// Execute (*Drop.sql) scripts.
        /// </summary>
        private void DeployDbDropExecute(string folderName, bool isFrameworkDb)
        {
            // FileNameList. For example "Framework/Framework.Cli/DeployDb/Config.sql"
            List<string> fileNameList = new List<string>();
            foreach (string fileName in UtilFramework.FileNameList(folderName, "*.sql"))
            {
                UtilFramework.Assert(fileName.ToLower().StartsWith(UtilFramework.FolderName.ToLower()));
                if (IsFileNameDrop(fileName))
                {
                    fileNameList.Add(fileName.Substring(UtilFramework.FolderName.Length));
                }
            }

            fileNameList = fileNameList.OrderByDescending(item => item).ToList(); // Reverse
            foreach (string fileName in fileNameList)
            {
                string fileNameFull = UtilFramework.FolderName + fileName;
                Console.WriteLine(string.Format("Execute {0}", fileNameFull));
                string sql = UtilFramework.FileLoad(fileNameFull);
                try
                {
                    Data.ExecuteNonQueryAsync(sql, null, isFrameworkDb, commandTimeout: 0).Wait();
                }
                catch
                {
                    UtilCli.ConsoleWriteLineColor("Already dropped or drop failed!", ConsoleColor.DarkYellow);
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
                }

                rowList = rowList.OrderBy(item => item.TableIdName).ThenBy(item => item.FieldNameCSharp).ToList();

                var upsertItem = UtilDalUpsertBuiltIn.UpsertItem.Create(rowList, new string[] { nameof(FrameworkField.TableId), nameof(FrameworkField.FieldNameCSharp) }, "Framework");
                UtilDalUpsertBuiltIn.UpsertAsync(upsertItem, AppCli.AssemblyList()).Wait();
            }
        }

        /// <summary>
        /// Populate sql BuiltIn tables.
        /// </summary>
        private void BuiltIn()
        {
            var builtInList = AppCli.CommandDeployDbBuiltInListInternal();
            List<Assembly> assemblyList = AppCli.AssemblyList(isIncludeApp: true, isIncludeFrameworkCli: true);

            UtilDalUpsertBuiltIn.UpsertAsync(builtInList.Result, assemblyList).Wait();
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();

            if (optionSilent.Value() != "on")
            {
                if (UtilCli.ConsoleReadYesNo(string.Format("Deploy to {0} database?", configCli.EnvironmentName)) == false)
                {
                    return;
                }
            }

            if (UtilCli.OptionGet(optionDrop))
            {
                // FolderNameDeployDb
                string folderNameDeployDbFramework = UtilFramework.FolderName + "Framework/Framework.Cli/DeployDb/";
                string folderNameDeployDbApplication = UtilFramework.FolderName + "Application.Cli/DeployDb/";

                Console.WriteLine("DeployDbDrop");
                DeployDbDropExecute(folderNameDeployDbApplication, isFrameworkDb: false);
                DeployDbDropExecute(folderNameDeployDbFramework, isFrameworkDb: true); // Uses ConnectionString in ConfigServer.json

                Console.WriteLine("DeployDb drop successful!");
            }
            else
            {
                // FolderNameDeployDb
                string folderNameDeployDbFramework = UtilFramework.FolderName + "Framework/Framework.Cli/DeployDb/";
                string folderNameDeployDbApplication = UtilFramework.FolderName + "Application.Cli/DeployDb/";

                // SqlInit
                string fileNameInit = UtilFramework.FolderName + "Framework/Framework.Cli/DeployDbInit/Init.sql";
                string sqlInit = UtilFramework.FileLoad(fileNameInit);
                Data.ExecuteNonQueryAsync(sqlInit, null, isFrameworkDb: true).Wait();

                UtilCli.ConsoleWriteLineColor("DeployDb run (*.sql) scripts", ConsoleColor.Green);
                DeployDbExecute(folderNameDeployDbFramework, isFrameworkDb: true); // Uses ConnectionString in ConfigServer.json
                DeployDbExecute(folderNameDeployDbApplication, isFrameworkDb: false);

                // Populate sql tables FrameworkTable, FrameworkField.
                UtilCli.ConsoleWriteLineColor("Update FrameworkTable, FrameworkField tables", ConsoleColor.Green);
                Meta();

                // Populate sql BuiltIn tables.
                UtilCli.ConsoleWriteLineColor("Update BuiltIn tables", ConsoleColor.Green);
                BuiltIn();

                Console.WriteLine("DeployDb successful!");
            }
        }
    }
}
