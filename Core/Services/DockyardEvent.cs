using Core.Interfaces;
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

        public void ProcessInbound(string userID, CrateDTO curStandardEventReport)
        {
            //check if CrateDTO is not null
            if (curStandardEventReport == null)
                throw new ArgumentNullException("Paramter Standard Event Report is null.");
            //check if can parse to Standard Event Report
            if (String.IsNullOrEmpty(curStandardEventReport.Label) || !curStandardEventReport.Label.Equals("Standard Event Report", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentNullException("CrateDTO passed is not a Standard Event Report.");

            //Matchup process
            var processNodeTemplates = _processTemplate.GetStandardEventSubscribers(userID, curStandardEventReport);
            using (var unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                foreach (var processNodeTemplate in processNodeTemplates)
                {
                    //4. When there's a match, it means that it's time to launch a new Process based on this ProcessTemplate, 
                    //so make the existing call to ProcessTemplate#LaunchProcess.
                    _processTemplate.LaunchProcess(unitOfWork, processNodeTemplate);
                }
            }
        }


    }
}
