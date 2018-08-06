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
        /// <param name="isFrameworkDb">If true, generate CSharp code for framework (nternal use only) otherwise generate code for Application.</param>
        public static void Run(bool isFrameworkDb, AppCliBase appCli)
        {
            MetaSql metaSql = new MetaSql(isFrameworkDb, appCli);
            MetaCSharp metaCSharp = new MetaCSharp(metaSql);
            string cSharp;
            new CSharpGenerate(metaCSharp).Run(out cSharp);
            if (isFrameworkDb == false)
            {
                UtilGenerate.FileSave(UtilFramework.FolderName + "Application.Database/Generate.cs", cSharp);
            }
            else
            {
                UtilGenerate.FileSave(UtilFramework.FolderName + "Framework/Framework/Database/Generate.cs", cSharp);
            }
        }
    }
}
