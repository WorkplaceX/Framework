namespace Framework.Component
{
    using Framework.Application;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Json Component.
    /// </summary>
    public class Component
    {
        public Component() : this(null) { }

        public Component(Component owner)
        {
            Constructor(owner, null);
        }

        internal void Constructor(Component owner, Type type)
        {
            if (type != null)
            {
                this.Type = type.Name;
            }
            else
            {
                this.Type = GetType().Name;
            }
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
        /// Override default type. Used to change Angular Selector.
        /// </summary>
        internal void TypeSet(Type type)
        {
            Type = type.Name;
        }

        /// <summary>
        /// Gets or sets TypeCSharp. Used when default property Type has been changed.
        /// </summary>
        public string TypeCSharp;

        public bool IsHide;

        /// <summary>
        /// Gets or sets Name. Used when it is necessary to differentiate two instances.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets custom html style classes for this component.
        /// </summary>
        public string CssClass;

        /// <summary>
        /// Gets json list.
        /// </summary>
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

        private void Owner(Component component, ref Component result)
        {
            if (component.List.Contains(this))
            {
                result = component;
            }
            if (result != null)
            {
                foreach (var item in List)
                {
                    item.Owner(component, ref result);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns owner of this json component.
        /// </summary>
        /// <param name="component">Component to start search from top to down.</param>
        public Component Owner(Component component)
        {
            Component result = null;
            Owner(component, ref result);
            return result;
        }
    }

    /// <summary>
    /// Json Grid. Rendered as html div element.
    /// </summary>
    public class Grid : Component
    {
        public Grid() { }

        public Grid(Component owner, GridName gridName)
            : base(owner)
        {
            this.GridName = gridName.Value;
        }

        public readonly string GridName;
    }

    /// <summary>
    /// Json GridKeyboard. Grid keyboard handler (Singleton). Manages grid navigation. For example arrow up, down and tab.
    /// </summary>
    public class GridKeyboard : Component
    {
        public GridKeyboard() { }

        public GridKeyboard(Component owner)
            : base(owner)
        {

        }
    }

    /// <summary>
    /// Json GridField. Hosts GridCell. Displays currently focused cell.
    /// </summary>
    public class GridFieldSingle : Component
    {
        public GridFieldSingle() { }

        public GridFieldSingle(Component owner)
            : base(owner)
        {

        }
    }

    /// <summary>
    /// Json GridFieldWithLabel. Display Label and GridField horizontally in one component.
    /// </summary>
    public class GridFieldWithLabel : Component
    {
        public GridFieldWithLabel() { }

        public GridFieldWithLabel(Component owner, string text, GridName gridName, string fieldName)
            : base(owner)
        {
            this.Text = text;
            this.GridName = gridName.Value;
            this.FieldName = fieldName;
        }

        public string Text;

        public readonly string GridName;

        public string FieldName;

        public string Index;
    }

    /// <summary>
    /// Json GridData. There is also a GridData object.
    /// </summary>
    public class GridDataJson
    {
        /// <summary>
        /// (GridName, GridQuery) List of all loaded data grids.
        /// </summary>
        public Dictionary<string, GridQuery> GridQueryList;

        /// <summary>
        /// (GridName, GridRow)
        /// </summary>
        public Dictionary<string, List<GridRow>> RowList;

        /// <summary>
        /// (GridName, GridColumn)
        /// </summary>
        public Dictionary<string, List<GridColumn>> ColumnList;

        /// <summary>
        /// (GridName, FieldName, Index(Filter, 0..99, New, Total), GridCell)
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

        /// <summary>
        /// Focused grid cell. Before focus has been updated based on IsClick. Used internaly for Lookup. Never sent to client.
        /// </summary>
        public string FocusGridNamePrevious;

        /// <summary>
        /// Focused grid cell. Before focus has been updated based on IsClick. Used internaly for Lookup. Never sent to client.
        /// </summary>
        public string FocusIndexPrevious;

        /// <summary>
        /// Focused grid cell. Before focus has been updated based on IsClick. Used internaly for Lookup. Never sent to client.
        /// </summary>
        public string FocusFieldNamePrevious;
    }

    public enum GridCellEnum
    {
        None = 0,

        /// <summary>
        /// Cell is rendered as button. To set button text, override method CellValueToText();
        /// </summary>
        Button = 1,

        /// <summary>
        /// Cell text is rendered as sanitized html. Override method CellValueToText(); to set html.
        /// </summary>
        Html = 2,

        /// <summary>
        /// Cell is rendered as file upload button. Override method CellValueToText(); to set button text.
        /// </summary>
        FileUpload = 3
    }

    /// <summary>
    /// Json GridCell. Cell in data grid.
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

        /// <summary>
        /// Gets or sets IsFocus.
        /// </summary>
        public bool IsFocus;

        public bool IsLookup;

        public bool IsClick;

        /// <summary>
        /// Gets or sets IsModify. Indicating text has been modified by user on last request.
        /// </summary>
        public bool IsModify;

        /// <summary>
        /// Gets or sets CellEnum. Render cell as button, html or file upload button.
        /// </summary>
        public GridCellEnum? CellEnum;

        /// <summary>
        /// Gets or sets custom html style classes. Used for example to display an indicator.
        /// </summary>
        public string CssClass;

        /// <summary>
        /// Gets or sets PlaceHolder. Text shown in field, if no entry.
        /// </summary>
        public string PlaceHolder;
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

        public bool IsVisible;
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

        /// <summary>
        /// Gets or sets IsFilter. Used to display gray background color in filter row.
        /// </summary>
        public bool IsFilter;

        public bool IsSelectGet()
        {
            return UtilApplication.IsSelectGet(IsSelect);
        }

        public void IsSelectSet(bool value)
        {
            IsSelect = UtilApplication.IsSelectSet(IsSelect, value);
        }

        /// <summary>
        /// Gets or sets error text for row.
        /// </summary>
        public string Error;
    }

    /// <summary>
    /// Json GridQuery. Parameter used to load grid.
    /// </summary>
    public class GridQuery
    {
        public string GridName;

        public string TypeRow;

        public string FieldNameOrderBy;

        public bool IsOrderByDesc;
    }

    /// <summary>
    /// Json App. This is the application root json component being transferred between server and client. There is also a Application object.
    /// </summary>
    public class AppJson : Component
    {
        public AppJson()
            : base(null)
        {

        }

        public bool IsBrowser;

        public string VersionServer;

        public string VersionClient;

        public int RequestCount; // Set by client.

        public int ResponseCount;

        public string ErrorProcess;

        public Guid? Session;

        public string RequestUrl;

        /// <summary>
        /// Gets or sets GridData.
        /// </summary>
        public GridDataJson GridDataJson;
    }

    /// <summary>
    /// Json Div. Rendered as html div element.
    /// </summary>
    public class Div : Component
    {
        public Div() : this(null)
        {
            TypeSet(typeof(Div));
        }

        public Div(Component owner)
            : base(owner)
        {
            TypeSet(typeof(Div));
        }
    }

    /// <summary>
    /// Json LayoutContainer. Rendered as html div element.
    /// </summary>
    public class LayoutContainer : Div
    {
        public LayoutContainer() : this(null) { }

        public LayoutContainer(Div owner)
            : base(owner)
        {

        }
    }

    /// <summary>
    /// Json LayoutRow. Rendered as html div element.
    /// </summary>
    public class LayoutRow : Div
    {
        public LayoutRow() : this(null) { }

        public LayoutRow(Component owner)
            : base(owner)
        {

        }
    }

    /// <summary>
    /// Json LayoutCell. Rendered as html div element.
    /// </summary>
    public class LayoutCell : Div
    {
        public LayoutCell() : this(null) { }

        public LayoutCell(LayoutRow owner)
            : base(owner)
        {

        }
    }

    /// <summary>
    /// Json Button. Rendered as html button element.
    /// </summary>
    public class Button : Component
    {
        public Button() : this(null) { }

        public Button(Component owner)
            : base(owner)
        {

        }

        public string Text;

        public bool IsClick;
    }

    /// <summary>
    /// Json Literal. Rendered as html div element.
    /// </summary>
    public class Literal : Component
    {
        public Literal() : this(null) { }

        public Literal(Component owner)
            : base(owner)
        {

        }

        public string TextHtml;
    }

    /// <summary>
    /// Json Label. Text rendered inside html div. See also property CssClass (Use for example CssClass = "floatLeft").
    /// </summary>
    public class Label : Component
    {
        public Label() : this(null) { }

        public Label(Component owner)
            : base(owner)
        {

        }

        public string Text;
    }
}
