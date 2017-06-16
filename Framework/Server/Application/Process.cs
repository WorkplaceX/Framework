namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;

    /// <summary>
    /// Call method Page2.ProcessBegin(); at the begin of the process chain.
    /// </summary>
    public class ProcessPageBegin : ProcessBase2
    {
        protected internal override void Process()
        {
            foreach (var page in ApplicationJson.ListAll<Page2>())
            {
                page.ProcessBegin(Application);
            }
        }
    }

    /// <summary>
    /// Call method Page2.ProcessEnd(); at the End of the process chain.
    /// </summary>
    public class ProcessPageEnd : ProcessBase2
    {
        protected internal override void Process()
        {
            foreach (var page in ApplicationJson.ListAll<Page2>())
            {
                page.ProcessEnd();
            }
        }
    }
}
