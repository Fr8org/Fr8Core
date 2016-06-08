namespace Fr8.Testing.Integration
{
    public static class Utilities
    {
        public static string NormalizeSchema(string endpoint)
        {
            if (endpoint.StartsWith("http://"))
            {
                return endpoint;
            }
            else if (endpoint.StartsWith("https://"))
            {
                return endpoint.Replace("https://", "http://");
            }
            else
            {
                return "http://" + endpoint;
            }
        }
    }
}
