using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;
using StructureMap;
using System.Collections.Generic;
using System;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActionDO TestActionWriteSqlServer1()
        {
            var actionTemplate = ActionTemplate();

            var curActionDO = new ActionDO
            {
                Id = 54,
                Name = "Write to Sql Server",
                CrateStorage = "",
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActivityTemplateDO ActionTemplate()
        {
            return new ActivityTemplateDO()
            {
                Id = 1,
                Name = "Send an Email",
                Plugin = new PluginDO { Name = "Send an Email", Version = "1", Endpoint = "", PluginStatus = PluginStatus.Active },
                
                Version = "1"
            };
        }

        public static ActivityTemplateDO ActivityTemplateSMS()
        {
            return new ActivityTemplateDO()
            {
                Id = 1,
                Name = "Send a Text (SMS) Message",
                Plugin = new PluginDO { Name = "Send a Text (SMS) Message", Version = "1", Endpoint = "", PluginStatus = PluginStatus.Active },
                Version = "1"
            };
        }        
        public static ActionDO TestAction1()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Name = "Action 1",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
            return curActionDO;
        }

        public static ActionDO TestAction2()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Id = 2,
                Name = "Action 2",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public ActionDO TestAction3()
        {
            var actionTemplate = ActionTemplate();
            var origActionDO = new ActionDO()
            {
                ParentActivityId = null,
                Name = "type 1",
                Id = 34,
                CrateStorage = "config settings",
              
                Ordering = 3,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };

            return origActionDO;
        }


        public static ActionDO TestAction4()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Id = 3,
                Name = "Send an Email",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActionDO TestAction5()
        {
            var actionTemplate = ActivityTemplateSMS();
            var curActionDO = new ActionDO
            {
                Id = 4,
                Name = "Send a Text (SMS) Message",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActionDO TestAction6()
        {
            var actionTemplate = ActionTemplate();
            actionTemplate.Name = null;

            return new ActionDO
            {
                Id = 6,
                ParentActivityId = 1,
                Ordering = 2,
                ActionState = ActionState.Unstarted,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction7()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 7,
                Name = "Action 7",
                ParentActivityId = 1,
                Ordering = 3,
                ActionState = ActionState.Unstarted,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction8()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 8,
                Name = "Action 8",
                ParentActivityId = 1,
                Ordering = 4,
                ActionState = ActionState.Unstarted,
                ParentActivity = FixtureData.TestActionList6(),
               
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction10()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 5,
                Name = "Action 5",
                ParentActivityId = 1,
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction9()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 2,
                ActionState = ActionState.Error,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }


        public static ActionDO TestAction20()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 1,
                Name = "Action 1",
                ParentActivityId = 1,
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction21()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 2,
                Name = "Action 2",
                ParentActivityId = 1,
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction22()
        {
            var actionTemplate = FixtureData.ActionTemplate();

            return new ActionDO
            {
                Id = 10,
                Name = "WriteToAzureSql",
                ParentActivityId = 1,
                CrateStorage = "JSON Config Settings",
                
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate

            };
        }

        public static ActionDO TestAction23()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActionDO
            {
                Id = 2,
                Name = "Action 2",
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO IntegrationTestAction()
        {
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var actionTemplate = ActionTemplate();

            var processDo = new ProcessDO()
            {
                Id = 1,
                CrateStorage = EnvelopeIdCrateJson(),
                ProcessTemplateId = TestProcessTemplate2().Id,
                ProcessState = 1
            };

            var actionListDo = new ActionListDO()
            {
                Process = processDo,
                ProcessID = ProcessState.Unstarted,
                Id = 1,
                ActionListType = ActionListType.Immediate
            };

            var actionDo = new ActionDO()
            {
                ParentActivity = actionListDo,
                ParentActivityId = 1,
                ActionState = ActionState.Unstarted,
                Name = "testaction",
              
                Id = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = EnvelopeIdCrateJson()
            };

            return actionDo;
        }
        public static CrateDTO GetEnvelopeIdCrate(string curEnvelopeId = "11f41f43-57bd-4568-86f5-9ceabdaafc43")
        {
            var crateFields = new List<FieldDTO>()
                    {
                        new FieldDTO() { Key = "EnvelopeId", Value = curEnvelopeId },
                        new FieldDTO() { Key = "ExternalEventType", Value = "1" },
                        new FieldDTO() { Key = "RecipientId", Value= "1" }
                    };
            var curEventData = new CrateDTO()
            {
                Contents = JsonConvert.SerializeObject(crateFields),
                Label = "Event Data",
                Id = Guid.NewGuid().ToString()
            };

            return curEventData;
        }

        public static string EnvelopeIdCrateJson()
        {
            return JsonConvert.SerializeObject(GetEnvelopeIdCrate());
        }

        public static ActionDO TestActionHealth1()
        {
            var actionDo = new ActionDO
            {
                Id = 1,
               
                ActionState = ActionState.Unstarted,
                Name = "testaction",
                CrateStorage = "config settings",
                ParentActivityId = 88,
                ActivityTemplateId = FixtureData.TestActivityTemplate1().Id
            };
            return actionDo;
        }

        public static ActionDO TestActionUnstarted()
        {
            var actionTemplate = ActionTemplate();
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var processDo = new ProcessDO()
            {
                Id = 1,
                CrateStorage = EnvelopeIdCrateJson(),
                ProcessState = 1
            };

            var actionListDo = new ActionListDO()
            {
                Process = processDo,
                ProcessID = ProcessState.Unstarted,
                Id = 1,
                ActionListType = ActionListType.Immediate
            };

            return new ActionDO
            {
                Id = 1,
                ActionState = ActionState.Unstarted,
                Name = "testaction",
                ParentActivity = actionListDo,
                
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestActionAuthenticate1()
        {
            PluginDO curPluginDO = new PluginDO()
            {
                Id = 1,
                Name = "AzureSqlServer",
                PluginStatus = 1,
                Version = "1" 
            };

            ActivityTemplateDO curActivityTemplateDO = new ActivityTemplateDO
            {
                Id = 1,
                //ActionType = "Write to Sql Server",
                //ParentPluginRegistration = "pluginAzureSqlServer",
                Version = "v1",
                AuthenticationType = "OAuth",
                Plugin = curPluginDO,
                PluginID = 1,
            };



            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "descr 1",
                Name = "template1",
                ProcessTemplateState = ProcessTemplateState.Active,
                DockyardAccount = FixtureData.TestDockyardAccount1()
            };

            var curProcessDO = new ProcessDO()
            {
                Id = 1,
                ProcessTemplateId = 1,
                ProcessTemplate = curProcessTemplateDO
            };

            var curActionListDO = new ActionListDO()
            {
                ProcessID = ProcessState.Unstarted,
                Id = 1,
                ActionListType = ActionListType.Immediate,
                Process = curProcessDO,
            };




            ActionDO curActionDO = new ActionDO();
            curActionDO.Id = 1;
            curActionDO.ActivityTemplateId = 1;
            curActionDO.ActivityTemplate = curActivityTemplateDO;
            curActionDO.ActionState = 1;
            curActionDO.Name = "testaction";
            curActionDO.ParentActivityId = 1;
            curActionDO.ParentActivity = curActionListDO;

            //  curActionDO.ConfigurationSettings = "config settings";
            //  curActionDO.ParentActionListId = 1;

            // curActionListDO.Actions.Add(curActionDO);

            //   curActionDO.ParentActionList = curActionListDO;



            return curActionDO;
        }

        public static ActionDO WaitForDocuSignEvent_Action()
        {
            string templateId = "58521204-58af-4e65-8a77-4f4b51fef626";
            var actionTemplate = ActionTemplate();
            ICrate _crate = ObjectFactory.GetInstance<ICrate>();
            IAction _action = ObjectFactory.GetInstance<IAction>();

            var fieldSelectDockusignTemplate = new DropdownListFieldDefinitionDTO()
            {
                Label = "Select DocuSign Template",
                Name = "Selected_DocuSign_Template",
                Required = true,
                Value = templateId,
                Events = new List<FieldEvent>() {
                     new FieldEvent("onSelect", "requestConfiguration")
                }
            };

            var actionDo = new ActionDO()
            {

                ActionState = ActionState.Unstarted,
                Name = "testaction",
               
                Id = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };

            var fields = new List<FieldDefinitionDTO>()
            {
                fieldSelectDockusignTemplate
            };

            var crateConfiguration = new List<CrateDTO>()
            {
                _crate.Create("Configuration_Controls", JsonConvert.SerializeObject(fields)),
            };

            _action.AddCrate(actionDo, crateConfiguration);

            return actionDo;
        }


        public static AuthorizationTokenDO TestActionAuthenticate2()
        {
            AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
            {
                Token = "TestToken",
                AuthorizationTokenState = AuthorizationTokenState.Active
            };
            return curAuthorizationTokenDO;
        }

        public static AuthorizationTokenDO TestActionAuthenticate3()
        {
            AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
            {
                Token = "TestToken",
                AuthorizationTokenState = AuthorizationTokenState.Revoked
            };
            return curAuthorizationTokenDO;
        }
    }
}