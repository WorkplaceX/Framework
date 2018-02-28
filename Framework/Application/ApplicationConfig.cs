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
            if (gridName == FrameworkNavigationDisplay.GridNameConfig)
            {
                // Returns static navigation for AppConfig.
                List<FrameworkNavigationDisplay> list = new List<FrameworkNavigationDisplay>();
                list.Add(new FrameworkNavigationDisplay() { Text = "Application", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageApplicationConfig)) });
                list.Add(new FrameworkNavigationDisplay() { Text = "Navigation", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageNavigationConfig)) });
                list.Add(new FrameworkNavigationDisplay() { Text = "Grid", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageGridConfig)) });
                list.Add(new FrameworkNavigationDisplay() { Text = "User", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageLoginUserConfig)) });
                result = list.AsQueryable();
            }
        }

        /// <summary>
        /// Returns BuiltIn Admin User for this AppConfig.
        /// </summary>
        public static FrameworkLoginUser UserAdmin()
        {
            return new FrameworkLoginUser("Admin", "Admin");
        }
    }

    public class PageConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Navigation(this, FrameworkNavigationDisplay.GridNameConfig);
            return;
            //
            new Label(this) { Text = $"Version={ UtilFramework.VersionServer }" };
            new Literal(this) { TextHtml = "<h1>Application</h1>" };
            new Grid(this, new GridName<FrameworkApplicationDisplay>());
            // ConfigGrid
            new Literal(this) { TextHtml = "<h1>Config Grid</h1>" };
            new Grid(this, new GridName<FrameworkConfigGridDisplay>());
            // ConfigColumn
            new Literal(this) { TextHtml = "<h1>Config Column</h1>" };
            new Grid(this, new GridName<FrameworkConfigColumnDisplay>());
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
            new Grid(this, new GridName<FrameworkApplicationDisplay>());
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
            new Grid(this, new GridName<FrameworkNavigationDisplay>());
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
            new Grid(this, new GridName<FrameworkConfigGridDisplay>());
            // ConfigColumn
            new Literal(this) { TextHtml = "<h2>Config Column</h2>" };
            new Grid(this, new GridName<FrameworkConfigColumnDisplay>());
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
            new Literal(this) { TextHtml = "<h1>Login User<h1>" };
            new Literal(this) { TextHtml = "<p>Following list shos all users having access to the system.<p>" };
            new Grid(this, new GridName<FrameworkLoginUserDisplay>());
        }
    }
}
