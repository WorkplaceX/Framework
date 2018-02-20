namespace Framework.Application.Config
{
    using Database.dbo;
    using Framework.Component;
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
                List<FrameworkNavigationView> list = new List<FrameworkNavigationView>();
                list.Add(new FrameworkNavigationView() { Text = "Application", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageApplicationConfig)) });
                list.Add(new FrameworkNavigationView() { Text = "Navigation", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageNavigationConfig)) });
                list.Add(new FrameworkNavigationView() { Text = "Grid", ComponentNameCSharp = UtilFramework.TypeToName(typeof(PageGridConfig)) });
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

    public class PageApplicationConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Literal(this) { TextHtml = "<h1>Application Configuration</h1>" };
            new Literal(this) { TextHtml = "<p>Following list shows all applications running on this instance. Configure for example url for each application.</p>" };
            new Grid(this, new GridName<FrameworkApplicationView>());
        }
    }

    public class PageNavigationConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Literal(this) { TextHtml = "<h1>Navigation Configuration</h1>" };
            new Literal(this) { TextHtml = "<p>Define the page navigation of each application.</p>" };
            new Grid(this, new GridName<FrameworkNavigationView>());
        }
    }

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
    /// Navigation bar.
    /// </summary>
    public class Navigation : Div
    {
        public Navigation() { }

        public Navigation(Component owner, GridName gridName = null) 
            : base(owner)
        {
            if (gridName == null)
            {
                gridName = new GridName<FrameworkNavigationView>(); // Default grid.
            }
            this.GridNameJson = Application.GridName.ToJson(gridName);
            new Grid(this, gridName).IsHide = true;
            new Div(this) { Name = "Navigation", CssClass = "navigation" };
            new Div(this) { Name = "Content" };
        }

        public string GridNameJson;

        public GridName GridName()
        {
            return Application.GridName.FromJson(GridNameJson);
        }

        public Div DivNavigation()
        {
            return List.OfType<Div>().Where(item => item.Name == "Navigation").First();
        }

        public Div DivContent()
        {
            return List.OfType<Div>().Where(item => item.Name == "Content").First();
        }

        public void ButtonIsClick(AppEventArg e)
        {
            var Row = (FrameworkNavigationView)e.App.GridData.RowGet(e.GridName, e.Index);
            Type type = null;
            if (Row.ComponentNameCSharp != null)
            {
                type = UtilFramework.TypeFromName(Row.ComponentNameCSharp, e.App.TypeComponentInNamespaceList());
            }
            Div divContent = DivContent();
            //
            if (type == null)
            {
                divContent.List.Clear();
            }
            else
            {
                if (UtilFramework.IsSubclassOf(type, typeof(Page)))
                {
                    e.App.PageShow(divContent, type);
                    new ProcessGridLoadDatabase().Run(e.App); // LoadDatabase if not yet loaded.
                }
                else
                {
                    divContent.List.Clear();
                    Component component = (Component)UtilFramework.TypeToObject(type);
                    component.Constructor(divContent, null);
                }
            }
        }

        /// <summary>
        /// Make sure, if there is no content shown, auto click button of first row.
        /// </summary>
        internal void ProcessButtonIsClickFirst(App app)
        {
            if (DivContent().List.Count == 0)
            {
                GridName gridName = GridName();
                if (app.GridData.QueryInternalIsExist(gridName))
                {
                    if (app.GridData.RowIndexList(gridName).Contains(Index.Row(0)))
                    { 
                        if (!app.GridData.IsErrorRowCell(gridName, Index.Row(0))) // Don't auto click button if there is errors.
                        {
                            ButtonIsClick(new AppEventArg(app, gridName, Index.Row(0), null));
                        }
                    }
                }
            }
        }

        protected internal override void RunEnd(App app)
        {
            Div divNavigation = DivNavigation();
            divNavigation.List.Clear();
            //
            GridName gridName = GridName();
            var indexList = app.GridData.RowIndexList(gridName).Where(item => item.Enum == IndexEnum.Index);
            foreach (Index index in indexList)
            {
                new GridFieldSingle(divNavigation, gridName, "Button", index) { CssClass = "btnNavigation" };
            }
        }
    }
}
