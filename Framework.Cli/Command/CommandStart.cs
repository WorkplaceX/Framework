namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Cli start command.
    /// </summary>
    internal class CommandStart : CommandBase
    {
        public CommandStart(AppCli appCli)
            : base(appCli, "start", "Start server and open client in browser")
        {

        }

        protected internal override void Execute()
        {
            // Build angular client
            var commandBuild = new CommandBuild(AppCli);
            UtilCli.OptionSet(ref commandBuild.OptionClientOnly, true);
            commandBuild.Execute();

            string folderName = UtilFramework.FolderName + @"Application.Server/";
            // Version tag and build only .NET Core server.
            UtilCli.VersionBuild(() =>
            {
                UtilCli.DotNet(folderName, "build");
            });
            UtilCli.DotNet(folderName, "run --no-build", false);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UtilCli.OpenWebBrowser("http://localhost:5000/"); // For port setting see also: Application.Server\Properties\launchSettings.json (applicationUrl, sslPort)
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Ubuntu list all running processes: 'ps'
                // To reboot Ubuntu type on Windows command prompt: 'wsl -t Ubuntu-18.04'
                // Ubuntu show processes tool: 'htop'
                UtilCli.ConsoleWriteLineColor("Info: Stop server with command 'killall -g -SIGKILL Application.Server'", ConsoleColor.Cyan); // Info
            }
        }
    }
}
