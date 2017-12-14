namespace Framework.BuildTool
{
    using Framework.Application;
    using Framework.BuildTool.DataAccessLayer;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public class CommandRunSqlCreate : Command
    {
        public CommandRunSqlCreate(AppBuildTool appBuildTool) 
            : base("runSqlCreate", "Run sql create scripts and update FrameworkApplicationType (Meta)")
        {
            this.AppBuildTool = appBuildTool;
            this.OptionDrop = OptionAdd("-d|--drop", "Run sql drop scripts");
        }

        public readonly AppBuildTool AppBuildTool;

        public readonly Option OptionDrop;

        private void RunSqlCreate(string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                IEnumerable<string> fileNameList;
                if (OptionDrop.IsOn)
                {
                    var fileNameListFramework = UtilFramework.FileNameList(UtilFramework.FolderName + "Submodule/Framework.BuildTool/SqlDrop/", "*.sql").OrderByDescending(item => item);
                    var fileNameListApplication = UtilFramework.FileNameList(UtilFramework.FolderName + "BuildTool/SqlDrop/", "*.sql").OrderByDescending(item => item);
                    fileNameList = fileNameListApplication.Union(fileNameListFramework).ToArray();
                }
                else
                {
                    var fileNameListFramework = UtilFramework.FileNameList(UtilFramework.FolderName + "Submodule/Framework.BuildTool/SqlCreate/", "*.sql").OrderBy(item => item);
                    var fileNameListApplication = UtilFramework.FileNameList(UtilFramework.FolderName + "BuildTool/SqlCreate/", "*.sql").OrderBy(item => item);
                    fileNameList = fileNameListFramework.Union(fileNameListApplication).ToArray();
                }
                foreach (string fileName in fileNameList)
                {
                    UtilFramework.Log(string.Format("### Start RunSqlCreate {0} OptionDrop={1};", fileName, OptionDrop.IsOn));
                    string text = UtilFramework.FileRead(fileName);
                    var sqlList = text.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sql in sqlList)
                    {
                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            if ((command.ExecuteScalar() as string) == "RETURN") // Sql script with GO and RETURN at top would not stop. Therefore use SELECT 'RETURN' if there is GO statements.
                            {
                                break;
                            }
                        }
                    }
                    UtilFramework.Log(string.Format("### Exit RunSqlCreate {0} OptionDrop={1};", fileName, OptionDrop.IsOn));
                }
            }
        }

        /// <summary>
        /// Returns SqlCreate, SqlDrop path.
        /// </summary>
        /// <param name="isFramework">Framework or Application.</param>
        /// <param name="isDrop">Add SqlCreate or SqlDrop to path.</param>
        /// <param name="isName">Return path as name without drive information and SqlCreate folder.</param>
        private string FolderName(bool isFramework, bool isDrop, bool isName)
        {
            string result = null;
            if (isName == false)
            {
                result += UtilFramework.FolderName;
            }
            if (isFramework == false)
            {
                result += "BuildTool/";
            }
            else
            {
                result += "Submodule/Framework.BuildTool/";
            }
            if (isName == false)
            {
                if (isDrop == false)
                {
                    result += "SqlCreate/";
                }
                else
                {
                    result += "SqlDrop/";
                }
            }
            return result;
        }

        private string FileNameToName(string fileName, bool isFramework, bool isDrop)
        {
            string folderNameFind = FolderName(isFramework, isDrop, false);
            string folderNameReplace = FolderName(isFramework, false, true);
            UtilFramework.Assert(fileName.StartsWith(folderNameFind));
            string result = fileName.Replace(folderNameFind, folderNameReplace);
            return result;
        }

        /// <summary>
        /// Populate NameList.
        /// </summary>
        private void NameList(bool isFramework, bool isDrop, ref string[] nameList)
        {
            string folderName = FolderName(isFramework, isDrop, false);
            var fileNameList = UtilFramework.FileNameList(folderName);
            var result = fileNameList.Select(item => FileNameToName(item, isFramework, isDrop)).ToArray();
            nameList = nameList.Union(result).Distinct().ToArray();
        }

        private bool IsRun(string fileName, bool isFramework, bool isDrop)
        {
            string name = FileNameToName(fileName, isFramework, isDrop);
            UtilBuildTool.SqlCommand("SELECT * FROM FrameworkScript WHERE Name = @Name", (command) =>
            {
                command.Parameters.Add("Name", System.Data.SqlDbType.NVarChar).Value = name;
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                    }
                }
            });
            return false;
        }

        private void Execute(bool isFramework, bool isDrop)
        {
            string folderName = FolderName(isFramework, isDrop, false);
            var fileNameList = UtilFramework.FileNameList(folderName, "*.sql").OrderBy(item => item);
            foreach (string fileName in fileNameList)
            {
                UtilFramework.Log(string.Format("### Start RunSqlCreate {0} OptionDrop={1};", fileName, OptionDrop.IsOn));

                string text = UtilFramework.FileRead(fileName);
                var sqlList = text.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string sql in sqlList)
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        if ((command.ExecuteScalar() as string) == "RETURN") // Sql script with GO and RETURN at top would not stop. Therefore use SELECT 'RETURN' if there is GO statements.
                        {
                            break;
                        }
                    }
                }
                UtilFramework.Log(string.Format("### Exit RunSqlCreate {0} OptionDrop={1};", fileName, OptionDrop.IsOn));
            }
        }

        private void RunSqlCreate()
        {
            // Create table FrameworkScript
            string fileNameScript = UtilFramework.FolderName + "Submodule/Framework.BuildTool/Sql/Script.sql";
            string sql = UtilDataAccessLayer.FileLoad(fileNameScript);
            UtilBuildTool.SqlCommand(sql, (sqlCommand) => sqlCommand.ExecuteNonQuery());
            //
            string[] nameList = new string[] { };
            NameList(false, false, ref nameList);
            NameList(false, true, ref nameList);
            NameList(true, false, ref nameList);
            NameList(true, true, ref nameList);
            //
            string sqlUpsert = @"
            MERGE INTO FrameworkScript AS Target
            USING ({0}) AS Source
	            ON NOT EXISTS(
                    SELECT Source.Name
                    EXCEPT
                    SELECT Target.Name)
            WHEN MATCHED THEN
	            UPDATE SET Target.IsExist = 1
            WHEN NOT MATCHED BY TARGET THEN
	            INSERT (Name, IsExist, IsRun)
	            VALUES (Source.Name, 1, 0);
            ";
            StringBuilder sqlSelect = new StringBuilder();
            bool isFirst = true;
            foreach (string name in nameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlSelect.Append(" UNION ALL\r\n");
                }
                sqlSelect.Append(string.Format("(SELECT '{0}' AS Name)", name));
            }
            sqlUpsert = string.Format(sqlUpsert, sqlSelect.ToString());
            UtilBuildTool.SqlCommand(sqlUpsert, (command) => command.ExecuteNonQuery());
        }

        public override void Run()
        {
            RunSqlCreate();
            RunSqlCreate(ConnectionManagerServer.ConnectionString);
            if (OptionDrop.IsOn == false)
            {
                new CommandRunSqlMeta(AppBuildTool).Run();
            }
        }
    }
}
