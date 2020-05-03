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
        private void Meta(DeployDbIntegrateResult result)
        {
            var assemblyList = AppCli.AssemblyList(isIncludeApp: true);
            List<Type> typeRowList = UtilDalType.TypeRowList(assemblyList);
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
                result.Add(rowList, nameof(FrameworkTable.TableNameCSharp));
            }

            // Field
            {
                List<FrameworkFieldIntegrate> rowList = new List<FrameworkFieldIntegrate>();
                foreach (Type typeRow in typeRowList)
                {
                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    var fieldList = UtilDalType.TypeRowToFieldList(typeRow);
                    foreach (var field in fieldList)
                    {
                        FrameworkFieldIntegrate fieldIntegrate = new FrameworkFieldIntegrate();
                        rowList.Add(fieldIntegrate);

                        fieldIntegrate.TableIdName = tableNameCSharp;
                        fieldIntegrate.FieldNameCSharp = field.PropertyInfo.Name;
                        fieldIntegrate.FieldNameSql = field.FieldNameSql;
                        fieldIntegrate.IsExist = true;
                    }
                }

                rowList = rowList.OrderBy(item => item.TableIdName).ThenBy(item => item.FieldNameCSharp).ToList();

                result.Add(rowList, nameof(FrameworkField.TableId), nameof(FrameworkField.FieldNameCSharp));
            }
        }

        /// <summary>
        /// Populate sql Integrate tables.
        /// </summary>
        private void Integrate()
        {
            var generateIntegrateResult = AppCli.CommandGenerateIntegrateInternal();
            var deployDbResult = new DeployDbIntegrateResult(generateIntegrateResult);
            List<Assembly> assemblyList = AppCli.AssemblyList(isIncludeApp: true, isIncludeFrameworkCli: true);

            // Populate sql tables FrameworkTable, FrameworkField.
            UtilCli.ConsoleWriteLineColor("Update FrameworkTable, FrameworkField tables", ConsoleColor.Green);
            Meta(deployDbResult);
            UtilDalUpsertIntegrate.UpsertAsync(deployDbResult.Result, assemblyList).Wait();

            // Populate sql Integrate tables.
            UtilCli.ConsoleWriteLineColor("Update Integrate tables", ConsoleColor.Green);
            AppCli.CommandDeployDbIntegrateInternal(deployDbResult);
            UtilDalUpsertIntegrate.UpsertAsync(deployDbResult.Result, assemblyList).Wait();
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();

            if (optionSilent.OptionGet() == false && configCli.EnvironmentNameGet() != "DEV")
            {
                if (UtilCli.ConsoleReadYesNo(string.Format("Deploy to {0} database?", configCli.EnvironmentName)) == false)
                {
                    return;
                }
            }

            if (optionDrop.OptionGet())
            {
                // FolderNameDeployDb
                string folderNameDeployDbFramework = UtilFramework.FolderName + "Framework/Framework.Cli/DeployDb/";
                string folderNameDeployDbApplication = UtilFramework.FolderName + "Application.Cli/DeployDb/";

                Console.WriteLine("DeployDbDrop");
                DeployDbDropExecute(folderNameDeployDbApplication, isFrameworkDb: false);
                DeployDbDropExecute(folderNameDeployDbFramework, isFrameworkDb: true); // Uses ConnectionString in ConfigServer.json

                UtilCli.ConsoleWriteLineColor("DeployDb drop successful!", ConsoleColor.Green);
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

                Integrate();

                UtilCli.ConsoleWriteLineColor("DeployDb successful!", ConsoleColor.Green);
            }
        }
    }
}
