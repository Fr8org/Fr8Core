using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Interfaces;
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
                var curDocuSignEnvelopeCrate = Crate.GetStorage(curProcessPayload.CrateStorage).CratesOfType<DocuSignEnvelopeCM>().Single(x => x.Label == "DocuSign Envelope Manifest");
                    
                string curFr8AccountId = string.Empty;
                if (curDocuSignEnvelopeCrate != null)
                {
                    DocuSignEnvelopeCM docuSignEnvelope = curDocuSignEnvelopeCrate.Value;
                    curFr8AccountId =
                        uow.AuthorizationTokenRepository.GetQuery()
                            .Single(auth => auth.ExternalAccountId.Equals(docuSignEnvelope.ExternalAccountId))
                            .UserDO.Id;

                    //store envelope in MT database
                    uow.MultiTenantObjectRepository.AddOrUpdate<DocuSignEnvelopeCM>(uow, curFr8AccountId, docuSignEnvelope, e => e.EnvelopeId);
                    uow.SaveChanges();
                }

                //get docu sign event crate from payload
                var curDocuSignEventCrate = Crate.GetStorage(curProcessPayload.CrateStorage).CratesOfType<DocuSignEventCM>().Single(x => x.Label == "DocuSign Event Manifest");

                if (curDocuSignEventCrate != null)
                {
                    DocuSignEventCM docuSignEvent = curDocuSignEventCrate.Value;

                    curFr8AccountId =
                        uow.AuthorizationTokenRepository.GetQuery()
                            .Single(auth => auth.ExternalAccountId.Equals(docuSignEvent.ExternalAccountId))
                            .UserDO.Id;

                    //store event in MT database
                    uow.MultiTenantObjectRepository.AddOrUpdate<DocuSignEventCM>(uow, curFr8AccountId, docuSignEvent, e => e.EnvelopeId);
                    uow.SaveChanges();
                }

                return curProcessPayload;
            }
        }

        
        protected override async Task<ActionDTO> InitialConfigurationResponse(ActionDTO curActionDTO)
        {
            ActionDO curActionDO = Mapper.Map<ActionDO>(curActionDTO);

            var curMergedUpstreamRunTimeObjects = await MergeUpstreamFields(curActionDO.Id, "Available Run-Time Objects");

            var fieldSelectObjectTypes = new DropDownListControlDefinitionDTO()
            {
                Label = "Save Which Data Types?",
                Name = "Save Object Name",
                Required = true,
                Events = new List<ControlEvent>(),
                Source = new FieldSourceDTO
                {
                    Label = curMergedUpstreamRunTimeObjects.Label,
                    ManifestType = curMergedUpstreamRunTimeObjects.ManifestType.Type
                }
            };

            var curConfigurationControlsCrate = PackControlsCrate(fieldSelectObjectTypes);

            FieldDTO[] curSelectedFields = curMergedUpstreamRunTimeObjects.Value.Fields.Select(field => new FieldDTO {Key = field.Key, Value = field.Value}).ToArray();

            var curSelectedObjectType = Crate.CreateDesignTimeFieldsCrate("SelectedObjectTypes", curSelectedFields);

            using (var updater = Crate.UpdateStorage(() => curActionDO.CrateStorage))
            {
                updater.CrateStorage.Add(curMergedUpstreamRunTimeObjects);
                updater.CrateStorage.Add(curConfigurationControlsCrate);
                updater.CrateStorage.Add(curSelectedObjectType);
            }

            return Mapper.Map<ActionDTO>(curActionDO);
        }
    }
}