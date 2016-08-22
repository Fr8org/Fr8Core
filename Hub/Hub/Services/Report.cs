using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Utility;
using Fr8.Infrastructure.Data.DataTransferObjects;
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

        public PagedResultDTO<IncidentDTO> GetIncidents(IUnitOfWork uow, PagedQueryDTO pagedQueryDto)
        {
            return GetHistory<IncidentDTO, IncidentDO>(uow, pagedQueryDto, uow.IncidentRepository.GetQuery());
        }

        public PagedResultDTO<FactDTO> GetFacts(IUnitOfWork uow, PagedQueryDTO pagedQueryDto)
        {
            return GetHistory<FactDTO, FactDO>(uow, pagedQueryDto, uow.FactRepository.GetQuery());
        }

        public PagedResultDTO<T> GetHistory<T, TS>(IUnitOfWork uow, PagedQueryDTO pagedQueryDto, IQueryable<TS> historyQuery) where T : HistoryItemDTO where TS : HistoryItemDO
        {
            ValidateInputQuery(ref pagedQueryDto);
            //get the current account
            var curAccount = _security.GetCurrentAccount(uow);

            var filteredResult = FilterHistoryItems(historyQuery, pagedQueryDto, curAccount);

            var totalItemCountForCurrentCriterias = filteredResult.Count();

            filteredResult = filteredResult.Page(pagedQueryDto.Page.Value, pagedQueryDto.ItemPerPage.Value);

            return new PagedResultDTO<T>()
            {
                Items = filteredResult.ToList().Select(Mapper.Map<T>).ToList(),
                CurrentPage = pagedQueryDto.Page.Value,
                TotalItemCount = totalItemCountForCurrentCriterias
            };
        }

        private void ValidateInputQuery(ref PagedQueryDTO pagedQueryDto)
        {
            //lets make sure our inputs are correct
            pagedQueryDto = pagedQueryDto ?? new PagedQueryDTO();
            pagedQueryDto.Page = pagedQueryDto.Page ?? 1;
            pagedQueryDto.Page = pagedQueryDto.Page < 1 ? 1 : pagedQueryDto.Page;
            pagedQueryDto.ItemPerPage = pagedQueryDto.ItemPerPage ?? DEFAULT_HISTORY_PAGE_SIZE;
            pagedQueryDto.ItemPerPage = pagedQueryDto.ItemPerPage < MIN_HISTORY_PAGE_SIZE ? MIN_HISTORY_PAGE_SIZE : pagedQueryDto.ItemPerPage;
        }

        private IQueryable<T> FilterHistoryItems<T>(IQueryable<T> historyQuery, PagedQueryDTO pagedQueryDto, Fr8AccountDO curAccount) where T : HistoryItemDO
        {
            if (pagedQueryDto.IsCurrentUser)
            {
                historyQuery = historyQuery.Where(i => i.Fr8UserId == curAccount.Id);
            }

            if (!string.IsNullOrEmpty(pagedQueryDto.Filter))
            {
                historyQuery = historyQuery.Where(c => c.Data.Contains(pagedQueryDto.Filter)
                                                    || c.ObjectId.Contains(pagedQueryDto.Filter)
                                                    || c.Activity.Contains(pagedQueryDto.Filter)
                                                    || c.Component.Contains(pagedQueryDto.Filter)
                                                    || c.Fr8UserId.Contains(pagedQueryDto.Filter)
                                                );
            }
            var isDescending = pagedQueryDto.OrderBy?.StartsWith("-") ?? false;
            historyQuery = isDescending
                ? historyQuery.OrderByDescending(p => p.CreateDate)
                : historyQuery.OrderBy(p => p.CreateDate);

            return historyQuery;
        }
    }
}
