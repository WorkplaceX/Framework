namespace Framework.Component
{
    using Framework.Application;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Json Component. Public fields and properties are serialized and deserialized.
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
            this.GridNameInternal = gridName;
            this.GridName = Framework.Application.GridName.ToJson(gridName);
        }

        /// <summary>
        /// Gets GridNameInternal. See also class ProcessGridLoad.
        /// </summary>
        internal readonly GridName GridNameInternal;

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
    /// Json GridField. Hosts GridCell. Displays currently selected cell.
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

        public GridFieldWithLabel(Component owner, string text, GridName gridName, string columnName)
            : base(owner)
        {
            this.Text = text;
            this.GridName = Framework.Application.GridName.ToJson(gridName);
            this.ColumnName = columnName;
        }

        public string Text;

        public readonly string GridName;

        public string ColumnName;

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
        /// (GridName, ColumnName, Index(Filter, 0..99, New, Total), GridCell)
        /// </summary>
        public Dictionary<string, Dictionary<string, Dictionary<string, GridCell>>> CellList;

        /// <summary>
        /// Selected grid cell.
        /// </summary>
        public string SelectGridName;

        /// <summary>
        /// Selected grid cell.
        /// </summary>
        public string SelectIndex;

        /// <summary>
        /// Selected grid cell.
        /// </summary>
        public string SelectColumnName;

        /// <summary>
        /// Selected grid cell. Before select has been updated based on IsClick. Used internaly for Lookup. Never sent to client.
        /// </summary>
        public string SelectGridNamePrevious;

        /// <summary>
        /// Selected grid cell. Before select has been updated based on IsClick. Used internaly for Lookup. Never sent to client.
        /// </summary>
        public string SelectIndexPrevious;

        /// <summary>
        /// Selected grid cell. Before select has been updated based on IsClick. Used internaly for Lookup. Never sent to client.
        /// </summary>
        public string SelectColumnNamePrevious;
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
        /// Text. Coming from client can be "" or null.
        /// </summary>
        public string T;

        /// <summary>
        /// Gets or sets IsO. If true, original text has been stored in property O.
        /// </summary>
        public bool? IsO;

        /// <summary>
        /// Original text. (Not user modified).
        /// </summary>
        public string O;

        /// <summary>
        /// Gets or sets error message.
        /// </summary>
        public string E;

        /// <summary>
        /// Gets or sets IsSelect.
        /// </summary>
        public bool IsSelect;

        /// <summary>
        /// Gets or sets IsLookup. If true lookup on cell is open.
        /// </summary>
        public bool IsLookup;

        /// <summary>
        /// Gets or sets GridNameLookup. This is the grid to display as lookup window.
        /// </summary>
        public string GridNameLookup;

        /// <summary>
        /// Gets or sets FocusId. GridCell can be displayed by multiple GridField. Focus has the one with FocusId. Used to show or hide Lookup.
        /// </summary>
        public int? FocusId;

        /// <summary>
        /// Gets FocusIdRequest. Sent by client if it got focus.
        /// </summary>
        public int? FocusIdRequest;

        public bool IsClick;

        /// <summary>
        /// Gets or sets IsModify. Indicating text has been modified by user on last request.
        /// </summary>
        public bool IsModify;

        /// <summary>
        /// Gets or sets IsDeleteKey. Sent by client indicating user pressed delete or backspace button.
        /// </summary>
        public bool IsDeleteKey;

        /// <summary>
        /// Gets or sets CellEnum. Render cell as button, html or file upload button.
        /// </summary>
        public GridCellEnum? CellEnum;

        /// <summary>
        /// Gets or sets custom html style classes. Used for example to display an indicator.
        /// </summary>
        public string CssClass;

        /// <summary>
        /// Gets or sets PlaceHolder. Text shown in cell, if no entry.
        /// </summary>
        public string PlaceHolder;
    }

    /// <summary>
    /// Json GridColumn.
    /// </summary>
    public class GridColumn
    {
        public string ColumnName;

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

        /// <summary>
        /// TableNameCSharp.
        /// </summary>
        public string TypeRow;

        public string ColumnNameOrderBy;

        public bool IsOrderByDesc;

        /// <summary>
        /// Gets or sets current database page index.
        /// </summary>
        public int PageIndex;

        /// <summary>
        /// Gets or sets current horizontal page index.
        /// </summary>
        public int PageHorizontalIndex;

        /// <summary>
        /// Gets or sets IsPageIndexNext. If true, next page has been clicked.
        /// </summary>
        public bool IsPageIndexNext;

        /// <summary>
        /// Gets or sets IsPageIndexPrevious. If true, previous page has been clicked.
        /// </summary>
        public bool IsPageIndexPrevious;

        /// <summary>
        /// Gets or sets IsPageHorizontalIndexNext. If true, next horizontal page has been clicked.
        /// </summary>
        public bool IsPageHorizontalIndexNext;

        /// <summary>
        /// Gets or sets IsPageHorizontalIndexPrevious. If true, previous horizontal page has been clicked.
        /// </summary>
        public bool IsPageHorizontalIndexPrevious;
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

        /// <summary>
        /// Gets or sets IsBrowser. If false, json has been rendered for universal and injected into html. If true, json has been rendered for client.
        /// </summary>
        public bool IsBrowser;

        public string VersionServer;

        public string VersionClient;

        public int RequestCount; // Set by client.

        public int ResponseCount;

        /// <summary>
        /// Gets or sets ErrorProcess. If error, browser reloads application. See also file dataService.ts
        /// </summary>
        public string ErrorProcess;

        public Guid? Session;

        /// <summary>
        /// Gets or sets RequestUrl. This value is set by the server.
        /// </summary>
        public string RequestUrl; // Used also for ClientLiveDevelopment.

        /// <summary>
        /// Gets or sets BrowserUrl. This value is set by the browser. It can be different from RequestUrl if application runs embeded in another webpage.
        /// </summary>
        public string BrowserUrl;

        /// <summary>
        /// Gets or sets GridData. Full save. AppJson is incremental save.
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
