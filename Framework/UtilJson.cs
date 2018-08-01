namespace Framework
{
    using Newtonsoft.Json;

    internal static class UtilJson
    {
        internal static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}
