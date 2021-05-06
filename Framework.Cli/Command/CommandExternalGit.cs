namespace Framework.Cli.Command
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;

    /// <summary>
    /// Cli ExternalGit command.
    /// </summary>
    internal class CommandExternalGit : CommandBase
    {
        public CommandExternalGit(AppCli appCli)
            : base(appCli, "externalGit", "Copy ExternalGit folders.")
        {

        }

        private CommandOption optionDelete;

        private CommandOption optionDeleteAll;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionDelete = configuration.Option("--delete", "Delete ExternalGit dest folders.", CommandOptionType.NoValue);
            optionDeleteAll = configuration.Option("--deleteAll", "Delete all ExternalGit dest folders.", CommandOptionType.NoValue);
        }

        /// <summary>
        /// Delete folder ExternalGit/ExternalProjectName/
        /// </summary>
        /// <param name="isDeleteAll">If true, delete folder ExternalGit/</param>
        private void ExecuteDelete(bool isDeleteAll = false)
        {
            var args = UtilExternalGit.ExternalArgs(!isDeleteAll);

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
            var args = UtilExternalGit.ExternalArgs();

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
                "    public static class FrameworkConfigGridIntegrateAppCli" + "ExternalGit" + args.ExternalProjectName);

            args.FileReplaceLine(
                args.CliDatabaseDestFolderName + "DatabaseIntegrate.cs",
                "    public static class FrameworkConfigFieldIntegrateAppCli",
                "    public static class FrameworkConfigFieldIntegrateAppCli" + "ExternalGit" + args.ExternalProjectName);

            // Copy folder CliDeployDb/
            Console.WriteLine("Copy folder CliDeployDb/");
            args.FolderCopy(args.CliDeployDbSourceFolderName, args.CliDeployDbDestFolderName);

            AppCli.CommandExternalGit(args);
        }


        protected internal override void Execute()
        {
            if (optionDelete.OptionGet())
            {
                // Delete folder ExternalGit/ExternalProjectName/
                ExecuteDelete();
            }
            else
            {
                if (optionDeleteAll.OptionGet())
                {
                    // Delete folder ExternalGit/
                    ExecuteDelete(true);
                }
                else
                {
                    // Copy folder ExternalGit/ExternalProjectName/
                    ExecuteCopy();
                }
            }
        }
    }
}
