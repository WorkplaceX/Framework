namespace Application.Doc
{
    using Database.Doc;
    using DatabaseIntegrate.Doc;
    using Framework.DataAccessLayer;
    using Framework.Json;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class AppMain : AppJson
    {
        public override async Task InitAsync()
        {
            CssFrameworkEnum = CssFrameworkEnum.Bulma;

            PageMain = new PageMain(this);
            await PageMain.InitAsync();
        }

        public PageMain PageMain;

        protected override void Setting(SettingArgs args, SettingResult result)
        {
            if (args.Grid != null)
            {
                // IsShowConfig for LoginRole Developer, Admin
                result.GridIsShowConfig = PageMain.LoginUserRoleAppList.Where(item => item.LoginRoleName == LoginRoleIntegrateApp.IdEnum.Developer.IdName() || item.LoginRoleName == LoginRoleIntegrateApp.IdEnum.Admin.IdName()).Any();

                // IsShowConfigDeveloper for LoginRole Developer
                result.GridIsShowConfigDeveloper = PageMain.LoginUserRoleAppList.Where(item => item.LoginRoleName == LoginRoleIntegrateApp.IdEnum.Developer.IdName()).Any();
            }
        }

        protected override async Task NavigateAsync(NavigateArgs args, NavigateResult result)
        {
            if (args.IsFileName("/storage-entry/", out string fileName))
            {
                var row = (await Data.Query<StorageFile>().Where(item => item.FileName == fileName).QueryExecuteAsync()).Single();
                result.Data = row.Data;
                if (args.HttpQuery.ContainsKey("imageThumbnail"))
                {
                    result.Data = row.DataImageThumbnail;
                }
            }
            else
            {
                result.IsSession = true;
            }
        }

        protected override async Task NavigateSessionAsync(NavigateArgs args, NavigateSessionResult result)
        {
            var row = PageMain.GridNavigate.RowList.SingleOrDefault(item => item.NavigatePath == args.NavigatePath);

            bool isPageNotFound = row == null;
            if (row != null)
            {
                if (PageMain.GridNavigate.RowSelect != row)
                {
                    PageMain.GridNavigate.RowSelect = row;
                }
                var pageType = Type.GetType("Application.Doc." + row.PageTypeName);
                if (pageType?.IsSubclassOf(typeof(Page)) == true)
                {
                    if (PageMain.Content.List.FirstOrDefault()?.GetType() != pageType)
                    {
                        PageMain.Content.ComponentListClear();
                        var page = (Page)Activator.CreateInstance(pageType, new object[] { PageMain.Content });
                        await page.InitAsync();
                    }
                }
                else
                {
                    isPageNotFound = true;
                }
                if (row.Name == "Download")
                {
                    result.Data = Encoding.UTF8.GetBytes("Hello world!");
                }
            }

            if (isPageNotFound)
            {
                PageMain.Content.ComponentListClear();
                await new PageNotFound(PageMain.Content).InitAsync();
                result.IsPageNotFound = true;
            }
        }

        public Alert AlertSessionExpired;

        protected override Task ProcessAsync()
        {
            if (IsSessionExpired)
            {
                if (AlertSessionExpired == null)
                {
                    AlertSessionExpired = new Alert(this, "Session expired!", AlertEnum.Warning);
                    IsScrollToTop = true;
                }
            }
            else
            {
                AlertSessionExpired?.ComponentRemove();
            }

            return base.ProcessAsync();
        }
    }
}
