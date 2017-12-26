namespace Framework.BuildTool.DataAccessLayer
{
    using System.Text;

    /// <summary>
    /// Generate CSharp code for database tables.
    /// </summary>
    public static class Script
    {
        /// <summary>
        /// Script to generate CSharp code.
        /// </summary>
        /// <param name="isFrameworkDb">If true, generate CSharp code for framework (nternal use only) otherwise generate code for Application.</param>
        public static void Run(bool isFrameworkDb, AppBuildTool appBuildTool)
        {
            MetaSql metaSql = new MetaSql(isFrameworkDb, appBuildTool);
            MetaCSharp metaCSharp = new MetaCSharp(metaSql);
            string cSharp;
            new CSharpGenerate(metaCSharp).Run(out cSharp);
            if (isFrameworkDb == false)
            {
                UtilGenerate.FileSave(ConnectionManager.DatabaseGenerateFileName, cSharp);
            }
            else
            {
                UtilGenerate.FileSave(ConnectionManager.DatabaseGenerateFrameworkFileName, cSharp);
            }
        }
    }
}
