namespace Framework.BuildTool
{
    public class CommandRunGulp : Command
    {
        public CommandRunGulp() 
            : base("runGulp", "npm run gulp; Run everytime when Client changes")
        {

        }

        public override void Run()
        {
            UtilFramework.Log("Universal>npm run gulp");
            UtilBuildTool.NpmRun(Framework.UtilFramework.FolderName + "Submodule/Universal/", "gulp");
            //
            UtilFramework.Log("Server>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(Framework.UtilFramework.FolderName + "Server/Universal/");
            //
            UtilFramework.Log("UniversalExpress>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(Framework.UtilFramework.FolderName + "Submodule/UniversalExpress/Universal/");
            //
            UtilFramework.Log("Universal>Copy Universal to Server and UniversalExpress");
            UtilBuildTool.DirectoryCopy(Framework.UtilFramework.FolderName + "Submodule/Universal/publish/", Framework.UtilFramework.FolderName + "Server/Universal/", "*.*", true);
            UtilBuildTool.DirectoryCopy(Framework.UtilFramework.FolderName + "Submodule/Universal/publish/", Framework.UtilFramework.FolderName + "Submodule/UniversalExpress/Universal/", "*.*", true);
            UtilBuildTool.FileCopy(Framework.UtilFramework.FolderName + "Submodule/Client/node_modules/bootstrap/dist/css/bootstrap.min.css", Framework.UtilFramework.FolderName + "Submodule/Framework/Server/wwwroot/bootstrap.min.css");
            UtilBuildTool.DirectoryCopy(Framework.UtilFramework.FolderName + "Submodule/Client/", Framework.UtilFramework.FolderName + "Submodule/Framework/Server/wwwroot/", "*.css", false);
        }
    }
}
