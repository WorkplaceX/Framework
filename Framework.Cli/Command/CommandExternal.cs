namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using static Framework.Cli.AppCli;

    /// <summary>
    /// Cli external command.
    /// </summary>
    internal class CommandExternal : CommandBase
    {
        public CommandExternal(AppCli appCli)
            : base(appCli, "external", "Run external prebuild .NET script.")
        {

        }

        protected internal override void Execute()
        {
            var args = UtilExternal.ExternalArgs();

            AppCli.CommandExternal(args);
        }
    }
}
