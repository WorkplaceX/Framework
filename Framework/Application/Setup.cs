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
            new Button(this, "CLICK");
            new Label(this, "Hello Setup <b>XYZ</b>");
            new Literal(this, "Literal") { Html = "Hello Setup <b>XYZ</b>", CssClass = "my" };
            new Literal(this, "L2") { Html = "<h1>LookUp</h1>" };
            new Grid(this, "Grid", "LookUp");
            new Literal(this, "L2") { Html = "<h1>Application</h1>" };
            new Grid(this, "Grid", "Application");
            app.GridData().LoadDatabase<FrameworkApplicationView>("Application");
            new Literal(this, "L2") { Html = "<h1>Config Column</h1>" };
            new Grid(this, "Grid", "ConfigColumn");
            app.GridData().LoadDatabase<FrameworkConfigColumnView>("ConfigColumn");
            app.GridData().SaveJson(app);

        }
    }
}
