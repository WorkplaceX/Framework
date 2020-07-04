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
        /// Process bootstrap modal dialog window.
        /// </summary>
        public static void ProcessBootstrapModal(AppJson appJson)
        {
            appJson.IsBootstrapModal = false;
            BootstrapModal.DivModalBackdropRemove(appJson);
            bool isExist = false;
            foreach (var item in appJson.ComponentListAll().OfType<BootstrapModal>())
            {
                item.ButtonClose?.ComponentMoveLast();
                isExist = true;
            }
            if (isExist)
            {
                appJson.IsBootstrapModal = true;
                BootstrapModal.DivModalBackdropCreate(appJson);
            }
        }

        /// <summary>
        /// User clicked home button for example on navbar.
        /// </summary>
        public static Task ProcessHomeIsClickAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.HomeIsClick, out _, out ComponentJson _))
            {
                // User clicked home button
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// User clicked internal link or clicked backward, forward navigation history. Instead of GET and download Angular again a POST command is sent.
        /// </summary>
        public static async Task ProcessLinkPostAsync(AppJson appJson)
        {
            if (UtilSession.Request(appJson, RequestCommand.LinkPost, out CommandJson commandJson, out ComponentJson _))
            {
                await appJson.NavigateSessionInternalAsync(commandJson.LinkPostPath, commandJson.LinkPostPathIsAddHistory);
            }
        }

        /// <summary>
        /// Returns all button recursively.
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
            if (UtilSession.Request(appJson, RequestCommand.BootstrapNavbarButtonIsClick, out CommandJson commandJson, out BootstrapNavbar navbar))
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
        /// Remove non Div components from DivContainer.
        /// </summary>
        public static void DivContainerRender(AppJson appJson)
        {
            foreach (var divContainer in appJson.ComponentListAll().OfType<DivContainer>())
            {
                List<ComponentJson> listRemove = new List<ComponentJson>(); // Collect items to remove.
                foreach (var item in divContainer.List)
                {
                    if (!(item is Div)) // ComponentJson.Type is not evalueted on DivComponent children!
                    {
                        listRemove.Add(item);
                    }
                }
                foreach (var item in listRemove)
                {
                    item.ComponentRemove();
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
                    Row row = grid.RowList[rowState.RowId.Value - 1];
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
                        if (!(bootstrapNavbarGrid.IsSelectedMode && row == grid.RowSelected)) // For example for language: do not show selected language again under drop down button.
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
                                int itemId = (int)propertyInfoId.GetValue(grid.RowList[rowState.RowId.Value - 1]);
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
                bootstrapNavbar.ButtonList = new List<BootstrapNavbarButton>();
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
                        if (item.IsSelectedMode)
                        {
                            if (item.Grid.RowSelected != null)
                            {
                                string textHtml = (string)propertyInfoTextHtml.GetValue(item.Grid.RowSelected);
                                var args = new BootstrapNavbarButtonArgs { BootstrapNavbar = bootstrapNavbar, Grid = item.Grid, Row = item.Grid.RowSelected };
                                var result = new BootstrapNavbarButtonResult { TextHtml = textHtml };
                                bootstrapNavbar.ButtonTextHtml(args, result);
                                buttonId += 1;
                                var button = new BootstrapNavbarButton { Id = buttonId, Grid = item.Grid, RowStateId = item.Grid.RowSelectedRowStateId.Value, TextHtml = result.TextHtml };
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