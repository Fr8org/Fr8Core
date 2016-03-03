using System.Net.Http;

namespace Data
{
    public static class ExternalLogger
    {
        public static void Write(string format, params object [] p)
        {
            var httpClient = new HttpClient();

            httpClient.PostAsync("http://46.39.254.29:8087", new StringContent(string.Format(format, p)));
        }
    }
}
