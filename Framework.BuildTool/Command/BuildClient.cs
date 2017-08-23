namespace Framework.BuildTool
{
    using Framework.Server;

    public class CommandBuildClient : Command
    {
        public CommandBuildClient() 
            : base("buildClient", "Run everytime Angular Client changes.")
        {

        }

        public override void Run()
        {
            UtilFramework.Log("Client>npm run universalBuild");
            UtilBuildTool.NpmRun(UtilFramework.FolderName + "Submodule/Client/", "universalBuild");
            //
            UtilFramework.Log("Server>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(UtilFramework.FolderName + "Server/Universal/");
            //
            UtilFramework.Log("UniversalExpress>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(UtilFramework.FolderName + "Submodule/Framework.UniversalExpress/Universal/");
            //
            string fileNameIndex = UtilServer.FileNameIndex();
            UtilFramework.Log("Universal>Copy Client to Server");
            UtilBuildTool.FileCopy(UtilFramework.FolderName + "Submodule/Client/dist/bundle.js", UtilFramework.FolderName + "Server/Universal/index.js");
            UtilBuildTool.FileCopy(fileNameIndex, UtilFramework.FolderName + "Server/Universal/src/index.html");
            UtilBuildTool.FileCopy(UtilFramework.FolderName + "Submodule/Client/dist/inline.*.bundle.js", UtilFramework.FolderName + "Server/Universal/inline.bundle.js");
            UtilBuildTool.FileCopy(UtilFramework.FolderName + "Submodule/Client/dist/polyfills.*.bundle.js", UtilFramework.FolderName + "Server/Universal/polyfills.bundle.js");
            UtilBuildTool.FileCopy(UtilFramework.FolderName + "Submodule/Client/dist/vendor.*.bundle.js", UtilFramework.FolderName + "Server/Universal/vendor.bundle.js");
            UtilBuildTool.FileCopy(UtilFramework.FolderName + "Submodule/Client/dist/main.*.bundle.js", UtilFramework.FolderName + "Server/Universal/main.bundle.js");
            //
            UtilFramework.Log("Universal>Copy Client to UniversalExpress");
            UtilBuildTool.FileCopy(UtilFramework.FolderName + "Submodule/Client/dist/bundle.js", UtilFramework.FolderName + "Submodule/Framework.UniversalExpress/Universal/index.js");
            UtilBuildTool.FileCopy(fileNameIndex, UtilFramework.FolderName + "Submodule/Framework.UniversalExpress/Universal/src/index.html");
        }
    }
}
