namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Server side root object.
    /// </summary>
    public class ApplicationServerBase
    {
        public ApplicationServerBase()
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
        /// Returns type of PageServer of applications main page.
        /// </summary>
        protected virtual internal Type TypePageServerMain()
        {
            return typeof(PageServer);
        }

        /// <summary>
        /// (TypePageServer, PageServer).
        /// </summary>
        internal Dictionary<Type, PageServer> pageServerList = new Dictionary<Type, PageServer>();

        /// <summary>
        /// Returns PageServer of type.
        /// </summary>
        public PageServer PageServer(Type typePageServer)
        {
            PageServer result = null;
            if (!pageServerList.ContainsKey(typePageServer))
            {
                result = (PageServer)Framework.Util.TypeToObject(typePageServer);
                result.Constructor(this);
                result = pageServerList[typePageServer];
            }
            return pageServerList[typePageServer];
        }

        /// <summary>
        /// Returns PageServer of type.
        /// </summary>
        public T PageServer<T>() where T : PageServer
        {
            return (T)PageServer(typeof(T));
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
        /// Returns visible PageServer.
        /// </summary>
        private PageServer PageServerVisible()
        {
            Type typePageServer;
            if (ApplicationJson.TypeNamePageVisible == null)
            {
                typePageServer = TypePageServerMain();
            }
            else
            {
                typePageServer = Framework.Util.TypeFromTypeName(ApplicationJson.TypeNamePageVisible, GetType());
            }
            PageServer result = (PageServer)Framework.Util.TypeToObject(typePageServer);
            result.Constructor(this);
            if (result.PageJson.IsInit == false)
            {
                result.Show();
            }
            return result;
        }

        private void PageJsonVisible()
        {
            foreach (Component component in ApplicationJson.List)
            {
                if (component.TypeNamePage != null)
                {
                    PageJson pageJson = Util.PageJson(ApplicationJson, component.TypeNamePage);
                    component.IsHide = !(component.TypeNamePage == ApplicationJson.TypeNamePageVisible);
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
            PageServer pageServer = PageServerVisible();
            pageServer.Process(); // Process visible page.
            PageJsonVisible();
            // Process
            {
                foreach (ProcessBase process in ProcessList)
                {
                    process.ProcessBegin(applicationJson);
                }
                foreach (ProcessBase process in ProcessList)
                {
                    process.ProcessEnd(applicationJson);
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
