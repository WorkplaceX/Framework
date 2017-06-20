namespace Framework.BuildTool
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Diagnostics;

    public class AppBuildTool
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
            Command.Register(commandLineApplication, new CommandToggleIsDebugDataJson());
            Command.Register(commandLineApplication, new CommandServe());
            Command.Register(commandLineApplication, new CommandUnitTest());
            Command.Register(commandLineApplication, new CommandRunSql());
            Command.Register(commandLineApplication, new CommandGenerate());
            Command.Register(commandLineApplication, new CommandRunGulp());
            Command.Register(commandLineApplication, new CommandInstallAll());
        }
    }
}
