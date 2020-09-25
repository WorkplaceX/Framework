namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using static Framework.Cli.AppCli;

    /// <summary>
    /// Cli start command.
    /// </summary>
    internal class CommandExternal : CommandBase
    {
        public CommandExternal(AppCli appCli)
            : base(appCli, "external", "Run external prebuild .NET script.")
        {

        }

        protected internal override void Execute()
        {
            UtilFramework.Assert(UtilFramework.FolderName.StartsWith(UtilFramework.FolderNameExternal));

            // ExternalGit/ProjectName/
            string externalGitProjectNamePath = UtilFramework.FolderName.Substring(UtilFramework.FolderNameExternal.Length);

            // Application/App/
            string appSourceFolderName = UtilFramework.FolderName + "Application/App/";
            string appDestFolderName = UtilFramework.FolderNameExternal + "Application/App/" + externalGitProjectNamePath;

            // Application.Database/Database/
            string databaseSourceFolderName = UtilFramework.FolderName + "Application.Database/Database/";
            string databaseDestFolderName = UtilFramework.FolderNameExternal + "Application.Database/Database/" + externalGitProjectNamePath;

            // Application.Website/
            string websiteSourceFolderName = UtilFramework.FolderName + "Application.Website/";
            string websiteDestFolderName = UtilFramework.FolderNameExternal + "Application.Website/" + externalGitProjectNamePath;

            // Application.Cli/App/
            string cliAppSourceFolderName = UtilFramework.FolderName + "Application.Cli/App/";
            string cliAppDestFolderName = UtilFramework.FolderNameExternal + "Application.Cli/App/" + externalGitProjectNamePath;

            // Application.Cli/App/
            string cliDatabaseSourceFolderName = UtilFramework.FolderName + "Application.Cli/Database/";
            string cliDatabaseDestFolderName = UtilFramework.FolderNameExternal + "Application.Cli/Database/" + externalGitProjectNamePath;

            // Application.Cli/DeployDb/
            string cliDeployDbSourceFolderName = UtilFramework.FolderName + "Application.Cli/DeployDb/";
            string cliDeployDbDestFolderName = UtilFramework.FolderNameExternal + "Application.Cli/DeployDb/" + externalGitProjectNamePath;

            // Angular
            string websiteAngularDestFolderName = UtilFramework.FolderNameExternal + "Framework/Framework.Angular/application/src/Application.Website/";

            var args = new ExternalPrebuildArgs {
                AppSourceFolderName = appSourceFolderName,
                AppDestFolderName = appDestFolderName,
                DatabaseSourceFolderName = databaseSourceFolderName,
                DatabaseDestFolderName = databaseDestFolderName,
                WebsiteSourceFolderName = websiteSourceFolderName,
                WebsiteDestFolderName = websiteDestFolderName,
                CliAppSourceFolderName = cliAppSourceFolderName,
                CliAppDestFolderName = cliAppDestFolderName,
                CliDatabaseSourceFolderName = cliDatabaseSourceFolderName,
                CliDatabaseDestFolderName = cliDatabaseDestFolderName,
                CliDeployDbSourceFolderName = cliDeployDbSourceFolderName,
                CliDeployDbDestFolderName = cliDeployDbDestFolderName,
                WebsiteAngularDestFolderName = websiteAngularDestFolderName 
            };
            AppCli.CommandExternal(args);
        }
    }
}
