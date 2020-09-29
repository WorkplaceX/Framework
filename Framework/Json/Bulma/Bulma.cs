using Framework.DataAccessLayer;
using Framework.Session;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Json.Bulma
{
    /// <summary>
    /// See also: https://bulma.io/documentation/components/navbar/
    /// </summary>
    public class BulmaNavbar : ComponentJson
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public BulmaNavbar(ComponentJson owner)
            : base(owner, nameof(BulmaNavbar))
        {

        }

        /// <summary>
        /// For example company logo.
        /// </summary>
        public string BrandTextHtml;

        [Serialize(SerializeEnum.Session)]
        internal List<BulmaNavbarGrid> GridList = new List<BulmaNavbarGrid>();

        [Serialize(SerializeEnum.Session)]
        internal BulmaNavbarFilter Filter;

        /// <summary>
        /// Add data grid to Navbar.
        /// </summary>
        /// <param name="grid">Data grid with Id, ParentId and TextHtml columns.</param>
        /// <param name="isNavbarEnd">If true, items are placed on the right hand side of the navbar.</param>
        /// <param name="isSelectedMode">If true, currently selected row is shown on top as drop down button. Used for example for language switch.</param>
        public void GridAdd(Grid grid, bool isNavbarEnd = false, bool isSelectedMode = false)
        {
            GridList.Add(new BulmaNavbarGrid { Grid = grid, IsNavbarEnd = isNavbarEnd, IsSelectedMode = isSelectedMode });
        }

        /// <summary>
        /// Add data grid search filter input element to navbar.
        /// </summary>
        /// <param name="grid">Data grid on which to search.</param>
        /// <param name="isNavbarEnd">If true, search box is placed to the right hand side of the navbar.</param>
        public void FilterSet(Grid grid, string fieldName, bool isNavbarEnd = false)
        {
            Filter = new BulmaNavbarFilter { Grid = grid, FieldNameCSharp = fieldName, IsNavbarEnd = isNavbarEnd };
        }

        /// <summary>
        /// Gets ItemStartList. Items on left hand side in navbar.
        /// </summary>
        internal List<BulmaNavbarItem> ItemStartList = new List<BulmaNavbarItem>();

        /// <summary>
        /// Gets ItemStartList. Items on left hand side in navbar.
        /// </summary>
        internal List<BulmaNavbarItem> ItemEndList = new List<BulmaNavbarItem>();

        private void ItemListAll(List<BulmaNavbarItem> itemList, List<BulmaNavbarItem> result)
        {
            if (itemList != null)
            {
                foreach (var navbarItem in itemList)
                {
                    result.Add(navbarItem);
                    ItemListAll(navbarItem.ItemList, result);
                }
            }
        }

        /// <summary>
        /// Returns ItemStartList and ItemEndList recursive.
        /// </summary>
        private List<BulmaNavbarItem> ItemListAll()
        {
            var result = new List<BulmaNavbarItem>();
            ItemListAll(ItemStartList, result);
            ItemListAll(ItemEndList, result);
            return result;
        }

        private void RowMap(BulmaNavbarRowMapArgs args, BulmaNavbarRowMapResult result, string fieldName)
        {
            var propertyInfo = args.Row.GetType().GetProperty(fieldName);
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(args.Row);
                result.GetType().GetField(fieldName).SetValue(result, value);
            }
        }

        /// <summary>
        /// Override this method to custom map incoming data rows to navbar items.
        /// </summary>
        protected virtual void RowMap(BulmaNavbarRowMapArgs args, BulmaNavbarRowMapResult result)
        {
            // Default row mapper
            RowMap(args, result, nameof(result.Id));
            RowMap(args, result, nameof(result.ParentId));
            RowMap(args, result, nameof(result.TextHtml));
            RowMap(args, result, nameof(result.NavigatePath));
            RowMap(args, result, nameof(result.IsDivider));
            RowMap(args, result, nameof(result.IsNavbarEnd));
            RowMap(args, result, nameof(result.Sort));
        }

        private static void Render(BulmaNavbar navbar, BulmaNavbarGrid navbarGrid, ref int navbarItemId)
        {
            Grid grid = navbarGrid.Grid;

            // Map all data grid rows
            var rowMapList = new List<BulmaNavbarRowMapResult>();
            foreach (var rowState in grid.RowStateList)
            {
                if (rowState.RowEnum == GridRowEnum.Index)
                {
                    var rowMapArgs = new BulmaNavbarRowMapArgs { Row = grid.RowList[rowState.RowId.Value - 1] };
                    var rowMapResult = new BulmaNavbarRowMapResult { RowStateId = rowState.Id, IsSelect = rowState.IsSelect };
                    navbar.RowMap(rowMapArgs, rowMapResult);
                    if (rowMapResult.IsHide == false)
                    {
                        rowMapList.Add(rowMapResult);
                    }
                }
            }
            rowMapList = rowMapList.OrderBy(item => item.Sort).ToList();

            // IsSelectedMode for example for language selection.
            if (navbarGrid.IsSelectedMode && rowMapList.Count > 0)
            {
                var rowMapTop = rowMapList.FirstOrDefault(item => item.RowStateId == grid.RowSelectedRowStateId);
                if (rowMapTop == null)
                {
                    rowMapTop = rowMapList.First();
                }
                foreach (var rowMap in rowMapList)
                {
                    if (rowMap != rowMapTop)
                    {
                        rowMap.ParentId = rowMapTop.Id;
                    }
                }
            }

            // Level 0
            Dictionary<int, BulmaNavbarItem> level0List = new Dictionary<int, BulmaNavbarItem>();
            foreach (var rowMap in rowMapList)
            {
                if (rowMap.ParentId == null)
                {
                    BulmaNavbarItemEnum itemEnum = BulmaNavbarItemEnum.Text;
                    // Level 0
                    var navbarItem = new BulmaNavbarItem { Id = navbarItemId += 1, ItemEnum = itemEnum, Grid = grid, RowStateId = rowMap.RowStateId, TextHtml = rowMap.TextHtml, NavigatePath = rowMap.NavigatePath, IsActive = rowMap.IsSelect };
                    level0List.Add(rowMap.Id, navbarItem);
                    bool isNavbarEnd = navbarGrid.IsNavbarEnd;
                    if (rowMap.IsNavbarEnd != null)
                    {
                        isNavbarEnd = rowMap.IsNavbarEnd.Value;
                    }
                    if (isNavbarEnd == false)
                    {
                        navbar.ItemStartList.Add(navbarItem);
                    }
                    else
                    {
                        navbar.ItemEndList.Add(navbarItem);
                    }
                }
            }

            // Level 1
            foreach (var rowMap in rowMapList)
            {
                if (rowMap.ParentId != null)
                {
                    if (level0List.TryGetValue(rowMap.ParentId.Value, out var navbarItemParent))
                    {
                        navbarItemParent.ItemEnum = BulmaNavbarItemEnum.Parent;

                        BulmaNavbarItemEnum itemEnum = BulmaNavbarItemEnum.Text;
                        if (rowMap.IsDivider)
                        {
                            itemEnum = BulmaNavbarItemEnum.Divider;
                        }
                        // Level 1
                        var navbarItem = new BulmaNavbarItem { Id = navbarItemId += 1, ItemEnum = itemEnum, Grid = grid, RowStateId = rowMap.RowStateId, TextHtml = rowMap.TextHtml, NavigatePath = rowMap.NavigatePath, IsActive = rowMap.IsSelect };
                        navbarItemParent.ItemList.Add(navbarItem);
                    }
                }
            }
        }

        internal static void Render(AppJson appJson)
        {
            int navbarItemId = 0;
            foreach (BulmaNavbar navbar in appJson.ComponentListAll().OfType<BulmaNavbar>())
            {
                // ItemList clear
                navbar.ItemStartList.Clear();
                navbar.ItemEndList.Clear();

                // Add level 0 and level 1 to navbar
                foreach (var navbarGrid in navbar.GridList)
                {
                    Render(navbar, navbarGrid, ref navbarItemId);
                }

                // Add data grid filter (input text) to navbar
                if (navbar.Filter != null)
                {
                    var filter = navbar.Filter;
                    var grid = filter.Grid;

                    // Get filter text from value store.
                    new GridFilter(grid).FilterValueList().TryGetValue(filter.FieldNameCSharp, out var gridFilterValue);
                    string filterText = gridFilterValue?.Text;
                    int rowSateId = grid.RowStateList.Single(item => item.RowEnum == GridRowEnum.Filter).Id;

                    // Filter input text box
                    var navbarItem = new BulmaNavbarItem { Id = navbarItemId += 1, ItemEnum = BulmaNavbarItemEnum.Filter, Grid = grid, FilterFieldNameCSharp = filter.FieldNameCSharp, RowStateId = rowSateId, FilterText = filterText, FilterPlaceholder = "Search" };
                    if (filter.IsNavbarEnd == false)
                    {
                        navbar.ItemStartList.Add(navbarItem);
                    }
                    else
                    {
                        navbar.ItemEndList.Add(navbarItem);
                    }
                }
            }
        }

        internal static void ProcessAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, CommandEnum.BulmaNavbarItemIsClick, out CommandJson commandJson, out BulmaNavbar navbar))
            {
                var navbarItem = navbar.ItemListAll().Single(item => item.Id == commandJson.BulmaNavbarItemId);
                Grid grid = navbarItem.Grid;

                // User clicked navbar button
                if (navbarItem.ItemEnum == BulmaNavbarItemEnum.Text)
                {
                    appJson.RequestJson.CommandAdd(new CommandJson { CommandEnum = CommandEnum.GridIsClickRow, Origin = RequestOrigin.Server, ComponentId = grid.Id, RowStateId = navbarItem.RowStateId });
                }

                // User changed navbar filter text
                if (navbarItem.ItemEnum == BulmaNavbarItemEnum.Filter)
                {
                    string filterText = commandJson.BulmaFilterText;
                    int rowStateId = navbarItem.RowStateId;
                    
                    var column = grid.ColumnList.Single(item => item.FieldNameCSharp == navbarItem.FilterFieldNameCSharp);
                    var cell = grid.CellList.Single(item => item.RowStateId == rowStateId && item.ColumnId == column.Id && item.CellEnum == GridCellEnum.Filter);

                    appJson.RequestJson.CommandAdd(new CommandJson { CommandEnum = CommandEnum.GridCellIsModify, Origin = RequestOrigin.Server, ComponentId = grid.Id, RowStateId = navbarItem.RowStateId, GridCellId = cell.Id, GridCellText = filterText });

                }
            }
        }
    }

    internal class BulmaNavbarGrid
    {
        public Grid Grid;

        /// <summary>
        /// Gets or sets IsSelectedMode. If true, currently selected row is shown on top as drop down button. Used for example for language switch.
        /// </summary>
        public bool IsSelectedMode;

        public bool IsNavbarEnd;
    }

    internal class BulmaNavbarFilter
    {
        public Grid Grid;

        public string FieldNameCSharp;

        public bool IsNavbarEnd;
    }

    internal enum BulmaNavbarItemEnum
    {
        None = 0,

        Text = 1,

        Divider = 2,

        /// <summary>
        /// Filter input text box.
        /// </summary>
        Filter = 3,

        Parent = 4
    }

    /// <summary>
    /// Dto for Angular bulma link or input html element.
    /// </summary>
    internal class BulmaNavbarItem
    {
        [Serialize(SerializeEnum.Both)]
        public int Id;

        public BulmaNavbarItemEnum ItemEnum;

        [Serialize(SerializeEnum.Session)]
        public Grid Grid;

        [Serialize(SerializeEnum.Session)]
        public int RowStateId;

        [Serialize(SerializeEnum.Client)]
        public string TextHtml;

        [Serialize(SerializeEnum.Both)]
        public string NavigatePath;

        [Serialize(SerializeEnum.Client)]
        public bool IsActive;

        [Serialize(SerializeEnum.Session)]
        public string FilterFieldNameCSharp;

        [Serialize(SerializeEnum.Client)]
        public string FilterText;

        [Serialize(SerializeEnum.Client)]
        public string FilterPlaceholder;

        [Serialize(SerializeEnum.Both)]
        public List<BulmaNavbarItem> ItemList = new List<BulmaNavbarItem>();
    }

    public class BulmaNavbarRowMapArgs
    {
        public Row Row;
    }

    public class BulmaNavbarRowMapResult
    {
        public int Id;

        public int? ParentId;

        public bool IsHide;

        public string TextHtml;

        public string NavigatePath;

        public bool IsDivider;

        public bool? IsNavbarEnd;

        public double? Sort;

        internal int RowStateId;

        internal bool IsSelect;
    }

    internal static class BulmaExtension
    {
        /// <summary>
        /// See also: https://bulma.io/documentation/elements/notification/
        /// </summary>
        public static Html BulmaAlert(this Page page, string textHtml, AlertEnum alertEnum, int index = 0)
        {
            string htmlText = "<div class='{{CssClass}}'><button class='delete'></button>{{TextHtml}}</div>";
            string cssClass = null;
            switch (alertEnum)
            {
                case AlertEnum.Info:
                    cssClass = "notification is-info";
                    break;
                case AlertEnum.Success:
                    cssClass = "notification is-success";
                    break;
                case AlertEnum.Warning:
                    cssClass = "notification is-warning";
                    break;
                case AlertEnum.Error:
                    cssClass = "notification is-danger";
                    break;
                default:
                    break;
            }
            htmlText = htmlText.Replace("{{CssClass}}", cssClass).Replace("{{TextHtml}}", textHtml);
            Html result = new Html(page) { TextHtml = htmlText, IsNoSanatize = true };
            result.ComponentMove(index);
            return result;
        }

        public static PageModal BulmaModal(this Page page)
        {
            var result = new PageModal(page);
            var divModal = new DivContainer(result) { CssClass = "modal is-active" };
            new Div(divModal) { CssClass = "modal-background" };
            var divModalContent = result.DivContent = new Div(divModal) { CssClass = "modal-content" };
            result.DivContent = new Div(divModalContent) { CssClass = "modal-card-body" };
            result.ButtonClose = new Button(new Div(divModal)) { CssClass = "modal-close is-large" };
            result.ComponentMove(0); // Move to top
            return result;
        }
    }
}
