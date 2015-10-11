using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Exceptions;
using Data.Interfaces.ManifestSchemas;
using Data.States;

namespace Core.Services
{
    public class DockyardEvent : IDockyardEvent
    {
        private readonly IProcessTemplate _processTemplate;
        private readonly IProcess _process;
        private readonly ICrate _crate;

        public DockyardEvent()
        {
            _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
            _process = ObjectFactory.GetInstance<IProcess>();
            _crate = ObjectFactory.GetInstance<ICrate>();
        }

        //public void ProcessInbound(string userID, EventReportMS curEventReport)
        //{
        //    //check if CrateDTO is not null
        //    if (curEventReport == null)
        //        throw new ArgumentNullException("Paramter Standard Event Report is null.");

        //    //Matchup process
        //    IList<ProcessTemplateDO> matchingProcessTemplates = _processTemplate.GetMatchingProcessTemplates(userID, curEventReport);
        //    using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
        //    {
        //        foreach (var processNodeTemplate in matchingProcessTemplates)
        //        {
        //            //4. When there's a match, it means that it's time to launch a new Process based on this ProcessTemplate, 
        //            //so make the existing call to ProcessTemplate#LaunchProcess.
        //            _processTemplate.LaunchProcess(unitOfWork, processNodeTemplate);
        //        }
        //    }
        //}

        public async Task ProcessInboundEvents(CrateDTO curCrateStandardEventReport)
        {
            EventReportMS eventReportMS = _crate.GetContents<EventReportMS>(curCrateStandardEventReport);


            if (eventReportMS.EventPayload == null)
                throw new ArgumentException("EventReport can't have a null payload");
            if (eventReportMS.ExternalAccountId == null)
                throw new ArgumentException("EventReport can't have a null ExternalAccountId");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //find the corresponding DockyardAccount
                var authToken = uow.AuthorizationTokenRepository
                    .FindOne(at => at.ExternalAccountId == eventReportMS.ExternalAccountId);
                if (authToken == null)
                {
                    return;
                }

                var curDockyardAccount = authToken.UserDO;

                //find this Account's ProcessTemplates
                var initialProcessTemplatesList = uow.ProcessTemplateRepository
                    .FindList(pt => pt.DockyardAccount.Id == curDockyardAccount.Id)
                    .Where(x => x.ProcessTemplateState == ProcessTemplateState.Active);

                var subscribingProcessTemplates = _processTemplate.MatchEvents(initialProcessTemplatesList.ToList(),
                    eventReportMS);



                await LaunchProcesses(subscribingProcessTemplates, curCrateStandardEventReport);
            
            }
        }

        public Task LaunchProcesses(List<ProcessTemplateDO> curProcessTemplates, CrateDTO curEventReport)
        {
            var processes = new List<Task>();

            foreach (var curProcessTemplate in curProcessTemplates)
            {
                //4. When there's a match, it means that it's time to launch a new Process based on this ProcessTemplate, 
                //so make the existing call to ProcessTemplate#LaunchProcess.
                processes.Add(LaunchProcess(curProcessTemplate, curEventReport));
            }
            
            return Task.WhenAll(processes);
        }

        public async Task LaunchProcess(ProcessTemplateDO curProcessTemplate, CrateDTO curEventData)
        {
            if (curProcessTemplate == null)
                throw new EntityNotFoundException(curProcessTemplate);

            if (curProcessTemplate.ProcessTemplateState != ProcessTemplateState.Inactive)
            {
                await _process.Launch(curProcessTemplate, curEventData);
            }
        }
    }
}
