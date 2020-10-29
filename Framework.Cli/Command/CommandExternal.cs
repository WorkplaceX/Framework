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
            UtilCli.FolderDelete(args.AppDestFolderName);

            // Copy folder Database/
            Console.WriteLine("Delete dest folder App/");
            UtilCli.FolderDelete(args.DatabaseDestFolderName);

            // Copy folder CliApp/
            Console.WriteLine("Delete dest folder CliApp/");
            UtilCli.FolderDelete(args.CliAppDestFolderName);


            // Copy folder CliDatabase/
            Console.WriteLine("Delete dest folder CliDatabase/");
            UtilCli.FolderDelete(args.CliDatabaseDestFolderName);


            // Copy folder CliDeployDb/
            Console.WriteLine("Delete dest folder CliDeployDb/");
            UtilCli.FolderDelete(args.CliDeployDbDestFolderName);

            // Copy folder Application.Website/
            Console.WriteLine("Delete dest folder Application.Website/");
            UtilCli.FolderDelete(args.WebsiteDestFolderName);
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
            args.FolderCopy(args.CliAppSourceFolderName, args.CliAppDestFolderName);
            Console.WriteLine("Copy folder CliApp/");

            // Copy folder CliDatabase/
            args.FolderCopy(args.CliDatabaseSourceFolderName, args.CliDatabaseDestFolderName);
            Console.WriteLine("Copy folder CliDatabase/");

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

            // Copy folder Application.Website/
            Console.WriteLine("Copy folder Application.Website/");
            args.FolderCopy(args.WebsiteSourceFolderName, args.WebsiteDestFolderName);

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
