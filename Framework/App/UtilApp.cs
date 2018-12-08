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

        public static void BootstrapNavbarRender()
        {
            var app = UtilServer.AppInternal;
            foreach (BootstrapNavbar bootstrapNavbar in app.AppJson.ListAll().OfType<BootstrapNavbar>())
            {
                bootstrapNavbar.ButtonList = new List<BootstrapNavbarButton>();
                if (bootstrapNavbar.GridIndex != null)
                {
                    GridSession gridSession = UtilSession.GridSessionFromIndex(bootstrapNavbar.GridIndex.Value);

                    PropertyInfo propertyInfo = UtilDalType.TypeRowToPropertyInfoList(gridSession.TypeRow).Where(item => item.Name == "Text" && item.PropertyType == typeof(string)).SingleOrDefault();
                    if (propertyInfo != null)
                    {
                        foreach (GridRowSession gridRowSession in gridSession.GridRowSessionList)
                        {
                            if (gridRowSession.RowEnum == GridRowEnum.Index)
                            {
                                string text = (string)propertyInfo.GetValue(gridRowSession.Row);
                                bool isActive = gridRowSession.IsSelect;
                                BootstrapNavbarButton button = new BootstrapNavbarButton() { TextHtml = text, IsActive = isActive };
                                bootstrapNavbar.ButtonList.Add(button);
                            }
                        }
                    }
                }
            }
        }
    }
}
