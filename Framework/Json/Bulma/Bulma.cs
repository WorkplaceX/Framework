using Framework.DataAccessLayer;
using System.Collections.Generic;

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

        /// <summary>
        /// Add data grid to Navbar.
        /// </summary>
        /// <param name="grid">Data grid with Id, ParentId and TextHtml columns.</param>
        /// <param name="isSelectedMode">If true, currently selected row is shown on top as drop down button. Used for example for language switch.</param>
        public void GridAdd(Grid grid, bool isSelectedMode = false)
        {
            GridList.Add(new BulmaNavbarGrid { Grid = grid, IsSelectedMode = isSelectedMode });
        }

        internal List<BulmaNavbarItem> ItemList;

        private void RowMap(Row args, BulmaNavbarRowMapResult result, string propertyName)
        {
            object value = args.GetType().GetProperty(propertyName).GetValue(args);
            result.GetType().GetProperty(propertyName).SetValue(result, value);
        }

        protected virtual void RowMap(Row args, BulmaNavbarRowMapResult result)
        {
            // Default row mapper
            RowMap(args, result, nameof(result.Id));
            RowMap(args, result, nameof(result.ParentId));
            RowMap(args, result, nameof(result.TextHtml));
            RowMap(args, result, nameof(result.IsDivider));
            RowMap(args, result, nameof(result.FilterFieldName));
            RowMap(args, result, nameof(result.IsNavbarEnd));
            RowMap(args, result, nameof(result.Sort));
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

    /// <summary>
    /// Angular dto for bulma button or input html element.
    /// </summary>
    public class BulmaNavbarItem
    {
        public Grid Grid;

        public Row Row;

        public string TextHtml;

        public bool IsDivider;

        public string FilterFieldName;

        [Serialize(SerializeEnum.Session)]
        public List<BulmaNavbarItem> ItemList = new List<BulmaNavbarItem>();
    }

    public class BulmaNavbarRowMapResult
    {
        public bool IsHide { get; set; }

        public int Id { get; set; }

        public int ParentId { get; set; }

        public string TextHtml { get; set; }

        public bool IsDivider { get; set; }

        public string FilterFieldName { get; set; }

        public bool IsNavbarEnd { get; set; }

        public double Sort { get; set; }
    }
}
