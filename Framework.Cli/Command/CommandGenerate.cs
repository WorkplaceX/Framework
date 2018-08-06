namespace Framework.Cli.Command
{
    using Framework.Cli.Generate;
    using Microsoft.Extensions.CommandLineUtils;

    public class CommandGenerate : CommandBase
    {
        public CommandGenerate(AppCliBase appCli)
            : base(appCli, "generate", "Generate CSharp code with database classes.")
        {

        }

        private CommandOption optionFramework;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionFramework = configuration.Option("-f|--framework", "Generate CSharp code for framework (internal use only).", CommandOptionType.NoValue);
        }

        private void ArgumentGenerate()
        {
        }

        protected internal override void Execute()
        {
            bool isFrameworkDb = optionFramework.Value() == "on";
            Script.Run(isFrameworkDb, AppCli);
        }
    }
}
