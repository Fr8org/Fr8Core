using Data.Entities;
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

        public static ActionDO TestAction3()
        {
            var curActionDO = new ActionDO
            {
                Id = 3,
                UserLabel = "Send an Email"
            };
            return curActionDO;
        }
        public static ActionDO TestAction4()
        {
            var curActionDO = new ActionDO
            {
                Id = 4,
                UserLabel = "Send a Text (SMS) Message"
            };
            return curActionDO;
        }
        public static ActionDO TestAction5()
        {
            var curActionDO = new ActionDO
            {
                Id = 5,
                UserLabel = ""
            };
            return curActionDO;
        }
    }
}