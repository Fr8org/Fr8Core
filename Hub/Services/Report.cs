using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Utility;
using Hub.Interfaces;
using StructureMap;
using Utilities;

namespace Hub.Services
{
    public class Report : IReport
    {
        private readonly ISecurityServices _security;
        private readonly IFact _fact;

        private const int DEFAULT_HISTORY_PAGE_SIZE = 10;
        private const int MIN_HISTORY_PAGE_SIZE = 5;
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

        public HistoryResultDTO GetIncidents(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO)
        {
            //get the current account
            var curAccount = _security.GetCurrentAccount(uow);
            //get the roles to check if the account has admin role
            var curAccountRoles = curAccount.Roles;
            //prepare variable for incidents
            var adminRoleId = uow.AspNetRolesRepository.GetQuery().Single(r => r.Name == "Admin").Id;
            //if this user does not have Admin role return empty list
            if (curAccountRoles != null && curAccountRoles.All(x => x.RoleId != adminRoleId)) { 
                return new HistoryResultDTO() { CurrentPage = 1, Items = new List<HistoryItemDTO>(), TotalItemCount = 0};
            }

            //lets make sure our inputs are correct
            historyQueryDTO = historyQueryDTO ?? new HistoryQueryDTO();
            historyQueryDTO.Page = historyQueryDTO.Page ?? 1;
            historyQueryDTO.Page = historyQueryDTO.Page < 1 ? 1 : historyQueryDTO.Page;
            historyQueryDTO.ItemPerPage = historyQueryDTO.ItemPerPage ?? DEFAULT_HISTORY_PAGE_SIZE;
            historyQueryDTO.ItemPerPage = historyQueryDTO.ItemPerPage < MIN_HISTORY_PAGE_SIZE ? MIN_HISTORY_PAGE_SIZE : historyQueryDTO.ItemPerPage;
            historyQueryDTO.IsDescending = historyQueryDTO.IsDescending ?? true;

            var historyQuery = uow.HistoryRepository.GetQuery();

            if (historyQueryDTO.IsCurrentUser)
            {
                historyQuery = historyQuery.Where(i => i.Fr8UserId == curAccount.Id);
            }

            if (!string.IsNullOrEmpty(historyQueryDTO.Filter))
            {
                historyQuery = historyQuery.Where(c => c.Data.Contains(historyQueryDTO.Filter) 
                                                    || c.ObjectId.Contains(historyQueryDTO.Filter) 
                                                    || c.Activity.Contains(historyQueryDTO.Filter)
                                                    || c.Component.Contains(historyQueryDTO.Filter)
                                                );
            }
            //lets allow ordering with just name for now

            historyQuery = historyQueryDTO.IsDescending.Value
                ? historyQuery.OrderByDescending(p => p.CreateDate)
                : historyQuery.OrderBy(p => p.CreateDate);
            

            var totalItemCountForCurrentCriterias = historyQuery.Count();

            historyQuery = historyQuery.Skip(historyQueryDTO.ItemPerPage.Value * (historyQueryDTO.Page.Value - 1))
                    .Take(historyQueryDTO.ItemPerPage.Value);

            return new HistoryResultDTO
            {
                Items = historyQuery.ToList().Select(Mapper.Map<HistoryItemDTO>).ToList(),
                CurrentPage = historyQueryDTO.Page.Value,
                TotalItemCount = totalItemCountForCurrentCriterias
            };
        }

        /// <summary>
        /// This method returns Incidents for Report
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">Number of incidents to show per page</param>
        /// <param name="allIncidents">This marks if all incidents should be returned or only having current user Id</param>
        /// <returns></returns>
        public List<IncidentDO> GetIncidents(IUnitOfWork uow, int page, int pageSize, bool isCurrentUser)
        {
            //get the current account
            var curAccount = _security.GetCurrentAccount(uow);
            //get the roles to check if the account has admin role
            var curAccountRoles = curAccount.Roles;
            //prepare variable for incidents
            var curIncidents = new List<IncidentDO>();
            var adminRoleId = uow.AspNetRolesRepository.GetQuery().Single(r => r.Name == "Admin").Id;
            //if this user does not have Admin role return empty list
            if(curAccountRoles != null && curAccountRoles.All(x => x.RoleId != adminRoleId))
                return curIncidents;
            //if user has Admin role and asked for only current user incidents
            if (isCurrentUser)
                curIncidents = uow.IncidentRepository.GetQuery()
                    .Where(i => i.Fr8UserId == curAccount.Id)
                    .OrderByDescending(i => i.CreateDate)
                    .Page(page, pageSize)
                    .ToList();
            //in the other case return incidents generated by all users
            else
            {
                curIncidents = uow.IncidentRepository.GetQuery()
                    .OrderByDescending(i => i.CreateDate)
                    .Page(page, pageSize)
                    .ToList();
            }
            return curIncidents;
        }
    }
}
