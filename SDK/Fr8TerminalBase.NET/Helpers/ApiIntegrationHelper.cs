using System;
using System.Threading.Tasks;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;

namespace Fr8.TerminalBase.Helpers
{
    public static class ApiIntegrationHelper
    {
        public static async Task ApiCall(this IOAuthApiIntegration integration, Func<AuthorizationToken, Task> apiCall, AuthorizationToken auth)
        {
            if (integration == null)
            {
                throw new ArgumentNullException(nameof(integration));
            }
            await integration.ApiCall(async x =>
                                            {
                                                await apiCall(x);
                                                return 0;
                                            }, auth);
        }
    }
}