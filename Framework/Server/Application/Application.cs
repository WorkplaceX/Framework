namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Server side root object.
    /// </summary>
    public class ApplicationBase
    {
        public ApplicationBase()
        {
            Process2Init(process2List);
        }

        /// <summary>
        ///  Returns assembly to search for classes when deserializing json. (For example: "Database.dbo.Airport")
        /// </summary>
        virtual internal Type TypeRowInAssembly()
        {
            return GetType();
        }

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type TypeComponentInNamespace()
        {
            return GetType();
        }

        /// <summary>
        /// Create main page on first html request.
        /// </summary>
        protected virtual internal Type TypePage2Main()
        {
            return typeof(Page2);
        }

        /// <summary>
        /// Returns type of Page of applications main page.
        /// </summary>
        protected virtual internal Type TypePageMain()
        {
            return typeof(Page);
        }

        /// <summary>
        /// (TypePage, Page).
        /// </summary>
        internal Dictionary<Type, Page> pageList = new Dictionary<Type, Page>();

        /// <summary>
        /// Returns existing Page instance of type or creates new one, if it doesn't exist.
        /// </summary>
        public Page PageInstance(Type typePage)
        {
            if (!pageList.ContainsKey(typePage))
            {
                Page page = (Page)Framework.Util.TypeToObject(typePage);
                page.Constructor(this);
                page = pageList[typePage];
                pageList[typePage] = page;
                if (page.PageJson.IsInit == false)
                {
                    page.ApplicationJsonInit();
                    page.PageJson.IsInit = true;
                }
            }
            return pageList[typePage];
        }

        /// <summary>
        /// Returns existing Page instance of type or creates new one, if it doesn't exist.
        /// </summary>
        public TPage PageInstance<TPage>() where TPage : Page
        {
            return (TPage)PageInstance(typeof(TPage));
        }

        /// <summary>
        /// Remove top level json Component and PageJson (Page state).
        /// </summary>
        public void PageRemove(Type typePage)
        {
            // Remove PageJson
            string type = Framework.Util.TypeToString(typePage);
            if (ApplicationJson.PageJsonList.ContainsKey(type))
            {
                ApplicationJson.PageJsonList.Remove(type);
            }
            //
            if (pageList.ContainsKey(typePage))
            {
                pageList.Remove(typePage);
            }
            if (ApplicationJson.TypePageVisible == type)
            {
                ApplicationJson.TypePageVisible = null;
            }
            ComponentRemove(typePage);
        }

        /// <summary>
        /// Remove top level json Component and PageJson.
        /// </summary>
        public void PageRemove<TPage>() where TPage : Page
        {
            PageRemove(typeof(TPage));
        }

        /// <summary>
        /// Show new page.
        /// </summary>
        /// <param name="typePage">Type of new page to show.</param>
        /// <param name="isPageVisibleRemove">Remove currently visible page and its state.</param>
        /// <returns>Returns instance of new page.</returns>
        public Page PageShow(Type typePage, bool isPageVisibleRemove = true)
        {
            if (isPageVisibleRemove)
            {
                if (ApplicationJson.TypePageVisible != null)
                {
                    Type typePageVisible = Framework.Util.TypeFromString(ApplicationJson.TypePageVisible, GetType());
                    PageRemove(typePageVisible);
                }
            }
            string type = Framework.Util.TypeToString(typePage);
            Page result = PageInstance(typePage); // Make sure page is created.
            ApplicationJson.TypePageVisible = type;
            return result;
        }

        /// <summary>
        /// Show new page.
        /// </summary>
        /// <typeparam name="TPage">Type of new page to show.</typeparam>
        /// <param name="isPageVisibleRemove">Remove currently visible page and its state.</param>
        /// <returns>Returns instance of new page.</returns>
        public TPage PageShow<TPage>(bool isPageVisibleRemove = true) where TPage : Page
        {
            return (TPage)PageShow(typeof(TPage), isPageVisibleRemove);
        }

        /// <summary>
        /// Returns visible Page.
        /// </summary>
        private Page PageVisible()
        {
            Type typePage;
            if (ApplicationJson.TypePageVisible == null)
            {
                typePage = TypePageMain();
                ApplicationJson.TypePageVisible = Framework.Util.TypeToString(typePage);
            }
            else
            {
                typePage = Framework.Util.TypeFromString(ApplicationJson.TypePageVisible, GetType());
            }
            Page result = PageInstance(typePage);
            return result;
        }

        private void ComponentVisible()
        {
            foreach (Component component in ApplicationJson.List)
            {
                if (component.TypePage != null)
                {
                    PageJson pageJson = Util.PageJson(ApplicationJson, component.TypePage);
                    component.IsHide = !(component.TypePage == ApplicationJson.TypePageVisible);
                }
            }
        }

        /// <summary>
        /// Remove all top level json components belonging to page.
        /// </summary>
        private void ComponentRemove(Type typePage)
        {
            string typePageString = Framework.Util.TypeToString(typePage);
            foreach (Component component in ApplicationJson.List.ToArray())
            {
                if (component.TypePage == typePageString)
                {
                    ApplicationJson.List.Remove(component);
                }
            }
        }

        public ApplicationJson ApplicationJson { get; private set; }

        public ApplicationJson Process(ApplicationJson applicationJson, string requestPath)
        {
            this.ApplicationJson = applicationJson;
            if (ApplicationJson == null) // First request.
            {
                ApplicationJson = new ApplicationJson();
                Type typePage = TypePage2Main();
                Page2Show(ApplicationJson, typePage);
                ApplicationJson.PageJsonList = new Dictionary<string, PageJson>();
            }
            //
            foreach (ProcessBase2 process in process2List)
            {
                process.Process();
            }
            //
            Page page = PageVisible();
            page.Process(); // Process visible page.
            ComponentVisible();
            return ApplicationJson;
        }

        private GridData gridData;

        /// <summary>
        /// Make sure method GridData.LoadJson(); has been called. It's called only once.
        /// </summary>
        /// <returns></returns>
        public GridData GridData()
        {
            if (gridData == null)
            {
                gridData = new GridData();
                gridData.LoadJson(ApplicationJson, this);
            }
            return gridData;
        }

        private bool isGridDataTextParse;

        /// <summary>
        /// Make sure method GridData.Text(); has been called. It's called only once.
        /// </summary>
        public void GridDataTextParse()
        {
            if (isGridDataTextParse == false)
            {
                isGridDataTextParse = true;
                GridData().TextParse();
            }
        }
        private ProcessList2 process2ListPrivate;

        private ProcessList2 process2List
        {
            get
            {
                if (process2ListPrivate == null)
                {
                    process2ListPrivate = new ProcessList2(this);
                }
                return process2ListPrivate;
            }
        }

        protected virtual void Process2Init(ProcessList2 processList)
        {
            processList.Add<ProcessPageBegin>();
            processList.Add<ProcessPageEnd>();
        }

        internal Page2 Page2Visible(Component owner)
        {
            return owner.List.OfType<Page2>().Where(item => item.IsHide == false).SingleOrDefault();
        }

        internal Page2 Page2Show(Component owner, Type typePage, bool isPageVisibleRemove = true)
        {
            Page2 pageVisible = Page2Visible(owner);
            if (pageVisible != null)
            {
                owner.List.Remove(pageVisible);
            }
            var list = owner.List.OfType<Page2>();
            foreach (Page2 page in list)
            {
                page.IsHide = true;
            }
            Page2 result = owner.List.OfType<Page2>().Where(item => item.GetType() == typePage).SingleOrDefault(); // Make sure there is only one page of type!
            if (result == null)
            {
                result = (Page2)Activator.CreateInstance(typePage);
                result.Constructor(owner, null);
                result.TypeSet(typeof(Page2));
                result.Init(this);
            }
            result.IsHide = false;
            return result;
        }

        internal TPage Page2Show<TPage>(Component owner, bool isPageVisibleRemove = true) where TPage : Page2, new()
        {
            return (TPage)Page2Show(owner, typeof(TPage), isPageVisibleRemove);
        }
    }
}
