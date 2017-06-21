namespace Framework.BuildTool
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    public static class UtilBuildTool
    {
        public static void MSBuild(string fileNameCsproj)
        {
            string fileName = BuildTool.ConnectionManager.MSBuildFileName;
            string workingDirectory = Framework.Util.FolderName;
            Start(workingDirectory, fileName, fileNameCsproj);
        }

        public static void DotNetBuild(string workingDirectory)
        {
            string fileName = BuildTool.ConnectionManager.DotNetFileName;
            Start(workingDirectory, fileName, "build");
        }

        public static void DotNetRestore(string workingDirectory)
        {
            string fileName = BuildTool.ConnectionManager.DotNetFileName;
            Start(workingDirectory, fileName, "restore");
        }

        public static void NpmInstall(string workingDirectory, bool isThrowException = true)
        {
            string fileName = BuildTool.ConnectionManager.NpmFileName;
            Start(workingDirectory, fileName, "install --loglevel error", isThrowException); // Do not show npm warnings.
        }

        public static void DirectoryCopy(string folderNameSource, string folderNameDest, string searchPattern, bool isAllDirectory)
        {
            var source = new DirectoryInfo(folderNameSource);
            var dest = new DirectoryInfo(folderNameDest);
            SearchOption searchOption = SearchOption.TopDirectoryOnly;
            if (isAllDirectory)
            {
                searchOption = SearchOption.AllDirectories;
            }
            foreach (FileInfo file in source.GetFiles(searchPattern, searchOption))
            {
                string fileNameSource = file.FullName;
                string fileNameDest = Path.Combine(dest.FullName, file.FullName.Substring(source.FullName.Length));
                FileCopy(fileNameSource, fileNameDest);
            }
        }

        public static void DirectoryDelete(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
        }

        public static void FileCopy(string fileNameSource, string fileNameDest)
        {
            string folderNameDest = new FileInfo(fileNameDest).DirectoryName;
            if (!Directory.Exists(folderNameDest))
            {
                Directory.CreateDirectory(folderNameDest);
            }
            File.Copy(fileNameSource, fileNameDest, true);
        }

        public static void OpenVisualStudioCode(string folderName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo info = new ProcessStartInfo(BuildTool.ConnectionManager.VisualStudioCodeFileName, folderName);
                info.CreateNoWindow = true;
                Process.Start(info);
            }
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")); // Works ok on windows
            }
        }

        public static void DotNetRun(string workingDirectory, bool isWait = true)
        {
            string fileName = BuildTool.ConnectionManager.DotNetFileName;
            Start(workingDirectory, fileName, "run", false, isWait);
        }

        public static void Start(string workingDirectory, string fileName, string arguments, bool isThrowException = true, bool isWait = true, KeyValuePair<string, string>? environment = null)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            if (environment != null)
            {
                info.Environment.Add(environment.Value.Key, environment.Value.Value);
            }
            info.WorkingDirectory = workingDirectory;
            info.FileName = fileName;
            info.Arguments = arguments;
            Console.WriteLine("### Start (WorkingDirectory={0}; FileName={1}; Arguments={2};)", workingDirectory, fileName, arguments);
            var process = Process.Start(info);
            if (isWait == true)
            {
                process.WaitForExit();
                Console.WriteLine("### Exit code={3} (WorkingDirectory={0}; FileName={1}; Arguments={2};)", workingDirectory, fileName, arguments, process.ExitCode);
                if (isThrowException && process.ExitCode != 0)
                {
                    throw new Exception("Script failed!"); // TODO Make sure it's passed to stderr. See also try, catch in method Util.MethodExecute();
                }
            }
        }

        public static void Node(string workingDirectory, string fileName, bool isWait = true)
        {
            string nodeFileName = BuildTool.ConnectionManager.NodeFileName;
            Start(workingDirectory, nodeFileName, fileName, false, isWait, new KeyValuePair<string, string>("PORT", "1337"));
        }

        public static void NpmRun(string workingDirectory, string script)
        {
            string fileName = BuildTool.ConnectionManager.NpmFileName;
            Start(workingDirectory, fileName, "run " + script);
        }
    }
}
