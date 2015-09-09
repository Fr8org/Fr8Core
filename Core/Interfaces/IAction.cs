using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
    public interface IAction
    {
        IEnumerable<TViewModel> GetAllActions<TViewModel>();

        //IEnumerable<ActivityTemplateDO> GetAvailableActions(IDockyardAccountDO curAccount);

        bool SaveOrUpdateAction(ActionDO currentActionDo);
        
        string GetConfigurationSettings(ActionDO curActionDO);

        IEnumerable<string> GetFieldDataSources(IUnitOfWork uow, ActionDO curActionDO);

        ActionDO GetById(int id);

        void Delete(int id);

        Task<int> Process(ActionDO curAction);

        string Authenticate(ActionDO curActionDO);
    }
}