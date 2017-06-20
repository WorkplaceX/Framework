namespace Framework.Tool
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Diagnostics;

    public class ToolBase
    {
        public void Run(string[] args)
        {
            CommandLineApplication commandLineApplication = Command.CommandLineApplicationCreate();
            RegisterCommand(commandLineApplication);
            try
            {
                commandLineApplication.Execute(args);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
            if (Debugger.IsAttached)
            {
                Console.ReadLine();
            }
        }

        protected virtual void RegisterCommand(CommandLineApplication commandLineApplication)
        {
            Command.Register(commandLineApplication, new CommandConnectionString());
            Command.Register(commandLineApplication, new CommandCheck());
            Command.Register(commandLineApplication, new CommandOpen());
        }
    }
}
