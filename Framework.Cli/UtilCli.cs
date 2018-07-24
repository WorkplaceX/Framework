
namespace Framework.Cli
{
    using System;
    using System.Diagnostics;

    public static class UtilCli
    {
        public static void DotNet(string workingDirectory, string arguments, bool isWait = true)
        {
            Start(workingDirectory, "dotnet", arguments, isWait);
        }

        public static void Start(string workingDirectory, string fileName, string arguments, bool isWait = true)
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

        public static void OpenWebBrowser(string url)
        {
            Start(null, "cmd", $"/c start {url}", false);
        }
    }
}
