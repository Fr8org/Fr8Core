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
        public static RouteDO TestRoute1()
        {
            var route = new RouteDO
            {
                Id = GetTestGuidById(33),
                Description = "descr 1",
                Name = "template1",
                RouteState = RouteState.Active,
              
                
            };
            return route;
        }

        public static RouteDO TestRoute2()
        {
            var route = new RouteDO
            {
                Id = GetTestGuidById(50),
                Description = "descr 2",
                Name = "template2",
                RouteState = RouteState.Active,

                //UserId = "testUser1"
                //Fr8Account = FixtureData.TestDockyardAccount1()
            };
            return route;
        }

        public static RouteDO TestRouteHealthDemo()
        {
            var healthRoute = new RouteDO
            {
                Id = GetTestGuidById(23),
                Description = "DO-866 HealthDemo Integration Test",
                Name = "HealthDemoIntegrationTest",
                RouteState = RouteState.Active,
            };
            return healthRoute;
        }

        public static RouteDO TestRouteWithSubroutes()
        {
            var curRouteDO = new RouteDO
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
                    ParentRouteNode = curRouteDO,
                };
                curRouteDO.ChildNodes.Add(curSubrouteDO);
            }

            return curRouteDO;
        }

        public static RouteDO TestRouteWithSubscribeEvent()
        {
            RouteDO routeDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                Fr8AccountDO testUser = TestDockyardAccount1();
                uow.UserRepository.Add(testUser);

                routeDO = new RouteDO()
                {
                    Id = GetTestGuidById(23),
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    RouteState = RouteState.Active,
                    Fr8Account = testUser
                };
                uow.RouteNodeRepository.Add(routeDO);

                var actionTemplate = ActionTemplate();

                var containerDO = new ContainerDO()
                {
                    Id = TestContainer_Id_1(),
                    RouteId = routeDO.Id,
                    ContainerState = 1
                };

                using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
                {
                    updater.CrateStorage.Add(GetEnvelopeIdCrate());
                }
                
                uow.ContainerRepository.Add(containerDO);



                SubrouteDO subrouteDO = new SubrouteDO()
                {
                    ParentRouteNode = routeDO,
                    StartingSubroute = true
                };
                uow.SubrouteRepository.Add(subrouteDO);
                routeDO.ChildNodes = new List<RouteNodeDO> {subrouteDO};
                routeDO.StartingSubroute = subrouteDO;


                var actionDo = new ActionDO()
                {
                    ParentRouteNode = routeDO,
                    ParentRouteNodeId = routeDO.Id,
                    Name = "testaction",

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

                uow.ActionRepository.Add(actionDo);
                subrouteDO.ChildNodes.Add(actionDo);

                uow.SaveChanges();
            }

            return routeDO;
        }

        public static RouteDO TestRoute3()
        {
            var curRouteDO = new RouteDO
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
                    ParentRouteNode = curRouteDO,
                    ChildNodes = FixtureData.TestActionList1(),
                };
                curRouteDO.ChildNodes.Add(curSubrouteDO);
            }

            return curRouteDO;
        }

        public static RouteDO TestRouteNoMatchingParentActivity()
        {
            var curRouteDO = new RouteDO
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
                    ParentRouteNode = curRouteDO,
                    ChildNodes = FixtureData.TestActionListParentActivityID12()
                };
                curRouteDO.ChildNodes.Add(curSubrouteDO);
            }

            return curRouteDO;
        }

        public static RouteDO TestRouteWithStartingSubroute()
        {
            var curRouteDO = new RouteDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1124 Proper  deletion of Route",
                Name = "TestRouteWithStartingSubroutes",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Id = GetTestGuidById(1),
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentRouteNode = curRouteDO,
                StartingSubroute = true
            };
            curRouteDO.ChildNodes.Add(curSubrouteDO);

            //FixtureData.TestActionList1 .TestActionList_ImmediateActions();
    
            return curRouteDO;
        }


        public static RouteDO TestRouteWithStartingSubrouteAndActionList()
        {
            var curRouteDO = new RouteDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1124 Proper  deletion of Route",
                Name = "TestRouteWithStartingSubroutes",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Id = GetTestGuidById(1),
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentRouteNode = curRouteDO,
                StartingSubroute = true
            };
            curRouteDO.ChildNodes.Add(curSubrouteDO);

            var curImmediateActionList = FixtureData.TestActionList_ImmediateActions();
            
            curSubrouteDO.ChildNodes.AddRange(curImmediateActionList);

            return curRouteDO;
        }


        public static RouteDO TestRouteWithStartingSubroutes_ID0()
            {
            var curRouteDO = new RouteDO
            {
                Description = "DO-1124 Proper  deletion of Route",
                Name = "TestRouteWithStartingSubroutes_ID0",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentRouteNode = curRouteDO,
                StartingSubroute = true
            };
            curRouteDO.ChildNodes.Add(curSubrouteDO);


            return curRouteDO;
        }

        public static RouteDO TestRoute_CanCreate()
        {
            var curRouteDO = new RouteDO
            {
                Description = "DO-1217 Unit Tests for Process#Create",
                Name = "DO-1217",
                RouteState = RouteState.Active,
                //Subroutes = new List<SubrouteDO>(),
            };
            return curRouteDO;
        }

        public static RouteDO TestRoute4()
        {
            var route = new RouteDO
            {
                Id = GetTestGuidById(30),
                Description = "Description 4",
                Name = "Route 4",
                RouteState = RouteState.Active,
                Fr8Account = FixtureData.TestDockyardAccount5()
            };
            return route;
        }

        public static RouteDO TestRoute5()
        {
            var route = new RouteDO
            {
                Id = GetTestGuidById(40),
                Description = "Description 5",
                Name = "Route 5",
                RouteState = RouteState.Active,
                Fr8Account = FixtureData.TestDockyardAccount5()
            };
            return route;
        }
        public static RouteDO TestContainerCreateAddsLogs()
        {
            var curRouteDO = new RouteDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1419 Container Create Adds Logs Test",
                Name = "Container Create",
                RouteState = RouteState.Active
             
            };

            return curRouteDO;
        }
    }
}