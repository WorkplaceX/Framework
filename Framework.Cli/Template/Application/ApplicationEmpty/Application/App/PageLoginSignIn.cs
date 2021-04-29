namespace Application.Doc
{
    using Database.Doc;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class PageLoginSignIn : Page
    {
        public PageLoginSignIn(ComponentJson owner)
            : base(owner)
        {
            new Html(this) { TextHtml = "<h1>User Sign In</h1>", CssClass = "title" };
            Grid = new GridSignIn(this);

            Button = new Button(this) { TextHtml = "Login", CssClass = "button is-primary" };
        }

        public override async Task InitAsync()
        {
            await Grid.LoadAsync();
        }

        public ComponentJson AlertError;

        protected override async Task ProcessAsync()
        {
            AlertError.ComponentRemove();
            if (Button.IsClick)
            {
                var loginUserSession = (LoginUser)Grid.RowSelect;
                var loginUserRoleAppList = (await Data.Query<LoginUserRoleApp>().Where(item => item.LoginUserName == loginUserSession.Name).QueryExecuteAsync());
                if (!loginUserRoleAppList.Any())
                {
                    this.AlertError = new Alert(this, "Username or password wrong!", AlertEnum.Error);
                }
                else
                {
                    var pageMain = this.ComponentOwner<PageMain>();
                    pageMain.LoginUserRoleAppList = loginUserRoleAppList;
                    var loginUserId = pageMain.LoginUserRoleAppList.First().LoginUserId;
                    await pageMain.GridNavigate.LoadAsync();
                    this.ComponentOwner<AppJson>().Navigate("/"); // Navigate to home after login
                }
                Button.TextHtml = string.Format("User={0};", ((LoginUser)Grid.RowSelect).Name);

                // Render grid ConfigDeveloper (coffee icon) if user is a developer.
                await Grid.LoadAsync();
            }
        }

        public GridSignIn Grid;

        public Button Button;
    }

    public class GridSignIn : Grid<LoginUser>
    {
        public GridSignIn(ComponentJson owner) : base(owner) 
        {
            LoginUserList.Add(new LoginUser { });
        }

        public List<LoginUser> LoginUserList = new List<LoginUser>();

        protected override void Query(QueryArgs args, QueryResult result)
        {
            result.Query = LoginUserList.AsQueryable();
        }

        protected override void QueryConfig(QueryConfigArgs args, QueryConfigResult result)
        {
            result.ConfigName = "SignIn";
            result.GridMode = GridMode.Stack;
        }

        protected override Task UpdateAsync(UpdateArgs args, UpdateResult result)
        {
            LoginUserList[0] = args.Row;
            result.IsHandled = true;
            return base.UpdateAsync(args, result);
        }
    }
}
