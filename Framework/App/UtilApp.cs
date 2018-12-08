namespace Framework.App
{
    using Framework.Server;
    using Framework.Json;
    using System.Threading.Tasks;
    using System.Linq;
    using Framework.Session;
    using System.Reflection;
    using Framework.Dal;
    using System.Collections.Generic;
    using static Framework.Session.UtilSession;

    internal static class UtilApp
    {
        public static async Task ProcessButtonAsync()
        {
            var app = UtilServer.AppInternal;
            foreach (Button button in app.AppJson.ListAll().OfType<Button>().Where(item => item.IsClick))
            {
                await button.Owner<Page>().ButtonClickAsync(button);
                button.IsClick = false;
            }
        }

        /// <summary>
        /// Divert navbar click to data grid row click.
        /// </summary>
        public static void ProcessBootstrapNavbar()
        {
            var app = UtilServer.AppInternal;
            foreach (BootstrapNavbar navbar in app.AppJson.ListAll().OfType<BootstrapNavbar>())
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
                                GridItem gridItem = UtilSession.GridItemList().Where(item => item.GridIndex == navbar.GridIndex).SingleOrDefault();
                                if (gridItem != null)
                                {
                                    foreach (GridRowItem gridRowItem in gridItem.GridRowList)
                                    {
                                        if (gridRowItem.RowIndex == button.RowIndex)
                                        {
                                            gridRowItem.GridRow.IsClick = true; // Divert navbar click to data grid row click.
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void BootstrapNavbarRender()
        {
            var app = UtilServer.AppInternal;
            foreach (BootstrapNavbar navbar in app.AppJson.ListAll().OfType<BootstrapNavbar>())
            {
                navbar.ButtonList = new List<BootstrapNavbarButton>();
                if (navbar.GridIndex != null)
                {
                    GridSession gridSession = UtilSession.GridSessionFromIndex(navbar.GridIndex.Value);

                    PropertyInfo propertyInfo = UtilDalType.TypeRowToPropertyInfoList(gridSession.TypeRow).Where(item => item.Name == "Text" && item.PropertyType == typeof(string)).SingleOrDefault();
                    if (propertyInfo != null)
                    {
                        for (int rowIndex = 0; rowIndex < gridSession.GridRowSessionList.Count; rowIndex++)
                        {
                            GridRowSession gridRowSession = gridSession.GridRowSessionList[rowIndex];
                            if (gridRowSession.RowEnum == GridRowEnum.Index)
                            {
                                string text = (string)propertyInfo.GetValue(gridRowSession.Row);
                                bool isActive = gridRowSession.IsSelect;
                                BootstrapNavbarButton button = new BootstrapNavbarButton() { RowIndex = rowIndex, TextHtml = text, IsActive = isActive };
                                navbar.ButtonList.Add(button);
                            }
                        }
                    }
                }
            }
        }
    }
}
