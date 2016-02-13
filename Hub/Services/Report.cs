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
        public Report()
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }
        /// <summary>
        /// Returns List of Fact
        /// </summary>
        /// <param name="uow">unit of work</param>
        /// <returns>List of Incident</returns>
        public List<FactDO> GetAllFacts(IUnitOfWork uow)
        {
            //get the current account
            var curAccount = _security.GetCurrentAccount(uow);
            //get the roles to check if the account has admin role
            var curAccountRoles = curAccount.Roles;
            //prepare variable for facts
            var curFacts = new List<FactDO>();
            //get the role id
            //var adminRoleId = uow.AspNetRolesRepository.GetAll().Single(r => r.Name == "Admin").Id;
            //provide all facts if the user has admin role

            curFacts = uow.FactRepository.GetAll()
                .Where(i => i.CreatedBy == curAccount)
                .OrderByDescending(i => i.CreateDate)
                .Take(200)
                .ToList();

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
            //get the role id

            curIncidents = uow.IncidentRepository.GetAll()
                .Where(i => i.CustomerId == curAccount.Id)
                .OrderByDescending(i => i.CreateDate)
                .Take(200).ToList();

            return curIncidents;
        }

    }
}
