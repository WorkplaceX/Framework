namespace Framework.Application.Setting
{
    using Database.dbo;
    using Framework.Component;
    using System;

    public class AppSetting : App
    {
        protected internal override Type TypePageMain()
        {
            return typeof(PageSetting);
        }
    }

    public class PageSetting : Page
    {
        protected internal override void InitJson(App app)
        {
            new Label(this) { Text = $"Version={ UtilFramework.VersionServer }" };
            new Literal(this) { TextHtml = "<h1>Application</h1>" };
            new Grid(this, new GridName<FrameworkApplicationView>());
            app.GridData.LoadDatabase(new GridName<FrameworkApplicationView>());
            // ConfigGrid
            new Literal(this) { TextHtml = "<h1>Config Grid</h1>" };
            new Grid(this, new GridName<FrameworkConfigGridView>());
            app.GridData.LoadDatabase(new GridName<FrameworkConfigGridView>());
            // ConfigColumn
            new Literal(this) { TextHtml = "<h1>Config Column</h1>" };
            new Grid(this, new GridName<FrameworkConfigColumnView>());
            app.GridData.LoadDatabase(new GridName<FrameworkConfigColumnView>());
        }
    }
}
