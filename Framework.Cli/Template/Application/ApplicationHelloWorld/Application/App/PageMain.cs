namespace Application.Doc
{
    using Database.dbo;
    using Database.Doc;
    using DatabaseIntegrate.Doc;
    using Framework.Json;
    using Framework.Json.Bulma;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class PageMain : Page
    {
        public PageMain(ComponentJson owner) 
            : base(owner) 
        {
            BulmaNavbar = new Navbar(this) { BrandTextHtml = "<b>Hello</b>World" };

            new Html(this)
            {
                TextHtml =
                @"
                <section class='hero is-primary'>
                  <div class='hero-body'>
                    <div class='container'>
                      <h1 class='title'>
                        Hello World
                      </h1>
                      <h2 class='subtitle'>
                        Getting Started
                      </h2>
                    </div>
                  </div>
                </section>
                "
            };

            Container = new Div(this) { CssClass = "container" };
            Content = new Div(Container);

            GridNavigate = new GridNavigate(this) { IsHide = true };
            GridLanguage = new Grid<Language>(this) { IsHide = true };

            new Html(this)
            {
                TextHtml =
                @"
                <footer class='footer'>
                  <div class='content has-text-centered'>
                    <p>
	                  (C) 2021 by HelloWorld " + Util.Version + @"
                    </p>
                  </div>
                </footer>
                "
            };

            BulmaNavbar.GridAdd(GridLanguage, isNavbarEnd: true, isSelectMode: true);
            BulmaNavbar.GridAdd(GridNavigate);
        }

        public override async Task InitAsync()
        {
            await Task.WhenAll(GridNavigate.LoadAsync(), GridLanguage.LoadAsync());
        }

        public Div Content;

        public Div Container;

        /// <summary>
        /// Gets LoginUserRoleApp. Currently singed in user with its roles.
        /// </summary>
        public List<LoginUserRoleApp> LoginUserRoleAppList = new List<LoginUserRoleApp>();

        /// <summary>
        /// Gets LoginUserName. Currently signed in user. Returns null 
        /// </summary>
        public string LoginUserName
        {
            get
            {
                string result = LoginUserRoleAppList.FirstOrDefault()?.LoginUserName;
                foreach (var item in LoginUserRoleAppList)
                {
                    Util.Assert(item.LoginUserName == result);
                }
                return result;
            }
        }

        public GridNavigate GridNavigate;

        /// <summary>
        /// Gets GridLanguage. Currently selected language.
        /// </summary>
        public Grid GridLanguage;

        public BulmaNavbar BulmaNavbar;
    }

    public class GridNavigate : Grid<NavigateDisplay>
    {
        public GridNavigate(ComponentJson owner) 
            : base(owner)
        {

        }

        protected override Task RowSelectAsync()
        {
            string navigatePath = RowSelect.NavigatePath;
            this.ComponentOwner<AppJson>().Navigate(navigatePath);
            return Task.FromResult(0);
        }

        protected override void QueryConfig(QueryConfigArgs args, QueryConfigResult result)
        {
            result.ConfigGrid = new FrameworkConfigGridIntegrate { RowCountMax = 100 };
            base.QueryConfig(args, result);
        }

        protected override void Query(QueryArgs args, QueryResult result)
        {
            // IdName
            var loginUserName = this.ComponentOwner<PageMain>().LoginUserName;
            if (loginUserName == null)
            {
                // IdName of Guest User
                loginUserName = LoginRoleIntegrateApp.IdEnum.Guest.IdName();
            }

            result.IsRowSelectFirst = false;
            result.Query = args.Query.Where(item => item.LoginUserName == loginUserName);
        }
    }

    public class Navbar : BulmaNavbar
    {
        public Navbar(ComponentJson owner) 
            : base(owner)
        {

        }

        protected override void RowMap(BulmaNavbarRowMapArgs args, BulmaNavbarRowMapResult result)
        {
            base.RowMap(args, result);
            if (args.Row is NavigateDisplay navigate) // Or Language
            {
                var loginUserName = this.ComponentOwner<PageMain>().LoginUserName;
                if (navigate.Name == "LoginSignOut")
                {
                    if (loginUserName == null)
                    {
                        result.IsHide = true;
                    }
                    else
                    {
                        result.TextHtml += " (" + loginUserName + ")";
                    }
                }
            }
        }
    }
}
