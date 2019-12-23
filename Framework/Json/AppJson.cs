namespace Framework.Json
{
    using Database.dbo;
    using Framework.App;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using Framework.Session;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using static Framework.Json.Page;
    using static Framework.Session.UtilSession;

    /// <summary>
    /// Json component tree. Store session state in field or property.
    /// </summary>
    public abstract class ComponentJson
    {
        /// <summary>
        /// Constructor for json deserialization.
        /// </summary>
        public ComponentJson() { }

        /// <summary>
        /// Constructor to programmatically create new object.
        /// </summary>
        public ComponentJson(ComponentJson owner)
        {
            Constructor(owner);
        }

        internal void Constructor(ComponentJson owner)
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

        public string Name;
    }

    /// <summary>
    /// Extension methods to manage json component tree.
    /// </summary>
    public static class ComponentJsonExtension
    {
        public static ComponentJson ComponentOwner(this ComponentJson component)
        {
            ComponentJson result = UtilServer.AppJson.ComponentListAll().Where(item => item.List.Contains(component)).Single();
            return result;
        }

        /// <summary>
        /// Returns owner of type T. Searches in parent and grand parents.
        /// </summary>
        public static T ComponentOwner<T>(this ComponentJson component) where T : ComponentJson
        {
            do
            {
                component = ComponentOwner(component);
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
        /// Returns all child components.
        /// </summary>
        public static List<ComponentJson> ComponentList(this ComponentJson component)
        {
            return component.List;
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

        public static ComponentJson ComponentGet(this ComponentJson owner, string name)
        {
            var resultList = owner.List.Where(item => item.Name == name).ToArray();
            if (resultList.Count() > 1)
            {
                throw new Exception(string.Format("Component with same name exists more than once! ({0})", name));
            }
            return resultList.SingleOrDefault();
        }

        public static T ComponentGet<T>(this ComponentJson owner, string name) where T : ComponentJson
        {
            ComponentJson result = owner.ComponentGet(name);
            if (result != null && !(result is T))
            {
                throw new Exception(string.Format("Component wrong type! (Name={0})", name));
            }
            return (T)owner.ComponentGet(name);
        }

        public static T ComponentGet<T>(this ComponentJson owner) where T : ComponentJson
        {
            return owner.ComponentGet<T>(typeof(T).Name);
        }

        /// <summary>
        /// Returns child component of Type T on index.
        /// </summary>
        public static T ComponentGet<T>(this ComponentJson owner, int index) where T : ComponentJson
        {
            return owner.ComponentList<T>()[index];
        }

        /// <summary>
        /// Returns new ComponentJson.
        /// </summary>
        public static T ComponentCreate<T>(this ComponentJson owner, string name, Action<T> init = null) where T : ComponentJson
        {
            if (UtilFramework.IsSubclassOf(typeof(T), typeof(Page)))
            {
                throw new Exception("Use await method ComponentPageShowAsync();");
            }
            T component = (T)Activator.CreateInstance(typeof(T), owner);
            component.Name = name;
            init?.Invoke(component);

            return component; // owner.Get<T>(name); // Do not check whether component with same name exists multiple times.
        }

        public static T ComponentCreate<T>(this ComponentJson owner, Action<T> init = null) where T : ComponentJson
        {
            return ComponentCreate<T>(owner, typeof(T).Name, init);
        }

        /// <summary>
        /// Returns ComponentJson or creates new if not yet exists.
        /// </summary>
        /// <param name="init">Callback method if ComponentJson has been created new. For example to init CssClass.</param>
        public static T ComponentGetOrCreate<T>(this ComponentJson owner, string name, Action<T> init = null) where T : ComponentJson
        {
            if (owner.ComponentGet(name) == null)
            {
                T component = (T)Activator.CreateInstance(typeof(T), owner);
                component.Name = name;
                init?.Invoke(component);
            }
            return owner.ComponentGet<T>(name);
        }

        public static T ComponentGetOrCreate<T>(this ComponentJson owner, Action<T> init = null) where T : ComponentJson
        {
            return ComponentGetOrCreate<T>(owner, typeof(T).Name, init);
        }

        /// <summary>
        /// Returns ComponentJson or creates new if not yet exists.
        /// </summary>
        /// <param name="init">Callback method if ComponentJson has been created new. For example to init CssClass.</param>
        public static T ComponentGetOrCreate<T>(this ComponentJson owner, int index, Action<T> init = null) where T : ComponentJson
        {
            int count = owner.ComponentList<T>().Count;
            while (count - 1 < index)
            {
                owner.ComponentCreate<T>(init);
                count += 1;
            }
            return owner.ComponentList<T>()[index];
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
        /// Shows page or creates new one if it does not yet exist. Similar to method ComponentGetOrCreate(); but additionally invokes page init async.
        /// </summary>
        public static async Task<T> ComponentPageShowAsync<T>(this ComponentJson owner, string name, PageShowEnum pageShowEnum = PageShowEnum.Default, Action<T> init = null) where T : Page
        {
            T result = null;
            if (pageShowEnum == PageShowEnum.SiblingHide)
            {
                foreach (Page page in owner.List.OfType<Page>())
                {
                    page.IsHide = true; // Hide
                }
            }
            if (ComponentGet(owner, name) == null)
            {
                result = (T)Activator.CreateInstance(typeof(T), owner);
                result.Name = name;
                init?.Invoke(result);
                await result.InitAsync();
            }
            result = ComponentGet<T>(owner, name);
            UtilFramework.Assert(result != null);
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
        /// Shows page or creates new one if it does not yet exist. Similar to method ComponentGetOrCreate(); but additionally invokes page init async.
        /// </summary>
        public static Task<T> ComponentPageShowAsync<T>(this ComponentJson owner, PageShowEnum pageShowEnum = PageShowEnum.None, Action<T> init = null) where T : Page
        {
            return ComponentPageShowAsync<T>(owner, typeof(T).Name, pageShowEnum, init);
        }

        /// <summary>
        /// Remove this component.
        /// </summary>
        public static void ComponentRemove(this ComponentJson component)
        {
            component?.ComponentOwner().List.Remove(component);
        }

        /// <summary>
        /// Returns index of this component.
        /// </summary>
        public static int ComponentIndex(this ComponentJson component)
        {
            return component.ComponentOwner().List.IndexOf(component);
        }

        /// <summary>
        /// Returns count of this component parents list.
        /// </summary>
        public static int ComponentCount(this ComponentJson component)
        {
            return component.ComponentOwner().List.Count();
        }

        /// <summary>
        /// Remove child component if exists.
        /// </summary>
        public static void ComponentRemoveItem(this ComponentJson component, string name)
        {
            var item = component.ComponentGet(name);
            if (item != null)
            {
                item.ComponentRemove();
            }
        }

        /// <summary>
        /// Remove child component if exists.
        /// </summary>
        public static void ComponentRemoveItem<T>(this ComponentJson component) where T : ComponentJson
        {
            component.ComponentRemoveItem(typeof(T).Name);
        }

        /// <summary>
        /// Move this component to index position.
        /// </summary>
        public static void ComponentMove(this ComponentJson component, int index)
        {
            var list = component?.ComponentOwner().List;
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
        public AppJson() { }

        public AppJson(ComponentJson owner)
            : base(owner)
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
            await UtilApp.ProcessButtonAsync(); // Button
            UtilApp.ProcessBootstrapModal(); // Modal dialog window

            foreach (Page page in UtilServer.AppJson.ComponentListAll().OfType<Page>())
            {
                await page.ProcessAsync();
            }

            UtilApp.DivContainerRender();
            UtilServer.AppInternal.AppSession.GridRender(); // Grid render
            UtilApp.BootstrapNavbarRender();

            SessionState = UtilServer.Session.GetString("Main") + "; Grid.Count=" + UtilServer.AppSession.GridSessionList.Count;
            UtilStopwatch.TimeStop("Process");
        }

        /// <summary>
        /// Gets or sets RequestCount. Used by client. Does not send new request while old is still pending.
        /// </summary>
        public int RequestCount { get; internal set; }

        /// <summary>
        /// Gets or sets RequestCount. Used by server to verify incoming request matches last response.
        /// </summary>
        public int ResponseCount { get; internal set; }

        /// <summary>
        /// Gets or sets IsInit. If false, app is not initialized. Method App.Init(): is called.
        /// </summary>
        public bool IsInit;

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
        /// Gets SessionState. Debug server side session state.
        /// </summary>
        public string SessionState { get; internal set; }

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

        /// <summary>
        /// Gets or sets BrowserUrl. This value is set by the browser. It can be different from RequestUrl if application runs embeded in another webpage.
        /// For example:  http://localhost:49323/config/data.txt
        /// </summary>
        public string BrowserUrl;

        /// <summary>
        /// Returns BrowserUrl. This value is set by the browser. It can be different from RequestUrl if application runs embeded in another webpage.
        /// For example: http://localhost:4200/
        /// </summary>
        public string BrowserUrlServer()
        {
            Uri uri = new Uri(BrowserUrl);
            string result = string.Format("{0}://{1}/", uri.Scheme, uri.Authority);
            return result;
        }
    }

    /// <summary>
    /// Json Button. Rendered as html button element.
    /// </summary>
    public sealed class Button : ComponentJson
    {
        public Button() { }

        public Button(ComponentJson owner)
            : base(owner)
        {

        }

        public string TextHtml;

        public bool IsClick;
    }

    /// <summary>
    /// Json Div. Rendered as html div element.
    /// </summary>
    public sealed class Div : ComponentJson
    {
        public Div() { }

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
        public DivContainer() { }

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
        public BootstrapNavbar() { }

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

    public sealed class BootstrapNavbarButton : ComponentJson
    {
        public BootstrapNavbarButton() { }

        public BootstrapNavbarButton(ComponentJson owner)
            : base(owner)
        {

        }

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

        public bool IsClick;

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
        public static Html BootstrapAlert(this Page page, string name, string textHtml, BootstrapAlertEnum alertEnum, int index = 0)
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
            Html result = page.ComponentGetOrCreate<Html>(name);
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
        public Grid() { }

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

        /// <summary>
        /// Gets or sets IsHide. If true, grid data (ColumnList and RowList) are not beeing transfered to client and back. See also method GridRender();
        /// But owner page and its method Page.GridRowSelectedAsync(); can still found and called for example by method ProcessBootstrapNavbarAsync();
        /// To hide other components use extension method Remove();
        /// </summary>
        public bool IsHide;

        public List<GridColumn> ColumnList;

        public List<GridRow> RowList;

        public GridIsClickEnum IsClickEnum;

        /// <summary>
        /// Gets or sets LookupGridIndex. If not null, this Lookup is a lookup of data grid LookupGridIndex.
        /// </summary>
        public int? LookupGridIndex;

        public int? LookupRowIndex;

        public int? LookupCellIndex;

        /// <summary>
        /// Returns true, if grid is a Lookup grid.
        /// </summary>
        internal bool GridLookupIsOpen()
        {
            return LookupGridIndex != null && this.ComponentOwner() is Grid;
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
            lookup.LookupGridIndex = gridIndex;
            lookup.LookupRowIndex = rowIndex;
            lookup.LookupCellIndex = cellIndex;

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
        public string Text;

        /// <summary>
        /// Gets or sets IsSort. If false, ascending. If true descending.
        /// </summary>
        public bool? IsSort;

        public bool IsClickSort;

        public bool IsClickConfig;
    }

    public sealed class GridRow
    {
        public List<GridCell> CellList;

        public bool IsClick;

        public bool IsSelect;

        public GridRowEnum RowEnum;

        public string ErrorSave;
    }

    public sealed class GridCell
    {
        /// <summary>
        /// Gets or sets json text. When coming from client Text can be null or ""!
        /// </summary>
        public string Text;

        public string Placeholder;

        public string ErrorParse;

        public string TextGet()
        {
            return UtilFramework.StringNull(Text);
        }

        public bool IsModify;

        public bool IsClick; // Show spinner

        /// <summary>
        /// Gets or sets MergeId. Used by the client to buffer user entered text during pending request.
        /// </summary>
        public int MergeId;

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
        public Html() { }

        public Html(ComponentJson owner)
            : base(owner)
        {

        }

        public string TextHtml;
    }

    public class Page : ComponentJson
    {
        public Page()
        {
            Type = typeof(Page).Name;
        }

        /// <summary>
        /// Constructor. Use method PageShowAsync(); to create new page.
        /// </summary>
        public Page(ComponentJson owner)
            : base(owner)
        {
            Type = typeof(Page).Name;
        }

        /// <summary>
        /// Gets or sets IsHide. If true, component and children are still being transferred to client and back to keep state.
        /// To hide other components use extension method Remove();
        /// </summary>
        public bool IsHide;

        /// <summary>
        /// Gets or sets TypeCSharp. Used when default property Type has been changed. Allows inheritance.
        /// </summary>
        public string TypeCSharp;

        /// <summary>
        /// Called once a lifetime when page is created.
        /// </summary>
        protected virtual internal Task InitAsync()
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
        /// Override this method for custom grid save implementation. Returns isHandled.
        /// </summary>
        /// <param name="grid">Data grid to save.</param>
        /// <param name="rowNew">Data row to insert. Set new primary key on this row.</param>
        /// <returns>Returns true, if custom save was handled.</returns>
        protected virtual internal Task<bool> GridInsertAsync(Grid grid, Row rowNew, DatabaseEnum databaseEnum)
        {
            return Task.FromResult(false);
        }

        public class ConfigResult
        {
            /// <summary>
            /// Gets or sets ConfigGridQuery. Should return one record.
            /// </summary>
            public IQueryable<FrameworkConfigGridBuiltIn> ConfigGridQuery { get; set; }

            /*

            // Other additional possible implementations:
            
            public Task<FrameworkConfigGridBuiltIn> ConfigGridQueryTask { get; set; }

            public FrameworkConfigGridBuiltIn ConfigGrid { get; set; }

            */

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
        protected virtual internal void GridQueryConfig(Grid grid, string tableNameCSharp, ConfigResult result)
        {
            result.ConfigFieldQuery = Data.Query<FrameworkConfigFieldBuiltIn>().Where(item => item.TableNameCSharp == tableNameCSharp && item.ConfigName == grid.ConfigName);

            // Example:
            // config.ConfigGridQuery = new [] { new FrameworkConfigGridBuiltIn { RowCountMax = 2 } }.AsQueryable();
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

        protected virtual internal void GridLookupQueryConfig(Grid grid, ConfigResult config)
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

        protected virtual internal Task ButtonClickAsync(Button button)
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
        /// Parse user entered cell filter text. Called only if text is not null.
        /// </summary>
        protected virtual internal void GridCellParseFilter(Grid grid, string fieldName, string text, Filter filter, out bool isHandled)
        {
            isHandled = false;
        }
    }
}
