namespace Framework.BuildTool
{
    using Framework.Application;
    using Framework.BuildTool.DataAccessLayer;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    public class CommandRunSqlColumn : Command
    {
        public CommandRunSqlColumn(AppBuildTool appBuildTool) 
            : base("runSqlColumn", "Run sql table FrameworkColumn update for all in source code defined columns.")
        {
            this.AppBuildTool = appBuildTool;
        }

        public readonly AppBuildTool AppBuildTool;

        private void RunSqlColumn()
        {
            UtilFramework.Log("### Start RunSqlColumn");
            foreach (Type typeRow in UtilDataAccessLayer.TypeRowList(UtilApplication.TypeRowInAssembly(AppBuildTool.App)))
            {
                foreach (Cell column in UtilDataAccessLayer.ColumnList(typeRow))
                {
                    // TODO UPSERT
                }
            }
            UtilFramework.Log("### Exit RunSqlColumn");
        }

        public override void Run()
        {
            RunSqlColumn();
        }
    }
}
