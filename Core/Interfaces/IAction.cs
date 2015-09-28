using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();
        ActionDO SaveOrUpdateAction(ActionDO currentActionDo);
        ActionDTO Configure(ActionDO curActionDO);
        ActionDO GetById(int id);
        void Delete(int id);
        ActionDO MapFromDTO(ActionDTO curActionDTO);
        Task<int> PrepareToExecute(ActionDO curAction, ProcessDO curProcessDO, IUnitOfWork uow);
        string Authenticate(ActionDO curActionDO);
        void AddCrate(ActionDO curActionDO, List<CrateDTO> curCrateDTOLists);
        List<CrateDTO> GetCrates(ActionDO curActionDO);
        string Activate(ActionDO curActionDO);
        string Deactivate(ActionDO curActionDO);
        IEnumerable<CrateDTO> GetCratesByManifestType(string curManifestType, CrateStorageDTO curCrateStorageDTO);
        ActivityDO UpdateCurrentActivity(int curActionId, IUnitOfWork uow);
    }
}