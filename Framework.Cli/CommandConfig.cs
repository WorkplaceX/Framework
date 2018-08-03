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

        private CommandArgument jsonArgument;

        private CommandArgument azureGitUrlArgument;

        protected internal override void Register(CommandLineApplication configuration)
        {
            jsonArgument = configuration.Argument("json", "Get or set configuration");
            azureGitUrlArgument = configuration.Argument("azureGitUrl", "Get or set Azure git url");
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();
            
            // Command "json"
            if (UtilCli.ArgumentValueIsExist(this, jsonArgument))
            {
                if (UtilCli.ArgumentValue(this, jsonArgument, out string json))
                {
                    // Write
                    configCli = UtilFramework.ConfigFromJson<ConfigCli>(json);
                    ConfigCli.Save(configCli);
                }
                else
                {
                    // Read
                    Console.WriteLine("ConfigCli.json for ci build server:");
                    json = UtilFramework.ConfigToJson(configCli);
                    json = json.Replace("\"", "'"); // To use it in command prompt.
                    Console.WriteLine(json);
                }
            }
            
            // Command "azureGitUrl"
            if (UtilCli.ArgumentValueIsExist(this, azureGitUrlArgument))
            {
                if (UtilCli.ArgumentValue(this, azureGitUrlArgument, out string value))
                {
                    // Write
                    configCli.AzureGitUrl = value;
                    ConfigCli.Save(configCli);
                }
                else
                {
                    // Read
                    Console.WriteLine(azureGitUrlArgument.Name + "=" + configCli.AzureGitUrl);
                }
            }
        }
    }
}
