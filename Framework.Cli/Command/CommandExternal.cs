namespace Framework.Cli.Command
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    /// <summary>
    /// Cli external command.
    /// </summary>
    internal class CommandExternal : CommandBase
    {
        public CommandExternal(AppCli appCli)
            : base(appCli, "external", "Copy ExternalGit folders.")
        {

        }

        private CommandOption optionDelete;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionDelete = configuration.Option("--delete", "Delete ExternalGit dest folders.", CommandOptionType.NoValue);
        }

        private void ExecuteDelete()
        {
            var args = UtilExternal.ExternalArgs();

            // Delete folder App/
            Console.WriteLine("Delete dest folder App/");
            UtilCliInternal.FolderDelete(args.AppDestFolderName);

            // Copy folder Database/
            Console.WriteLine("Delete dest folder App/");
            UtilCliInternal.FolderDelete(args.DatabaseDestFolderName);

            // Copy folder CliApp/
            Console.WriteLine("Delete dest folder CliApp/");
            UtilCliInternal.FolderDelete(args.CliAppDestFolderName);


            // Copy folder CliDatabase/
            Console.WriteLine("Delete dest folder CliDatabase/");
            UtilCliInternal.FolderDelete(args.CliDatabaseDestFolderName);

            // Copy folder CliDeployDb/
            Console.WriteLine("Delete dest folder CliDeployDb/");
            UtilCliInternal.FolderDelete(args.CliDeployDbDestFolderName);
        }

        private void ExecuteCopy()
        {
            var args = UtilExternal.ExternalArgs();

            // Copy folder App/
            Console.WriteLine("Copy folder App/");
            args.FolderCopy(args.AppSourceFolderName, args.AppDestFolderName);

            // Copy folder Database/
            Console.WriteLine("Copy folder Database/");
            args.FolderCopy(args.DatabaseSourceFolderName, args.DatabaseDestFolderName);

            // Copy folder CliApp/
            Console.WriteLine("Copy folder CliApp/");
            args.FolderCopy(args.CliAppSourceFolderName, args.CliAppDestFolderName);

            // Copy folder CliDatabase/
            Console.WriteLine("Copy folder CliDatabase/");
            args.FolderCopy(args.CliDatabaseSourceFolderName, args.CliDatabaseDestFolderName);

            Console.WriteLine("Update file DatabaseIntegrate.cs");
            args.FileReplaceLine(
                args.CliDatabaseDestFolderName + "DatabaseIntegrate.cs",
                "    public static class FrameworkConfigGridIntegrateAppCli",
                "    public static class FrameworkConfigGridIntegrateAppCli" + "External" + args.ExternalProjectName);

            args.FileReplaceLine(
                args.CliDatabaseDestFolderName + "DatabaseIntegrate.cs",
                "    public static class FrameworkConfigFieldIntegrateAppCli",
                "    public static class FrameworkConfigFieldIntegrateAppCli" + "External" + args.ExternalProjectName);

            // Copy folder CliDeployDb/
            Console.WriteLine("Copy folder CliDeployDb/");
            args.FolderCopy(args.CliDeployDbSourceFolderName, args.CliDeployDbDestFolderName);

            AppCli.CommandExternal(args);
        }


        protected internal override void Execute()
        {
            if (optionDelete.OptionGet())
            {
                // Delete ExternalGit
                ExecuteDelete();
            }
            else
            {
                // Copy ExternalGit
                ExecuteCopy();
            }
        }
    }
}
