using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using terminalGoogle.Activities;
using System.Configuration;

namespace terminalGoogleTests.Integration
{
    public class HealthMonitor_FixtureData
    {

        protected ICrateManager CrateManager;
        public HealthMonitor_FixtureData()
        {

            CrateManager = new CrateManager();
        }

        private static string GoogleTestAccountToken 
        {
            get
            {
                return ConfigurationManager.AppSettings["GoogleTestAccountToken"];
            }
        }

        private static string VolunteerFormId
        {
            get
            {
                return ConfigurationManager.AppSettings["VolunteerFormId"];
            }
        }

        private static string GoogleAccount
        {
            get
            {
                return ConfigurationManager.AppSettings["GoogleAccount"];
            }
        }

        public static AuthorizationTokenDTO Google_AuthToken()
        {
            return new AuthorizationTokenDTO()
            {
                Token = GoogleTestAccountToken
            };
        }

        public static AuthorizationTokenDTO Google_AuthToken1()
        {
            return new AuthorizationTokenDTO()
            {
                Token = GoogleTestAccountToken
            };
        }

        protected Crate<StandardConfigurationControlsCM> PackControlsCrate(params ControlDefinitionDTO[] controlsList)
        {
            return Crate<StandardConfigurationControlsCM>.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(controlsList));
        }

        private Crate PackCrate_GoogleForms()
        {
           return Crate.FromContent("Available Forms", new KeyValueListCM(new KeyValueDTO("Volunteer Sign Up Form (Team Rubicon)", VolunteerFormId)));
        }

        public static ActivityTemplateSummaryDTO Monitor_Form_Responses_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Monitor_Form_Responses_TEST",
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
                selectedKey = "Volunteer Sign Up Form (Team Rubicon)",
                Value = VolunteerFormId,
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

            using (var crateStorage = CrateManager.GetUpdatableStorage(curActivityDTO))
            {
                crateStorage.Add(configurationControlsCrate);
                crateStorage.Add(crateDesignTimeFields);
                crateStorage.Add("Standard Event Subscriptions", new EventSubscriptionCM("Google", "Google Form Response"));
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
        private ICrateStorage WrapPayloadDataCrate(List<KeyValueDTO> payloadFields)
        {
            return new CrateStorage(Crate.FromContent("Payload Data", new StandardPayloadDataCM(payloadFields)));
        }

        private Crate PayloadRaw()
        {
            List<KeyValueDTO> payloadFields = new List<KeyValueDTO>();
            payloadFields.Add(new KeyValueDTO() { Key = "user_id", Value = GoogleAccount });
            payloadFields.Add(new KeyValueDTO() { Key = "response", Value = "What is your pets name=cat&What is your favorite book?=book&Who is your favorite superhero?=hero&" });
            var eventReportContent = new EventReportCM
            {
                EventNames = "Google Form Response",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = GoogleAccount,
                Manufacturer = "Google"
            };

            //prepare the event report
            var curEventReport = Crate.FromContent("Standard Event Report", eventReportContent);
            return curEventReport;
        }

        private Crate PayloadEmptyRaw()
        {
            List<KeyValueDTO> payloadFields = new List<KeyValueDTO>();
            var eventReportContent = new EventReportCM
            {
                EventNames = "Google Form Response",
                EventPayload = WrapPayloadDataCrate(payloadFields),
                ExternalAccountId = GoogleAccount,
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

        public static ActivityTemplateSummaryDTO Get_Google_Sheet_Data_v1_ActivityTemplate()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Get_Google_Sheet_Data_TEST",
                Version = "1",
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

            var curFields = new KeyValueListCM(
                new KeyValueDTO
                {
                    Key = "Column_Only",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1o0cle_rnfVtmeLqDDeF40dRWKL6FSCuQz5E84pcCpTs"
                },
                new KeyValueDTO
                {
                    Key = "Row_Only",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1pzbssu5vuCqv5LMTdIQ7SCqVFaQR0_d7MnB7oGonzf0"
                },
                new KeyValueDTO
                {
                    Key = "Row_And_Column",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1zG93EWaycPyCdM9OJf03C2knK9Neu09OutAl2p7NZbw"
                },
                new KeyValueDTO
                {
                    Key="Empty_First_Row",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1Nzf_s2OyZTxG8ppxzvypH6s1ePvUT_ALPffZchuM14o"
                },
                new KeyValueDTO
                {
                    Key="OneRow_WithHeader",
                    Value = @"https://spreadsheets.google.com/feeds/spreadsheets/private/full/1XES9LEK6WmSp5adZ8F-_cfoE7EeLMgPr6NhRPyGaSfM"
                }
            );

            return Crate.FromContent("Select a Google Spreadsheet", curFields);
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
