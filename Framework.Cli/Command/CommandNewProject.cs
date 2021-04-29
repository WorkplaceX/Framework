using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Framework.Cli.Command
{
    /// <summary>
    /// Cli command to create a new project from template.
    /// For debug run in an empty folder: "dotnet run --project C:\Temp\GitHub\ApplicationDoc\ExternalGit\ApplicationDoc\Framework\Framework.Cli -- new".
    /// </summary>
    internal class CommandNewProject : CommandBase
    {
        public CommandNewProject(AppCli appCli)
            : base(appCli, "new", "Create new project")
        {

        }

        private static void FolderCopy(string folderNameSource, string folderNameDest)
        {
            // Copy Application template
            // Console.WriteLine(string.Format("Copy {0} to {1}", folderNameSource, folderNameDest));
            UtilCliInternal.FolderCopy(folderNameSource, folderNameDest);
        }

        protected internal override void Execute()
        {
            Uri baseUri = new Uri(typeof(CommandNewProject).Assembly.Location);
            string folderNameFramework = new Uri(baseUri, "../../../../").AbsolutePath;

            string folderNameApplication = folderNameFramework + "Framework.Cli/Template/Application/ApplicationHelloWorld/";

            string folderNameSource = folderNameApplication;
            string folderNameDest = Directory.GetCurrentDirectory().Replace(@"\", "/") + "/";

            // Console.WriteLine("Source=" + folderNameSource);
            // Console.WriteLine("Dest=" + folderNameDest);

            // Check dest folder is empty
            var list = Directory.GetFileSystemEntries(folderNameDest);
            var isGitOnly = list.Length == 1 && list[0] == folderNameDest + ".git"; // Empty git folder
            if (list.Length > 0 && !isGitOnly)
            {
                UtilCliInternal.ConsoleWriteLineColor("This folder needs to be empty!", ConsoleColor.Red);
                return;
            }

            var isSubmodule = UtilCliInternal.ConsoleReadYesNo("For Framework use Submodule?");
            var isApplicationWebsite = UtilCliInternal.ConsoleReadYesNo("Copy folder Application.WebSite?");

            Console.WriteLine("Installing...");

            // Copy Application
            FolderCopy(folderNameSource, folderNameDest);

            if (isSubmodule)
            {
                var info = new ProcessStartInfo
                {
                    WorkingDirectory = folderNameDest,
                    FileName = "git",
                    Arguments = "submodule add https://github.com/WorkplaceX/Framework.git"
                };
                Process.Start(info).WaitForExit();
            }
            else
            {
                // Copy Framework
                UtilCliInternal.FolderCreate(folderNameDest + "Framework/");
                FolderCopy(folderNameFramework, folderNameDest + "Framework/");
            }

            // Start new cli
            UtilCliInternal.ConsoleWriteLineColor("Installation successfull!", ConsoleColor.Green);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UtilCliInternal.ConsoleWriteLineColor("Start cli now with command .\\wpx.cmd", ConsoleColor.DarkGreen);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                UtilCliInternal.ConsoleWriteLineColor("Start cli now with command ./wpx.sh", ConsoleColor.DarkGreen);
            }
        }
    }
}
