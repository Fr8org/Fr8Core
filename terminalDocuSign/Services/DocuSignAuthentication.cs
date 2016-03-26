using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StructureMap;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Utilities.Configuration.Azure;

namespace terminalDocuSign.Services
{
    //public class DocuSignAuthentication
    //{
    //    public async Task<string> ObtainOAuthToken(string username, string password, string baseUrl)
    //    {
    //        var client = ObjectFactory.GetInstance<IRestfulServiceClient>();
    //        try
    //        {
    //            var response = await client
    //            .PostAsync(new Uri(new Uri(baseUrl), "oauth2/token"),
    //                (HttpContent)new FormUrlEncodedContent(new[]
    //                {
    //                    new KeyValuePair<string, string>("grant_type", "password"),
    //                    new KeyValuePair<string, string>("client_id", CloudConfigurationManager.GetSetting("DocuSignIntegratorKey")),
    //                    new KeyValuePair<string, string>("username", username),
    //                    new KeyValuePair<string, string>("password", password),
    //                    new KeyValuePair<string, string>("scope", "api"),
    //                }));

    //            var responseObject = JsonConvert.DeserializeAnonymousType(response, new { access_token = "" });

    //            return responseObject.access_token;
    //        }
    //        catch (Exception ex)
    //        {
    //            throw;
    //        }
    //    }

    //}
}