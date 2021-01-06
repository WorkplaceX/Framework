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
        /// Gets FolderNameExternal. This is the root folder name of the parent application, if this application is cloned into parents ExternalGit/ folder.
        /// </summary>
        public static string FolderNameExternal
        {
            get
            {
                if (!IsExternal)
                {
                    throw new Exception("This Application is not cloned into ExternalGit/ folder!");
                }
                return new Uri(new Uri(UtilFramework.FolderName), "../../").AbsolutePath;
            }
        }

        /// <summary>
        /// Gets IsExternalGit. Returns true if this application is cloned into ExternalGit/ folder.
        /// </summary>
        public static bool IsExternal
        {
            get
            {
                var folderName = new Uri(new Uri(UtilFramework.FolderName), "../").AbsolutePath;
                return folderName.EndsWith("/ExternalGit/");
            }
        }

        /// <summary>
        /// Returns ExternalProjectName without reading file ConfigCli. json. External file Config.Cli.json does not contain this value. See also file ConfigCli.json of host cli.
        /// </summary>
        public static string ExternalProjectName()
        {
            UtilFramework.Assert(IsExternal);

            UtilFramework.Assert(UtilFramework.FolderName.StartsWith(UtilExternal.FolderNameExternal));

            // ExternalGit/ProjectName/
            string externalGitProjectNamePath = UtilFramework.FolderName.Substring(UtilExternal.FolderNameExternal.Length);

            UtilFramework.Assert(externalGitProjectNamePath.StartsWith("ExternalGit/"));
            UtilFramework.Assert(externalGitProjectNamePath.EndsWith("/"));

            string result = externalGitProjectNamePath.Substring("ExternalGit/".Length);
            result = result.Substring(0, result.Length - 1);

            return result;
        }

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
        /// Collect rowList from external FrameworkConfigGridIntegrateAppCli (ConfigGrid).
        /// </summary>
        public static void CommandDeployDbIntegrate(AppCli appCli, List<FrameworkConfigGridIntegrate> rowList)
        {
            foreach (var type in appCli.AssemblyApplicationCli.GetTypes())
            {
                if (type.FullName.StartsWith("DatabaseIntegrate.dbo.FrameworkConfigGridIntegrateAppCli"))
                {
                    if (type.FullName != "DatabaseIntegrate.dbo.FrameworkConfigGridIntegrateAppCli")
                    {
                        var typeCliExternal = appCli.AssemblyApplicationCli.GetType(type.FullName);
                        var propertyInfo = typeCliExternal.GetProperty(nameof(FrameworkConfigGridIntegrateFramework.RowList));
                        var rowApplicationCliList = (List<FrameworkConfigGridIntegrate>)propertyInfo.GetValue(null);
                        rowList.AddRange(rowApplicationCliList);
                    }
                }
            }
        }

        /// <summary>
        /// Collect rowList from external FrameworkConfigFieldIntegrateAppCli (ConfigField).
        /// </summary>
        public static void CommandDeployDbIntegrate(AppCli appCli, List<FrameworkConfigFieldIntegrate> rowList)
        {
            foreach (var type in appCli.AssemblyApplicationCli.GetTypes())
            {
                if (type.FullName.StartsWith("DatabaseIntegrate.dbo.FrameworkConfigFieldIntegrateAppCli"))
                {
                    if (type.FullName != "DatabaseIntegrate.dbo.FrameworkConfigFieldIntegrateAppCli")
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
            // ExternalGit/ProjectName/
            string externalGitProjectNamePath = "ExternalGit/" + UtilExternal.ExternalProjectName() + "/";

            // Application/App/
            string appSourceFolderName = UtilFramework.FolderName + "Application/App/";
            string appDestFolderName = UtilExternal.FolderNameExternal + "Application/App/" + externalGitProjectNamePath;

            // Application.Database/Database/
            string databaseSourceFolderName = UtilFramework.FolderName + "Application.Database/Database/";
            string databaseDestFolderName = UtilExternal.FolderNameExternal + "Application.Database/Database/" + externalGitProjectNamePath;

            // Application.Website/
            string websiteSourceFolderName = UtilFramework.FolderName + "Application.Website/";
            string websiteDestFolderName = UtilExternal.FolderNameExternal + "Application.Website/" + externalGitProjectNamePath;

            // Application.Cli/App/
            string cliAppSourceFolderName = UtilFramework.FolderName + "Application.Cli/App/";
            string cliAppDestFolderName = UtilExternal.FolderNameExternal + "Application.Cli/App/" + externalGitProjectNamePath;

            // Application.Cli/App/
            string cliDatabaseSourceFolderName = UtilFramework.FolderName + "Application.Cli/Database/";
            string cliDatabaseDestFolderName = UtilExternal.FolderNameExternal + "Application.Cli/Database/" + externalGitProjectNamePath;

            // Application.Cli/DeployDb/
            string cliDeployDbSourceFolderName = UtilFramework.FolderName + "Application.Cli/DeployDb/";
            string cliDeployDbDestFolderName = UtilExternal.FolderNameExternal + "Application.Cli/DeployDb/" + externalGitProjectNamePath;

            // Angular
            string websiteAngularDestFolderName = UtilExternal.FolderNameExternal + "Framework/Framework.Angular/application/src/Application.Website/";

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
                WebsiteAngularDestFolderName = websiteAngularDestFolderName,
                ExternalProjectName = UtilExternal.ExternalProjectName(),
            };

            return result;
        }
    }
}
