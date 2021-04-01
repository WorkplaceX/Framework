using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Launch
{
    /// <summary>
    /// Launch Application.Cli from command prompt in a WorkplaceX root directory.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var folderName = Directory.GetCurrentDirectory();
            var folderNameExe = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Program)).Location);
            var fileNameCsproj = folderName + Path.DirectorySeparatorChar + "Application.Cli" + Path.DirectorySeparatorChar + "Application.Cli.csproj";

            if (!File.Exists(fileNameCsproj))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("This is not a WorkplaceX root directory! Expected (*.csproj) file does not exist. ({0})", fileNameCsproj));
                Console.ResetColor();
                return;
            }

            if (!FileWpxExist())
            {
                if (ConsoleReadYesNo("Add wpx command to user environment?"))
                {
                    var envPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                    envPath += folderNameExe;
                    // Add this folder to user environment PATH.
                    Environment.SetEnvironmentVariable("PATH", envPath, EnvironmentVariableTarget.User);

                    Console.WriteLine("Close window and open it again. The wpx command is now ready!");
                    return;
                }
            }

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "dotnet";
            processInfo.WorkingDirectory = folderName;
            string arguments = "run --project Application.Cli --";
            foreach (var item in args)
            {
                arguments += " " + item;
            }
            processInfo.Arguments = arguments;

            var process = Process.Start(processInfo);
            process.WaitForExit();
        }

        /// <summary>
        /// Returns true, if wpx.exe exists in one of the environment paths.
        /// </summary>
        static bool FileWpxExist()
        {
            bool result = false;
            var envPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            var envPathlist = new List<string>(envPath.Split(";"));
            foreach (var folderName in envPathlist)
            {
                string fileName = folderName + Path.DirectorySeparatorChar + "wpx.exe";
                if (File.Exists(fileName))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Wait for user interaction.
        /// </summary>
        static bool ConsoleReadYesNo(string text)
        {
            string consoleReadLine;
            do
            {
                Console.Write(text + " [y/n] ");
                consoleReadLine = Console.ReadLine().ToUpper();
            } while (!(consoleReadLine == "Y" || consoleReadLine == "N"));
            return consoleReadLine == "Y";
        }
    }
}
