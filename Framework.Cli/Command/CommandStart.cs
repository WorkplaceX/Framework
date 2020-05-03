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
            // Build angular client
            if (!UtilCli.FolderNameExist(UtilFramework.FolderName + "Application.Server/Framework/Framework.Angular/"))
            {
                var commandBuild = new CommandBuild(AppCli);
                UtilCli.OptionSet(ref commandBuild.OptionClientOnly, true);
                commandBuild.Execute();
            }

            if (optionWatch.OptionGet())
            {
                ConfigCli configCli = ConfigCli.Load();

                var website = configCli.WebsiteList.First(item => item.FolderNameDist != null); // TODO choose if multiple
                
                string folderNameNpmBuilt = UtilFramework.FolderName + website.FolderNameNpmBuild;
                string folderNameDist = UtilFramework.FolderName + website.FolderNameDist;
                
                string folderNameAngular = UtilFramework.FolderName + "Framework/Framework.Angular/application/";
                string folderNameCustomComponent = UtilFramework.FolderName + "Application.Website/Shared/CustomComponent/";

                UtilCli.ConsoleWriteLineColor("Port: http://localhost:4200/", System.ConsoleColor.Green);
                UtilCli.ConsoleWriteLineColor("Website: " + folderNameNpmBuilt, System.ConsoleColor.Green);
                UtilCli.ConsoleWriteLineColor("CustomComponent: " + folderNameCustomComponent, System.ConsoleColor.Green);
                UtilCli.ConsoleWriteLineColor("Framework: " + folderNameAngular, System.ConsoleColor.Green);

                FileSync fileSync = new FileSync();
                fileSync.AddFolder(folderNameDist, folderNameAngular + "src/Application.Website/Default/"); // TODO
                fileSync.AddFolder(folderNameCustomComponent, folderNameAngular + "src/Application.Website/Shared/CustomComponent/");

                UtilCli.Npm(folderNameNpmBuilt, "run build -- --watch", isWait: false);
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
        private readonly Dictionary<string, string> folderNameList = new Dictionary<string, string>();

        public void AddFolder(string folderNameSource, string folderNameDest)
        {
            folderNameList.Add(folderNameSource, folderNameDest);
            var watcher = new FileSystemWatcher
            {
                Path = folderNameSource,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.*"
            };
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
