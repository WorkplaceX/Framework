namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Process
    {
        protected virtual internal void Run(App app)
        {

        }
    }
    public class ProcessList : IEnumerable<Process>
    {
        internal ProcessList(App app)
        {
            this.App = app;
        }

        public readonly App App;

        private List<Process> processList = new List<Process>();

        public IEnumerator<Process> GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return processList.GetEnumerator();
        }

        private Process ProcessListInsert(Type typeProcess, Type typeProcessFind, bool isAfter)
        {
            // Already exists?
            foreach (Process process in processList)
            {
                Framework.Util.Assert(process.GetType() != typeProcess, "Page already contains process!");
            }
            // Create process
            Process result = (Process)Framework.Util.TypeToObject(typeProcess);
            if (typeProcessFind == null)
            {
                processList.Add(result);
            }
            else
            {
                int index = -1;
                bool isFind = false;
                // Find process
                foreach (Process process in processList)
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
        public TProcess Add<TProcess>() where TProcess : Process
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), null, false);
        }

        public TProcess AddBefore<TProcess, TProcessFind>() where TProcess : Process where TProcessFind : Process
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), false);
        }

        public TProcess AddAfter<TProcess, TProcessFind>() where TProcess : Process where TProcessFind : Process
        {
            return (TProcess)ProcessListInsert(typeof(TProcess), typeof(TProcessFind), true);
        }

        /// <summary>
        /// Returns process of this page.
        /// </summary>
        public T Get<T>() where T : Process
        {
            return (T)processList.Where(item => item.GetType() == typeof(T)).FirstOrDefault();
        }
    }

    /// <summary>
    /// Set Button.IsClick to false.
    /// </summary>
    public class ProcessButtonIsClickFalse : Process
    {
        protected internal override void Run(App app)
        {
            foreach (Button button in app.AppJson.ListAll().OfType<Button>())
            {
                button.IsClick = false;
            }
        }
    }

    /// <summary>
    /// Call method Page.ProcessBegin(); at the begin of the process chain.
    /// </summary>
    public class ProcessPageBegin : Process
    {
        protected internal override void Run(App app)
        {
            foreach (var page in app.AppJson.ListAll().OfType<Page>())
            {
                page.RunBegin(app);
            }
        }
    }

    /// <summary>
    /// Call method Page.ProcessEnd(); at the End of the process chain.
    /// </summary>
    public class ProcessPageEnd : Process
    {
        protected internal override void Run(App app)
        {
            foreach (var page in app.AppJson.ListAll().OfType<Page>())
            {
                page.RunEnd();
            }
        }
    }
}
