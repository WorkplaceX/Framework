namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Framework.Cli.Generate;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Command line interface application.
    /// </summary>
    public class AppCli
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppCli()
        {
            this.commandLineApplication = new CommandLineApplication();

            RegisterCommand();
            RegisterCommandInit();
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
                Console.WriteLine("Version={0};", UtilFramework.Version);

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
    }
}
