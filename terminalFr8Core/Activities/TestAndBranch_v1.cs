using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using AutoMapper;
using Data.Constants;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Managers;
using TerminalBase;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;
using Utilities;

namespace terminalFr8Core.Actions
{
    public class TestAndBranch_v1 : BaseTerminalActivity
    {
        public async Task<PayloadDTO> Run(ActivityDO curActivityDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var curPayloadDTO = await GetPayload(curActivityDO, containerId);
            return Success(curPayloadDTO);
        }

        public override async Task<ActivityDO> Configure(ActivityDO curActionDataPackageDO, AuthorizationTokenDO authTokenDO)
        {
            return await ProcessConfigurationRequest(curActionDataPackageDO, ConfigurationEvaluator, authTokenDO);
        }

        protected override async Task<ActivityDO> InitialConfigurationResponse(ActivityDO curActivityDO, AuthorizationTokenDO authTokenDO)
        {
            var curUpstreamFields =
                (await GetDesignTimeFields(curActivityDO, CrateDirection.Upstream))
                .Fields
                .ToArray();

            var queryFieldsCrate = Crate.FromContent(
                "Queryable Criteria",
                new StandardQueryFieldsCM(
                    curUpstreamFields.Select(
                        x => new QueryFieldDTO(
                            x.Key,
                            x.Key,
                            QueryFieldType.String,
                            new TextBox()
                            {
                                Name = "QueryField_" + x.Key
                            }
                        )
                    )
                )
            );


            //build a controls crate to render the pane
            var configurationControlsCrate = CreateControlsCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDO))
            {
                crateStorage.Replace(AssembleCrateStorage(configurationControlsCrate));
                crateStorage.Add(queryFieldsCrate);
            }

            return curActivityDO;
        }

        private Crate CreateControlsCrate()
        {
            var transition = new ContainerTransition
            {
                Label = "Please enter transition",
                Name = "transition"
            };


            return PackControlsCrate(transition);
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActivityDO curActivityDO)
        {
            if (CrateManager.IsStorageEmpty(curActivityDO))
            {
                return ConfigurationRequestType.Initial;
            }

            var controlsMS = CrateManager.GetStorage(curActivityDO).CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

            if (controlsMS == null)
            {
                return ConfigurationRequestType.Initial;
            }

            var durationControl = controlsMS.Controls.FirstOrDefault(x => x.Type == ControlTypes.ContainerTransition && x.Name == "transition");

            if (durationControl == null)
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}