namespace Framework.BuildTool
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Command to run from CLI.
    /// </summary>
    public class Command
    {
        public Command(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets command name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Gets command description.
        /// </summary>
        public readonly string Description;

        private List<Argument> argumentList = new List<Argument>();

        /// <summary>
        /// Add a named argument (parameter) to pass the command.
        /// </summary>
        public Argument ArgumentAdd(string name, string description)
        {
            name = string.Format("[{0}]", name);
            Argument result = new Argument(name, description);
            argumentList.Add(result);
            return result;
        }

        private List<Option> optionList = new List<Option>();

        /// <summary>
        /// Add a command option switch.
        /// </summary>
        /// <param name="template">For example: "-g|--get"</param>
        public Option OptionAdd(string template, string description)
        {
            Option result = new Option(template, description);
            optionList.Add(result);
            return result;
        }

        /// <summary>
        /// Override this method to implement command execution code.
        /// </summary>
        public virtual void Run()
        {

        }

        public static void Register(CommandLineApplication commandLineApplication, Command command)
        {
            commandLineApplication.Command(command.Name, (configuration) =>
            {
                configuration.Description = command.Description;
                configuration.HelpOption("-h|--help");
                foreach (Argument argument in command.argumentList)
                {
                    CommandArgument commandArgument = configuration.Argument(argument.Name, argument.Description);
                    argument.Constructor(commandArgument);
                }
                foreach (Option option in command.optionList)
                {
                    CommandOption commandOption = configuration.Option(option.Tamplate, option.Description, CommandOptionType.NoValue);
                    option.Constructor(commandOption);
                }
                configuration.OnExecute(() =>
                {
                    command.Run();
                    return 0;
                });
            });
        }

        private static void CommandShortCutExecute(CommandLineApplication commandLineApplication, string commandShortCut)
        {
            try
            {
                commandLineApplication.Execute(commandShortCut.Split(' '));
            }
            catch (Exception exception)
            {
                if (Debugger.IsAttached)
                {
                    UtilFramework.LogColor(ConsoleColor.Red);
                    string exceptionText = UtilFramework.ExceptionToText(exception);
                    UtilFramework.Log(exceptionText);
                    UtilFramework.LogColorDefault();
                }
            }
        }

        internal static CommandLineApplication CommandLineApplicationCreate(List<string> commandShortCutList)
        {
            var result = new CommandLineApplication();
            result.Name = "BuildTool";
            result.HelpOption("-h|--help"); 
            result.OnExecute(() => 
            {
                Console.WindowHeight = 36;
                UtilFramework.LogColor(ConsoleColor.Blue);
                UtilFramework.Log(@"  ____            _   _       _   _____                   _      ____   _       ___ ");
                UtilFramework.Log(@" | __ )   _   _  (_) | |   __| | |_   _|   ___     ___   | |    / ___| | |     |_ _|");
                UtilFramework.Log(@" |  _ \  | | | | | | | |  / _` |   | |    / _ \   / _ \  | |   | |     | |      | | ");
                UtilFramework.Log(@" | |_) | | |_| | | | | | | (_| |   | |   | (_) | | (_) | | |   | |___  | |___   | | ");
                UtilFramework.Log(@" |____/   \__,_| |_| |_|  \__,_|   |_|    \___/   \___/  |_|    \____| |_____| |___|");
                UtilFramework.LogColorDefault();
                // Default function, when no arguments passed from CLI.
                //
                result.Execute("-h"); // Show list of available commands.
                UtilFramework.Log("");
                //
                UtilFramework.Log("ShortCut:");
                for (int i = 0; i < commandShortCutList.Count; i++)
                {
                    UtilFramework.Log(string.Format("{0}={1}", i + 1, commandShortCutList[i]));
                }
                Console.Write(">");
                string line = Console.ReadLine(); // Read from command line.
                bool isFind = false;
                for (int i = 0; i < commandShortCutList.Count; i++)
                {
                    if (line == (i + 1).ToString())
                    {
                        isFind = true;
                        CommandShortCutExecute(result, commandShortCutList[i]);
                        break;
                    }
                }
                if (isFind == false)
                {
                    CommandShortCutExecute(result, line);
                }
                Console.Write("Press Enter...");
                Console.ReadLine();
                return 0;
            });
            return result;
        }
    }

    /// <summary>
    /// Command named argument (parameter) to pass from CLI.
    /// </summary>
    public class Argument
    {
        internal Argument(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        internal void Constructor(CommandArgument commandArgument)
        {
            this.commandArgument = commandArgument;
        }

        private CommandArgument commandArgument { get; set; }

        public readonly string Name;

        public readonly string Description;

        public string Value
        {
            get
            {
                return commandArgument.Value;
            }
        }
    }

    /// <summary>
    /// Command option switch to pass from CLI. For example: "-g|--get"
    /// </summary>
    public class Option
    {
        internal Option(string template, string description)
        {
            this.Tamplate = template;
            this.Description = description;
        }

        internal void Constructor(CommandOption commandOption)
        {
            this.commandOption = commandOption;
        }

        private CommandOption commandOption { get; set; }

        public readonly string Tamplate;

        public readonly string Description;

        public bool IsOn
        {
            get
            {
                return commandOption.Value() == "on";
            }
        }
    }
}
