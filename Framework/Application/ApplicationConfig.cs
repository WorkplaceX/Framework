namespace Framework.Application.Config
{
    using Database.dbo;
    using Framework.Component;
    using System;

    public class AppConfig : App
    {
        protected internal override Type TypePageMain()
        {
            return typeof(PageConfig);
        }
    }

    public class PageConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Label(this) { Text = $"Version={ UtilFramework.VersionServer }" };
            new Literal(this) { TextHtml = "<h1>Application</h1>" };
            new Grid(this, new GridName<FrameworkApplicationView>());
            // ConfigGrid
            new Literal(this) { TextHtml = "<h1>Config Grid</h1>" };
            new Grid(this, new GridName<FrameworkConfigGridView>());
            // ConfigColumn
            new Literal(this) { TextHtml = "<h1>Config Column</h1>" };
            new Grid(this, new GridName<FrameworkConfigColumnView>());
        }
    }

    public class PageCmsConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Literal(this) { TextHtml = "<h1>Navigation</h1>" };
            new Grid(this, new GridName<FrameworkCmsNavigationView>());
        }
    }
}
