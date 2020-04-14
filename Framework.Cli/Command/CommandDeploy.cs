namespace Framework.Cli.Command
{
    using Framework.Cli.Config;

    /// <summary>
    /// Cli deploy command.
    /// </summary>
    internal class CommandDeploy : CommandBase
    {
        public CommandDeploy(AppCli appCli)
            : base(appCli, "deploy", "Deploy app to Azure git")
        {

        }

        protected internal override void Execute()
        {
            // Make sure deploy has latest ConfigCli.json data.
            CommandBuild.InitConfigServer(AppCli); // Copy ConnectionString from ConfigCli.json to ConfigServer.json.
            CommandBuild.ConfigServerPublish();

            ConfigCli configCli = ConfigCli.Load();
            string deployAzureGitUrl = UtilFramework.StringNull(configCli.EnvironmentGet().DeployAzureGitUrl); // For example: "https://MyUsername:MyPassword@my22.scm.azurewebsites.net:443/my22.git"
            if (deployAzureGitUrl == null)
            {
                UtilCli.ConsoleWriteLineColor(nameof(ConfigCliEnvironment.DeployAzureGitUrl) + " not set!", System.ConsoleColor.Green);
            }
            else
            {
                string folderName = UtilFramework.FolderName + "Application.Server/";
                string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp3.1/publish/";
                string folderNamePublishGit = folderNamePublish + ".git";

                UtilCli.FolderDelete(folderNamePublishGit); // Undo git init.
                UtilCli.Start(folderNamePublish, "git", "init");
                UtilCli.Start(folderNamePublish, "git", "config user.email \"deploy@deploy.deploy\""); // Prevent: Error "Please tell me who you are". See also: http://www.thecreativedev.com/solution-github-please-tell-me-who-you-are-error/
                UtilCli.Start(folderNamePublish, "git", "config user.name \"Deploy\"");
                UtilCli.Start(folderNamePublish, "git", "config core.autocrlf false"); // Prevent "LF will be replaced by CRLF" error in stderr.
                UtilCli.Start(folderNamePublish, "git", "add ."); // Can throw "LF will be replaced by CRLF".
                UtilCli.Start(folderNamePublish, "git", "commit -m Deploy");
                UtilCli.Start(folderNamePublish, "git", "remote add azure " + deployAzureGitUrl);
                UtilCli.Start(folderNamePublish, "git", "push azure master -f", isRedirectStdErr: true); // Do not write to stderr. Can be tested with "dotnet run -- deploy [DeployAzureGitUrl] 2>Error.txt"
            }
        }
    }
}
