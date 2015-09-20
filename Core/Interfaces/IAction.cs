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
        
        CrateStorageDTO Configure(ActionDO curActionDO);

        IEnumerable<string> GetFieldDataSources(IUnitOfWork uow, ActionDO curActionDO);

        ActionDO GetById(int id);

        void Delete(int id);

        Task<int> Process(ActionDO curAction, ProcessDO curProcessDO);

        string Authenticate(ActionDO curActionDO);
        void AddCrate(ActionDO curActionDO, List<CrateDTO> curCrateDTOLists);
        List<CrateDTO> GetCrates(ActionDO curActionDO);
        IEnumerable<CrateDTO> GetCratesByManifestType(string curManifestType, CrateStorageDTO curCrateStorageDTO);
    }
}