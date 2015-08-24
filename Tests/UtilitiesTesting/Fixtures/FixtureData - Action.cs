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
                ParentPluginRegistration = "AzureSqlServerPluginRegistration_V1",
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

        public static ActionDO TestAction7()
        {
            return new ActionDO
            {
                Id = 2,
                ActionState = ActionState.Inprocess
            };
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
            var curActionDO = new ActionDO
            {
                Id = 4,
                ActionType = "Send a Text (SMS) Message"
            };
            return curActionDO;
        }
        public static ActionDO TestAction6()
        {
            var curActionDO = new ActionDO
            {
                Id = 5,
                ActionType = ""
            };
            return curActionDO;
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
    }
}