namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
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
                {
                    ConfigCli configCli = ConfigCli.Load();

                    Console.WriteLine("Select Website:");

                    for (int i = 0; i < configCli.WebsiteList.Count; i++)
                    {
                        Console.WriteLine(string.Format("{0}={1}", i + 1, configCli.WebsiteList[i].FolderNameNpmBuild));
                    }

                    Console.Write("Website:");

                    var websiteIndex = int.Parse(Console.ReadLine()) - 1;

                    var website = configCli.WebsiteList[websiteIndex];

                    string folderNameNpmBuilt = UtilFramework.FolderName + website.FolderNameNpmBuild;
                    string folderNameDist = UtilFramework.FolderName + website.FolderNameDist;

                    string folderNameAngular = UtilFramework.FolderName + "Framework/Framework.Angular/application/";
                    string folderNameCustomComponent = UtilFramework.FolderName + "Application.Website/Shared/CustomComponent/";

                    Console.WriteLine("Copy folder dist/");
                    UtilCli.FolderCopy(folderNameDist, folderNameAngular + "src/Application.Website/dist/", "*.*", true);

                    Console.WriteLine("Copy folder CustomComponent/");
                    UtilCli.FolderCopy(folderNameCustomComponent, folderNameAngular + "src/Application.Website/Shared/CustomComponent/", "*.*", true);

                    bool isFileSync = UtilCli.ConsoleReadYesNo("Start FileSync?");
                    bool isWebsiteWatch = UtilCli.ConsoleReadYesNo("Start Website watch?");
                    bool isAngular = UtilCli.ConsoleReadYesNo("Start Angular?");
                    bool isServer = UtilCli.ConsoleReadYesNo("Start Server?");

                    // FileSync
                    if (isFileSync)
                    {
                        FileSync fileSync = new FileSync();
                        fileSync.AddFolder(folderNameDist, folderNameAngular + "src/Application.Website/dist/");
                        fileSync.AddFolder(folderNameAngular + "src/Application.Website/Shared/CustomComponent/", folderNameCustomComponent);
                    }

                    // Website --watch
                    if (isWebsiteWatch)
                    {
                        UtilCli.Npm(folderNameNpmBuilt, "run build -- --watch", isWait: false);
                    }

                    // Angular client
                    if (isAngular)
                    {
                        UtilCli.Npm(folderNameAngular, "start -- --disable-host-check", isWait: false); // disable-host-check to allow for example http://localhost2:4200/
                    }

                    // .NET Server
                    if (isServer)
                    {
                        string folderName = UtilFramework.FolderName + @"Application.Server/";
                        UtilCli.DotNet(folderName, "run", isWait: false);
                    }

                    void heartBeat()
                    {
                        Console.WriteLine();
                        Console.WriteLine(UtilFramework.DateTimeToString(DateTime.Now));
                        if (isAngular)
                        {
                            UtilCli.ConsoleWriteLineColor("Angular: http://" + website.DomainNameList.First().DomainName + ":4200/", System.ConsoleColor.Green);
                        }
                        if (isServer)
                        {
                            UtilCli.ConsoleWriteLineColor("Server: http://" + website.DomainNameList.First().DomainName + ":50919/", System.ConsoleColor.Green);
                        }
                        if (isFileSync)
                        {
                            UtilCli.ConsoleWriteLineColor("Modify Website: " + folderNameNpmBuilt, System.ConsoleColor.Green);
                            UtilCli.ConsoleWriteLineColor("Modify CustomComponent: " + folderNameCustomComponent, System.ConsoleColor.Green);
                        }
                        if (isAngular)
                        {
                            UtilCli.ConsoleWriteLineColor("Modify Angular: " + folderNameAngular, System.ConsoleColor.Green);
                        }
                        Task.Delay(5000).ContinueWith((Task task) => heartBeat());
                    }

                    if (isFileSync || isWebsiteWatch || isAngular || isServer)
                    {
                        heartBeat();
                        while (true)
                        {
                            Console.ReadLine(); // Program would end and with it FileSync
                        }
                    }
                }
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
                    UtilCli.ConsoleWriteLineColor("Stop server with command 'killall -SIGKILL Application.Server node dotnet'", System.ConsoleColor.Yellow);
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
            Console.WriteLine("DETECT!");
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
