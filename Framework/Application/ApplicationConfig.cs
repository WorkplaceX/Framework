namespace Framework.Application.Config
{
    using Database.dbo;
    using Framework.Component;
    using System;
    using System.Linq;

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
            var div = new Div(this);
            new Grid(div, new GridName<FrameworkCmsNavigationView>());
            div = new Div(this);
            new CmsNavigation(this);
        }
    }

    public class CmsNavigation : Div
    {
        public CmsNavigation() { }

        public CmsNavigation(Component owner) 
            : base(owner)
        {
            new Div(this) { Name = "Navigation" };
            new Div(this) { Name = "Content" };
        }

        public Div DivNavigation()
        {
            return List.OfType<Div>().Where(item => item.Name == "Navigation").First();
        }

        public Div DivContent()
        {
            return List.OfType<Div>().Where(item => item.Name == "Content").First();
        }

        protected internal override void RunEnd(App app)
        {
            Div divNavigation = DivNavigation();
            divNavigation.List.Clear();
            // new GridFieldSingle(divNavigation, new GridName<FrameworkCmsNavigationView>(), "Text", Index.Filter); // Search the navigation bar.
            var indexList = app.GridData.IndexList(new GridName<FrameworkCmsNavigationView>()).Where(item => item.Enum == IndexEnum.Index);
            foreach (Index index in indexList)
            {
                new GridFieldSingle(divNavigation, new GridName<FrameworkCmsNavigationView>(), "Button", index) { CssClass = "btnCmsNavigation" };
            }
        }
    }
}
