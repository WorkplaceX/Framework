namespace Framework.Cli
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Diagnostics;

    public static class UtilCli
    {
        public static void Run(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Framework CLI");
            }

            CommandLineApplication commandLineApplication = new CommandLineApplication();

            // Command Build
            commandLineApplication.Command("Build", (command) => {
                command.Description = "Build Application";
                CommandOption commandOption = command.Option("-a", "Build server and client.", CommandOptionType.NoValue);
                // Execute
                command.OnExecute(() => {
                    Console.WriteLine("Build...");
                    if (commandOption.Value() == "on")
                    {
                        Console.WriteLine("All");
                    }
                    return 0;
                });
                command.HelpOption("-h | --help");
            });

            commandLineApplication.HelpOption("-h | --help");
            commandLineApplication.Execute(args);

            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }
    }
}
