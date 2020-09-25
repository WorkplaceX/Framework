namespace Framework.Cli.Command
{
    using System;

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

            // Copy folder App/
            args.FolderCopy(args.AppSourceFolderName, args.AppDestFolderName);

            // Copy folder Database/
            args.FolderCopy(args.DatabaseSourceFolderName, args.DatabaseDestFolderName);

            // Copy folder CliApp/
            args.FolderCopy(args.CliAppSourceFolderName, args.CliAppDestFolderName);

            // Copy folder CliDatabase/
            args.FolderCopy(args.CliDatabaseSourceFolderName, args.CliDatabaseDestFolderName);

            args.FileReplaceLine(
                args.CliDatabaseDestFolderName + "DatabaseIntegrate.cs",
                "    public static class FrameworkConfigGridIntegrateApplicationCli",
                "    public static class FrameworkConfigGridIntegrateApplicationCli" + "External" + args.ExternalProjectName);

            args.FileReplaceLine(
                args.CliDatabaseDestFolderName + "DatabaseIntegrate.cs",
                "    public static class FrameworkConfigFieldIntegrateApplicationCli",
                "    public static class FrameworkConfigFieldIntegrateApplicationCli" + "External" + args.ExternalProjectName);

            // Copy folder CliDeployDb/
            args.FolderCopy(args.CliDeployDbSourceFolderName, args.CliDeployDbDestFolderName);

            // Copy folder Application.Website/
            Console.WriteLine("Copy folder Application.Website/");
            args.FolderCopy(args.WebsiteSourceFolderName, args.WebsiteDestFolderName);

            AppCli.CommandExternal(args);
        }
    }
}
