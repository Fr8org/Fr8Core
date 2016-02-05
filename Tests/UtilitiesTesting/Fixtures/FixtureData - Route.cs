using System.Collections.Generic;
using System.Linq;
using Data.Crates;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Utilities.Serializers.Json;

namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static PlanDO TestRoute1()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(33),
                Description = "descr 1",
                Name = "template1",
                RouteState = RouteState.Active,
              
                
            };
            return plan;
        }

        public static PlanDO TestRoute2()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(50),
                Description = "descr 2",
                Name = "template2",
                RouteState = RouteState.Active,

                //UserId = "testUser1"
                //Fr8Account = FixtureData.TestDockyardAccount1()
            };
            return plan;
        }

        public static PlanDO TestRouteHealthDemo()
        {
            var healthRoute = new PlanDO
            {
                Id = GetTestGuidById(23),
                Description = "DO-866 HealthDemo Integration Test",
                Name = "HealthDemoIntegrationTest",
                RouteState = RouteState.Active,
            };
            return healthRoute;
        }

        public static PlanDO TestRouteWithSubroutes()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-982 Process Node Template Test",
                Name = "RouteWithSubroutes",
                RouteState = RouteState.Active,
            };

            for (int i = 1; i <= 4; ++i)
            {
                var curSubrouteDO = new SubrouteDO()
                {
                    Id = GetTestGuidById(i),
                    Name = string.Format("curSubrouteDO-{0}", i),
                    ParentRouteNode = curPlanDO,
                    RootRouteNode = curPlanDO
                };
                curPlanDO.ChildNodes.Add(curSubrouteDO);
            }

            return curPlanDO;
        }

        public static PlanDO TestRouteWithSubscribeEvent()
        {
            PlanDO planDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO testUser = TestDockyardAccount1();
                uow.UserRepository.Add(testUser);

                planDO = new PlanDO()
                {
                    Id = GetTestGuidById(23),
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    RouteState = RouteState.Active,
                    Fr8Account = testUser
                };
                uow.RouteNodeRepository.Add(planDO);

                var actionTemplate = ActionTemplate();

                var containerDO = new ContainerDO()
                {
                    Id = TestContainer_Id_1(),
                    PlanId = planDO.Id,
                    ContainerState = 1
                };

                using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
                {
                    updater.CrateStorage.Add(GetEnvelopeIdCrate());
                }
                
                uow.ContainerRepository.Add(containerDO);



                SubrouteDO subrouteDO = new SubrouteDO()
                {
                    ParentRouteNode = planDO,
                    RootRouteNode = planDO,
                    StartingSubroute = true
                };
                uow.SubrouteRepository.Add(subrouteDO);
                planDO.ChildNodes = new List<RouteNodeDO> {subrouteDO};
                planDO.StartingSubroute = subrouteDO;


                var actionDo = new ActivityDO()
                {
                    ParentRouteNode = planDO,
                    ParentRouteNodeId = planDO.Id,
                    Id = GetTestGuidById(1),
                    ActivityTemplateId = actionTemplate.Id,
                    ActivityTemplate = actionTemplate,
                    Ordering = 1
                };

                ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();

                var serializer = new JsonSerializer();
                EventSubscriptionCM eventSubscriptionMS = new EventSubscriptionCM();
                eventSubscriptionMS.Subscriptions = new List<string>();
                eventSubscriptionMS.Subscriptions.Add("DocuSign Envelope Sent");
                eventSubscriptionMS.Subscriptions.Add("Write to SQL AZure");

                using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(actionDo))
                {
                    updater.CrateStorage.Add(Crate.FromContent("Standard Event Subscriptions", eventSubscriptionMS));
                }

                uow.ActivityRepository.Add(actionDo);
                subrouteDO.ChildNodes.Add(actionDo);

                uow.SaveChanges();
            }

            return planDO;
        }

        public static PlanDO TestRouteWithSubscribeEvent(Fr8AccountDO user)
        {
            PlanDO planDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.UserRepository.Add(user);

                planDO = new PlanDO()
                {
                    Id = GetTestGuidById(23),
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    RouteState = RouteState.Active,
                    Fr8Account = user
                };
                uow.RouteNodeRepository.Add(planDO);

                var actionTemplate = ActionTemplate();

                var containerDO = new ContainerDO()
                {
                    Id = TestContainer_Id_1(),
                    PlanId = planDO.Id,
                    ContainerState = 1
                };

                using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
                {
                    updater.CrateStorage.Add(GetEnvelopeIdCrate());
                }

                uow.ContainerRepository.Add(containerDO);



                SubrouteDO subrouteDO = new SubrouteDO()
                {
                    ParentRouteNode = planDO,
                    RootRouteNode = planDO,
                    StartingSubroute = true
                };
                uow.SubrouteRepository.Add(subrouteDO);
                planDO.ChildNodes = new List<RouteNodeDO> { subrouteDO };
                planDO.StartingSubroute = subrouteDO;


                var actionDo = new ActivityDO()
                {
                    ParentRouteNode = planDO,
                    ParentRouteNodeId = planDO.Id,
                    Id = GetTestGuidById(1),
                    ActivityTemplateId = actionTemplate.Id,
                    ActivityTemplate = actionTemplate,
                    Ordering = 1
                };

                ICrateManager crate = ObjectFactory.GetInstance<ICrateManager>();

                var serializer = new JsonSerializer();
                EventSubscriptionCM eventSubscriptionMS = new EventSubscriptionCM();
                eventSubscriptionMS.Subscriptions = new List<string>();
                eventSubscriptionMS.Subscriptions.Add("DocuSign Envelope Sent");
                eventSubscriptionMS.Subscriptions.Add("Write to SQL AZure");

                using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(actionDo))
                {
                    updater.CrateStorage.Add(Crate.FromContent("Standard Event Subscriptions", eventSubscriptionMS));
                }

                uow.ActivityRepository.Add(actionDo);
                subrouteDO.ChildNodes.Add(actionDo);

                uow.SaveChanges();
            }
            return planDO;
        }


        public static PlanDO TestRoute3()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                RouteState = RouteState.Active,
            };

            for (int i = 1; i <= 2; ++i)
            {
                var curSubrouteDO = new SubrouteDO()
                {
                    Id = GetTestGuidById(i),
                    Name = string.Format("curSubrouteDO-{0}", i),
                    ParentRouteNode = curPlanDO,
                    RootRouteNode = curPlanDO,
                    ChildNodes = FixtureData.TestActionList1(),
                };
                curPlanDO.ChildNodes.Add(curSubrouteDO);
            }

            return curPlanDO;
        }

        public static PlanDO TestRouteNoMatchingParentActivity()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                RouteState = RouteState.Active,
            };

            for (int i = 1; i <= 2; ++i)
            {
                var curSubrouteDO = new SubrouteDO()
                {
                    Id = GetTestGuidById(i),
                    Name = string.Format("curSubrouteDO-{0}", i),
                    ParentRouteNode = curPlanDO,
                    RootRouteNode = curPlanDO,
                    ChildNodes = FixtureData.TestActionListParentActivityID12()
                };
                curPlanDO.ChildNodes.Add(curSubrouteDO);
            }

            return curPlanDO;
        }

        public static PlanDO TestRouteWithStartingSubroute()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1124 Proper  deletion of Plan",
                Name = "TestRouteWithStartingSubroutes",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Id = GetTestGuidById(1),
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentRouteNode = curPlanDO,
                RootRouteNode = curPlanDO,
                StartingSubroute = true
            };
            curPlanDO.ChildNodes.Add(curSubrouteDO);

            //FixtureData.TestActionList1 .TestActionList_ImmediateActions();
    
            return curPlanDO;
        }


        public static PlanDO TestRouteWithStartingSubrouteAndActionList()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1124 Proper  deletion of Plan",
                Name = "TestRouteWithStartingSubroutes",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Id = GetTestGuidById(1),
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentRouteNode = curPlanDO,
                RootRouteNode = curPlanDO,
                StartingSubroute = true
            };
            curPlanDO.ChildNodes.Add(curSubrouteDO);

            var curImmediateActionList = FixtureData.TestActionList_ImmediateActions();
            
            curSubrouteDO.ChildNodes.AddRange(curImmediateActionList);

            return curPlanDO;
        }


        public static PlanDO TestRouteWithStartingSubroutes_ID0()
            {
            var curPlanDO = new PlanDO
            {
                Description = "DO-1124 Proper  deletion of Plan",
                Name = "TestRouteWithStartingSubroutes_ID0",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentRouteNode = curPlanDO,
                RootRouteNode = curPlanDO,
                StartingSubroute = true
            };
            curPlanDO.ChildNodes.Add(curSubrouteDO);


            return curPlanDO;
        }

        public static PlanDO TestRoute_CanCreate()
        {
            var curPlanDO = new PlanDO
            {
                Description = "DO-1217 Unit Tests for Process#Create",
                Name = "DO-1217",
                RouteState = RouteState.Active,
                //Subroutes = new List<SubrouteDO>(),
            };
            return curPlanDO;
        }

        public static PlanDO TestRoute4()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(30),
                Description = "Description 4",
                Name = "Plan 4",
                RouteState = RouteState.Active,
                Fr8Account = FixtureData.TestDockyardAccount5()
            };
            return plan;
        }

        public static PlanDO TestRoute5()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(40),
                Description = "Description 5",
                Name = "Plan 5",
                RouteState = RouteState.Active,
                Fr8Account = FixtureData.TestDockyardAccount5()
            };
            return plan;
        }
        public static PlanDO TestContainerCreateAddsLogs()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1419 Container Create Adds Logs Test",
                Name = "Container Create",
                RouteState = RouteState.Active
             
            };

            return curPlanDO;
        }
    }
}