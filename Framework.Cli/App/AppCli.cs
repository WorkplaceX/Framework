namespace Framework.Cli.Command
{
    using Database.dbo;
    using DatabaseBuiltIn.dbo;
    using Framework.Cli.Config;
    using Framework.Cli.Generate;
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
                return typeof(FrameworkScript).Assembly;
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
        protected virtual void RegisterCommand()
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
        /// Run command line interface.
        /// </summary>
        public void Run(string[] args)
        {
            Title(args);

            try
            {
                commandLineApplication.Execute(args);
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
        protected virtual void CommandDeployDbBuiltIn(List<DeployDbBuiltInItem> list)
        {

        }

        /// <summary>
        /// Group of BuiltIn TypeRow.
        /// </summary>
        public class DeployDbBuiltInItem
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            internal DeployDbBuiltInItem(List<Row> rowList, string[] fieldNameKeyList, string tableNameSqlReferencePrefix)
            {
                this.RowList = rowList;
                this.FieldNameKeyList = fieldNameKeyList;
                this.TableNameSqlReferencePrefix = tableNameSqlReferencePrefix;
            }

            /// <summary>
            /// Constructor for DeployDbBuiltInItem.
            /// </summary>
            /// <param name="fieldNameKeyList">For example (UserId, RoleId).</param>
            /// <param name="tableNameSqlReferencePrefix">Used to find reference tables. If value is for example Login, column UserIdName would be referenced to table LoginUserBuiltIn if exists.</param>
            public static DeployDbBuiltInItem Create<TRow>(List<TRow> rowList, string[] fieldNameKeyList, string tableNameSqlReferencePrefix = null) where TRow : Row
            {
                return new DeployDbBuiltInItem(rowList.Cast<Row>().ToList(), fieldNameKeyList, tableNameSqlReferencePrefix);
            }

            /// <summary>
            /// Constructor for DeployDbBuiltInItem.
            /// </summary>
            /// <param name="fieldNameKey">For example Name.</param>
            /// <param name="tableNameSqlReferencePrefix">Used to find reference tables. If value is for example Login, column UserIdName would be referenced to table LoginUserBuiltIn if exists.</param>
            public static DeployDbBuiltInItem Create<TRow>(List<TRow> rowList, string fieldNameKey, string tableNameSqlReferencePrefix = null) where TRow : Row
            {
                return new DeployDbBuiltInItem(rowList.Cast<Row>().ToList(), new string[] { fieldNameKey }, tableNameSqlReferencePrefix);
            }

            /// <summary>
            /// Gets RowList. Items have to be all of same TypeRow.
            /// </summary>
            public readonly List<Row> RowList;

            /// <summary>
            /// Add RowList to RowList.
            /// </summary>
            internal void RowListUnion(List<Row> rowList)
            {
                foreach (var item in rowList)
                {
                    this.RowList.Add(item);
                }
            }

            /// <summary>
            /// Gets FieldNameKeyList. Sql unique index for upsert.
            /// </summary>
            public readonly string[] FieldNameKeyList;

            /// <summary>
            /// Gets TableNameSqlReferencePrefix. Used to find reference tables. If value is for example Login, column UserIdName would be referenced to table LoginUserBuiltIn if exists.
            /// </summary>
            public readonly string TableNameSqlReferencePrefix;

            /// <summary>
            /// Gets TypeRow. Based on items in RowList.
            /// </summary>
            public Type TypeRow
            {
                get
                {
                    var result = RowList.FirstOrDefault()?.GetType();
                    foreach (var item in RowList)
                    {
                        UtilFramework.Assert(item.GetType() == result, "RowList contains mixed TypeRow!");
                    }
                    return result;
                }
            }
        }

        /// <summary>
        /// Returns BuiltIn rows to deploy to sql database.
        /// </summary>
        protected internal List<DeployDbBuiltInItem> CommandDeployDbBuiltInListInternal()
        {
            var result = new List<DeployDbBuiltInItem>();

            // FrameworkConfigGridBuiltIn
            {
                var item = DeployDbBuiltInItem.Create(FrameworkConfigGridBuiltInTableFrameworkCli.RowList, new string[] { "TableId", "ConfigName" }, "Framework");
                result.Add(item);

                // Read FrameworkConfigGridBuiltIn.RowListList from Application.Cli project.
                string nameCli = "DatabaseBuiltIn.dbo.FrameworkConfigGridBuiltInTableApplicationCli"; // See also method GenerateCSharpTableNameClass();
                var typeCli = AssemblyApplicationCli.GetType(nameCli);
                PropertyInfo propertyInfo = typeCli.GetProperty(nameof(FrameworkConfigGridBuiltInTableFrameworkCli.RowList));
                var rowList = (List<FrameworkConfigGridBuiltIn>)propertyInfo.GetValue(null);
                item.RowList.AddRange(rowList);
            }

            // FrameworkConfigFieldBuiltIn
            {
                var item = DeployDbBuiltInItem.Create(FrameworkConfigFieldBuiltInTableFrameworkCli.RowList, new string[] { "ConfigGridId", "FieldId" }, "Framework");
                result.Add(item);

                // Read FrameworkConfigFieldBuiltInCli.List from Application.Cli project.
                string nameCli = "DatabaseBuiltIn.dbo.FrameworkConfigFieldBuiltInTableApplicationCli"; // See also method GenerateCSharpTableNameClass();
                var typeCli = AssemblyApplicationCli.GetType(nameCli);
                PropertyInfo propertyInfo = typeCli.GetProperty(nameof(FrameworkConfigFieldBuiltInTableFrameworkCli.RowList));
                var rowList = (List<FrameworkConfigFieldBuiltIn>)propertyInfo.GetValue(null);
                item.RowList.AddRange(rowList);
            }

            // Application (custom) BuiltIn data rows to deploy to database.
            var list = new List<DeployDbBuiltInItem>();
            CommandDeployDbBuiltIn(list);
            result.AddRange(list);

            return result;
        }

        /// <summary>
        /// Returns BuiltIn rows to generate CSharp code.
        /// </summary>
        protected internal List<GenerateBuiltInItem> CommandGenerateBuiltInListInternal()
        {
            var result = new List<GenerateBuiltInItem>();

            // FrameworkConfigGridBuiltIn
            {
                var rowList = Data.Query<FrameworkConfigGridBuiltIn>().OrderBy(item => item.IdName).ToList<FrameworkConfigGridBuiltIn>();
                var typeRowIsFrameworkDbList = UtilDalType.TypeRowIsFrameworkDbFromTableNameCSharpList(rowList.Select(item => item.TableNameCSharp).ToList()); // TableNameCSharp declared in Framework assembly.
                // Framework (.\cli.cmd generate -f)
                {
                    var rowFilterList = rowList.Where(item => typeRowIsFrameworkDbList.ContainsValue(item.TableNameCSharp)).ToList(); // Filter Framework.
                    result.Add(new GenerateBuiltInItem(
                        isFrameworkDb: true,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigGridBuiltIn),
                        rowList: rowFilterList.ToList<Row>()
                    ));
                }
                // Application (.\cli.cmd generate)
                {
                    List<Assembly> assemblyList = new List<Assembly>(new Assembly[] { AssemblyApplication, AssemblyApplicationDatabase });
                    var typeRowList = UtilDalType.TypeRowFromTableNameCSharpList(rowList.Select(item => item.TableNameCSharp).ToList(), assemblyList); // TableNameCSharp declared in Application assembly.
                    var rowFilterList = rowList.Where(item => !typeRowIsFrameworkDbList.ContainsValue(item.TableNameCSharp) && typeRowList.ContainsValue(item.TableNameCSharp)).ToList(); // Filter Application.
                    result.Add(new GenerateBuiltInItem(
                        isFrameworkDb: false,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigGridBuiltIn),
                        rowList: rowFilterList.ToList<Row>()
                    ));
                }
            }

            // FrameworkConfigFieldBuiltIn
            {
                var rowList = Data.Query<FrameworkConfigFieldBuiltIn>().OrderBy(item => item.FieldIdName).ToList<FrameworkConfigFieldBuiltIn>();
                var typeRowIsFrameworkDbList = UtilDalType.TypeRowIsFrameworkDbFromTableNameCSharpList(rowList.Select(item => item.TableNameCSharp).ToList()); // TableNameCSharp declared in Framework assembly.
                // Framework (.\cli.cmd generate -f)
                {
                    var fieldNameCSharpList = UtilDalType.FieldNameCSharpFromTypeRowList(typeRowIsFrameworkDbList);
                    var rowFilterList = rowList.Where(item => typeRowIsFrameworkDbList.ContainsValue(item.TableNameCSharp)).ToList(); // Filter FrameworkDb.
                    rowFilterList = rowList.Where(item => fieldNameCSharpList.Contains(new Tuple<string, string>(item.TableNameCSharp, item.FieldNameCSharp))).ToList(); // Filter FieldNameCSharp declared in Framework assembly.
                    result.Add(new GenerateBuiltInItem(
                        isFrameworkDb: true,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigFieldBuiltIn),
                        rowList: rowFilterList.ToList<Row>()
                    ));
                }
                // Application (.\cli.cmd generate)
                {
                    List<Assembly> assemblyList = new List<Assembly>(new Assembly[] { AssemblyApplication, AssemblyApplicationDatabase });
                    var typeRowList = UtilDalType.TypeRowFromTableNameCSharpList(rowList.Select(item => item.TableNameCSharp).ToList(), assemblyList); // TableNameCSharp declared in Application assembly.
                    var fieldNameCSharpList = UtilDalType.FieldNameCSharpFromTypeRowList(typeRowList);
                    var rowFilterList = rowList.Where(item => !typeRowIsFrameworkDbList.ContainsValue(item.TableNameCSharp) && typeRowList.ContainsValue(item.TableNameCSharp)).ToList(); // Filter Application.
                    rowFilterList = rowList.Where(item => fieldNameCSharpList.Contains(new Tuple<string, string>(item.TableNameCSharp, item.FieldNameCSharp))).ToList(); // Filter FieldNameCSharp declared in Application assembly.
                    result.Add(new GenerateBuiltInItem(
                        isFrameworkDb: false,
                        isApplication: false,
                        typeRow: typeof(FrameworkConfigFieldBuiltIn),
                        rowList: rowFilterList.ToList<Row>()
                    ));
                }
            }

            // Application (custom) BuiltIn data rows to generate CSharp code from.
            var list = new List<GenerateBuiltInItem>();
            CommandGenerateBuiltIn(list);
            result.AddRange(list); // Combine FrameworkConfig with Application.

            return result;
        }

        /// <summary>
        /// Override this method to add BuiltIn data rows to list. Used by cli generate command to generate CSharp code.
        /// Note: Cli generate command is not BuiltIn table reference aware. Data is generated in CSharp code as it is.
        /// </summary>
        protected virtual void CommandGenerateBuiltIn(List<GenerateBuiltInItem> list)
        {

        }

        /// <summary>
        /// Group of BuiltIn TypeRow.
        /// Note: Cli generate command is not BuiltIn table reference aware. Data is generated in CSharp code as it is.
        /// </summary>
        public class GenerateBuiltInItem
        {
            /// <summary>
            /// Constructor for Framework and Application.
            /// </summary>
            internal GenerateBuiltInItem(bool isFrameworkDb, bool isApplication, Type typeRow, List<Row> rowList)
            {
                this.IsFrameworkDb = isFrameworkDb;
                this.IsApplication = isApplication;
                this.TypeRow = typeRow;
                this.RowList = rowList;
                UtilDalType.TypeRowToTableNameSql(TypeRow, out string schemaNameSql, out string tableNameSql);
                this.SchemaNameCSharp = UtilDalType.TypeRowToSchemaNameCSharp(TypeRow);
                this.TableNameCSharp = UtilDalType.TypeRowToTableNameCSharpWithoutSchema(TypeRow);

                foreach (var row in RowList)
                {
                    UtilFramework.Assert(row.GetType() == TypeRow);
                }
            }

            /// <summary>
            /// Constructor for Application.
            /// </summary>
            private GenerateBuiltInItem(bool isApplication, Type typeRow, List<Row> rowList) 
                : this(false, isApplication, typeRow, rowList)
            {

            }

            /// <summary>
            /// Constructor for GenerateBuiltInItem.
            /// </summary>
            /// <param name="isApplication">If true, RowList will be available at runtime as BuiltIn CSharp code with additional IdEnum if row contains IdName column. If false, RowList will be generated into cli as CSharp code only.</param>
            public static GenerateBuiltInItem Create<TRow>(List<TRow> rowList, bool isApplication = false) where TRow : Row
            {
                return new GenerateBuiltInItem(isApplication, typeof(TRow), rowList.Cast<Row>().ToList());
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
            /// Gets RowList. Items need to be all of same TypeRow.
            /// </summary>
            public readonly List<Row> RowList;
        }
    }
}
