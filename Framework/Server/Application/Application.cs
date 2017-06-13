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
            this.ProcessInit();
        }

        public List<ProcessBase> ProcessList = new List<ProcessBase>();

        public void ProcessListInsertAfter<T>(ProcessBase process) where T : ProcessBase
        {
            int index = -1;
            int count = 0;
            foreach (var item in ProcessList)
            {
                if (item.GetType() == typeof(T))
                {
                    index = count;
                }
                count += 1;
            }
            Framework.Util.Assert(index != -1, "Process not found!");
            ProcessList.Insert(index, process);
        }

        public T ProcessListGet<T>() where T : ProcessBase
        {
            return (T)ProcessList.Where(item => item.GetType() == typeof(T)).FirstOrDefault();
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
        /// Returns Page of type.
        /// </summary>
        public Page Page(Type typePage)
        {
            if (!pageList.ContainsKey(typePage))
            {
                Page page = (Page)Framework.Util.TypeToObject(typePage);
                page.Constructor(this);
                page = pageList[typePage];
                pageList[typePage] = page;
            }
            return pageList[typePage];
        }

        /// <summary>
        /// Returns Page of type.
        /// </summary>
        public T Page<T>() where T : Page
        {
            return (T)Page(typeof(T));
        }

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

        public void PageRemove<T>() where T : Page
        {
            PageRemove(typeof(T));
        }

        protected virtual void ProcessInit()
        {
            ProcessList.Add(new ProcessApplicationInit(this));
            ProcessList.Add(new ProcessGridFilter(this));
            ProcessList.Add(new ProcessGridOrderBy(this));
            ProcessList.Add(new ProcessGridSave(this));
            ProcessList.Add(new ProcessGridRowFirstIsClick(this));
            ProcessList.Add(new ProcessGridIsIsClick(this));
            ProcessList.Add(new ProcessGridLookUp(this));
            ProcessList.Add(new ProcessGridCellButtonIsClick(this));
            ProcessList.Add(new ProcessJson(this));
            ProcessList.Add(new ProcessGridCellIsModifyFalse(this));
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
            }
            else
            {
                typePage = Framework.Util.TypeFromString(ApplicationJson.TypePageVisible, GetType());
            }
            Page result = (Page)Framework.Util.TypeToObject(typePage);
            result.Constructor(this);
            if (result.PageJson.IsInit == false)
            {
                result.Show();
            }
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
            // Process
            {
                foreach (ProcessBase process in ProcessList)
                {
                    // process.ProcessBegin(applicationJson);
                }
                foreach (ProcessBase process in ProcessList)
                {
                    // process.ProcessEnd(applicationJson);
                }
            }
            return ApplicationJson;
        }

        protected virtual internal void ApplicationJsonInit(ApplicationJson applicationJson)
        {
            var container = new LayoutContainer(applicationJson, "Container");
            var rowHeader = new LayoutRow(container, "Header");
            var cellHeader1 = new LayoutCell(rowHeader, "HeaderCell1");
            var rowContent = new LayoutRow(container, "Content");
            var cellContent1 = new LayoutCell(rowContent, "ContentCell1");
            var cellContent2 = new LayoutCell(rowContent, "ContentCell2");
            new Label(cellContent2, "Enter text");
            var rowFooter = new LayoutRow(container, "Footer");
            var cellFooter1 = new LayoutCell(rowFooter, "FooterCell1");
            var button = new Button(cellFooter1, "Hello");
        }
    }
}
