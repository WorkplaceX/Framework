namespace Framework.Application.Setup
{
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
        }
    }
}
