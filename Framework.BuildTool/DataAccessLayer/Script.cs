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
        /// <param name="isFramework">If true, generate CSharp code for framework. For internal use only.</param>
        public static void Run(bool isFramework)
        {
            MetaSql metaSql = new MetaSql(isFramework);
            MetaCSharp metaCSharp = new MetaCSharp(metaSql);
            StringBuilder result = new StringBuilder();
            string cSharp;
            new CSharpGenerate(metaCSharp).Run(out cSharp);
            if (isFramework == false)
            {
                UtilDataAccessLayer.FileSave(ConnectionManager.DatabaseGenerateFileName, cSharp);
            }
            else
            {
                UtilDataAccessLayer.FileSave(ConnectionManager.DatabaseGenerateFrameworkFileName, cSharp);
            }
        }
    }
}
