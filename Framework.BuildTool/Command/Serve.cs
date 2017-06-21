namespace Framework.BuildTool
{
    public class CommandServe : Command
    {
        public CommandServe()
            : base("serve", "Serve web page. Wait ca. 30 seconds.")
        {
            this.Client = OptionAdd("-c|--client", "Start client server only.");
        }

        public readonly Option Client;

        public readonly Option ClientAndServer;

        public override void Run()
        {
            if (Client.IsOn)
            {
                UtilBuildTool.NpmRun(Framework.UtilFramework.FolderName + "Submodule/Client/", "start");
            }
            else
            {
                UtilBuildTool.DotNetRun(Framework.UtilFramework.FolderName + "Server/", false);
                UtilBuildTool.Node(Framework.UtilFramework.FolderName + "Submodule/UniversalExpress/Universal/", "index.js", false);
                UtilBuildTool.OpenBrowser("http://localhost:5000");
            }
        }
    }
}
