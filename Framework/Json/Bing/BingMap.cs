namespace Framework.Json.Bing
{
    /// <summary>
    /// BingMap element. See also: https://www.bingmapsportal.com/ and https://www.bing.com/api/maps/sdk/mapcontrol/isdk/
    /// </summary>
    public sealed class BingMap : ComponentJson
    {
        public BingMap(ComponentJson owner)
            : base(owner)
        {

        }

        public string Long;

        public string Lat;

        public string Key;
    }
}
