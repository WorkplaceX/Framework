
namespace Framework.Cli
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class UtilCli
    {
        internal static void DotNet(string workingDirectory, string arguments, bool isWait = true)
        {
            Start(workingDirectory, "dotnet", arguments, isWait);
        }

        internal static void Start(string workingDirectory, string fileName, string arguments, bool isWait = true)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            UtilFramework.ConsoleWriteLine(string.Format("### {4} Process Begin (WorkingDirectory={0}; FileName={1}; Arguments={2}; IsWait={3}", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.Green);

            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = workingDirectory;
            info.FileName = fileName;
            info.Arguments = arguments;
            var process = Process.Start(info);
            if (isWait)
            {
                process.WaitForExit();
            }

            UtilFramework.ConsoleWriteLine(string.Format("### {4} Process End (WorkingDirectory={0}; FileName={1}; Arguments={2}; IsWait={3}", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.DarkGreen);
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
    }
}
