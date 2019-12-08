namespace Framework.Cli.Command
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Cli start command.
    /// </summary>
    public class CommandStart : CommandBase
    {
        public CommandStart(AppCli appCli)
            : base(appCli, "start", "Start server and open browser")
        {

        }

        protected internal override void Execute()
        {
            CommandBuild.InitConfigWebServer(AppCli); // Copy ConnectionString from ConfigCli.json to ConfigWebServer.json.

            string folderName = UtilFramework.FolderName + @"Application.Server/";
            UtilCli.VersionBuild(() =>
            {
                UtilCli.DotNet(folderName, "build");
            });
            UtilCli.DotNet(folderName, "run --no-build", false);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UtilCli.OpenWebBrowser("http://localhost:50919/"); // For port setting see also: Application.Server\Properties\launchSettings.json (applicationUrl, sslPort)
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Ubuntu list all running processes: 'ps'
                // To reboot Ubuntu type on Windows command prompt: 'wsl -t Ubuntu-18.04'
                // Ubuntu show processes tool: 'htop'
                UtilCli.ConsoleWriteLineColor("Stop server with command: 'killall -SIGKILL Application.Server node dotnet'", System.ConsoleColor.Yellow);
            }

        }
    }
}
