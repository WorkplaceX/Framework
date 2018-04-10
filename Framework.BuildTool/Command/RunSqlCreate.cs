﻿namespace Framework.BuildTool
{
    using Database.dbo;
    using Framework.BuildTool.DataAccessLayer;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class CommandRunSqlCreate : Command
    {
        public CommandRunSqlCreate(AppBuildTool appBuildTool) 
            : base("runSqlCreate", "Run sql create scripts and update FrameworkApplicationType (CSharp meta)")
        {
            this.AppBuildTool = appBuildTool;
            this.OptionDrop = OptionAdd("-d|--drop", "Run sql drop scripts");
        }

        public readonly AppBuildTool AppBuildTool;

        public readonly Option OptionDrop;

        /// <summary>
        /// Returns SqlCreate, SqlDrop folder name.
        /// </summary>
        /// <param name="isFrameworkDb">Framework or Application.</param>
        /// <param name="isDrop">Add SqlCreate or SqlDrop to folder name.</param>
        /// <param name="isName">Return folder name as name without drive information and SqlCreate folder.</param>
        private string FolderName(bool isFrameworkDb, bool isDrop, bool isName)
        {
            string result = null;
            if (isName == false)
            {
                result += UtilFramework.FolderName;
            }
            if (isFrameworkDb == false)
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

        /// <summary>
        /// Same file in SqlCreate and SqlDrop folder have the same name.
        /// </summary>
        private string FileNameToName(string fileName, bool isFrameworkDb, bool isDrop)
        {
            string folderNameFind = FolderName(isFrameworkDb, isDrop, false);
            string folderNameReplace = FolderName(isFrameworkDb, false, true);
            UtilFramework.Assert(fileName.StartsWith(folderNameFind));
            string result = fileName.Replace(folderNameFind, folderNameReplace);
            return result;
        }

        /// <summary>
        /// Same file in SqlCreate and SqlDrop folder have the same name.
        /// </summary>
        /// <param name="name">For example: (Submodule/Framework.BuildTool/v1.0/Framework.sql)</param>
        /// <returns>Returns for example: (C:/Temp/GitHub/ApplicationDemo/Submodule/Framework.BuildTool/SqlCreate/v1.0/Framework.sql)</returns>
        private string FileNameFromName(string name, bool isFrameworkDb, bool isDrop)
        {
            string folderNameFind = FolderName(isFrameworkDb, isDrop, true);
            string folderNameReplace = FolderName(isFrameworkDb, isDrop, false);
            UtilFramework.Assert(name.StartsWith(folderNameFind));
            string result = name.Replace(folderNameFind, folderNameReplace);
            return result;
        }

        /// <summary>
        /// Populate NameList. Returns (*.sql) scripts without "SqlCreate\" or "SqlDrop\" in path.
        /// </summary>
        private void NameList(bool isFrameworkDb, bool isDrop, ref string[] nameList)
        {
            string folderName = FolderName(isFrameworkDb, isDrop, false);
            var fileNameList = UtilFramework.FileNameList(folderName, "*.sql");
            var result = fileNameList.Select(item => FileNameToName(item, isFrameworkDb, isDrop)).ToArray();
            nameList = nameList.Union(result).Distinct().ToArray();
        }

        /// <summary>
        /// Returns true, if script is marked as IsRun on database.
        /// </summary>
        /// <param name="fileName">SqlCreate or SqlDrop script.</param>
        private bool IsRunGet(string fileName, bool isFrameworkDb, bool isDrop)
        {
            string name = FileNameToName(fileName, isFrameworkDb, isDrop);
            List<SqlParameter> paramList = new List<SqlParameter>();
            UtilDataAccessLayer.ExecuteParameterAdd("@Name", name, System.Data.SqlDbType.NVarChar, paramList);
            var resultSet = UtilDataAccessLayer.ExecuteReader("SELECT * FROM FrameworkScript WHERE Name = @Name", paramList);
            bool result = UtilDataAccessLayer.ExecuteResultCopy<FrameworkScript>(resultSet, 0).First().IsRun;
            //
            return result;
        }

        /// <summary>
        /// Set IsRun flag on database table FrameworkScript.
        /// </summary>
        private void IsRunSet(string fileName, bool isFrameworkDb, bool isDrop, bool value, DateTime dateTime)
        {
            string name = FileNameToName(fileName, isFrameworkDb, isDrop);
            object dateTimeCreate = DBNull.Value;
            object dateTimeDrop = DBNull.Value;
            if (value == true)
            {
                dateTimeCreate = dateTime;
            }
            else
            {
                dateTimeDrop = dateTime;
            }
            List<SqlParameter> paramList = new List<SqlParameter>();
            UtilDataAccessLayer.ExecuteParameterAdd("@IsRun", value, System.Data.SqlDbType.Bit, paramList);
            UtilDataAccessLayer.ExecuteParameterAdd("@Name", name, System.Data.SqlDbType.NVarChar, paramList);
            UtilDataAccessLayer.ExecuteParameterAdd("@DateCreate", dateTimeCreate, System.Data.SqlDbType.DateTime2, paramList);
            UtilDataAccessLayer.ExecuteParameterAdd("@DateDrop", dateTimeDrop, System.Data.SqlDbType.DateTime2, paramList);
            UtilBuildTool.SqlExecute(
                "UPDATE FrameworkScript SET IsRun = @IsRun, DateCreate = @DateCreate, DateDrop = @DateDrop WHERE Name = @Name",
                isFrameworkDb: true,
                paramList: paramList
            );
        }

        /// <summary>
        /// Execute SqlCreate, SqlDrop scripts.
        /// </summary>
        private void ScriptExecute(bool isFrameworkDb, bool isDrop)
        {
            DateTime dateTime = DateTime.Now;
            // Run existing scripts.
            {
                string folderName = FolderName(isFrameworkDb, isDrop, false);
                var fileNameList = UtilFramework.FileNameList(folderName, "*.sql").OrderBy(item => item);
                if (isDrop)
                {
                    fileNameList = fileNameList.OrderByDescending(item => item);
                }
                foreach (string fileName in fileNameList)
                {
                    bool isRun = IsRunGet(fileName, isFrameworkDb, isDrop);
                    UtilFramework.Log(string.Format("### Start RunSql={0}; OptionDrop={1}; IsRun={2};", fileName, OptionDrop.IsOn, isRun));
                    if (isRun == isDrop)
                    {
                        string text = UtilFramework.FileRead(fileName);
                        var sqlList = text.Split(new string[] { "\r\nGO", "\nGO", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                        UtilBuildTool.SqlExecute(sqlList.ToList(), isFrameworkDb);
                        IsRunSet(fileName, isFrameworkDb, isDrop, !isDrop, dateTime);
                    }
                    UtilFramework.Log(string.Format("### Exit RunSql={0}; OptionDrop={1}; IsRun={2};", fileName, OptionDrop.IsOn, isRun));
                }
            }
            // Set flag for not existing scripts.
            {
                string folderName = FolderName(isFrameworkDb, !isDrop, false);
                var fileNameList = UtilFramework.FileNameList(folderName, "*.sql").OrderBy(item => item);
                foreach (string fileName in fileNameList)
                {
                    string name = FileNameToName(fileName, isFrameworkDb, !isDrop);
                    string fileNameNotExist = FileNameFromName(name, isFrameworkDb, isDrop);
                    if (!File.Exists(fileNameNotExist))
                    {
                        IsRunSet(fileNameNotExist, isFrameworkDb, isDrop, !isDrop, dateTime);
                    }
                }
            }
        }

        /// <summary>
        /// Insert new scripts and mark missing ones as not exist.
        /// </summary>
        private void Upsert(string[] nameList)
        {
            UtilBuildTool.SqlExecute("UPDATE FrameworkScript SET IsExist = 0", true);
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
            UtilBuildTool.SqlExecute(sqlUpsert, true);
        }

        private void RunSqlCreate()
        {
            // Create table FrameworkScript
            string fileNameScript = UtilFramework.FolderName + "Submodule/Framework.BuildTool/Sql/Script.sql";
            string sql = UtilGenerate.FileLoad(fileNameScript);
            UtilBuildTool.SqlExecute(sql, true);
            //
            string[] nameList = new string[] { };
            NameList(false, false, ref nameList);
            NameList(false, true, ref nameList);
            NameList(true, false, ref nameList);
            NameList(true, true, ref nameList);
            //
            Upsert(nameList);
            //
            ScriptExecute(true, OptionDrop.IsOn);
            ScriptExecute(false, OptionDrop.IsOn);
        }

        public override void Run()
        {
            RunSqlCreate();
            if (OptionDrop.IsOn == false)
            {
                new CommandRunSqlMeta(AppBuildTool).Run();
            }
        }
    }
}
