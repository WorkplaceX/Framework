﻿namespace Framework.Cli.Command
{
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
}