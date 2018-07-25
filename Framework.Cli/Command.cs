using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;

namespace Framework.Cli
{
    /// <summary>
    /// Cli build command.
    /// </summary>
    public class CommandBuild : CommandBase
    {
        public CommandBuild(AppCliBase appCli)
            : base(appCli, "build", "Build client and server")
        {

        }

        protected internal override void Execute()
        {
            string folderName = UtilFramework.FolderName + @"Application.Server/";
            UtilCli.DotNet(folderName, "build");
        }
    }

    /// <summary>
    /// Cli start command.
    /// </summary>
    public class CommandStart : CommandBase
    {
        public CommandStart(AppCliBase appCli)
            : base(appCli, "start", "Start server and open browser")
        {

        }

        protected internal override void Execute()
        {
            string folderName = UtilFramework.FolderName + @"Application.Server/";
            UtilCli.DotNet(folderName, "build");
            UtilCli.DotNet(folderName, "run --no-build", false);
            string d = UtilFramework.FolderName;
            UtilCli.OpenWebBrowser("http://localhost:56093/"); // For port setting see also: Application.Server\Properties\launchSettings.json
        }
    }

    public class CommandDeploy : CommandBase
    {
        public CommandDeploy(AppCliBase appCli) 
            : base(appCli, "deploy", "Deploy to Azure git")
        {

        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();
            string azureGitUrl = configCli.AzureGitUrl;
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp2.0/publish/";

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");

            UtilCli.DotNet(folderName, "publish");
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Publish failed!");
        }
    }
}
