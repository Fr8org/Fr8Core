using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;
using StructureMap;
using Utilities;

namespace Hub.Services
{
    public class Report : IReport
    {
        private readonly ISecurityServices _security;
        private readonly IFact _fact;
        public Report()
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
            _fact = ObjectFactory.GetInstance<IFact>();
        }
        /// <summary>
        /// Returns List of Fact
        /// </summary>
        /// <param name="uow">unit of work</param>
        /// <returns>List of Incident</returns>
        public IList<FactDO> GetAllFacts(IUnitOfWork uow)
        {
            //get the current account
            var curAccount = _security.GetCurrentAccount(uow);
            //get the roles to check if the account has admin role
            var curAccountRoles = curAccount.Roles;
            var curFacts = _fact.GetAll(uow, curAccountRoles);
            return curFacts;
        }
        /// <summary>
        /// Returns List of Incident
        /// </summary>
        /// <param name="uow">unit of work</param>
        /// <returns>List of Incident</returns>
        public List<IncidentDO> GetAllIncidents(IUnitOfWork uow)
        {
            //get the current account
            var curAccount = _security.GetCurrentAccount(uow);
            //get the roles to check if the account has admin role
            var curAccountRoles = curAccount.Roles;
            //prepare variable for incidents
            var curIncidents = new List<IncidentDO>();
            var adminRoleId = uow.AspNetRolesRepository.GetQuery().Single(r => r.Name == "Admin").Id;
            //get the role id
            if (curAccountRoles.Any(x => x.RoleId == adminRoleId))
            curIncidents = uow.IncidentRepository.GetQuery()
                .OrderByDescending(i => i.CreateDate)
                .Take(200).ToList();
            return curIncidents;
        }

    }
}
