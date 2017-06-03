namespace Framework.Server.Application.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Json Component.
    /// </summary>
    public class Component
    {
        public Component() : this(null, null) { }

        public Component(Component owner, string text)
        {
            Constructor(owner, text);
        }

        private void Constructor(Component owner, string text)
        {
            this.Type = GetType().Name;
            this.Text = text;
            if (owner != null)
            {
                if (owner.List == null)
                {
                    owner.List = new List<Component>();
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

        public List<Component> List = new List<Component>();

        private void ListAll(List<Component> result)
        {
            result.AddRange(List);
            foreach (var item in List)
            {
                item.ListAll(result);
            }
        }

        public List<Component> ListAll()
        {
            List<Component> result = new List<Component>();
            ListAll(result);
            return result;
        }

        public List<T> ListAll<T>() where T : Component
        {
            List<Component> result = ListAll();
            return result.OfType<T>().ToList();
        }

        protected virtual internal void Process(ApplicationServerBase applicationServer, ApplicationJson applicationJson)
        {

        }
    }

    /// <summary>
    /// Json Grid. Rendered as html div element.
    /// </summary>
    public class Grid : Component
    {
        public Grid() { }

        public Grid(Component owner, string text, string gridName)
            : base(owner, text)
        {
            this.GridName = gridName;
        }

        public string GridName;
    }

    /// <summary>
    /// Json GridKeyboard. Grid keyboard handler (Singleton). Manages grid navigation. For example arrow up, down and tab.
    /// </summary>
    public class GridKeyboard : Component
    {
        public GridKeyboard() { }

        public GridKeyboard(Component owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Json GridField.
    /// </summary>
    public class GridField : Component
    {
        public GridField() { }

        public GridField(Component owner, string text, string gridName, string fieldName, string gridIndex)
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
    /// Json GridData. There is also a GridDataServer object.
    /// </summary>
    public class GridDataJson
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
    /// Json GridCell.
    /// </summary>
    public class GridCell
    {
        /// <summary>
        /// Text.
        /// </summary>
        public string T;

        /// <summary>
        /// Gets or sets IsO. If true, original text has been stored in property O.
        /// </summary>
        public bool IsO;

        /// <summary>
        /// Original text. (Not user modified).
        /// </summary>
        public string O;

        /// <summary>
        /// Gets or sets error message.
        /// </summary>
        public string E;

        public bool IsSelect;

        public bool IsClick;
    }

    /// <summary>
    /// Json GridColumn.
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
    /// Json GridRow.
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

        /// <summary>
        /// Gets or sets error text for row.
        /// </summary>
        public string Error;
    }

    /// <summary>
    /// Json GridLoad. Parameter used to load grid.
    /// </summary>
    public class GridLoad
    {
        public string GridName;

        public string TypeRowName;

        public string FieldNameOrderBy;

        public bool IsOrderByDesc;
    }

    /// <summary>
    /// Json Application. Root object being transferred between server and client. There is also a ApplicationServer object.
    /// </summary>
    public class ApplicationJson : Component
    {
        public ApplicationJson()
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
        public GridDataJson GridDataJson;
    }

    /// <summary>
    /// Json LayoutContainer. Rendered as html div element.
    /// </summary>
    public class LayoutContainer : Component
    {
        public LayoutContainer() : this(null, null) { }

        public LayoutContainer(Component owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Json LayoutRow. Rendered as html div element.
    /// </summary>
    public class LayoutRow : Component
    {
        public LayoutRow() : this(null, null) { }

        public LayoutRow(LayoutContainer owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Json LayoutCell. Rendered as html div element.
    /// </summary>
    public class LayoutCell : Component
    {
        public LayoutCell() : this(null, null) { }

        public LayoutCell(LayoutRow owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Json Button. Rendered as html button element.
    /// </summary>
    public class Button : Component
    {
        public Button() : this(null, null) { }

        public Button(Component owner, string text)
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
    /// Json Literal. Rendered as html div element.
    /// </summary>
    public class Literal : Component
    {
        public Literal() : this(null, null) { }

        public Literal(Component owner, string text)
            : base(owner, text)
        {

        }

        public string Html;
    }

    /// <summary>
    /// Json Label.
    /// </summary>
    public class Label : Component
    {
        public Label() : this(null, null) { }

        public Label(Component owner, string text)
            : base(owner, text)
        {

        }
    }

    /// <summary>
    /// Json LabelGridSaveState.
    /// </summary>
    public class LabelGridSaveState : Label
    {
        public LabelGridSaveState() : this(null, null) { }

        public LabelGridSaveState(Component owner, string text)
            : base(owner, text)
        {
            TypeSet(typeof(Label)); // Render as Label.
        }

        protected internal override void Process(ApplicationServerBase applicationServer, ApplicationJson applicationJson)
        {
            Text = string.Format("IsModify={0};", applicationServer.ProcessListGet<ProcessGridSave>().IsModify);
            base.Process(applicationServer, applicationJson);
        }
    }
}
