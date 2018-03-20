namespace UnitTest.Application
{
    using Database.UnitTest.Application;
    using Framework;
    using Framework.Application;
    using Framework.Component;
    using Framework.Server;
    using System;
    using System.Linq;

    public class UnitTestApplication : UnitTestBase
    {
        /// <summary>
        /// Filter for not nullable bool has to bee empty.
        /// </summary>
        public void GridNotNullableFilter()
        {
            Emulate.Process(out App app, out AppJson appJson);
            GridCell gridCell = Emulate.GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Filter);
            UtilFramework.Assert(gridCell.T == null);
            UtilFramework.Assert(gridCell.PlaceHolder == "Search");
        }

        /// <summary>
        /// User adds two rows to database.
        /// </summary>
        public void GridUserEnterRow()
        {
            Emulate.Process(out App app, out AppJson appJson);
            // User clicks cell and enters "X" text.
            Emulate.GridCellIsClick(appJson, out app, out appJson, MyRow.GridName, "Text", Index.New);
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "Text", Index.New, "X");
            GridCell gridCell = Emulate.GridCellGet(appJson, MyRow.GridName, "Text", new Index("0"));
            UtilFramework.Assert(gridCell.T == "X"); // New record entered by user.
            
            // User enters "Y" text.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "Text", Index.New, "Y");
            gridCell = Emulate.GridCellGet(appJson, MyRow.GridName, "Text", new Index("1"));
            UtilFramework.Assert(gridCell.T == "Y"); // New record entered by user.
        }

        /// <summary>
        /// User modifies text.
        /// </summary>
        public void GridUserModify()
        {
            Emulate.Process(out App app, out AppJson appJson);
            Guid? sessionOne = appJson.Session;
            // User enters "X2" text.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "Text", Index.Row(0), "X2");

            // Read back from new session
            Emulate.Process(out app, out appJson);
            Guid? sessionTow = appJson.Session;
            UtilFramework.Assert(sessionOne != sessionTow);
            GridCell gridCell = Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0));
            UtilFramework.Assert(gridCell.T == "X2"); // User modified text.
        }

        /// <summary>
        /// User filters data grid.
        /// </summary>
        public void UserGridFilter()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4); // With filter and new

            // User enters "X" text into filter.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "Text", Index.Filter, "X");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // User enters "" text into filter.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "Text", Index.Filter, "");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");
        }

        /// <summary>
        /// User clicks column sort.
        /// </summary>
        public void GridUserSort()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(Emulate.GridColumnGet(appJson, MyRow.GridName, "Text").Text == "Text");
            
            // Click column
            Emulate.GridColumnClick(appJson, out app, out appJson, MyRow.GridName, "Text");
            UtilFramework.Assert(Emulate.GridColumnGet(appJson, MyRow.GridName, "Text").Text.StartsWith("▲"));
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // Click column 2nd time
            Emulate.GridColumnClick(appJson, out app, out appJson, MyRow.GridName, "Text");
            UtilFramework.Assert(Emulate.GridColumnGet(appJson, MyRow.GridName, "Text").Text.StartsWith("▼"));
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "Y");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
        }

        /// <summary>
        /// User filters grid and clicks next page. Filter still applies.
        /// </summary>
        public void GridUserFilterThenNextPage()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User enters "X" text into filter.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "Text", Index.Filter, "X");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // User click next page button. Filter remains applied.
            Emulate.GridPageIndexNextClick(appJson, out app, out appJson, MyRow.GridName);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");

            // User click column header for sorting. Filter remains applied.
            Emulate.GridColumnClick(appJson, out app, out appJson, MyRow.GridName, "Text2");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 3);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "Text", Index.Row(0)).T == "X2");
        }

        public void GridUserFilter()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User writes "y" into filter of not nullable column.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActive", Index.Filter, "y");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 2);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Filter).T == "Yes"); // IsNamingConvention for bool cicked in!

            // User writes "" into filter of not nullable column. (Removes filter)
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActive", Index.Filter, "");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Filter).T == null);

            // User writes "y" into filter of nullable column.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActiveNullable", Index.Filter, "y");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 2);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Filter).T == "Yes"); // IsNamingConvention for bool cicked in!

            // User writes "" into filter of nullable column. (Removes filter)
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActiveNullable", Index.Filter, "");
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Filter).T == null);
        }

        public void GridUserInvalidValue()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User writes "y" into row of not nullable column.
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).T == "No");
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActive", Index.Row(0), "Yes");
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).T == "Yes");

            // User writes "" into row of not nullable column.
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActive", Index.Row(0), "");
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).T == null);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActive", Index.Row(0)).E == "Value invalid!");
        }

        public void GridUserNew()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 4);

            // User clicks cell and writes "y" into new row of nullable column.
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.New).T == null);
            Emulate.GridCellIsClick(appJson, out app, out appJson, MyRow.GridName, "IsActiveNullable", Index.New);
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActiveNullable", Index.New, "y");
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Row(2)).T == "Yes");
        }

        public void GridTextNull()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(MyRow.GridName).Count == 5);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Filter).T == null);
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.New).T == null);

            // User writes ""
            Emulate.GridCellTextSet(appJson, out app, out appJson, MyRow.GridName, "IsActiveNullable", Index.Row(0), "");
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "IsActiveNullable", Index.Row(0)).T == null);
        }

        public void GridUserLookup()
        {
            Emulate.Process(out App app, out AppJson appJson);
            UtilFramework.Assert(app.GridData.RowInternalList(Airport.GridName).Count == 2);

            // User enters two airports
            Emulate.GridCellIsClick(appJson, out app, out appJson, Airport.GridName, "Code", Index.New);
            Emulate.GridCellTextSet(appJson, out app, out appJson, Airport.GridName, "Code", Index.New, "LAX");
            Emulate.GridCellTextSet(appJson, out app, out appJson, Airport.GridName, "Text", Index.New, "Los Angeles");
            Emulate.GridCellIsClick(appJson, out app, out appJson, Airport.GridName, "Code", Index.New);
            Emulate.GridCellTextSet(appJson, out app, out appJson, Airport.GridName, "Code", Index.New, "DTW");
            Emulate.GridCellTextSet(appJson, out app, out appJson, Airport.GridName, "Text", Index.New, "Detroit Metropolitan Wayne County Airport");
            // UtilFramework.Assert(app.GridData.RowInternalList(Airport.GridName).Count == 4); // TODO
            UtilFramework.Assert(app.GridData.QueryInternalIsExist(Airport.GridNameLookup) == false); // No lookup data loaded
            UtilFramework.Assert(Emulate.GridCellGet(appJson, MyRow.GridName, "AirportCode", Index.Row(0)).IsLookup == false);
        }
    }

    public static class Emulate
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
            appJsonResponse = app.Run(appJsonRequest, Guid.NewGuid()); // Process rquest.
            //
            if (isDebugHtml)
            {
                if (isUniversalRunning == false)
                {
                    isUniversalRunning = true;
                    UtilServer.StartUniversalServer();
                }
                string json = Framework.Json.JsonConvert.Serialize(appJsonResponse, app.TypeComponentInNamespaceList());
                string url = "http://localhost:4000/Universal/index.js"; // Call Universal server when running in Visual Studio.
                string html = UtilServer.WebPost(url, json, true).Result;
                string fileName = UtilFramework.FolderName + "ApplicationDebug.html";
                UtilFramework.FileWrite(fileName, html);
            }
            //
            appResponse = new MyApp();
            appResponse.Run(appJsonResponse, Guid.NewGuid(), false); // Deserialize but do not run process.
        }

        /// <summary>
        /// First request of a session.
        /// </summary>
        public static void Process(out App appResponse, out AppJson appJsonResponse)
        {
            Process(null, out appResponse, out appJsonResponse);
        }

        public static GridCell GridCellGet(AppJson appJson, GridName gridName, string columnName, Index index)
        {
            string gridNameJson = GridName.ToJson(gridName);
            string indexJson = Index.ToJson(index);
            GridCell result = appJson.GridDataJson.CellList[gridNameJson][columnName][indexJson];
            return result;
        }

        public static GridColumn GridColumnGet(AppJson appJson, GridName gridName, string columnName)
        {
            string gridNameJson = GridName.ToJson(gridName);
            GridColumn result = appJson.GridDataJson.ColumnList[gridNameJson].Where(item => item.ColumnName == columnName).First();
            return result;
        }

        /// <summary>
        /// Emulate client component.ts of user entering text.
        /// </summary>
        private static void GridCellTextSet(AppJson appJsonRequest, GridName gridName, string columnName, Index index, string textNew)
        {
            GridCell gridCell = GridCellGet(appJsonRequest, gridName, columnName, index);
            // Backup old text.
            if (gridCell.IsO == null)
            {
                gridCell.IsO = true;
                gridCell.O = gridCell.T;
            }
            // New text back to original text.
            if (gridCell.IsO == true && gridCell.O == textNew)
            {
                gridCell.IsO = null;
                gridCell.O = null;
            }
            // TextOld (Not the same like Original! Used to detect IsDeleteKey)
            gridCell.TOld = textNew;

            gridCell.T = textNew;
            gridCell.IsModify = true;
            // GridSave icon.
            if (gridCell.CssClass == null || gridCell.CssClass.IndexOf("gridSave") == -1)
            {
                gridCell.CssClass += " gridSave";
            }
        }

        public static void GridCellTextSet(AppJson appJsonRequest, out App appResponse, out AppJson appJsonResponse, GridName gridName, string columnName, Index index, string textNew)
        {
            GridCellTextSet(appJsonRequest, gridName, columnName, index, textNew);
            Process(appJsonRequest, out appResponse, out appJsonResponse);
        }

        /// <summary>
        /// Emulate client component.ts of user clicking a cell.
        /// </summary>
        private static void GridCellIsClick(AppJson appJsonRequest, GridName gridName, string columnName, Index index)
        {
            GridCell gridCell = GridCellGet(appJsonRequest, gridName, columnName, index);
            gridCell.IsClick = true;
        }

        public static void GridCellIsClick(AppJson appJsonRequest, out App appResponse, out AppJson appJsonResponse, GridName gridName, string columnName, Index index)
        {
            GridCellIsClick(appJsonRequest, gridName, columnName, index);
            Process(appJsonRequest, out appResponse, out appJsonResponse);
        }

        /// <summary>
        /// Emulate client component.ts of user clicking column sort.
        /// </summary>
        private static void GridColumnClick(AppJson appJsonRequest, GridName gridName, string columnName)
        {
            GridColumn gridColumn = GridColumnGet(appJsonRequest, gridName, columnName);
            gridColumn.IsClick = true;
        }

        public static void GridColumnClick(AppJson appJsonRequest, out App appResponse, out AppJson appJsonResponse, GridName gridName, string columnName)
        {
            GridColumnClick(appJsonRequest, gridName, columnName);
            Process(appJsonRequest, out appResponse, out appJsonResponse);
        }

        private static void GridPageIndexNextClick(AppJson appJsonRequest, GridName gridName)
        {
            string gridNameJson = GridName.ToJson(gridName);
            appJsonRequest.GridDataJson.GridQueryList[gridNameJson].IsPageIndexNext = true;
        }

        public static void GridPageIndexNextClick(AppJson appJsonRequest, out App appResponse, out AppJson appJsonResponse, GridName gridName)
        {
            GridPageIndexNextClick(appJsonRequest, gridName);
            Process(appJsonRequest, out appResponse, out appJsonResponse);
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
            new Grid(app.AppJson, Airport.GridName);
        }
    }
}

namespace Database.UnitTest.Application
{
    using System.Linq;
    using Framework.Application;
    using Framework.DataAccessLayer;

    [SqlTable("dbo", "MyRow")]
    public class MyRow : Row
    {
        public static GridNameType GridName = new GridName<MyRow>();

        [SqlColumn("Id", null, true)]
        public int Id { get; set; }

        [SqlColumn("Text", typeof(MyRow_Text))]
        public string Text { get; set; }

        [SqlColumn("Text2", typeof(MyRow_Text))]
        public string Text2 { get; set; }

        [SqlColumn("IsActive", null)]
        public bool IsActive { get; set; }

        [SqlColumn("IsActiveNullable", null)]
        public bool? IsActiveNullable { get; set; }

        [SqlColumn("AirportCode", typeof(MyRow_AirportCode))]
        public string AirportCode { get; set; }

        public string AirportText { get; set; }
    }

    public class Airport : Row
    {
        public static GridNameType GridName = new GridName<Airport>();

        public static GridNameType GridNameLookup = new GridName<Airport>("Lookup");

        public int Id { get; set; }

        [SqlColumn("Code", null)]
        public string Code { get; set; }

        [SqlColumn("Text", null)]
        public string Text { get; set; }
    }

    public class MyRow_Text : Cell<MyRow>
    {

    }

    public class MyRow_AirportCode : Cell<MyRow>
    {
        protected internal override void Lookup(out GridNameType gridName, out IQueryable query, AppEventArg e)
        {
            gridName = Airport.GridNameLookup;
            query = UtilDataAccessLayer.Query<Airport>();
        }

        protected internal override void LookupIsClick(Row rowLookup, AppEventArg e)
        {
            base.LookupIsClick(rowLookup, e);
        }
    }
}
