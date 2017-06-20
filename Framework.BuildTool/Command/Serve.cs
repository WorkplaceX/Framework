namespace Framework.BuildTool
{
    public class CommandServe : Command
    {
        public CommandServe()
            : base("serve", "Serve web page")
        {
            this.Client = OptionAdd("-c|--client", "Start client server only.");
        }

        public readonly Option Client;

        public readonly Option ClientAndServer;

        public override void Run()
        {
            if (Client.IsOn)
            {
                UtilBuildTool.NpmRun(Framework.Util.FolderName + "Submodule/Client/", "start");
            }
            else
            {
                UtilBuildTool.DotNetRun(Framework.Util.FolderName + "Server/", false);
                UtilBuildTool.Node(Framework.Util.FolderName + "Submodule/UniversalExpress/Universal/", "index.js", false);
                UtilBuildTool.OpenBrowser("http://localhost:5000");
            }
        }
    }
}
