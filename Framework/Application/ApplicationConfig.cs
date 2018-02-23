namespace Framework.Application.Config
{
    using Database.dbo;
    using Framework.Component;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AppConfig : App
    {
        protected internal override Type TypePageMain()
        {
            return typeof(PageConfig);
        }

        protected internal override void RowQuery(ref IQueryable result, GridName gridName)
        {
            if (gridName == FrameworkNavigationView.GridNameConfig)
            {
                // Returns static navigation for AppConfig.
                List<FrameworkNavigationView> list = new List<FrameworkNavigationView>();
                list.Add(new FrameworkNavigationView() { Text = "Application", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageApplicationConfig)) });
                list.Add(new FrameworkNavigationView() { Text = "Navigation", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageNavigationConfig)) });
                list.Add(new FrameworkNavigationView() { Text = "Grid", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageGridConfig)) });
                list.Add(new FrameworkNavigationView() { Text = "User", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageLoginUserConfig)) });
                result = list.AsQueryable();
            }
        }
    }

    public class PageConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Navigation(this, FrameworkNavigationView.GridNameConfig);
            return;
            //
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

    /// <summary>
    /// List of applications running on this instance.
    /// </summary>
    public class PageApplicationConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Literal(this) { TextHtml = "<h1>Application Configuration</h1>" };
            new Literal(this) { TextHtml = "<p>Following list shows all applications running on this instance. Configure for example url for each application.</p>" };
            new Grid(this, new GridName<FrameworkApplicationView>());
        }
    }

    /// <summary>
    /// Navigation pane configuration.
    /// </summary>
    public class PageNavigationConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Literal(this) { TextHtml = "<h1>Navigation Configuration</h1>" };
            new Literal(this) { TextHtml = "<p>Define the page navigation of each application.</p>" };
            new Grid(this, new GridName<FrameworkNavigationView>());
        }
    }

    /// <summary>
    /// Data grid configuration.
    /// </summary>
    public class PageGridConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Literal(this) { TextHtml = "<h1>Grid Configuration</h1>" };
            new Literal(this) { TextHtml = "<p>Following list shows all available data grids. Configure for example header text or IsReadOnly for each column.</p>" };
            // ConfigGrid
            new Literal(this) { TextHtml = "<h2>Config Grid</h2>" };
            new Grid(this, new GridName<FrameworkConfigGridView>());
            // ConfigColumn
            new Literal(this) { TextHtml = "<h2>Config Column</h2>" };
            new Grid(this, new GridName<FrameworkConfigColumnView>());
        }
    }

    /// <summary>
    /// LoginUser configuration.
    /// </summary>
    public class PageLoginUserConfig : Page
    {
        public PageLoginUserConfig() { }

        public PageLoginUserConfig(Component owner)
            : base(owner)
        {

        }

        protected internal override void InitJson(App app)
        {
            var literalImage = new Literal(this);
            string url = UtilServer.EmbeddedUrl(app, "/UserLogin.png");
            literalImage.TextHtml = string.Format("<img class='imgLogo' src='{0}' />", url);
            new Literal(this) { TextHtml = "<h1>Login User List<h1>" };
            new Literal(this) { TextHtml = "<p>Following list shos all users having access to the system.<p>" };
            new Grid(this, new GridName<FrameworkLoginUser>());
        }
    }
}
