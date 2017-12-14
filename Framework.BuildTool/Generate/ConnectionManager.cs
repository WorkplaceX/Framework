namespace Framework.BuildTool.DataAccessLayer
{
    public static class ConnectionManager
    {
        public static string SchemaFileName
        {
            get
            {
                return UtilFramework.FolderName + "Submodule/Framework.BuildTool/Generate/Sql/Schema.sql";
            }
        }

        public static string DatabaseGenerateFileName
        {
            get
            {
                return UtilFramework.FolderName + "Application/Database/Database.Generate.cs";
            }
        }

        public static string DatabaseGenerateFrameworkFileName
        {
            get
            {
                return UtilFramework.FolderName + "Submodule/Framework/Database/Database.Generate.cs";
            }
        }
    }
}
