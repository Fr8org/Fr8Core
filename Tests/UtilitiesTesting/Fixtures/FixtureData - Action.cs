using Data.Entities;
using Data.States;
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
                UserLabel = "Action 1"
            };
            return curActionDO;
        }

        public static TemplateDO TestTemplate1()
        {
            var curTemplateDO = new TemplateDO(new Template())
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
            return new ActionDO
            {
                Id = 2,
                ActionState = ActionState.Inprocess
            };
        }

    }
}