namespace Framework.Cli.Command
{
    using Microsoft.Extensions.CommandLineUtils;

    public class CommandBase
    {
        public CommandBase(AppCli appCli, string name, string description)
        {
            this.AppCli = appCli;
            this.AppCli.CommandList.Add(this);
            this.Name = name;
            this.Description = description;
        }

        public readonly AppCli AppCli;

        public readonly string Name;

        public readonly string Description;

        internal CommandLineApplication Configuration;

        protected virtual internal void Execute()
        {

        }

        /// <summary>
        /// Override to register command arguments and options.
        /// For example: configuration.Option("-a", "Build all.", CommandOptionType.NoValue);
        /// </summary>
        protected virtual internal void Register(CommandLineApplication configuration)
        {

        }
    }
}
