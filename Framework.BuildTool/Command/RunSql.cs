namespace Framework.BuildTool
{
    using System;
    using System.Data.SqlClient;

    public class CommandRunSql : Command
    {
        public CommandRunSql() 
            : base("runSql", "Run sql scripts")
        {

        }

        private void RunSql(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            var fileNameList = Framework.UtilFramework.FileNameList(Framework.UtilFramework.FolderName + "BuildTool/Sql/", "*.sql");
            foreach (string fileName in fileNameList)
            {
                string text = Framework.UtilFramework.FileRead(fileName);
                var sqlList = text.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string sql in sqlList)
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public override void Run()
        {
            RunSql(Server.ConnectionManager.ConnectionString);
        }
    }
}
