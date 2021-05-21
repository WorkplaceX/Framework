namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;

    /// <summary>
    /// Cli deploy command.
    /// </summary>
    internal class CommandDeploy : CommandBase
    {
        public CommandDeploy(AppCli appCli)
            : base(appCli, "deploy", "Deploy app to Azure git")
        {

        }

        protected internal override void Register(CommandLineApplication configuration)
        {
            OptionAzure = configuration.Option("-a|--azure", "Deploy to Azure.", CommandOptionType.NoValue);
            OptionLocal = configuration.Option("-l|--folder", "Deploy to local folder.", CommandOptionType.NoValue);
        }

        internal CommandOption OptionAzure;
        
        internal CommandOption OptionLocal;

        protected internal override void Execute()
        {
            CommandBuild.ConfigServerPublish();

            ConfigCli configCli = ConfigCli.Load();
            UtilCliInternal.ConsoleWriteLineColor("Information! Always run build command first in order to deploy latest version.", System.ConsoleColor.Cyan); // Information
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/net5.0/publish/";

            // Deploy Azure
            if (OptionAzure.OptionGet())
            {
                string deployAzureGitUrl = UtilFramework.StringNull(configCli.EnvironmentGet().DeployAzureGitUrl); // For example: "https://MyUsername:MyPassword@my22.scm.azurewebsites.net:443/my22.git"
                if (deployAzureGitUrl == null)
                {
                    UtilCliInternal.ConsoleWriteLineColor("Warning! " + nameof(ConfigCliEnvironment.DeployAzureGitUrl) + " not set! (" + configCli.EnvironmentName + ")", System.ConsoleColor.Yellow); // Warning
                }
                else
                {
                    string folderNamePublishGit = folderNamePublish + ".git";

                    UtilCliInternal.FolderDelete(folderNamePublishGit); // Undo git init.
                    UtilCliInternal.Start(folderNamePublish, "git", "init -b master"); // External system to push to.
                    UtilCliInternal.Start(folderNamePublish, "git", "config user.email \"deploy@deploy.deploy\""); // Prevent: Error "Please tell me who you are". See also: http://www.thecreativedev.com/solution-github-please-tell-me-who-you-are-error/
                    UtilCliInternal.Start(folderNamePublish, "git", "config user.name \"Deploy\"");
                    UtilCliInternal.Start(folderNamePublish, "git", "config core.autocrlf false"); // Prevent "LF will be replaced by CRLF" error in stderr.
                    UtilCliInternal.Start(folderNamePublish, "git", "add ."); // Can throw "LF will be replaced by CRLF".
                    UtilCliInternal.Start(folderNamePublish, "git", "commit -m Deploy");
                    UtilCliInternal.Start(folderNamePublish, "git", "remote add azure " + deployAzureGitUrl);
                    UtilCliInternal.Start(folderNamePublish, "git", "push azure master -f", isRedirectStdErr: true); // Do not write to stderr. Can be tested with "dotnet run -- deploy [DeployAzureGitUrl] 2>Error.txt"
                }
            }

            // Deploy local folder
            if (OptionLocal.OptionGet())
            {
                string deployLocalFolderName = UtilFramework.StringNull(configCli.EnvironmentGet().DeployLocalFolderName); // For example: "C:\Temp\Publish\"
                if (deployLocalFolderName == null)
                {
                    UtilCliInternal.ConsoleWriteLineColor("Warning! " + nameof(ConfigCliEnvironment.DeployLocalFolderName) + " not set! (" + configCli.EnvironmentName + ")", System.ConsoleColor.Yellow); // Warning
                }
                else
                {
                    UtilCliInternal.FolderDelete(deployLocalFolderName);
                    UtilCliInternal.FolderCopy(folderNamePublish, deployLocalFolderName);
                }
            }
        }
    }
}
