using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using fr8.Infrastructure.Data.Constants;
using fr8.Infrastructure.Data.DataTransferObjects;


namespace Hub.Interfaces
{
    public interface IActivity
    {
        Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO currentActivityDo);
        Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO curActivityDO);
        ActivityDO GetById(IUnitOfWork uow, Guid id);

        Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, Guid activityTemplateId, 
                                             string label = null, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null);

        Task<PayloadDTO> Run(IUnitOfWork uow, ActivityDO curActivityDO, ActivityExecutionMode curActionExecutionMode, ContainerDO curContainerDO);
        Task<ActivityDTO> Activate(ActivityDO curActivityDO);
        Task Deactivate(ActivityDO curActivityDO);
        Task<T> GetActivityDocumentation<T>(ActivityDTO curActivityDTO, bool isSolution = false) where T : class;
        List<string> GetSolutionNameList(string terminalName);
        Task Delete(Guid id);
        Task DeleteChildNodes(Guid id);
    }
}