﻿namespace Framework.BuildTool
{
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    public class CommandRunSqlCreate : Command
    {
        public CommandRunSqlCreate(AppBuildTool appBuildTool) 
            : base("runSqlCreate", "Run sql create scripts")
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

        public override void Run()
        {
            RunSqlCreate(ConnectionManagerServer.ConnectionString);
            if (OptionDrop.IsOn == false)
            {
                new CommandRunSqlMeta(AppBuildTool).Run();
            }
        }
    }
}