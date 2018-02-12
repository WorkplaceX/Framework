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

        private static void GridPageIndexNextClick(AppJson appJson, GridName gridName)
        {
            string gridNameJson = UtilApplication.GridNameToJson(gridName);
            appJson.GridDataJson.GridQueryList[gridNameJson].IsPageIndexNext = true;
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
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
        }

        /// <summary>
        /// User filters grid and clicks next page. Filter still applies.
        /// </summary>
        public void GridUserFilterThenNextPage()
        {
            Process(null, out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User enters "X" text into filter.
            GridCellTextSet(appJson, MyRow.GridName, "Text", Index.Filter, "X");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // User click next page button. Filter remains applied.
            GridPageIndexNextClick(appJson, MyRow.GridName);
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // User click column header for sorting. Filter remains applied.
            GridColumnClick(appJson, MyRow.GridName, "Text2");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");
        }

        public void GridUserFilter()
        {
            Process(null, out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User writes "y" into filter of not nullable column.
            GridCellTextSet(appJson, MyRow.GridName, "IsActive", Index.Filter, "y");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 2);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Filter).T == "Yes"); // IsNamingConvention for bool cicked in!

            // User writes "" into filter of not nullable column. (Removes filter)
            GridCellTextSet(appJson, MyRow.GridName, "IsActive", Index.Filter, "");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Filter).T == "");

            // User writes "y" into filter of nullable column.
            GridCellTextSet(appJson, MyRow.GridName, "IsActiveNullable", Index.Filter, "y");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 2);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Filter).T == "Yes"); // IsNamingConvention for bool cicked in!

            // User writes "" into filter of nullable column. (Removes filter)
            GridCellTextSet(appJson, MyRow.GridName, "IsActiveNullable", Index.Filter, "");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Filter).T == "");
        }

        public void GridUserInvalidValue()
        {
            Process(null, out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User writes "y" into row of not nullable column.
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).T == "No"); 
            GridCellTextSet(appJson, MyRow.GridName, "IsActive", Index.Row(0), "Yes");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).T == "Yes");

            // User writes "" into row of not nullable column.
            GridCellTextSet(appJson, MyRow.GridName, "IsActive", Index.Row(0), "");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).T == "");
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).E == "Value invalid!");
        }

        public void GridUserNew()
        {
            Process(null, out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User writes "y" into new row of nullable column.
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.New).T == null);
            GridCellTextSet(appJson, MyRow.GridName, "IsActiveNullable", Index.New, "y");
            Process(appJson, out app, out appJson);
            UtilFramework.Assert(GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Row(2)).T == "Yes");
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
