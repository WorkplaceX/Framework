using Framework.Server;
using System;
using System.IO;

namespace Framework.Cli.Command
{
    /// <summary>
    /// Cli command to create a new project from template.
    /// </summary>
    internal class CommandNewProject : CommandBase
    {
        public CommandNewProject(AppCli appCli)
            : base(appCli, "new", "Create new project")
        {

        }

        protected internal override void Execute()
        {
            Console.WriteLine("Source=" + typeof(CommandNewProject).Assembly.Location);
            Console.WriteLine("Dest=" + Directory.GetCurrentDirectory());
        }
    }
}
