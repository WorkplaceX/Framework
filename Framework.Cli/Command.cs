namespace Framework.Cli
{
    /// <summary>
    /// Cli build command.
    /// </summary>
    public class CommandBuild : CommandBase
    {
        public CommandBuild(AppCliBase appCli)
            : base(appCli, "Build", "Build client and server")
        {

        }
    }

    /// <summary>
    /// Cli start command.
    /// </summary>
    public class CommandStart : CommandBase
    {
        public CommandStart(AppCliBase appCli)
            : base(appCli, "Start", "Start server")
        {

        }
    }
}
