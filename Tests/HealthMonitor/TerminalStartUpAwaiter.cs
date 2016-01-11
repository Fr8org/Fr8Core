using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace HealthMonitor
{
    public class TerminalStartUpAwaiter
    {
        private const int MaxHttpTimeout = 5000;
        private const int MaxAwaitPeriod = 180000;

        public List<string> AwaitStartUp()
        {
            var appSettingKeys = new List<string>();
            
            foreach (string key in ConfigurationManager.AppSettings.Keys)
            {
                appSettingKeys.Add(key);
            }

            var terminalUrlKeys = appSettingKeys
                .Where(x => x.StartsWith("terminal") && x.EndsWith("Url"))
                .ToList();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                var successful = new List<string>();

                foreach (var terminalUrlKey in terminalUrlKeys)
                {
                    var url = ConfigurationManager.AppSettings[terminalUrlKey];
                    
                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var responseTask = httpClient.GetAsync(url);
                            if (responseTask.Wait(MaxHttpTimeout))
                            {
                                using (var response = responseTask.Result)
                                {
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        successful.Add(terminalUrlKey);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                foreach (var terminalUrlKey in successful)
                {
                    terminalUrlKeys.Remove(terminalUrlKey);
                }

                if (stopwatch.ElapsedMilliseconds > MaxAwaitPeriod || terminalUrlKeys.Count == 0)
                {
                    break;
                }
            }

            return terminalUrlKeys;
        }
    }
}
