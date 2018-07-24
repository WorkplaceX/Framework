namespace Framework.Cli
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Command line interface application.
    /// </summary>
    public class AppCliBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppCliBase()
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
                Console.WriteLine("Version={0}; {1}", UtilFramework.VersionServer, UtilFramework.VersionClient);

                commandLineApplication.Execute("-h"); // Show list of available commands.
            }
        }

        /// <summary>
        /// Override to register new commands.
        /// </summary>
        protected virtual void RegisterCommand()
        {
            new CommandBuild(this);
            new CommandStart(this);
            new CommandDeploy(this);
        }

        private void RegisterCommandInit()
        {
            foreach (CommandBase command in CommandList)
            {
                commandLineApplication.Command(command.Name, (configuration) =>
                {
                    configuration.Description = command.Description;
                    command.Register(configuration);
                    configuration.OnExecute(() =>
                    {
                        int result = 0;
                        try
                        {
                            UtilFramework.ConsoleWriteLine($"Execute Framework CLI Command ({command.Name})", ConsoleColor.Yellow);
                            command.Execute();
                        }
                        catch (Exception exception)
                        {
                            result = 1;
                            Console.WriteLine(exception);
                        }
                        return result;
                    });

                    configuration.HelpOption("-h | --help"); // Command help (to show arguments and options)
                });
            }
            commandLineApplication.HelpOption("-h | --help"); // Command line interface help (to show commands)
        }

        /// <summary>
        /// Run command line interface.
        /// </summary>
        public void Run(string[] args)
        {
            Title(args);

            commandLineApplication.Execute(args);

            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }
    }

    public class CommandBase
    {
        public CommandBase(AppCliBase appCli, string name, string description)
        {
            this.AppCli = appCli;
            this.AppCli.CommandList.Add(this);
            this.Name = name;
            this.Description = description;
        }

        public readonly AppCliBase AppCli;

        public readonly string Name;

        public readonly string Description;

        protected virtual internal void Execute()
        {

        }

        /// <summary>
        /// Override to register command arguments and options.
        /// For example: configuration.Option("-a", "Build all.", CommandOptionType.NoValue);
        /// </summary>
        protected virtual internal void Register(CommandLineApplication configuration)
        {

        }
    }
}
