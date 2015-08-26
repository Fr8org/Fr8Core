using Data.Entities;
using Data.States;
using Data.Wrappers;
using DocuSign.Integrations.Client;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActionDO TestAction1()
        {
            var curActionDO = new ActionDO
            {
                Id = 1,
                UserLabel = "Action 1",
                ActionListId = null
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
            return new ActionDO
            {
                Id = 2,
                UserLabel = "Action 2",
                ActionListId = null,
                Ordering = 2
            };
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
            return new ActionDO
            {
                Id = 2,
                ActionState = ActionState.Error
            };
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
                ParentPluginRegistration = "Core.PluginRegistrations.AzureSqlServerPluginRegistration_v1"
            };
        }
    }
}