namespace Framework.Build.DataAccessLayer
{
    public static class ConnectionManager
    {
        public static string SchemaFileName
        {
            get
            {
                return Framework.Util.FolderName + "Framework/Build/DataAccessLayer/Sql/Schema.sql";
            }
        }

        public static string DatabaseLockFileName
        {
            get
            {
                return Framework.Util.FolderName + "Application/DataAccessLayer/Database.lock.cs";
            }
        }
    }
}
