namespace Framework.Server.Application.Json
{
    using System;

    public class Page : Component
    {
        /// <summary>
        /// Create new page with method Application.PageShow();
        /// </summary>
        public Page()
        {

        }

        protected virtual internal void InitJson(ApplicationBase application)
        {

        }

        /// <summary>
        /// Show page. Create if it doesn't exist.
        /// </summary>
        /// <param name="isPageVisibleRemove">Remove currently visible page and it's state.</param>
        public Page PageShow(ApplicationBase application, Type typePage, bool isPageVisibleRemove = true)
        {
            return application.PageShow(this.Owner(application.ApplicationJson), typePage, isPageVisibleRemove);
        }

        /// <summary>
        /// Show page. Create if it doesn't exist.
        /// </summary>
        /// <param name="isPageVisibleRemove">Remove currently visible page and it's state.</param>
        public TPage PageShow<TPage>(ApplicationBase application, bool isPageVisibleRemove = true) where TPage : Page
        {
            return (TPage)application.PageShow(this.Owner(application.ApplicationJson), typeof(TPage), isPageVisibleRemove);
        }

        protected virtual internal void ProcessBegin(ApplicationBase application)
        {

        }

        protected virtual internal void ProcessEnd()
        {

        }
    }
}

namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections;

    public class ProcessList : IEnumerable<ProcessBase>
    {
        internal ProcessList(ApplicationBase application)
        {
            this.Application = application;
        }

        public readonly ApplicationBase Application;

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

    public class ProcessBase
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
}
