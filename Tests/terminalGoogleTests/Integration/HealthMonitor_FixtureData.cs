using System;
using System.Collections.Generic;
using System.Linq;
using Hub.Managers;
using Fr8Data.Control;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using terminalGoogle.Actions;

namespace terminalGoogleTests.Integration
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
                Token = @"{""AccessToken"":""ya29.CjHXAnhqySXYWbq-JE3Nqpq18L_LGYw3xx_T-lD6jeQd6C2mMoKzQhTWRWFSkPcX-pH_"",""RefreshToken"":""1/ZmUihiXxjwiVd-kQe46hDXKB95VaHM5yP-6bfrS-EUUMEudVrK5jSpoR30zcRFq6"",""Expires"":""2017-11-28T13:29:12.653075+05:00""}"
            };
        }

        public static AuthorizationTokenDTO Google_AuthToken1()
        {
            return new AuthorizationTokenDTO()
            {
                Token = @"{""AccessToken"":""ya29.CjHXAnhqySXYWbq-JE3Nqpq18L_LGYw3xx_T-lD6jeQd6C2mMoKzQhTWRWFSkPcX-pH_"",""RefreshToken"":""1/ZmUihiXxjwiVd-kQe46hDXKB95VaHM5yP-6bfrS-EUUMEudVrK5jSpoR30zcRFq6"",""Expires"":""2017-11-28T13:29:12.653075+05:00""}"
            };
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

        public static ActivityTemplateDTO Monitor_Form_Responses_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Monitor_Form_Responses_TEST",
                Version = "1",
                Terminal = new TerminalDTO()
                {
                    AuthenticationType = AuthenticationType.External
                }
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
        private Crate PackCrate_ConfigurationControlsWithNoListItems()
        {
            var fieldSelectTemplate = new DropDownList()
            {
                Label = "Select Google Form",
                Name = "Selected_Google_Form",
                Required = true,
                Source = new FieldSourceDTO
                {
                    Label = "Available Forms",
                    ManifestType = CrateManifestTypes.StandardDesignTimeFields
                }
            };

            var controls = PackControlsCrate(fieldSelectTemplate);
            return controls;
        }

        public void ActivateCrateStorage(ActivityDTO curActivityDTO, Crate curCrate)
        {
            var configurationControlsCrate = curCrate;
            var crateDesignTimeFields = PackCrate_GoogleForms();
            var eventCrate = CreateEventSubscriptionCrate();

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDTO))
            {
                crateStorage.Add(configurationControlsCrate);
                crateStorage.Add(crateDesignTimeFields);
                crateStorage.Add(eventCrate);
            }
        }

        public static Fr8DataDTO Monitor_Form_Responses_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Monitor_Form_Responses_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Form Responses",
                AuthToken = Google_AuthToken1(),
                ActivityTemplate = activityTemplate
            };
            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public Fr8DataDTO Monitor_Form_Responses_v1_ActivateDeactivate_Fr8DataDTO()
        {
            var activityTemplate = Monitor_Form_Responses_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Form Responses",
                AuthToken = Google_AuthToken1(),
                ActivityTemplate = activityTemplate,
                ParentPlanNodeId = Guid.NewGuid()
            };

            ActivateCrateStorage(activity, PackCrate_ConfigurationControls());
            return new Fr8DataDTO { ActivityDTO = activity };
        }
        public Fr8DataDTO Monitor_Form_Responses_v1_Followup_Fr8DataDTO()
        {
            var activityTemplate = Monitor_Form_Responses_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Form Responses",
                AuthToken = Google_AuthToken1(),
                ActivityTemplate = activityTemplate,
                ParentPlanNodeId = Guid.NewGuid()
            };

            ActivateCrateStorage(activity, PackCrate_ConfigurationControlsWithNoListItems());
            return new Fr8DataDTO { ActivityDTO = activity };
        }
        private ICrateStorage WrapPayloadDataCrate(List<FieldDTO> payloadFields)
        {
            return new CrateStorage(Fr8Data.Crates.Crate.FromContent("Payload Data", new StandardPayloadDataCM(payloadFields)));
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

        public ActivityDTO Monitor_Form_Responses_v1_Run_ActivityDTO()
        {
            var activityTemplate = Monitor_Form_Responses_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Form Responses",
                AuthToken = Google_AuthToken1(),
                ActivityTemplate = activityTemplate
            };
            ActivateCrateStorage(activity, PackCrate_ConfigurationControls());
            using (var crateStorage = CrateManager.GetUpdatableStorage(activity))
            {
                crateStorage.Add(PayloadRaw());
            }
            return activity;
        }

        public ActivityDTO Monitor_Form_Responses_v1_Run_EmptyPayload()
        {
            var activityTemplate = Monitor_Form_Responses_v1_ActivityTemplate();

            var activity = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Monitor Form Responses",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate
            };
            ActivateCrateStorage(activity, PackCrate_ConfigurationControls());
            using (var crateStorage = CrateManager.GetUpdatableStorage(activity))
            {
                crateStorage.Add(PayloadEmptyRaw());
            }
            return activity;
        }

        public static ActivityTemplateDTO Get_Google_Sheet_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateDTO()
            {
                Id = Guid.NewGuid(),
                Name = "Get_Google_Sheet_Data_TEST",
                Version = "1"
            };
        }
        public static Fr8DataDTO Get_Google_Sheet_Data_v1_InitialConfiguration_Fr8DataDTO()
        {
            var activityTemplate = Get_Google_Sheet_Data_v1_ActivityTemplate();

            var activityDTO = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get Google Sheet Data",
                AuthToken = Google_AuthToken(),
                ActivityTemplate = activityTemplate
            };

            return new Fr8DataDTO { ActivityDTO = activityDTO };
        }

        public ActivityDTO Get_Google_Sheet_Data_v1_Followup_Configuration_Request_ActivityDTO_With_Crates()
        {

            var activityTemplate = Get_Google_Sheet_Data_v1_ActivityTemplate();

            var curActivityDto = new ActivityDTO()
            {
                Id = Guid.NewGuid(),
                Label = "Get Google Sheet Data",
                AuthToken = Google_AuthToken1(),
                ActivityTemplate = activityTemplate
            };

            return curActivityDto;

        }
        public Crate PackCrate_GoogleSpreadsheets()
        {
            Crate crate;

            var curFields = new List<FieldDTO>()
            {
                new FieldDTO
                {
                    Key = "Column_Only",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1o0cle_rnfVtmeLqDDeF40dRWKL6FSCuQz5E84pcCpTs"
                },
                new FieldDTO
                {
                    Key = "Row_Only",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1pzbssu5vuCqv5LMTdIQ7SCqVFaQR0_d7MnB7oGonzf0"
                },
                new FieldDTO
                {
                    Key = "Row_And_Column",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1zG93EWaycPyCdM9OJf03C2knK9Neu09OutAl2p7NZbw"
                },
                new FieldDTO
                {
                    Key="Empty_First_Row",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1Nzf_s2OyZTxG8ppxzvypH6s1ePvUT_ALPffZchuM14o"
                },
                new FieldDTO
                {
                    Key="OneRow_WithHeader",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1XES9LEK6WmSp5adZ8F-_cfoE7EeLMgPr6NhRPyGaSfM"
                }
            }.ToArray();
            crate = CrateManager.CreateDesignTimeFieldsCrate("Select a Google Spreadsheet", curFields);

            return crate;
        }

        private Crate Get_Google_Sheet_Data_v1_PackCrate_ConfigurationControls(Tuple<string, string> spreadsheetTuple)
        {
            var activityUi = new Get_Google_Sheet_Data_v1.ActivityUi();
            activityUi.SpreadsheetList.ListItems = new[] { new ListItem { Key = spreadsheetTuple.Item1, Value = spreadsheetTuple.Item2 } }.ToList();
            activityUi.SpreadsheetList.selectedKey = spreadsheetTuple.Item1;
            activityUi.SpreadsheetList.Value = spreadsheetTuple.Item2;
            return PackControlsCrate(activityUi.Controls.ToArray());
        }

        public void Get_Google_Sheet_Data_v1_AddPayload(ActivityDTO activityDTO, string spreadsheet)
        {
            var caseTuple = CaseTuple(spreadsheet);
            var configurationControlsCrate = Get_Google_Sheet_Data_v1_PackCrate_ConfigurationControls(caseTuple);
            using (var crateStorage = CrateManager.GetUpdatableStorage(activityDTO))
            {
                crateStorage.Add(configurationControlsCrate);
            }
        }

        public Tuple<string, string> CaseTuple(string spreadsheet)
        {
            switch (spreadsheet)
            {
                case "Row_And_Column":
                    return new Tuple<string, string>("Row_And_Column", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1RgskCZyY_lKj5DsweJvut5wDVHVXcmxj90LPxUuwivA");
                case "Row_Only":
                    return new Tuple<string, string>("Row_Only", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1KjUeDKo-1TmI8w5pjXWhy_uBZb3mEtVW1fSF_ytrHJI");
                case "Column_Only":
                    return new Tuple<string, string>("Column_Only", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1K4SbUWSd5TYrF2Uk5P9n1bxuBse1dquFqrPiUsWOvqI");
                case "Empty_First_Row":
                    return new Tuple<string, string>("Empty_First_Row", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1Nzf_s2OyZTxG8ppxzvypH6s1ePvUT_ALPffZchuM14o");
                case "OneRow_WithHeader":
                    return new Tuple<string, string>("OneRow_WithHeader", "https://spreadsheets.google.com/feeds/spreadsheets/private/full/1XES9LEK6WmSp5adZ8F-_cfoE7EeLMgPr6NhRPyGaSfM");
                default:
                    return null;
            }
        }

    }
}
