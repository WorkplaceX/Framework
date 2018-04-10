namespace Framework.BuildTool
{
    using Framework.DataAccessLayer;
    using Framework.Server;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    public static class UtilBuildTool
    {
        /// <summary>
        /// Execute sql command. Run multiple sql statements.
        /// </summary>
        public static void SqlExecute(List<string> sqlList, bool isFrameworkDb)
        {
            UtilDataAccessLayer.Execute(sqlList, isFrameworkDb, new List<SqlParameter>(), true, (sqlCommand) => sqlCommand.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute sql command. Run one sql statement.
        /// </summary>
        public static void SqlExecute(string sql, bool isFrameworkDb, List<SqlParameter> paramList, bool isUseParam = true)
        {
            List<string> sqlList = new List<string>();
            sqlList.Add(sql);
            UtilDataAccessLayer.Execute(sqlList, isFrameworkDb, paramList, isUseParam, (sqlCommand) => sqlCommand.ExecuteNonQuery());
        }

        /// <summary>
        /// Execute sql command. Run one sql statement.
        /// </summary>
        public static void SqlExecute(string sql, bool isFrameworkDb)
        {
            SqlExecute(sql, isFrameworkDb, new List<SqlParameter>());
        }

        public static CommandLineApplication CommandLineApplicationCreate()
        {
            List<string> commandShortCutList = new List<string>();
            commandShortCutList.Add("check");
            commandShortCutList.Add("installAll");
            commandShortCutList.Add("runSqlCreate");
            commandShortCutList.Add("runSqlCreate --drop");
            commandShortCutList.Add("generate");
            commandShortCutList.Add("generate --framework");
            commandShortCutList.Add("buildClient");
            commandShortCutList.Add("serve --clientLiveDevelopment");
            //
            return Command.CommandLineApplicationCreate(commandShortCutList);
        }

        public static void DotNetBuild(string workingDirectory)
        {
            string fileName = ConnectionManagerBuildTool.DotNetFileName;
            Start(workingDirectory, fileName, "build");
        }

        public static void DotNetPublish(string workingDirectory)
        {
            string fileName = ConnectionManagerBuildTool.DotNetFileName;
            Start(workingDirectory, fileName, "publish");
        }

        public static void DotNetRestore(string workingDirectory)
        {
            string fileName = ConnectionManagerBuildTool.DotNetFileName;
            Start(workingDirectory, fileName, "restore");
        }

        public static void NpmInstall(string workingDirectory, bool isThrowException = true)
        {
            string fileName = ConnectionManagerBuildTool.NpmFileName;
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
                foreach (FileInfo fileInfo in new DirectoryInfo(folderName).GetFiles("*.*", SearchOption.AllDirectories))
                {
                    fileInfo.Attributes = FileAttributes.Normal; // See also: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it/30673648
                }
                Directory.Delete(folderName, true);
            }
        }

        public static void FileCopy(string fileNameSource, string fileNameDest)
        {
            if (fileNameSource.Contains("*"))
            {
                string fileNameSourceNoStar = fileNameSource.Replace("*", "");
                var fileInfo = new FileInfo(fileNameSourceNoStar);
                string directory = fileInfo.DirectoryName + Path.DirectorySeparatorChar;
                string fileName = fileNameSource.Substring(directory.Length);
                fileNameSource = Directory.GetFiles(directory, fileName).Single();
            }
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
                ProcessStartInfo info = new ProcessStartInfo(ConnectionManagerBuildTool.VisualStudioCodeFileName, folderName);
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
            string fileName = ConnectionManagerBuildTool.DotNetFileName;
            Start(workingDirectory, fileName, "run", false, isWait);
        }

        public static void Start(string workingDirectory, string fileName, string arguments, bool isThrowException = true, bool isWait = true, KeyValuePair<string, string>? environment = null, bool isRedirectStdErr = false)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            if (environment != null)
            {
                info.Environment.Add(environment.Value.Key, environment.Value.Value);
            }
            info.WorkingDirectory = workingDirectory;
            info.FileName = fileName;
            info.Arguments = arguments;
            if (isRedirectStdErr)
            {
                info.RedirectStandardError = true; // Do not write to stderr.
            }
            Console.WriteLine("### Start (FileName={1}; Arguments={2}; WorkingDirectory={0};)", workingDirectory, fileName, arguments);
            var process = Process.Start(info);
            if (isWait == true)
            {
                process.WaitForExit();
                if (isRedirectStdErr)
                {
                    string errorText = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(errorText))
                    {
                        Console.WriteLine("### STDERR (FileName={1}; Arguments={2}; WorkingDirectory={0}; STDERR={3};)", workingDirectory, fileName, arguments, errorText); // Write stderr into stdout.
                    }
                }
                Console.WriteLine("### Exit code={3} (FileName={1}; Arguments={2}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, process.ExitCode);
                if (isThrowException && process.ExitCode != 0)
                {
                    // TODO Make sure it's passed to stderr. See also try, catch in method Util.MethodExecute();
                    throw new Exception("Script failed!"); // Make sure BuildTool intallAll command has been run.
                }
            }
        }

        public static void Node(string workingDirectory, string fileName, bool isWait = true)
        {
            string nodeFileName = ConnectionManagerBuildTool.NodeFileName;
            Start(workingDirectory, nodeFileName, fileName, false, isWait, new KeyValuePair<string, string>("PORT", "4000")); // Default port. See also: Submodule/Client/src/server.ts
        }

        public static void NpmRun(string workingDirectory, string script, bool isWait = true)
        {
            string fileName = ConnectionManagerBuildTool.NpmFileName;
            Start(workingDirectory, fileName, "run " + script, isWait: isWait);
        }
    }
}
