namespace Framework.Cli.Generate
{
    using Framework.Cli.Command;

    /// <summary>
    /// Generate CSharp code for database tables.
    /// </summary>
    public static class Script
    {
        /// <summary>
        /// Script to generate CSharp code.
        /// </summary>
        /// <param name="isFrameworkDb">If true, generate CSharp code for framework library (internal use only) otherwise generate code for Application.</param>
        public static void Run(bool isFrameworkDb, AppCli appCli)
        {
            MetaSql metaSql = new MetaSql(isFrameworkDb, appCli);
            MetaCSharp metaCSharp = new MetaCSharp(metaSql);

            new CSharpGenerate(metaCSharp).Run(isFrameworkDb, out string cSharp);
            var builtInlist = appCli.CommandGenerateBuiltInListInternal();
            new GenerateCSharpBuiltIn().Run(out string cSharpCli, isFrameworkDb, isApplication: false, builtInList: builtInlist);
            new GenerateCSharpBuiltIn().Run(out string cSharpApplication, isFrameworkDb, isApplication: true, builtInList: builtInlist);
            if (isFrameworkDb == false)
            {
                UtilFramework.FileSave(UtilFramework.FolderName + "Application.Database/Database/Database.cs", cSharp);
                UtilFramework.FileSave(UtilFramework.FolderName + "Application.Database/Database/DatabaseBuiltIn.cs", cSharpApplication);
                UtilFramework.FileSave(UtilFramework.FolderName + "Application.Cli/Database/DatabaseBuiltIn.cs", cSharpCli);
            }
            else
            {
                UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework/Database/Database.cs", cSharp);
                UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework/Database/DatabaseBuiltIn.cs", cSharpApplication);
                UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework.Cli/Database/DatabaseBuiltIn.cs", cSharpCli);
            }
        }
    }
}
