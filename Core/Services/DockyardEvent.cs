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
using Data.Interfaces.ManifestSchemas;
using Data.States;

namespace Core.Services
{
    public class DockyardEvent : IDockyardEvent
    {
        private readonly IProcessTemplate _processTemplate;
        private readonly ICrate _crate;

        public DockyardEvent()
        {
            _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
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

        public void ProcessInboundEvents(CrateDTO curCrateStandardEventReport)
        {
            EventReportMS eventReportMS = _crate.GetContents<EventReportMS>(curCrateStandardEventReport);


            if (eventReportMS.EventPayload == null)
                throw new ArgumentException("EventReport can't have a null payload");
            if (eventReportMS.ExternalAccountId == null)
                throw new ArgumentException("EventReport can't have a null ExternalAccountId");
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //find the corresponding DockyardAccount
                DockyardAccountDO curDockyardAccount = 
                    uow.AuthorizationTokenRepository.FindOne(
                        at => at.ExternalAccountId == eventReportMS.ExternalAccountId)
                        .UserDO;

                //find this Account's ProcessTemplates
                var initialProcessTemplatesList = uow.ProcessTemplateRepository
                    .FindList(pt => pt.DockyardAccount == curDockyardAccount)
                    .Where(x => x.ProcessTemplateState == ProcessTemplateState.Active);

                var subscribingProcessTemplates = _processTemplate.MatchEvents(initialProcessTemplatesList.ToList(),
                    eventReportMS);



                _processTemplate.LaunchProcesses(subscribingProcessTemplates, curCrateStandardEventReport);

            }


        }
    }
}
