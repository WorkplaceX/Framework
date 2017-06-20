namespace Framework.BuildTool
{
    public class CommandInstallAll : Command
    {
        public CommandInstallAll() 
            : base("installAll", "npm install; dotnet restore; Python error can be ignored")
        {

        }

        public override void Run()
        {
            Util.Log("Client>npm install");
            UtilBuildTool.NpmInstall(Framework.Util.FolderName + "Submodule/Client/");
            Util.Log("Universal>npm install");
            UtilBuildTool.NpmInstall(Framework.Util.FolderName + "Submodule/Universal/", false); // Throws always an exception!
            // Application
            Util.Log("Application>dotnet restore");
            UtilBuildTool.DotNetRestore(Framework.Util.FolderName + "Application/");
            Util.Log("Application>dotnet build");
            UtilBuildTool.DotNetBuild(Framework.Util.FolderName + "Application/");
            // Server
            Util.Log("Server>dotnet restore");
            UtilBuildTool.DotNetRestore(Framework.Util.FolderName + "Server/");
            Util.Log("Server>dotnet build");
            UtilBuildTool.DotNetBuild(Framework.Util.FolderName + "Server/");
            // Office
            if (Framework.Util.IsLinux == false)
            {
                UtilBuildTool.MSBuild(Framework.Util.FolderName + "Submodule/Office/Office.csproj"); // Office is not (yet) a .NET Core library.
            }
            new CommandRunGulp().Run();
        }
    }
}
