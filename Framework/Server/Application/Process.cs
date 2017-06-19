namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System.Linq;

    /// <summary>
    /// Set Button.IsClick to false.
    /// </summary>
    public class ProcessButtonIsClickFalse : ProcessBase
    {
        protected internal override void Process()
        {
            foreach (Button button in ApplicationJson.ListAll().OfType<Button>())
            {
                button.IsClick = false;
            }
        }
    }

    /// <summary>
    /// Call method Page.ProcessBegin(); at the begin of the process chain.
    /// </summary>
    public class ProcessPageBegin : ProcessBase
    {
        protected internal override void Process()
        {
            foreach (var page in ApplicationJson.ListAll().OfType<Page>())
            {
                page.ProcessBegin(Application);
            }
        }
    }

    /// <summary>
    /// Call method Page.ProcessEnd(); at the End of the process chain.
    /// </summary>
    public class ProcessPageEnd : ProcessBase
    {
        protected internal override void Process()
        {
            foreach (var page in ApplicationJson.ListAll().OfType<Page>())
            {
                page.ProcessEnd();
            }
        }
    }
}
