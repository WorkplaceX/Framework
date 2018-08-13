namespace Framework.App
{
    using Framework.Server;
    using Framework.Json;
    using System.Threading.Tasks;
    using System.Linq;

    internal static class UtilApp
    {
        public static async Task ProcessAsync()
        {
            var app = UtilServer.App;
            var pageList = app.AppJson.ListAll().OfType<Page>().ToList();
            foreach (Button button in app.AppJson.ListAll().OfType<Button>().Where(item => item.IsClick))
            {
                await app.ButtonClickAsync(button);
                foreach (Page page in pageList)
                {
                    await page.ButtonClickAsync(button);
                }
                button.IsClick = false;
            }
        }
    }
}
