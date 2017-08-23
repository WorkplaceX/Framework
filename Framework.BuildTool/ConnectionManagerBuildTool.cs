using Framework.Server;
using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.IO;

namespace Framework.BuildTool
{
    /// <summary>
    /// Build config json.
    /// </summary>
    public class ConfigBuildTool
    {
        public string[] NodeFileName;

        public string[] NpmFileName;

        public string[] VisualStudioCodeFileName;

        public static string JsonFileName
        {
            get
            {
                return UtilFramework.FolderName + "Submodule/Framework.BuildTool/ConnectionManagerBuildTool.json";
            }
        }

        public static ConfigBuildTool Instance
        {
            get
            {
                string json = UtilFramework.FileRead(JsonFileName);
                var result = JsonConvert.DeserializeObject<ConfigBuildTool>(json);
                return result;
            }
        }
    }

    public static class ConnectionManagerBuildTool
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
                foreach (string fileName in ConfigBuildTool.Instance.NpmFileName)
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
                foreach (string fileName in ConfigBuildTool.Instance.VisualStudioCodeFileName)
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
                foreach (string fileName in ConfigBuildTool.Instance.NodeFileName)
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
            if (!File.Exists(ConfigServer.JsonFileName))
            {
                File.Copy(ConfigServer.JsonTxtFileName, ConfigServer.JsonFileName);
            }
        }

        /// <summary>
        /// Check dev connection string.
        /// </summary>
        public static void ConnectionStringCheck()
        {
            JsonFileCreateIfNotExists();
            string connectionStringSwitch = ConfigServer.Instance.ConnectionStringSwitch;
            string ip = UtilFramework.Ip();
            UtilFramework.Log(string.Format("SQL Connection check ({0}) from {1}", connectionStringSwitch, ip));
            string connectionString = ConnectionManagerServer.ConnectionString;
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
                UtilFramework.Log(string.Format("Error: SQL Connection failed! ({0} - {1})", ConfigServer.JsonFileName, exception.Message));
            }
        }

        private static void FileNameCheck()
        {
            if (!File.Exists(ConnectionManagerBuildTool.NodeFileName))
            {
                UtilFramework.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManagerBuildTool.NodeFileName, ConfigBuildTool.JsonFileName));
            }
            if (!File.Exists(ConnectionManagerBuildTool.NpmFileName))
            {
                UtilFramework.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManagerBuildTool.NpmFileName, ConfigBuildTool.JsonFileName));
            }
            if (!File.Exists(ConnectionManagerBuildTool.VisualStudioCodeFileName))
            {
                UtilFramework.Log(string.Format("Warning: File not found! Visual Studio Code. ({0}; {1})", ConnectionManagerBuildTool.VisualStudioCodeFileName, ConfigBuildTool.JsonFileName));
            }
        }

        public static void Run()
        {
            ConnectionStringCheck();
            FileNameCheck();
        }
    }
}
