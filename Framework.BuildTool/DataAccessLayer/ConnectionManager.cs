namespace Framework.BuildTool.DataAccessLayer
{
    public static class ConnectionManager
    {
        public static string SchemaFileName
        {
            get
            {
                return Framework.UtilFramework.FolderName + "Submodule/Framework/Build/DataAccessLayer/Sql/Schema.sql";
            }
        }

        public static string DatabaseLockFileName
        {
            get
            {
                return Framework.UtilFramework.FolderName + "Application/DataAccessLayer/Database.lock.cs";
            }
        }
    }
}
