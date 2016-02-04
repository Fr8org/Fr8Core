using System;
using Data.Interfaces.DataTransferObjects;
using Data.Entities;
using Data.Crates;
using Data.Control;
using Data.Interfaces.Manifests;
using System.Collections.Generic;
using Hub.Managers;
using System.Linq;
using System.Runtime.InteropServices;

namespace terminalGoogleTests.Unit
{
    public class HealthMonitor_FixtureData
    {

        protected ICrateManager CrateManager;
        public HealthMonitor_FixtureData()
        {

            CrateManager = new CrateManager();
        }

        public static AuthorizationTokenDTO Google_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{""AccessToken"":""ya29.PwJez2aHwjGxsxcho6TfaFseWjPbi1ThgINsgiawOKLlzyIgFJHkRdq76YrnuiGT3jhr"",""RefreshToken"":""1/HVhoZXzxFrPyC0JVlbEIF_VOBDm_IhrKoLKnt6QpyFRIgOrJDtdun6zK6XiATCKT"",""Expires"":""2015-12-03T11:12:43.0496208+08:00""}"
            };
        }
        public static AuthorizationTokenDTO Google_AuthToken1()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{""AccessToken"":""ya29.OgLf-SvZTHJcdN9tIeNEjsuhIPR4b7KBoxNOuELd0T4qFYEa001kslf31Lme9OQCl6S5"",""RefreshToken"":""1/04H9hNCEo4vfX0nHHEdViZKz1CtesK8ByZ_TOikwVDc"",""Expires"":""2015-11-28T13:29:12.653075+05:00""}"
            };
        }
        protected Crate PackControls(StandardConfigurationControlsCM page)
        {
            return PackControlsCrate(page.Controls.ToArray());
        }

        protected Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(controlsList));
        }

        private Crate PackCrate_GoogleForms()
        {
            Crate crate;

            var curFields = new List<FieldDTO>() { new FieldDTO() { Key = "Survey Form", Value = "1z7mIQdHeFIpxBm92sIFB52B7SwyEO3IT5LiUcmojzn8" } }.ToArray();
            crate = CrateManager.CreateDesignTimeFieldsCrate("Available Forms", curFields);

            return crate;
        }

        private Crate CreateEventSubscriptionCrate()
        {
            var subscriptions = new string[] {
                "Google Form Response"
            };

            return CrateManager.CreateStandardEventSubscriptionsCrate(
                "Standard Event Subscriptions",
                "Google",
                subscriptions.ToArray()
                );
        }

        public static ActivityTemplateDTO Receive_Google_Form_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Receive_Google_Form_TEST",
                Version = "1"
            };
        }

        private Crate PackCrate_ConfigurationControls()
        {
            var fieldSelectTemplate = new DropDownList()
            {
                Label = "Select Google Form",
                Name = "Selected_Google_Form",
                Required = true,
                selectedKey = "Survey Form",
                Value = "1z7mIQdHeFIpxBm92sIFB52B7SwyEO3IT5LiUcmojzn8",
                Source = new FieldSourceDTO
                {
                    Label = "Available Forms",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            var controls = PackControlsCrate(fieldSelectTemplate);
            return controls;
        }

        public void ActivateCrateStorage(ActivityDTO curActivityDO)
        {
            var configurationControlsCrate = PackCrate_ConfigurationControls();
            var crateDesignTimeFields = PackCrate_GoogleForms();
            var eventCrate = CreateEventSubscriptionCrate();

            using (var updater = CrateManager.UpdateStorage(curActivityDO))
            {
                updater.CrateStorage.Add(configurationControlsCrate);
                updater.CrateStorage.Add(crateDesignTimeFields);
                updater.CrateStorage.Add(eventCrate);
            }
        }

        public static Fr8DataDTO Receive_Google_Form_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Receive_Google_Form_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Receive_Google_Form",
                Label = "Receive Google Form Response",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public Fr8DataDTO Receive_Google_Form_v1_ActivateDeactivate_Fr8DataDTO()
        {
            var activityTemplate = Receive_Google_Form_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Receive_Google_Form",
                Label = "Receive Google Form Response",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id,
                ParentRouteNodeId = Guid.NewGuid(),
            };

            ActivateCrateStorage(activity);
            return new Fr8DataDTO { ActivityDTO = activity };
        }

        private CrateStorage WrapPayloadDataCrate(List<FieldDTO> payloadFields)
        {
            return new CrateStorage(Data.Crates.Crate.FromContent("Payload Data", new StandardPayloadDataCM(payloadFields)));
        }

        private Crate PayloadRaw()
        {
            List<FieldDTO> payloadFields = new List<FieldDTO>();
            payloadFields.Add(new FieldDTO() { Key = "user_id", Value = "g_admin@dockyard.company" });
            payloadFields.Add(new FieldDTO() { Key = "response", Value = "What is your pets name=cat&What is your favorite book?=book&Who is your favorite superhero?=hero&" });
            var eventReportContent = new EventReportCM
            {
                EventNames = "Google Form Response",
                ContainerDoId = "",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = "g_admin@dockyard.company",
                Manufacturer = "Google"
            };

            //prepare the event report
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);
            return curEventReport;
        }

        private Crate PayloadEmptyRaw()
        {
            List<FieldDTO> payloadFields = new List<FieldDTO>();
            var eventReportContent = new EventReportCM
            {
                EventNames = "Google Form Response",
                ContainerDoId = "",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = "g_admin@dockyard.company",
                Manufacturer = "Google"
            };

            //prepare the event report
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);
            return curEventReport;
        }

        public ActivityDTO Receive_Google_Form_v1_Run_ActionDTO()
        {
            var activityTemplate = Receive_Google_Form_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Receive_Google_Form",
                Label = "Receive Google Form Response",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
            using (var updater = CrateManager.UpdateStorage(activity))
            {
                updater.CrateStorage.Add(PayloadRaw());
            }
            return activity;
        }

        public ActivityDTO Receive_Google_Form_v1_Run_EmptyPayload()
        {
            var activityTemplate = Receive_Google_Form_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Receive_Google_Form",
                Label = "Receive Google Form Response",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };
            using (var updater = CrateManager.UpdateStorage(activity))
            {
                updater.CrateStorage.Add(PayloadEmptyRaw());
            }
            return activity;
        }

        public static ActivityTemplateDTO Extract_Spreadsheet_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = 1,
                Name = "Get_Google_Sheet_Data_TEST",
                Version = "1"
            };
        }
        public static Fr8DataDTO Extract_Spreadsheet_Data_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Extract_Spreadsheet_Data_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Get_Google_Sheet_Data",
                Label = "Get Google Sheet Data",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public ActivityDTO Extract_Spreadsheet_Data_v1_Followup_Configuration_Request_ActionDTO_With_Crates()
        {

            var activityTemplate = Extract_Spreadsheet_Data_v1_ActivityTemplate();

            var curActivityDto = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Get_Google_Sheet_Data",
                Label = "Get Google Sheet Data",
                AuthToken = Google_AuthToken1(),
                ActivityTemplate = activityTemplate,
                ActivityTemplateId = activityTemplate.Id,
            };
            
            return curActivityDto;

        }
        public Crate PackCrate_GoogleSpreadsheets()
        {
            Crate crate;

            var curFields = new List<FieldDTO>()
            {
                new FieldDTO() { Key = "Column_Only", Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1L2TxytQKnYLtHlB3fZ4lb91FKSmmoFk6FJipuDW0gWo" },
                new FieldDTO() { Key = "Row_Only", Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/126yxCJDSZHJoR6d8BYk0wW7tZpl2pcl29F8QXIYVGMQ"},
                new FieldDTO() {Key = "Row_And_Column", Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1v67fCdV9NItrKRgLHPlp3CS2ia9duUkwKQOAUcQciJ0"},
                new FieldDTO(){Key="Empty_First_Row", Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1Nzf_s2OyZTxG8ppxzvypH6s1ePvUT_ALPffZchuM14o"}
            }.ToArray();
            crate = CrateManager.CreateDesignTimeFieldsCrate("Select a Google Spreadsheet", curFields);

            return crate;
        }
        public StandardFileDescriptionCM GetUpstreamCrate()
        {
            return new StandardFileDescriptionCM
            {
                DockyardStorageUrl = "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1L2TxytQKnYLtHlB3fZ4lb91FKSmmoFk6FJipuDW0gWo",
                Filename = "Column_Only",
                Filetype = "Google_Spreadsheet"
            };
        }
        private Crate Extract_Spreadsheet_Data_v1_PackCrate_ConfigurationControls(Tuple<string, string> spreadsheetTuple)
        {
            var controlList = new List<ControlDefinitionDTO>();
            var spreadsheetControl = new DropDownList()
            {
                Label = "Select a Google Spreadsheet",
                Name = "select_spreadsheet",
                selectedKey = spreadsheetTuple.Item1,
                Value = spreadsheetTuple.Item2,
                Selected = true,
                Source = new FieldSourceDTO
                {
                    Label = "Select a Google Spreadsheet",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                },
            };
            controlList.Add(spreadsheetControl);
            return PackControlsCrate(controlList.ToArray());
        }

        public void Extract_Spreadsheet_Data_v1_AddPayload(ActivityDTO activityDTO, string spreadsheet)
        {
            var caseTuple = CaseTuple(spreadsheet);
            var configurationControlsCrate = Extract_Spreadsheet_Data_v1_PackCrate_ConfigurationControls(caseTuple);
            var crateDesignTimeFields = PackCrate_GoogleSpreadsheets();
            using (var updater = CrateManager.UpdateStorage(activityDTO))
            {
                updater.CrateStorage.Add(configurationControlsCrate);
                updater.CrateStorage.Add(crateDesignTimeFields);
            }
        }

        public Tuple<string, string> CaseTuple(string spreadsheet)
        {
            switch (spreadsheet)
            {
                case "Row_And_Column":
                    return new Tuple<string, string>("Row_And_Column", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1v67fCdV9NItrKRgLHPlp3CS2ia9duUkwKQOAUcQciJ0");
                case "Row_Only":
                    return new Tuple<string, string>("Row_Only", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/126yxCJDSZHJoR6d8BYk0wW7tZpl2pcl29F8QXIYVGMQ");
                case "Column_Only":
                    return new Tuple<string, string>("Column_Only", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1L2TxytQKnYLtHlB3fZ4lb91FKSmmoFk6FJipuDW0gWo");
                case "Empty_First_Row":
                    return new Tuple<string, string>("Empty_First_Row", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1Nzf_s2OyZTxG8ppxzvypH6s1ePvUT_ALPffZchuM14o");
                default:
                    return null;
            }
        }

    }
}
