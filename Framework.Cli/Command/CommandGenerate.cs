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
        
        private CommandOption optionOnly;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionFramework = configuration.Option("-f|--framework", "Generate CSharp code for framework (internal use only)", CommandOptionType.NoValue);
            optionSilent = configuration.Option("-s|--silent", "No command line user interaction.", CommandOptionType.NoValue);
            optionOnly = configuration.Option("-o|--only", "Do not run integrate program.", CommandOptionType.NoValue);
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();

            if (optionSilent.OptionGet() == false && configCli.EnvironmentNameGet() != "DEV")
            {
                if (UtilCliInternal.ConsoleReadYesNo(string.Format("Generate CSharp code from {0} database?", configCli.EnvironmentName)) == false)
                {
                    return;
                }
            }

            bool isFrameworkDb = optionFramework.OptionGet();
            if (Script.Run(isFrameworkDb, AppCli, optionOnly.OptionGet()))
            {
                UtilCliInternal.ConsoleWriteLineColor("Generate successful!", ConsoleColor.Green);
            }
        }
    }
}
