namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections;
    using System.Runtime.CompilerServices;

    public class Page
    {
        virtual internal void Constructor(ApplicationBase application)
        {
            this.Application = application;
            Application.pageList.Add(GetType(), this);
            ProcessInit(processList);
        }

        public ApplicationBase Application { get; private set; }

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
        /// Initialize page json object.
        /// </summary>
        protected virtual internal void ApplicationJsonInit()
        {

        }

        protected virtual void ProcessInit(ProcessList processList)
        {
            processList.Add<ProcessPage>();
            processList.Add<ProcessJson>();
            processList.Add<ProcessButtonIsClickFalse>();
        }

        protected virtual internal void ProcessPage()
        {

        }

        public void Process()
        {
            foreach (ProcessBase process in processList)
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

        protected T StateGet<T>([CallerMemberName] string name = null)
        {
            T result = default(T);
            if (PageJson.StateList == null)
            {
                PageJson.StateList = new Dictionary<string, object>();
            }
            if (PageJson.StateList.ContainsKey(name))
            {
                result = (T)PageJson.StateList[name];
            }
            return result;
        }

        protected void StateSet(object value, [CallerMemberName] string name = null)
        {
            if (PageJson.StateList == null)
            {
                PageJson.StateList = new Dictionary<string, object>();
            }
            PageJson.StateList[name] = value;
        }
    }

    public class ProcessList2 : IEnumerable<ProcessBase2>
    {
        internal ProcessList2(ApplicationBase application)
        {
            this.Application = application;
        }

        public readonly ApplicationBase Application;

        private List<ProcessBase2> processList = new List<ProcessBase2>();

        public IEnumerator<ProcessBase2> GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        private ProcessBase2 ProcessListInsert(Type typeProcess, Type typeProcessFind, bool isAfter)
        {
            // Already exists?
            foreach (ProcessBase2 process in processList)
            {
                Framework.Util.Assert(process.GetType() != typeProcess, "Page already contains process!");
            }
            // Create process
            ProcessBase2 result = (ProcessBase2)Framework.Util.TypeToObject(typeProcess);
            result.Constructor(Application);
            if (typeProcessFind == null)
            {
                processList.Add(result);
            }
            else
            {
                int index = -1;
                bool isFind = false;
                // Find process
                foreach (ProcessBase2 process in processList)
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
        public TProcess Add<TProcess>() where TProcess : ProcessBase2
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), null, false);
        }

        public TProcess AddBefore<TProcess, TProcessFind>() where TProcess : ProcessBase2 where TProcessFind : ProcessBase2
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), false);
        }

        public TProcess AddAfter<TProcess, TProcessFind>() where TProcess : ProcessBase2 where TProcessFind : ProcessBase2
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), true);
        }

        /// <summary>
        /// Returns process of this page.
        /// </summary>
        public T Get<T>() where T : ProcessBase2
        {
            return (T)processList.Where(item => item.GetType() == typeof(T)).FirstOrDefault();
        }
    }

    public class ProcessList : IEnumerable<ProcessBase>
    {
        internal ProcessList(Page page)
        {
            this.Page = page;
        }

        public readonly Page Page;

        private List<ProcessBase> processList = new List<ProcessBase>();

        public IEnumerator<ProcessBase> GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        private ProcessBase ProcessListInsert(Type typeProcess, Type typeProcessFind, bool isAfter)
        {
            // Already exists?
            foreach (ProcessBase process in processList)
            {
                Framework.Util.Assert(process.GetType() != typeProcess, "Page already contains process!");
            }
            // Create process
            ProcessBase result = (ProcessBase)Framework.Util.TypeToObject(typeProcess);
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
                foreach (ProcessBase process in processList)
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
        public TProcess Add<TProcess>() where TProcess : ProcessBase
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), null, false);
        }

        public TProcess AddBefore<TProcess, TProcessFind>() where TProcess : ProcessBase where TProcessFind : ProcessBase
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), false);
        }

        public TProcess AddAfter<TProcess, TProcessFind>() where TProcess : ProcessBase where TProcessFind : ProcessBase
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), true);
        }

        /// <summary>
        /// Returns process of this page.
        /// </summary>
        public T Get<T>() where T : ProcessBase
        {
            return (T)processList.Where(item => item.GetType() == typeof(T)).FirstOrDefault();
        }
    }

    public class ProcessBase2
    {
        internal void Constructor(ApplicationBase application)
        {
            this.Application = application;
        }

        public ApplicationBase Application { get; private set; }

        public ApplicationJson ApplicationJson
        {
            get
            {
                return Application.ApplicationJson;
            }
        }

        protected virtual internal void Process()
        {

        }
    }

    public class ProcessBase
    {
        internal void Constructor(Page page)
        {
            this.Page = page;
        }

        public Page Page { get; private set; }

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

    /// <summary>
    /// Set Button.IsClick to false.
    /// </summary>
    public class ProcessButtonIsClickFalse : ProcessBase
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
    public class ProcessPage : ProcessBase
    {
        protected internal override void Process()
        {
            Page.ProcessPage();
        }
    }

    /// <summary>
    /// Call method Component.Process(); on json class.
    /// </summary>
    public class ProcessJson : ProcessBase
    {
        protected internal override void Process()
        {
            foreach (Component component in ApplicationJson.ListAll())
            {
                component.Process(Application, ApplicationJson);
            }
        }
    }
}
