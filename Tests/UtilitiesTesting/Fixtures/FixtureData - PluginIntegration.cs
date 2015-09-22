using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static string TestPlugin_DocuSign_EndPoint = "localhost:60001";
        public static string TestPlugin_Core_EndPoint = "localhost:60002";

        public static ActionListDO TestActionList_ImmediateActions()
        {
            return new ActionListDO()
            {
                ActionListType = ActionListType.Immediate,
                Name = "ImmediateActions"
            };
        }

        public static PluginDO TestPlugin_DocuSign()
        {
            return new PluginDO
            {
                Name = "pluginDocuSign",
                PluginStatus = PluginStatus.Active,
                Endpoint = TestPlugin_DocuSign_EndPoint,
                Version = "1"
            };
        }

        public static PluginDO TestPlugin_Core()
        {
            return new PluginDO
            {
                Name = "pluginDockyardCore",
                Endpoint = TestPlugin_Core_EndPoint,
                PluginStatus = PluginStatus.Active,
                Version = "1"
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_WaitForDocuSignEvent()
        {
            return new ActivityTemplateDO()
            {
                Name = "Wait_For_DocuSign_Event",
                Version = "1",
                Plugin = TestPlugin_DocuSign()
            };
        }

        public static ActivityTemplateDO TestActivityTemplateDO_FilterUsingRunTimeData()
        {
            return new ActivityTemplateDO()
            {
                Name = "FilterUsingRunTimeData",
                Version = "1",
                Plugin = TestPlugin_Core()
            };
        }

        public static ActionDO TestAction_Blank()
        {
            return new ActionDO()
            {
                Name = "New Action #1",
                CrateStorage = ""
            };
        }
    }
}
