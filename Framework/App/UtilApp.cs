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
            var app = UtilServer.AppInternal;
            foreach (Button button in app.AppJson.ComponentListAll().OfType<Button>().Where(item => item.IsClick))
            {
                await button.ComponentOwner<Page>().ButtonClickAsync(button);
                button.IsClick = false;
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
        /// Process navbar button click.
        /// </summary>
        public static async Task ProcessBootstrapNavbarAsync()
        {
            var app = UtilServer.AppInternal;
            foreach (BootstrapNavbar navbar in app.AppJson.ComponentListAll().OfType<BootstrapNavbar>())
            {
                if (navbar.ButtonList != null)
                {
                    foreach (BootstrapNavbarButton button in navbar.ButtonList)
                    {
                        if (button.IsClick)
                        {
                            button.IsClick = false;
                            if (navbar.GridIndex != null)
                            {
                                GridItem gridItem = UtilSession.GridItemList().Where(item => item.GridIndex == navbar.GridIndex).First();

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
        /// Set Div.IsListDiv flag.
        /// </summary>
        public static void BootstrapRowRender()
        {
            var app = UtilServer.AppInternal;
            foreach (var bootstrapRow in app.AppJson.ComponentListAll().OfType<BootstrapRow>())
            {
                bootstrapRow.CssClassAdd("row");
                List<ComponentJson> listRemove = new List<ComponentJson>();
                foreach (var item in bootstrapRow.List)
                {
                    if (item.GetType() != typeof(Div)) // ComponentJson.Type is not evalueted on BootstrapRow children!
                    {
                        listRemove.Add(item);
                    }
                }
                foreach (var item in listRemove)
                {
                    bootstrapRow.List.Remove(item);
                }
            }
        }

        /// <summary>
        /// Add BootstrapNavbarButton.
        /// </summary>
        /// <param name="buttonList">List to add buttons.</param>
        /// <param name="gridSession">Grid on which to search (child) buttons.</param>
        /// <param name="findParentId">Add buttons with this parentId.</param>
        private static void BootstrapNavbarRender(ref List<BootstrapNavbarButton> buttonList, GridSession gridSession, int? findParentId, PropertyInfo propertyInfoId, PropertyInfo propertyInfoParentId, PropertyInfo propertyInfoText)
        {
            for (int rowIndex = 0; rowIndex < gridSession.GridRowSessionList.Count; rowIndex++)
            {
                GridRowSession gridRowSession = gridSession.GridRowSessionList[rowIndex];
                if (gridRowSession.RowEnum == GridRowEnum.Index)
                {
                    int itemId = (int)propertyInfoId.GetValue(gridRowSession.Row);
                    int? itemParentId = (int?)propertyInfoParentId.GetValue(gridRowSession.Row);
                    string itemText = (string)propertyInfoText.GetValue(gridRowSession.Row);
                    bool isActive = gridRowSession.IsSelect;
                    if (itemParentId == findParentId)
                    {
                        BootstrapNavbarButton button = new BootstrapNavbarButton() { RowIndex = rowIndex, TextHtml = itemText, IsActive = isActive };
                        if (buttonList == null)
                        {
                            buttonList = new List<BootstrapNavbarButton>();
                        }
                        buttonList.Add(button);
                        BootstrapNavbarRender(ref button.ButtonList, gridSession, itemId, propertyInfoId, propertyInfoParentId, propertyInfoText);
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
                if (navbar.GridIndex != null)
                {
                    GridSession gridSession = UtilSession.GridSessionFromIndex(navbar.GridIndex.Value);

                    PropertyInfo propertyInfoId = UtilDalType.TypeRowToPropertyInfoList(gridSession.TypeRow).Where(item => item.Name == "Id" && item.PropertyType == typeof(int)).SingleOrDefault();
                    PropertyInfo propertyInfoParentId = UtilDalType.TypeRowToPropertyInfoList(gridSession.TypeRow).Where(item => item.Name == "ParentId" && item.PropertyType == typeof(int?)).SingleOrDefault();
                    PropertyInfo propertyInfoText = UtilDalType.TypeRowToPropertyInfoList(gridSession.TypeRow).Where(item => item.Name == "Text" && item.PropertyType == typeof(string)).SingleOrDefault();

                    BootstrapNavbarRender(ref navbar.ButtonList, gridSession, findParentId: null, propertyInfoId, propertyInfoParentId, propertyInfoText);
                }
            }
        }
    }
}
