using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using TerminalBase.Infrastructure;
using Newtonsoft.Json;
using Utilities.Logging;
using Utilities;
using System.Net.Http;
using System.Linq;
using System.Net.Http.Headers;
using Fr8Data.Constants;
using Fr8Data.DataTransferObjects;

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

            var exceptionMessage = terminalError.GetFullExceptionMessage() + "  \n\r   " + terminalError.ToString();//string.Format("{0}\r\n{1}", terminalError.Message, terminalError.StackTrace);
            try
            {
                return Json(_baseTerminalEvent.SendTerminalErrorIncident(terminalName, exceptionMessage, terminalError.GetType().Name,userId));
            }
            catch (Exception ex)
            {
                string errorMessage = $"An error has occurred in terminal [{terminalName}]. {exceptionMessage} | Fr8UserId = {userId} \n\r "
                                    + $"Additionally, an error has occurred while trying to post error details to the Hub. {ex.Message}";

                Logger.LogError(errorMessage);
                //Logger.GetLogger().ErrorFormat(errorMessage, terminalName, exceptionMessage, ex.ToString());
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

        /*
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

        private void SetCurrentUser(object curObject, string userId, string userEmail)
        {
            var baseTerminalAction = curObject as BaseTerminalActivity;

            if (baseTerminalAction == null)
            {
                return;
            }

            baseTerminalAction.SetCurrentUser(userId, userEmail);
        }

        private void ConfigureHubCommunicator(object curObject, string terminalName)
        {
            var baseTerminalAction = curObject as BaseTerminalActivity;

            if (baseTerminalAction == null)
            {
                return;
            }

            baseTerminalAction.HubCommunicator.Configure(terminalName);
        }*/

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

        void LogWhenRequestRecived(string actionPath,string terminalName, string activityId)
        {
            Logger.LogInfo($"[{terminalName}] received /{actionPath} call  at {DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")} for ActivityID {activityId}", terminalName);
        }

        void LogWhenRequestResponded(string actionPath,string terminalName, string activityId)
        {
            Logger.LogInfo($"[{terminalName}] responded to /{actionPath} call  at {DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss tt")} for ActivityID {activityId}", terminalName);
        }

        // For /Configure and /Activate actions that accept ActionDTO
        public async Task<object> HandleFr8Request(string curTerminal, string curActionPath, Fr8DataDTO curDataDTO)
        {
            if (curDataDTO?.ActivityDTO == null)
            {
                Logger.LogError($"curDataDTO activity DTO is null", curTerminal);
                throw new ArgumentNullException(nameof(curDataDTO.ActivityDTO));
            }
                

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

            string curAssemblyName = string.Format("{0}.Actions.{1}_v{2}", curTerminal, activityTemplateName, curActionDTO.ActivityTemplate.Version);

            Type calledType = Type.GetType(curAssemblyName + ", " + curTerminal);
            if (calledType == null)
                throw new ArgumentException(string.Format("Action {0}_v{1} doesn't exist in {2} terminal.",
                    curActionDTO.ActivityTemplate.Name,
                    curActionDTO.ActivityTemplate.Version,
                    curTerminal), "curActionDTO");

            MethodInfo curMethodInfo = calledType.GetMethod(curActionPath, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            
            object curObject = Activator.CreateInstance(calledType);

            if (_integrationTestMode)
            {
                //BindTestHubCommunicator(curObject, curDataDTO.ExplicitData);
            }

            var curActivityDTO = Mapper.Map<ActivityDTO>(curActionDTO);
            //this is a comma separated string
            var curDocumentation = curActionDTO.Documentation;

            var curAuthTokenDO = curActionDTO.AuthToken;

            Task<ActivityDTO> response;
            var currentUserId = curAuthTokenDO?.UserId;
            var currentUserEmail = curAuthTokenDO?.ExternalAccountId;
            //Set Current user of action
            //SetCurrentUser(curObject, currentUserId, currentUserEmail);
            //ConfigureHubCommunicator(curObject, curTerminal);
            try
            {
                // null checking
                curActionDTO = curActionDTO ?? new ActivityDTO();
                curActionDTO.ActivityTemplate = curActionDTO.ActivityTemplate ?? new ActivityTemplateDTO();
                curActionDTO.ActivityTemplate.Terminal = curActionDTO.ActivityTemplate.Terminal ?? new TerminalDTO();
                curActivityDTO = curActivityDTO ?? new ActivityDTO();

                //log when request start proceeding
                LogWhenRequestRecived(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                switch (curActionPath.ToLower())
                {
                    case "configure":
                        {
                            var resultActionDO = await (Task<ActivityDTO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDTO, curAuthTokenDO });

                            LogWhenRequestResponded(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                            return Mapper.Map<ActivityDTO>(resultActionDO);
                        }
                    case "run":
                    case "executechildactivities":
                        {
                            OnStartActivity(curTerminal, activityTemplateName, IntegrationTestMode);
                            var resultPayloadDTO = await (Task<PayloadDTO>)curMethodInfo
                                .Invoke(curObject, new Object[] { curActivityDTO, curDataDTO.ContainerId, curAuthTokenDO });
                            await OnCompletedActivity(curTerminal, IntegrationTestMode);

                            LogWhenRequestResponded(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                            return resultPayloadDTO;        
                        }
                    case "initialconfigurationresponse":
                        {
                            Task<ActivityDTO> resutlActionDO = (Task<ActivityDTO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDTO, curAuthTokenDO });

                            var resultICR = await resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));

                            LogWhenRequestResponded(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                            return resultICR;
                        }
                    case "followupconfigurationresponse":
                        {
                            Task<ActivityDTO> resutlActionDO = (Task<ActivityDTO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDTO, curAuthTokenDO });

                            var resultFCR = await resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));

                            LogWhenRequestResponded(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                            return resultFCR;
                        }
                    case "activate":
                        {
                            //activate is an optional method so it may be missing
                            if (curMethodInfo == null) return Mapper.Map<ActivityDTO>(curActivityDTO);

                            Task<ActivityDTO> resutlActionDO = (Task<ActivityDTO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDTO, curAuthTokenDO });

                            var resultA = await resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));

                            LogWhenRequestResponded(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                            return resultA;
                        }
                    case "deactivate":
                        {
                            //deactivate is an optional method so it may be missing
                            if (curMethodInfo == null) return Mapper.Map<ActivityDTO>(curActivityDTO);

                            Task<ActivityDTO> resutlActionDO;
                            var param = curMethodInfo.GetParameters();
                            if (param.Length == 2)
                                resutlActionDO = (Task<ActivityDTO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDTO, curAuthTokenDO });
                            else
                            {
                                response = (Task<ActivityDTO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDTO });

                                var resultD = await response.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));

                                LogWhenRequestResponded(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                                return resultD;
                            }

                            return resutlActionDO.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));
                        }
                    case "documentation":
                    {
                        if (curMethodInfo == null)
                        {
                            return getDefaultDocumentation();
                        }

                        var resultDCN = await HandleDocumentationRequest(curObject, curMethodInfo, curActivityDTO, curDocumentation); 
                        
                        LogWhenRequestResponded(curActionPath.ToLower(), curActionDTO.ActivityTemplate.Terminal.Name, curActivityDTO.Id.ToString());

                        return resultDCN;
                    }
                    default:
                        response = (Task<ActivityDTO>)curMethodInfo.Invoke(curObject, new Object[] { curActivityDTO });

                        var result = await response.ContinueWith(x => Mapper.Map<ActivityDTO>(x.Result));

                        return result;
                }

                
            }
            catch (Exception e)
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };

                //Logger.GetLogger().Error($"Exception caught while processing {curActionPath} for {this.GetType()}", e);
                Logger.LogError($"Exception caught while processing {curActionPath} for {this.GetType()} with exception {e.Data} and stack trace {e.StackTrace} and message {e.GetFullExceptionMessage()}", curTerminal);
                var endpoint = (curActivityDTO.ActivityTemplate != null && curActivityDTO.ActivityTemplate.Terminal != null && curActivityDTO.ActivityTemplate.Terminal.Endpoint != null) ? curActivityDTO.ActivityTemplate.Terminal.Endpoint : "<no terminal url>";
                //EventManager.TerminalInternalFailureOccurred(endpoint, JsonConvert.SerializeObject(curActivityDTO, settings), e, curActivityDTO.Id.ToString());
                //TODO check this
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

        private async Task<dynamic> HandleDocumentationRequest(object classInstance,MethodInfo curMethodInfo, ActivityDTO curActivityDTO, string curDocumentation)
        {
            if (!curDocumentation.IsNullOrEmpty() && curDocumentation.Split(',').Contains("MainPage"))
            {
                Task<SolutionPageDTO> resultSolutionPageDTO = (Task<SolutionPageDTO>)curMethodInfo
                    .Invoke(classInstance, new Object[] { curActivityDTO, curDocumentation });
                return await resultSolutionPageDTO;
            }
            if (!curDocumentation.IsNullOrEmpty() && curDocumentation.Split(',').Contains("HelpMenu"))
            {
                Task<ActivityResponseDTO> resultActivityRepsonceDTO = (Task<ActivityResponseDTO>)curMethodInfo
                    .Invoke(classInstance, new Object[] { curActivityDTO, curDocumentation });
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
