namespace UnitTest.Application
{
    using Database.UnitTest.Application;
    using Framework;
    using Framework.Application;
    using Framework.Component;
    using Framework.Json;
    using Framework.Server;
    using System;
    using System.Linq;

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
        private static void Process(AppJson appJsonRequest, out App appResponse, out AppJson appJsonResponse)
        {
            // string jsonInText = appJson == null ? null : JsonConvert.Serialize(appJson, new Type[] { typeof(UtilFramework), typeof(UnitTestApplication) });
            // appJson = JsonConvert.Deserialize<AppJson>(jsonInText, new Type[] { typeof(UtilFramework), typeof(UnitTestApplication) });
            UtilFramework.UnitTest(typeof(MyApp)); // Enable InMemory database.
            MyApp app = new MyApp();
            appJsonResponse = app.Run(appJsonRequest); // Process rquest.
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
            appResponse = new MyApp();
            appResponse.Run(appJsonResponse, false); // Deserialize but do not run process.
        }

        private static GridCell GridCellGet(AppJson appJson, GridName gridName, string columnName, Index index)
        {
            string gridNameJson = UtilApplication.GridNameToJson(gridName);
            string indexJson = UtilApplication.IndexToJson(index);
            GridCell result = appJson.GridDataJson.CellList[gridNameJson][columnName][indexJson];
            return result;
        }

        private static GridColumn GridColumnGet(AppJson appJson, GridName gridName, string columnName)
        {
            string gridNameJson = UtilApplication.GridNameToJson(gridName);
            GridColumn result = appJson.GridDataJson.ColumnList[gridNameJson].Where(item => item.ColumnName == columnName).First();
            return result;
        }

        /// <summary>
        /// Emulate client component.ts of user entering text.
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
        /// Emulate client component.ts of user clicking column sort.
        /// </summary>
        private static void GridColumnClick(AppJson appJson, GridName gridName, string columnName)
        {
            GridColumn gridColumn = GridColumnGet(appJson, gridName, columnName);
            gridColumn.IsClick = true;
        }

        /// <summary>
        /// Filter for not nullable bool has to bee empty.
        /// </summary>
        public void GridNotNullableFilter()
        {
            Process(null, out App app, out AppJson appJson);
            GridCell gridCell = GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Filter);
            UtilFramework.Assert(gridCell.T == null);
            UtilFramework.Assert(gridCell.PlaceHolder == "Search");
        }

        /// <summary>
        /// User adds two rows to database.
        /// </summary>
        public void GridUserEnterRow()
        {
            Process(null, out App app, out AppJson appJson);
            // User enters "X" text.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.New, "X");
            Process(appJson, out app, out appJson);
            GridCell gridCell = GridCellGet(appJson, MyRow.GridName, "Text", new Index("0"));
            UtilFramework.Assert(gridCell.T == "X"); // New record entered by user.
            
            // User enters "Y" text.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.New, "Y");
            Process(appJson, out app, out appJson);
            gridCell = GridCellGet(appJson, MyRow.GridName, "Text", new Index("1"));
            UtilFramework.Assert(gridCell.T == "Y"); // New record entered by user.
        }

        /// <summary>
        /// User modifies text.
        /// </summary>
        public void GridUserModify()
        {
            Process(null, out App app, out AppJson appJson);
            Guid? sessionOne = appJson.Session;
            // User enters "X" text.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.Row(0), "X2");
            Process(appJson, out app, out appJson);

            // Read back from new session
            Process(null, out app, out appJson);
            Guid? sessionTow = appJson.Session;
            UtilFramework.Assert(sessionOne != sessionTow);
            GridCell gridCell = GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0));
            UtilFramework.Assert(gridCell.T == "X2"); // User modified text.
        }

        /// <summary>
        /// User filters data grid.
        /// </summary>
        public void UserGridFilter()
        {
            Process(null, out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4); // With filter and new
            
            // User enters "X" text into filter.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.Filter, "X");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // User enters "" text into filter.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.Filter, "");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");
        }

        /// <summary>
        /// User clicks column sort.
        /// </summary>
        public void GridUserSort()
        {
            Process(null, out App app, out AppJson appJson);
            UtilFramework.Assert(GridColumnGet(appJson, MyRow.GridName, "Text").Text == "Text");
            
            // Click column
            GridColumnClick(appJson, MyRow.GridName, "Text");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(GridColumnGet(appJson, MyRow.GridName, "Text").Text.StartsWith("▲"));
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // Click column 2nd time
            GridColumnClick(appJson, MyRow.GridName, "Text");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(GridColumnGet(appJson, MyRow.GridName, "Text").Text.StartsWith("▼"));
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "Y");
            // UtilFramework.Assert(response.App.GridData.RowList(MyRow.GridName).Count == 4);
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
