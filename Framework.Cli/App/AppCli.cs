﻿namespace Framework.Cli
{
    using Database.dbo;
    using DatabaseIntegrate.dbo;
    using Framework.Cli.Command;
    using Framework.Cli.Config;
    using Framework.Cli.Generate;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Command line interface application.
    /// </summary>
    public class AppCli
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assemblyApplicationDatabase">Register Application.Database.dll</param>
        /// <param name="assemblyApplication">Register Application.dll</param>
        public AppCli(Assembly assemblyApplicationDatabase, Assembly assemblyApplication)
        {
            this.AssemblyApplicationDatabase = assemblyApplicationDatabase;
            this.AssemblyApplication = assemblyApplication;

            this.commandLineApplication = new CommandLineApplication();

            RegisterCommand();
            RegisterCommandInit();
        }

        /// <summary>
        /// Constructor for standalone mode.
        /// </summary>
        internal AppCli() 
        {
            IsStandaloneMode = true;

            this.commandLineApplication = new CommandLineApplication();

            RegisterCommand();
            RegisterCommandInit();
        }

        /// <summary>
        /// Gets IsStandaloneMode. If true, cli has been started for example with npx to create a new project.
        /// </summary>
        internal bool IsStandaloneMode;

        /// <summary>
        /// Gets "Application.Database" assembly. This assembly hosts from database generated rows.
        /// </summary>
        public readonly Assembly AssemblyApplicationDatabase;

        /// <summary>
        /// Gets "Application" assembly. This assembly hosts business logic.
        /// </summary>
        public readonly Assembly AssemblyApplication;

        /// <summary>
        /// Returns list of AppType declared in AssemblyApplication.
        /// </summary>
        /// <param name="isExternalGit">If true, AppType in ExternalGit are included.</param>
        public string[] AppTypeNameList(bool isExternalGit)
        {
            var appJsonTypeList = AssemblyApplication.GetTypes().Where(item => item.IsSubclassOf(typeof(AppJson))).ToList();

            if (!isExternalGit)
            {
                // Exclude ExternalGit
                appJsonTypeList = appJsonTypeList.Where(item => item.GetCustomAttributes<UtilFramework.ExternalGitAttribute>().Count() == 0).ToList();
            }

            return appJsonTypeList.Select(item => item.FullName).ToArray();
        }

        /// <summary>
        /// Gets "Framework" assembly.
        /// </summary>
        internal Assembly AssemblyFramework
        {
            get
            {
                return UtilFramework.AssemblyFramework;
            }
        }

        /// <summary>
        /// Gets "Framework.Cli" assembly.
        /// </summary>
        internal Assembly AssemblyFrameworkCli
        {
            get
            {
                return typeof(AppCli).Assembly;
            }
        }

        /// <summary>
        /// Gets "Application.Cli" assembly.
        /// </summary>
        internal Assembly AssemblyApplicationCli
        {
            get
            {
                Assembly result = GetType().Assembly;
                UtilFramework.Assert(result != AssemblyFrameworkCli);
                return result;
            }
        }

        /// <summary>
        /// Returns Framework, Application.Database, Application and Framework.Cli assembly when running in cli mode.
        /// </summary>
        /// <param name="isIncludeApp">If true, Application assembly (with App class and derived custom logic) is included.</param>
        /// <param name="isIncludeFrameworkCli">If true, Framework.Cli assembly is included</param>
        /// <returns>List of assemblies.</returns>
        internal List<Assembly> AssemblyList(bool isIncludeApp = false, bool isIncludeFrameworkCli = false)
        {
            List<Assembly> result = new List<Assembly>
            {
                AssemblyFramework,
                AssemblyApplicationDatabase
            };
            if (isIncludeApp)
            {
                result.Add(AssemblyApplication);
            }
            if (isIncludeFrameworkCli)
            {
                result.Add(AssemblyFrameworkCli);
            }

            // No assembly double in result!
            int count = result.Count();
            result = result.Distinct().ToList();
            UtilFramework.Assert(count == result.Count);

            return result;
        }

        private readonly CommandLineApplication commandLineApplication;

        internal readonly List<CommandBase> CommandList = new List<CommandBase>();

        private void Title(string[] args)
        {
            if (args.Length == 0)
            {
                // See also: http://patorjk.com/software/taag/#p=display&f=Ivrit&t=WorkplaceX%20CLI
                string text = @"
                 __        __         _          _               __  __   ____ _     ___ 
                 \ \      / /__  _ __| | ___ __ | | __ _  ___ ___\ \/ /  / ___| |   |_ _|
                  \ \ /\ / / _ \| '__| |/ / '_ \| |/ _` |/ __/ _ \\  /  | |   | |    | | 
                   \ V  V / (_) | |  |   <| |_) | | (_| | (_|  __//  \  | |___| |___ | | 
                    \_/\_/ \___/|_|  |_|\_\ .__/|_|\__,_|\___\___/_/\_\  \____|_____|___|
                                          |_|                                            ";
                text = text.Replace(Environment.NewLine + "                 ", Environment.NewLine);
                text = text.Substring(Environment.NewLine.Length);
                var color = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(text);
                }
                finally
                {
                    Console.ForegroundColor = color;
                }
                commandLineApplication.Execute("-h"); // Show list of available commands.
            }
        }

        /// <summary>
        /// Override to register new commands.
        /// </summary>
        internal virtual void RegisterCommand()
        {
            if (!IsStandaloneMode)
            {
                new CommandConfig(this);
                new CommandGenerate(this);
                new CommandBuild(this);
                new CommandStart(this);
                new CommandDeploy(this);
                new CommandDeployDb(this);
                new CommandEnvironment(this);
                new CommandTest(this);
                if (UtilExternalGit.IsExternalGit)
                {
                    new CommandExternalGit(this);
                }
            }
            else
            {
                new CommandNewProject(this);
            }
        }

        /// <summary>
        /// Override this method to define default cli configuration. This method is called if file ConfigCli.json does not yet exist.
        /// </summary>
        protected virtual internal void InitConfigCli(ConfigCli configCli)
        {

        }

        private void RegisterCommandInit()
        {
            foreach (CommandBase command in CommandList)
            {
                commandLineApplication.Command(command.Name, (configuration) =>
                {
                    configuration.Description = command.Description;
                    command.Configuration = configuration;
                    command.Register(configuration);
                    configuration.OnExecute(() =>
                    {
                        UtilCliInternal.ConsoleWriteLineColor($"Execute Framework Command Cli ({command.Name})", ConsoleColor.Green);
                        command.Execute();
                        return 0;
                    });

                    configuration.HelpOption("-h | --help"); // Command help (to show arguments and options)
                });
            }
            commandLineApplication.HelpOption("-h | --help"); // Command line interface help (to show commands)
            commandLineApplication.VersionOption("-v | --version", string.Format("\b" + "WorkplaceX={0};", UtilFramework.Version)); // Backspace because by default CommandLineApplication adds a space.
        }

        /// <summary>
        /// Overwrite this method to filter out only specific application tables and fields for which to generate code. For example only tables starting with "Explorer".
        /// </summary>
        protected virtual internal void CommandGenerateFilter(GenerateFilterArgs args, GenerateFilterResult result)
        {

        }

        /// <summary>
        /// Sql field.
        /// </summary>
        public class GenerateFilterFieldSql
        {
            internal GenerateFilterFieldSql(MetaSqlSchema metaSqlSchema)
            {
                MetaSqlSchema = metaSqlSchema;
            }

            internal readonly MetaSqlSchema MetaSqlSchema;

            /// <summary>
            /// Gets SchemaName. This is the database sql schema name.
            /// </summary>
            public string SchemaName
            {
                get
                {
                    return MetaSqlSchema.SchemaName;
                }
            }

            /// <summary>
            /// Gets TableName. This is the database sql table name.
            /// </summary>
            public string TableName
            {
                get
                {
                    return MetaSqlSchema.TableName;
                }
            }

            /// <summary>
            /// Gets FieldName. This is the sql field name.
            /// </summary>
            public string FieldName
            {
                get
                {
                    return MetaSqlSchema.FieldName;
                }
            }
        }

        public class GenerateFilterArgs
        {
            internal GenerateFilterArgs(MetaSqlSchema[] list)
            {
                FieldSqlList = new List<GenerateFilterFieldSql>();
                TypeRowCalculatedList = new List<Type>();

                foreach (var item in list)
                {
                    FieldSqlList.Add(new GenerateFilterFieldSql(item));
                }
            }

            /// <summary>
            /// Gets FieldSqlList. This is the database sql field list to generate CSharp code.
            /// </summary>
            public List<GenerateFilterFieldSql> FieldSqlList { get; private set; }

            /// <summary>
            /// Gets TypeRowCalculatedList. Used to generate (FrameworkConfigGrid and FrameworkConfigField) Integrate CSharp code from database for calculated rows.
            /// </summary>
            public List<Type> TypeRowCalculatedList { get; private set; }
        }

        public class GenerateFilterResult
        {
            /// <summary>
            /// Gets or sets FieldSqlList. Used to generate CSharp code from database meta schema.
            /// </summary>
            public List<GenerateFilterFieldSql> FieldSqlList { get; set; }

            /// <summary>
            /// Gets or sets TypeRowCalculatedList. Used to generate (FrameworkConfigGrid and FrameworkConfigField) Integrate CSharp code from database for calculated rows.
            /// </summary>
            public List<Type> TypeRowCalculatedList { get; set; }

            internal MetaSqlSchema[] List
            {
                get
                {
                    return FieldSqlList?.Select(item => item.MetaSqlSchema).ToArray();
                }
            }
        }

        /// <summary>
        /// Copy ConfigCli.json to ConfigServer.json and validate ConnectionString exists.
        /// </summary>
        private void CopyConfigCliToConfigServer(bool isWarning)
        {
            ConfigCli.CopyConfigCliToConfigServer();
            var configCli = ConfigCli.Load();
            var environment = configCli.EnvironmentGet();
            if (UtilFramework.StringNull(environment.ConnectionStringApplication) == null || UtilFramework.StringNull(environment.ConnectionStringFramework) == null)
            {
                if (isWarning)
                {
                    UtilCliInternal.ConsoleWriteLineColor(string.Format("Warning! No ConnectionString for {0}! Set it with command cli config connectionString.", environment.EnvironmentName), ConsoleColor.Yellow); // Warning
                }
            }
        }

        /// <summary>
        /// Run command line interface.
        /// </summary>
        public void Run(string[] args)
        {
            Title(args);
            try
            {
                if (!IsStandaloneMode)
                {
                    ConfigCli.Init(this);
                    var configCli = ConfigCli.Load();
                    ConfigCli.Save(configCli); // Reset ConfigCli.json
                    ConfigCli.CopyConfigCliToConfigServer();
                    CommandEnvironment.ConsoleWriteLineCurrentEnvironment(configCli);
                    CopyConfigCliToConfigServer(isWarning: false); // Copy from ConfigCli.json to ConfigServer.json
                }
                commandLineApplication.Execute(args);
                if (!IsStandaloneMode)
                {
                    CopyConfigCliToConfigServer(isWarning: true); // Copy new values from ConfigCli.json to ConfigServer.json
                }
            }
            catch (Exception exception) // For example unrecognized option
            {
                UtilCliInternal.ConsoleWriteLineError(exception);
                Environment.ExitCode = 1; // echo %errorlevel%
            }
        }

        /// <summary>
        /// Override this method to add Integrate data rows to list. Used by cli deployDb command to deploy Integrate rows to database.
        /// </summary>
        protected virtual internal void CommandDeployDbIntegrate(DeployDbIntegrateResult result)
        {

        }

        /// <summary>
        /// Use reflection to get value from Application assemblies.
        /// </summary>
        internal List<T> CommandDeployDbIntegrateInternalRowListGet<T>(string typeName) where T : Row
        {
            var assembly = AssemblyApplicationCli;
            if (!typeName.Contains("AppCli"))
            {
                assembly = AssemblyApplicationDatabase;
            }
            var typeCli = assembly.GetType(typeName);
            UtilFramework.Assert(typeCli != null, string.Format("Type not found! ({0})", typeName));
            PropertyInfo propertyInfo = typeCli.GetProperty("RowList");
            var rowList = (List<T>)propertyInfo.GetValue(null);
            return rowList;
        }

        /// <summary>
        /// Returns Integrate rows to deploy to sql database.
        /// </summary>
        internal void CommandDeployDbIntegrateInternal(DeployDbIntegrateResult result)
        {
            // FrameworkTable
            var tableNameCSharpList = result.Result[0].RowList.Select(item => (((FrameworkTable)item).TableNameCSharp)).ToArray();

            // FrameworkConfigGridIntegrate
            {
                var rowList = FrameworkConfigGridIntegrateFramework.RowList;

                // Read FrameworkConfigGridIntegrate.RowListList from Application.Cli project.
                string nameCli = "DatabaseIntegrate.dbo.FrameworkConfigGridIntegrateAppCli"; // See also method GenerateCSharpTableNameClass();
                var typeCli = AssemblyApplicationCli.GetType(nameCli);
                UtilFramework.Assert(typeCli != null, string.Format("Type not found! See also method GenerateCSharpTableNameClass(); ({0})", nameCli));
                PropertyInfo propertyInfo = typeCli.GetProperty(nameof(FrameworkConfigGridIntegrateFramework.RowList));
                var rowApplicationCliList = (List<FrameworkConfigGridIntegrate>)propertyInfo.GetValue(null);
                // Filter out records on FrameworkConfigGrid which do not exist on FrameworkTable (because class row has been removed in CSharp code)
                rowApplicationCliList = rowApplicationCliList.Where(item => tableNameCSharpList.Contains(item.TableNameCSharp)).ToList();
                rowList.AddRange(rowApplicationCliList);

                // Collect rowList from ExternalGit FrameworkConfigGridIntegrateAppCli (ConfigGrid).
                UtilExternalGit.CommandDeployDbIntegrate(this, rowList);

                result.Add(rowList);
            }

            // FrameworkConfigFieldIntegrate
            {
                var rowList = FrameworkConfigFieldIntegrateFrameworkCli.RowList;

                // Read FrameworkConfigFieldIntegrateCli.List from Application.Cli project.
                string nameCli = "DatabaseIntegrate.dbo.FrameworkConfigFieldIntegrateAppCli"; // See also method GenerateCSharpTableNameClass();
                var typeCli = AssemblyApplicationCli.GetType(nameCli);
                UtilFramework.Assert(typeCli != null, string.Format("Type not found! See also method GenerateCSharpTableNameClass(); ({0})", nameCli));
                PropertyInfo propertyInfo = typeCli.GetProperty(nameof(FrameworkConfigFieldIntegrateFrameworkCli.RowList));
                var rowApplicationCliList = (List<FrameworkConfigFieldIntegrate>)propertyInfo.GetValue(null);
                // Filter out records on FrameworkConfigField which do not exist on FrameworkTable (because class row has been removed in CSharp code)
                rowApplicationCliList = rowApplicationCliList.Where(item => tableNameCSharpList.Contains(item.TableNameCSharp)).ToList();
                rowList.AddRange(rowApplicationCliList);

                // Collect rowList from ExternalGit FrameworkConfigFieldIntegrateAppCli (ConfigField).
                UtilExternalGit.CommandDeployDbIntegrate(this, rowList);

                result.Add(rowList);
            }

            // FrameworkLanguage
            {
                var rowLanguageList = new List<FrameworkLanguageIntegrate>();
                var rowItemList = new List<FrameworkLanguageItem>();
                var rowTextList = new List<FrameworkLanguageTextIntegrate>();
                foreach (var appTypeName in AppTypeNameList(isExternalGit: true))
                {
                    var rowLanguageLocalList = CommandDeployDbIntegrateInternalRowListGet<FrameworkLanguageIntegrate>("DatabaseIntegrate.dbo.FrameworkLanguageIntegrateApp" + appTypeName.Replace(".", ""));
                    rowLanguageList.AddRange(rowLanguageLocalList);
                    var rowItemLocalList = CommandDeployDbIntegrateInternalRowListGet<FrameworkLanguageItem>("DatabaseIntegrate.dbo.FrameworkLanguageItemAppCli" + appTypeName.Replace(".", ""));
                    rowItemList.AddRange(rowItemLocalList);
                    var rowTextLocalList = CommandDeployDbIntegrateInternalRowListGet<FrameworkLanguageTextIntegrate>("DatabaseIntegrate.dbo.FrameworkLanguageTextIntegrateAppCli" + appTypeName.Replace(".", ""));
                    rowTextList.AddRange(rowTextLocalList);
                }
                result.Add(rowLanguageList);
                result.Add(rowItemList);
                result.Add(rowTextList);
            }

            // Add application (custom) Integrate data rows to deploy to database
            CommandDeployDbIntegrate(result);

            // Call method CommandDeployDbIntegrate(); on ExternalGit AppCli.
            UtilExternalGit.CommandDeployDbIntegrate(this, result);
        }

        public class DeployDbIntegrateResult
        {
            internal DeployDbIntegrateResult(GenerateIntegrateResult generateIntegrateResult)
            {
                this.GenerateIntegrateResult = generateIntegrateResult;
                this.Result = new List<UtilDalUpsertIntegrate.UpsertItem>();
            }

            internal readonly GenerateIntegrateResult GenerateIntegrateResult;

            internal List<UtilDalUpsertIntegrate.UpsertItem> Result;

            public void Add<TRow>(List<TRow> rowList) where TRow : Row
            {
                Type typeRow = typeof(TRow);

                // Reference from GenerateIntegrate to DeployDbIntegrate
                var referenceFilterList = GenerateIntegrateResult.ResultReference.Where(item => item.TypeRowIntegrate == typeof(TRow)).ToList();

                // Make sure reference tables are deployed.
                foreach (var item in referenceFilterList)
                {
                    if (item.TypeRowReferenceIntegrate != typeRow) // Exclude hierarchical reference
                    {
                        int referenceCount = Result.Count(itemLocal => itemLocal.TypeRow == item.TypeRowReferenceIntegrate || itemLocal.TypeRow == item.TypeRowReference);
                        UtilFramework.Assert(referenceCount > 0, string.Format("Reference table not yet deployed! ({0})", UtilDalType.TypeRowToTableNameCSharp(item.TypeRowReferenceIntegrate)));
                    }
                }

                // Key from GenerateIntegrate
                var typeRowUnderlying = typeRow;
                string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                if (tableNameCSharp.EndsWith("Integrate"))
                {
                    string tableNameCSharpUnderlying = tableNameCSharp.Substring(0, tableNameCSharp.Length - "Integrate".Length);
                    typeRowUnderlying = GenerateIntegrateResult.TableNameCSharpList.SingleOrDefault(item => item.Value == tableNameCSharpUnderlying).Key;
                    UtilFramework.Assert(typeRowUnderlying != null, string.Format("Use underlying sql table! ({0})", tableNameCSharp));
                }
                UtilFramework.Assert(GenerateIntegrateResult.ResultKey.ContainsKey(typeRowUnderlying), string.Format("TypRow no unique key defined! See also method GenerateIntegrateResult.AddKey(); ({0})", tableNameCSharp));
                var fieldNameCSharpKeyList = GenerateIntegrateResult.ResultKey[typeRowUnderlying];

                // Result
                var result = UtilDalUpsertIntegrate.UpsertItem.Create(rowList, fieldNameCSharpKeyList, referenceFilterList);

                // Make sure table is not already added.
                if (Result.Count > 0 && Result[^1].TypeRow != result.TypeRow) // Do not test ist previous is identical (because of hierarchical reference calling this method multiple times).
                {
                    UtilFramework.Assert(Result.Count(item => item.TypeRow == result.TypeRow) == 0, string.Format("Table already added! ({0})", UtilDalType.TypeRowToTableNameCSharp(result.TypeRow)));
                }

                Result.Add(result);
            }

            /// <summary>
            /// Add hierarchical list with Id and ParentId column.
            /// </summary>
            public void Add<TRow>(List<TRow> rowList, Func<TRow, object> idSelector, Func<TRow, object> parentIdSelector, Func<TRow, object> sortSelector) where TRow : Row
            {
                List<TRow> rowLevelList = null;
                while (OrderByHierarchical(rowList, idSelector, parentIdSelector, sortSelector, ref rowLevelList)) // Step through all levels.
                {
                    Add<TRow>(rowLevelList);
                }
            }

            private static bool OrderByHierarchical<TRow>(List<TRow> rowAllList, Func<TRow, object> idSelector, Func<TRow, object> parentIdSelector, Func<TRow, object> sortSelector, ref List<TRow> rowLevelList) where TRow : Row
            {
                if (rowLevelList == null)
                {
                    rowLevelList = rowAllList.Where(item => parentIdSelector(item) == null).ToList();
                }
                else
                {
                    var idList = rowLevelList.Select(item => idSelector(item)).ToList();
                    rowLevelList = rowAllList.Where(item => idList.Contains(parentIdSelector(item))).OrderBy(item => sortSelector(item)).ToList();
                }
                return rowLevelList.Count() != 0;
            }
        }

        /// <summary>
        /// Returns Integrate rows to generate CSharp code.
        /// </summary>
        /// <param name="isDeployDb">Method is called from command cli generate or cli deployDb.</param>
        /// <param name="tableNameCSharpApplicationFilterList">TableNameCSharp defined in method AppCli.CommandGenerateFilter();</param>
        internal GenerateIntegrateResult CommandGenerateIntegrateInternal(bool isDeployDb, List<string> tableNameCSharpApplicationFilterList)
        {
            var result = new GenerateIntegrateResult(AssemblyList(true, true));

            result.AddKey<FrameworkTable>(nameof(FrameworkTable.TableNameCSharp));

            // Do not generate CSharp code for table FrameworkTable and FrameworkField. Add reference for deoplyDb.
            result.AddKey<FrameworkField>(nameof(FrameworkField.TableId), nameof(FrameworkField.FieldNameCSharp));
            result.AddReference<FrameworkField, FrameworkTable>(nameof(FrameworkFieldIntegrate.TableId));

            var tableNameCSharpFrameworkList = UtilDalType.TableNameCSharpList(AssemblyFramework); // TableNameCSharp declared in Framework assembly.
            var tableNameCSharpApplicationList = UtilDalType.TableNameCSharpList(AssemblyApplication, AssemblyApplicationDatabase); // TableNameCSharp declared in Application assembly.
            var fieldNameCSharpFrameworkList = UtilDalType.FieldNameCSharpList(AssemblyFramework); // FieldNameCSharp declared in Framework assembly
            var fieldNameCSharpApplicationList = UtilDalType.FieldNameCSharpList(AssemblyApplication, AssemblyApplicationDatabase); // FieldNameCSharp declared in Framework assembly

            // Filter out tables defined in method AppCli.CommandGenerateFilter();
            if (tableNameCSharpApplicationFilterList != null)
            {
                tableNameCSharpApplicationList = tableNameCSharpApplicationList.Where(item => tableNameCSharpApplicationFilterList.Contains(item.Value)).ToDictionary(item => item.Key, item => item.Value);
                fieldNameCSharpApplicationList = fieldNameCSharpApplicationList.Where(item => tableNameCSharpApplicationFilterList.Contains(item.TableNameCSharp)).ToList();
            }

            // Prevent build error "An expression tree may not contain a tuple literal".
            var fieldNameCSharpFrameworkNoTupleList = fieldNameCSharpFrameworkList.Select(item => item.TableNameCSharp + "/" + item.FieldNameCSharp);
            var fieldNameCSharpApplicationNoTupleList = fieldNameCSharpApplicationList.Select(item => item.TableNameCSharp + "/" + item.FieldNameCSharp);

            // FrameworkConfigGridIntegrate
            {
                var rowList = Data.Query<FrameworkConfigGridIntegrate>();
                
                // Framework (.\wpx.cmd generate -f)
                {
                    var rowFilterList = rowList.Where(item => tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter Framework.
                    rowFilterList = rowFilterList.OrderBy(item => item.IdName);
                    result.Add(
                        isFrameworkDb: true,
                        isApplication: true, // Make FrameworkConfigGrid available in application insted of cli. Enum can be used for example to get strongly typed ConfigName.
                        query: rowFilterList
                    );
                }
                // Application (.\wpx.cmd generate)
                {
                    var rowFilterList = rowList.Where(item => !tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp) && tableNameCSharpApplicationList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter (not Framework and Application).
                    rowFilterList = rowFilterList.OrderBy(item => item.IdName);
                    result.Add(
                        isFrameworkDb: false,
                        isApplication: false,
                        query: rowFilterList
                    );
                }
                result.AddKey<FrameworkConfigGrid>(nameof(FrameworkConfigGrid.TableId), nameof(FrameworkConfigGrid.ConfigName));
                result.AddReference<FrameworkConfigGrid, FrameworkTable>(nameof(FrameworkConfigGrid.TableId));
            }

            // FrameworkConfigFieldIntegrate
            {
                var rowList = Data.Query<FrameworkConfigFieldIntegrate>();
                // Framework (.\wpx.cmd generate -f)
                {
                    var rowFilterList = rowList.Where(item => tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter FrameworkDb.
                    rowFilterList = rowList.Where(item => fieldNameCSharpFrameworkNoTupleList.Contains(item.TableNameCSharp + "/" + item.FieldNameCSharp)); // Filter FieldNameCSharp declared in Framework assembly.
                    rowFilterList = rowFilterList.OrderBy(item => item.FieldIdName);
                    result.Add(
                        isFrameworkDb: true,
                        isApplication: false,
                        query: rowFilterList
                    );
                }
                // Application (.\wpx.cmd generate)
                {
                    var rowFilterList = rowList.Where(item => !tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp) && tableNameCSharpApplicationList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter (not Framework and Application).
                    rowFilterList = rowList.Where(item => fieldNameCSharpApplicationNoTupleList.Contains(item.TableNameCSharp + "/" + item.FieldNameCSharp)); // Filter FieldNameCSharp declared in Application assembly.
                    rowFilterList = rowFilterList.OrderBy(item => item.FieldIdName);
                    result.Add(
                        isFrameworkDb: false,
                        isApplication: false,
                        query: rowFilterList
                    );
                }
                result.AddKey<FrameworkConfigField>(nameof(FrameworkConfigField.ConfigGridId), nameof(FrameworkConfigField.FieldId), nameof(FrameworkConfigField.InstanceName));
                result.AddReference<FrameworkConfigField, FrameworkConfigGrid>(nameof(FrameworkConfigField.ConfigGridId));
                result.AddReference<FrameworkConfigField, FrameworkField>(nameof(FrameworkConfigField.FieldId));
            }

            // FrameworkLanguage
            {
                // See also method CommandDeployDbIntegrateInternal();
                var appTypeNameList = AppTypeNameList(isExternalGit: false);
                foreach (var appTypeName in appTypeNameList)
                {
                    var rowLanguageList = Data.Query<FrameworkLanguageIntegrate>().Where(item => item.AppTypeName == appTypeName).OrderBy(item => item.AppTypeName).ThenBy(item => item.Name);
                    result.Add(
                        isFrameworkDb: false,
                        isApplication: true,
                        query: rowLanguageList,
                        appTypeName: appTypeName);

                    var rowLanguageItemList = Data.Query<FrameworkLanguageItem>().Where(item => appTypeNameList.Contains(item.AppTypeName)).OrderBy(item => item.AppTypeName).ThenBy(item => item.Name);
                    result.Add(
                        isFrameworkDb: false,
                        isApplication: false,
                        query: rowLanguageItemList,
                        appTypeName: appTypeName);

                    var rowLanguageTextList = Data.Query<FrameworkLanguageTextIntegrate>().Where(item => appTypeNameList.Contains(item.AppTypeName)).OrderBy(item => item.AppTypeName).ThenBy(item => item.LanguageIdName).ThenBy(item => item.ItemIdName);
                    result.Add(
                        isFrameworkDb: false,
                        isApplication: false,
                        query: rowLanguageTextList,
                        appTypeName: appTypeName);
                }
                result.AddKey<FrameworkLanguage>(nameof(FrameworkLanguage.AppTypeName), nameof(FrameworkLanguage.Name));
                result.AddKey<FrameworkLanguageItem>(nameof(FrameworkLanguageItem.AppTypeName), nameof(FrameworkLanguageItem.Name));
                result.AddKey<FrameworkLanguageText>(nameof(FrameworkLanguageText.AppTypeName), nameof(FrameworkLanguageText.LanguageId), nameof(FrameworkLanguageText.ItemId));
                result.AddReference<FrameworkLanguageText, FrameworkLanguage>(nameof(FrameworkLanguageText.LanguageId));
                result.AddReference<FrameworkLanguageText, FrameworkLanguageItem>(nameof(FrameworkLanguageText.ItemId));
            }

            // Application (custom) Integrate data rows to generate CSharp code from.
            CommandGenerateIntegrate(result);

            // Call method CommandGenerateIntegrate(); on ExternalGit AppCli for deployDb only. Not for cli generate command.
            if (isDeployDb)
            {
                UtilExternalGit.CommandGenerateIntegrate(this, result);
            }

            return result;
        }

        /// <summary>
        /// Override this method to add Integrate data rows to list. Used by cli generate command to generate CSharp code.
        /// Note: Cli generate command is not Integrate table reference aware. Data is generated in CSharp code as it is.
        /// </summary>
        protected virtual internal void CommandGenerateIntegrate(GenerateIntegrateResult result)
        {

        }

        /// <summary>
        /// Group of Integrate TypeRow.
        /// Note: Cli generate command is not Integrate table reference aware. Data is generated in CSharp code as it is.
        /// </summary>
        internal class GenerateIntegrateItem
        {
            /// <summary>
            /// Constructor for Framework and Application.
            /// </summary>
            internal GenerateIntegrateItem(GenerateIntegrateResult owner, bool isFrameworkDb, bool isApplication, Type typeRow, IQueryable<Row> query, string appTypeName)
            {
                this.Owner = owner;
                this.IsFrameworkDb = isFrameworkDb;
                this.IsApplication = isApplication;
                this.TypeRow = typeRow;
                this.Query = query;
                this.AppTypeName = appTypeName;
                UtilDalType.TypeRowToTableNameSql(TypeRow, out _, out _);
                this.SchemaNameCSharp = UtilDalType.TypeRowToSchemaNameCSharp(TypeRow);
                this.TableNameSql = UtilDalType.TypeRowToTableNameWithSchemaSql(TypeRow);
                this.TableNameCSharp = UtilDalType.TypeRowToTableNameCSharpWithoutSchema(TypeRow);
            }

            /// <summary>
            /// Constructor for Application.
            /// </summary>
            private GenerateIntegrateItem(GenerateIntegrateResult owner, bool isApplication, Type typeRow, IQueryable<Row> query) 
                : this(owner, false, isApplication, typeRow, query, null)
            {

            }

            /// <summary>
            /// Constructor for GenerateIntegrateItem.
            /// </summary>
            /// <param name="isApplication">If true, RowList will be available at runtime as Integrate CSharp code with additional IdEnum if row contains IdName column. If false, RowList will be generated into cli as CSharp code only.</param>
            public static GenerateIntegrateItem Create<TRow>(GenerateIntegrateResult owner, IQueryable<TRow> query, bool isApplication = false) where TRow : Row
            {
                return new GenerateIntegrateItem(owner, isApplication, typeof(TRow), query.Cast<Row>());
            }

            /// <summary>
            /// Gets Owner.
            /// </summary>
            public readonly GenerateIntegrateResult Owner;

            /// <summary>
            /// Gets IsFrameworkDb. If true, RowList is generated into Framework library (internal use only). If false, RowList is generated into Application library.
            /// </summary>
            public readonly bool IsFrameworkDb;

            /// <summary>
            /// Gets IsApplication. If true, RowList will be available at runtime. If false, RowList will be generated into cli.
            /// </summary>
            public readonly bool IsApplication;

            /// <summary>
            /// Gets TypeRow. From database returned RowList can be empty.
            /// </summary>
            public readonly Type TypeRow;

            /// <summary>
            /// Gets SchemaNameCSharp.
            /// </summary>
            public readonly string SchemaNameCSharp;

            /// <summary>
            /// Gets TableNameSql. Includes sql schema.
            /// </summary>
            public readonly string TableNameSql;

            /// <summary>
            /// Gets TableNameCSharp. Without schema.
            /// </summary>
            public readonly string TableNameCSharp;

            /// <summary>
            /// Gets Query. Items need to be all of same TypeRow.
            /// </summary>
            public readonly IQueryable<Row> Query;

            /// <summary>
            /// Gets AppTypeName. If not null, AppTypeName is used as suffix. Used to split sql table with AppTypeName column into multiple different classes for each app.
            /// </summary>
            public readonly string AppTypeName;

            /// <summary>
            /// Gets RowList. Items need to be all of same TypeRow.
            /// </summary>
            public List<Row> RowList
            {
                get
                {
                    List<Row> result = Query.QueryExecute();
                    foreach (var item in result)
                    {
                        UtilFramework.Assert(item.GetType() == TypeRow);
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Return from database loaded Integrate rows to generate CSharp code.
        /// </summary>
        public class GenerateIntegrateResult
        {
            internal GenerateIntegrateResult(List<Assembly> assemblyList)
            {
                AssemblyList = assemblyList;
                TableNameCSharpList = UtilDalType.TableNameCSharpList(AssemblyList.ToArray());
                Result = new List<GenerateIntegrateItem>();
            }

            internal readonly List<Assembly> AssemblyList;

            internal readonly Dictionary<Type, string> TableNameCSharpList;

            internal List<GenerateIntegrateItem> Result;

            internal readonly List<UtilDalUpsertIntegrate.Reference> ResultReference = new List<UtilDalUpsertIntegrate.Reference>();

            /// <summary>
            /// (TypeRow, FieldNameCSharp).
            /// </summary>
            internal readonly Dictionary<Type, string[]> ResultKey = new Dictionary<Type, string[]>();

            /// <summary>
            /// (TypeRow, FieldNameCSharp, FileName)
            /// </summary>
            internal readonly Dictionary<Type, Dictionary<string, Func<Row, string>>> ResultBlob = new Dictionary<Type, Dictionary<string, Func<Row, string>>>();

            private void ResultAdd(GenerateIntegrateItem value)
            {
                Result.Add(value);
            }

            internal void Add(bool isFrameworkDb, bool isApplication, IQueryable<Row> query, string appTypeName = null)
            {
                var typeRow = query.ElementType;
                var result = new GenerateIntegrateItem(this, isFrameworkDb, isApplication, typeRow, query, appTypeName);
                ResultAdd(result);
            }

            /// <summary>
            /// Add from database loaded Integrate rows to generate CSharp code.
            /// </summary>
            /// <param name="isApplication">If true, RowList will be available at runtime as Integrate CSharp code with additional IdEnum if row contains IdName column. If false, RowList will be generated into cli as CSharp code only.</param>
            public void Add<TRow>(IQueryable<TRow> query, bool isApplication = false) where TRow : Row
            {
                var result = GenerateIntegrateItem.Create(this, query, isApplication);
                ResultAdd(result);
            }

            /// <summary>
            /// Add unique key.
            /// </summary>
            public void AddKey<TRow>(params string[] fieldNameKeyList) where TRow : Row
            {
                Type typeRow = typeof(TRow);

                // Assert table name ends with Integrate
                string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                UtilFramework.Assert(!tableNameCSharp.EndsWith("Integrate"), string.Format("Do not add Integrate. Use underlying sql table! ({0})", tableNameCSharp));

                // Assert field exists
                var fieldNameCSharpList = UtilDalType.TypeRowToFieldList(typeRow).Select(item => item.FieldNameCSharp).ToList();
                foreach (var fieldNameCSharp in fieldNameKeyList)
                {
                    UtilFramework.Assert(fieldNameCSharpList.Contains(fieldNameCSharp), string.Format("Field not found! ({0})", fieldNameCSharp));
                }

                if (ResultKey.ContainsKey(typeof(TRow)))
                {
                    UtilFramework.Assert(ResultKey[typeRow].SequenceEqual(fieldNameKeyList), string.Format("TypeRow added with different FieldNameKeyList! ({0})", UtilDalType.TypeRowToTableNameCSharp(typeRow)));
                }
                else
                {
                    ResultKey.Add(typeRow, fieldNameKeyList);
                }
            }

            /// <summary>
            /// Store field value not in CSharp code. Store value in an external file in folder Blob/. Will be applied for cli data and for type byte[] and string.
            /// </summary>
            /// <typeparam name="TRow">Data row.</typeparam>
            /// <param name="fieldName">For example binary field "Data".</param>
            /// <param name="fileName">Unique file name. For example "abc.jpg". It will be prefixed by framework with database schema name and table name. For Integrate table typically use IdName.</param>
            public void AddBlob<TRow>(string fieldName, Func<TRow, string> fileName) where TRow : Row
            {
                Type typeRow = typeof(TRow);

                // Assert method Add(); has been called before.
                UtilFramework.Assert(Result.Where(item => item.TypeRow == typeRow).Any(), string.Format("No query defined yet! Use first method Add(); ({0})", typeRow.Name));

                // Assert field exists
                var fieldNameCSharpList = UtilDalType.TypeRowToFieldList(typeRow).Select(item => item.FieldNameCSharp).ToList();
                UtilFramework.Assert(fieldNameCSharpList.Contains(fieldName), string.Format("Field not found! ({0})", fieldName));

                // ResultBlob
                if (!ResultBlob.ContainsKey(typeRow))
                {
                    ResultBlob.Add(typeRow, new Dictionary<string, Func<Row, string>>());
                }
                ResultBlob[typeRow].Add(fieldName, (row) => fileName((TRow)row));
            }

            /// <summary>
            /// Add reference table. Parameter fieldNameId references a table. For example: "UserId". It also needs a corresponding "UserIdName". Command cli generate will produce "UserId = 0" CSharp code.
            /// </summary>
            /// <typeparam name="TRow">For example: "LoginUserRole".</typeparam>
            /// <typeparam name="TRowReference">For example: "LoginUser".</typeparam>
            /// <param name="fieldNameId">For example: "UserId". Needs a corresponding "UserIdName".</param>
            public void AddReference<TRow, TRowReference>(string fieldNameId) where TRow : Row where TRowReference : Row
            {
                string fieldNameIdCSharp = fieldNameId;

                Type typeRowResult;
                string fieldNameIdCSharpResult;
                string fieldNameIdSqlResult;
                Type typeRowIntegrateResult;
                string fieldNameIdNameCSharpResult;
                string fieldNameIdNameSqlResult;
                Type typeRowReferenceResult;
                Type typeRowReferenceIntegrateResult;

                // Row
                {
                    Type typeRow = typeof(TRow);
                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    UtilFramework.Assert(!tableNameCSharp.EndsWith("Integrate"), string.Format("Do not add Integrate. Use underlying sql table! ({0})", tableNameCSharp));

                    var fieldId = UtilDalType.TypeRowToFieldList(typeRow).Where(item => item.FieldNameCSharp == fieldNameIdCSharp).FirstOrDefault();
                    UtilFramework.Assert(fieldId != null, string.Format("Field not found! ({0}.{1})", tableNameCSharp, fieldNameIdCSharp));

                    typeRowResult = typeRow;
                    fieldNameIdCSharpResult = fieldId.FieldNameCSharp;
                    fieldNameIdSqlResult = fieldId.FieldNameSql;
                }

                // Row Integrate
                {
                    Type typeRow = typeof(TRow);
                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    var tableIntegrate = TableNameCSharpList.Where(item => item.Value == tableNameCSharp + "Integrate").SingleOrDefault();
                    Type typeRowIntegrate = tableIntegrate.Key;
                    string tableNameIntegrate = tableIntegrate.Value;
                    UtilFramework.Assert(tableNameIntegrate != null, string.Format("CSharp Integrate row not found! Run command cli generate? ({0})", tableNameCSharp));

                    var fieldIntegrateId = UtilDalType.TypeRowToFieldList(typeRowIntegrate).Where(item => item.FieldNameCSharp == fieldNameIdCSharp).FirstOrDefault();
                    UtilFramework.Assert(fieldIntegrateId != null, string.Format("Field not found! ({0}.{1})", tableNameIntegrate, fieldNameIdCSharp));

                    var fieldIntegrateIdName = UtilDalType.TypeRowToFieldList(typeRowIntegrate).Where(item => item.FieldNameCSharp == fieldNameIdCSharp + "Name").FirstOrDefault();
                    UtilFramework.Assert(fieldIntegrateIdName != null, string.Format("CSharp field not found! Run command cli generate? ({0}.{1})", tableNameIntegrate, fieldNameIdCSharp + "Name"));

                    typeRowIntegrateResult = typeRowIntegrate;
                    fieldNameIdNameCSharpResult = fieldIntegrateIdName.FieldNameCSharp;
                    fieldNameIdNameSqlResult = fieldIntegrateIdName.FieldNameCSharp;
                }

                // Row Reference
                {
                    Type typeRow = typeof(TRowReference);
                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    UtilFramework.Assert(!tableNameCSharp.EndsWith("Integrate"), string.Format("Do not add Integrate. Use underlying sql table! ({0})", tableNameCSharp));

                    var fieldId = UtilDalType.TypeRowToFieldList(typeRow).Where(item => item.FieldNameCSharp == "Id").FirstOrDefault();
                    UtilFramework.Assert(fieldId != null, string.Format("Field not found! ({0}.{1})", tableNameCSharp, "Id"));

                    typeRowReferenceResult = typeRow;
                }

                // Row Reference Integrate
                {
                    Type typeRow = typeof(TRowReference);
                    string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(typeRow);
                    var tableIntegrate = TableNameCSharpList.Where(item => item.Value == tableNameCSharp + "Integrate").SingleOrDefault();
                    Type typeRowIntegrate = tableIntegrate.Key;
                    string tableNameIntegrate = tableIntegrate.Value;
                    UtilFramework.Assert(tableNameIntegrate != null, string.Format("Integrate not found! ({0})", tableNameIntegrate));

                    var fieldIntegrateId = UtilDalType.TypeRowToFieldList(typeRowIntegrate).Where(item => item.FieldNameCSharp == "Id").FirstOrDefault();
                    UtilFramework.Assert(fieldIntegrateId != null, string.Format("Field not found! ({0}.{1})", tableNameIntegrate, "Id"));

                    var fieldIntegrateIdName = UtilDalType.TypeRowToFieldList(typeRowIntegrate).Where(item => item.FieldNameCSharp == "IdName").FirstOrDefault();
                    UtilFramework.Assert(fieldIntegrateIdName != null, string.Format("Field not found! ({0}.{1})", tableNameIntegrate, "IdName"));

                    typeRowReferenceIntegrateResult = typeRowIntegrate;
                }

                // Result
                var reference = new UtilDalUpsertIntegrate.Reference(typeRowResult, fieldNameIdCSharpResult, fieldNameIdSqlResult, typeRowIntegrateResult, fieldNameIdNameCSharpResult, fieldNameIdNameSqlResult, typeRowReferenceResult, typeRowReferenceIntegrateResult);
                UtilFramework.Assert(ResultReference.Count(item => item.TypeRow == reference.TypeRow && item.FieldNameIdCSharp == reference.FieldNameIdCSharp) == 0, "Reference already defined!");
                ResultReference.Add(reference);
            }
        }

        /// <summary>
        /// Override if this application is cloned into ExternalGit/ folder. See also command cli ExternalGit.
        /// </summary>
        /// <param name="args">Some utils for example to copy files.</param>
        protected virtual internal void CommandExternalGit(ExternalGitArgs args)
        {

        }

        /// <summary>
        /// Args for cli ExternalGit command.
        /// </summary>
        public class ExternalGitArgs
        {
            /// <summary>
            /// Gets AppSourceFolderName. This is folder ExternalGit/ProjectName/Application/App/
            /// </summary>
            internal string AppSourceFolderName { get; set; }

            /// <summary>
            /// Gets AppDestFolderName. This is folder Application/App/ExternalGit/ProjectName/
            /// </summary>
            internal string AppDestFolderName { get; set; }

            /// <summary>
            /// Gets DatabaseSourceFolderName. This is folder ExternalGit/ProjectName/Application.Database/Database/
            /// </summary>
            internal string DatabaseSourceFolderName { get; set; }

            /// <summary>
            /// Gets DatabaseDestFolderName. This is folder Application.Database/Database/ExternalGit/ProjectName/
            /// </summary>
            internal string DatabaseDestFolderName { get; set; }

            /// <summary>
            /// Gets CliAppSourceFolderName. This is folder Application.Cli/App/
            /// </summary>
            internal string CliAppSourceFolderName { get; set; }

            /// <summary>
            /// Gets CliAppDestFolderName. This is folder Application.Cli/App/ExternalGit/ProjectName/
            /// </summary>
            internal string CliAppDestFolderName { get; set; }

            /// <summary>
            /// Gets CliDatabaseSourceFolderName. This is folder Application.Cli/Database/
            /// </summary>
            internal string CliDatabaseSourceFolderName { get; set; }

            /// <summary>
            /// Gets CliDatabaseDestFolderName. This is folder Application.Cli/Database/ExternalGit/ProjectName/
            /// </summary>
            internal string CliDatabaseDestFolderName { get; set; }

            /// <summary>
            /// Gets DeployDbSourceFolderName. This is folder Application.Cli/DeployDb/
            /// </summary>
            internal string CliDeployDbSourceFolderName { get; set; }

            /// <summary>
            /// Gets DeployDbDestFolderName. This is folder Application.Cli/DeployDb/ExternalGit/ProjectName/
            /// </summary>
            internal string CliDeployDbDestFolderName { get; set; }

            /// <summary>
            /// Gets ExternalProjectName. See also file ConfigCli.json of host cli.
            /// </summary>
            internal string ExternalProjectName { get; set; }

            /// <summary>
            /// Copy file from source to dest. Creates new dest folder if it doesn't exist.
            /// </summary>
            public void FileCopy(string fileNameSource, string fileNameDest)
            {
                UtilCliInternal.FileCopy(fileNameSource, fileNameDest);
            }

            /// <summary>
            /// Copy folder. Delete before copy.
            /// </summary>
            internal void FolderCopy(string folderNameSource, string folderNameDest)
            {
                UtilCliInternal.FolderDelete(folderNameDest);
                UtilCliInternal.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
            }

            /// <summary>
            /// Returns files in folder and subfolders.
            /// </summary>
            /// <param name="searchPattern">For example "*.*"</param>
            internal List<string> FileNameList(string folderName, string searchPattern)
            {
                return UtilCliInternal.FileNameList(folderName, searchPattern);
            }

            /// <summary>
            /// Find and replace a line in a text file.
            /// </summary>
            internal void FileReplaceLine(string fileName, string find, string replace)
            {
                string text = UtilFramework.FileLoad(fileName);
                text = UtilFramework.ReplaceLine(text, find, replace);
                UtilFramework.FileSave(fileName, text);
            }
        }
    }
}