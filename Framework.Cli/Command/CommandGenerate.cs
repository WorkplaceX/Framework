namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Framework.Cli.Generate;
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    /// <summary>
    /// Cli generate command to generate CSharp code.
    /// </summary>
    internal class CommandGenerate : CommandBase
    {
        public CommandGenerate(AppCli appCli)
            : base(appCli, "generate", "Generate CSharp code classes from database schema")
        {

        }

        private CommandOption optionFramework;

        private CommandOption optionSilent;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionFramework = configuration.Option("-f|--framework", "Generate CSharp code for framework (internal use only)", CommandOptionType.NoValue);
            optionSilent = configuration.Option("-s|--silent", "No command line user interaction.", CommandOptionType.NoValue);
        }

        protected internal override void Execute()
        {
            CommandBuild.InitConfigServer(AppCli); // Copy ConnectionString from ConfigCli.json to ConfigServer.json.

            ConfigCli configCli = ConfigCli.Load();
            CommandEnvironment.ConsoleWriteLineCurrentEnvironment(configCli);

            if (optionSilent.Value() != "on")
            {
                if (UtilCli.ConsoleReadYesNo("Generate?") == false)
                {
                    return;
                }
            }

            bool isFrameworkDb = optionFramework.Value() == "on";
            if (Script.Run(isFrameworkDb, AppCli))
            {
                Console.WriteLine("Generate successful!");
            }
        }
    }
}
