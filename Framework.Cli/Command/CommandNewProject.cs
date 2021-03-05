using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Framework.Cli.Command
{
    /// <summary>
    /// Cli command to create a new project from template.
    /// </summary>
    internal class CommandNewProject : CommandBase
    {
        public CommandNewProject(AppCli appCli)
            : base(appCli, "new", "Create new project")
        {

        }

        private static void FolderCopy(string folderNameSource, string folderNameDest)
        {
            // Copy ApplicationHelloWorld template
            // Console.WriteLine(string.Format("Copy {0} to {1}", folderNameSource, folderNameDest));
            UtilCli.FolderCopy(folderNameSource, folderNameDest);
        }

        protected internal override void Execute()
        {
            Uri baseUri = new Uri(typeof(CommandNewProject).Assembly.Location);
            string folderNameFramework = new Uri(baseUri, "../../../../").AbsolutePath;

            string folderNameApplicationHelloWorld = folderNameFramework + "Framework.Cli/Template/ApplicationHelloWorld/";

            string folderNameSource = folderNameApplicationHelloWorld;
            string folderNameDest = Directory.GetCurrentDirectory().Replace(@"\", "/") + "/";

            // Console.WriteLine("Source=" + folderNameSource);
            // Console.WriteLine("Dest=" + folderNameDest);

            // Check dest folder is empty
            if (Directory.GetFileSystemEntries(folderNameDest).Length > 0)
            {
                UtilCli.ConsoleWriteLineColor("This folder needs to be empty!", ConsoleColor.Red);
                return;
            }

            Console.WriteLine("Installing...");

            // Copy ApplicationHelloWorld
            FolderCopy(folderNameSource, folderNameDest);

            // Copy Framework
            UtilCli.FolderCreate(folderNameDest + "Framework/");
            FolderCopy(folderNameFramework, folderNameDest + "Framework/");

            // Start new cli
            UtilCli.ConsoleWriteLineColor("Installation successfull!", ConsoleColor.Green);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UtilCli.ConsoleWriteLineColor("Start cli now with command .\\cli.cmd", ConsoleColor.DarkGreen);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                UtilCli.ConsoleWriteLineColor("Start cli now with command ./cli.sh", ConsoleColor.DarkGreen);
            }
        }
    }
}
