namespace Framework.Json
{
    using Framework.Application;
    using Framework.Dal;
    using Framework.Server;
    using Framework.Session;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Reflection;
    using System.Threading.Tasks;

    public class ComponentJson
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
        public List<ComponentJson> List = new List<ComponentJson>();

        public string Name;
    }

    public static class ComponentJsonExtension
    {
        public static ComponentJson Owner(this ComponentJson component)
        {
            ComponentJson result = UtilServer.App.AppJson.ListAll().Where(item => item.List.Contains(component)).Single();
            return result;
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
            ListAll(component, result);
            return result;
        }

        public static T Create<T>(this ComponentJson owner, string name, Action<ComponentJson, string> create) where T : ComponentJson
        {
            if (owner.ComponentByName(name) == null)
            {
                create(owner, name);
            }
            return owner.ComponentByName<T>(name);
        }

        public static ComponentJson ComponentByName(this ComponentJson owner, string name)
        {
            return owner.List.Where(item => item.Name == name).SingleOrDefault();
        }

        public static T ComponentByName<T>(this ComponentJson owner, string name) where T : ComponentJson
        {
            return (T)ComponentByName(owner, name);
        }
    }

    public class AppJson : ComponentJson
    {
        public AppJson() { }

        public AppJson(ComponentJson owner) 
            : base(owner)
        {

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
    public class Button : ComponentJson
    {
        public Button() : this(null) { }

        public Button(ComponentJson owner)
            : base(owner)
        {

        }

        public string Text;

        public bool IsClick;
    }

    public class Grid : ComponentJson
    {
        public Grid() : this(null) { }

        public Grid(ComponentJson owner)
            : base(owner)
        {

        }

        /// <summary>
        /// Load data into grid. Override method App.Query(); to define query. It's also called to reload data.
        /// </summary>
        public async Task LoadAsync()
        {
            await UtilServer.App.AppSession.GridLoadAsync(this);
        }

        public int? Id { get; internal set; }

        internal int Index()
        {
            return (int)Id - 1;
        }

        public GridHeader Header;

        public List<GridRow> RowList;

        /// <summary>
        /// Returns currently selected row.
        /// </summary>
        public Row Select()
        {
            Row result = null;
            if (Id != null)
            {
                result = UtilServer.App.AppSession.GridSessionList[Index()].RowSessionList.Where(rowSession => rowSession.IsSelect).Select(item => item.Row).FirstOrDefault();
            }
            return result;
        }
    }

    public class GridHeader
    {
        public List<GridColumn> ColumnList;
    }

    public class GridColumn
    {
        public string Text;

        public string SearchText;

        public bool IsClick;

        public bool IsModify;
    }

    public class GridRow
    {
        public List<GridCell> CellList;

        public bool IsClick;

        public bool IsSelect;
    }

    public class GridCell
    {
        public string Text;

        public bool IsModify;

        public int MergeId;
    }
}
