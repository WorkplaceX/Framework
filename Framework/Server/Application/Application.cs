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

        private ApplicationJson applicationJson;

        public ApplicationJson ApplicationJson
        {
            get
            {
                return applicationJson;
            }
        }

        public ApplicationJson Process(ApplicationJson applicationJson, string requestPath)
        {
            if (applicationJson == null) // First request.
            {
                applicationJson = new ApplicationJson();
                applicationJson.PageJsonList = new Dictionary<string, PageJson>();
            }
            this.applicationJson = applicationJson;
            //
            Page page = PageVisible();
            page.Process(); // Process visible page.
            ComponentVisible();
            return ApplicationJson;
        }
    }
}
