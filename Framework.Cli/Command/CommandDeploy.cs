namespace Framework.Cli.Command
{
    using Framework.Cli.Config;

    public class CommandDeploy : CommandBase
    {
        public CommandDeploy(AppCli appCli)
            : base(appCli, "deploy", "Deploy app to Azure git")
        {

        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();
            string deployAzureGitUrl = configCli.DeployAzureGitUrl; // For example: "https://MyUsername:MyPassword@my22.scm.azurewebsites.net:443/my22.git"
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp2.2/publish/";
            string folderNamePublishGit = folderNamePublish + ".git";

            UtilCli.FolderDelete(folderNamePublishGit); // Undo git init.
            UtilCli.Start(folderNamePublish, "git", "init");
            UtilCli.Start(folderNamePublish, "git", "config user.email \"deploy@deploy.deploy\""); // Prevent: Error "Please tell me who you are". See also: http://www.thecreativedev.com/solution-github-please-tell-me-who-you-are-error/
            UtilCli.Start(folderNamePublish, "git", "config user.name \"Deploy\"");
            UtilCli.Start(folderNamePublish, "git", "remote add azure " + deployAzureGitUrl);
            UtilCli.Start(folderNamePublish, "git", "fetch --all", isRedirectStdErr: true); // Another possibility is argument "-q" to do not write to stderr.
            UtilCli.Start(folderNamePublish, "git", "config core.autocrlf false"); // Prevent "LF will be replaced by CRLF" error in stderr.
            UtilCli.Start(folderNamePublish, "git", "add ."); // Can throw "LF will be replaced by CRLF".
            UtilCli.Start(folderNamePublish, "git", "commit -m Deploy");
            UtilCli.Start(folderNamePublish, "git", "push azure master -f", isRedirectStdErr: true); // Do not write to stderr. Can be tested with "dotnet run -- deploy [DeployAzureGitUrl] 2>Error.txt"
        }
    }
}
