namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    /// <summary>
    /// Cli environment command.
    /// </summary>
    internal class CommandEnvironment : CommandBase
    {
        public CommandEnvironment(AppCli appCli)
            : base(appCli, "env", "Select current environment to use for all command cli")
        {

        }

        private CommandArgument argumentName;

        protected internal override void Register(CommandLineApplication configuration)
        {
            argumentName = configuration.Argument("name", "Get or set current environment name (dev, test, prod)");
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();
            string environmentNameOld = configCli.EnvironmentName;

            if (UtilCli.ArgumentValueIsExist(this, argumentName))
            {
                if (UtilCli.ArgumentValue(this, argumentName, out string name))
                {
                    configCli.EnvironmentName = name?.ToUpper();
                }
            }

            configCli.EnvironmentGet(); // Get or init

            if (configCli.EnvironmentName != environmentNameOld)
            {
                ConsoleWriteLineCurrentEnvironment(configCli);
            }

            ConfigCli.Save(configCli);
        }

        public static void ConsoleWriteLineCurrentEnvironment(ConfigCli configCli)
        {
            UtilCli.ConsoleWriteLineColor(string.Format("Current Environment (Name={0})", configCli.EnvironmentNameGet()), ConsoleColor.Green);
        }
    }
}
