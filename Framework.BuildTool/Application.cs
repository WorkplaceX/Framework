namespace Framework.BuildTool
{
    using Database.dbo;
    using Framework.Application;
    using Framework.Application.Setup;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class AppBuildTool
    {
        public AppBuildTool(App app)
        {
            this.App = app;
        }

        /// <summary>
        /// Gets App. Used for TypeRowInAssembly.
        /// </summary>
        public readonly App App;

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
        }

        /// <summary>
        /// Override to register additional application specific commands or clear command list.
        /// </summary>
        protected virtual void RegisterCommand(List<Command> result)
        {

        }

        /// <summary>
        /// Override to register application on table FrameworkApplication.
        /// </summary>
        protected virtual void DbFrameworkApplicationView(List<FrameworkApplicationView> result)
        {

        }

        internal List<FrameworkApplicationView> DbFrameworkApplicationView()
        {
            List<FrameworkApplicationView> result = new List<FrameworkApplicationView>();
            result.Add(new FrameworkApplicationView() { Name = "Setup", Path = "setup", IsActive = true, Type = UtilFramework.TypeToName(typeof(AppSetup)) });
            DbFrameworkApplicationView(result);
            return result;
        }

        private void RegisterCommand(CommandLineApplication commandLineApplication)
        {
            List<Command> result = new List<Command>();
            result.Add(new CommandConnectionString());
            result.Add(new CommandCheck());
            result.Add(new CommandOpen());
            result.Add(new CommandServe());
            result.Add(new CommandUnitTest());
            result.Add(new CommandRunSql());
            result.Add(new CommandRunSqlTable(this));
            result.Add(new CommandGenerate());
            result.Add(new CommandBuildClient());
            result.Add(new CommandInstallAll());
            RegisterCommand(result);
            foreach (Command command in result)
            {
                Command.Register(commandLineApplication, command);
            }
        }
    }
}
