namespace Framework.Application
{
    using Database.dbo;
    using Framework.Application.Config;
    using Framework.Component;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;

    internal class AppSelectorResult
    {
        public App App;

        public string AppJsonInText;

        public AppJson AppJsonIn;

        public string RequestPathBase;

        /// <summary>
        /// Gets or sets Session defined in cookie. See also AppJson.Session
        /// </summary>
        public Guid? Session;
    }

    /// <summary>
    /// Run multiple applications on same ASP.NET Core and same database instance. Mapping of url to class App is defined in sql view FrameworkApplicationDisplay.
    /// AppSelector has to be in the same assembly like the App classes.
    /// </summary>
    public class AppSelector
    {
        /// <summary>
        /// Constructor with default app, if no database connection exists.
        /// </summary>
        internal AppSelector(Type typeAppDefault)
        {
            this.TypeAppDefault = typeAppDefault;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppSelector()
            : this(null)
        {

        }

        /// <summary>
        /// Gets TypeAppDefault. This is the default app, if no database connection exists.
        /// </summary>
        public readonly Type TypeAppDefault;

        /// <summary>
        /// Database access of sql view FrameworkApplicationDisplay.
        /// </summary>
        protected virtual List<FrameworkApplicationDisplay> DbApplicationList()
        {
            List<FrameworkApplicationDisplay> result;
            result = UtilDataAccessLayer.Query<FrameworkApplicationDisplay>().ToList();
            result = result.Where(item => item.IsExist == true && item.IsActive == true).OrderByDescending(item => item.Path).ToList(); // OrderByDescending: Make sure empty path is last match. And sql view FrameworkApplicationView exists (Execute BuildTool runSqlCreate command). 
            return result;
        }

        internal AppSelectorResult Create(WebControllerBase webController, string controllerPath)
        {
            AppSelectorResult result = new AppSelectorResult();
            result.RequestPathBase = controllerPath;
            string requestUrl = webController.HttpContext.Request.Path.ToString();
            if (!requestUrl.EndsWith("/"))
            {
                requestUrl += "/";
            }
            //
            // CreateApp(webController, controllerPath, result);
            //
            CreateAppFromSessionCookie(webController, result);
            if (result.App == null)
            {
                CreateAppFromUrl(webController, controllerPath, requestUrl, result);
            }
            AppJson(webController, result);
            webController.Response.Cookies.Append("Session", result.Session.ToString());
            return result;
        }

        private static void CreateApp(WebControllerBase webController, string controllerPath, AppSelectorResult result)
        {
            string sessionText = webController.Request.Cookies["Session"];
            if (Guid.TryParse(sessionText, out Guid session))
            {
                result.Session = session;
            }
            //
            List<SqlParameter> listParam = new List<SqlParameter>();
            UtilDataAccessLayer.ExecuteParameterAdd("@X", "f8", System.Data.SqlDbType.NVarChar, listParam);
            
            // var d3 = UtilDataAccessLayer.ExecuteReader("SELECT @X AS F", listParam, false);
            // var d2 = UtilDataAccessLayer.ExecuteReader("EXEC MyProc @Id = @P0", listParam, false);
            // var l = UtilDataAccessLayer.ExecuteResultCopy<FrameworkApplication>(d2, 0);
            //
            List<SqlParameter> paramList = new List<SqlParameter>();
            UtilDataAccessLayer.ExecuteParameterAdd("@Path", controllerPath, System.Data.SqlDbType.NVarChar, paramList);
            UtilDataAccessLayer.ExecuteParameterAdd("@UserName", App.UserGuest().UserName, System.Data.SqlDbType.NVarChar, paramList);
            UtilDataAccessLayer.ExecuteParameterAdd("@UserNameIsBuiltIn", true, System.Data.SqlDbType.Int, paramList);
            UtilDataAccessLayer.ExecuteParameterAdd("@Session", result.Session, System.Data.SqlDbType.UniqueIdentifier, paramList);

            var d = UtilDataAccessLayer.ExecuteReader("EXEC FrameworkLogin @Path, @UserName, @UserNameIsBuiltIn, @Session", paramList, false);
        }

        private static void CreateAppFromSessionCookie(WebControllerBase webController, AppSelectorResult result)
        {
            string sessionText = webController.Request.Cookies["Session"];
            if (Guid.TryParse(sessionText, out Guid session))
            {
                result.Session = session;
            }
        }

        /// <summary>
        /// Extract AppJson from POST request.
        /// </summary>
        private static void AppJson(WebControllerBase webController, AppSelectorResult result)
        {
            string appJsonInText = UtilServer.StreamToString(webController.Request.Body);
            if (appJsonInText != "")
            {
                result.AppJsonInText = appJsonInText;
                result.AppJsonIn = JsonConvert.Deserialize<AppJson>(appJsonInText, result.App.TypeComponentInNamespaceList());
                result.Session = result.AppJsonIn.Session;
            }
            else
            {
                result.Session = Guid.NewGuid(); // New Session.
            }
        }

        /// <summary>
        /// CreateApp if App can't be determined by session. For example first request.
        /// </summary>
        private void CreateAppFromUrl(WebControllerBase webController, string controllerPath, string requestUrl, AppSelectorResult result)
        {
            // Create App
            foreach (FrameworkApplicationDisplay frameworkApplication in DbApplicationList())
            {
                string path = frameworkApplication.Path;
                if (string.IsNullOrEmpty(path))
                {
                    path = null;
                }
                else
                {
                    if (!path.EndsWith("/"))
                    {
                        path = path + "/";
                    }
                }
                if (requestUrl.StartsWith(controllerPath + path))
                {
                    Type typeInAssembly = GetType();
                    if (TypeAppDefault != null)
                    {
                        typeInAssembly = TypeAppDefault;
                    }
                    Type type = UtilFramework.TypeFromName(frameworkApplication.TypeName, UtilFramework.TypeInAssemblyList(typeInAssembly));
                    if (UtilFramework.IsSubclassOf(type, typeof(App)))
                    {
                        result.App = (App)UtilFramework.TypeToObject(type);
                        result.App.Constructor(webController, frameworkApplication, null);
                        result.RequestPathBase = controllerPath + path;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Server side root object.
    /// </summary>
    public class App
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public App()
        {
            this.IsNamingConvention = true;
            ProcessInit(processList);
        }

        internal void Constructor(WebControllerBase webController, FrameworkApplicationDisplay dbFrameworkApplication, List<FrameworkSessionPermissionDisplay> dbFrameworkSessionPermissionDisplay)
        {
            this.WebController = webController;
            this.DbFrameworkApplication = dbFrameworkApplication;
            this.DbFrameworkSessionPermissionDisplay = dbFrameworkSessionPermissionDisplay;
        }

        internal WebControllerBase WebController { get; private set; } // TODO Use WebController.MemoryCache also to prevent "Information Disclosure" like table names on json object. See also https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/information-disclosure.

        /// <summary>
        /// Returns value from ASP.NET memory cache.
        /// </summary>
        /// <param name="valueSet">Function to create value, if it doesn't exist.</param>
        internal TValue MemoryCacheValueGet<TValue>(object key, Func<TValue> valueSet)
        {
            TValue result;
            result = WebController.MemoryCache.GetOrCreate<TValue>(key, entry => { return valueSet(); });
            return result;
        }

        /// <summary>
        /// Gets DbFrameworkApplication. Used in connection with class AppSelector. See also database table FrameworkApplication.
        /// </summary>
        public FrameworkApplicationDisplay DbFrameworkApplication { get; private set; }

        /// <summary>
        /// Gets DbFrameworkSessionPermissionDisplay. This list is returned by the stored procedure FrameworkLogin.
        /// </summary>
        public List<FrameworkSessionPermissionDisplay> DbFrameworkSessionPermissionDisplay { get; private set; }

        /// <summary>
        /// Gets ApplicationId. See also sql table: FrameworkApplication.
        /// </summary>
        public int ApplicationId
        {
            get
            {
                return DbFrameworkSessionPermissionDisplay.First().ApplicationId.Value;
            }
        }

        /// <summary>
        /// Returns true, if application has certain permission.
        /// </summary>
        public bool IsPermission(FrameworkLoginPermissionDisplay permission)
        {
            var typeInAssemblyList = UtilFramework.TypeInAssemblyList(GetType());
            Type appTypePermission = UtilFramework.TypeFromName(permission.ApplicationTypeName); // AppType declared on permission.
            return DbFrameworkSessionPermissionDisplay.Where
                (
                    item => item.PermissionName == permission.PermissionName && 
                    UtilFramework.IsSubclassOf(UtilFramework.TypeFromName(item.ApplicationTypeName, typeInAssemblyList), appTypePermission) // Permission can be declared on a base App.
                ).Count() > 0;
        }

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type[] TypeComponentInNamespaceList()
        {
            return (new Type[] {
                GetType(), // Namespace of running application.
                typeof(App), // Used for example for class Navigation.
                typeof(AppConfig) // Used for example to show configuration pages on running application and not only on AppConfig.
            }).Distinct().ToArray(); // Enable serialization of components in App and AppConfig namespace.
        }

        /// <summary>
        /// Returns type of main page. Used for first html request.
        /// </summary>
        protected virtual internal Type TypePageMain()
        {
            return typeof(Page);
        }

        /// <summary>
        /// Gets or sets AppJson. This is the application root json component being transferred between server and client.
        /// </summary>
        public AppJson AppJson { get; private set; }

        /// <summary>
        /// Gets IsNamingConvention. If true, bool is displayed as "Yes", "No" instead of "True", "False". Or if ColumnName ends with "Id", column is hidden.
        /// </summary>
        public bool IsNamingConvention { get; private set; }

        /// <summary>
        /// Called after method UtilDataAccessLayer.RowValueToText();
        /// </summary>
        protected virtual internal void CellRowValueToText(Cell cell, ref string result, AppEventArg e)
        {
            if (IsNamingConvention)
            {
                Type type = UtilFramework.TypeUnderlying(cell.TypeColumn);
                if (type == typeof(bool))
                {
                    if ((bool?)cell.Value == false)
                    {
                        result = "No";
                    }
                    if ((bool?)cell.Value == true)
                    {
                        result = "Yes";
                    }
                }
                if (type == typeof(DateTime))
                {
                    // result = string.Format("{0:yyyy-MM-dd HH:mm}", cell.Value); // Use this line to get format with time.
                }
            }
        }

        /// <summary>
        /// Called before text is parsed with method UtilDataAccessLayer.ValueFromText();
        /// </summary>
        protected virtual internal void CellRowValueFromText(Cell cell, ref string result, AppEventArg e)
        {
            if (IsNamingConvention)
            {
                Type type = UtilFramework.TypeUnderlying(cell.TypeColumn);
                if (type == typeof(bool))
                {
                    if (result != null)
                    {
                        if (result.ToUpper() == "YES")
                        {
                            result = "True";
                        }
                        if (result.ToUpper() == "NO")
                        {
                            result = "False";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called before user entered text is parsed with method UtilDataAccessLayer.RowValueToText();
        /// </summary>
        /// <param name="cell">Cell to parse.</param>
        /// <param name="text">Parsed result by default to adjust.</param>
        protected virtual internal void CellTextParse(Cell cell, ref string text, AppEventArg e)
        {
            if (IsNamingConvention)
            {
                Type type = UtilFramework.TypeUnderlying(cell.TypeColumn);
                // Bool
                if (type == typeof(bool))
                {
                    string textUpper = text == null ? null : text.ToUpper();
                    if (textUpper != null)
                    {
                        if (textUpper == "YES")
                        {
                            text = "True";
                        }
                        if (textUpper == "NO")
                        {
                            text = "False";
                        }
                    }
                }
            }
        }

        protected virtual internal void CellTextParseAuto(Cell cell, ref string text, AppEventArg e)
        {
            if (IsNamingConvention)
            {
                Type type = UtilFramework.TypeUnderlying(cell.TypeColumn);
                // Bool
                if (type == typeof(bool))
                {
                    string textLocal = text == null ? null : text.ToUpper();
                    if (textLocal != null)
                    {
                        if (textLocal.StartsWith("Y"))
                        {
                            text = "True";
                        }
                        if (textLocal.StartsWith("N"))
                        {
                            text = "False";
                        }
                    }
                }
                // DateTime
                if (type == typeof(DateTime))
                {
                    if (text != null)
                    {
                        // Make user entered text less restrictive. Allow for example "2018-1-1" and "2018-01-01"
                        string textLocal = text;
                        object value = UtilDataAccessLayer.RowValueFromText(textLocal, cell.TypeColumn);
                        string textCompare = UtilDataAccessLayer.RowValueToText(value, cell.TypeColumn);
                        if (textCompare != null)
                        {
                            if (textLocal.Replace("0", "") == textCompare.Replace("0", ""))
                            {
                                text = textCompare;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called before method Cell.ConfigColumn();
        /// </summary>
        protected virtual internal void CellConfigColumn(Cell column, ConfigColumn result, AppEventArg e)
        {
            if (IsNamingConvention)
            {
                result.IsVisible = UtilApplication.ConfigColumnNameSqlIsId(column.ColumnNameSql) == false;
            }
        }

        /// <summary>
        /// Called after method Row.Query(); Used to override default Row query.
        /// </summary>
        protected virtual internal void RowQuery(ref IQueryable result, GridName gridName)
        {

        }

        /// <summary>
        /// Process AppJson request and return AppJson response.
        /// </summary>
        /// <param name="appJson">AppJson request. (POST)</param>
        /// <param name="sessionNew">New session for first request.</param>
        /// <param name="isRun">If false, used for unit test only. To test GridData object.</param>
        /// <returns>AppJson response.</returns>
        internal AppJson Run(AppJson appJson, Guid sessionNew, bool isRun = true)
        {
            this.AppJson = appJson;
            if (isRun)
            {
                if (AppJson == null || AppJson.Session == null) // First request.
                {
                    int requestCount = AppJson != null ? AppJson.RequestCount : 0;
                    AppJson = new AppJson();
                    AppJson.BrowserUrl = appJson?.BrowserUrl;
                    AppJson.RequestCount = requestCount;
                    AppJson.Session = sessionNew;
                    AppJson.RequestUrl = UtilServer.RequestUrl(false);
                    GridData.SaveJson(); // Initialize AppJson.GridDataJson object.
                    Type typePage = TypePageMain();
                    PageShow(AppJson, typePage);
                }
                //
                foreach (Process process in processList)
                {
                    process.Run(this);
                }
                //
                AppJson.ResponseCount += 1;
                AppJson.VersionServer = UtilFramework.VersionServer;
            }
            //
            return AppJson;
        }

        private GridData gridData;

        /// <summary>
        /// Gets GridData. It makes sure method GridData.LoadJson(); has been called. It's called only once.
        /// </summary>
        public GridData GridData
        {
            get
            {
                if (gridData == null)
                {
                    gridData = new GridData(this);
                    gridData.LoadJson();
                }
                return gridData;
            }
        }

        private ProcessList processListPrivate;

        private ProcessList processList
        {
            get
            {
                if (processListPrivate == null)
                {
                    processListPrivate = new ProcessList(this);
                }
                return processListPrivate;
            }
        }

        /// <summary>
        /// Override this method to register new process.
        /// </summary>
        internal virtual void ProcessInit(ProcessList processList)
        {
            processList.Add<ProcessDivBegin>();
            // Grid
            {
                processList.Add<ProcessGridIsClick>(); // Needs to run before ProcessGridLoadDatabase. It prepares the data.
                processList.Add<ProcessGridLoadDatabase>(); 
                processList.Add<ProcessGridOrderBy>();
                processList.Add<ProcessGridPageIndex>();
                processList.Add<ProcessGridTextParse>();
                processList.Add<ProcessGridLookupRowIsClick>();
                processList.Add<ProcessGridFilter>();
                processList.Add<ProcessGridIsClickMasterDetail>();
                processList.Add<ProcessGridSaveDatabase>();
                processList.Add<ProcessGridLookupOpen>();
                processList.Add<ProcessGridCellButtonIsClick>();
                processList.Add<ProcessGridRowSelectFirst>();
                processList.Add<ProcessNavigationButtonIsClickFirst>();
                processList.Add<ProcessGridIsInsert>();
                processList.Add<ProcessGridSaveJson>(); // SaveJson
                processList.Add<ProcessGridOrderByText>();
                processList.Add<ProcessGridCellIsSelect>();
                processList.Add<ProcessGridFocus>();
                processList.Add<ProcessGridFieldWithLabelIndex>();
                processList.Add<ProcessGridCellIsModifyFalse>();
                processList.Add<ProcessGridIsClickFalse>();
            }
            //
            processList.Add<ProcessButtonIsClickFalse>();
            processList.Add<ProcessLayout>();
            processList.Add<ProcessDivEnd>();
        }

        /// <summary>
        /// Returns currently visible page.
        /// </summary>
        public Page PageVisible(Component owner)
        {
            return owner.List.OfType<Page>().Where(item => item.IsHide == false).SingleOrDefault();
        }

        /// <summary>
        /// Show page. Creates new one if it doesn't exist.
        /// </summary>
        public Page PageShow(Component owner, Type typePage, bool isPageVisibleRemove = true)
        {
            Page pageVisible = PageVisible(owner);
            if (pageVisible != null)
            {
                owner.List.Remove(pageVisible);
            }
            var list = owner.List.OfType<Page>();
            foreach (Page page in list)
            {
                page.IsHide = true;
            }
            Page result = owner.List.OfType<Page>().Where(item => item.GetType() == typePage).SingleOrDefault(); // Make sure there is only one page of type!
            if (result == null)
            {
                result = (Page)UtilFramework.TypeToObject(typePage);
                result.Constructor(owner, typeof(Div));
                result.InitJson(this);
            }
            result.IsHide = false;
            return result;
        }

        /// <summary>
        /// Show page. Creates new one if it doesn't exist.
        /// </summary>
        public TPage PageShow<TPage>(Component owner, bool isPageVisibleRemove = true) where TPage : Page, new()
        {
            return (TPage)PageShow(owner, typeof(TPage), isPageVisibleRemove);
        }

        /// <summary>
        /// Returns BuiltIn guest User for this app.
        /// </summary>
        public static FrameworkLoginUser UserGuest()
        {
            return new FrameworkLoginUser("Guest", "Guest");
        }

        /// <summary>
        /// Returns BuiltIn Permission to configure Factory settings.
        /// </summary>
        public static FrameworkLoginPermissionDisplay PermissionFactoryFull()
        {
            return new FrameworkLoginPermissionDisplay(typeof(App), "FactoryFull", "Configure factory settings.", "Developer");
        }

        /// <summary>
        /// Returns BuiltIn Permission to add and remove users.
        /// </summary>
        /// <returns></returns>
        public static FrameworkLoginPermissionDisplay PermissionUserFull()
        {
            return new FrameworkLoginPermissionDisplay(typeof(App), "UserFull", "Add and remove User.");
        }

        public static FrameworkLoginPermissionDisplay PermissionRoleFull()
        {
            return new FrameworkLoginPermissionDisplay(typeof(App), "RoleFull", "Define User Role.");
        }
    }

    /// <summary>
    /// Navigation pane.
    /// </summary>
    public class Navigation : Div
    {
        public Navigation() { }

        public Navigation(Component owner, GridName gridName = null)
            : base(owner)
        {
            if (gridName == null)
            {
                gridName = new GridName<FrameworkNavigationDisplay>(); // Default grid.
            }
            this.GridNameJson = Application.GridName.ToJson(gridName);
            new Grid(this, gridName).IsHide = true;
            new Div(this) { Name = "Navigation", CssClass = "navigation" }; // Navigation pane.
            new Div(this) { Name = "Content" }; // Content pane.
        }

        public string GridNameJson;

        /// <summary>
        /// Returns sql FrameworkNavigationView.
        /// </summary>
        public GridName GridName()
        {
            return Application.GridName.FromJson(GridNameJson);
        }

        /// <summary>
        /// Returns navigation pane.
        /// </summary>
        /// <returns></returns>
        public Div DivNavigation()
        {
            return List.OfType<Div>().Where(item => item.Name == "Navigation").First();
        }

        /// <summary>
        /// Returns content pane.
        /// </summary>
        public Div DivContent()
        {
            return List.OfType<Div>().Where(item => item.Name == "Content").First();
        }

        /// <summary>
        /// Button is clicked on sql FrameworkNavigationView data grid.
        /// </summary>
        public void ButtonIsClick(AppEventArg e)
        {
            var Row = (FrameworkNavigationDisplay)e.App.GridData.RowGet(e.GridName, e.Index);
            Type type = null;
            if (Row.ComponentNameCSharp != null)
            {
                type = UtilFramework.TypeFromName(Row.ComponentNameCSharp, e.App.TypeComponentInNamespaceList());
            }
            Div divContent = DivContent();
            //
            if (type == null)
            {
                divContent.List.Clear();
                e.App.PageShow(divContent, typeof(Page)); // Empty page. Prevents method ProcessButtonIsClickFirst(); to display default page.
            }
            else
            {
                if (UtilFramework.IsSubclassOf(type, typeof(Page)))
                {
                    e.App.PageShow(divContent, type);
                    new ProcessGridLoadDatabase().Run(e.App); // LoadDatabase if not yet loaded.
                }
                else
                {
                    divContent.List.Clear();
                    Component component = (Component)UtilFramework.TypeToObject(type);
                    component.Constructor(divContent, null);
                }
            }
        }

        /// <summary>
        /// Make sure, if there is no content shown, auto click button of first row.
        /// </summary>
        internal void ProcessButtonIsClickFirst(App app)
        {
            if (DivContent().List.Count == 0)
            {
                GridName gridName = GridName();
                if (app.GridData.QueryInternalIsExist(gridName))
                {
                    if (app.GridData.RowIndexList(gridName).Contains(Index.Row(0)))
                    {
                        if (!app.GridData.IsErrorRowCell(gridName, Index.Row(0))) // Don't auto click button if there is errors.
                        {
                            ButtonIsClick(new AppEventArg(app, gridName, Index.Row(0), null));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populate navigation pane with buttons.
        /// </summary>
        protected internal override void RunEnd(App app)
        {
            Div divNavigation = DivNavigation();
            divNavigation.List.Clear();
            //
            GridName gridName = GridName();
            var indexList = app.GridData.RowIndexList(gridName).Where(item => item.Enum == IndexEnum.Index);
            foreach (Index index in indexList)
            {
                new GridFieldSingle(divNavigation, gridName, "Button", index) { CssClass = "btnNavigation" };
            }
        }
    }
}
