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
        public AppCli(Assembly assemblyDatabase, Assembly assemblyApp)
        {
            this.AssemblyDatabase = assemblyDatabase;
            this.AssemblyApp = assemblyApp;

            this.commandLineApplication = new CommandLineApplication();

            RegisterCommand();
            RegisterCommandInit();
        }

        /// <summary>
        /// Gets AssemblyDatabase. This assembly hosts from database generated rows.
        /// </summary>
        public readonly Assembly AssemblyDatabase;

        /// <summary>
        /// Gets AssemblyApp. This assembly hosts business logic.
        /// </summary>
        public readonly Assembly AssemblyApp;

        /// <summary>
        /// Gets Framework assembly.
        /// </summary>
        internal Assembly AssemblyFramework
        {
            get
            {
                return typeof(FrameworkScript).Assembly;
            }
        }

        /// <summary>
        /// Gets Framework.Cli assembly.
        /// </summary>
        internal Assembly AssemblyFrameworkCli
        {
            get
            {
                return typeof(AppCli).Assembly;
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
            result.Add(AssemblyDatabase);
            if (isIncludeApp)
            {
                result.Add(AssemblyApp);
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
                Console.WriteLine("Framework {0};", UtilFramework.Version);

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
                        UtilFramework.ConsoleWriteLineColor($"Execute Framework CLI Command ({command.Name})", ConsoleColor.Yellow);
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
        protected virtual internal MetaSqlSchema[] GenerateFilter(MetaSqlSchema[] list)
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
                UtilFramework.ConsoleWriteLineError(exception);
                Environment.ExitCode = 1; // echo %errorlevel%
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.WriteLine("Press enter...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Override this method to return application BuiltIn rows. Used by cli for deployDb command to deploy BuiltIn rows to database.
        /// </summary>
        protected virtual List<DeployDbBuiltInItem> DeployDbBuiltInList()
        {
            var result = new List<DeployDbBuiltInItem>();
            return result;
        }

        /// <summary>
        /// Group of BuiltIn TypeRow.
        /// </summary>
        public class DeployDbBuiltInItem
        {
            /// <summary>
            /// Gets or sets RowList. Items have to be all of same TypeRow.
            /// </summary>
            public List<Row> RowList;

            /// <summary>
            /// Gets or sets FieldNameKeyList. Sql unique index for upsert.
            /// </summary>
            public string[] FieldNameKeyList;

            /// <summary>
            /// Gets or sets TableNameSqlReferencePrefex. Used to find reference tables.
            /// </summary>
            public string TableNameSqlReferencePrefex;
        }

        /// <summary>
        /// Returns BuiltIn rows to deploy to sql database.
        /// </summary>
        protected virtual internal List<DeployDbBuiltInItem> DeployDbBuiltInListInternal()
        {
            var result = new List<DeployDbBuiltInItem>();

            // FrameworkConfigGridBuiltIn
            {
                var item = new DeployDbBuiltInItem();
                result.Add(item);
                item.RowList = new List<Row>(FrameworkConfigGridBuiltInCli.List);
                item.FieldNameKeyList = new string[] { "TableId", "ConfigName" };
                item.TableNameSqlReferencePrefex = "Framework";
            }

            // FrameworkConfigFieldBuiltIn
            {
                var item = new DeployDbBuiltInItem();
                result.Add(item);
                item.RowList = new List<Row>(FrameworkConfigFieldBuiltInCli.List);
                item.FieldNameKeyList = new string[] { "ConfigGridId", "FieldId" };
                item.TableNameSqlReferencePrefex = "Framework";
            }

            result.AddRange(DeployDbBuiltInList());
            return result;
        }
    }
}
