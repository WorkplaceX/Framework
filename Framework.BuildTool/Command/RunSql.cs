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
            var fileNameList = Framework.Util.FileNameList(Framework.Util.FolderName + "BuildTool/Sql/");
            foreach (string fileName in fileNameList)
            {
                string text = Framework.Util.FileRead(fileName);
                var sqlList = text.Split(new string[] { "\r\nGO", "\nGO" }, StringSplitOptions.RemoveEmptyEntries);
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
