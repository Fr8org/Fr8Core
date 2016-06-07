using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Utility;
using fr8.Infrastructure.Data.DataTransferObjects;
using Hub.Interfaces;
using StructureMap;

namespace Hub.Services
{
    public class Report : IReport
    {
        private readonly ISecurityServices _security;

        private const int DEFAULT_HISTORY_PAGE_SIZE = 10;
        private const int MIN_HISTORY_PAGE_SIZE = 5;
        public Report()
        {
            _security = ObjectFactory.GetInstance<ISecurityServices>();
        }

        public HistoryResultDTO<IncidentDTO> GetIncidents(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO)
        {
            return GetHistory<IncidentDTO, IncidentDO>(uow, historyQueryDTO, uow.IncidentRepository.GetQuery());
        }

        public HistoryResultDTO<FactDTO> GetFacts(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO)
        {
            return GetHistory<FactDTO, FactDO>(uow, historyQueryDTO, uow.FactRepository.GetQuery());
        }

        public HistoryResultDTO<T> GetHistory<T, TS>(IUnitOfWork uow, HistoryQueryDTO historyQueryDTO, IQueryable<TS> historyQuery) where T : HistoryItemDTO where TS : HistoryItemDO
        {
            ValidateInputQuery(ref historyQueryDTO);
            //get the current account
            var curAccount = _security.GetCurrentAccount(uow);
            //if this user does not have Admin role return empty list
            if (!DoesUserHaveAccessToHistory(uow, curAccount))
            {
                return new HistoryResultDTO<T>()
                {
                    CurrentPage = 1,
                    Items = new List<T>(),
                    TotalItemCount = 0
                };
            }

            var filteredResult = FilterHistoryItems(historyQuery, historyQueryDTO, curAccount);

            var totalItemCountForCurrentCriterias = filteredResult.Count();

            filteredResult = filteredResult.Page(historyQueryDTO.Page.Value, historyQueryDTO.ItemPerPage.Value);

            return new HistoryResultDTO<T>()
            {
                Items = filteredResult.ToList().Select(Mapper.Map<T>).ToList(),
                CurrentPage = historyQueryDTO.Page.Value,
                TotalItemCount = totalItemCountForCurrentCriterias
            };
        }
        private bool DoesUserHaveAccessToHistory(IUnitOfWork uow, Fr8AccountDO curAccount)
        {
            //get the roles to check if the account has admin role
            var curAccountRoles = curAccount.Roles;
            //prepare variable for incidents
            var adminRoleId = uow.AspNetRolesRepository.GetQuery().Single(r => r.Name == "Admin").Id;
            //if this user does not have Admin role return empty list
            return curAccountRoles == null || curAccountRoles.Any(x => x.RoleId == adminRoleId);
        }

        private void ValidateInputQuery(ref HistoryQueryDTO historyQueryDTO)
        {
            //lets make sure our inputs are correct
            historyQueryDTO = historyQueryDTO ?? new HistoryQueryDTO();
            historyQueryDTO.Page = historyQueryDTO.Page ?? 1;
            historyQueryDTO.Page = historyQueryDTO.Page < 1 ? 1 : historyQueryDTO.Page;
            historyQueryDTO.ItemPerPage = historyQueryDTO.ItemPerPage ?? DEFAULT_HISTORY_PAGE_SIZE;
            historyQueryDTO.ItemPerPage = historyQueryDTO.ItemPerPage < MIN_HISTORY_PAGE_SIZE ? MIN_HISTORY_PAGE_SIZE : historyQueryDTO.ItemPerPage;
            historyQueryDTO.IsDescending = historyQueryDTO.IsDescending ?? true;
        }

        private IQueryable<T> FilterHistoryItems<T>(IQueryable<T> historyQuery, HistoryQueryDTO historyQueryDTO, Fr8AccountDO curAccount) where T : HistoryItemDO
        {
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
                                                    || c.Fr8UserId.Contains(historyQueryDTO.Filter)
                                                );
            }

            historyQuery = historyQueryDTO.IsDescending.Value
                ? historyQuery.OrderByDescending(p => p.CreateDate)
                : historyQuery.OrderBy(p => p.CreateDate);

            return historyQuery;
        }
    }
}
