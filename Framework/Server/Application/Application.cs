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
            return (T)ProcessList.Where(item => item.GetType() == typeof(T)).First();
        }

        protected virtual void ProcessInit()
        {
            ProcessList.Add(new ProcessGridSave(this));
            ProcessList.Add(new ProcessGridRowFirstIsClick(this));
            ProcessList.Add(new ProcessGridIsIsClick(this));
            ProcessList.Add(new ProcessGridOrderBy(this));
            ProcessList.Add(new ProcessGridLookUp(this));
            ProcessList.Add(new ProcessJson(this));
        }

        public ApplicationJson Process(ApplicationJson applicationJsonIn, string requestPath)
        {
            ApplicationJson applicationJsonOut = Framework.Server.DataAccessLayer.Util.JsonObjectClone<ApplicationJson>(applicationJsonIn);
            if (applicationJsonOut == null || applicationJsonOut.Session == Guid.Empty)
            {
                applicationJsonOut = applicationJsonCreate();
            }
            else
            {
                applicationJsonOut.ResponseCount += 1;
            }
            applicationJsonOut.Name = ".NET Core=" + DateTime.Now.ToString("HH:mm:ss.fff");
            applicationJsonOut.VersionServer = Framework.Util.VersionServer;
            // Process
            {
                foreach (ProcessBase process in ProcessList)
                {
                    process.ProcessBegin(applicationJsonIn, applicationJsonOut);
                    process.ProcessBegin(applicationJsonOut);
                }
                foreach (ProcessBase process in ProcessList)
                {
                    process.ProcessEnd(applicationJsonIn, applicationJsonOut);
                    process.ProcessEnd(applicationJsonOut);
                }
            }
            return applicationJsonOut;
        }

        protected virtual ApplicationJson applicationJsonCreate()
        {
            ApplicationJson result = new ApplicationJson();
            result.Session = Guid.NewGuid();
            //
            var container = new LayoutContainer(result, "Container");
            var rowHeader = new LayoutRow(container, "Header");
            var cellHeader1 = new LayoutCell(rowHeader, "HeaderCell1");
            var rowContent = new LayoutRow(container, "Content");
            var cellContent1 = new LayoutCell(rowContent, "ContentCell1");
            var cellContent2 = new LayoutCell(rowContent, "ContentCell2");
            new Label(cellContent2, "Enter text");
            var rowFooter = new LayoutRow(container, "Footer");
            var cellFooter1 = new LayoutCell(rowFooter, "FooterCell1");
            var button = new Button(cellFooter1, "Hello");
            //
            return result;
        }
    }
}
