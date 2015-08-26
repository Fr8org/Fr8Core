using Data.Entities;
using Data.States;
using Data.Wrappers;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {

        public static ActionDO TestActionWriteSqlServer1()
        {
            var curActionDO = new ActionDO
            {
                Id = 54,
                UserLabel = "Save to Sql Server",
                ActionType = "Write to Sql Server",
                ParentPluginRegistration = "AzureSqlServerPluginRegistration_v1",
                ConfigurationSettings = "",
                FieldMappingSettings = "",
                PayloadMappings = "",
                Ordering = 1,
                ActionState = ActionState.Unstarted
            };
            return curActionDO;
        }
        public static ActionDO TestAction1()
        {
            var curActionDO = new ActionDO
            {
                Id = 1,
                UserLabel = "Action 1"
            };
            return curActionDO;
        }

        public static TemplateDO TestTemplate1()
        {
            var curTemplateDO = new TemplateDO(new DocuSignTemplate())
            {
                Id = 1
            };

            return curTemplateDO;
        }

        public static ActionDO TestAction2()
        {
            var curActionDO = new ActionDO
            {
                Id = 2,
                UserLabel = "Action 2"
            };
            return curActionDO;
        }

        public ActionDO TestAction3()
        {
            var origActionDO = new ActionDO()
            {
                ActionListId = null,
                ActionType = "type 1",
                Id = 34,
                ConfigurationSettings = "config settings",
                FieldMappingSettings = "fieldMappingSettings",
                UserLabel = "my test action",
                Ordering = 3
            };

            return origActionDO;
        }


        public static ActionDO TestAction4()
        {
            var curActionDO = new ActionDO
            {
                Id = 3,
                ActionType = "Send an Email"
            };
            return curActionDO;
        }
        public static ActionDO TestAction5()
        {
            return new ActionDO
            {
                Id = 5,
                UserLabel = "Action 5",
                ActionListId = 1,
                Ordering = 1,
                ActionState = ActionState.Unstarted
            };
        }

        public static ActionDO TestAction6()
        {
            return new ActionDO
            {
                Id = 6,
                UserLabel = "Action 6",
                ActionListId = 1,
                Ordering = 2,
                ActionState = ActionState.Unstarted
            };
        }

        public static ActionDO TestAction7()
        {
            return new ActionDO
            {
                Id = 7,
                UserLabel = "Action 7",
                ActionListId = 1,
                Ordering = 3,
                ActionState = ActionState.Unstarted
            };
        }

        public static ActionDO TestAction8()
        {
            return new ActionDO
            {
                Id = 8,
                UserLabel = "Action 8",
                ActionListId = 1,
                Ordering = 4,
                ActionState = ActionState.Unstarted,
                ActionList = FixtureData.TestActionList6(),
                ParentPluginRegistration = "AzureSqlServerPluginRegistration_v1",
                PayloadMappings = "x"
            };
        }

        public static ActionDO TestAction9()
        {
            return new ActionDO
            {
                Id = 2,
                ActionState = ActionState.Error
            };
        }


        public static ActionDO TestAction20()
        {
            return new ActionDO
            {
                Id = 1,
                UserLabel = "Action 1",
                ActionListId = 1,
                Ordering = 1
            };
        }

        public static ActionDO TestAction21()
        {
            return new ActionDO
            {
                Id = 2,
                UserLabel = "Action 2",
                ActionListId = 1,
                Ordering = 2
            };
        }

        public static ActionDO IntegrationTestAction()
        {
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

            var actionDo = new ActionDO()
            {
                ActionList = actionListDo,
                ActionListId = 1,
                ActionState = ActionState.Unstarted,
                ActionType = "testaction",
                ParentPluginRegistration = "AzureSqlServerPluginRegistration_v1",
                FieldMappingSettings = FixtureData.FieldMappings,
                Id = 1
            };

            return actionDo;
        }
    }
}