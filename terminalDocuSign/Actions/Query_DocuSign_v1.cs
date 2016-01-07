using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using StructureMap;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Interfaces;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Query_DocuSign_v1  : BaseTerminalAction
    {
        public class ActionUi : StandardConfigurationControlsCM
        {
            [JsonIgnore]
            public TextBox SearchText { get; set; }
            
            [JsonIgnore]
            public DropDownList Folder { get; set; }

            [JsonIgnore]
            public DropDownList Status { get; set; }

            public ActionUi()
            {
                Controls = new List<ControlDefinitionDTO>();

                Controls.Add(new TextArea
                {
                    IsReadOnly = true,
                    Label = "",
                    Value = "<p>Search for DocuSign Envelopes where the following are true:</p>" +
                            "<div>Envelope contains text:</div>"
                });

                Controls.Add((SearchText = new TextBox
                {
                    Name = "SearchText",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                }));

                Controls.Add((Folder = new DropDownList
                {
                    Label = "Envelope is in folder:",
                    Name = "Folder",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                    Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "Folders")
                }));

                Controls.Add((Status = new DropDownList
                {
                    Label = "Envelope has status:",
                    Name = "Status",
                    Events = new List<ControlEvent> {ControlEvent.RequestConfig},
                    Source = new FieldSourceDTO(CrateManifestTypes.StandardDesignTimeFields, "Statuses")
                }));
            }
        }

        private readonly IDocuSignFolder _docuSignFolder;
        private readonly DocuSignManager _docuSignManager;

        public Query_DocuSign_v1()
        {
            _docuSignFolder = ObjectFactory.GetInstance<IDocuSignFolder>();
            _docuSignManager = ObjectFactory.GetInstance<DocuSignManager>();
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var payload = await GetProcessPayload(curActionDO, containerId);

            if (NeedsAuthentication(authTokenDO))
            {
                return NeedsAuthenticationError(payload);
            }

            var ui = Crate.GetStorage(curActionDO).CrateContentsOfType<StandardConfigurationControlsCM>().SingleOrDefault();

            if (ui == null)
            {
                return Error(payload, "Action was not configured correctly");
            }


            var settings = GetDocusignQuery(ui);
            
            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);
            var payloadCm = new StandardPayloadDataCM();
            var envelopes = _docuSignManager.SearchDocusign(docuSignAuthDto, settings);

            foreach (var envelope in envelopes)
            {
                var row = new PayloadObjectDTO();

                row.PayloadObject.Add(new FieldDTO("Id", envelope.EnvelopeId));
                row.PayloadObject.Add(new FieldDTO("Name", envelope.Name));
                row.PayloadObject.Add(new FieldDTO("Subject", envelope.Subject));
                row.PayloadObject.Add(new FieldDTO("Status", envelope.Status));
                row.PayloadObject.Add(new FieldDTO("OwnerName", envelope.OwnerName));
                row.PayloadObject.Add(new FieldDTO("SenderName", envelope.SenderName));
                row.PayloadObject.Add(new FieldDTO("SenderEmail", envelope.SenderEmail));
                row.PayloadObject.Add(new FieldDTO("Shared", envelope.Shared));
                row.PayloadObject.Add(new FieldDTO("CompletedDate", envelope.CompletedDateTime.ToString(CultureInfo.InvariantCulture)));
                row.PayloadObject.Add(new FieldDTO("CreatedDateTime", envelope.CreatedDateTime.ToString(CultureInfo.InvariantCulture)));

                payloadCm.PayloadObjects.Add(row);
            }
            
            using (var updater = Crate.UpdateStorage(payload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", payloadCm));
            }

            return Success(payload);
        }
        
        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            if (NeedsAuthentication(authTokenDO))
            {
                throw new ApplicationException("No AuthToken provided.");
            }

            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.AddRange(PackDesignTimeData(docuSignAuthDto));
            }
            
            return Task.FromResult(curActionDO);
        }


        private static DocusignQuery GetDocusignQuery(StandardConfigurationControlsCM ui)
        {
            var controls = new ActionUi();

            controls.ClonePropertiesFrom(ui);

            var settings = new DocusignQuery();

            settings.Folder = controls.Folder.Value;
            settings.Status = controls.Status.Value;
            settings.SearchText = controls.SearchText.Value;

            return settings;
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.RemoveByLabel("Queryable Criteria");
                
                return curActionDO;
            }
        }

        private IEnumerable<Crate> PackDesignTimeData(DocuSignAuth authDTO)
        {
            var folders = _docuSignFolder.GetFolders(authDTO.Email, authDTO.ApiPassword);
            var fields = new List<FieldDTO>();
            
            foreach (var folder in folders)
            {
                fields.Add(new FieldDTO(folder.Name, folder.FolderId));
            }

            yield return Data.Crates.Crate.FromContent("Folders", new StandardDesignTimeFieldsCM(fields));
            yield return Data.Crates.Crate.FromContent("Statuses", new StandardDesignTimeFieldsCM(DocusignQuery.Statuses));
        }

        public override ConfigurationRequestType ConfigurationEvaluator(ActionDO curActionDO)
        {
            if (Crate.IsStorageEmpty(curActionDO))
            {
                return ConfigurationRequestType.Initial;
            }

            return ConfigurationRequestType.Followup;
        }
    }
}