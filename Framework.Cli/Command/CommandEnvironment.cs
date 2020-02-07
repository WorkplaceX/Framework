namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    public class CommandEnvironment : CommandBase
    {
        public CommandEnvironment(AppCli appCli)
            : base(appCli, "env", "Select current environment to use for all cli commands")
        {

        }

        private CommandArgument argumentName;

        protected internal override void Register(CommandLineApplication configuration)
        {
            argumentName = configuration.Argument("name", "Get or set current environment name (dev, test, prod)");
        }


        protected internal override void Execute()
        {
            ConfigCli.Init(AppCli);
            ConfigCli configCli = ConfigCli.Load();

            if (UtilCli.ArgumentValueIsExist(this, argumentName))
            {
                if (UtilCli.ArgumentValue(this, argumentName, out string name))
                {
                    configCli.EnvironmentName = name?.ToUpper();
                }
            }

            configCli.EnvironmentGet();
            UtilCli.ConsoleWriteLineColor(string.Format("Current EnvironmentName={0}", configCli.EnvironmentNameGet()), ConsoleColor.Green);

            ConfigCli.Save(configCli);
            CommandBuild.InitConfigWebServer(AppCli); // Copy ConnectionString from ConfigCli.json to ConfigWebServer.json.
        }
    }
}
