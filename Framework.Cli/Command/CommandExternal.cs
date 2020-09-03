namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
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
            string appSourceFolderName = UtilFramework.FolderName + "Application/App/";
            string appDestFolderName = UtilFramework.FolderNameExternal + "Application/App/ExternalGit/";
            string databaseSourceFolderName = UtilFramework.FolderName + "Application.Database/Database/";
            string databaseDestFolderName = UtilFramework.FolderNameExternal + "Application.Database/Database/ExternalGit/";
            string websiteSourceFolderName = UtilFramework.FolderName + "Application.Website/";
            string websiteDestFolderName = UtilFramework.FolderNameExternal + "Application.Website/ExternalGit/";
            string websiteAngularDestFolderName = UtilFramework.FolderNameExternal + "Framework/Framework.Angular/application/src/Application.Website/";
            var args = new ExternalPrebuildArgs {
                AppSourceFolderName = appSourceFolderName,
                AppDestFolderName = appDestFolderName,
                DatabaseSourceFolderName = databaseSourceFolderName,
                DatabaseDestFolderName = databaseDestFolderName,
                WebsiteSourceFolderName = websiteSourceFolderName,
                WebsiteDestFolderName = websiteDestFolderName,
                WebsiteAngularDestFolderName = websiteAngularDestFolderName 
            };
            AppCli.ExternalPrebuild(args);
        }
    }
}
