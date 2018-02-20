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

    public class PageNavigationConfig : Page
    {
        protected internal override void InitJson(App app)
        {
            new Literal(this) { TextHtml = "<h1>Navigation</h1>" };
            new Grid(this, new GridName<FrameworkNavigationView>());
            new Navigation(this);
        }
    }

    /// <summary>
    /// Navigation bar.
    /// </summary>
    public class Navigation : Div
    {
        public Navigation() { }

        public Navigation(Component owner) 
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
                GridName gridName = new GridName<FrameworkNavigationView>();
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
            var indexList = app.GridData.RowIndexList(new GridName<FrameworkNavigationView>()).Where(item => item.Enum == IndexEnum.Index);
            foreach (Index index in indexList)
            {
                new GridFieldSingle(divNavigation, new GridName<FrameworkNavigationView>(), "Button", index) { CssClass = "btnNavigation" };
            }
        }
    }
}
