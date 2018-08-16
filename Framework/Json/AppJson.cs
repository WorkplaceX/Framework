﻿namespace Framework.Json
{
    using Framework.App;
    using Framework.Dal;
    using Framework.Server;
    using Framework.Session;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;

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

        public string Type;

        public string TrackBy;

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

    public static class ComponentJsonExtension
    {
        public static ComponentJson Owner(this ComponentJson component)
        {
            ComponentJson result = UtilServer.AppJson.ListAll().Where(item => item.List.Contains(component)).Single();
            return result;
        }

        /// <summary>
        /// Returns owner of type T. Searches in parent and grand parents.
        /// </summary>
        public static T Owner<T>(this ComponentJson component) where T : ComponentJson
        {
            do
            {
                component = Owner(component);
                if (component is T)
                {
                    return (T)component;
                }
            } while (component != null);
            return null;
        }


        private static void ListAll(ComponentJson component, List<ComponentJson> result)
        {
            result.AddRange(component.List);
            foreach (var item in component.List)
            {
                ListAll(item, result);
            }
        }

        public static List<ComponentJson> ListAll(this ComponentJson component)
        {
            List<ComponentJson> result = new List<ComponentJson>();
            result.Add(component);
            ListAll(component, result);
            return result;
        }

        public static ComponentJson Get(this ComponentJson owner, string name)
        {
            return owner.List.Where(item => item.Name == name).SingleOrDefault();
        }

        public static T Get<T>(this ComponentJson owner, string name) where T : ComponentJson
        {
            return owner.Get(name) as T;
        }

        public static T Get<T>(this ComponentJson owner) where T : ComponentJson
        {
            return owner.Get<T>(typeof(T).Name);
        }

        public static T GetOrCreate<T>(this ComponentJson owner, string name, Action<T> init = null) where T : ComponentJson
        {
            if (owner.Get(name) == null)
            {
                T component = (T)Activator.CreateInstance(typeof(T), owner);
                component.Name = name;
                init?.Invoke(component);
            }
            return owner.Get<T>(name);
        }

        public static T GetOrCreate<T>(this ComponentJson owner, Action<T> init = null) where T : ComponentJson
        {
            return GetOrCreate<T>(owner, typeof(T).Name, init);
        }

        public enum PageShowEnum
        {
            /// <summary>
            /// Add page to sibling pages.
            /// </summary>
            None = 0,

            /// <summary>
            /// Remove sibling pages.
            /// </summary>
            SiblingRemove = 1,

            /// <summary>
            /// Hide sibling pages and keep their state.
            /// </summary>
            SiblingHide = 2,
        }

        public static async Task<T> PageShowAsync<T>(this ComponentJson owner, string name, PageShowEnum pageShow = PageShowEnum.None, Action<T> init = null) where T : Page
        {
            T result = null;
            if (pageShow == PageShowEnum.SiblingHide)
            {
                foreach (Page page in owner.List.OfType<Page>())
                {
                    page.IsHide = true; // Hide
                }
            }
            if (Get(owner, name) == null)
            {
                result = (T)Activator.CreateInstance(typeof(T), owner);
                result.Name = name;
                await result.InitAsync();
                init?.Invoke(result);
            }
            result = Get<T>(owner, name);
            UtilFramework.Assert(result != null);
            result.IsHide = false; // Show
            if (pageShow == PageShowEnum.SiblingRemove)
            {
                owner.List.OfType<Page>().ToList().ForEach(page =>
                {
                    if (page != result) { page.Remove(); }
                });
            }
            return result;
        }

        public static Task<T> PageShowAsync<T>(this ComponentJson owner, PageShowEnum pageShow = PageShowEnum.None, Action<T> init = null) where T : Page
        {
            return PageShowAsync<T>(owner, typeof(T).Name, pageShow, init);
        }

        public static void Remove(this ComponentJson component)
        {
            component?.Owner().List.Remove(component);
        }

        /// <summary>
        /// Returns currently selected row.
        /// </summary>
        public static Row RowSelected(this Grid grid)
        {
            Row result = null;
            if (grid.Id != null) // Loaded
            {
                result = UtilServer.AppInternal.AppSession.GridSessionList[grid.Index()].GridRowSessionList.Where(gridRowSession => gridRowSession.IsSelect).Select(item => item.Row).FirstOrDefault();
            }
            return result;
        }
    }

    public class AppJson : Page
    {
        public AppJson() { }

        public AppJson(ComponentJson owner) 
            : base(owner)
        {

        }

        internal async Task InitInternalAsync()
        {
            await InitAsync();
            UtilServer.Session.SetString("Main", string.Format("App start: {0}", UtilFramework.DateTimeToString(DateTime.Now.ToUniversalTime())));
        }

        internal async Task ProcessInternalAsync()
        {
            await UtilServer.AppInternal.AppSession.ProcessAsync(); // Grid process
            await UtilApp.ProcessAsync(); // Button

            foreach (Page page in UtilServer.AppJson.ListAll().OfType<Page>())
            {
                await page.ProcessAsync();
            }

            UtilServer.AppInternal.AppSession.GridRender(); // Grid render

            SessionState = UtilServer.Session.GetString("Main") + "; Grid.Count=" + UtilServer.AppSession.GridSessionList.Count;
        }

        /// <summary>
        /// Gets or sets RequestCount. Used by client. Does not send new request while old is still pending.
        /// </summary>
        public int RequestCount;

        /// <summary>
        /// Gets or sets RequestCount. Used by server to verify incoming request matches last response.
        /// </summary>
        public int ResponseCount { get; internal set; }

        /// <summary>
        /// Gets or sets IsInit. If false, app is not initialized. Method App.Init(): is called.
        /// </summary>
        public bool IsInit;

        public string Version { get; set; }

        public string VersionBuild { get; set; }

        public bool IsServerSideRendering { get; set; }

        public string Session { get; set; }

        public string SessionApp { get; set; }

        /// <summary>
        /// Gets SessionState. Debug server side session state.
        /// </summary>
        public string SessionState { get; set; }

        /// <summary>
        /// Gets or sets IsReload. If true, client reloads page. For example if session expired.
        /// </summary>
        public bool IsReload { get; set; }

        /// <summary>
        /// Gets or sets RequestUrl. This value is set by the server. For example: http://localhost:49323/config/app.json
        /// </summary>
        public string RequestUrl;

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

        public string Text;

        public bool IsClick;
    }

    public sealed class Grid : ComponentJson
    {
        public Grid() { }

        public Grid(ComponentJson owner)
            : base(owner)
        {

        }

        /// <summary>
        /// Load data into grid. Override method App.Query(); to define query. It's also called to reload data.
        /// </summary>
        public async Task LoadAsync()
        {
            await UtilServer.AppInternal.AppSession.GridLoadAsync(this);
        }

        public int? Id { get; internal set; }

        internal int Index()
        {
            return (int)Id - 1;
        }

        public GridHeader Header;

        public List<GridRow> RowList;

        public Grid GridLookup()
        {
            if (List.Count == 0 || !(List[0] is Grid))
            {
                List.Clear();
                new Grid(this);
            }
            return (Grid)List[0];
        }
    }

    public sealed class GridHeader
    {
        public List<GridColumn> ColumnList;
    }

    public sealed class GridColumn
    {
        public string Text;

        public string SearchText;

        public bool IsClick;

        public bool IsModify;
    }

    public sealed class GridRow
    {
        public List<GridCell> CellList;

        public bool IsClick;

        public bool IsSelect;

        public GridRowEnum RowEnum;
    }

    public sealed class GridCell
    {
        public string Text;

        public bool IsModify;

        /// <summary>
        /// Gets or sets MergeId. Used by the client to buffer user entered text during pending request.
        /// </summary>
        public int MergeId;

        public bool IsLookup;
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

        public bool IsHide;

        /// <summary>
        /// Gets or sets TypeCSharp. Used when default property Type has been changed. Allows inheritance.
        /// </summary>
        public string TypeCSharp;

        /// <summary>
        /// Called on first request.
        /// </summary>
        protected virtual internal async Task InitAsync()
        {
            await Task.Run(() => { });
        }

        /// <summary>
        /// Returns query to load data grid. Override this method to define sql query.
        /// </summary>
        /// <param name="grid">Grid to get query to load.</param>
        /// <returns>If value null, grid has no header and rows. If value is method UtilDal.QueryEmpty(); grid has header but no rows.</returns>
        protected virtual internal IQueryable GridLoadQuery(Grid grid)
        {
            return null;
        }

        protected virtual internal async Task GridRowSelectChangeAsync(Grid grid)
        {
            await Task.Run(() => { });
        }

        protected virtual internal async Task ProcessAsync()
        {
            await Task.Run(() => { });
        }

        protected virtual internal async Task ButtonClickAsync(Button button)
        {
            await Task.Run(() => { });
        }
    }
}
