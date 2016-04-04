using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Interfaces;
using Newtonsoft.Json;

namespace DockyardTest.Services.Container
{
    class ActivityServiceMock : IActivity
    {
        private readonly IActivity _activity;

        public readonly List<ActivityExecutionCall> ExecutedActivities = new List<ActivityExecutionCall>();
        public readonly Dictionary<Guid, ActivityMockBase> CustomActivities = new Dictionary<Guid, ActivityMockBase>();

        public ActivityServiceMock(IActivity activity)
        {
            _activity = activity;
        }

        public IEnumerable<TViewModel> GetAllActivities<TViewModel>()
        {
            return _activity.GetAllActivities<TViewModel>();
        }

        public Task<ActivityDTO> SaveOrUpdateActivity(ActivityDO currentActivityDo)
        {
            return _activity.SaveOrUpdateActivity(currentActivityDo);
        }

        public Task<ActivityDTO> Configure(IUnitOfWork uow, string userId, ActivityDO curActivityDO, bool saveResult = true)
        {
            return _activity.Configure(uow, userId, curActivityDO, saveResult);
        }

        public ActivityDO GetById(IUnitOfWork uow, Guid id)
        {
            return _activity.GetById(uow, id);
        }

        public ActivityDO MapFromDTO(ActivityDTO curActivityDTO)
        {
            return _activity.MapFromDTO(curActivityDTO);
        }

        public Task<PlanNodeDO> CreateAndConfigure(IUnitOfWork uow, string userId, int actionTemplateId, string label = null, int? order = null, Guid? parentNodeId = null, bool createPlan = false, Guid? authorizationTokenId = null)
        {
            return _activity.CreateAndConfigure(uow, userId, actionTemplateId, label, order, parentNodeId, createPlan, authorizationTokenId);
        }

        public Task<PayloadDTO> Run(IUnitOfWork uow, ActivityDO curActivityDO, ActivityExecutionMode curActionExecutionMode, ContainerDO curContainerDO)
        {
            ActivityMockBase activity;

            var payload = new PayloadDTO(curContainerDO.Id)
            {
                CrateStorage = CrateStorageSerializer.Default.ConvertToDto(CrateStorageSerializer.Default.ConvertFromDto(JsonConvert.DeserializeObject<CrateStorageDTO>(curContainerDO.CrateStorage)))
            };

            ExecutedActivities.Add(new ActivityExecutionCall(curActionExecutionMode, curActivityDO.Id));

            if (CustomActivities.TryGetValue(curActivityDO.Id, out activity))
            {
                activity.Run(curActivityDO.Id, curActionExecutionMode, payload);
            }

            return Task.FromResult(payload);
        }

        public Task<ActivityDTO> Activate(ActivityDO curActivityDO)
        {
            return _activity.Activate(curActivityDO);
        }

        public Task<ActivityDTO> Deactivate(ActivityDO curActivityDO)
        {
            return _activity.Deactivate(curActivityDO);
        }

        public Task<T> GetActivityDocumentation<T>(ActivityDTO curActivityDTO, bool isSolution = false)
        {
            return _activity.GetActivityDocumentation<T>(curActivityDTO, isSolution);
        }

        public List<string> GetSolutionList(string terminalName)
        {
            return _activity.GetSolutionList(terminalName);
        }

        public void Delete(Guid id)
        {
            _activity.Delete(id);
        }
    }

}
