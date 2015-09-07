using Data.Entities;
using Data.States;
using Data.Wrappers;

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
                ParentPluginRegistration = "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1",
                ConfigurationStore = "",
                FieldMappingSettings = "",
                PayloadMappings = "",
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActionTemplateDO ActionTemplate()
        {
            return new ActionTemplateDO()
            {
                Id = 1,
                Name = "Send an Email",
                DefaultEndPoint = "AzureSqlServer",
                Version = "1"
            };
        }

        public static ActionTemplateDO ActionTemplateSMS()
        {
            return new ActionTemplateDO()
            {
                Id = 1,
                Name = "Send a Text (SMS) Message",
                DefaultEndPoint = "AzureSqlServer",
                Version = "1"
            };
        }        public static ActionDO TestAction1()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Name = "Action 1",
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate,
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
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
            return curActionDO;
        }

        public ActionDO TestAction3()
        {
            var actionTemplate = ActionTemplate();
            var origActionDO = new ActionDO()
            {
                ParentActionListId = null,
                Name = "type 1",
                Id = 34,
                ConfigurationStore= "config settings",
                FieldMappingSettings = "fieldMappingSettings",
                Ordering = 3,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
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
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActionDO TestAction5()
        {
            var actionTemplate = ActionTemplateSMS();
            var curActionDO = new ActionDO
            {
                Id = 4,
                Name = "Send a Text (SMS) Message",
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
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
                ParentActionListId = 1,
                Ordering = 2,
                ActionState = ActionState.Unstarted,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction7()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 7,
                Name = "Action 7",
                ParentActionListId = 1,
                Ordering = 3,
                ActionState = ActionState.Unstarted,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction8()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 8,
                Name = "Action 8",
                ParentActionListId = 1,
                Ordering = 4,
                ActionState = ActionState.Unstarted,
                ParentActionList = FixtureData.TestActionList6(),
                ParentPluginRegistration = "AzureSqlServerPluginRegistration_v1",
                PayloadMappings = "x",
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction10()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 5,
                Name = "Action 5",
                ParentActionListId = 1,
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction9()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 2,
                ActionState = ActionState.Error,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }


        public static ActionDO TestAction20()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 1,
                Name = "Action 1",
                ParentActionListId = 1,
                Ordering = 1,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction21()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = 2,
                Name = "Action 2",
                ParentActionListId = 1,
                Ordering = 2,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction22()
        {
            var actionTemplate = FixtureData.ActionTemplate();

            return new ActionDO
            {
                Id = 10,
                Name = "WriteToAzureSql",
                ParentActionListId = 1,
                ConfigurationStore = "JSON Config Settings",
                FieldMappingSettings = "JSON Field Mapping Settings",
                ParentPluginRegistration = "AzureSql",
                Ordering = 1,
                ActionState = ActionState.Unstarted,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate

            };
        }

        public static ActionDO TestAction23()
        {
            var actionTemplate = FixtureData.TestActionTemplateDO1();
            return new ActionDO
            {
                Id = 2,
                Name = "Action 2",
                Ordering = 2,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }

        public static ActionDO IntegrationTestAction()
        {
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var actionTemplate = ActionTemplate();

            var processDo = new ProcessDO()
            {
                Id = 1,
                EnvelopeId = envelopeId,
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
                ParentActionList = actionListDo,
                ParentActionListId = 1,
                ActionState = ActionState.Unstarted,
                Name = "testaction",
                ParentPluginRegistration = "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1",
                FieldMappingSettings = FieldMappings,
                Id = 1,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };

            return actionDo;
        }

        public static ActionDO TestActionHealth1()
        {
            var actionDo = new ActionDO
            {
                Id = 1,
                ParentPluginRegistration = "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1",
                FieldMappingSettings = FieldMappings,
                ActionState = ActionState.Unstarted,
                Name = "testaction",
                ConfigurationStore= "config settings",
                ParentActionListId = 88,
                ActionTemplateId = FixtureData.TestActionTemplate1().Id
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
                EnvelopeId = envelopeId,
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
                ParentPluginRegistration = "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1",
                ParentActionList = actionListDo,
                FieldMappingSettings = FixtureData.FieldMappings,
                ActionTemplateId = actionTemplate.Id,
                ActionTemplate = actionTemplate
            };
        }
    }
}