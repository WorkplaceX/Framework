namespace Framework.BuildTool
{
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
        /// Wrap SqlCommand into SqlConnection.
        /// </summary>
        private static void SqlCommand(string sql, Action<SqlCommand> execute, bool isFrameworkDb, params SqlParameter[] paramList)
        {
            string connectionString = ConnectionManagerServer.ConnectionString(isFrameworkDb);
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                {
                    sqlCommand.Parameters.AddRange(paramList);
                    execute(sqlCommand); // Call back
                }
            }
        }

        /// <summary>
        /// Execute sql command.
        /// </summary>
        public static void SqlCommand(string sql, bool isFrameworkDb, params SqlParameter[] paramList)
        {
            SqlCommand(sql, (sqlCommand) => sqlCommand.ExecuteNonQuery(), isFrameworkDb, paramList);
        }

        /// <summary>
        /// Read table from database.
        /// </summary>
        public static List<Dictionary<string, object>> SqlRead(string sql, bool isFrameworkDb, params SqlParameter[] paramList)
        {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
            SqlCommand(sql, (sqlCommand) =>
            {
                sqlCommand.Parameters.AddRange(paramList);
                using (SqlDataReader reader = sqlCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        result.Add(row);
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string fieldName = reader.GetName(i);
                            object value = reader.GetValue(i);
                            row.Add(fieldName, value);
                        }
                    }
                }
            }, isFrameworkDb);
            return result;
        }

        public static CommandLineApplication CommandLineApplicationCreate()
        {
            List<string> commandShortCutList = new List<string>();
            commandShortCutList.Add("buildClient");
            commandShortCutList.Add("serve --client");
            commandShortCutList.Add("generate");
            commandShortCutList.Add("generate --framework");
            commandShortCutList.Add("runSqlCreate");
            commandShortCutList.Add("runSqlCreate --drop");
            //
            return Command.CommandLineApplicationCreate(commandShortCutList);
        }

        public static void DotNetBuild(string workingDirectory)
        {
            string fileName = ConnectionManagerBuildTool.DotNetFileName;
            Start(workingDirectory, fileName, "build");
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
                    // TODO Make sure it's passed to stderr. See also try, catch in method Util.MethodExecute();
                    throw new Exception("Script failed!"); // Make sure BuildTool intallAll command has been run.
                }
            }
        }

        public static void Node(string workingDirectory, string fileName, bool isWait = true)
        {
            string nodeFileName = ConnectionManagerBuildTool.NodeFileName;
            Start(workingDirectory, nodeFileName, fileName, false, isWait, new KeyValuePair<string, string>("PORT", "1337"));
        }

        public static void NpmRun(string workingDirectory, string script, bool isWait = true)
        {
            string fileName = ConnectionManagerBuildTool.NpmFileName;
            Start(workingDirectory, fileName, "run " + script, isWait: isWait);
        }
    }
}
