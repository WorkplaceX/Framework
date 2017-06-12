namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections;

    public class ProcessList : IEnumerable<Process2Base>
    {
        internal ProcessList(PageServer pageServer)
        {
            this.PageServer = pageServer;
        }

        public readonly PageServer PageServer;

        private List<Process2Base> processList = new List<Process2Base>();

        public IEnumerator<Process2Base> GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        private Process2Base ProcessListInsert(Type typeProcess, Type typeProcessFind, bool isAfter)
        {
            // Already exists?
            foreach (Process2Base process in processList)
            {
                Framework.Util.Assert(process.GetType() != typeProcess, "Page already contains process!");
            }
            // Create process
            Process2Base result = (Process2Base)Framework.Util.TypeToObject(typeProcess);
            result.Constructor(PageServer);
            if (typeProcessFind == null)
            {
                processList.Add(result);
            }
            else
            {
                int index = -1;
                bool isFind = false;
                // Find process
                foreach (Process2Base process in processList)
                {
                    index += 1;
                    if (process.GetType() == typeProcessFind)
                    {
                        isFind = true;
                        break;
                    }
                }
                Framework.Util.Assert(isFind, "Process not found!");
                if (isAfter)
                {
                    index += 1;
                }
                processList.Insert(index, result);
            }
            return result;
        }

        /// <summary>
        /// Create process for this page.
        /// </summary>
        public TProcess Add<TProcess>() where TProcess : Process2Base
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), null, false);
        }

        public TProcess AddBefore<TProcess, TProcessFind>() where TProcess : Process2Base where TProcessFind : Process2Base
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), false);
        }

        public TProcess AddAfter<TProcess, TProcessFind>() where TProcess : Process2Base where TProcessFind : Process2Base
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), true);
        }

        /// <summary>
        /// Returns process of this page.
        /// </summary>
        public T Get<T>() where T : Process2Base
        {
            return (T)processList.Where(item => item.GetType() == typeof(T)).FirstOrDefault();
        }
    }

    public class PageServer
    {
        virtual internal void Constructor(ApplicationServerBase applicationServer)
        {
            this.applicationServer = applicationServer;
            ApplicationServer.pageServerList.Add(GetType(), this);
            ProcessInit();
        }

        private ApplicationServerBase applicationServer;

        public ApplicationServerBase ApplicationServer
        {
            get
            {
                return applicationServer;
            }
        }

        private ProcessList processList;

        public ProcessList ProcessList
        {
            get
            {
                if (processList == null)
                {
                    processList = new ProcessList(this);
                }
                return processList;
            }
        }

        /// <summary>
        /// Initialize page json object.
        /// </summary>
        protected virtual internal void ProcessApplicationJsonInit()
        {

        }

        protected virtual void ProcessInit()
        {
            ProcessList.Add<ProcessPageServerInit>();
            ProcessList.Add<ProcessPageServer>();
            ProcessList.Add<ProcessButtonIsClickFalse>();
        }

        protected virtual internal void ProcessPage()
        {

        }

        public void Process()
        {
            foreach (Process2Base process in ProcessList)
            {
                process.Process();
            }
        }

        public string TypeNamePageServer()
        {
            return Framework.Util.TypeToTypeName(GetType());
        }

        public ApplicationJson ApplicationJson
        {
            get
            {
                return ApplicationServer.ApplicationJson;
            }
        }

        public PageJson PageJson
        {
            get
            {
                string typeNamePageServer = TypeNamePageServer();
                return Util.PageJson(ApplicationJson, typeNamePageServer);
            }
        }

        /// <summary>
        /// Make this page visible.
        /// </summary>
        public void Show()
        {
            ApplicationJson.TypeNamePageVisible = TypeNamePageServer();
        }

        protected T StateGet<T>(string name)
        {
            T result = default(T);
            if (PageJson.StateList.ContainsKey(name))
            {
                result = (T)PageJson.StateList[name];
            }
            return result;
        }

        protected void StateSet(string name, object value)
        {
            if (PageJson.StateList == null)
            {
                PageJson.StateList = new Dictionary<string, object>();
            }
            PageJson.StateList[name] = value; 
        }
    }

    /// <summary>
    /// Page to process data grid.
    /// </summary>
    public class PageServerGridData : PageServer
    {
        protected override void ProcessInit()
        {
            base.ProcessInit();
        }
    }

    public abstract class Process2Base
    {
        internal void Constructor(PageServer pageServer)
        {
            this.pageServer = pageServer;
        }

        private PageServer pageServer;

        public PageServer PageServer
        {
            get
            {
                return pageServer;
            }
        }

        public ApplicationServerBase ApplicationServer
        {
            get
            {
                return PageServer.ApplicationServer;
            }
        }

        public ApplicationJson ApplicationJson
        {
            get
            {
                return PageServer.ApplicationServer.ApplicationJson;
            }
        }

        protected virtual internal void Process()
        {

        }
    }

    public abstract class Process2Base<TPageServer> : Process2Base where TPageServer : PageServer
    {
        public new TPageServer PageServer
        {
            get
            {
                return (TPageServer)base.PageServer;
            }
        }
    }

    /// <summary>
    /// Initialize a page the first time.
    /// </summary>
    public class ProcessPageServerInit : Process2Base
    {
        protected internal override void Process()
        {
            if (PageServer.PageJson.IsInit == false)
            {
                PageServer.ProcessApplicationJsonInit();
                PageServer.PageJson.IsInit = true;
            }
        }
    }

    /// <summary>
    /// Set Button.IsClick to false.
    /// </summary>
    public class ProcessButtonIsClickFalse : Process2Base
    {
        protected internal override void Process()
        {
            foreach (Button button in ApplicationJson.ListAll<Button>())
            {
                button.IsClick = false;
            }
        }
    }

    /// <summary>
    /// Call method PageServer.ProcessPage();
    /// </summary>
    public class ProcessPageServer : Process2Base
    {
        protected internal override void Process()
        {
            PageServer.ProcessPage();
        }
    }
}
