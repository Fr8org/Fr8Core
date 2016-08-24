using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;


namespace Hub.Interfaces
{
    public interface IActivity
    {
        Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO currentActivityDo);
        Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO curActivityDO);
        ActivityDO GetById(IUnitOfWork uow, Guid id);

        Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, 
            string userId, 
            Guid activityTemplateId,
            string label = null, 
            string name = null, 
            int? order = null, 
            Guid? parentNodeId = null, 
            bool createPlan = false, 
            Guid? authorizationTokenId = null,
            PlanVisibility newPlanVisibility = PlanVisibility.Standard);

        Task<PayloadDTO> Run(IUnitOfWork uow, ActivityDO curActivityDO, ActivityExecutionMode curActionExecutionMode, ContainerDO curContainerDO);
        Task<ActivityDTO> Activate(ActivityDO curActivityDO);
        Task Deactivate(ActivityDO curActivityDO);
        Task<T> GetActivityDocumentation<T>(ActivityDTO curActivityDTO, bool isSolution = false) where T : class;
        List<string> GetSolutionNameList(string terminalName);
        Task Delete(Guid id);
        Task DeleteChildNodes(Guid id);
        bool Exists(Guid id);

        Task<ActivityDO> GetSubordinateActivity(IUnitOfWork uow, Guid id);
    }
}