namespace Framework.Cli
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    public static class UtilCli
    {
        public static void Run(string[] args)
        {
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
        }
    }
}
