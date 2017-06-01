namespace Framework.Server.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

    /// <summary>
    /// Json data.
    /// </summary>
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

    /// <summary>
    /// Json data.
    /// </summary>
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

    /// <summary>
    /// Json data.
    /// </summary>
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

    /// <summary>
    /// Json data.
    /// </summary>
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
}
