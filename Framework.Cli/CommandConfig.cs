namespace Framework.Cli
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    public class CommandConfig : CommandBase
    {
        public CommandConfig(AppCliBase appCli)
            : base(appCli, "config", "Read and write configuration")
        {

        }

        private CommandArgument azureGitUrlArgument;

        private CommandOption getOption;

        protected internal override void Register(CommandLineApplication configuration)
        {
            azureGitUrlArgument = configuration.Argument("azureGitUrl", "Set Azure git url");
            getOption = configuration.Option("-g | --get", "Get value", CommandOptionType.NoValue);
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();
            if (azureGitUrlArgument.Value != null)
            {
                if (getOption.Value() == "on")
                {
                    Console.WriteLine(azureGitUrlArgument.Name + "=" + configCli.AzureGitUrl);
                }
                else
                {
                    string value = UtilCli.ArgumentValue(azureGitUrlArgument);
                    configCli.AzureGitUrl = value;
                    ConfigCli.Save(configCli);
                }
            }
        }
    }
}
