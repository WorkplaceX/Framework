namespace Application.Cli
{
    using Application;
    using Database.dbo;
    using DatabaseIntegrate.dbo;
    using Framework.Cli;
    using Framework.Cli.Config;
    using Framework.DataAccessLayer;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Command line interface application.
    /// </summary>
    public class AppCliMain : AppCli
    {
        public AppCliMain() :
            base(
                typeof(HelloWorld).Assembly, // Register Application.Database dll
                typeof(AppMain).Assembly) // Register Application dll
        {

        }

        /// <summary>
        /// Set default values to create file ConfigCli.json if it does not exist.
        /// </summary>
        protected override void InitConfigCli(ConfigCli configCli)
        {
            string appTypeName = UtilCli.AppTypeName(typeof(AppMain));
            var folderNameAngular = File.Exists(UtilCli.FolderName + "Application.Website/") ? "Application.Website/" : "Framework/Framework.Cli/Template/Application.Website/";

            configCli.WebsiteList.Add(new ConfigCliWebsite()
            {
                DomainNameList = new List<ConfigCliWebsiteDomain>(new ConfigCliWebsiteDomain[] { new ConfigCliWebsiteDomain { EnvironmentName = "DEV", DomainName = "localhost", AppTypeName = appTypeName } }),
                FolderNameAngular = folderNameAngular,
            });

            // Default ConnectionString (Windows)
            configCli.EnvironmentGet().ConnectionString = "Data Source=localhost; Initial Catalog=Application; Integrated Security=True;";
        }

        /// <summary>
        /// Cli command generate.
        /// </summary>
        protected override void CommandGenerateIntegrate(GenerateIntegrateResult result)
        {
            // Hello World
            result.Add(Data.Query<HelloWorldIntegrate>().OrderBy(item => item.Name));
            result.AddKey<HelloWorld>(nameof(HelloWorld.Name));
        }

        /// <summary>
        /// Cli command deploy.
        /// </summary>
        protected override void CommandDeployDbIntegrate(DeployDbIntegrateResult result)
        {
            // Hello World
            result.Add(HelloWorldIntegrateAppCli.RowList);
        }
    }
}
