namespace Framework.BuildTool
{
    public class CommandBuildClient : Command
    {
        public CommandBuildClient() 
            : base("buildClient", "Run everytime Angular Client changes.")
        {

        }

        public override void Run()
        {

            UtilFramework.Log("Client>npm run universalBuild");
            UtilBuildTool.NpmRun(Framework.UtilFramework.FolderName + "Submodule/Client/", "universalBuild");
            //
            UtilFramework.Log("Server>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(Framework.UtilFramework.FolderName + "Server/Universal/");
            //
            UtilFramework.Log("UniversalExpress>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(Framework.UtilFramework.FolderName + "Submodule/UniversalExpress/Universal/");
            //
            UtilFramework.Log("Universal>Copy Client to Server");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Submodule/Client/dist/bundle.js", Framework.UtilFramework.FolderName + "Server/Universal/index.js");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Server/wwwroot/index.html", Framework.UtilFramework.FolderName + "Server/Universal/src/index.html");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Submodule/Client/dist/inline.*.bundle.js", Framework.UtilFramework.FolderName + "Server/Universal/inline.bundle.js");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Submodule/Client/dist/polyfills.*.bundle.js", Framework.UtilFramework.FolderName + "Server/Universal/polyfills.bundle.js");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Submodule/Client/dist/vendor.*.bundle.js", Framework.UtilFramework.FolderName + "Server/Universal/vendor.bundle.js");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Submodule/Client/dist/main.*.bundle.js", Framework.UtilFramework.FolderName + "Server/Universal/main.bundle.js");
            //
            UtilFramework.Log("Universal>Copy Client to UniversalExpress");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Submodule/Client/dist/bundle.js", Framework.UtilFramework.FolderName + "Submodule/UniversalExpress/Universal/index.js");
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Server/wwwroot/index.html", Framework.UtilFramework.FolderName + "Submodule/UniversalExpress/Universal/src/index.html");
        }
    }
}
