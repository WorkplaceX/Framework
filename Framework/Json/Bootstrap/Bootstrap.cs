namespace Framework.Json.Bootstrap
{
    using Framework.DataAccessLayer;
    using System.Collections.Generic;

    /// <summary>
    /// See also: https://getbootstrap.com/docs/4.1/components/navbar/
    /// Change background color with style "background-color: red !important".
    /// </summary>
    public sealed class BootstrapNavbar : ComponentJson
    {
        public BootstrapNavbar(ComponentJson owner)
            : base(owner)
        {

        }

        public string BrandTextHtml;

        /// <summary>
        /// Gets or sets GridIndexList. Data grid row should have a field "Text" and "ParentId" for hierarchical navigation.
        /// </summary>
        public List<int> GridIndexList = new List<int>(); // Empty list is removed by json serializer.

        [Serialize(SerializeEnum.Session)]
        internal List<BootstrapNavbarGrid> GridList = new List<BootstrapNavbarGrid>();

        /// <summary>
        /// Add data grid to Navbar.
        /// </summary>
        /// <param name="grid">Data grid with Id, ParentId and TextHtml columns.</param>
        /// <param name="isSelectedMode">If true, currently selected row is shown on top as drop down button. Used for example for language switch.</param>
        public void GridAdd(Grid grid, bool isSelectedMode = false)
        {
            GridList.Add(new BootstrapNavbarGrid { Grid = grid, IsSelectedMode = isSelectedMode });
        }

        internal List<BootstrapNavbarButton> ButtonList;

        /*
        <nav class="navbar-dark bg-primary sticky-top">
        <ul style="display: flex; flex-wrap: wrap"> // Wrapp if too many li.
        <a style="display:inline-block;">
        <i class="fas fa-spinner fa-spin text-white"></i>         
        */
    }

    public class BootstrapNavbarButtonArgs
    {
        public BootstrapNavbar BootstrapNavbar;

        public Grid Grid;

        public Row Row;
    }

    public class BootstrapNavbarButtonResult
    {
        public string TextHtml;
    }

    internal sealed class BootstrapNavbarGrid
    {
        public Grid Grid;

        /// <summary>
        /// Gets or sets IsSelectedMode. If true, currently selected row is shown on top as drop down button. Used for example for language switch.
        /// </summary>
        public bool IsSelectedMode;
    }

    internal sealed class BootstrapNavbarButton
    {
        public int Id;

        /// <summary>
        /// Gets or sets Grid. For example navigation and language buttons can be shown in the Navbar.
        /// </summary>
        public Grid Grid;

        /// <summary>
        /// Gets or sets RowStateId. Can be null for example for drop down button for language. See also <see cref="BootstrapNavbarGrid.IsSelectedMode"/>
        /// </summary>
        public int RowStateId;

        public string TextHtml;

        public bool IsActive;

        /// <summary>
        /// Gets or sets IsDropDown. True, if button has level 2 navigation.
        /// </summary>
        public bool IsDropDown;

        public List<BootstrapNavbarButton> ButtonList;
    }
}
