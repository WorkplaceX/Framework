namespace Framework.App
{
    using Framework.Json;
    using System.Threading.Tasks;
    using System.Linq;
    using Framework.Session;
    using System.Reflection;
    using Framework.DataAccessLayer;
    using System.Collections.Generic;

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
            var request = appJson.RequestJson;

            if (UtilSession.Request(appJson, RequestCommand.BootstrapNavbarButtonIsClick, out RequestJson requestJson, out BootstrapNavbar navbar))
            {
                if (navbar.ButtonList != null)
                {
                    var buttonList = new List<BootstrapNavbarButton>();
                    ProcessBootstrapNavbarButtonListAll(navbar.ButtonList, buttonList);
                    foreach (BootstrapNavbarButton button in buttonList)
                    {
                        if (request.BootstrapNavbarButtonId == button.Id)
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
                    if (item.GetType() != typeof(Div)) // ComponentJson.Type is not evalueted on DivComponent children!
                    {
                        listRemove.Add(item);
                    }
                }
                foreach (var item in listRemove)
                {
                    divContainer.List.Remove(item);
                }
            }
        }

        /// <summary>
        /// Add BootstrapNavbarButton.
        /// </summary>
        /// <param name="buttonList">List to add buttons.</param>
        /// <param name="gridSession">Grid on which to search (child) buttons.</param>
        /// <param name="findParentId">Add buttons with this parentId.</param>
        private static void BootstrapNavbarRender(BootstrapNavbarButton buttonParent, ref List<BootstrapNavbarButton> buttonList, Grid grid, int? findParentId, PropertyInfo propertyInfoId, PropertyInfo propertyInfoParentId, PropertyInfo propertyInfoTextHtml, ref int buttonId)
        {
            foreach (var rowState in grid.RowStateList)
            {
                if (rowState.RowEnum == GridRowEnum.Index)
                {
                    string itemText = (string)propertyInfoTextHtml.GetValue(grid.RowList[rowState.RowId.Value - 1]);
                    int? itemParentId = (int?)propertyInfoParentId?.GetValue(grid.RowList[rowState.RowId.Value - 1]); // Null if row does not have field "ParentId".
                    bool isActive = rowState.IsSelect;
                    if (itemParentId == findParentId)
                    {
                        if (buttonParent != null)
                        {
                            buttonParent.IsDropDown = true; // Has children.
                        }
                        buttonId += 1;
                        BootstrapNavbarButton button = new BootstrapNavbarButton { Id = buttonId, Grid = grid, RowStateId = rowState.Id, TextHtml = itemText, IsActive = isActive };
                        if (buttonList == null)
                        {
                            buttonList = new List<BootstrapNavbarButton>();
                        }
                        buttonList.Add(button);
                        if (propertyInfoParentId != null) // Hierarchical navigation
                        {
                            int itemId = (int)propertyInfoId.GetValue(grid.RowList[rowState.RowId.Value - 1]);
                            BootstrapNavbarRender(button, ref button.ButtonList, grid, itemId, propertyInfoId, propertyInfoParentId, propertyInfoTextHtml, ref buttonId);
                        }
                    }
                }
            }
        }

        public static void BootstrapNavbarRender(AppJson appJson)
        {
            int buttonId = 0; // BootstrapNavbarButton.Id
            foreach (BootstrapNavbar navbar in appJson.ComponentListAll().OfType<BootstrapNavbar>())
            {
                navbar.ButtonList = new List<BootstrapNavbarButton>();
                foreach (var item in navbar.GridList)
                {
                    var propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(item.Grid.TypeRow);

                    PropertyInfo propertyInfoId = propertyInfoList.Where(item => item.Name == "Id" && item.PropertyType == typeof(int)).SingleOrDefault();
                    PropertyInfo propertyInfoParentId = propertyInfoList.Where(item => item.Name == "ParentId" && item.PropertyType == typeof(int?)).SingleOrDefault();
                    PropertyInfo propertyInfoTextHtml = propertyInfoList.Where(item => item.Name == "TextHtml" && item.PropertyType == typeof(string)).SingleOrDefault();

                    if (propertyInfoParentId != null)
                    {
                        UtilFramework.Assert(propertyInfoId != null, "Row needs a column called 'Id'!");
                    }
                    UtilFramework.Assert(propertyInfoTextHtml != null, string.Format("Row needs a column called 'TextHtml' ({0})!", UtilDalType.TypeRowToTableNameCSharp(item.Grid.TypeRow)));

                    BootstrapNavbarRender(null, ref navbar.ButtonList, item.Grid, findParentId: null, propertyInfoId, propertyInfoParentId, propertyInfoTextHtml, ref buttonId);
                }
            }
        }
    }
}
