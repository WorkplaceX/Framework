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
                return Framework.Util.FolderName + "Submodule/Framework.BuildTool/ConnectionManager.json";
            }
        }

        public static Config Instance
        {
            get
            {
                string json = Framework.Util.FileRead(JsonFileName);
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
                if (Framework.Util.IsLinux)
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
                if (Framework.Util.IsLinux)
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
        /// <summary>
        /// Check dev connection string.
        /// </summary>
        public static void ConnectionStringCheck()
        {
            if (!File.Exists(Server.Config.JsonFileName))
            {
                File.Copy(Server.Config.JsonTxtFileName, Server.Config.JsonFileName);
            }
            string connectionStringSwitch = Server.Config.Instance.ConnectionStringSwitch;
            string ip = Framework.Util.Ip();
            Util.Log(string.Format("SQL Connection check ({0}) from {1}", connectionStringSwitch, ip));
            string connectionString = Server.ConnectionManager.ConnectionString;
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }
                Util.Log("SQL Connection [ok]");
            }
            catch (Exception exception)
            {
                Util.Log(string.Format("Error: SQL Connection failed! ({0} - {1})", Server.Config.JsonFileName, exception.Message));
            }
        }

        private static void FileNameCheck()
        {
            if (!File.Exists(BuildTool.ConnectionManager.NodeFileName))
            {
                Util.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManager.NodeFileName, BuildTool.Config.JsonFileName));
            }
            if (!File.Exists(BuildTool.ConnectionManager.NpmFileName))
            {
                Util.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManager.NpmFileName, BuildTool.Config.JsonFileName));
            }
            if (!File.Exists(BuildTool.ConnectionManager.VisualStudioCodeFileName))
            {
                Util.Log(string.Format("Warning: File not found! Visual Studio Code. ({0}; {1})", ConnectionManager.VisualStudioCodeFileName, BuildTool.Config.JsonFileName));
            }
            if (!File.Exists(BuildTool.ConnectionManager.MSBuildFileName))
            {
                Util.Log(string.Format("Error: File not found! ({0}; {1})", ConnectionManager.MSBuildFileName, Build.Config.JsonFileName));
            }
        }

        private static void IsDebugDataJson()
        {
            Util.Log(string.Format("IsDebugDataJson={0}", Server.Config.Instance.IsDebugJson));
        }

        public static void Run()
        {
            ConnectionStringCheck();
            FileNameCheck();
            IsDebugDataJson();
        }
    }
}
