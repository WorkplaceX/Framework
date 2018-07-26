﻿
namespace Framework.Cli
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class UtilCli
    {
        internal static void DotNet(string workingDirectory, string arguments, bool isWait = true)
        {
            Start(workingDirectory, "dotnet", arguments, isWait);
        }

        /// <summary>
        /// Start script.
        /// </summary>
        /// <param name="isRedirectStdErr">If true, do not write to stderr.</param>
        internal static void Start(string workingDirectory, string fileName, string arguments, bool isWait = true, bool isRedirectStdErr = false)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UtilFramework.ConsoleWriteLineColor(string.Format("### {4} Process Begin (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.Green);

            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = workingDirectory;
            info.FileName = fileName;
            info.Arguments = arguments;
            if (isRedirectStdErr)
            {
                info.RedirectStandardError = true; // Do not write to stderr.
            }
            var process = Process.Start(info);
            if (isWait)
            {
                process.WaitForExit();
                if (isRedirectStdErr)
                {
                    string errorText = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        UtilFramework.ConsoleWriteLineColor(string.Format("### {4} Process StdErr (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.DarkGreen); // Write stderr to stdout.
                        UtilFramework.ConsoleWriteLineColor(errorText, ConsoleColor.DarkGreen);
                    }
                }
            }

            UtilFramework.ConsoleWriteLineColor(string.Format("### {4} Process End (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.DarkGreen);
        }

        internal static void OpenWebBrowser(string url)
        {
            Start(null, "cmd", $"/c start {url}", false);
        }

        internal static void FolderNameDelete(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                foreach (FileInfo fileInfo in new DirectoryInfo(folderName).GetFiles("*.*", SearchOption.AllDirectories))
                {
                    fileInfo.Attributes = FileAttributes.Normal; // See also: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it/30673648
                }
                Directory.Delete(folderName, true);
            }
        }

        internal static string ArgumentValue(CommandArgument commandArgument)
        {
            string result = commandArgument.Value;
            UtilFramework.Assert(commandArgument.Name.ToLower() == result.Substring(0, commandArgument.Name.Length).ToLower());
            if (result.ToUpper().StartsWith(commandArgument.Name.ToUpper()))
            {
                result = result.Substring(commandArgument.Name.Length);
            }
            if (result.StartsWith("="))
            {
                result = result.Substring(1);
            }
            result = UtilFramework.StringNull(result);
            return result;
        }
    }
}
