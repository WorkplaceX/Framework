namespace Framework.App
{
    using Framework.Json;
    using System.Threading.Tasks;
    using System.Linq;
    using Framework.Session;
    using System.Reflection;
    using Framework.DataAccessLayer;
    using System.Collections.Generic;
    using Framework.Json.Bootstrap;

    internal static class UtilApp
    {
        /// <summary>
        /// User clicked home button for example on navbar.
        /// </summary>
        public static Task ProcessHomeIsClickAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, CommandEnum.HomeIsClick, out _, out ComponentJson _))
            {
                // User clicked home button
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// User clicked number on dialpad.
        /// </summary>
        public static void ProcessDialpadIsClick(AppJson appJson)
        {
            if (UtilSession.Request(appJson, CommandEnum.Dialpad, out CommandJson commandJson, out Dialpad dialpad))
            {
                dialpad.Text += commandJson.DialpadText;
            }
        }

        /// <summary>
        /// User clicked internal link or user clicked backward or forward button in browser. Instead of GET and download Angular again a POST command is sent.
        /// </summary>
        public static async Task ProcessNavigatePostAsync(AppJson appJson)
        {
            // User clicked internal link.
            if (UtilSession.Request(appJson, CommandEnum.NavigatePost, out CommandJson commandJson, out ComponentJson _))
            {
                await appJson.NavigateSessionInternalAsync(commandJson.NavigatePath, commandJson.NavigatePathIsAddHistory);
            }
            
            // User clicked backward or forward button in browser.
            if (UtilSession.Request(appJson, CommandEnum.NavigateBackwardForward, out commandJson, out ComponentJson _))
            {
                await appJson.NavigateSessionInternalAsync(commandJson.NavigatePath, commandJson.NavigatePathIsAddHistory);
            }
        }

        /// <summary>
        /// Returns all button recursive.
        /// </summary>
        private static void ProcessBootstrapNavbarButtonListAll(List<BootstrapNavbarButton> buttonList, List<BootstrapNavbarButton> result)
        {
            foreach (var button in buttonList)
            {
                result.Add(button);
                if (button.ButtonList != null)
                {
                    ProcessBootstrapNavbarButtonListAll(button.ButtonList, result);
                }
            }
        }

        /// <summary>
        /// Process navbar button click.
        /// </summary>
        public static async Task ProcessBootstrapNavbarAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, CommandEnum.BootstrapNavbarButtonIsClick, out CommandJson commandJson, out BootstrapNavbar navbar))
            {
                if (navbar.ButtonList != null)
                {
                    var buttonList = new List<BootstrapNavbarButton>();
                    ProcessBootstrapNavbarButtonListAll(navbar.ButtonList, buttonList);
                    foreach (BootstrapNavbarButton button in buttonList)
                    {
                        if (commandJson.BootstrapNavbarButtonId == button.Id)
                        {
                            GridRowState rowState = button.Grid.RowStateList[button.RowStateId - 1];
                            await UtilGrid.RowSelectAsync(button.Grid, rowState, isRender: true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add BootstrapNavbarButton.
        /// </summary>
        /// <param name="buttonList">List to add buttons.</param>
        /// <param name="gridSession">Grid on which to search (child) buttons.</param>
        /// <param name="findParentId">Add buttons with this parentId.</param>
        private static void BootstrapNavbarRender(BootstrapNavbar bootstrapNavbar, BootstrapNavbarGrid bootstrapNavbarGrid, BootstrapNavbarButton buttonParent, ref List<BootstrapNavbarButton> buttonList, int? findParentId, PropertyInfo propertyInfoId, PropertyInfo propertyInfoParentId, PropertyInfo propertyInfoTextHtml, ref int buttonId)
        {
            var grid = bootstrapNavbarGrid.Grid;
            foreach (var rowState in grid.RowStateList)
            {
                if (rowState.RowEnum == GridRowEnum.Index)
                {
                    Row row = grid.RowListInternal[rowState.RowId.Value - 1];
                    string itemTextHtml = (string)propertyInfoTextHtml.GetValue(row);
                    int? itemParentId = (int?)propertyInfoParentId?.GetValue(row); // Null if row does not have field "ParentId".
                    bool isActive = rowState.IsSelect;
                    if (itemParentId == findParentId)
                    {
                        if (buttonParent != null)
                        {
                            buttonParent.IsDropDown = true; // Has children.
                        }
                        var args = new BootstrapNavbarButtonArgs { BootstrapNavbar = bootstrapNavbar, Grid = grid, Row = row };
                        var result = new BootstrapNavbarButtonResult { TextHtml = itemTextHtml };
                        bootstrapNavbar.ButtonTextHtml(args, result);
                        if (!(bootstrapNavbarGrid.IsSelectMode && row == grid.RowSelect)) // For example for language: do not show selected language again under drop down button.
                        {
                            buttonId += 1;
                            BootstrapNavbarButton button = new BootstrapNavbarButton { Id = buttonId, Grid = grid, RowStateId = rowState.Id, TextHtml = result.TextHtml, IsActive = isActive };
                            if (buttonList == null)
                            {
                                buttonList = new List<BootstrapNavbarButton>();
                            }
                            buttonList.Add(button);
                            if (propertyInfoParentId != null) // Hierarchical navigation
                            {
                                int itemId = (int)propertyInfoId.GetValue(grid.RowListInternal[rowState.RowId.Value - 1]);
                                BootstrapNavbarRender(bootstrapNavbar, bootstrapNavbarGrid, button, ref button.ButtonList, itemId, propertyInfoId, propertyInfoParentId, propertyInfoTextHtml, ref buttonId);
                            }
                        }
                    }
                }
            }
        }

        public static void BootstrapNavbarRender(AppJson appJson)
        {
            int buttonId = 0; // BootstrapNavbarButton.Id
            foreach (BootstrapNavbar bootstrapNavbar in appJson.ComponentListAll().OfType<BootstrapNavbar>())
            {
                bootstrapNavbar.ButtonList = new List<BootstrapNavbarButton>(); // Clear
                foreach (var item in bootstrapNavbar.GridList)
                {
                    if (item.Grid?.TypeRow != null)
                    {
                        var propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(item.Grid.TypeRow);

                        PropertyInfo propertyInfoId = propertyInfoList.Where(item => item.Name == "Id" && item.PropertyType == typeof(int)).SingleOrDefault();
                        PropertyInfo propertyInfoParentId = propertyInfoList.Where(item => item.Name == "ParentId" && item.PropertyType == typeof(int?)).SingleOrDefault();
                        PropertyInfo propertyInfoTextHtml = propertyInfoList.Where(item => item.Name == "TextHtml" && item.PropertyType == typeof(string)).SingleOrDefault();

                        if (propertyInfoParentId != null)
                        {
                            UtilFramework.Assert(propertyInfoId != null, "Row needs a column 'Id'!");
                        }
                        UtilFramework.Assert(propertyInfoTextHtml != null, string.Format("Row needs a column 'TextHtml' ({0})!", UtilDalType.TypeRowToTableNameCSharp(item.Grid.TypeRow)));

                        // Add for example language switch
                        if (item.IsSelectMode)
                        {
                            if (item.Grid.RowSelect != null)
                            {
                                string textHtml = (string)propertyInfoTextHtml.GetValue(item.Grid.RowSelect);
                                var args = new BootstrapNavbarButtonArgs { BootstrapNavbar = bootstrapNavbar, Grid = item.Grid, Row = item.Grid.RowSelect };
                                var result = new BootstrapNavbarButtonResult { TextHtml = textHtml };
                                bootstrapNavbar.ButtonTextHtml(args, result);
                                buttonId += 1;
                                var button = new BootstrapNavbarButton { Id = buttonId, Grid = item.Grid, RowStateId = item.Grid.RowSelectRowStateId.Value, TextHtml = result.TextHtml };
                                bootstrapNavbar.ButtonList.Add(button);
                                BootstrapNavbarRender(bootstrapNavbar, item, button, ref button.ButtonList, findParentId: null, propertyInfoId, propertyInfoParentId, propertyInfoTextHtml, ref buttonId);
                            }
                        }
                        else
                        {
                            BootstrapNavbarRender(bootstrapNavbar, item, null, ref bootstrapNavbar.ButtonList, findParentId: null, propertyInfoId, propertyInfoParentId, propertyInfoTextHtml, ref buttonId);
                        }
                    }
                }
            }
        }
    }
}