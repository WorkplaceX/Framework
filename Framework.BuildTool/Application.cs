namespace Framework.BuildTool
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
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
                UtilFramework.LogError(UtilFramework.ExceptionToText(exception));
                Environment.Exit(1); // echo Exit Code is %errorlevel%
            }
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
            }
        }

        protected virtual void RegisterCommand(List<Command> commandList)
        {

        }

        private void RegisterCommand(CommandLineApplication commandLineApplication)
        {
            List<Command> commandList = new List<Command>();
            commandList.Add(new CommandConnectionString());
            commandList.Add(new CommandCheck());
            commandList.Add(new CommandOpen());
            commandList.Add(new CommandToggleIsDebugDataJson());
            commandList.Add(new CommandServe());
            commandList.Add(new CommandUnitTest());
            commandList.Add(new CommandRunSql());
            commandList.Add(new CommandGenerate());
            commandList.Add(new CommandBuildClient());
            commandList.Add(new CommandInstallAll());
            RegisterCommand(commandList);
            foreach (Command command in commandList)
            {
                Command.Register(commandLineApplication, command);
            }
        }
    }
}
