namespace Framework.Application.Setup
{
    using Database.dbo;
    using Framework.Component;
    using System;

    public class AppSetup : App
    {
        protected internal override Type TypePageMain()
        {
            return typeof(PageSetup);
        }
    }

    public class PageSetup : Page
    {
        protected internal override void InitJson(App app)
        {
            new Label(this) { Text = $"Version={ UtilFramework.VersionServer }" };
            new Literal(this) { TextHtml = "<h1>LookUp</h1>" };
            new Grid(this, new GridName("LookUp"));
            new Literal(this) { TextHtml = "<h1>Application</h1>" };
            new Grid(this, new GridName("Application"));
            app.GridData.LoadDatabase<FrameworkApplicationView>(new GridName("Application"));
            // ConfigTable
            new Literal(this) { TextHtml = "<h1>Config Table</h1>" };
            new Grid(this, new GridName("ConfigTable"));
            app.GridData.LoadDatabase<FrameworkConfigTableView>(new GridName("ConfigTable"));
            // ConfigColumn
            new Literal(this) { TextHtml = "<h1>Config Column</h1>" };
            new Grid(this, new GridName("ConfigColumn"));
            app.GridData.LoadDatabase<FrameworkConfigColumnView>(new GridName("ConfigColumn"));
            app.GridData.SaveJson();
        }
    }
}
