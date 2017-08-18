using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;

namespace Framework.BuildTool
{
    /// <summary>
    /// Build config json.
    /// </summary>
    public class Config
    {
        public string[] NodeFileName;

        public string[] NpmFileName;

        public string[] VisualStudioCodeFileName;

        public string[] MSBuildFileName;

        public static string JsonFileName
        {
            get
            {
                return UtilFramework.FolderName + "Submodule/Framework.BuildTool/ConnectionManager.json";
            }
        }

        public static Config Instance
        {
            get
            {
                string json = UtilFramework.FileRead(JsonFileName);
                var result = JsonConvert.DeserializeObject<Config>(json);
                return result;
            }
        }
    }

    public static class ConnectionManager
    {
        public static string NpmFileName
        {
            get
            {
                string result = "npm.cmd";
                if (UtilFramework.IsLinux)
                {
                    result = "npm";
                }
                foreach (string fileName in Config.Instance.NpmFileName)
                {
                    if (File.Exists(fileName))
                    {
                        result = fileName;
                    }
                }
                return result;
            }
        }

        public static string DotNetFileName
        {
            get
            {
                string result = "dotnet.exe";
                if (UtilFramework.IsLinux)
                {
                    result = "dotnet";
                }
                return result;
            }
        }

        public static string VisualStudioCodeFileName
        {
            get
            {
                string result = "code.exe";
                foreach (string fileName in Config.Instance.VisualStudioCodeFileName)
                {
                    if (File.Exists(fileName))
                    {
                        result = fileName;
                    }
                }
                return result;
            }
        }

        public static string MSBuildFileName
        {
            get
            {
                string result = "MSBuild.exe";
                foreach (string fileName in Config.Instance.MSBuildFileName)
                {
                    if (File.Exists(fileName))
                    {
                        result = fileName;
                    }
                }
                return result;
            }
        }

        public static string NodeFileName
        {
            get
            {
                string result = "node.exe";
                foreach (string fileName in Config.Instance.NodeFileName)
                {
                    if (File.Exists(fileName))
                    {
                        result = fileName;
                    }
                }
                return result;
            }
        }
    }

    public static class ConnectionManagerCheck
    {
        public static void JsonFileCreateIfNotExists()
        {
            if (!File.Exists(Server.Config.JsonFileName))
            {
                File.Copy(Server.Config.JsonTxtFileName, Server.Config.JsonFileName);
            }
        }

        /// <summary>
        /// Check dev connection string.
        /// </summary>
        public static void ConnectionStringCheck()
        {
            JsonFileCreateIfNotExists();
            string connectionStringSwitch = Server.Config.Instance.ConnectionStringSwitch;
            string ip = UtilFramework.Ip();
            UtilFramework.Log(string.Format("SQL Connection check ({0}) from {1}", connectionStringSwitch, ip));
            string connectionString = Server.ConnectionManager.ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }
                UtilFramework.Log("SQL Connection [ok]");
            }
            catch (Exception exception)
            {
                UtilFramework.Log(string.Format("Error: SQL Connection failed! ({0} - {1})", Server.Config.JsonFileName, exception.Message));
            }
        }

        private static void FileNameCheck()
        {
            if (!File.Exists(BuildTool.ConnectionManager.NodeFileName))
            {
                UtilFramework.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManager.NodeFileName, BuildTool.Config.JsonFileName));
            }
            if (!File.Exists(BuildTool.ConnectionManager.NpmFileName))
            {
                UtilFramework.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManager.NpmFileName, BuildTool.Config.JsonFileName));
            }
            if (!File.Exists(BuildTool.ConnectionManager.VisualStudioCodeFileName))
            {
                UtilFramework.Log(string.Format("Warning: File not found! Visual Studio Code. ({0}; {1})", ConnectionManager.VisualStudioCodeFileName, BuildTool.Config.JsonFileName));
            }
            if (!File.Exists(BuildTool.ConnectionManager.MSBuildFileName))
            {
                UtilFramework.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManager.MSBuildFileName, BuildTool.Config.JsonFileName));
            }
        }

        public static void Run()
        {
            ConnectionStringCheck();
            FileNameCheck();
        }
    }
}
