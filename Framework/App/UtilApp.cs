namespace Framework.App
{
    using Framework.Server;
    using Framework.Json;
    using System.Threading.Tasks;
    using System.Linq;
    using Framework.Session;
    using System.Reflection;
    using Framework.DataAccessLayer;
    using System.Collections.Generic;
    using static Framework.Session.UtilSession;
    using System;

    internal static class UtilApp
    {
        /// <summary>
        /// Process button click.
        /// </summary>
        public static async Task ProcessButtonAsync()
        {
            var appJson = UtilServer.AppJson;
            if (appJson.RequestJson.Command == RequestCommand.ButtonIsClick)
            {
                Button button = (Button)appJson.RootComponentJsonList[appJson.RequestJson.ComponentId];
                await button.ComponentOwner<Page>().ButtonClickAsync(button);
            }
        }

        /// <summary>
        /// Process bootstrap modal dialog window.
        /// </summary>
        public static void ProcessBootstrapModal()
        {
            var app = UtilServer.AppInternal;
            app.AppJson.IsBootstrapModal = false;
            BootstrapModal.DivModalBackdropRemove(app.AppJson);
            bool isExist = false;
            foreach (var item in app.AppJson.ComponentListAll().OfType<BootstrapModal>())
            {
                item.ButtonClose()?.ComponentMoveLast();
                isExist = true;
            }
            if (isExist)
            {
                app.AppJson.IsBootstrapModal = true;
                BootstrapModal.DivModalBackdropCreate(app.AppJson);
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
        public static async Task ProcessBootstrapNavbarAsync()
        {
            var app = UtilServer.AppInternal;
            foreach (BootstrapNavbar navbar in app.AppJson.ComponentListAll().OfType<BootstrapNavbar>())
            {
                if (navbar.ButtonList != null)
                {
                    var buttonList = new List<BootstrapNavbarButton>();
                    ProcessBootstrapNavbarButtonListAll(navbar.ButtonList, buttonList);
                    foreach (BootstrapNavbarButton button in buttonList)
                    {
                        if (button.IsClick)
                        {
                            button.IsClick = false;
                            if (button.GridIndex != null)
                            {
                                GridItem gridItem = UtilSession.GridItemList().Where(item => item.GridIndex == button.GridIndex).First();

                                // Set IsSelect
                                // See also method ProcessGridRowIsClick();
                                foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                                {
                                    if (gridRowItem.GridRowSession != null) // Outgoing grid might have less rows
                                    {
                                        gridRowItem.GridRowSession.IsSelect = false;
                                    }
                                }
                                foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                                {
                                    if (gridRowItem.GridRowSession != null && gridRowItem.RowIndex == button.RowIndex)
                                    {
                                        gridRowItem.GridRowSession.IsSelect = true;
                                        break;
                                    }
                                }
                                if (gridItem.Grid == null)
                                {
                                    throw new Exception("Grid has been removed! Use property Grid.IsHide instead.");
                                }
                                await gridItem.Grid.ComponentOwner<Page>().GridRowSelectedAsync(gridItem.Grid);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove non Div components from DivContainer.
        /// </summary>
        public static void DivContainerRender()
        {
            var app = UtilServer.AppInternal;
            foreach (var divContainer in app.AppJson.ComponentListAll().OfType<DivContainer>())
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
        private static void BootstrapNavbarRender(BootstrapNavbarButton buttonParent, ref List<BootstrapNavbarButton> buttonList, GridSession gridSession, int? findParentId, PropertyInfo propertyInfoId, PropertyInfo propertyInfoParentId, PropertyInfo propertyInfoTextHtml)
        {
            int gridIndex = UtilSession.GridSessionToIndex(gridSession);
            for (int rowIndex = 0; rowIndex < gridSession.GridRowSessionList.Count; rowIndex++)
            {
                GridRowSession gridRowSession = gridSession.GridRowSessionList[rowIndex];
                if (gridRowSession.RowEnum == GridRowEnum.Index)
                {
                    string itemText = (string)propertyInfoTextHtml.GetValue(gridRowSession.Row);
                    int? itemParentId = (int?)propertyInfoParentId?.GetValue(gridRowSession.Row); // Null if row does not have field "ParentId".
                    bool isActive = gridRowSession.IsSelect;
                    if (itemParentId == findParentId)
                    {
                        if (buttonParent != null)
                        {
                            buttonParent.IsDropDown = true; // Has children.
                        }
                        BootstrapNavbarButton button = new BootstrapNavbarButton(null) { GridIndex = gridIndex, RowIndex = rowIndex, TextHtml = itemText, IsActive = isActive }; // TODO
                        if (buttonList == null)
                        {
                            buttonList = new List<BootstrapNavbarButton>();
                        }
                        buttonList.Add(button);
                        if (propertyInfoParentId != null) // Hierarchical navigation
                        {
                            int itemId = (int)propertyInfoId.GetValue(gridRowSession.Row);
                            BootstrapNavbarRender(button, ref button.ButtonList, gridSession, itemId, propertyInfoId, propertyInfoParentId, propertyInfoTextHtml);
                        }
                    }
                }
            }
        }

        public static void BootstrapNavbarRender()
        {
            var app = UtilServer.AppInternal;
            foreach (BootstrapNavbar navbar in app.AppJson.ComponentListAll().OfType<BootstrapNavbar>())
            {
                navbar.ButtonList = new List<BootstrapNavbarButton>();
                foreach (int gridIndex in navbar.GridIndexList)
                {
                    GridSession gridSession = UtilSession.GridSessionFromIndex(gridIndex);

                    var propertyInfoList = UtilDalType.TypeRowToPropertyInfoList(gridSession.TypeRow);

                    PropertyInfo propertyInfoId = propertyInfoList.Where(item => item.Name == "Id" && item.PropertyType == typeof(int)).SingleOrDefault();
                    PropertyInfo propertyInfoParentId = propertyInfoList.Where(item => item.Name == "ParentId" && item.PropertyType == typeof(int?)).SingleOrDefault();
                    PropertyInfo propertyInfoTextHtml = propertyInfoList.Where(item => item.Name == "TextHtml" && item.PropertyType == typeof(string)).SingleOrDefault();

                    if (propertyInfoParentId != null)
                    {
                        UtilFramework.Assert(propertyInfoId != null, "Row needs a column called 'Id'!");
                    }
                    UtilFramework.Assert(propertyInfoTextHtml != null, string.Format("Row needs a column called 'TextHtml' ({0})!", UtilDalType.TypeRowToTableNameCSharp(gridSession.TypeRow)));

                    BootstrapNavbarRender(null, ref navbar.ButtonList, gridSession, findParentId: null, propertyInfoId, propertyInfoParentId, propertyInfoTextHtml);
                }
            }
        }
    }
}
