using Data.Entities;
using Data.States;
using System.Collections.Generic;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {

        public static ActionListDO TestActionListHealth1()
        {
            string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var processDo = new ProcessDO
            {
                Id = 1,
                EnvelopeId = envelopeId,
                ProcessState = 1,
                Name = "test name",
                ProcessTemplateId = TestProcessTemplateHealthDemo().Id
            };

            return new ActionListDO
            {
                Id = 88,
                Name = "list1",
                ActionListType = ActionListType.Immediate,
                ProcessNodeTemplateID = 50,
                CurrentActivity = TestActionHealth1(),
                Process = processDo
            };
        }

        public static ActionListDO TestActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 1,
                ProcessNodeTemplateID = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate
            };
            curActionListDO.Actions.Add(TestAction20());
            curActionListDO.Actions.Add(TestAction21());

            return curActionListDO;
        }

        public static ActionListDO TestEmptyActionList()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 4,
                ProcessNodeTemplateID = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate
            };
            return curActionListDO;
        }

      

        public static ActionListDO TestActionListMedical()
        {
            var curActionListDO = new ActionListDO
            {
                Id = 4,
                ProcessNodeTemplateID = 1,
                Name = "list1",
                ActionListType = ActionListType.Immediate,                    
            };
            return curActionListDO;
        }

        public static ActionListDO TestActionList3()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentActivity = TestAction21(),
                ActionListState = ActionListState.Inprocess
            };
        }

        public static ActionListDO TestActionList4()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentActivity = TestAction21(),
                ActionListState = ActionListState.Unstarted
            };
        }

        public static ActionListDO TestActionList5()
        {
            return new ActionListDO
            {
                Id = 2,
                ActionListType = ActionListType.Immediate,
                CurrentActivity = FixtureData.TestAction6(),
                ActionListState = ActionListState.Unstarted,
                Actions = new System.Collections.Generic.List<ActionDO>() 
                { 
                    FixtureData.TestAction10(),
                    FixtureData.TestAction7(),
                    FixtureData.TestAction8()             
                }
            };
        }

        public static ActionListDO TestActionList6()
        {
            ProcessDO processDO = FixtureData.TestProcess1();
            processDO.EnvelopeId = "";
            return new ActionListDO
            {
                Id = 2,
                ActionListType = ActionListType.Immediate,
                ActionListState = ActionListState.Unstarted,
                Process = processDO
            };
        }

        public static ActionListDO TestActionList7()
        {
            return new ActionListDO
            {
                Id = 2,
                CurrentActivity = FixtureData.TestAction6(),
                ActionListState = ActionListState.Unstarted,
                Actions = new System.Collections.Generic.List<ActionDO>() 
                { 
                    FixtureData.TestAction10(),
                    FixtureData.TestAction7(),
                    FixtureData.TestAction8()             
                }
            };
        }
		  /// <summary>
		  /// Big tree from https://maginot.atlassian.net/wiki/display/SH/Getting+Upstream+and+Downstream+Activities+Lists
		  /// </summary>
		  /// <returns></returns>
		  public static List<ActionListDO> TreeFromWikiPage()
		  {
			  List<ActionListDO> actionLists = new List<ActionListDO>();

			  var actionTempate = new ActionTemplateDO()
			  {
				  Id = 1,
				  ActionType = "Test action",
				  ParentPluginRegistration = "Test registration",
				  Version = "1"
			  };
			  ActionListDO al_1 = new ActionListDO() { Id = 1, Ordering = 1, ActionListType = ActionListType.Immediate, Name = "Fly To Kiev" };
			  ActionDO a_23 = new ActionDO() { Id = 23, ActionTemplate = actionTempate, Name = "Drive to  Ariport" };
			  al_1.Actions.Add(a_23);
			  a_23.ParentActionList = al_1;

			  ActionListDO al_43 = new ActionListDO() { Id = 43, Ordering = 2, ActionListType = ActionListType.Immediate, Name = "Board Plane" };
			  al_43.ParentActionList = al_1;
			  ActionDO a_44 = new ActionDO() { Id = 44, Ordering = 1, ActionTemplate = actionTempate, Name = "Check Baggage" };
			  a_44.ParentActionList = al_43;
			  al_43.Actions.Add(a_44);
			  ActionDO a_46 = new ActionDO() { Id = 46, Ordering = 2, ActionTemplate = actionTempate, Name = "Buy Ticket" };
			  a_46.ParentActionList = al_43;
			  al_43.Actions.Add(a_46);
			  ActionDO a_48 = new ActionDO() { Id = 48, Ordering = 3, ActionTemplate = actionTempate, Name = "Get on Plane" };
			  a_48.ParentActionList = al_43;
			  al_43.Actions.Add(a_48);

			  ActionListDO al_52 = new ActionListDO() { Id = 52, Ordering = 3, ActionListType = ActionListType.Immediate, Name = "BLA BLA" };
			  ActionDO a_53 = new ActionDO() { Id = 53, Ordering = 1, ActionTemplate = actionTempate, Name = "A1" };
			  a_53.ParentActionList = al_52;
			  al_52.Actions.Add(a_53);

			  ActionListDO al_54 = new ActionListDO() { Id = 54, Ordering = 2, ActionListType = ActionListType.Immediate, Name = "AL2" };
			  al_54.ParentActionList = al_52;

			  ActionDO a_56 = new ActionDO() { Id = 56, Ordering = 1, ActionTemplate = actionTempate, Name = "A11" };
			  a_56.ParentActionList = al_54;
			  al_54.Actions.Add(a_56);
			  ActionDO a_57 = new ActionDO() { Id = 57, Ordering = 2, ActionTemplate = actionTempate, Name = "A22" };
			  a_57.ParentActionList = al_54;
			  al_54.Actions.Add(a_57);
			  ActionDO a_58 = new ActionDO() { Id = 58, Ordering = 3, ActionTemplate = actionTempate, Name = "A33" };
			  a_58.ParentActionList = al_54;
			  al_54.Actions.Add(a_58);

			  ActionDO a_55 = new ActionDO() { Id = 55, Ordering = 3, ActionTemplate = actionTempate, Name = "A3" };
			  a_55.ParentActionList = al_52;
			  al_52.Actions.Add(a_55);

			  ActionListDO al_59 = new ActionListDO() { Id = 59, Ordering = 4, ActionListType = ActionListType.Immediate, Name = "BLA BLA2" };
			  ActionDO a_60 = new ActionDO() { Id = 60, Ordering = 1, ActionTemplate = actionTempate, Name = "A1" };
			  a_60.ParentActionList = al_59;
			  al_59.Actions.Add(a_60);

			  ActionListDO al_61 = new ActionListDO() { Id = 61, Ordering = 2, ActionListType = ActionListType.Immediate, Name = "AL2" };
			  al_61.ParentActionList = al_59;
			  ActionDO a_63 = new ActionDO() { Id = 63, Ordering = 1, ActionTemplate = actionTempate, Name = "A11" };
			  a_63.ParentActionList = al_61;
			  al_61.Actions.Add(a_63);
			  ActionDO a_64 = new ActionDO() { Id = 64, Ordering = 2, ActionTemplate = actionTempate, Name = "A22" };
			  a_64.ParentActionList = al_61;
			  al_61.Actions.Add(a_64);
			  ActionDO a_65 = new ActionDO() { Id = 65, Ordering = 3, ActionTemplate = actionTempate, Name = "A33" };
			  a_65.ParentActionList = al_61;
			  al_61.Actions.Add(a_65);

			  ActionDO a_62 = new ActionDO() { Id = 62, Ordering = 3, ActionTemplate = actionTempate, Name = "A3" };
			  a_62.ParentActionList = al_59;
			  al_59.Actions.Add(a_62);

			  al_43.ParentActionList = al_1;
			  al_52.ParentActionList = al_1;
			  al_59.ParentActionList = al_1;

			  actionLists.Add(al_1);
			  actionLists.Add(al_43);
			  actionLists.Add(al_52);
			  actionLists.Add(al_54);
			  actionLists.Add(al_59);
			  actionLists.Add(al_61);
			  return actionLists;
		  }
    }
}
