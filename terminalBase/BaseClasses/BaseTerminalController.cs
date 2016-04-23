using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Data.Constants;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using TerminalBase.Infrastructure;
using Utilities.Configuration.Azure;
using Newtonsoft.Json;
using Data.Infrastructure;
using Utilities.Logging;
using Utilities;
using System.Net.Http;
using System.IO;
using System.Linq;
using System.Web.Routing;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Hub.Exceptions;
using System.Runtime.ExceptionServices;

namespace TerminalBase.BaseClasses
{
    //this is a quasi base class. We can't use inheritance directly because it's across project boundaries, but
    //we can generate instances of this.
    public class BaseTerminalController : ApiController
    {
        private readonly BaseTerminalEvent _baseTerminalEvent;
        private bool _integrationTestMode;

        public bool IntegrationTestMode
        {
            get
            {
                return _integrationTestMode;
            }
        }

        public BaseTerminalController()
        {
            _baseTerminalEvent = new BaseTerminalEvent();
        }

        /// <summary>
        /// Reports Terminal Error incident
        /// </summary>
        [HttpGet]
        public IHttpActionResult ReportTerminalError(string terminalName, Exception terminalError,string userId = null)
        {
            if (_integrationTestMode)
                return Ok();

            var exceptionMessage = terminalError.GetFullExceptionMessage() + "      \r\n" + terminalError.ToString();//string.Format("{0}\r\n{1}", terminalError.Message, terminalError.StackTrace);
            try
            {
                return Json(_baseTerminalEvent.SendTerminalErrorIncident(terminalName, exceptionMessage, terminalError.GetType().Name,userId));
            }
            catch (Exception ex)
            {
                string errorMessage = "An error has occurred in terminal {0}.             \r\n{1}             \r\n";
                errorMessage += "Additionally, an error has occurred while trying to post error details to the Hub.             \r\n{2}";
                Logger.GetLogger().ErrorFormat(errorMessage, terminalName, exceptionMessage, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Reports start up incident
        /// </summary>
        /// <param name="terminalName">Name of the terminal which is starting up</param>
        [HttpGet]
        public IHttpActionResult AfterStartup(string terminalName)
        {
            return null;

            //TODO: Commented during development only. So that app loads fast.
            //return Json(ReportStartUp(terminalName));
        }

        /// <summary>
        /// Reports start up event by making a Post request
        /// </summary>
        /// <param name="terminalName"></param>
        private Task<string> ReportStartUp(string terminalName)
        {
            if (_integrationTestMode)
                return Task.FromResult<string>(String.Empty);

            return _baseTerminalEvent.SendEventOrIncidentReport(terminalName, "Terminal Incident");
        }


        private void BindTestHubCommunicator(object curObject, string explicitData)
        {
            var baseTerminalAction = curObject as BaseTerminalActivity;

            if (baseTerminalAction == null)
            {
                return;
            }

            baseTerminalAction.HubCommunicator = new TestMonitoringHubCommunicator(explicitData);
        }

        private void BindExplicitDataHubCommunicator(object curObject, string explicitData)
        {
            var baseTerminalAction = curObject as BaseTerminalActivity;

            if (baseTerminalAction == null)
            {
                return;
            }

            baseTerminalAction.HubCommunicator = new ExplicitDataHubCommunicator(explicitData);
        }

        private void SetCurrentUser(object curObject, string userId)
        {
            var baseTerminalAction = curObject as BaseTerminalActivity;

            if (baseTerminalAction == null)
            {
                return;
            }

            baseTerminalAction.SetCurrentUser(userId);
        }

        private void ConfigureHubCommunicator(object curObject, string terminalName)
        {
            var baseTerminalAction = curObject as BaseTerminalActivity;

            if (baseTerminalAction == null)
            {
                return;
            }

            baseTerminalAction.HubCommunicator.Configure(terminalName);
        }

        /// <summary>
        /// Reports event when process an action
        /// </summary>
        /// <param name="terminalName"></param>
        private Task<string> ReportEvent(string terminalName)
        {
            if (_integrationTestMode)
                return Task.FromResult<string>(String.Empty);

            return _baseTerminalEvent.SendEventOrIncidentReport(terminalName, "Terminal Event");
        }

        // For /Configure and /Activate actions that accept ActionDTO
        public async Task<object> HandleFr8Request(string curTerminal, string curActionPath, Fr8DataDTO curDataDTO)
        {
            if (curDataDTO?.ActivityDTO == null)
                throw new ArgumentNullException(nameof(curDataDTO.ActivityDTO));

            if (curDataDTO.ActivityDTO.ActivityTemplate == null)
                throw new ArgumentException("ActivityTemplate is null", nameof(curDataDTO.ActivityDTO));

            _integrationTestMode = false;

            var curActionDTO = curDataDTO.ActivityDTO;
            var activityTemplateName = curActionDTO.ActivityTemplate.Name;
            if (activityTemplateName.EndsWith("_TEST"))
            {
                _integrationTestMode = true;
                activityTemplateName = activityTemplateName
                    .Substring(0, activityTemplateName.Length - "_TEST".Length);
            }

            string curAssemblyName = string.Format("{0}.Activities.{1}_v{2}", curTerminal, activityTemplateName, curActionDTO.ActivityTemplate.Version);

            Type calledType = Type.GetType(curAssemblyName + ", " + curTerminal);
            if (calledType == null)
                throw new ArgumentException(string.Format("Activity {0}_v{1} doesn't exist in {2} terminal.",
                    curActionDTO.ActivityTemplate.Name,
                    curActionDTO.ActivityTemplate.Version,
                    curTerminal), "curActionDTO");

            MethodInfo curMethodInfo = calledType.GetMethod(curActionPath, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            object curObject = Activator.CreateInstance(calledType);

            if (_integrationTestMode)
            {
                BindTestHubCommunicator(curObject, curDataDTO.ExplicitData);
            }

            var curActivityDO = Mapper.Map<ActivityDO>(curActionDTO);
            //this is a comma separated string
            var curDocumentation = curActionDTO.Documentation;

            var curAuthTokenDO = Mapper.Map<AuthorizationTokenDO>(curActionDTO.AuthToken);

            Task<ActivityDO> response;
            var currentUserId = curAuthTokenDO != null ? curAuthTokenDO.UserID : null;
            //Set Current user of action
            SetCurrentUser(curObject, currentUserId);
            ConfigureHubCommunicator(curObject, curTerminal);
            try
            {
                switch (curActionPath.ToLower())
                {
                    case "configure":
                        {
                            var resutlActionDO = await (Task<ActivityDO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDO, curAuthTokenDO });
                            return Mapper.Map<ActivityDTO>(resutlActionDO);
                        }
                    case "run":
                    case "executechildactivities":
                        {                            
                            OnStartActivity(curTerminal, activityTemplateName, IntegrationTestMode);
                            var resultPayloadDTO = await (Task<PayloadDTO>)curMethodInfo
                                .Invoke(curObject, new Object[] { curActivityDO, curDataDTO.ContainerId, curAuthTokenDO });
                            await OnCompletedActivity(curTerminal, IntegrationTestMode);
                            
                            return resultPayloadDTO;        
                        }
                    case "initialconfigurationresponse":
                        {
                            Task<ActivityDO> resutlActionDO = (Task<ActivityDO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDO, curAuthTokenDO });
                            return await resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));
                        }
                    case "followupconfigurationresponse":
                        {
                            Task<ActivityDO> resutlActionDO = (Task<ActivityDO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDO, curAuthTokenDO });
                            return await resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));
                        }
                    case "activate":
                        {
                            //activate is an optional method so it may be missing
                            if (curMethodInfo == null) return Mapper.Map<ActivityDTO>(curActivityDO);

                            Task<ActivityDO> resutlActionDO = (Task<ActivityDO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDO, curAuthTokenDO });
                            return await resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));
                        }
                    case "deactivate":
                        {
                            //deactivate is an optional method so it may be missing
                            if (curMethodInfo == null) return Mapper.Map<ActivityDTO>(curActivityDO);

                            Task<ActivityDO> resutlActionDO;
                            var param = curMethodInfo.GetParameters();
                            if (param.Length == 2)
                                resutlActionDO = (Task<ActivityDO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDO, curAuthTokenDO });
                            else
                            {
                                response = (Task<ActivityDO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDO });
                                return await response.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result)); ;
                            }

                            return resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));
                        }
                    case "documentation":
                    {
                        if (curMethodInfo == null)
                        {
                            return getDefaultDocumentation();
                        }
                        return await HandleDocumentationRequest(curObject, curMethodInfo, curActivityDO, curDocumentation);
                    }
                    default:
                        response = (Task<ActivityDO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDO });
                        return await response.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result)); ;
                }
            }
            catch (Exception e)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                var endpoint = (curActivityDO.ActivityTemplate != null && curActivityDO.ActivityTemplate.Terminal != null && curActivityDO.ActivityTemplate.Terminal.Endpoint != null) ? curActivityDO.ActivityTemplate.Terminal.Endpoint : "<no terminal url>";
                EventManager.TerminalInternalFailureOccurred(endpoint, JsonConvert.SerializeObject(curActivityDO, settings), e, curActivityDO.Id.ToString());

                throw;              
            }
        }
        private void OnStartActivity(string terminalName, string actionName, bool isTestActivityTemplate)
        {
            if (isTestActivityTemplate)
                return;

            _baseTerminalEvent.SendEventReport(
                terminalName,
                string.Format("{0} began processing this Container at {1}. Sending to Action {2}", terminalName, DateTime.Now.ToString("G"), actionName));
        }

        private Task OnCompletedActivity(string terminalName, bool isTestActivityTemplate)
        {
            if (isTestActivityTemplate)
                return Task.FromResult<object>(null);

            return Task.Run(() =>
             _baseTerminalEvent.SendEventReport(
                 terminalName,
                 string.Format("{0} completed processing this Container at {1}.", terminalName, DateTime.Now.ToString("G"))));
        }

        public HttpResponseMessage GetActivityDocumentation(string helpPath)
        {
            string htmlContent = FindDocumentation(helpPath);

            var response = new HttpResponseMessage();
            response.Content = new StringContent(htmlContent);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

            return response;
        }

        private string FindDocumentation(string helppath)
        {
            string filename = System.Web.Hosting.HostingEnvironment.MapPath(string.Format("~\\Documentation\\{0}.html", helppath));
            string content = "";

            try
            {
                content = System.IO.File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            return content;
        }

        private async Task<dynamic> HandleDocumentationRequest(object classInstance,MethodInfo curMethodInfo, ActivityDO curActivityDO, string curDocumentation)
        {
            if (!curDocumentation.IsNullOrEmpty() && curDocumentation.Split(',').Contains("MainPage"))
            {
                Task<SolutionPageDTO> resultSolutionPageDTO = (Task<SolutionPageDTO>)curMethodInfo
                    .Invoke(classInstance, new Object[] { curActivityDO, curDocumentation });
                return await resultSolutionPageDTO;
            }
            if (!curDocumentation.IsNullOrEmpty() && curDocumentation.Split(',').Contains("HelpMenu"))
            {
                Task<ActivityResponseDTO> resultActivityRepsonceDTO = (Task<ActivityResponseDTO>)curMethodInfo
                    .Invoke(classInstance, new Object[] { curActivityDO, curDocumentation });
                return await resultActivityRepsonceDTO;
            }
            return Task.FromResult(new ActivityResponseDTO {Type = ActivityResponse.Error.ToString(), Body = "Unknown display method"});
        }

        private SolutionPageDTO getDefaultDocumentation()
        {
            var curSolutionPage = new SolutionPageDTO
            {
                Name = "No Documentation method found",
                Body = "Please add the Documentation method to the Solution class"
            };

            return curSolutionPage;
        }
    }
}
