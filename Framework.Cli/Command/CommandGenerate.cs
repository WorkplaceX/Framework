namespace Framework.Cli.Command
{
    using Framework.Cli.Generate;
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    public class CommandGenerate : CommandBase
    {
        public CommandGenerate(AppCli appCli)
            : base(appCli, "generate", "Generate CSharp code classes from database schema")
        {

        }

        private CommandOption optionFramework;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionFramework = configuration.Option("-f|--framework", "Generate CSharp code for framework (internal use only)", CommandOptionType.NoValue);
        }

        private void ArgumentGenerate()
        {
        }

        protected internal override void Execute()
        {
            CommandBuild.InitConfigWebServer(AppCli); // Copy ConnectionString from ConfigCli.json to ConfigWebServer.json. Command reads ConnectionString from ConfigWebServer.json.

            bool isFrameworkDb = optionFramework.Value() == "on";
            Script.Run(isFrameworkDb, AppCli);

            Console.WriteLine("Generate successful!");
        }
    }
}
