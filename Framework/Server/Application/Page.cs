namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections;

    public class ProcessList : IEnumerable<Process2Base>
    {
        internal ProcessList(Page page)
        {
            this.Page = page;
        }

        public readonly Page Page;

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
            result.Constructor(Page);
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

    public class Page
    {
        virtual internal void Constructor(ApplicationBase application)
        {
            this.application = application;
            Application.pageList.Add(GetType(), this);
            ProcessInit();
        }

        private ApplicationBase application;

        public ApplicationBase Application
        {
            get
            {
                return application;
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
            ProcessList.Add<ProcessPageInit>();
            ProcessList.Add<ProcessPage>();
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

        public string TypePage()
        {
            return Framework.Util.TypeToString(GetType());
        }

        public ApplicationJson ApplicationJson
        {
            get
            {
                return Application.ApplicationJson;
            }
        }

        public PageJson PageJson
        {
            get
            {
                string typePage = TypePage();
                return Util.PageJson(ApplicationJson, typePage);
            }
        }

        /// <summary>
        /// Make this page visible.
        /// </summary>
        public void Show()
        {
            ApplicationJson.TypePageVisible = TypePage();
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
    public class PageGrid : Page
    {
        protected override void ProcessInit()
        {
            base.ProcessInit();
        }
    }

    public abstract class Process2Base
    {
        internal void Constructor(Page page)
        {
            this.page = page;
        }

        private Page page;

        public Page Page
        {
            get
            {
                return page;
            }
        }

        public ApplicationBase Application
        {
            get
            {
                return Page.Application;
            }
        }

        public ApplicationJson ApplicationJson
        {
            get
            {
                return Page.Application.ApplicationJson;
            }
        }

        protected virtual internal void Process()
        {

        }
    }

    public abstract class Process2Base<TPage> : Process2Base where TPage : Page
    {
        public new TPage Page
        {
            get
            {
                return (TPage)base.Page;
            }
        }
    }

    /// <summary>
    /// Initialize a page the first time.
    /// </summary>
    public class ProcessPageInit : Process2Base
    {
        protected internal override void Process()
        {
            if (Page.PageJson.IsInit == false)
            {
                Page.ProcessApplicationJsonInit();
                Page.PageJson.IsInit = true;
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
    /// Call method Page.ProcessPage();
    /// </summary>
    public class ProcessPage : Process2Base
    {
        protected internal override void Process()
        {
            Page.ProcessPage();
        }
    }
}
