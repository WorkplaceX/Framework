using Framework.DataAccessLayer;
using System.Collections.Generic;

namespace Framework.Json.Bulma
{
    /// <summary>
    /// See also: https://bulma.io/documentation/components/navbar/
    /// </summary>
    public class BulmaNavbar : ComponentJson
    {
        internal BulmaNavbar(ComponentJson owner)
            : base(owner, nameof(BulmaNavbar))
        {

        }

        public BulmaNavbar(ComponentJson owner, BulmaNavbarAdapter bulmaNavbarAdapter) 
            : this(owner)
        {
            BulmaNavbarAdapter = bulmaNavbarAdapter;
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

        [Serialize(SerializeEnum.Session)]
        public BulmaNavbarAdapter BulmaNavbarAdapter;
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

    /// <summary>
    /// Map row fields to known bulma fields.
    /// </summary>
    public class BulmaNavbarAdapter : ComponentJson
    {
        public BulmaNavbarAdapter(ComponentJson owner) 
            : base(owner, null)
        {

        }

        public virtual void Read(Row args, BulmaNavbarAdapterResult result)
        {

        }
    }

    public class BulmaNavbarAdapterResult
    {
        public bool IsHide { get; set; }

        public int Id { get; set; }

        public int ParentId { get; set; }

        public string TextHtml { get; set; }

        public bool IsDivider { get; set; }

        public string FilterFieldName { get; set; }

        public bool IsNavbarEnd { get; set; }
    }
}
