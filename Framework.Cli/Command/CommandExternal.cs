namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using static Framework.Cli.AppCli;

    /// <summary>
    /// Cli start command.
    /// </summary>
    internal class CommandExternal : CommandBase
    {
        public CommandExternal(AppCli appCli)
            : base(appCli, "external", "Run external prebuild .NET script.")
        {

        }

        protected internal override void Execute()
        {
            string applicationWebsiteSourceFolderName = UtilFramework.FolderName + "Application.Website/";
            string applicationWebsiteDestFolderName = UtilFramework.FolderNameExternal + "Framework/Framework.Angular/application/src/Application.Website/";
            var args = new ExternalPrebuildArgs { 
                ApplicationWebsiteSourceFolderName = applicationWebsiteSourceFolderName,
                ApplicationWebsiteDestFolderName = applicationWebsiteDestFolderName 
            };
            AppCli.ExternalPrebuild(args);
        }
    }
}
