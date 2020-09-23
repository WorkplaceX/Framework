using DocumentFormat.OpenXml.Office.CustomUI;
using Framework.DataAccessLayer;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Json.Bulma
{
    /// <summary>
    /// See also: https://bulma.io/documentation/components/navbar/
    /// </summary>
    public class BulmaNavbar : ComponentJson
    {
        public BulmaNavbar(ComponentJson owner)
            : base(owner, nameof(BulmaNavbar))
        {

        }

        public string BrandTextHtml;

        [Serialize(SerializeEnum.Session)]
        internal List<BulmaNavbarGrid> GridList = new List<BulmaNavbarGrid>();

        [Serialize(SerializeEnum.Session)]
        internal BulmaNavbarFilter Filter;

        /// <summary>
        /// Add data grid to Navbar.
        /// </summary>
        /// <param name="grid">Data grid with Id, ParentId and TextHtml columns.</param>
        /// <param name="isSelectedMode">If true, currently selected row is shown on top as drop down button. Used for example for language switch.</param>
        public void GridAdd(Grid grid, bool isSelectedMode = false)
        {
            GridList.Add(new BulmaNavbarGrid { Grid = grid, IsSelectedMode = isSelectedMode });
        }

        /// <summary>
        /// Add data grid search filter to navbar.
        /// </summary>
        public void FilterSet(Grid grid, string fieldName, bool isNavbarEnd = false)
        {
            Filter = new BulmaNavbarFilter { Grid = grid, FieldName = fieldName, IsNavbarEnd = isNavbarEnd };
        }

        /// <summary>
        /// Gets ItemStartList. Items on left hand side in navbar.
        /// </summary>
        [Serialize(SerializeEnum.Client)]
        internal List<BulmaNavbarItem> ItemStartList = new List<BulmaNavbarItem>();

        /// <summary>
        /// Gets ItemStartList. Items on left hand side in navbar.
        /// </summary>
        [Serialize(SerializeEnum.Client)]
        internal List<BulmaNavbarItem> ItemEndList = new List<BulmaNavbarItem>();

        private void RowMap(BulmaNavbarRowMapArgs args, BulmaNavbarRowMapResult result, string fieldName)
        {
            var propertyInfo = args.Row.GetType().GetProperty(fieldName);
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(args.Row);
                result.GetType().GetField(fieldName).SetValue(result, value);
            }
        }

        protected virtual void RowMap(BulmaNavbarRowMapArgs args, BulmaNavbarRowMapResult result)
        {
            // Default row mapper
            RowMap(args, result, nameof(result.Id));
            RowMap(args, result, nameof(result.ParentId));
            RowMap(args, result, nameof(result.TextHtml));
            RowMap(args, result, nameof(result.IsDivider));
            RowMap(args, result, nameof(result.IsNavbarEnd));
            RowMap(args, result, nameof(result.Sort));
        }

        private static void Render(BulmaNavbar navbar, Grid grid, ref int navbarItemId)
        {
            // Map all data grid rows
            var rowMapList = new List<BulmaNavbarRowMapResult>();
            foreach (var rowState in grid.RowStateList)
            {
                if (rowState.RowEnum == Session.GridRowEnum.Index)
                {
                    var rowMapArgs = new BulmaNavbarRowMapArgs { Row = rowState.RowGet(grid) };
                    var rowMapResult = new BulmaNavbarRowMapResult { RowStateId = rowState.Id };
                    navbar.RowMap(rowMapArgs, rowMapResult);
                    if (rowMapResult.IsHide == false)
                    {
                        rowMapList.Add(rowMapResult);
                    }
                }
            }
            rowMapList = rowMapList.OrderBy(item => item.Sort).ToList();

            // Level 0
            Dictionary<int, BulmaNavbarItem> level0List = new Dictionary<int, BulmaNavbarItem>();
            foreach (var item in rowMapList)
            {
                if (item.ParentId == null)
                {
                    BulmaNavbarItemEnum itemEnum = BulmaNavbarItemEnum.Text;
                    navbarItemId += 1;
                    var navbarItem = new BulmaNavbarItem { Id = navbarItemId, ItemEnum = itemEnum, Grid = grid, RowStateId = item.RowStateId, TextHtml = item.TextHtml };
                    level0List.Add(item.Id, navbarItem);
                    if (item.IsNavbarEnd == false)
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
            foreach (var item in rowMapList)
            {
                if (item.ParentId != null)
                {
                    if (level0List.TryGetValue(item.ParentId.Value, out var navbarItemParent))
                    {
                        navbarItemParent.ItemEnum = BulmaNavbarItemEnum.Parent;

                        BulmaNavbarItemEnum itemEnum = BulmaNavbarItemEnum.Text;
                        if (item.IsDivider)
                        {
                            itemEnum = BulmaNavbarItemEnum.Divider;
                        }
                        var navbarItem = new BulmaNavbarItem { Id = navbarItemId, ItemEnum = itemEnum, Grid = grid, RowStateId = item.RowStateId, TextHtml = item.TextHtml };
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
                // Add level 0 and level 1 to navbar
                foreach (var grid in navbar.GridList)
                {
                    Render(navbar, grid.Grid, ref navbarItemId);
                }

                // Add data grid filter to navbar
                if (navbar.Filter != null)
                {
                    var filter = navbar.Filter;
                    navbarItemId += 1;
                    var navbarItem = new BulmaNavbarItem { Id = navbarItemId, ItemEnum = BulmaNavbarItemEnum.Filter, Grid = filter.Grid, FilterFieldName = filter.FieldName, FilterPlaceholder = "Search" };
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
    }

    public class BulmaNavbarGrid
    {
        public Grid Grid;

        /// <summary>
        /// Gets or sets IsSelectedMode. If true, currently selected row is shown on top as drop down button. Used for example for language switch.
        /// </summary>
        public bool IsSelectedMode;
    }

    public class BulmaNavbarFilter
    {
        public Grid Grid;

        public string FieldName;

        public bool IsNavbarEnd;
    }

    public enum BulmaNavbarItemEnum
    {
        None = 0,

        Text = 1,

        Divider = 2,

        Filter = 3,

        Parent = 4
    }

    /// <summary>
    /// Dto for Angular bulma button or input html element.
    /// </summary>
    public class BulmaNavbarItem
    {
        [Serialize(SerializeEnum.Both)]
        public int Id;

        [Serialize(SerializeEnum.Client)]
        public BulmaNavbarItemEnum ItemEnum;

        [Serialize(SerializeEnum.Session)]
        public Grid Grid;

        [Serialize(SerializeEnum.Session)]
        public int RowStateId;

        [Serialize(SerializeEnum.Client)]
        public string TextHtml;

        [Serialize(SerializeEnum.Session)]
        public string FilterFieldName;

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

        public bool IsDivider;

        public bool IsNavbarEnd;

        public double Sort;

        internal int RowStateId;
    }
}
