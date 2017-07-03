namespace Framework.BuildTool
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;

    public class Command
    {
        public Command(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        public readonly string Name;

        public readonly string Description;

        private List<Argument> argumentList = new List<Argument>();

        public IReadOnlyList<Argument> ArgumentList
        {
            get
            {
                return argumentList;
            }
        }

        public Argument ArgumentAdd(string name, string description)
        {
            name = string.Format("[{0}]", name);
            Argument result = new Argument(name, description);
            argumentList.Add(result);
            return result;
        }

        private List<Option> optionList = new List<Option>();

        public Option OptionAdd(string template, string description)
        {
            Option result = new Option(template, description);
            optionList.Add(result);
            return result;
        }

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

        public static CommandLineApplication CommandLineApplicationCreate()
        {
            var result = new CommandLineApplication();
            result.Name = "Tool";
            result.HelpOption("-h|--help");
            result.OnExecute(() => {
                // Default function, when no arguments.
                result.Execute("-h");
                UtilFramework.Log("");
                UtilFramework.Log("ShortCut:");
                UtilFramework.Log("1=buildClient");
                UtilFramework.Log("2=serve -c");
                UtilFramework.Log("3=generate");
                string line = Console.ReadLine();
                if (line == "1")
                {
                    result.Execute("buildClient");
                }
                if (line == "2")
                {
                    result.Execute("serve", "-c");
                }
                if (line == "3")
                {
                    result.Execute("generate");
                }
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
                return 0;
            });
            return result;
        }
    }

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
