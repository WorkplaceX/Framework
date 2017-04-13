using Newtonsoft.Json;
using System;
using System.Data.SqlClient;

namespace Framework.Build
{
    public abstract class ScriptBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public ScriptBase(string[] args)
        {
            this.args = args;
        }

        private readonly string[] args;

        /// <summary>
        /// Returns command line argument.
        /// </summary>
        /// <param name="index">Command line argument index.</param>
        public string ArgGet(int index)
        {
            string result = null;
            if (index >= 0 && index < args.Length)
            {
                result = args[index];
            }
            return result;
        }

        [Description("Enter ConnectionString for ConnectionManager.json", 0.5)]
        public void ConnectionString()
        {
            string connectionStringSwitch = Server.Config.Instance.ConnectionStringSwitch;
            string connectionString = Server.ConnectionManager.ConnectionString;
            Util.Log(string.Format("{0}={1}", connectionStringSwitch, connectionString));
            Util.Log("Enter new ConnectionString:");
            string connectionStringNew = ArgGet(1);
            if (connectionStringNew == null)
            {
                connectionStringNew = Console.ReadLine();
            }
            //
            Server.Config config = Server.Config.Instance;
            config.ConnectionStringSet(connectionStringNew);
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            Framework.Util.FileWrite(Server.Config.JsonFileName, json);
            Util.Log(string.Format("File updated. ({0})", Server.Config.JsonFileName));
            ConnectionManagerCheck.Run();
        }

        [Description("npm install; dotnet restore; Python error can be ignored", 1)]
        public void InstallAll()
        {
            Util.Log("Client>npm install");
            Util.NpmInstall(Framework.Util.FolderName + "Submodule/Client/");
            Util.Log("Server>npm install");
            Util.NpmInstall(Framework.Util.FolderName + "Server/"); 
            Util.Log("Universal>npm install");
            Util.NpmInstall(Framework.Util.FolderName + "Submodule/Universal/", false); // Throws always an exception!
            // Application
            Util.Log("Application>dotnet restore");
            Util.DotNetRestore(Framework.Util.FolderName + "Application/");
            Util.Log("Application>dotnet build");
            Util.DotNetBuild(Framework.Util.FolderName + "Application/");
            // Server
            Util.Log("Server>dotnet restore");
            Util.DotNetRestore(Framework.Util.FolderName + "Server/");
            Util.Log("Server>dotnet build");
            Util.DotNetBuild(Framework.Util.FolderName + "Server/");
            // Office
            if (Framework.Util.IsLinux == false)
            {
                Util.MSBuild(Framework.Util.FolderName + "Submodule/Office/Office.csproj"); // Office is not (yet) a .NET Core library.
            }
            RunGulp();
        }

        [Description("Start Server and UniversalExpress", 2)]
        public void StartServerAndClient()
        {
            Util.DotNetRun(Framework.Util.FolderName + "Server/", false);
            Util.Node(Framework.Util.FolderName + "Submodule/UniversalExpress/Universal/", "index.js", false);
            Util.OpenBrowser("http://localhost:5000");
        }

        [Description("VS Code", 3)]
        public void OpenClient()
        {
            Util.OpenVisualStudioCode(Framework.Util.FolderName + "Submodule/Client/");
        }

        [Description("npm run start", 4)]
        public void StartClient()
        {
            Util.NpmRun(Framework.Util.FolderName + "Submodule/Client/", "start");
        }

        [Description("npm run gulp; Run everytime when Client changes", 5)]
        public void RunGulp()
        {
            Util.Log("Server>npm run gulp");
            Util.NpmRun(Framework.Util.FolderName + "Server/", "gulp");
        }

        [Description("VS Code", 6)]
        public void OpenServer()
        {
            Util.OpenVisualStudioCode(Framework.Util.FolderName + "Server/");
        }

        [Description("VS Code", 7)]
        public void OpenUniversal()
        {
            Util.OpenVisualStudioCode(Framework.Util.FolderName + "Submodule/Universal/");
        }

        [Description("Open Visual Studio", 8)]
        public void OpenApplication()
        {
            Util.OpenBrowser(Framework.Util.FolderName + "Application.sln");
        }

        [Description("Toggle IsDebugDataJson flag", 8)]
        public void ToggleIsDebugDataJson()
        {
            Server.Config config = Server.Config.Instance;
            config.IsDebugJson = !config.IsDebugJson;
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            Framework.Util.FileWrite(Server.Config.JsonFileName, json);
            Util.Log(string.Format("File updated. ({0})", Server.Config.JsonFileName));
        }

        private void RunSql(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            var fileNameList = Framework.Util.FileNameList(Framework.Util.FolderName + "Build/Sql/");
            foreach (string fileName in fileNameList)
            {
                string text = Framework.Util.FileRead(fileName);
                var sqlList = text.Split(new string[] { "\r\nGO", "\nGO" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string sql in sqlList)
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        [Description("Run SQL scripts and generate CSharp DTO's", 9)]
        public virtual void RunSql()
        {
            RunSql(Framework.Server.ConnectionManager.ConnectionString);
            GenerateCSharp();
        }

        [Description("Generate CSharp DTO's", 10)]
        public virtual void GenerateCSharp()
        {
            Build.DataAccessLayer.Script.Run();
            Util.Log(string.Format("File updated. ({0})", Build.DataAccessLayer.ConnectionManager.DatabaseLockFileName));
        }

        [Description("Run unit tests", 11)]
        public void UnitTest()
        {
            Util.DotNetRun(Framework.Util.FolderName + "Submodule/UnitTest/");
        }
    }
}
