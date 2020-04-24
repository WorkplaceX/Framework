namespace Framework.Cli
{
    using Database.dbo;
    using DatabaseBuiltIn.dbo;
    using Framework.Cli.Command;
    using Framework.Cli.Config;
    using Framework.Cli.Generate;
    using Framework.Config;
    using Framework.DataAccessLayer;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        /// Gets "Application.Database" assembly. This assembly hosts from database generated rows.
        /// </summary>
        public readonly Assembly AssemblyApplicationDatabase;

        /// <summary>
        /// Gets "Application" assembly. This assembly hosts business logic.
        /// </summary>
        public readonly Assembly AssemblyApplication;

        /// <summary>
        /// Gets "Framework" assembly.
        /// </summary>
        internal Assembly AssemblyFramework
        {
            get
            {
                return typeof(FrameworkDeployDb).Assembly;
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
        public List<Assembly> AssemblyList(bool isIncludeApp = false, bool isIncludeFrameworkCli = false)
        {
            List<Assembly> result = new List<Assembly>();
            result.Add(AssemblyFramework);
            result.Add(AssemblyApplicationDatabase);
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
                // http://patorjk.com/software/taag/#p=display&f=Ivrit&t=Framework%20CLI
                string text = @"
                  _____                                            _        ____ _     ___ 
                 |  ___| __ __ _ _ __ ___   _____      _____  _ __| | __   / ___| |   |_ _|
                 | |_ | '__/ _` | '_ ` _ \ / _ \ \ /\ / / _ \| '__| |/ /  | |   | |    | | 
                 |  _|| | | (_| | | | | | |  __/\ V  V / (_) | |  |   <   | |___| |___ | | 
                 |_|  |_|  \__,_|_| |_| |_|\___| \_/\_/ \___/|_|  |_|\_\   \____|_____|___|
                ";
                text = text.Replace("\n                 ", "\n");
                text = text.Substring(2);
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
                Console.WriteLine("WorkplaceX={0};", UtilFramework.Version);

                commandLineApplication.Execute("-h"); // Show list of available commands.
            }
        }

        /// <summary>
        /// Override to register new commands.
        /// </summary>
        internal virtual void RegisterCommand()
        {
            new CommandConfig(this);
            new CommandGenerate(this);
            new CommandBuild(this);
            new CommandStart(this);
            new CommandDeploy(this);
            new CommandDeployDb(this);
            new CommandEnvironment(this);
            new CommandTest(this);
        }

        /// <summary>
        /// Override this method to change default cli configuration.
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
                        UtilCli.ConsoleWriteLineColor($"Execute Framework Cli Command ({command.Name})", ConsoleColor.Yellow);
                        command.Execute();
                        return 0;
                    });

                    configuration.HelpOption("-h | --help"); // Command help (to show arguments and options)
                });
            }
            commandLineApplication.HelpOption("-h | --help"); // Command line interface help (to show commands)
        }

        /// <summary>
        /// Overwrite this method to filter out only specific application tables and fields for which to generate code. For example only tables starting with "Explorer".
        /// </summary>
        /// <param name="list">Input list.</param>
        /// <returns>Returns filtered output list.</returns>
        protected virtual internal MetaSqlSchema[] CommandGenerateFilter(MetaSqlSchema[] list)
        {
            // return list.Where(item => item.SchemaName == "dbo" && item.TableName.StartsWith("Explorer")).ToArray();
            return list;
        }

        /// <summary>
        /// Copy ConfigCli.json to ConfigServer.json and validate ConnectionString exists.
        /// </summary>
        private void ConfigToServer()
        {
            ConfigCli.ConfigToServer();
            var configCli = ConfigCli.Load();
            var environment = configCli.EnvironmentGet();
            if (UtilFramework.StringNull(environment.ConnectionStringFramework) == null || UtilFramework.StringNull(environment.ConnectionStringFramework) == null)
            {
                UtilCli.ConsoleWriteLineColor(string.Format("No ConnectionString for {0}! Set it with cli command: config connectionString", environment.EnvironmentName), ConsoleColor.Yellow);
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
                ConfigCli.Init(this);
                var configCli = ConfigCli.Load();
                ConfigCli.Save(configCli); // Reset ConfigCli.json
                ConfigCli.ConfigToServer();
                CommandEnvironment.ConsoleWriteLineCurrentEnvironment(configCli);
                commandLineApplication.Execute(args);
                ConfigToServer(); // Copy new values from ConfigCli.json to ConfigServer.json
            }
            catch (Exception exception) // For example unrecognized option
            {
                UtilCli.ConsoleWriteLineError(exception);
                Environment.ExitCode = 1; // echo %errorlevel%
            }
        }

        /// <summary>
        /// Override this method to add BuiltIn data rows to list. Used by cli deployDb command to deploy BuiltIn rows to database.
        /// </summary>
        protected virtual void CommandDeployDbBuiltIn(DeployDbBuiltInResult result)
        {

        }

        /// <summary>
        /// Returns BuiltIn rows to deploy to sql database.
        /// </summary>
        internal DeployDbBuiltInResult CommandDeployDbBuiltInInternal()
        {
            var result = new DeployDbBuiltInResult();

            // FrameworkConfigGridBuiltIn
            {
                var rowList = FrameworkConfigGridBuiltInTableFrameworkCli.RowList;

                // Read FrameworkConfigGridBuiltIn.RowListList from Application.Cli project.
                string nameCli = "DatabaseBuiltIn.dbo.FrameworkConfigGridBuiltInApplicationCli"; // See also method GenerateCSharpTableNameClass();
                var typeCli = AssemblyApplicationCli.GetType(nameCli);
                UtilFramework.Assert(typeCli != null, string.Format("Type not found! See also method GenerateCSharpTableNameClass(); ({0})", nameCli));
                PropertyInfo propertyInfo = typeCli.GetProperty(nameof(FrameworkConfigGridBuiltInTableFrameworkCli.RowList));
                var rowApplicationCliList = (List<FrameworkConfigGridBuiltIn>)propertyInfo.GetValue(null);
                rowList.AddRange(rowApplicationCliList);

                result.Add(rowList, new string[] { "TableId", "ConfigName" });
            }

            // FrameworkConfigFieldBuiltIn
            {
                var rowList = FrameworkConfigFieldBuiltInTableFrameworkCli.RowList;

                // Read FrameworkConfigFieldBuiltInCli.List from Application.Cli project.
                string nameCli = "DatabaseBuiltIn.dbo.FrameworkConfigFieldBuiltInApplicationCli"; // See also method GenerateCSharpTableNameClass();
                var typeCli = AssemblyApplicationCli.GetType(nameCli);
                UtilFramework.Assert(typeCli != null, string.Format("Type not found! See also method GenerateCSharpTableNameClass(); ({0})", nameCli));
                PropertyInfo propertyInfo = typeCli.GetProperty(nameof(FrameworkConfigFieldBuiltInTableFrameworkCli.RowList));
                var rowApplicationCliList = (List<FrameworkConfigFieldBuiltIn>)propertyInfo.GetValue(null);
                rowList.AddRange(rowApplicationCliList);

                result.Add(rowList, new string[] { "ConfigGridId", "FieldId", "InstanceName" });
            }

            // Application (custom) BuiltIn data rows to deploy to database
            CommandDeployDbBuiltIn(result);

            // Copy TableNameSqlReferencePrefix from GenerateBuiltIn to DeployDbBuiltIn
            var resultGenerate = new GenerateBuiltInResult();
            CommandGenerateBuiltIn(resultGenerate);
            var tableNameSqlReferencePrefixList = resultGenerate.TableNameSqlReferencePrefixList;
            foreach (var item in result.Result)
            {
                if (tableNameSqlReferencePrefixList.ContainsKey(item.TypeRow))
                {
                    UtilFramework.Assert(item.TableNameSqlReferencePrefix == null);
                    item.TableNameSqlReferencePrefix = tableNameSqlReferencePrefixList[item.TypeRow];
                }
            }

            return result;
        }

        public class DeployDbBuiltInResult
        {
            internal DeployDbBuiltInResult()
            {
                this.Result = new List<UtilDalUpsertBuiltIn.UpsertItem>();
            }

            internal List<UtilDalUpsertBuiltIn.UpsertItem> Result;

            public void Add<TRow>(List<TRow> rowList, string[] fieldNameKeyList) where TRow : Row
            {
                var result = UtilDalUpsertBuiltIn.UpsertItem.Create(rowList, fieldNameKeyList, tableNameSqlReferencePrefix: null); // For TableNameSqlReferencePrefix definition see also class GenerateBuiltInResult.
                Result.Add(result);
            }

            public void Add<TRow>(List<TRow> rowList, string fieldNameKey) where TRow : Row
            {
                Add(rowList, new string[] { fieldNameKey });
            }

            /// <summary>
            /// Add hierarchical list with Id and ParentId column.
            /// </summary>
            public void Add<TRow>(List<TRow> rowList, string[] fieldNameKeyList, Func<TRow, int> idSelector, Func<TRow, int?> parentIdSelector, Func<TRow, object> sortSelector) where TRow : Row
            {
                List<TRow> rowLevelList = null;
                while (OrderByHierarchical(rowList, idSelector, parentIdSelector, sortSelector, ref rowLevelList)) // Step through all levels.
                {
                    Add<TRow>(rowLevelList, fieldNameKeyList);
                }
            }

            /// <summary>
            /// Overload.
            /// </summary>
            public void Add<TRow>(List<TRow> rowList, string fieldNameKey, Func<TRow, int> idSelector, Func<TRow, int?> parentIdSelector, Func<TRow, object> sortSelector) where TRow : Row
            {
                Add(rowList, new string[] { fieldNameKey }, idSelector, parentIdSelector, sortSelector);
            }

            private static bool OrderByHierarchical<TRow>(List<TRow> rowAllList, Func<TRow, int> idSelector, Func<TRow, int?> parentIdSelector, Func<TRow, object> sortSelector, ref List<TRow> rowLevelList) where TRow : Row
            {
                if (rowLevelList == null)
                {
                    rowLevelList = rowAllList.Where(item => parentIdSelector(item) == null).ToList();
                }
                else
                {
                    var idList = rowLevelList.Select(item => (int?)idSelector(item)).ToList();
                    rowLevelList = rowAllList.Where(item => idList.Contains(parentIdSelector(item))).OrderBy(item => sortSelector(item)).ToList();
                }
                return rowLevelList.Count() != 0;
            }
        }

        /// <summary>
        /// Returns BuiltIn rows to generate CSharp code.
        /// </summary>
        internal GenerateBuiltInResult CommandGenerateBuiltInListInternal()
        {
            var result = new GenerateBuiltInResult();

            var tableNameCSharpFrameworkList = UtilDalType.TableNameCSharpList(typeof(Data).Assembly); // TableNameCSharp declared in Framework assembly.
            var tableNameCSharpApplicationList = UtilDalType.TableNameCSharpList(AssemblyApplication, AssemblyApplicationDatabase); // TableNameCSharp declared in Application assembly.
            var fieldNameCSharpFrameworkList = UtilDalType.FieldNameCSharpList(typeof(Data).Assembly).Select(item => item.Item1 + "/" + item.Item2); // FieldNameCSharp declared in Framework assembly
            var fieldNameCSharpApplicationList = UtilDalType.FieldNameCSharpList(AssemblyApplication, AssemblyApplicationDatabase).Select(item => item.Item1 + "/" + item.Item2); // FieldNameCSharp declared in Framework assembly

            // FrameworkConfigGridBuiltIn
            {
                var rowList = Data.Query<FrameworkConfigGridBuiltIn>();
                
                // Framework (.\cli.cmd generate -f)
                {
                    var rowFilterList = rowList.Where(item => tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter Framework.
                    rowFilterList = rowFilterList.OrderBy(item => item.IdName);
                    result.Add(
                        isFrameworkDb: true,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigGridBuiltIn),
                        query: rowFilterList,
                        tableNameSqlReferencePrefix: "Framework"
                    );
                }
                // Application (.\cli.cmd generate)
                {
                    var rowFilterList = rowList.Where(item => !tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp) && tableNameCSharpApplicationList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter (not Framework and Application).
                    rowFilterList = rowFilterList.OrderBy(item => item.IdName);
                    result.Add(
                        isFrameworkDb: false,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigGridBuiltIn),
                        query: rowFilterList,
                        tableNameSqlReferencePrefix: "Framework"
                    );
                }
            }

            // FrameworkConfigFieldBuiltIn
            {
                var rowList = Data.Query<FrameworkConfigFieldBuiltIn>();
                // Framework (.\cli.cmd generate -f)
                {
                    var rowFilterList = rowList.Where(item => tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter FrameworkDb.
                    rowFilterList = rowList.Where(item => fieldNameCSharpFrameworkList.Contains(item.TableNameCSharp + "/" + item.FieldNameCSharp)); // Filter FieldNameCSharp declared in Framework assembly.
                    rowFilterList = rowFilterList.OrderBy(item => item.FieldIdName);
                    result.Add(
                        isFrameworkDb: true,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigFieldBuiltIn),
                        query: rowFilterList,
                        tableNameSqlReferencePrefix: "Framework"
                    );
                }
                // Application (.\cli.cmd generate)
                {
                    var rowFilterList = rowList.Where(item => !tableNameCSharpFrameworkList.Values.ToArray().Contains(item.TableNameCSharp) && tableNameCSharpApplicationList.Values.ToArray().Contains(item.TableNameCSharp)); // Filter (not Framework and Application).
                    rowFilterList = rowList.Where(item => fieldNameCSharpApplicationList.Contains(item.TableNameCSharp + "/" + item.FieldNameCSharp)); // Filter FieldNameCSharp declared in Application assembly.
                    rowFilterList = rowFilterList.OrderBy(item => item.FieldIdName);
                    result.Add(
                        isFrameworkDb: false,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigFieldBuiltIn),
                        query: rowFilterList,
                        tableNameSqlReferencePrefix: "Framework"
                    );
                }
            }

            // Application (custom) BuiltIn data rows to generate CSharp code from.
            CommandGenerateBuiltIn(result);

            return result;
        }

        /// <summary>
        /// Override this method to add BuiltIn data rows to list. Used by cli generate command to generate CSharp code.
        /// Note: Cli generate command is not BuiltIn table reference aware. Data is generated in CSharp code as it is.
        /// </summary>
        protected virtual void CommandGenerateBuiltIn(GenerateBuiltInResult result)
        {

        }

        /// <summary>
        /// Group of BuiltIn TypeRow.
        /// Note: Cli generate command is not BuiltIn table reference aware. Data is generated in CSharp code as it is.
        /// </summary>
        internal class GenerateBuiltInItem
        {
            /// <summary>
            /// Constructor for Framework and Application.
            /// </summary>
            internal GenerateBuiltInItem(bool isFrameworkDb, bool isApplication, Type typeRow, IQueryable<Row> query, string tableNameSqlReferencePrefix)
            {
                this.IsFrameworkDb = isFrameworkDb;
                this.IsApplication = isApplication;
                this.TypeRow = typeRow;
                this.Query = query;
                this.TableNameSqlReferencePrefix = tableNameSqlReferencePrefix;
                UtilDalType.TypeRowToTableNameSql(TypeRow, out string schemaNameSql, out string tableNameSql);
                this.SchemaNameCSharp = UtilDalType.TypeRowToSchemaNameCSharp(TypeRow);
                this.TableNameCSharp = UtilDalType.TypeRowToTableNameCSharpWithoutSchema(TypeRow);
            }

            /// <summary>
            /// Constructor for Application.
            /// </summary>
            private GenerateBuiltInItem(bool isApplication, Type typeRow, IQueryable<Row> query, string tableNameSqlReferencePrefix) 
                : this(false, isApplication, typeRow, query, tableNameSqlReferencePrefix)
            {

            }

            /// <summary>
            /// Constructor for GenerateBuiltInItem.
            /// </summary>
            /// <param name="isApplication">If true, RowList will be available at runtime as BuiltIn CSharp code with additional IdEnum if row contains IdName column. If false, RowList will be generated into cli as CSharp code only.</param>
            public static GenerateBuiltInItem Create<TRow>(IQueryable<TRow> query, bool isApplication = false, string tableNameSqlReferencePrefix = null) where TRow : Row
            {
                return new GenerateBuiltInItem(isApplication, typeof(TRow), query.Cast<Row>(), tableNameSqlReferencePrefix);
            }

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
            /// Gets TableNameCSharp. Without schema.
            /// </summary>
            public readonly string TableNameCSharp;

            /// <summary>
            /// Gets Query. Items need to be all of same TypeRow.
            /// </summary>
            public readonly IQueryable<Row> Query;

            /// <summary>
            /// Gets TableNameSqlReferencePrefix. Used to find reference tables. If value is for example Login, column (UserIdName or MailUserIdName) is referenced to table LoginUserBuiltIn if exists.
            /// </summary>
            public readonly string TableNameSqlReferencePrefix;

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
        /// Return from database loaded BuiltIn rows to generate CSharp code.
        /// </summary>
        public class GenerateBuiltInResult
        {
            internal GenerateBuiltInResult()
            {
                this.Result = new List<GenerateBuiltInItem>();
            }

            internal List<GenerateBuiltInItem> Result;

            internal Dictionary<Type, string> TableNameSqlReferencePrefixList
            {
                get
                {
                    var result = new Dictionary<Type, string>();
                    foreach (var item in Result)
                    {
                        if (result.ContainsKey(item.TypeRow))
                        {
                            UtilFramework.Assert(result[item.TypeRow] == item.TableNameSqlReferencePrefix);
                        }
                        else
                        {
                            result.Add(item.TypeRow, item.TableNameSqlReferencePrefix);
                        }
                    }
                    return result;
                }
            }

            private void ResultAdd(GenerateBuiltInItem value)
            {
                foreach (var item in Result.Where(item => item.TypeRow == value.TypeRow))
                {
                    UtilFramework.Assert(item.TableNameSqlReferencePrefix == value.TableNameSqlReferencePrefix, string.Format("Inconsistent TableNameSqlReferencePrefix! ({0}; {1}; {2})", UtilFramework.TypeToName(value.TypeRow), value.TableNameSqlReferencePrefix, item.TableNameSqlReferencePrefix));
                }
                Result.Add(value);
            }

            internal void Add(bool isFrameworkDb, bool isApplication, Type typeRow, IQueryable<Row> query, string tableNameSqlReferencePrefix)
            {
                var result = new GenerateBuiltInItem(isFrameworkDb, isApplication, typeRow, query, tableNameSqlReferencePrefix);
                ResultAdd(result);
            }

            /// <summary>
            /// Add from database loaded BuiltIn rows to generate CSharp code.
            /// </summary>
            /// <param name="isApplication">If true, RowList will be available at runtime as BuiltIn CSharp code with additional IdEnum if row contains IdName column. If false, RowList will be generated into cli as CSharp code only.</param>
            public void Add<TRow>(IQueryable<TRow> query, bool isApplication = false, string tableNameSqlReferencePrefix = null) where TRow : Row
            {
                var result = GenerateBuiltInItem.Create(query, isApplication, tableNameSqlReferencePrefix);
                ResultAdd(result);
            }
        }
    }
}
