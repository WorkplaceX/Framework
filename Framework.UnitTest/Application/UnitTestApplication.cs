namespace UnitTest.Application
{
    using Database.UnitTest.Application;
    using Framework;
    using Framework.Application;
    using Framework.Component;
    using Framework.Server;

    public class UnitTestApplication : UnitTestBase
    {
        /// <summary>
        /// Gets or sets isDebugHtml. Set manually to true. Writes every response into ApplicationDebug.html.
        /// </summary>
        private static bool isDebugHtml = false;

        private static bool isUniversalRunning;

        /// <summary>
        /// Process request.
        /// </summary>
        private static (App AppResponse, AppJson AppJsonResponse) Process(AppJson appJsonRequest)
        {
            UtilFramework.UnitTest(typeof(MyApp)); // Enable InMemory database.
            MyApp app = new MyApp();
            AppJson appJsonResponse = app.Run(appJsonRequest); // Process rquest.
            //
            if (isDebugHtml)
            {
                if (isUniversalRunning == false)
                {
                    isUniversalRunning = true;
                    UtilServer.StartUniversalServer();
                }
                string json = Framework.Json.JsonConvert.Serialize(appJsonResponse, app.TypeComponentInNamespace());
                string url = "http://localhost:4000/Universal/index.js"; // Call Universal server when running in Visual Studio.
                string html = UtilServer.WebPost(url, json, true).Result;
                string fileName = UtilFramework.FolderName + "ApplicationDebug.html";
                UtilFramework.FileWrite(fileName, html);
            }
            //
            App appResponse = new MyApp();
            appResponse.Run(appJsonResponse, false); // Deserialize but do not run process.
            return (appResponse, appJsonResponse);
        }

        private static GridCell GridCellGet(AppJson appJson, GridName gridName, string columnName, Index index)
        {
            string gridNameJson = UtilApplication.GridNameToJson(gridName);
            string indexJson = UtilApplication.IndexToJson(index);
            GridCell result = appJson.GridDataJson.CellList[gridNameJson][columnName][indexJson];
            return result;
        }

        /// <summary>
        /// Emulate client component.ts
        /// </summary>
        private static void GridCellTextSet(AppJson appJson, GridName gridName, string columnName, Index index, string textNew)
        {
            GridCell gridCell = GridCellGet(appJson, gridName, columnName, index);
            // Backup old text.
            if (gridCell.IsO == null)
            {
                gridCell.IsO = true;
                gridCell.O = gridCell.T;
            }
            // New text back to old text.
            if (gridCell.IsO == true && gridCell.O == textNew)
            {
                gridCell.IsO = null;
                gridCell.O = null;
            }
            // IsDeleteKey
            int textLength = gridCell.T == null ? 0 : gridCell.T.Length;
            int textNewLength = textNew == null ? 0 : textNew.Length;
            bool isDeleteKey = textNewLength < textLength;

            gridCell.T = textNew;
            gridCell.IsModify = true;
            gridCell.IsDeleteKey = isDeleteKey;
            // GridSave icon.
            if (gridCell.CssClass == null || gridCell.CssClass.IndexOf("gridSave") == -1)
            {
                gridCell.CssClass += " gridSave";
            }
        }

        /// <summary>
        /// Filter for not nullable bool has to bee empty.
        /// </summary>
        public void GridNotNullableFilter()
        {
            AppJson appJson = Process(null).AppJsonResponse;
            GridCell gridCell = GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Filter);
            UtilFramework.Assert(gridCell.T == null);
            UtilFramework.Assert(gridCell.PlaceHolder == "Search");
        }

        /// <summary>
        /// User adds two rows to database.
        /// </summary>
        public void GridUserEnterRow()
        {
            AppJson appJson = Process(null).AppJsonResponse;
            // User enters "X" text.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.New, "X");
            appJson = Process(appJson).AppJsonResponse;
            GridCell gridCell = GridCellGet(appJson, MyRow.GridName, "Text", new Index("0"));
            UtilFramework.Assert(gridCell.T == "X"); // New record entered by user.
            
            // User enters "Y" text.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.New, "Y");
            appJson = Process(appJson).AppJsonResponse;
            gridCell = GridCellGet(appJson, MyRow.GridName, "Text", new Index("1"));
            UtilFramework.Assert(gridCell.T == "Y"); // New record entered by user.
        }
    }
}

namespace UnitTest.Application
{
    using Framework.Application;
    using Framework.Component;
    using Database.UnitTest.Application;
    using System;

    public class MyApp : App
    {
        protected internal override Type TypePageMain()
        {
            return typeof(MyPage);
        }
    }

    public class MyPage : Page
    {
        protected internal override void InitJson(App app)
        {
            new Grid(app.AppJson, MyRow.GridName);
        }
    }
}

namespace Database.UnitTest.Application
{
    using Framework.Application;
    using Framework.DataAccessLayer;

    [SqlTable("dbo", "MyRow")]
    public class MyRow : Row
    {
        public static GridNameTypeRow GridName = new GridName<MyRow>();

        [SqlColumn(null, null, true)]
        public int Id { get; set; }

        [SqlColumn("Text", typeof(MyRow_Text))]
        public string Text { get; set; }

        [SqlColumn("Text2", typeof(MyRow_Text))]
        public string Text2 { get; set; }

        [SqlColumn("IsActive", null)]
        public bool IsActive { get; set; }

        [SqlColumn("IsActiveNullable", null)]
        public bool? IsActiveNullable { get; set; }
    }

    public class MyRow_Text : Cell<MyRow>
    {

    }
}
