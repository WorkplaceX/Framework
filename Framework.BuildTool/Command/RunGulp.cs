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
            Util.Log("Universal>npm run gulp");
            UtilBuildTool.NpmRun(Framework.Util.FolderName + "Submodule/Universal/", "gulp");
            //
            Util.Log("Server>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(Framework.Util.FolderName + "Server/Universal/");
            //
            Util.Log("UniversalExpress>Directory Universal/ clean");
            UtilBuildTool.DirectoryDelete(Framework.Util.FolderName + "Submodule/UniversalExpress/Universal/");
            //
            Util.Log("Universal>Copy Universal to Server and UniversalExpress");
            UtilBuildTool.DirectoryCopy(Framework.Util.FolderName + "Submodule/Universal/publish/", Framework.Util.FolderName + "Server/Universal/", "*.*", true);
            UtilBuildTool.DirectoryCopy(Framework.Util.FolderName + "Submodule/Universal/publish/", Framework.Util.FolderName + "Submodule/UniversalExpress/Universal/", "*.*", true);
            UtilBuildTool.FileCopy(Framework.Util.FolderName + "Submodule/Client/node_modules/bootstrap/dist/css/bootstrap.min.css", Framework.Util.FolderName + "Submodule/Framework/Server/wwwroot/bootstrap.min.css");
            UtilBuildTool.DirectoryCopy(Framework.Util.FolderName + "Submodule/Client/", Framework.Util.FolderName + "Submodule/Framework/Server/wwwroot/", "*.css", false);
        }
    }
}
