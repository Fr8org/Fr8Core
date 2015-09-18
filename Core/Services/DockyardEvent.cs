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

namespace Core.Services
{
    public class DockyardEvent : IDockyardEvent
    {
        private readonly IProcessTemplate _processTemplate;

        public DockyardEvent()
        {
            _processTemplate = ObjectFactory.GetInstance<IProcessTemplate>();
        }

        public void ProcessInbound(string userID, EventReportMS curEventReport)
        {
            //check if CrateDTO is not null
            if (curEventReport == null)
                throw new ArgumentNullException("Paramter Standard Event Report is null.");
          
            //Matchup process
            IList<ProcessTemplateDO> matchingProcessTemplates = _processTemplate.GetMatchingProcessTemplates(userID, curEventReport);
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var processNodeTemplate in matchingProcessTemplates)
                {
                    //4. When there's a match, it means that it's time to launch a new Process based on this ProcessTemplate, 
                    //so make the existing call to ProcessTemplate#LaunchProcess.
                    _processTemplate.LaunchProcess(unitOfWork, processNodeTemplate);
                }
            }
        }


    }
}
