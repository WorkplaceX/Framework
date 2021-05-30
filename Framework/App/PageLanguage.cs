namespace Framework.Json
{
    using Database.dbo;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    internal class PageLanguage : PageModal
    {
        public PageLanguage(ComponentJson owner, Type typeRow, string fieldNameCSharp) 
            : base(owner)
        {
            TypeRow = typeRow;
            FieldNameCSharp = fieldNameCSharp;
        }

        public Type TypeRow;

        public string FieldNameCSharp;

        internal GridLanguage GridLanguage;

        internal GridLanguageDisplay GridLanguageDisplay;

        public bool GridLanguageDisplayIsShowAll;

        public Button ButtonShowAll;

        public override async Task InitAsync()
        {
            new Html(DivHeader) { TextHtml = "<h1>Language</h1>" };
            new Html(DivBody) { TextHtml = "<h2>Language</h2>" };
            GridLanguage = new GridLanguage(DivBody);
            new Html(DivBody) { TextHtml = "<h2>Translate</h2>" };
            GridLanguageDisplay = new GridLanguageDisplay(DivBody);

            ButtonShowAll = new Button(DivBody) { TextHtml = "Show All", CssClass = "button is-primary" };

            await GridLanguage.LoadAsync();
        }

        protected internal override async Task ProcessAsync()
        {
            if (ButtonClose.IsClick)
            {
                // Grid Language reload
                var appJson = this.ComponentOwner<AppJson>();
                var gridList = appJson.ComponentListAll<Grid>();
                foreach (var grid in gridList)
                {
                    if (grid != GridLanguage) // Do not reload local grid
                    {
                        if (appJson.SettingInternal(grid).GridIsLanguage)
                        {
                            // Queue reload
                            appJson.RequestJson.CommandAdd(new CommandJson { CommandEnum = CommandEnum.GridIsClickEnum, ComponentId = grid.Id, GridIsClickEnum = GridIsClickEnum.Reload });
                        }
                    }
                }

                // Queue rerender. Used for example for navbar.
                Session.UtilGrid.QueueRerender(GridLanguageDisplay);
            }
            if (ButtonShowAll.IsClick)
            {
                GridLanguageDisplayIsShowAll = !GridLanguageDisplayIsShowAll;
                await GridLanguageDisplay.LoadAsync();
            }

            await base.ProcessAsync();
        }
    }

    internal class GridLanguage : Grid<FrameworkLanguage>
    {
        public GridLanguage(ComponentJson owner) 
            : base(owner)
        {

        }

        protected override void Query(QueryArgs args, QueryResult result)
        {
            // Select row with currently selected language
            var appJson = this.ComponentOwner<AppJson>();
            var languageName = appJson.SettingInternal(this).GridLanguageName;
            if (languageName != null)
            {
                result.RowSelect = (rowList) => rowList.SingleOrDefault(item => item.Name == languageName);
            }
        }

        protected internal override async Task RowSelectAsync()
        {
            await this.ComponentOwner<PageLanguage>().GridLanguageDisplay.LoadAsync();
        }

        protected override async Task InsertAsync(InsertArgs args, InsertResult result)
        {
            var appJson = this.ComponentOwner<AppJson>();
            args.Row.AppTypeName = appJson.GetType().FullName;
            await Data.InsertAsync(args.Row);
            result.IsHandled = true;
        }

        protected override async Task UpdateAsync(UpdateArgs args, UpdateResult result)
        {
            await Data.UpdateAsync(args.Row);
            result.IsHandled = true;
        }
    }

    internal class GridLanguageDisplay : Grid<FrameworkLanguageDisplay>
    {
        public GridLanguageDisplay(ComponentJson owner) 
            : base(owner)
        {

        }

        protected override void Query(QueryArgs args, QueryResult result)
        {
            var pageLanguage = this.ComponentOwner<PageLanguage>();
            var languageName = pageLanguage.GridLanguage.RowSelect?.Name;

            var itemName = pageLanguage.TypeRow.FullName + "." + pageLanguage.FieldNameCSharp + ".";

            result.Query = Data.Query<FrameworkLanguageDisplay>().Where(item => item.LanguageName == languageName);
            if (this.ComponentOwner<PageLanguage>().GridLanguageDisplayIsShowAll == false)
            {
                result.Query = result.Query.Where(item => item.LanguageName == languageName && item.ItemName.StartsWith(itemName));
            }
        }

        protected override async Task UpdateAsync(UpdateArgs args, UpdateResult result)
        {
            var row = new FrameworkLanguageText { Id = args.Row.TextId.GetValueOrDefault(), AppTypeName = args.Row.LanguageAppTypeName, LanguageId = args.Row.LanguageId, ItemId = args.Row.ItemId, Text = args.Row.TextText };
            if (row.Id == 0)
            {
                await Data.InsertAsync(row);
                result.Row.TextId = row.Id; // New Id
            }
            else
            {
                await Data.UpdateAsync(row);
            }

            // Make new translation available
            UtilServer.ServiceGet<BackgroundFrameworkService>().LanguageUpdate(args.Row.LanguageAppTypeName, args.Row.LanguageName, args.Row.ItemName, args.Row.ItemTextDefault, args.Row.TextText);

            result.IsHandled = true;
        }
    }
}
