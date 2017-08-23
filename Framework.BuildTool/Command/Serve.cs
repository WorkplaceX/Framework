namespace Framework.BuildTool
{
    public class CommandServe : Command
    {
        public CommandServe()
            : base("serve", "Serve .NET web page. Wait ca. 30 seconds.")
        {
            this.Client = OptionAdd("-c|--client", "Start npm client server only.");
        }

        public readonly Option Client;

        public override void Run()
        {
            if (Client.IsOn)
            {
                UtilBuildTool.NpmRun(UtilFramework.FolderName + "Submodule/Client/", "start");
            }
            else
            {
                UtilBuildTool.DotNetRun(UtilFramework.FolderName + "Server/", false);
                UtilBuildTool.Node(UtilFramework.FolderName + "Submodule/Framework.UniversalExpress/Universal/", "index.js", false);
                UtilBuildTool.OpenBrowser("http://localhost:5000");
            }
        }
    }
}
