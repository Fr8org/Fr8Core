using System;
using System.Collections.Generic;
using Data.Entities;
using Data.States;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static ProcessNodeDO TestProcessNode()
        {
            var processNode = new ProcessNodeDO();
            processNode.Id = 50;
            processNode.ParentContainerId = TestContainer_Id_49();
            processNode.SubrouteId = GetTestGuidById(50);
            processNode.Subroute = TestSubrouteDO1();
            processNode.ProcessNodeState = ProcessNodeState.Unstarted;
            processNode.ParentContainer = TestContainer1();

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode1()
        {
            var processNode = new ProcessNodeDO();
            processNode.Id = 50;
            processNode.ParentContainerId = TestContainer_Id_49();

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode2()
        {

            var processNode = new ProcessNodeDO();
            processNode.Id = 51;
            processNode.ParentContainerId = TestContainer_Id_49();
            processNode.Subroute = TestSubrouteDO1();
            processNode.Subroute.ChildNodes.AddRange(TestActivityList5());

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode3()
        {
            var processNode = new ProcessNodeDO();
            processNode.Id = 51;
            processNode.ParentContainerId = TestContainer_Id_49();
            processNode.Subroute = TestSubrouteDO2();
            processNode.Subroute.ChildNodes.AddRange(TestActivityList5());

            return processNode;
        }

        public static ProcessNodeDO TestProcessNode4()
        {

            var processNode = new ProcessNodeDO();
            processNode.Id = 1;
            processNode.ParentContainerId = TestContainer_Id_49();
            processNode.SubrouteId = GetTestGuidById(50);
            processNode.Subroute = TestSubrouteDO3();
            processNode.Subroute.ChildNodes.AddRange(TestActivityList6());

            return processNode;
        }
    }
}


public static class ListHelper
{
    public static void AddRange<T>(this IList<T> that, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            that.Add(item);
        }
    }
}
