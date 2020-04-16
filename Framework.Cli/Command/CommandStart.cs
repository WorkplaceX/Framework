namespace Framework.Cli.Command
{
    using Microsoft.Extensions.CommandLineUtils;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
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

        private CommandOption optionWatch;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionWatch = configuration.Option("--watch", "Start client only. With file watch.", CommandOptionType.NoValue);
        }

        protected internal override void Execute()
        {
            if (optionWatch.Value() == "on")
            {
                string folderNameAngular = UtilFramework.FolderName + "Framework/Framework.Angular/application/";
                string folderNameWebsiteDefault = UtilFramework.FolderName + "Application.Website/Default/"; // TODO choose if multiple
                string folderNameCustomComponent = UtilFramework.FolderName + "Application.Website/Shared/CustomComponent/";

                UtilCli.ConsoleWriteLineColor("Port: http://localhost:4200/", System.ConsoleColor.Green);
                UtilCli.ConsoleWriteLineColor("Website: " + folderNameWebsiteDefault, System.ConsoleColor.Green);
                UtilCli.ConsoleWriteLineColor("CustomComponent: " + folderNameCustomComponent, System.ConsoleColor.Green);
                UtilCli.ConsoleWriteLineColor("Framework: " + folderNameAngular, System.ConsoleColor.Green);

                FileSync fileSync = new FileSync();
                fileSync.AddFolder(folderNameWebsiteDefault + "dist/", folderNameAngular + "src/Application.Website/Default/");
                fileSync.AddFolder(folderNameCustomComponent, folderNameAngular + "src/Application.Website/Shared/CustomComponent/");

                UtilCli.Npm(folderNameWebsiteDefault, "run build -- --watch", isWait: false);
                UtilCli.Npm(folderNameAngular, "start", isWait: true);
            }
            else
            {
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

    internal class FileSync
    {
        /// <summary>
        /// (FolderNameSource, FolderNameDest).
        /// </summary>
        private Dictionary<string, string> folderNameList = new Dictionary<string, string>();

        public void AddFolder(string folderNameSource, string folderNameDest)
        {
            folderNameList.Add(folderNameSource, folderNameDest);
            var watcher = new FileSystemWatcher();
            watcher.Path = folderNameSource;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            watcher.Changed += Changed;
            watcher.EnableRaisingEvents = true;
        }

        private bool isFileSync;

        private bool isChange;

        private void Changed(object sender, FileSystemEventArgs e)
        {
            if (isFileSync == false)
            {
                isFileSync = true; // Lock
                Task.Delay(100).ContinueWith((Task t) => { // Wait for further possible changes (debounce)
                    try
                    {
                        do
                        {
                            isChange = false;
                            UtilCli.ConsoleWriteLineColor("FileSync...", System.ConsoleColor.Green);
                            foreach (var item in folderNameList)
                            {
                                string folderNameSource = item.Key;
                                string folderNameDest = item.Value;
                                // UtilCli.FolderDelete(folderNameDest);
                                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                            }
                        } while (isChange); // Change happened while syncing
                    }
                    finally
                    {
                        isFileSync = false;
                    }
                });
            }
            else
            {
                isChange = true;
            }
        }
    }
}
