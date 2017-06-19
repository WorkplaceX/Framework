namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System.Linq;

    /// <summary>
    /// Set Button.IsClick to false.
    /// </summary>
    public class ProcessButtonIsClickFalse : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            foreach (Button button in application.ApplicationJson.ListAll().OfType<Button>())
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
        protected internal override void Run(ApplicationBase application)
        {
            foreach (var page in application.ApplicationJson.ListAll().OfType<Page>())
            {
                page.RunBegin(application);
            }
        }
    }

    /// <summary>
    /// Call method Page.ProcessEnd(); at the End of the process chain.
    /// </summary>
    public class ProcessPageEnd : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            foreach (var page in application.ApplicationJson.ListAll().OfType<Page>())
            {
                page.RunEnd();
            }
        }
    }
}
