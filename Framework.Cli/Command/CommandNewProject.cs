using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Framework.Cli.Command
{
    /// <summary>
    /// Cli command to create a new project from template.
    /// For debug run in an empty folder: "dotnet run --project C:\Temp\GitHub\ApplicationDoc\Framework\Framework.Cli -- new".
    /// Npm does not package file .gitignore. Copy all .gitignore to .gitignore.txt
    ///   1) Update version in file package.json
    ///   2) git commit
    ///   3) forfiles /S /M .gitignore /C "cmd /c copy @file @file.txt"
    ///   4) npm publish
    ///   5) git add .
    ///   6) git reset --hard
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

            string folderNameApplication = folderNameFramework + "Framework.Cli/Template/Application/ApplicationEmpty/";
            string folderNameApplicationWebsite = folderNameFramework + "Framework.Cli/Template/Application.Website/";

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

            Console.WriteLine("For details on how to answer see: https://www.workplacex.org/new-application/");
            var isCopyFramework = UtilCliInternal.ConsoleReadYesNo("Copy folder Framework? - (Otherwise git submodule is used)");
            var isCopyApplicationWebsite = UtilCliInternal.ConsoleReadYesNo("Copy folder Application.Website? - (otherwise framework website is used)");

            Console.WriteLine("Installing...");

            // Copy Application
            FolderCopy(folderNameSource, folderNameDest);

            if (isCopyFramework)
            {
                // Copy Framework
                FolderCopy(folderNameFramework, folderNameDest + "Framework/");
            }
            else
            {
                // Git Init
                Console.WriteLine("git init");
                var info = new ProcessStartInfo
                {
                    WorkingDirectory = folderNameDest,
                    FileName = "git",
                    Arguments = "init"
                };
                Process.Start(info).WaitForExit();

                // Git Submodule Add
                Console.WriteLine("git submodule add https://github.com/WorkplaceX/Framework.git");
                info = new ProcessStartInfo
                {
                    WorkingDirectory = folderNameDest,
                    FileName = "git",
                    Arguments = "submodule add https://github.com/WorkplaceX/Framework.git"
                };
                Process.Start(info).WaitForExit();
            }

            if (isCopyApplicationWebsite)
            {
                // Copy Application.Website
                FolderCopy(folderNameApplicationWebsite, folderNameDest + "Application.Website/");
            }

            // Rename all file .gitignore.txt to .gitignore
            var fileList = Directory.GetFiles(folderNameDest, ".gitignore.txt", SearchOption.AllDirectories);
            if (fileList.Length == 0)
            {
                throw new Exception("File .gitignore.txt not found!"); // Run 'forfiles /S /M .gitignore /C "cmd /c copy @file @file.txt"' before npm publish! npm does not publish .gitignore files!
            }
            foreach (var item in fileList)
            {
                var fileNameSource = item;
                var fileNameDest = fileNameSource.Substring(0, fileNameSource.Length - ".txt".Length);
                if (!File.Exists(fileNameDest))
                {
                    File.Move(fileNameSource, fileNameDest);
                }
                else
                {
                    File.Delete(fileNameSource);
                }
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
