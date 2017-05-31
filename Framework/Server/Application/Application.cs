namespace Framework.Server.Application
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Linq;

    public static class Util
    {
        private static List<GridColumn> GridToJsonColumnList(Type typeRow)
        {
            var result = new List<GridColumn>();
            //
            var cellList = Framework.Server.DataAccessLayer.Util.ColumnList(typeRow);
            double widthPercentTotal = 0;
            bool isLast = false;
            for (int i = 0; i < cellList.Count; i++)
            {
                // Text
                string text = cellList[i].FieldNameSql;
                cellList[i].ColumnText(ref text);
                // WidthPercent
                isLast = i == cellList.Count;
                double widthPercentAvg = Math.Round(((double)100 - widthPercentTotal) / ((double)cellList.Count - (double)i), 2);
                double widthPercent = widthPercentAvg;
                cellList[i].ColumnWidthPercent(ref widthPercent);
                widthPercent = Math.Round(widthPercent, 2);
                if (isLast)
                {
                    widthPercent = 100 - widthPercentTotal;
                }
                else
                {
                    if (widthPercentTotal + widthPercent > 100)
                    {
                        widthPercent = widthPercentAvg;
                    }
                }
                widthPercentTotal = widthPercentTotal + widthPercent;
                result.Add(new GridColumn() { FieldName = cellList[i].FieldNameSql, Text = text, WidthPercent = widthPercent });
            }
            return result;
        }

        public static void TypeRowValidate(Type typeRow, ref List<DataAccessLayer.Row> rowList)
        {
            if (rowList == null)
            {
                rowList = new List<DataAccessLayer.Row>();
            }
            foreach (DataAccessLayer.Row row in rowList)
            {
                Framework.Util.Assert(row.GetType() == typeRow);
            }
        }

        public static void GridToJson(JsonApplication jsonApplication, string gridName, Type typeRow, List<DataAccessLayer.Row> rowList)
        {
            GridData gridData = jsonApplication.GridData;
            //
            if (gridData.GridLoadList == null)
            {
                gridData.GridLoadList = new Dictionary<string, Application.GridLoad>();
            }
            gridData.GridLoadList[gridName] = new Application.GridLoad() { GridName = gridName, TypeRowName = DataAccessLayer.Util.TypeRowToName(typeRow) };
            // Row
            if (gridData.RowList == null)
            {
                gridData.RowList = new Dictionary<string, List<Application.GridRow>>();
            }
            gridData.RowList[gridName] = new List<GridRow>();
            // Column
            if (gridData.ColumnList == null)
            {
                gridData.ColumnList = new Dictionary<string, List<Application.GridColumn>>();
            }
            gridData.ColumnList[gridName] = GridToJsonColumnList(typeRow);
            // Cell
            if (gridData.CellList == null)
            {
                gridData.CellList = new Dictionary<string, Dictionary<string, Dictionary<string, Application.GridCell>>>();
            }
            gridData.CellList[gridName] = new Dictionary<string, Dictionary<string, Application.GridCell>>();
            //
            PropertyInfo[] propertyInfoList = null;
            for (int index = 0; index < rowList.Count; index++)
            {
                object row = rowList[index];
                gridData.RowList[gridName].Add(new GridRow() { Index = index.ToString() });
                if (propertyInfoList == null && typeRow != null)
                {
                    propertyInfoList = typeRow.GetTypeInfo().GetProperties();
                }
                if (propertyInfoList != null)
                {
                    foreach (PropertyInfo propertyInfo in propertyInfoList)
                    {
                        string fieldName = propertyInfo.Name;
                        object value = propertyInfo.GetValue(row);
                        string textJson = DataAccessLayer.Util.ValueToText(value); // Framework.Server.DataAccessLayer.Util.ValueToJson(value);
                        if (!gridData.CellList[gridName].ContainsKey(fieldName))
                        {
                            gridData.CellList[gridName][fieldName] = new Dictionary<string, GridCell>();
                        }
                        gridData.CellList[gridName][fieldName][index.ToString()] = new GridCell() { T = textJson };
                    }
                }
            }
        }

        /// <summary>
        /// Load data into CellList. Not visual.
        /// </summary>
        public static void GridToJson(JsonApplication jsonApplication, string gridName, Type typeRow)
        {
            List<DataAccessLayer.Row> rowList = Framework.Server.DataAccessLayer.Util.Select(typeRow, 0, 15);
            GridToJson(jsonApplication, gridName, typeRow, rowList);
        }

        public class Grid
        {
            public Grid(Type typeRow)
            {
                this.TypeRow = typeRow;
                this.RowList = new List<DataAccessLayer.Row>();
            }

            public List<DataAccessLayer.Row> RowList;

            public readonly Type TypeRow;
        }

        /// <summary>
        /// Returns row index. Excludes Header and Total.
        /// </summary>
        public static int? GridIndexFromJson(GridRow gridRow)
        {
            int result;
            if (int.TryParse(gridRow.Index, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static Grid GridFromJson(JsonApplication jsonApplication, string gridName, Type typeInAssembly)
        {
            GridData gridData = jsonApplication.GridData;
            //
            string typeRowName = gridData.GridLoadList[gridName].TypeRowName;
            Type typeRow = DataAccessLayer.Util.TypeRowFromName(typeRowName, typeInAssembly);
            Grid result = new Grid(typeRow);
            foreach (GridRow row in gridData.RowList[gridName])
            {
                DataAccessLayer.Row resultRow = (DataAccessLayer.Row)Activator.CreateInstance(typeRow);
                result.RowList.Add(resultRow);
                foreach (var column in gridData.ColumnList[gridName])
                {
                    string text = gridData.CellList[gridName][column.FieldName][row.Index].T;
                    PropertyInfo propertyInfo = typeRow.GetProperty(column.FieldName);
                    object value = DataAccessLayer.Util.ValueFromText(text, propertyInfo.PropertyType);
                    propertyInfo.SetValue(resultRow, value);
                }
            }
            return result;
        }
    }

    public class ProcessGridOrderBy : ProcessBase
    {
        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            foreach (string gridName in jsonApplication.GridData.ColumnList.Keys)
            {
                foreach (GridColumn gridColumn in jsonApplication.GridData.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        GridLoad gridLoad = jsonApplication.GridData.GridLoadList[gridName];
                        if (gridLoad.FieldNameOrderBy == gridColumn.FieldName)
                        {
                            gridLoad.IsOrderByDesc = !gridLoad.IsOrderByDesc;
                        }
                        else
                        {
                            gridLoad.FieldNameOrderBy = gridColumn.FieldName;
                            gridLoad.IsOrderByDesc = true;
                        }
                        break;
                    }
                }
            }
        }

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            foreach (string gridName in jsonApplication.GridData.ColumnList.Keys)
            {
                GridLoad gridLoad = jsonApplication.GridData.GridLoadList[gridName];
                foreach (GridColumn gridColumn in jsonApplication.GridData.ColumnList[gridName])
                {
                    gridColumn.IsClick = false;
                    if (gridColumn.FieldName == gridLoad.FieldNameOrderBy)
                    {
                        if (gridLoad.IsOrderByDesc)
                        {
                            gridColumn.Text = "▼" + " " + gridColumn.FieldName;
                        }
                        else
                        {
                            gridColumn.Text = "▲" + " " + gridColumn.FieldName;
                        }
                    }
                    else
                    {
                        gridColumn.Text = gridColumn.FieldName;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Call method Process(); on class JsonComponent.
    /// </summary>
    public class ProcessJson : ProcessBase
    {
        public ProcessJson(ApplicationBase application)
        {
            this.Application = application;
        }

        public readonly ApplicationBase Application;

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            foreach (JsonComponent jsonComponent in jsonApplication.ListAll())
            {
                jsonComponent.Process(Application, jsonApplication);
            }
        }
    }

    public class ProcessGridIsIsClick : ProcessBase
    {
        private void ProcessGridSelectRowClear(JsonApplication jsonApplicatio, string gridName)
        {
            foreach (GridRow gridRow in jsonApplicatio.GridData.RowList[gridName])
            {
                gridRow.IsSelectSet(false);
            }
        }

        private void ProcessGridSelectCell(JsonApplication jsonApplication, string gridName, string index, string fieldName)
        {
            GridData gridData = jsonApplication.GridData;
            gridData.FocusGridName = gridName;
            gridData.FocusIndex = index;
            gridData.FocusFieldName = fieldName;
            ProcessGridSelectCellClear(jsonApplication);
            gridData.CellList[gridName][fieldName][index].IsSelect = true;
        }

        private void ProcessGridSelectCellClear(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (string gridName in gridData.RowList.Keys)
            {
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    foreach (var gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsSelect = false;
                    }
                }
            }
        }

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (GridLoad gridLoad in gridData.GridLoadList.Values)
            {
                string gridName = gridLoad.GridName;
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        ProcessGridSelectRowClear(jsonApplication, gridName);
                        gridRow.IsSelectSet(true);
                    }
                    foreach (var gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            ProcessGridSelectCell(jsonApplication, gridName, gridRow.Index, gridColumn.FieldName);
                        }
                    }
                }
            }
        }

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (GridLoad gridLoad in gridData.GridLoadList.Values)
            {
                string gridName = gridLoad.GridName;
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    gridRow.IsClick = false;
                    foreach (var gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsClick = false;
                    }
                }
            }
        }
    }

    public class ProcessGridSave : ProcessBase
    {
        public ProcessGridSave(ApplicationBase application)
        {
           this.Application = application;
        }

        public readonly ApplicationBase Application;

        public bool IsModify;

        private void TextToValue(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (string gridName in gridData.GridLoadList.Keys)
            {
                var grid = Util.GridFromJson(jsonApplication, gridName, Application.GetType());
            }
        }

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            IsModify = false;
            GridData gridData = jsonApplication.GridData;
            foreach (string gridName in gridData.GridLoadList.Keys)
            {
                var grid = Util.GridFromJson(jsonApplication, gridName, Application.GetType());
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    foreach (GridColumn gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsO)
                        {
                            IsModify = true;
                        }
                    }
                }
            }
        }
    }

    public class ProcessGridRowFirstIsClick : ProcessBase
    {
        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (string gridName in gridData.RowList.Keys)
            {
                bool isSelect = false; // A row is selected
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    if (gridRow.IsSelectGet() || gridRow.IsClick)
                    {
                        isSelect = true;
                        break;
                    }
                }
                if (isSelect == false)
                {
                    foreach (GridRow gridRow in gridData.RowList[gridName])
                    {
                        int index;
                        if (int.TryParse(gridRow.Index, out index)) // Exclude "Header"
                        {
                            gridRow.IsClick = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    public class ProcessGridLookUp : ProcessBase
    {
        public ProcessGridLookUp(ApplicationBase application) 
        {
            this.Application = application;
        }

        public readonly ApplicationBase Application;

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            if (gridData.FocusFieldName != null)
            {
                var grid = Util.GridFromJson(jsonApplication, gridData.FocusGridName, Application.GetType());
                var row = grid.RowList[int.Parse(gridData.FocusIndex)];
                DataAccessLayer.Cell cell = DataAccessLayer.Util.CellList(row).Where(item => item.FieldNameCSharp == gridData.FocusFieldName).First();
                Type typeRow;
                List<DataAccessLayer.Row> rowList;
                cell.LookUp(out typeRow, out rowList);
                Util.TypeRowValidate(typeRow, ref rowList);
                Util.GridToJson(jsonApplication, "LookUp", typeRow, rowList);
                var d = Util.GridFromJson(jsonApplication, "LookUp", Application.GetType()); // TODO
            }
        }

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            bool isExist = false; // Focused field exists
            if (gridData.FocusFieldName != null)
            {
                if (gridData.RowList[gridData.FocusGridName].Exists(item => item.Index == gridData.FocusIndex)) // Focused row exists
                {
                    if (gridData.ColumnList[gridData.FocusGridName].Exists(item => item.FieldName == gridData.FocusFieldName)) // Focused column exists
                    {
                        isExist = true;
                    }
                }
            }
            if (isExist == false)
            {
                if (jsonApplication.GridData != null)
                {
                    jsonApplication.GridData.FocusFieldName = null;
                    jsonApplication.GridData.FocusGridName = null;
                    jsonApplication.GridData.FocusIndex = null;
                }
            }
        }
    }

    public abstract class ProcessBase
    {
        protected virtual internal void ProcessBegin(JsonApplication jsonApplication)
        {

        }

        protected virtual internal void ProcessBegin(JsonApplication jsonApplicationIn, JsonApplication jsonApplicationOut)
        {

        }

        protected virtual internal void ProcessEnd(JsonApplication jsonApplication)
        {

        }

        protected virtual internal void ProcessEnd(JsonApplication jsonApplicationIn, JsonApplication jsonApplicationOut)
        {

        }
    }

    /// <summary>
    /// Rendered as html div element.
    /// </summary>
    public class Grid : JsonComponent
    {
        public Grid() { }

        public Grid(JsonComponent owner, string text, string gridName)
            : base(owner, text)
        {
            this.GridName = gridName;
        }

        public string GridName;
    }

    /// <summary>
    /// Grid keyboard handler (Singleton). Manages grid navigation. For example arrow up, down and tab.
    /// </summary>
    public class GridKeyboard : JsonComponent
    {
        public GridKeyboard() { }

        public GridKeyboard(JsonComponent owner, string text)
            : base(owner, text)
        {

        }
    }

    public class GridField : JsonComponent
    {
        public GridField() { }

        public GridField(JsonComponent owner, string text, string gridName, string fieldName, string gridIndex)
            : base(owner, text)
        {
            this.GridName = gridName;
            this.FieldName = fieldName;
            this.GridIndex = gridIndex;
        }

        public string GridName;

        public string FieldName;

        public string GridIndex;
    }

    public class GridData
    {
        /// <summary>
        /// (GridName, GridLoad) List of all loaded data grids.
        /// </summary>
        public Dictionary<string, GridLoad> GridLoadList;

        /// <summary>
        /// (GridName, GridRow)
        /// </summary>
        public Dictionary<string, List<GridRow>> RowList;

        /// <summary>
        /// (GridName, GridColumn)
        /// </summary>
        public Dictionary<string, List<GridColumn>> ColumnList;

        /// <summary>
        /// (GridName, FieldName, Index(Filter, 0..99, Total), GridCell)
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>> CellList;

        /// <summary>
        /// Focused grid cell.
        /// </summary>
        public string FocusGridName;

        /// <summary>
        /// Focused grid cell.
        /// </summary>
        public string FocusIndex;

        /// <summary>
        /// Focused grid cell.
        /// </summary>
        public string FocusFieldName;
    }

    public class GridCell
    {
        /// <summary>
        /// Text.
        /// </summary>
        public string T;

        /// <summary>
        /// Gets or sets IsO. If true, old text has been stored in property O.
        /// </summary>
        public bool IsO;

        /// <summary>
        /// Old text.
        /// </summary>
        public string O;

        public bool IsSelect;

        public bool IsClick;
    }

    public class GridColumn
    {
        public string FieldName;

        public string Text;

        public double WidthPercent;

        /// <summary>
        /// Gets or sets IsClick. Used to switch the sort order of a data grid.
        /// </summary>
        public bool IsClick;

        /// <summary>
        /// Gets or sets IsUpdate. If true, postback to server is done after every key stroke. Used for example for Typeahead.
        /// (Currently in client handled always as true).
        /// </summary>
        public bool IsUpdate;
    }

    public class GridRow
    {
        public string Index;

        public bool IsClick;

        /// <summary>
        /// Bitwise (01=Select; 10=MouseOver; 11=Select and MouseOver).
        /// </summary>
        public int IsSelect;

        public bool IsSelectGet()
        {
            return (IsSelect & 1) == 1;
        }

        public void IsSelectSet(bool value)
        {
            if (value)
            {
                IsSelect = IsSelect | 1;
            }
            else
            {
                IsSelect = IsSelect & 2;
            }
        }
    }

    /// <summary>
    /// Parameter used to load grid.
    /// </summary>
    public class GridLoad
    {
        public string GridName;

        public string TypeRowName;

        public string FieldNameOrderBy;

        public bool IsOrderByDesc;
    }

    public class JsonApplication : JsonComponent
    {
        public JsonApplication()
            : base(null, "Json")
        {

        }

        /// <summary>
        /// GET not POST json when debugging client. See also file json.json.
        /// </summary>
        public bool IsJsonGet;

        public string Name;

        public Guid Session;

        public bool IsBrowser;

        public string VersionServer;

        public string VersionClient;

        public int RequestCount; // Set by client.

        public int ResponseCount;

        /// <summary>
        /// Gets or sets GridData.
        /// </summary>
        public GridData GridData;
    }

    public class JsonComponent
    {
        public JsonComponent() : this(null, null) { }

        public JsonComponent(JsonComponent owner, string text)
        {
            Constructor(owner, text);
        }

        private void Constructor(JsonComponent owner, string text)
        {
            this.Type = GetType().Name;
            this.Text = text;
            if (owner != null)
            {
                if (owner.List == null)
                {
                    owner.List = new List<JsonComponent>();
                }
                int count = 0;
                foreach (var item in owner.List)
                {
                    if (item.Key.StartsWith(this.Type + "-"))
                    {
                        count += 1;
                    }
                }
                this.Key = this.Type + "-" + count.ToString();
                owner.List.Add(this);
            }
        }

        /// <summary>
        /// Gets or sets Key. Used by Angular trackBy. Value is set by Framework before sending to client.
        /// </summary>
        public string Key;

        public string Type;

        /// <summary>
        /// Overwrite default type. Used to change Angular Selector.
        /// </summary>
        public void TypeSet(Type type)
        {
            Type = type.Name;
        }

        /// <summary>
        /// Gets or sets TypeCSharp. Used when default property Type has been changed.
        /// </summary>
        public string TypeCSharp;

        public string Text;

        /// <summary>
        /// Gets or sets custom html style classes.
        /// </summary>
        public string Class;

        public List<JsonComponent> List = new List<JsonComponent>();

        private void ListAll(List<JsonComponent> result)
        {
            result.AddRange(List);
            foreach (var item in List)
            {
                item.ListAll(result);
            }
        }

        public List<JsonComponent> ListAll()
        {
            List<JsonComponent> result = new List<JsonComponent>();
            ListAll(result);
            return result;
        }

        public List<T> ListAll<T>() where T : JsonComponent
        {
            List<JsonComponent> result = ListAll();
            return result.OfType<T>().ToList();
        }

        protected virtual internal void Process(ApplicationBase application, JsonApplication jsonApplication)
        {

        }
    }

    /// <summary>
    /// Rendered as html div element.
    /// </summary>
    public class LayoutContainer : JsonComponent
    {
        public LayoutContainer() : this(null, null) { }

        public LayoutContainer(JsonComponent owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Rendered as html div element.
    /// </summary>
    public class LayoutRow : JsonComponent
    {
        public LayoutRow() : this(null, null) { }

        public LayoutRow(LayoutContainer owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Rendered as html div element.
    /// </summary>
    public class LayoutCell : JsonComponent
    {
        public LayoutCell() : this(null, null) { }

        public LayoutCell(LayoutRow owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Rendered as html button element.
    /// </summary>
    public class Button : JsonComponent
    {
        public Button() : this(null, null) { }

        public Button(JsonComponent owner, string text)
            : base(owner, text)
        {
            if (IsClick)
            {
                Text += "."; // TODO
            }
        }

        public bool IsClick;
    }

    /// <summary>
    /// Rendered as html div element.
    /// </summary>
    public class Literal : JsonComponent
    {
        public Literal() : this(null, null) { }

        public Literal(JsonComponent owner, string text)
            : base(owner, text)
        {

        }

        public string Html;
    }

    public class Label : JsonComponent
    {
        public Label() : this(null, null) { }

        public Label(JsonComponent owner, string text)
            : base(owner, text)
        {

        }
    }

    public class LabelGridSaveState : Label
    {
        public LabelGridSaveState() : this(null, null) { }

        public LabelGridSaveState(JsonComponent owner, string text)
            : base(owner, text)
        {
            TypeSet(typeof(Label)); // Render as Label.
        }

        protected internal override void Process(ApplicationBase application, JsonApplication jsonApplication)
        {
            Text = string.Format("IsModify={0};", application.ProcessListGet<ProcessGridSave>().IsModify);
            base.Process(application, jsonApplication);
        }
    }

    public class ApplicationBase
    {
        public ApplicationBase()
        {
            this.ProcessInit();
        }

        public List<ProcessBase> ProcessList = new List<ProcessBase>();

        public void ProcessListInsertAfter<T>(ProcessBase process) where T : ProcessBase
        {
            int index = -1;
            int count = 0;
            foreach (var item in ProcessList)
            {
                if (item.GetType() == typeof(T))
                {
                    index = count;
                }
                count += 1;
            }
            Framework.Util.Assert(index != -1, "Process not found!");
            ProcessList.Insert(index, process);
        }

        public T ProcessListGet<T>() where T : ProcessBase
        {
            return (T)ProcessList.Where(item => item.GetType() == typeof(T)).First();
        }

        protected virtual void ProcessInit()
        {
            ProcessList.Add(new ProcessGridSave(this));
            ProcessList.Add(new ProcessGridRowFirstIsClick());
            ProcessList.Add(new ProcessGridIsIsClick());
            ProcessList.Add(new ProcessGridOrderBy());
            ProcessList.Add(new ProcessGridLookUp(this));
            ProcessList.Add(new ProcessJson(this));
        }

        public JsonApplication Process(JsonApplication jsonApplicationIn, string requestPath)
        {
            JsonApplication jsonApplicationOut = Framework.Server.DataAccessLayer.Util.JsonObjectClone<JsonApplication>(jsonApplicationIn);
            if (jsonApplicationOut == null || jsonApplicationOut.Session == Guid.Empty)
            {
                jsonApplicationOut = JsonApplicationCreate();
            }
            else
            {
                jsonApplicationOut.ResponseCount += 1;
            }
            jsonApplicationOut.Name = ".NET Core=" + DateTime.Now.ToString("HH:mm:ss.fff");
            jsonApplicationOut.VersionServer = Framework.Util.VersionServer;
            // Process
            {
                foreach (ProcessBase process in ProcessList)
                {
                    process.ProcessBegin(jsonApplicationIn, jsonApplicationOut);
                    process.ProcessBegin(jsonApplicationOut);
                }
                foreach (ProcessBase process in ProcessList)
                {
                    process.ProcessEnd(jsonApplicationIn, jsonApplicationOut);
                    process.ProcessEnd(jsonApplicationOut);
                }
            }
            return jsonApplicationOut;
        }

        protected virtual JsonApplication JsonApplicationCreate()
        {
            JsonApplication result = new JsonApplication();
            result.Session = Guid.NewGuid();
            //
            var container = new LayoutContainer(result, "Container");
            var rowHeader = new LayoutRow(container, "Header");
            var cellHeader1 = new LayoutCell(rowHeader, "HeaderCell1");
            var rowContent = new LayoutRow(container, "Content");
            var cellContent1 = new LayoutCell(rowContent, "ContentCell1");
            var cellContent2 = new LayoutCell(rowContent, "ContentCell2");
            new Label(cellContent2, "Enter text");
            var rowFooter = new LayoutRow(container, "Footer");
            var cellFooter1 = new LayoutCell(rowFooter, "FooterCell1");
            var button = new Button(cellFooter1, "Hello");
            //
            return result;
        }
    }
}
