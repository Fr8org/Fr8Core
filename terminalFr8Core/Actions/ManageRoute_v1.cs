using System;
using System.Threading.Tasks;
using Data.Crates;
<<<<<<< HEAD
using Data.Entities;
=======
>>>>>>> dev
using Data.Interfaces.DataTransferObjects;
using Hub.Managers;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using terminalFr8Core.Infrastructure;

namespace terminalFr8Core.Actions
{
<<<<<<< HEAD
    public class ManageRoute_v1 : BaseTerminalAction
=======
    public class ManageRoute_v1 : BasePluginAction
>>>>>>> dev
    {
        private readonly FindObjectHelper _findObjectHelper = new FindObjectHelper();


        #region Configuration.

<<<<<<< HEAD
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
=======
        public override ConfigurationRequestType ConfigurationEvaluator(ActionDTO curActionDTO)
        {
            if (curActionDTO.CrateStorage == null 
                || curActionDTO.CrateStorage.Crates == null
                || curActionDTO.CrateStorage.Crates.Length == 0)
>>>>>>> dev
            {
                return ConfigurationRequestType.Initial;
            }
            else
            {
                return ConfigurationRequestType.Followup;
            }
        }

<<<<<<< HEAD
        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO=null)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
=======
        protected override Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            using (var updater = Crate.UpdateStorage(curActionDTO))
>>>>>>> dev
            {
                var crateStorage = updater.CrateStorage;
                AddRunNowButton(crateStorage);
            }

<<<<<<< HEAD
            return Task.FromResult(curActionDO);
=======
            return Task.FromResult(curActionDTO);
>>>>>>> dev
        }

        private void AddRunNowButton(CrateStorage crateStorage)
        {
            AddControl(
                crateStorage,
                new ControlDefinitionDTO(ControlTypes.ManageRoute)
                {
                    Name = "ManageRoute",
                    Label = "Manage Route"
                }
            );
        }

        #endregion Configuration.


        #region Execution.

<<<<<<< HEAD
        public Task<PayloadDTO> Run(ActionDO curActionDTO, int containerId, AuthorizationTokenDO authTokenDO=null)
=======
        public Task<PayloadDTO> Run(ActionDTO curActionDTO)
>>>>>>> dev
        {
            return Task.FromResult<PayloadDTO>(null);
        }

        #endregion Execution.
    }
}