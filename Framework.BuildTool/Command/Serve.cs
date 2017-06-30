namespace Framework.BuildTool
{
    public class CommandServe : Command
    {
        public CommandServe()
            : base("serve", "Serve .NET web page. Wait ca. 30 seconds.")
        {
            this.Client = OptionAdd("-c|--client", "Start npm client server only. Opens Chrome in CORS mode.");
        }

        public readonly Option Client;

        public readonly Option ClientAndServer;

        public override void Run()
        {
            if (Client.IsOn)
            {
                UtilBuildTool.NpmRun(Framework.UtilFramework.FolderName + "Submodule/Client/", "start", false);
                string workingDirectory = Framework.UtilFramework.FolderName;
                UtilBuildTool.Start(workingDirectory, @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", "--disable-web-security --user-data-dir", isWait: false);
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
