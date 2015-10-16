using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using PluginBase.BaseClasses;
using pluginDocuSign.DataTransferObjects;
using fr8.Microsoft.Azure;

namespace pluginDocuSign.Controllers
{    
    [RoutePrefix("actions")]
    public class ActionController : ApiController
    {
        private const string curPlugin = "pluginDocuSign";
        private BasePluginController _basePluginController = new BasePluginController();


        [HttpPost]
        [Route("configure")]
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await (Task<ActionDTO>) _basePluginController
                .HandleDockyardRequest(curPlugin, "Configure", curActionDTO);
        }

        [HttpPost]
        [Route("activate")]
        public ActionDTO Activate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Activate", curActionDTO);
        }

        [HttpPost]
        [Route("deactivate")]
        public ActionDTO Deactivate(ActionDTO curActionDTO)
        {
            return (ActionDTO)_basePluginController.HandleDockyardRequest(curPlugin, "Deactivate", curActionDTO);
        }

        [HttpPost]
        [Route("authenticate_internal")]
        public async Task<AuthTokenDTO> Authenticate(CredentialsDTO curCredentials)
        {
            // Auth sequence according to https://www.docusign.com/p/RESTAPIGuide/RESTAPIGuide.htm#OAuth2/OAuth2%20Token%20Request.htm
            var oauthToken = await ObtainOAuthToken(curCredentials, CloudConfigurationManager.GetSetting("endpoint"));

            if (string.IsNullOrEmpty(oauthToken))
            {
                return null;
            }

            var docuSignAuthDTO = new DocuSignAuthDTO()
            {
                Email = curCredentials.Username,
                ApiPassword = oauthToken
            };

            return new AuthTokenDTO()
            {
                Token = JsonConvert.SerializeObject(docuSignAuthDTO),
                ExternalAccountId = curCredentials.Username
            };
        }

        private HttpClient CreateHttpClient(string endPoint)
        {
            return new HttpClient() { BaseAddress = new Uri(endPoint) };
        }

        private async Task<string> ObtainOAuthToken(CredentialsDTO curCredentials, string baseUrl)
        {
            var response = await CreateHttpClient(baseUrl)
                .PostAsync("oauth2/token",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("grant_type", "password"), 
                        new KeyValuePair<string, string>("client_id", CloudConfigurationManager.GetSetting("DocuSignIntegratorKey")), 
                        new KeyValuePair<string, string>("username", curCredentials.Username),
                        new KeyValuePair<string, string>("password", curCredentials.Password),
                        new KeyValuePair<string, string>("scope", "api"), 
                    }));
            try
            {
                var responseAsString = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeAnonymousType(responseAsString, new { access_token = "" });

                return responseObject.access_token;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                response.Dispose();
            }
        }

        [HttpPost]
        [Route("run")]
        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            return await (Task<PayloadDTO>)_basePluginController.HandleDockyardRequest(
                curPlugin, "Run", actionDto);
        }
    }
}