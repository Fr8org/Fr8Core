using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Fr8Data.Constants;
using Fr8Data.DataTransferObjects;


namespace Hub.Interfaces
{
    public interface IActivity
    {
        IEnumerable<TViewModel> GetAllActivities<TViewModel>();
        Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO currentActivityDo);
        Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO curActivityDO, bool saveResult = true);
        ActivityDO GetById(IUnitOfWork uow, Guid id);
        ActivityDO MapFromDTO(ActivityDTO curActivityDTO);

//      ActivityDO Create(IUnitOfWork uow, int activityTemplateId, string name, string label, int? order, PlanNodeDO parentNode, Guid? authorizationTokenId = null);

        Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, Guid activityTemplateId, 
                                             string label = null, string name = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null);

        Task<PayloadDTO> Run(IUnitOfWork uow, ActivityDO curActivityDO, ActivityExecutionMode curActionExecutionMode, ContainerDO curContainerDO);
        Task<ActivityDTO> Activate(ActivityDO curActivityDO);
        Task<ActivityDTO> Deactivate(ActivityDO curActivityDO);
        Task<T> GetActivityDocumentation<T>(ActivityDTO curActivityDTO, bool isSolution = false) where T : class;
        List<string> GetSolutionNameList(string terminalName);
        void Delete(Guid id);
    }
}