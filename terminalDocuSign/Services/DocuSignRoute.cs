using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using StructureMap;
using terminalDocuSign.Interfaces;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Services
{
    /// <summary>
    /// Service to create DocuSign related routes in Hub
    /// </summary>
    public class DocuSignRoute : IDocuSignRoute
    {
        private readonly IActivityTemplate _activityTemplate;
        private readonly IAction _action;
        private readonly IHubCommunicator _hubCommunicator;

        public DocuSignRoute()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _action = ObjectFactory.GetInstance<IAction>();
            _hubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>();
        }

        /// <summary>
        /// Creates Monitor All DocuSign Events route with Record DocuSign Events and Store MT Data actions.
        /// </summary>
        public async Task CreateRoute_MonitorAllDocuSignEvents(string curFr8UserId, AuthorizationTokenDTO authTokenDTO)
        {
            //first check if this exists
            var emptyMonitorRoute = new RouteEmptyDTO
            {
                Name = "MonitorAllDocuSignEvents",
                Description = "MonitorAllDocuSignEvents",
                RouteState = RouteState.Active,
                Tag = "monitor"
            };
            var monitorDocusignRoute = await _hubCommunicator.CreateRoute(emptyMonitorRoute, curFr8UserId);
            var activityTemplates = await _hubCommunicator.GetActivityTemplates(null, curFr8UserId);
            var recordDocusignEventsTemplate = GetActivityTemplate(activityTemplates, "Record_DocuSign_Events");
            var storeMTDataTemplate = GetActivityTemplate(activityTemplates, "StoreMTData");
            await _hubCommunicator.CreateAndConfigureAction(recordDocusignEventsTemplate.Id, "Record_DocuSign_Events",
                curFr8UserId, "Record DocuSign Events", monitorDocusignRoute.StartingSubrouteId, false, new Guid(authTokenDTO.Id));
            await _hubCommunicator.CreateAndConfigureAction(storeMTDataTemplate.Id, "StoreMTData",
                curFr8UserId, "Store MT Data", monitorDocusignRoute.StartingSubrouteId);
            var routeDO = Mapper.Map<RouteDO>(monitorDocusignRoute);
            await _hubCommunicator.ActivateRoute(routeDO, curFr8UserId);
        }

        private ActivityTemplateDTO GetActivityTemplate(IEnumerable<ActivityTemplateDTO> activityList, string activityTemplateName)
        {
            var template = activityList.FirstOrDefault(x => x.Name == activityTemplateName);
            if (template == null)
            {
                throw new Exception(string.Format("ActivityTemplate {0} was not found", activityTemplateName));
            }

            return template;
        }

        private RouteDO GetExistingRoute(IUnitOfWork uow, string routeName, string fr8AccountEmail)
        {
            if (uow.RouteRepository.GetQuery().Any(existingRoute =>
                existingRoute.Name.Equals(routeName) &&
                existingRoute.Fr8Account.Email.Equals(fr8AccountEmail)))
            {
                return uow.RouteRepository.GetQuery().First(existingRoute =>
                    existingRoute.Name.Equals(routeName) &&
                    existingRoute.Fr8Account.Email.Equals(fr8AccountEmail));
            }

            return null;
        }
    }
}