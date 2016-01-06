using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using Utilities;

namespace Hub.Services
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
            var factDO = uow.FactRepository.GetAll().OrderByDescending(i => i.CreateDate).Take(200).ToList();
            return factDO;
        }

        /// <summary>
        /// Returns List of Incident
        /// </summary>
        /// <param name="uow">unit of work</param>
        /// <returns>List of Incident</returns>
        public List<IncidentDO> GetAllIncidents(IUnitOfWork uow)
        {            
            var incidentDO = uow.IncidentRepository.GetAll().OrderByDescending(i => i.CreateDate).Take(200).ToList();
            return incidentDO;
        }

    }
}
