using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

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
            var fileNameCsprojRelative = Path.DirectorySeparatorChar + "Application.Cli" + Path.DirectorySeparatorChar + "Application.Cli.csproj";
            var fileNameCsproj = folderName + fileNameCsprojRelative;

            // Does wpx command exist in any environment path?
            if (!FileWpxExist())
            {
                if (ConsoleReadYesNo("Add wpx command to environment?"))
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        // Linux
                        LinuxSetEnvironmentVariable(folderNameExe);
                    }
                    else
                    {
                        // Windows
                        var envPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
                        envPath += folderNameExe;
                        // Add this folder to user environment PATH.
                        Environment.SetEnvironmentVariable("PATH", envPath, EnvironmentVariableTarget.User);
                    }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Command wpx is now ready! Close window and open it again.");
                    Console.ResetColor();
                    return;
                }
            }

            // Is current directory a WorkplaceX root directory?
            if (!File.Exists(fileNameCsproj))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(string.Format("This is not a WorkplaceX root directory! Expected (*.csproj) file does not exist. ({0})", fileNameCsprojRelative));
                Console.ResetColor();
                return;
            }

            // Start Application.Cli
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

            // Windows, Linux
            var target = EnvironmentVariableTarget.User;
            string pathSeperator = ";";
            string fileNameWpx = "wpx.exe";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                target = EnvironmentVariableTarget.Process;
                pathSeperator = ":";
                fileNameWpx = "wpx";
            }

            // Check for wpx.exe in every path.
            var envPath = Environment.GetEnvironmentVariable("PATH", target);
            var envPathlist = new List<string>(envPath.Split(pathSeperator));
            foreach (var folderName in envPathlist)
            {
                string fileName = folderName + Path.DirectorySeparatorChar + fileNameWpx;
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
            Console.ForegroundColor = ConsoleColor.Green;
            string consoleReadLine;
            do
            {
                Console.Write(text + " [y/n] ");
                consoleReadLine = Console.ReadLine().ToUpper();
            } while (!(consoleReadLine == "Y" || consoleReadLine == "N"));
            Console.ResetColor();
            return consoleReadLine == "Y";
        }

        /// <summary>
        /// Add path to bash environment variable.
        /// </summary>
        static void LinuxSetEnvironmentVariable(string path)
        {
            string argument = "echo 'export PATH=\"$PATH:" + path + "\"' >> $HOME/.bashrc";
            argument = argument.Replace("\"", "\\\""); // Escape
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "/bin/bash";
            processInfo.Arguments = "-c \"" + argument + "\"";
            var process = Process.Start(processInfo);
            process.WaitForExit();
        }
    }
}
