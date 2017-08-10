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
            new Button(this) { Text = "CLICK" };
            new Label(this) { Text = "Hello Setup <b>XYZ</b>" };
            new Literal(this) { TextHtml = "Hello Setup <b>XYZ</b>", CssClass = "my" };
            new Literal(this) { TextHtml = "<h1>LookUp</h1>" };
            new Grid(this, "LookUp");
            new Literal(this) { TextHtml = "<h1>Application</h1>" };
            new Grid(this, "Application");
            app.GridData().LoadDatabase<FrameworkApplicationView>("Application");
            new Literal(this) { TextHtml = "<h1>Config Column</h1>" };
            new Grid(this, "ConfigColumn");
            app.GridData().LoadDatabase<FrameworkConfigColumnView>("ConfigColumn");
            app.GridData().SaveJson(app);

        }
    }
}
