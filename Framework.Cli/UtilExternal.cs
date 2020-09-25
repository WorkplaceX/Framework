using Database.dbo;
using DatabaseIntegrate.dbo;
using System;
using System.Collections.Generic;
using static Framework.Cli.AppCli;

namespace Framework.Cli
{
    /// <summary>
    /// Util for cli external command.
    /// </summary>
    internal static class UtilExternal
    {
        /// <summary>
        /// Call method CommandGenerateIntegrate(); on external AppCli.
        /// </summary>
        public static void CommandGenerateIntegrate(AppCli appCli, GenerateIntegrateResult result)
        {
            foreach (var type in appCli.AssemblyApplicationCli.GetTypes())
            {
                if (UtilFramework.IsSubclassOf(type, typeof(AppCli)))
                {
                    if (type != appCli.GetType())
                    {
                        var appCliExternal = (AppCli)Activator.CreateInstance(type);
                        appCliExternal.CommandGenerateIntegrate(result);
                    }
                }
            }
        }

        /// <summary>
        /// Call method CommandDeployDbIntegrate(); on external AppCli.
        /// </summary>
        public static void CommandDeployDbIntegrate(AppCli appCli, DeployDbIntegrateResult result)
        {
            foreach (var type in appCli.AssemblyApplicationCli.GetTypes())
            {
                if (UtilFramework.IsSubclassOf(type, typeof(AppCli)))
                {
                    if (type != appCli.GetType())
                    {
                        var appCliExternal = (AppCli)Activator.CreateInstance(type);
                        appCliExternal.CommandDeployDbIntegrate(result);
                    }
                }
            }

        }

        /// <summary>
        /// Collect rowList from external FrameworkConfigGridIntegrateApplicationCli (ConfigGrid).
        /// </summary>
        public static void CommandDeployDbIntegrate(AppCli appCli, List<FrameworkConfigGridIntegrate> rowList)
        {
            foreach (var type in appCli.AssemblyApplicationCli.GetTypes())
            {
                if (type.FullName.StartsWith("DatabaseIntegrate.dbo.FrameworkConfigGridIntegrateApplicationCli"))
                {
                    if (type.FullName != "DatabaseIntegrate.dbo.FrameworkConfigGridIntegrateApplicationCli")
                    {
                        var typeCliExternal = appCli.AssemblyApplicationCli.GetType(type.FullName);
                        var propertyInfo = typeCliExternal.GetProperty(nameof(FrameworkConfigGridIntegrateFrameworkCli.RowList));
                        var rowApplicationCliList = (List<FrameworkConfigGridIntegrate>)propertyInfo.GetValue(null);
                        rowList.AddRange(rowApplicationCliList);
                    }
                }
            }
        }

        /// <summary>
        /// Collect rowList from external FrameworkConfigFieldIntegrateApplicationCli (ConfigField).
        /// </summary>
        public static void CommandDeployDbIntegrate(AppCli appCli, List<FrameworkConfigFieldIntegrate> rowList)
        {
            foreach (var type in appCli.AssemblyApplicationCli.GetTypes())
            {
                if (type.FullName.StartsWith("DatabaseIntegrate.dbo.FrameworkConfigFieldIntegrateApplicationCli"))
                {
                    if (type.FullName != "DatabaseIntegrate.dbo.FrameworkConfigFieldIntegrateApplicationCli")
                    {
                        var typeCliExternal = appCli.AssemblyApplicationCli.GetType(type.FullName);
                        var propertyInfo = typeCliExternal.GetProperty(nameof(FrameworkConfigFieldIntegrateFrameworkCli.RowList));
                        var rowApplicationCliList = (List<FrameworkConfigFieldIntegrate>)propertyInfo.GetValue(null);
                        rowList.AddRange(rowApplicationCliList);
                    }
                }
            }
        }

        /// <summary>
        /// Returns ExternalArgs.
        /// </summary>
        public static ExternalArgs ExternalArgs()
        {
            UtilFramework.Assert(UtilFramework.FolderName.StartsWith(UtilFramework.FolderNameExternal));

            // ExternalGit/ProjectName/
            string externalGitProjectNamePath = UtilFramework.FolderName.Substring(UtilFramework.FolderNameExternal.Length);

            // Application/App/
            string appSourceFolderName = UtilFramework.FolderName + "Application/App/";
            string appDestFolderName = UtilFramework.FolderNameExternal + "Application/App/" + externalGitProjectNamePath;

            // Application.Database/Database/
            string databaseSourceFolderName = UtilFramework.FolderName + "Application.Database/Database/";
            string databaseDestFolderName = UtilFramework.FolderNameExternal + "Application.Database/Database/" + externalGitProjectNamePath;

            // Application.Website/
            string websiteSourceFolderName = UtilFramework.FolderName + "Application.Website/";
            string websiteDestFolderName = UtilFramework.FolderNameExternal + "Application.Website/" + externalGitProjectNamePath;

            // Application.Cli/App/
            string cliAppSourceFolderName = UtilFramework.FolderName + "Application.Cli/App/";
            string cliAppDestFolderName = UtilFramework.FolderNameExternal + "Application.Cli/App/" + externalGitProjectNamePath;

            // Application.Cli/App/
            string cliDatabaseSourceFolderName = UtilFramework.FolderName + "Application.Cli/Database/";
            string cliDatabaseDestFolderName = UtilFramework.FolderNameExternal + "Application.Cli/Database/" + externalGitProjectNamePath;

            // Application.Cli/DeployDb/
            string cliDeployDbSourceFolderName = UtilFramework.FolderName + "Application.Cli/DeployDb/";
            string cliDeployDbDestFolderName = UtilFramework.FolderNameExternal + "Application.Cli/DeployDb/" + externalGitProjectNamePath;

            // Angular
            string websiteAngularDestFolderName = UtilFramework.FolderNameExternal + "Framework/Framework.Angular/application/src/Application.Website/";

            var result = new ExternalArgs
            {
                AppSourceFolderName = appSourceFolderName,
                AppDestFolderName = appDestFolderName,
                DatabaseSourceFolderName = databaseSourceFolderName,
                DatabaseDestFolderName = databaseDestFolderName,
                WebsiteSourceFolderName = websiteSourceFolderName,
                WebsiteDestFolderName = websiteDestFolderName,
                CliAppSourceFolderName = cliAppSourceFolderName,
                CliAppDestFolderName = cliAppDestFolderName,
                CliDatabaseSourceFolderName = cliDatabaseSourceFolderName,
                CliDatabaseDestFolderName = cliDatabaseDestFolderName,
                CliDeployDbSourceFolderName = cliDeployDbSourceFolderName,
                CliDeployDbDestFolderName = cliDeployDbDestFolderName,
                WebsiteAngularDestFolderName = websiteAngularDestFolderName
            };

            return result;
        }
    }
}
