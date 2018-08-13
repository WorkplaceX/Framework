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
            foreach (Button button in app.AppJson.ListAll().OfType<Button>().Where(item => item.IsClick))
            {
                await button.Owner<Page>().ButtonClickAsync(button);
                button.IsClick = false;
            }
        }
    }
}
