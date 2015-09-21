using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data.Interfaces;
using Data.Wrappers;
using Utilities;
using Data.Entities;
using Core.Interfaces;

namespace Core.Services
{
    public class Report : IReport
    {
        public Report() 
        {
          
        }      
      

        /// <summary>
        /// Returns List of Fact
        /// </summary>
        /// <param name="uow">unit of work</param>
        /// <returns>List of Incident</returns>
        public List<FactDO> GetAllFacts(IUnitOfWork uow)
        {
            var factDO = uow.FactRepository.GetAll().ToList();
            return factDO;
        }

        /// <summary>
        /// Returns List of Incident
        /// </summary>
        /// <param name="uow">unit of work</param>
        /// <returns>List of Incident</returns>
        public List<IncidentDO> GetAllIncidents(IUnitOfWork uow)
        {            
            var incidentDO = uow.IncidentRepository.GetAll().ToList();
            return incidentDO;
        }

    }
}
