using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;
using Newtonsoft.Json;
using StructureMap;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalFr8Core.Actions
{
    public class StoreMTData_v1 : BasePluginAction
    {
        public async Task<ActionDTO> Configure(ActionDTO curActionDTO)
        {
            return await ProcessConfigurationRequest(curActionDTO, actionDTO => ConfigurationRequestType.Initial);
        }

        public Task<object> Activate(ActionDTO curActionDTO)
        {
            //No activation logic decided yet
            return null;
        }

        public Task<object> Deactivate(ActionDTO curDataPackage)
        {
            //No deactivation logic decided yet
            return null;
        }

        public async Task<PayloadDTO> Run(ActionDTO actionDto)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //get the process payload
                var curProcessPayload = await GetProcessPayload(actionDto.ProcessId);

                //get docu sign envelope crate from payload
                var curDocuSignEnvelopeCrate =
                    Crate.GetCratesByLabel("DocuSign Envelope Manifest", curProcessPayload.CrateStorageDTO()).Single();

                if (curDocuSignEnvelopeCrate != null)
                {
                    DocuSignEnvelopeCM docuSignEnvelope =
                        JsonConvert.DeserializeObject<DocuSignEnvelopeCM>(curDocuSignEnvelopeCrate.Contents);

                    //store envelope in MT database
                    uow.MultiTenantObjectRepository.AddOrUpdate<DocuSignEnvelopeCM>(uow, docuSignEnvelope,
                        e => e.EnvelopeId);
                    uow.SaveChanges();
                }

                //get docu sign event crate from payload
                var curDocuSignEventCrate =
                    Crate.GetCratesByLabel("DocuSign Event Manifest", curProcessPayload.CrateStorageDTO()).Single();

                if (curDocuSignEventCrate != null)
                {
                    DocuSignEventCM docuSignEvent =
                        JsonConvert.DeserializeObject<DocuSignEventCM>(curDocuSignEventCrate.Contents);

                    //store event in MT database
                    uow.MultiTenantObjectRepository.AddOrUpdate<DocuSignEventCM>(uow, docuSignEvent, e => e.EnvelopeId);
                    uow.SaveChanges();
                }

                return curProcessPayload;
            }
        }

        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            CrateDTO curMergedUpstreamRunTimeObjects =
                await MergeUpstreamFields(curActionDO.Id, "Available Run-Time Objects");

            var fieldSelectObjectTypes = new DropDownListControlDefinitionDTO()
            {
                Label = "Save Which Data Types?",
                Name = "Save Object Name",
                Required = true,
                Events = new List<ControlEvent>(),
                Source = new FieldSourceDTO
                {
                    Label = curMergedUpstreamRunTimeObjects.Label,
                    ManifestType = curMergedUpstreamRunTimeObjects.ManifestType
                }
            };

            var curConfigurationControlsCrate = PackControlsCrate(fieldSelectObjectTypes);

            FieldDTO[] curSelectedFields =
                JsonConvert.DeserializeObject<StandardDesignTimeFieldsCM>(curMergedUpstreamRunTimeObjects.Contents)
                    .Fields.Select(field => new FieldDTO {Key = field.Key, Value = field.Value})
                    .ToArray();

            var curSelectedObjectType = Crate.CreateDesignTimeFieldsCrate("SelectedObjectTypes", curSelectedFields);

            curActionDO.UpdateCrateStorageDTO(new List<CrateDTO>
            {
                curMergedUpstreamRunTimeObjects,
                curConfigurationControlsCrate,
                curSelectedObjectType
            });

            return Mapper.Map<ActionDTO>(curActionDO);
        }
    }
}