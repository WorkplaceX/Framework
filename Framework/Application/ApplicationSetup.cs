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
            new Literal(this) { TextHtml = "<h1>Lookup</h1>" };
            new Grid(this, new GridName("Lookup"));
            new Literal(this) { TextHtml = "<h1>Application</h1>" };
            new Grid(this, new GridName<FrameworkApplicationView>());
            app.GridData.LoadDatabase(new GridName<FrameworkApplicationView>());
            // ConfigTable
            new Literal(this) { TextHtml = "<h1>Config Table</h1>" };
            new Grid(this, new GridName<FrameworkConfigTableView>());
            app.GridData.LoadDatabase(new GridName<FrameworkConfigTableView>());
            // ConfigColumn
            new Literal(this) { TextHtml = "<h1>Config Column</h1>" };
            new Grid(this, new GridName<FrameworkConfigColumnView>());
            app.GridData.LoadDatabase(new GridName<FrameworkConfigColumnView>());
            app.GridData.SaveJson();
        }
    }
}
