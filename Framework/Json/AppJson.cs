namespace Framework.Json
{
    using Database.dbo;
    using Framework.App;
    using Framework.DataAccessLayer;
    using Framework.DataAccessLayer.DatabaseMemory;
    using Framework.Server;
    using Framework.Session;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Text;
    using System.Threading.Tasks;
    using static Framework.DataAccessLayer.UtilDalType;
    using static Framework.Json.Page;
    using static Framework.Session.UtilSession;

    internal enum RequestCommand
    {
        None = 0,

        ButtonIsClick = 1,

        GridIsClickSort = 2,

        GridIsClickConfig = 3,

        GridIsClickRow = 4,

        GridIsClickEnum = 5,

        GridCellIsModify = 6,

        BootstrapNavbarButtonIsClick = 7,

        Grid2IsClickSort = 8,

        Grid2CellIsModify = 9,
    }

    /// <summary>
    /// Request sent by Angular client.
    /// </summary>
    internal class RequestJson
    {
        public RequestCommand Command { get; set; }

        public int Grid2CellId { get; set; }

        public string Grid2CellText { get; set; }

        /// <summary>
        /// Gets or sets Id. This is ComponentJson.Id.
        /// </summary>
        public int ComponentId { get; set; }

        public int GridColumnId { get; set; }

        public int GridRowId { get; set; }

        public int GridCellId { get; set; }

        public GridIsClickEnum GridIsClickEnum { get; set; }

        public string GridCellText { get; set; }

        /// <summary>
        /// Gets GridCellTextIsInternal. If true, text has been set internally by look select row.
        /// </summary>
        public bool GridCellTextIsInternal; // TODO Command Queue

        public int BootstrapNavbarButtonId { get; set; }

        public int RequestCount { get; set; }

        public int ResponseCount { get; set; }

        public string BrowserUrl { get; set; }
    }

    /// <summary>
    /// Json component tree. Stores session state in public or internal fields and properties.
    /// </summary>
    public abstract class ComponentJson
    {
        /// <summary>
        /// Constructor to programmatically create new object.
        /// </summary>
        public ComponentJson(ComponentJson owner)
        {
            Constructor(owner, isDeserialize: false);
        }

        internal void Constructor(ComponentJson owner, bool isDeserialize)
        {
            this.Owner = owner;
            if (Owner == null)
            {
                this.Root = this;
                this.RootComponentJsonList = new Dictionary<int, ComponentJson>(); // Init list.
                this.RootReferenceList = new List<(object obj, UtilJson.DeclarationProperty property, int id)>();
            }
            else
            {
                this.Root = owner.Root;
            }
            if (!isDeserialize)
            {
                Root.RootIdCount += 1;
                this.Id = Root.RootIdCount;
                Root.RootComponentJsonList.Add(Id, this); // Id is not yet available if deserialize.
            }

            if (isDeserialize == false)
            {
                this.Type = GetType().Name;
                if (owner != null)
                {
                    if (owner.List == null)
                    {
                        owner.List = new List<ComponentJson>();
                    }
                    int count = 0;
                    foreach (var item in owner.List)
                    {
                        if (item.TrackBy.StartsWith(this.Type + "-"))
                        {
                            count += 1;
                        }
                    }
                    this.TrackBy = this.Type + "-" + count.ToString();
                    owner.List.Add(this);
                }
            }
        }

        [Serialize(SerializeEnum.Ignore)]
        public ComponentJson Owner { get; internal set; }

        [Serialize(SerializeEnum.Ignore)]
        internal bool IsRemoved;

        [Serialize(SerializeEnum.Ignore)]
        internal ComponentJson Root;

        internal int RootIdCount;

        /// <summary>
        /// (Id, ComponentJson).
        /// </summary>
        [Serialize(SerializeEnum.Ignore)]
        internal Dictionary<int, ComponentJson> RootComponentJsonList;

        /// <summary>
        /// (Object, Property, ReferenceId). Used for deserialization.
        /// </summary>
        [Serialize(SerializeEnum.Ignore)]
        internal List<(object obj, UtilJson.DeclarationProperty property, int id)> RootReferenceList;

        /// <summary>
        /// Solve ComponentJson references after deserialization.
        /// </summary>
        internal void RootReferenceSolve()
        {
            UtilFramework.Assert(Owner == null);
            UtilFramework.Assert(Root == this);
            foreach (var item in Root.RootReferenceList)
            {
                UtilFramework.Assert(item.property.IsList == false, "Reference to ComponentJson in List not supported!");
                ComponentJson componentJson = Root.RootComponentJsonList[item.id];
                item.property.ValueSet(item.obj, componentJson);
            }
        }

        /// <summary>
        /// Gets Id.
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Gets or sets Type. Used by Angular. See also <see cref="Page"/>.
        /// </summary>
        public string Type;

        public string TrackBy { get; internal set; }

        /// <summary>
        /// Gets or sets custom html style classes for this component.
        /// </summary>
        public string CssClass;

        /// <summary>
        /// Gets json list.
        /// </summary>
        public List<ComponentJson> List = new List<ComponentJson>(); // Empty list is removed by json serializer.

        /// <summary>
        /// Gets or sets IsHide. If true component is not sent to client.
        /// </summary>
        public bool IsHide;
    }

    /// <summary>
    /// Extension methods to manage json component tree.
    /// </summary>
    public static class ComponentJsonExtension
    {
        /// <summary>
        /// Returns owner of type T. Searches in parent and grand parents.
        /// </summary>
        public static T ComponentOwner<T>(this ComponentJson component) where T : ComponentJson
        {
            do
            {
                component = component.Owner;
                if (component is T)
                {
                    return (T)component;
                }
            } while (component != null);
            return null;
        }

        private static void ComponentListAll(ComponentJson component, List<ComponentJson> result)
        {
            result.AddRange(component.List);
            foreach (var item in component.List)
            {
                ComponentListAll(item, result);
            }
        }

        /// <summary>
        /// Returns list of all child components recursively including this.
        /// </summary>
        public static List<ComponentJson> ComponentListAll(this ComponentJson component)
        {
            List<ComponentJson> result = new List<ComponentJson>();
            result.Add(component);
            ComponentListAll(component, result);
            return result;
        }

        /// <summary>
        /// Returns all child components of type T.
        /// </summary>
        public static List<T> ComponentList<T>(this ComponentJson component) where T : ComponentJson
        {
            var result = new List<T>();
            foreach (var item in component.List)
            {
                if (UtilFramework.IsSubclassOf(item.GetType(), typeof(T)))
                {
                    result.Add((T)item);
                }
            }
            return result;
        }

        public enum PageShowEnum
        {
            None = 0,

            /// <summary>
            /// Add page to sibling pages.
            /// </summary>
            Default = 1,

            /// <summary>
            /// Remove sibling pages.
            /// </summary>
            SiblingRemove = 1,

            /// <summary>
            /// Hide sibling pages and keep their state.
            /// </summary>
            SiblingHide = 2,
        }

        /// <summary>
        /// Shows page or creates new one if it does not yet exist. Invokes also page init async.
        /// </summary>
        public static async Task<T> ComponentPageShowAsync<T>(this ComponentJson owner, T page, PageShowEnum pageShowEnum = PageShowEnum.Default, Action<T> init = null) where T : Page
        {
            T result = page;
            if (page != null && page.IsRemoved == false)
            {
                UtilFramework.Assert(page.Owner == owner, "Wrong Page.Owner!");
            }
            if (pageShowEnum == PageShowEnum.SiblingHide)
            {
                foreach (Page item in owner.List.OfType<Page>())
                {
                    item.IsHide = true; // Hide
                }
            }
            if (page == null || page.IsRemoved)
            {
                result = (T)Activator.CreateInstance(typeof(T), owner);
                init?.Invoke(result);
                await result.InitAsync();
            }
            result.IsHide = false; // Show
            if (pageShowEnum == PageShowEnum.SiblingRemove)
            {
                owner.List.OfType<Page>().ToList().ForEach(page =>
                {
                    if (page != result) { page.ComponentRemove(); }
                });
            }
            return result;
        }

        /// <summary>
        /// Creates new page. Invokes also page init async.
        /// </summary>
        public static Task<T> ComponentPageShowAsync<T>(this ComponentJson owner, PageShowEnum pageShowEnum = PageShowEnum.None, Action<T> init = null) where T : Page
        {
            return ComponentPageShowAsync<T>(owner, null, pageShowEnum, init);
        }

        /// <summary>
        /// Remove this component.
        /// </summary>
        public static void ComponentRemove(this ComponentJson component)
        {
            component?.Owner.List.Remove(component);
            component.Owner = null;
            component.IsRemoved = true;
        }

        /// <summary>
        /// Returns index of this component in parents list.
        /// </summary>
        public static int ComponentIndex(this ComponentJson component)
        {
            return component.Owner.List.IndexOf(component);
        }

        /// <summary>
        /// Returns count of this component parents list.
        /// </summary>
        public static int ComponentCount(this ComponentJson component)
        {
            return component.Owner.List.Count();
        }

        /// <summary>
        /// Move this component to index position.
        /// </summary>
        public static void ComponentMove(this ComponentJson component, int index)
        {
            var list = component?.Owner.List;
            list.Remove(component);
            list.Insert(index, component);
        }

        /// <summary>
        /// Move this component to last index.
        /// </summary>
        public static void ComponentMoveLast(this ComponentJson component)
        {
            component.ComponentMove(component.ComponentCount() - 1);
        }

        /// <summary>
        /// Returns currently selected row.
        /// </summary>
        public static Row GridRowSelected(this Grid grid)
        {
            Row result = null;
            if (grid.Index != null) // Loaded
            {
                int gridIndex = UtilSession.GridToIndex(grid);
                result = UtilServer.AppInternal.AppSession.GridSessionList[gridIndex].GridRowSessionList.Where(gridRowSession => gridRowSession.IsSelect).Select(item => item.Row).FirstOrDefault();
            }
            return result;
        }

        /// <summary>
        /// Returns currently selected row.
        /// </summary>
        public static T GridRowSelected<T>(this Grid grid) where T : Row
        {
            return (T)GridRowSelected(grid);
        }

        /// <summary>
        /// Add css class to ComponentJson.
        /// </summary>
        public static void CssClassAdd(this ComponentJson component, string value)
        {
            string cssClass = component.CssClass;
            string cssClassWholeWord = " " + cssClass + " ";
            if (!cssClassWholeWord.Contains(" " + value + " "))
            {
                if (UtilFramework.StringNull(cssClass) == null)
                {
                    component.CssClass = value;
                }
                else
                {
                    component.CssClass += " " + value;

                }
            }
        }

        /// <summary>
        /// Remove css class from ComponentJson.
        /// </summary>
        public static void CssClassRemove(this ComponentJson component, string value)
        {
            string cssClass = component.CssClass;
            string cssClassWholeWord = " " + cssClass + " ";
            if (cssClassWholeWord.Contains(" " + value + " "))
            {
                component.CssClass = cssClassWholeWord.Replace(" " + value + " ", "").Trim();
            }
        }
    }

    public class AppJson : Page
    {
        public AppJson()            
            : base(null)
        {

        }

        /// <summary>
        /// Returns NamingConvention for app related sql tables.
        /// </summary>
        protected virtual NamingConvention NamingConventionApp()
        {
            return new NamingConvention();
        }

        private NamingConvention namingConventionFramework;

        private NamingConvention namingConventionApp;

        internal NamingConvention NamingConventionInternal(Type typeRow)
        {
            if (UtilDalType.TypeRowIsFrameworkDb(typeRow))
            {
                if (namingConventionFramework == null)
                {
                    namingConventionFramework = new NamingConvention();
                }
            }
            if (namingConventionApp == null)
            {
                namingConventionApp = NamingConventionApp();
            }
            return namingConventionApp;
        }

        internal async Task InitInternalAsync()
        {
            await InitAsync();
            UtilServer.Session.SetString("Main", string.Format("App start: {0}", UtilFramework.DateTimeToString(DateTime.Now.ToUniversalTime())));
        }

        internal async Task ProcessInternalAsync()
        {
            UtilStopwatch.TimeStart("Process");
            await UtilServer.AppInternal.AppSession.ProcessAsync(); // Grid process
            await UtilApp.ProcessBootstrapNavbarAsync();

            foreach (Page page in UtilServer.AppJson.ComponentListAll().OfType<Page>())
            {
                await page.ProcessAsync();
            }

            UtilApp.ProcessBootstrapModal(); // Modal dialog window
            
            UtilApp.DivContainerRender();
            UtilServer.AppInternal.AppSession.GridRender(); // Grid render
            UtilApp.BootstrapNavbarRender();

            UtilStopwatch.TimeStop("Process");
        }

        /// <summary>
        /// Gets RequestJson. Payload of current request.
        /// </summary>
        [Serialize(SerializeEnum.Ignore)]
        internal RequestJson RequestJson;

        /// <summary>
        /// Gets or sets RequestCount. Used by client. Does not send new request while old is still pending.
        /// </summary>
        public int RequestCount { get; internal set; }

        /// <summary>
        /// Gets ResponseCount. Used by server to verify incoming request matches last response.
        /// </summary>
        public int ResponseCount { get; internal set; }

        /// <summary>
        /// Gets IsSessionExpired. If true, session expired and application has been recycled.
        /// </summary>
        public bool IsSessionExpired { get; internal set; }

        public string Version { get; set; }

        public string VersionBuild { get; set; }

        public bool IsServerSideRendering { get; set; }

        public string Session { get; set; }

        public string SessionApp { get; set; }

        /// <summary>
        /// Gets or sets IsModal. Indicating an object PageModal exists in the component tree. 
        /// Used for example for html "body class='modal-open'" to enable vertical scroll bar.
        /// </summary>
        public bool IsBootstrapModal { get; set; }

        /// <summary>
        /// Gets or sets IsReload. If true, client reloads page. For example if session expired.
        /// </summary>
        public bool IsReload { get; internal set; }

        /// <summary>
        /// Gets RequestUrl. This value is set by the server. For example: http://localhost:49323/". Used by client for app.json post. See also method: UtilServer.RequestUrl();
        /// </summary>
        public string RequestUrl { get; internal set; }

        /// <summary>
        /// Gets EmbeddedUrl. Value used by Angular client on first app.json POST to indicate application is embedded an running on other website.
        /// </summary>
        public string EmbeddedUrl { get; internal set; }
    }

    /// <summary>
    /// Json Button. Rendered as html button element.
    /// </summary>
    public sealed class Button : ComponentJson
    {
        public Button(ComponentJson owner)
            : base(owner)
        {

        }

        public string TextHtml;

        /// <summary>
        /// Gets IsClick. If true, user clicked the button.
        /// </summary>
        [Serialize(SerializeEnum.Ignore)]
        public bool IsClick
        {
            get
            {
                var requestJson = ((AppJson)Root).RequestJson;
                return requestJson.Command == RequestCommand.ButtonIsClick && requestJson.ComponentId == Id;
            }
        }
    }

    /// <summary>
    /// Json Div. Rendered as html div element.
    /// </summary>
    public sealed class Div : ComponentJson
    {
        public Div(ComponentJson owner)
            : base(owner)
        {

        }
    }

    /// <summary>
    /// Renders div with child divs without Angular selector div in between. Used for example for css flexbox, css grid and Bootstrap row.
    /// </summary>
    public sealed class DivContainer : ComponentJson
    {
        public DivContainer(ComponentJson owner)
            : base(owner)
        {

        }
    }

    /// <summary>
    /// See also: https://getbootstrap.com/docs/4.1/components/navbar/
    /// Change background color with style "background-color: red !important".
    /// </summary>
    public sealed class BootstrapNavbar : ComponentJson
    {
        public BootstrapNavbar(ComponentJson owner)
            : base(owner)
        {

        }

        public string BrandTextHtml;

        /// <summary>
        /// Gets or sets GridIndexList. Data grid row should have a field "Text" and "ParentId" for hierarchical navigation.
        /// </summary>
        public List<int> GridIndexList = new List<int>(); // Empty list is removed by json serializer.

        public List<BootstrapNavbarButton> ButtonList;

        /*
        <nav class="navbar-dark bg-primary sticky-top">
        <ul style="display: flex; flex-wrap: wrap"> // Wrapp if too many li.
        <a style="display:inline-block;">
        <i class="fas fa-spinner fa-spin text-white"></i>         
        */
    }

    public sealed class BootstrapNavbarButton
    {
        public int Id;

        /// <summary>
        /// Gets or sets GridIndex. For example navigation and language buttons can be shown in the Navbar.
        /// </summary>
        public int? GridIndex;

        /// <summary>
        /// RowIndex needs to be stored because unlike in the data grid sequence of buttons is different because filter row is omitted.
        /// </summary>
        public int RowIndex;

        public string TextHtml;

        public bool IsActive;

        /// <summary>
        /// Gets or sets IsDropDown. True, if button has level 2 navigation.
        /// </summary>
        public bool IsDropDown;

        public List<BootstrapNavbarButton> ButtonList;
    }

    public enum BootstrapAlertEnum
    {
        None = 0,

        Info = 1,

        Success = 2,

        Warning = 3,

        Error = 4
    }

    public static class BootstrapExtension
    {
        /// <summary>
        /// Show bootstrap alert (on per page).
        /// </summary>
        public static Html BootstrapAlert(this Page page, string textHtml, BootstrapAlertEnum alertEnum, int index = 0)
        {
            string htmlTextAlert = "<div class='alert {{CssClass}}' role='alert'>{{TextHtml}}</div>";
            string cssClass = null;
            switch (alertEnum)
            {
                case BootstrapAlertEnum.Info:
                    cssClass = "alert-info";
                    break;
                case BootstrapAlertEnum.Success:
                    cssClass = "alert-success";
                    break;
                case BootstrapAlertEnum.Warning:
                    cssClass = "alert-warning";
                    break;
                case BootstrapAlertEnum.Error:
                    cssClass = "alert-danger";
                    break;
                default:
                    break;
            }
            htmlTextAlert = htmlTextAlert.Replace("{{CssClass}}", cssClass).Replace("{{TextHtml}}", textHtml);
            Html result = new Html(page);
            result.TextHtml = htmlTextAlert;
            result.ComponentMove(index);
            return result;
        }
    }

    /// <summary>
    /// Represents the position of a data grid in the json component tree.
    /// </summary>
    public sealed class Grid : ComponentJson
    {
        public Grid(ComponentJson owner)
            : base(owner)
        {

        }

        /// <summary>
        /// Load data into grid. Override method Page.GridQuery(); to define query. It's also called to reload data.
        /// </summary>
        public async Task LoadAsync()
        {
            await UtilServer.AppInternal.AppSession.GridLoadAsync(this);
        }

        /// <summary>
        /// Gets Index. This is the grid session index. It's unique accross a session. Available once method LoadAsync(); has been called. See also class GridSession.
        /// </summary>
        public int? Index { get; internal set; }

        /// <summary>
        /// Gets or sets ConfigName. See also sql table FrameworkConfigGrid.
        /// </summary>
        public string ConfigName;

        public List<GridColumn> ColumnList;

        public List<GridRow> RowList;

        public GridIsClickEnum IsClickEnum;

        /// <summary>
        /// Gets or sets LookupDestGridIndex. If not null, this data gird is a lookup window with destination data grid LookupDestGridIndex.
        /// </summary>
        public int? LookupDestGridIndex;

        public int? LookupDestRowIndex;

        public int? LookupDestCellIndex;

        /// <summary>
        /// Returns true, if grid is a Lookup grid.
        /// </summary>
        internal bool GridLookupIsOpen()
        {
            return LookupDestGridIndex != null && this.Owner is Grid;
        }

        /// <summary>
        /// Returns Lookup window for this data grid.
        /// </summary>
        internal Grid GridLookup()
        {
            if (List.Count == 0 || !(List[0] is Grid))
            {
                List.Clear();
                new Grid(this);
            }
            return (Grid)List[0];
        }

        /// <summary>
        /// Opens Lookup for this grid.
        /// </summary>
        internal void GridLookupOpen(GridItem gridItem, GridRowItem gridRowItem, GridCellItem gridCellItem)
        {
            UtilFramework.Assert(UtilSession.GridToIndex(this) == gridItem.GridIndex);

            int gridIndex = UtilSession.GridToIndex(this);
            int rowIndex = gridRowItem.RowIndex;
            int cellIndex = gridCellItem.CellIndex;

            Grid lookup = GridLookup();
            lookup.LookupDestGridIndex = gridIndex;
            lookup.LookupDestRowIndex = rowIndex;
            lookup.LookupDestCellIndex = cellIndex;

            GridLookupClose(gridItem);
            gridCellItem.GridCellSession.IsLookup = true;
        }

        /// <summary>
        /// Closes Lookup of this grid.
        /// </summary>
        internal void GridLookupClose(GridItem gridItem, bool isForce = false)
        {
            foreach (GridRowItem gridRowItem in gridItem.GridRowList)
            {
                foreach (GridCellItem gridCellItem in gridRowItem.GridCellList)
                {
                    if (gridCellItem.GridCellSession.IsLookup)
                    {
                        gridCellItem.GridCellSession.IsLookup = false;
                        if (isForce)
                        {
                            gridCellItem.GridCellSession.IsLookupCloseForce = true;
                        }
                    }
                }
            }
        }
    }

    public class Grid2 : ComponentJson
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Grid2(ComponentJson owner) 
            : base( owner)
        {

        }

        private void Render()
        {
            Page page = this.ComponentOwner<Page>();
            CellList = new List<Grid2Cell>();
            RowStateList = new List<Grid2RowState>();
            StringBuilder styleColumnList = new StringBuilder();
            int cellId = 0;
            int rowStateId = 0;
            // Render Filter
            RowStateList.Add(new Grid2RowState { Id = rowStateId += 1 });
            foreach (var column in ColumnList)
            {
                styleColumnList.Append("minmax(0, 1fr) ");
                CellList.Add(new Grid2Cell
                {
                    Id = cellId += 1,
                    ColumnId = column.Id,
                    RowStateId = rowStateId,
                    RowEnum = GridRowEnum.Filter,
                    ColumnText = column.ColumnText,
                    Placeholder = "Search",
                    IsSort = column.IsSort,
                });
            }
            // Render Index
            var fieldList = UtilDalType.TypeRowToFieldListDictionary(TypeRow);
            int rowId = 0;
            foreach (var row in RowList)
            {
                RowStateList.Add(new Grid2RowState { Id = rowStateId += 1, RowId = rowId += 1 });
                foreach (var column in ColumnList)
                {
                    var field = fieldList[column.FieldNameCSharp];
                    string text = null;
                    object value = field.PropertyInfo.GetValue(row);
                    if (value != null)
                    {
                        text = page.GridCellText(this, row, field.PropertyInfo.Name); // Custom convert database value to cell text.
                        text = UtilFramework.StringNull(text);
                        if (text == null)
                        {
                            text = field.FrameworkType().CellTextFromValue(value);
                        }
                    }
                    CellList.Add(new Grid2Cell
                    {
                        Id = cellId += 1,
                        ColumnId = column.Id,
                        RowStateId = rowStateId,
                        RowEnum = GridRowEnum.Index,
                        Text = text,
                    });
                }
            }
            // Render New
            RowStateList.Add(new Grid2RowState { Id = rowStateId += 1 });
            foreach (var column in ColumnList)
            {
                CellList.Add(new Grid2Cell
                {
                    Id = cellId += 1,
                    ColumnId = column.Id,
                    RowStateId = rowStateId,
                    RowEnum = GridRowEnum.New,
                    Placeholder = "New",
                });
            }
            StyleColumn = styleColumnList.ToString();
        }

        private static async Task ProcessIsClickSortAsync()
        {
            if (UtilSession.Request<Grid2>(RequestCommand.Grid2IsClickSort, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                Grid2Column column = grid.ColumnList[cell.ColumnId - 1];
                // Reset sort on other columns
                foreach (var item in grid.ColumnList)
                {
                    if (item != column)
                    {
                        item.IsSort = null;
                    }
                }
                if (column.IsSort == null)
                {
                    column.IsSort = false;
                }
                else
                {
                    column.IsSort = !column.IsSort;
                }
                await grid.ReloadAsync();
                grid.Render();
            }
        }

        private static void ProcessCellIsModifyWarning(Grid2 grid, Grid2Cell cell)
        {
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    if (item.IsModified)
                    {
                        if (item.ErrorParse == null && item.ErrorSave == null)
                        {
                            item.Warning = "Not saved because of other errors!";
                        }
                    }
                    else
                    {
                        item.Warning = null;
                    }
                }
            }
        }

        /// <summary>
        /// Call after successful save.
        /// </summary>
        private static void ProcessCellIsModifyReset(Grid2 grid, Grid2Cell cell)
        {
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    item.IsModified = false;
                    item.TextOld = null;
                    item.ErrorParse = null;
                    item.ErrorSave = null;
                    item.Warning = null;
                }
            }
        }

        /// <summary>
        /// Parse
        /// </summary>
        private static void ProcessCellIsModifyParse(Grid2 grid, Page page, Row rowNew, Grid2Column column, Field field, Grid2Cell cell)
        {
            cell.ErrorParse = null;
            // Parse
            try
            {
                // Validate
                if (cell.Text == null)
                {
                    if (!UtilFramework.IsNullable(field.PropertyInfo.PropertyType))
                    {
                        throw new Exception("Value can not be null!");
                    }
                }
                // Parse custom
                bool isHandled = false;
                if (cell.Text != null)
                {
                    page.GridCellParse(grid, column.FieldNameCSharp, cell.Text, rowNew, out isHandled); // Custom parse of user entered text.
                }
                // Parse default
                if (!isHandled)
                {
                    Data.CellTextParse(field, cell.Text, rowNew, out string errorParse);
                    cell.ErrorParse = errorParse;
                }
            }
            catch (Exception exception)
            {
                cell.ErrorParse = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Save (Update).
        /// </summary>
        private static async Task ProcessCellIsModifyUpdateAsync(Grid2 grid, Page page, Row row, Row rowNew, Grid2Cell cell)
        {
            // Save
            try
            {
                bool isHandled = await page.GridUpdateAsync(grid, row, rowNew, grid.DatabaseEnum);
                if (!isHandled)
                {
                    await Data.UpdateAsync(row, rowNew, grid.DatabaseEnum);
                }
            }
            catch (Exception exception)
            {
                cell.ErrorSave = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Save (Insert).
        /// </summary>
        private static async Task ProcessCellIsModifyInsertAsync(Grid2 grid, Page page, Row rowNew, Grid2Cell cell)
        {
            // Save
            try
            {
                // Save custom
                bool isHandled = await page.GridInsertAsync(grid,  rowNew, grid.DatabaseEnum);
                if (!isHandled)
                {
                    // Save default
                    await Data.InsertAsync(rowNew, grid.DatabaseEnum);
                }
            }
            catch (Exception exception)
            {
                cell.ErrorSave = UtilFramework.ExceptionToString(exception);
            }
        }

        /// <summary>
        /// Reset ErrorSave on all cells on row.
        /// </summary>
        private static void ProcessCellIsModifyErrorSaveReset(Grid2 grid, Grid2Cell cell)
        {
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    item.ErrorSave = null;
                }
            }
        }

        /// <summary>
        /// Returns true, if row has a not solved parse error.
        /// </summary>
        private static bool ProcessCellIsModifyIsErrorParse(Grid2 grid, Grid2Cell cell)
        {
            bool result = false;
            foreach (var item in grid.CellList)
            {
                if (item.RowStateId == cell.RowStateId)
                {
                    result = result || item.ErrorParse != null;
                }
            }
            return result;
        }

        /// <summary>
        /// Track IsModified and TextOld.
        /// </summary>
        private static void ProcessCellIsModifyTextOld(Grid2Cell cell, RequestJson requestJson)
        {
            if (cell.IsModified == false)
            {
                cell.IsModified = true;
                cell.TextOld = cell.Text;
            }
            else
            {
                if (requestJson.Grid2CellText == cell.TextOld)
                {
                    cell.IsModified = false;
                    cell.TextOld = null;
                }
            }
        }

        private static async Task ProcessCellIsModify()
        {
            if (UtilSession.Request<Grid2>(RequestCommand.Grid2CellIsModify, out RequestJson requestJson, out Grid2 grid))
            {
                Grid2Cell cell = grid.CellList[requestJson.Grid2CellId - 1];
                Grid2Column column = grid.ColumnList[cell.ColumnId - 1];
                var field = UtilDalType.TypeRowToFieldListDictionary(grid.TypeRow)[column.FieldNameCSharp];
                Page page = grid.ComponentOwner<Page>();

                // Track IsModified
                ProcessCellIsModifyTextOld(cell, requestJson);

                cell.Text = requestJson.Grid2CellText;
                cell.Warning = null;
                switch (cell.RowEnum)
                {
                    case GridRowEnum.Filter:
                        break;
                    case GridRowEnum.Index:
                        {
                            Grid2RowState rowState = grid.RowStateList[cell.RowStateId - 1];
                            Row row = grid.RowList[rowState.RowId.Value - 1];
                            if (rowState.RowNew == null)
                            {
                                rowState.RowNew = (Row)Activator.CreateInstance(grid.TypeRow);
                                Data.RowCopy(row, rowState.RowNew);
                            }
                            // ErrorSave reset
                            ProcessCellIsModifyErrorSaveReset(grid, cell);
                            // Parse
                            ProcessCellIsModifyParse(grid, page, rowState.RowNew, column, field, cell);
                            if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                            {
                                // Save
                                await ProcessCellIsModifyUpdateAsync(grid, page, row, rowState.RowNew, cell);
                                if (cell.ErrorSave == null)
                                {
                                    Data.RowCopy(rowState.RowNew, row); // Copy new Id to 
                                    ProcessCellIsModifyReset(grid, cell);
                                }
                            }
                            ProcessCellIsModifyWarning(grid, cell);
                        }
                        break;
                    case GridRowEnum.New:
                        {
                            Grid2RowState rowState = grid.RowStateList[cell.RowStateId - 1];
                            if (rowState.RowNew == null)
                            {
                                rowState.RowNew = (Row)Activator.CreateInstance(grid.TypeRow);
                            }
                            // ErrorSave reset
                            ProcessCellIsModifyErrorSaveReset(grid, cell);
                            // Parse
                            ProcessCellIsModifyParse(grid, page, rowState.RowNew, column, field, cell);
                            if (!ProcessCellIsModifyIsErrorParse(grid, cell))
                            {
                                // Save
                                await ProcessCellIsModifyInsertAsync(grid, page, rowState.RowNew, cell);
                                if (cell.ErrorSave == null)
                                {
                                    grid.RowList.Add(rowState.RowNew);
                                    foreach (var item in grid.CellList)
                                    {
                                        if (item.RowStateId == cell.RowStateId) // Cells in same row
                                        {
                                            item.RowEnum = GridRowEnum.Index; // From New to Index
                                            item.Placeholder = null;
                                        }
                                    }
                                    rowState.RowNew = null;
                                    ProcessCellIsModifyReset(grid, cell);
                                    grid.Render();
                                }
                            }
                            ProcessCellIsModifyWarning(grid, cell);
                        }
                        break;
                    default:
                        throw new Exception("Enum unknown!");
                }
            }
        }

        /// <summary>
        /// Process incoming RequestJson.
        /// </summary>
        internal static async Task ProcessAsync()
        {
            // IsClickSort
            await ProcessIsClickSortAsync();

            // Grid2CellIsModify
            await ProcessCellIsModify();
        }

        /// <summary>
        /// TypeRow of loaded data grid.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal Type TypeRow;

        /// <summary>
        /// DatabaseEnum of loaded grid.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal DatabaseEnum DatabaseEnum;

        /// <summary>
        /// Reload rows after sort, filter. Do not load grid and field configuration.
        /// </summary>
        public async Task ReloadAsync()
        {
            Page page = this.ComponentOwner<Page>();
            IQueryable query = page.Grid2Query(this);
            var typeRow = query?.ElementType;
            if (typeRow != null && typeRow == TypeRow) // Make sure grid and field configuration are correct.
            {
                var configGrid = ConfigGridList.Where(item => item.ConfigName == this.ConfigName).SingleOrDefault(); // LINQ to memory
                query = Data.QuerySkipTake(query, 0, configGrid.RowCountMax == null ? 10 : configGrid.RowCountMax.Value); // By default load 10 rows.

                // Sort
                foreach (var column in ColumnList)
                {
                    if (column.IsSort != null)
                    {
                        query = Data.QueryOrderBy(query, column.FieldNameCSharp, column.IsSort.Value);
                    }
                }

                // Load row
                this.RowList = await Data.SelectAsync(query);
            }
        }

        /// <summary>
        /// Load data into grid. Override method Page.GridQuery(); to define query. It's also called to reload data.
        /// </summary>
        public async Task LoadAsync()
        {
            if (GridData == null)
            {
                Page page = this.ComponentOwner<Page>();
                IQueryable query = page.Grid2Query(this);
                TypeRow = query?.ElementType;
                DatabaseEnum = DatabaseMemoryInternal.DatabaseEnum(query);
                if (TypeRow != null)
                {
                    // Get config grid and field query
                    Page.GridConfigResult gridConfigResult = new Page.GridConfigResult();
                    page.Grid2QueryConfig(this, UtilDalType.TypeRowToTableNameCSharp(TypeRow), gridConfigResult);
                    
                    // Load config grid
                    this.ConfigGridList = await Data.SelectAsync(gridConfigResult.ConfigGridQuery);
                    var configGrid = ConfigGridList.Where(item => item.ConfigName == this.ConfigName).SingleOrDefault(); // LINQ to memory
                    query = Data.QuerySkipTake(query, 0, configGrid.RowCountMax == null ? 10 : configGrid.RowCountMax.Value); // By default load 10 rows.

                    // Load config field (Task)
                    var configFieldListTask = Data.SelectAsync(gridConfigResult.ConfigFieldQuery);

                    // Load row (Task)
                    var rowListTask = Data.SelectAsync(query);

                    await Task.WhenAll(configFieldListTask, rowListTask); // Load config field and row in parallel

                    // Load config field
                    this.ConfigFieldList = configFieldListTask.Result;

                    // Load row
                    this.RowList = rowListTask.Result;

                    var configFieldDictionary = this.ConfigFieldList.ToDictionary(item => (item.ConfigName, item.FieldNameCSharp), item => item);

                    this.ColumnList = new List<Grid2Column>();
                    AppJson appJson = page.ComponentOwner<AppJson>();
                    var fieldList = UtilDalType.TypeRowToFieldListDictionary(TypeRow);
                    foreach (var propertyInfo in UtilDalType.TypeRowToPropertyInfoList(TypeRow))
                    {
                        var field = fieldList[propertyInfo.Name];
                        configFieldDictionary.TryGetValue((this.ConfigName, propertyInfo.Name), out FrameworkConfigFieldBuiltIn configField);
                        NamingConvention namingConvention = appJson.NamingConventionInternal(TypeRow);
                        string columnText = namingConvention.ColumnTextInternal(TypeRow, propertyInfo.Name, configField?.Text);
                        bool isVisible = namingConvention.ColumnIsVisibleInternal(TypeRow, propertyInfo.Name, configField?.IsVisible);
                        double sort = namingConvention.ColumnSortInternal(TypeRow, propertyInfo.Name, field, configField?.Sort);
                        this.ColumnList.Add(new Grid2Column { 
                            FieldNameCSharp = field.FieldNameCSharp, 
                            ColumnText = columnText, 
                            Description = configField?.Description, 
                            IsVisible = isVisible, 
                            Sort = sort, 
                            SortField = field.Sort });
                    }
                    this.ColumnList = this.ColumnList.
                        Where(item => item.IsVisible == true).
                        OrderBy(item => item.Sort).
                        ThenBy(item => item.SortField).ToList(); // Make it deterministic if multiple columns have same Sort.
                    int columnId = 0;
                    foreach (var column in this.ColumnList)
                    {
                        column.Id = columnId += 1;
                    }
                }
                Render();
            }
        }

        /// <summary>
        /// Gets or sets ConfigGridList. Can contain multiple configurations. See also property Grid.GridData.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        internal List<FrameworkConfigGridBuiltIn> ConfigGridList;

        [Serialize(SerializeEnum.Session)]
        internal List<FrameworkConfigFieldBuiltIn> ConfigFieldList;

        [Serialize(SerializeEnum.Session)]
        internal List<Row> RowList;

        [Serialize(SerializeEnum.Session)]
        public string ConfigName;

        /// <summary>
        /// Gets or sets GridData. If not null, data is displayed from another grid. For example with different configuration.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        public Grid2 GridData;

        [Serialize(SerializeEnum.Session)]
        internal List<Grid2Column> ColumnList;

        [Serialize(SerializeEnum.Session)]
        internal List<Grid2RowState> RowStateList;

        /// <summary>
        /// Gets or sets GridCellList.
        /// </summary>
        internal List<Grid2Cell> CellList;

        internal string StyleColumn;
    }

    internal sealed class Grid2Column
    {
        public int Id;

        public string FieldNameCSharp;

        /// <summary>
        /// Gets or sets ColumnText. This is the header text for filter.
        /// </summary>
        public string ColumnText;

        /// <summary>
        /// Gets or sets Description. Shown with an information icon in header.
        /// </summary>
        public string Description;

        public bool IsVisible;

        /// <summary>
        /// Gets or sets Sort. Order as defined in data grid field config.
        /// </summary>
        public double? Sort;

        /// <summary>
        /// Gets or sets SortField. Order as defined in sql database schema.
        /// </summary>
        public int SortField;

        public bool? IsSort;

        public FilterOperator FilterOperator;

        public object FilterValue;
    }

    internal sealed class Grid2RowState
    {
        public int Id;

        public int? RowId; // Filter and New do not have a row.

        public bool IsSelect;

        public Row RowNew;
    }

    /// <summary>
    /// Grid cell display.
    /// </summary>
    internal sealed class Grid2Cell
    {
        public int Id;

        [Serialize(SerializeEnum.Session)]
        public int ColumnId;

        [Serialize(SerializeEnum.Session)]
        public int RowStateId;

        public GridRowEnum RowEnum;

        /// <summary>
        /// Gets or sets ColumnText. Header for Filter.
        /// </summary>
        public string ColumnText;

        /// <summary>
        /// Gets or sets json text.
        /// </summary>
        public string Text;

        /// <summary>
        /// Gets or sets TextOld. This is the text before save.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        public string TextOld;

        /// <summary>
        /// Gets IsModified. True, if there is modified text not yet saved to the database.
        /// </summary>
        [Serialize(SerializeEnum.Session)]
        public bool IsModified;

        public string ErrorParse;

        public string ErrorSave;

        public string Warning;

        public string Placeholder;

        public bool IsSelect;

        public bool? IsSort;
    }

    /// <summary>
    /// Grid paging.
    /// </summary>
    public enum GridIsClickEnum
    {
        None = 0,
        PageUp = 1,
        PageDown = 2,
        PageLeft = 3,
        PageRight = 4,
        Reload = 5,
        Config = 6,
    }

    public sealed class GridColumn
    {
        public int Id;

        public string Text;

        /// <summary>
        /// Gets or sets Description. Hover with mouse over column information icon to see tooltip window.
        /// </summary>
        public string Description;

        /// <summary>
        /// Gets or sets IsSort. If false, ascending. If true descending.
        /// </summary>
        public bool? IsSort;
    }

    public sealed class GridRow
    {
        public int Id;

        public List<GridCell> CellList;

        public bool IsSelect;

        public GridRowEnum RowEnum;

        public string ErrorSave;
    }

    public sealed class GridCell
    {
        public int Id;

        /// <summary>
        /// Gets or sets json text. Coming from client can be null or empty.
        /// </summary>
        public string Text;

        public string Placeholder;

        public string ErrorParse;

        public string TextGet()
        {
            return UtilFramework.StringNull(Text);
        }

        /// <summary>
        /// Gets or sets IsLookup. If true, field shows an open Lookup window.
        /// </summary>
        public bool IsLookup;

        /// <summary>
        /// Gets or sets Html. Use for example to transform plain text into a hyper link.
        /// </summary>
        public string Html;

        /// <summary>
        /// Gets or sets HtmlIsEdit. If true, html is rendered and additionally input text box is shown to edit plain html. Applies only if Html is not null.
        /// </summary>
        public bool HtmlIsEdit;

        /// <summary>
        /// Gets or sets HtmlLeft. Use for example to render an image on the left hand side in the cell.
        /// </summary>
        public string HtmlLeft;

        /// <summary>
        /// Gets or sets HtmlRight. Use for example to render an indicator icon on the right hand side in the cell. 
        /// </summary>
        public string HtmlRight;

        /// <summary>
        /// Gets or sets IsReadOnly. If true, user can not edit text.
        /// </summary>
        public bool IsReadOnly;

        /// <summary>
        /// Gets or sets IsPassword. If true, user can not read text.
        /// </summary>
        public bool IsPassword;

        /// <summary>
        /// Gets or sets Align. Defines text allign of centent in the data grid cell.
        /// </summary>
        public AlignEnum Align;
    }

    public sealed class Html : ComponentJson
    {
        public Html(ComponentJson owner)
            : base(owner)
        {

        }

        public string TextHtml;
    }

    public class Page : ComponentJson
    {
        /// <summary>
        /// Constructor. Use method PageShowAsync(); to create new page.
        /// </summary>
        public Page(ComponentJson owner)
            : base(owner)
        {
            Type = typeof(Page).Name;
        }

        /// <summary>
        /// Gets or sets TypeCSharp. Used when default property Type has been changed. Allows inheritance.
        /// </summary>
        public string TypeCSharp;

        /// <summary>
        /// Calle once a lifetime when page is created.
        /// </summary>
        public virtual Task InitAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Returns query to load data grid. Override this method to define sql query.
        /// </summary>
        /// <param name="grid">Grid to get query to load.</param>
        /// <returns>If value null, grid has no header and rows. If value is method Data.QueryEmpty(); grid has header but no rows.</returns>
        protected virtual internal IQueryable GridQuery(Grid grid)
        {
            return null;
        }

        /// <summary>
        /// Returns query to load data grid. Override this method to define sql query.
        /// </summary>
        /// <param name="grid">Grid to get query to load.</param>
        /// <returns>If value null, grid has no header columns and no rows. If value is equal to method Data.QueryEmpty(); grid has header columns but no data rows.</returns>
        protected virtual internal IQueryable Grid2Query(Grid2 grid)
        {
            return null;
        }

        /// <summary>
        /// Override this method for custom grid save implementation. Return isHandled.
        /// </summary>
        /// <param name="grid">Data grid to save.</param>
        /// <param name="row">Data row to update.</param>
        /// <param name="rowNew">New data row to save to database.</param>
        /// <returns>Returns true, if custom save was handled.</returns>
        protected virtual internal Task<bool> GridUpdateAsync(Grid grid, Row row, Row rowNew, DatabaseEnum databaseEnum)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Override this method for custom grid save implementation. Return isHandled.
        /// </summary>
        /// <param name="grid">Data grid to save.</param>
        /// <param name="row">Data row to update.</param>
        /// <param name="rowNew">New data row to save to database.</param>
        /// <returns>Returns true, if custom save was handled.</returns>
        protected virtual internal Task<bool> GridUpdateAsync(Grid2 grid, Row row, Row rowNew, DatabaseEnum databaseEnum)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Override this method for custom grid save implementation. Returns isHandled.
        /// </summary>
        /// <param name="grid">Data grid to save.</param>
        /// <param name="rowNew">Data row to insert. Set new primary key on this row.</param>
        /// <returns>Returns true, if custom save was handled.</returns>
        protected virtual internal Task<bool> GridInsertAsync(Grid grid, Row rowNew, DatabaseEnum databaseEnum)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Override this method for custom grid save implementation. Returns isHandled.
        /// </summary>
        /// <param name="grid">Data grid to save.</param>
        /// <param name="rowNew">Data row to insert. Set new primary key on this row.</param>
        /// <returns>Returns true, if custom save was handled.</returns>
        protected virtual internal Task<bool> GridInsertAsync(Grid2 grid, Row rowNew, DatabaseEnum databaseEnum)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Contains one query for data grid configuration and one query for data grid field configuration.
        /// </summary>
        public class GridConfigResult
        {
            /// <summary>
            /// Gets or sets ConfigGridQuery. Should return one record.
            /// </summary>
            public IQueryable<FrameworkConfigGridBuiltIn> ConfigGridQuery { get; set; }

            /// <summary>
            /// Gets or sets ConfigFieldQuery.
            /// </summary>
            public IQueryable<FrameworkConfigFieldBuiltIn> ConfigFieldQuery { get; set; }
        }

        /// <summary>
        /// Returns configuration of data grid to load.
        /// </summary>
        /// <param name="grid">Json data grid to load.</param>
        /// <param name="tableNameCSharp">Type of row to load.</param>
        protected virtual internal void GridQueryConfig(Grid grid, string tableNameCSharp, GridConfigResult result)
        {
            result.ConfigGridQuery = Data.Query<FrameworkConfigGridBuiltIn>().Where(item => item.TableNameCSharp == tableNameCSharp && item.ConfigName == grid.ConfigName);

            result.ConfigFieldQuery = Data.Query<FrameworkConfigFieldBuiltIn>().Where(item => item.TableNameCSharp == tableNameCSharp && item.ConfigName == grid.ConfigName);

            // Example for static configuration:
            // result.ConfigGridQuery = new [] { new FrameworkConfigGridBuiltIn { RowCountMax = 2 } }.AsQueryable();
        }

        /// <summary>
        /// Returns configuration query of data grid to load.
        /// </summary>
        /// <param name="grid">Json data grid to load.</param>
        /// <param name="tableNameCSharp">Type of row to load.</param>
        protected virtual internal void Grid2QueryConfig(Grid2 grid, string tableNameCSharp, GridConfigResult result)
        {
            result.ConfigGridQuery = Data.Query<FrameworkConfigGridBuiltIn>().Where(item => item.TableNameCSharp == tableNameCSharp /* && item.ConfigName == grid.ConfigName */); // Multiple configuration can be loaded. See also Grid.Data.

            result.ConfigFieldQuery = Data.Query<FrameworkConfigFieldBuiltIn>().Where(item => item.TableNameCSharp == tableNameCSharp /* && item.ConfigName == grid.ConfigName */); // Multiple configuration can be Loaded. See also Grid.GridData.

            // Example for static configuration:
            // result.ConfigGridQuery = new [] { new FrameworkConfigGridBuiltIn { RowCountMax = 2 } }.AsQueryable();
        }

        /// <summary>
        /// Override this method for custom implementation. Method is called when data row has been selected. Get selected row with method grid.GridRowSelected(); and reload for example a detail data grid.
        /// </summary>
        protected virtual internal Task GridRowSelectedAsync(Grid grid)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Override this method to return a linq query for the lookup data grid.
        /// </summary>
        protected virtual internal IQueryable GridLookupQuery(Grid grid, Row row, string fieldName, string text)
        {
            return null; // No lookup data grid.
        }

        protected virtual internal void GridLookupQueryConfig(Grid grid, GridConfigResult config)
        {
            // Example:
            // config.ConfigGridQuery = new [] { new FrameworkConfigGridBuiltIn { RowCountMax = 2 } }.AsQueryable();
        }

        /// <summary>
        /// Override this method to extract text from lookup grid for further processing. 
        /// Process wise there is no difference between user selecting a row on the lookup grid or entering text manually.
        /// </summary>
        /// <param name="grid">Grid on which lookup has been selected.</param>
        /// <param name="fieldName">Cell on which lookup has been selected</param>
        /// <param name="gridRowEnum">Row type on which lookup has been selected (for example filter row).</param>
        /// <param name="rowLookupSelected">Lookup row which has been selected by user.</param>
        /// <returns>Returns text like entered by user for further processing.</returns>
        protected virtual internal string GridLookupRowSelected(Grid grid, string fieldName, GridRowEnum gridRowEnum, Row rowLookupSelected)
        {
            return null;
        }

        /// <summary>
        /// Override this method to implement custom process at the end of the process chain. Called once every request.
        /// </summary>
        protected virtual internal Task ProcessAsync()
        {
            return Task.FromResult(0);
        }

        public enum AlignEnum
        {
            /// <summary>
            /// None.
            /// </summary>
            None = 0,

            /// <summary>
            /// Align text left.
            /// </summary>
            Left = 1,

            /// <summary>
            /// Align data grid cell text in center .
            /// </summary>
            Center = 2,

            /// <summary>
            /// Align data grid cell text right.
            /// </summary>
            Right = 3,
        }

        /// <summary>
        /// Provides additional annotation information for a data grid cell.
        /// </summary>
        public class GridCellAnnotationResult
        {
            /// <summary>
            /// Gets or sets Html. Use for example to transform plain text into a hyper link. For empty html set "&nbsp;" to keep the layout consistent with none empty html fields.
            /// </summary>
            public string Html;

            /// <summary>
            /// Gets or sets HtmlIsEdit. If true, html is rendered and additionally input text box is shown to edit plain html. Applies only if Html is not null.
            /// </summary>
            public bool HtmlIsEdit;

            /// <summary>
            /// Gets or sets HtmlLeft. Use for example to render an image on the left hand side in the cell.
            /// </summary>
            public string HtmlLeft;

            /// <summary>
            /// Gets or sets HtmlRight. Use for example to render an indicator icon on the right hand side in the cell. 
            /// </summary>
            public string HtmlRight;

            /// <summary>
            /// Gets or sets IsReadOnly. If true, user can not edit text.
            /// </summary>
            public bool IsReadOnly;

            /// <summary>
            /// Gets or sets IsPassword. If true, user can not read text.
            /// </summary>
            public bool IsPassword;

            /// <summary>
            /// Gets or sets Align. Defines text allign of centent in the data grid cell.
            /// </summary>
            public AlignEnum Align;
        }

        /// <summary>
        /// Override this method for custom implementation of converting database value to front end grid cell text. Called only if value is not null.
        /// </summary>
        /// <returns>Returns cell text. If null is returned, framework does default conversion of value to string.</returns>
        protected virtual internal string GridCellText(Grid grid, Row row, string fieldName)
        {
            return null;
        }

        /// <summary>
        /// Override this method for custom implementation of converting database value to front end grid cell text. Called only if value is not null.
        /// </summary>
        /// <returns>Returns cell text. If null is returned, framework does default conversion of value to string.</returns>
        protected virtual internal string Grid2CellText(Grid2 grid, Row row, string fieldName)
        {
            return null;
        }

        /// <summary>
        /// Override this method for custom implementation of converting database value to front end grid cell text. Called only if value is not null.
        /// </summary>
        /// <returns>Returns cell text. If null is returned, framework does default conversion of value to string. Otherwise return empty string.</returns>
        protected virtual internal string GridCellText(Grid2 grid, Row row, string fieldName)
        {
            return null;
        }

        /// <summary>
        /// Override this method to provide additional custom annotation information for a data grid cell. This information is provided on every render request.
        /// </summary>
        /// <param name="grid">Data grid on this page.</param>
        /// <param name="fieldName">Data grid column name.</param>
        /// <param name="gridRowEnum">Data grid row type.</param>
        /// <param name="row">Data grid row if applicable for row type.</param>
        /// <param name="result">Returns data grid cell annotation.</param>
        protected virtual internal void GridCellAnnotation(Grid grid, string fieldName, GridRowEnum gridRowEnum, Row row, GridCellAnnotationResult result)
        {

        }

        /// <summary>
        /// Parse user entered cell text into database value. Called only if text is not null. Write parsed value to row. (Or for example multiple fields on row for UOM)
        /// </summary>
        /// <param name="row">Write user parsed value to row.</param>
        /// <param name="isHandled">If true, framework does default parsing of user entered text.</param>
        protected virtual internal void GridCellParse(Grid grid, string fieldName, string text, Row row, out bool isHandled)
        {
            isHandled = false;
        }

        /// <summary>
        /// Parse user entered cell text into database value. Called only if text is not null. Write parsed value to row. (Or for example multiple fields on row for UOM)
        /// </summary>
        /// <param name="row">Write user parsed value to row.</param>
        /// <param name="isHandled">If true, framework does default parsing of user entered text.</param>
        protected virtual internal void GridCellParse(Grid2 grid, string fieldName, string text, Row row, out bool isHandled)
        {
            isHandled = false;
        }

        /// <summary>
        /// Parse user entered cell filter text. Called only if text is not null.
        /// </summary>
        protected virtual internal void GridCellParseFilter(Grid grid, string fieldName, string text, Filter filter, out bool isHandled)
        {
            isHandled = false;
        }
    }
}
