namespace Framework.Application
{
    using Database.dbo;
    using Framework.Component;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Run multiple applications on same ASP.NET Core and same database instance. Mapping of url to class App is defined in sql FrameworkApplicationView.
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
        /// Database access of sql FrameworkApplicationView.
        /// </summary>
        protected virtual List<FrameworkApplicationView> DbApplicationList()
        {
            List<FrameworkApplicationView> result;
            result = UtilDataAccessLayer.Query<FrameworkApplicationView>().ToList();
            result = result.Where(item => item.IsExist == true && item.IsActive == true).OrderByDescending(item => item.Path).ToList(); // OrderByDescending: Make sure empty path is last match. And sql view FrameworkApplicationView exists (Execute BuildTool runSqlCreate command). 
            return result;
        }

        internal App Create(WebControllerBase webController, string controllerPath, out string requestPathBase)
        {
            App result = null;
            requestPathBase = controllerPath;
            string requestUrl = webController.HttpContext.Request.Path.ToString();
            if (!requestUrl.EndsWith("/"))
            {
                requestUrl += "/";
            }
            foreach (FrameworkApplicationView frameworkApplication in DbApplicationList())
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
                    Type type = UtilFramework.TypeFromName(frameworkApplication.Type, UtilFramework.TypeInAssemblyList(typeInAssembly));
                    if (UtilFramework.IsSubclassOf(type, typeof(App)))
                    {
                        result = (App)UtilFramework.TypeToObject(type);
                        result.Constructor(webController, frameworkApplication);
                        requestPathBase = controllerPath + path;
                        break;
                    }
                }
            }
            return result;
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

        internal void Constructor(WebControllerBase webController, FrameworkApplicationView dbFrameworkApplication)
        {
            this.WebController = webController;
            this.DbFrameworkApplication = dbFrameworkApplication;
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
        public FrameworkApplicationView DbFrameworkApplication { get; private set; }

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type TypeComponentInNamespace()
        {
            return GetType();
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
        /// <param name="result">Parsed result by default to adjust.</param>
        /// <param name="isDeleteKey">User pressed delete or backspace key.</param>
        protected virtual internal void CellTextParse(Cell cell, ref string result, bool isDeleteKey, AppEventArg e)
        {
            if (IsNamingConvention)
            {
                Type type = UtilFramework.TypeUnderlying(cell.TypeColumn);
                // Bool
                if (type == typeof(bool))
                {
                    string text = result == null ? null : result.ToUpper();
                    if (text != null)
                    {
                        if (text == "YES")
                        {
                            result = "True";
                        }
                        if (text == "NO")
                        {
                            result = "False";
                        }
                        // Key short cut
                        if (isDeleteKey == false)
                        {
                            if (text.StartsWith("Y"))
                            {
                                result = "True";
                            }
                            if (text.StartsWith("N"))
                            {
                                result = "False";
                            }
                        }
                    }
                }
                // DateTime
                if (type == typeof(DateTime))
                {
                    if (result != null)
                    {
                        // Make user entered text less restrictive. Allow for example "2018-1-1" and "2018-01-01"
                        string text = result;
                        object value = UtilDataAccessLayer.RowValueFromText(text, cell.TypeColumn);
                        string textCompare = UtilDataAccessLayer.RowValueToText(value, cell.TypeColumn);
                        if (textCompare != null)
                        {
                            if (text.Replace("0", "") == textCompare.Replace("0", ""))
                            {
                                result = textCompare;
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
        /// Process AppJson request and return AppJson response.
        /// </summary>
        /// <param name="appJson">AppJson request. (POST)</param>
        /// <param name="isRun">If false, used for unit test only. To test GridData object.</param>
        /// <returns>AppJson response.</returns>
        internal AppJson Run(AppJson appJson, bool isRun = true)
        {
            this.AppJson = appJson;
            if (isRun)
            {
                if (AppJson == null || AppJson.Session == null) // First request.
                {
                    int requestCount = AppJson != null ? AppJson.RequestCount : 0;
                    AppJson = new AppJson();
                    AppJson.RequestCount = requestCount;
                    AppJson.Session = Guid.NewGuid();
                    AppJson.RequestUrl = UtilServer.RequestUrl();
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
            processList.Add<ProcessPageBegin>();
            // Grid
            {
                processList.Add<ProcessGridLoadJson>();
                processList.Add<ProcessGridIsClick>();
                processList.Add<ProcessGridOrderBy>();
                processList.Add<ProcessGridPageIndex>();
                processList.Add<ProcessGridTextParse>();
                processList.Add<ProcessGridLookupIsClick>();
                processList.Add<ProcessGridFilter>();
                processList.Add<ProcessGridIsClickMasterDetail>();
                processList.Add<ProcessGridSaveDatabase>();
                processList.Add<ProcessGridLookup>();
                processList.Add<ProcessGridCellButtonIsClick>();
                processList.Add<ProcessGridRowSelectFirst>();
                processList.Add<ProcessGridIsInsert>();
                processList.Add<ProcessGridSaveJson>(); // Save
                processList.Add<ProcessGridOrderByText>();
                processList.Add<ProcessGridCellIsSelect>();
                processList.Add<ProcessGridFocus>();
                processList.Add<ProcessGridFieldWithLabelIndex>();
                processList.Add<ProcessGridIsClickFalse>();
            }
            //
            processList.Add<ProcessButtonIsClickFalse>();
            processList.Add<ProcessLayout>();
            processList.Add<ProcessPageEnd>();
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
    }
}
