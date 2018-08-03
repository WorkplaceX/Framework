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
                    try
                    {
                        configCli = UtilFramework.ConfigFromJson<ConfigCli>(json);
                    } catch (Exception exception)
                    {
                        throw new Exception("ConfigCliJson invalid!", exception);
                    }
                    ConfigCli.Save(configCli);
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

            // Read
            {
                Console.WriteLine("Add the following environment variable to ci build server (including quotation marks):");
                string json = UtilFramework.ConfigToJson(configCli);
                json = json.Replace("\"", "'"); // To use it in command prompt.
                Console.WriteLine("ConfigCliJson=\"{0}\"", json);
            }
        }
    }
}
