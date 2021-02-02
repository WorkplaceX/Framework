using Framework.Config;

namespace Framework.Json.Bing
{
    /// <summary>
    /// BingMap element. See also: https://www.bingmapsportal.com/ and https://www.bing.com/api/maps/sdk/mapcontrol/isdk/
    /// For key see also: file ConfigCli.json BingMapKey
    /// </summary>
    public sealed class BingMap : ComponentJson
    {
        public BingMap(ComponentJson owner)
            : base(owner, nameof(BingMap))
        {
            Key = UtilFramework.StringNull(ConfigServer.Load().BingMapKey);
        }

        public string Long;

        public string Lat;

        public string Key;
    }
}
