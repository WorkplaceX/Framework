namespace Framework.BuildTool.DataAccessLayer
{
    public static class ConnectionManager
    {
        public static string SchemaFileName
        {
            get
            {
                return Framework.UtilFramework.FolderName + "Submodule/Framework.BuildTool/DataAccessLayer/Sql/Schema.sql";
            }
        }

        public static string DatabaseGenerateFileName
        {
            get
            {
                return Framework.UtilFramework.FolderName + "Application/Database/Database.Generate.cs";
            }
        }
    }
}
