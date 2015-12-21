using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Data.Control;
using Data.Crates;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Hub.Managers;
using Newtonsoft.Json;
using terminalDocuSign.DataTransferObjects;
using terminalDocuSign.Infrastructure;
using terminalDocuSign.Services;
using TerminalBase.BaseClasses;
using TerminalBase.Infrastructure;

namespace terminalDocuSign.Actions
{
    public class Query_DocuSign_v1  : BaseTerminalAction
    {
        public class RuntimeConfiguration : Manifest
        {
            public string SearchText;
            public DateTime? FromDate;
            public DateTime? ToDate;
            public string Status;
            public string Folder;

            public RuntimeConfiguration()
                : base(new CrateManifestType("Query_DocuSign_v1_RuntimeConfiguration", 1000000 + 1))
            {
            }
        }        

        private class ActionUi : StandardConfigurationControlsCM
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

        private readonly DocuSignManager _docuSignManager;

        static Query_DocuSign_v1()
        {
            ManifestDiscovery.Default.RegisterManifest(typeof(RuntimeConfiguration));
        }

        public Query_DocuSign_v1()
        {
            _docuSignManager = new DocuSignManager();
        }

        public async Task<PayloadDTO> Run(ActionDO curActionDO, Guid containerId, AuthorizationTokenDO authTokenDO)
        {
            var configuration = Crate.GetStorage(curActionDO).CrateContentsOfType<RuntimeConfiguration>().SingleOrDefault();

            if (configuration == null)
            {
                throw new Exception("Action was not configured correctly");
            }

            var payload = await GetProcessPayload(curActionDO, containerId);
            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);
            var docusignFolder = new DocusignFolder();
            var payloadCm = new StandardPayloadDataCM();

            if (string.IsNullOrWhiteSpace(configuration.Folder) || configuration.Folder == "<any>")
            {
                foreach (var folder in docusignFolder.GetFolders(docuSignAuthDto.Email, docuSignAuthDto.ApiPassword))
                {
                    SearchFolder(configuration, docusignFolder, folder.FolderId, docuSignAuthDto, payloadCm);
                }
            }
            else
            {
                SearchFolder(configuration, docusignFolder, configuration.Folder, docuSignAuthDto, payloadCm);
            }

            using (var updater = Crate.UpdateStorage(payload))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Sql Query Result", payloadCm));
            }

            return payload;
        }

        private void SearchFolder(RuntimeConfiguration configuration, DocusignFolder docusignFolder, string folder, DocuSignAuth docuSignAuthDto, StandardPayloadDataCM payload)
        {
            var envelopes = docusignFolder.Search(docuSignAuthDto.Email, docuSignAuthDto.ApiPassword, configuration.SearchText, folder, configuration.Status == "<any>" ? null : configuration.Status, configuration.FromDate, configuration.ToDate);
            
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

                payload.PayloadObjects.Add(row);
            }
        }

        protected override Task<ActionDO> InitialConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            var docuSignAuthDto = JsonConvert.DeserializeObject<DocuSignAuth>(authTokenDO.Token);

            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                updater.CrateStorage.Add(Data.Crates.Crate.FromContent("Runtime Configuration", new RuntimeConfiguration()));
                updater.CrateStorage.Add(PackControls(new ActionUi()));
                updater.CrateStorage.AddRange(PackDesignTimeData(docuSignAuthDto));
            }
            
            return Task.FromResult(curActionDO);
        }

        protected override async Task<ActionDO> FollowupConfigurationResponse(ActionDO curActionDO, AuthorizationTokenDO authTokenDO)
        {
            using (var updater = Crate.UpdateStorage(curActionDO))
            {
                var ui = updater.CrateStorage.CrateContentsOfType<StandardConfigurationControlsCM>().FirstOrDefault();

                if (ui == null)
                {
                    updater.DiscardChanges();
                    return curActionDO;
                }

                var controls = new ActionUi();
               
                controls.ClonePropertiesFrom(ui);

                var config = updater.CrateStorage.CrateContentsOfType<RuntimeConfiguration>().First();

                config.Folder = controls.Folder.Value;
                config.Status = controls.Status.Value;
                config.SearchText = controls.SearchText.Value;
                
                updater.CrateStorage.RemoveByLabel("Queryable Criteria");
                
                return curActionDO;
            }
        }

        private IEnumerable<Crate> PackDesignTimeData(DocuSignAuth authDTO)
        {
            var docusignFolder = new DocusignFolder();
            var folders = docusignFolder.GetFolders(authDTO.Email, authDTO.ApiPassword);
            var fields = new List<FieldDTO>();
            
            foreach (var folder in folders)
            {
                fields.Add(new FieldDTO(folder.Name, folder.FolderId));
            }

            yield return Data.Crates.Crate.FromContent("Folders", new StandardDesignTimeFieldsCM(fields));


            yield return Data.Crates.Crate.FromContent("Statuses", new StandardDesignTimeFieldsCM(new[]
            {
                new FieldDTO("Any status", "<any>"),
                new FieldDTO("Sent", "sent"),
                new FieldDTO("Delivered", "delivered"),
                new FieldDTO("Signed", "signed"),
                new FieldDTO("Completed", "completed"),
                new FieldDTO("Declined", "declined"),
                new FieldDTO("Voided", "voided"),
                new FieldDTO("Timed Out", "timedout"),
                new FieldDTO("Authoritative Copy", "authoritativecopy"),
                new FieldDTO("Transfer Completed", "transfercompleted"),
                new FieldDTO("Template", "template"),
                new FieldDTO("Correct", "correct"),
                new FieldDTO("Created", "created"),
                new FieldDTO("Delivered", "delivered"),
                new FieldDTO("Signed", "signed"),
                new FieldDTO("Declined", "declined"),
                new FieldDTO("Completed", "completed"),
                new FieldDTO("Fax Pending", "faxpending"),
                new FieldDTO("Auto Responded", "autoresponded"),
            }));
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