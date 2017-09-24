namespace Framework.Application
{
    using Database.dbo;
    using Framework.Component;
    using Framework.DataAccessLayer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Run multiple applications on same ASP.NET Core and same database instance.
    /// </summary>
    public class AppSelector
    {
        /// <summary>
        /// Constructor with default app, if no database connection exists.
        /// </summary>
        public AppSelector(Type typeAppDefault)
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

        protected virtual List<FrameworkApplicationView> DbApplicationList()
        {
            List<FrameworkApplicationView> result;
            if (UtilDataAccessLayer.IsConnectionString == false)
            {
                result = new List<FrameworkApplicationView>();
                if (TypeAppDefault != null)
                {
                    result.Add(new FrameworkApplicationView() { Path = null, Type = UtilFramework.TypeToName(TypeAppDefault) }); // Register class AppMain programmatically, if no database connection.
                }
            }
            else
            {
                result = UtilDataAccessLayer.Query<FrameworkApplicationView>().Where(item => item.IsActive == true).OrderByDescending(item => item.Path).ToList(); // Make sure empty path is last match.
            }
            return result;
        }

        internal App Create(ControllerBase controller, string controllerPath, out string requestPathBase)
        {
            App result = null;
            requestPathBase = controllerPath;
            string requestUrl = controller.HttpContext.Request.Path.ToString();
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
                        result.Constructor(frameworkApplication);
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
            ProcessInit(processList);
        }

        internal void Constructor(FrameworkApplicationView dbFrameworkApplication)
        {
            this.DbFrameworkApplication = dbFrameworkApplication;
        }

        /// <summary>
        /// Gets DbFrameworkApplication. Used in connection with class AppSelector. See also database table FrameworkApplication.
        /// </summary>
        public FrameworkApplicationView DbFrameworkApplication { get; private set; }

        /// <summary>
        /// (TypeRowName, Config)
        /// </summary>
        private Dictionary<string, List<FrameworkConfigColumnView>> cacheDbConfigColumnList = new Dictionary<string, List<FrameworkConfigColumnView>>();

        /// <summary>
        /// Returns ConfigColumnList for a table.
        /// </summary>
        protected virtual internal List<FrameworkConfigColumnView> DbConfigColumnList(Type typeRow)
        {
            List<FrameworkConfigColumnView> result;
            string typeRowName = UtilDataAccessLayer.TypeRowToName(typeRow);
            if (cacheDbConfigColumnList.ContainsKey(typeRowName))
            {
                result = cacheDbConfigColumnList[typeRowName];
            }
            else
            {
                result = UtilDataAccessLayer.Query<FrameworkConfigColumnView>().Where(item => item.TableName == typeRowName & item.TableIsExist == true & item.ColumnIsExist == true).ToList();
                cacheDbConfigColumnList[typeRowName] = result;
            }
            return result;
        }

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
        /// Define for example grid column header globaly. See also method Cell.InfoColumn();
        /// </summary>
        protected virtual internal void InfoColumn(GridName gridName, Type typeRow, InfoColumn result)
        {
            
        }

        /// <summary>
        /// Define for example grid column header globaly. See also method Cell.InfoCell();
        /// </summary>
        protected virtual internal void InfoCell(GridName gridName, Index index, Cell cell, InfoCell result)
        {
            switch (index.Enum)
            {
                case IndexEnum.Filter:
                    result.PlaceHolder = "Search";
                    result.CssClass.Add("gridFilter");
                    break;
                case IndexEnum.New:
                    result.PlaceHolder = "New";
                    result.CssClass.Add("gridNew");
                    break;
            }
        }

        /// <summary>
        /// Called after method UtilDataAccessLayer.ValueToText();
        /// </summary>
        protected virtual internal void CellValueToText(GridName gridName, Index index, Cell cell, ref string result)
        {

        }

        /// <summary>
        /// Called before user entered text is parsed with UtilDataAccessLayer.ValueFromText();
        /// </summary>
        protected virtual internal void CellValueFromText(GridName gridName, Index index, Cell cell, ref string result)
        {

        }

        protected virtual internal void ColumnIsVisible(GridName gridName, Cell cell, ref bool result)
        {

        }

        internal AppJson Run(AppJson appJson, HttpContext httpContext)
        {
            this.AppJson = appJson;
            if (AppJson == null || AppJson.Session == null) // First request.
            {
                int requestCount = AppJson != null ? AppJson.RequestCount : 0;
                AppJson = new AppJson();
                AppJson.RequestCount = requestCount;
                AppJson.Session = Guid.NewGuid();
                AppJson.RequestUrl = string.Format("{0}://{1}/", httpContext.Request.Scheme, httpContext.Request.Host.Value);
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

        private bool isGridDataTextParse;

        /// <summary>
        /// Make sure method GridData.Text(); has been called. It's called only once.
        /// </summary>
        internal void GridDataTextParse()
        {
            if (isGridDataTextParse == false)
            {
                isGridDataTextParse = true;
                GridData.TextParse();
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
                processList.Add<ProcessGridIsClick>();
                processList.Add<ProcessGridOrderBy>();
                processList.Add<ProcessGridFilter>();
                processList.Add<ProcessGridIsClickMasterDetail>();
                processList.Add<ProcessGridLookUpIsClick>();
                processList.Add<ProcessGridLookUp>();
                processList.Add<ProcessGridSave>();
                processList.Add<ProcessGridCellButtonIsClick>();
                processList.Add<ProcessGridRowSelectFirst>();
                processList.Add<ProcessGridSaveJson>(); // Save
                processList.Add<ProcessGridOrderByText>();
                processList.Add<ProcessGridFocus>();
                processList.Add<ProcessGridFieldWithLabelIndex>();
                processList.Add<ProcessGridCellIsModifyFalse>();
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
