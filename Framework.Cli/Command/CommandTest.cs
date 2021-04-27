namespace Framework.Cli.Command
{
    /// <summary>
    /// Cli test command to run unit tests.
    /// </summary>
    internal class CommandTest : CommandBase
    {
        public CommandTest(AppCli appCli)
            : base(appCli, "test", "Run unit tests")
        {

        }

        protected internal override void Execute()
        {
            string folderName = UtilFramework.FolderName + "Framework/Framework.Test/";
            UtilCliInternal.DotNet(folderName, "run");
        }
    }
}
