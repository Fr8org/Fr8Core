using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using StructureMap;
using System.Collections.Generic;
using System.Linq;
using Data.Interfaces.ManifestSchemas;
using Utilities.Serializers.Json;
namespace UtilitiesTesting.Fixtures
{
    public partial class FixtureData
    {
        public static RouteDO TestRoute1()
        {
            var route = new RouteDO
            {
				Id = 33,
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
                Id = 50,
                Description = "descr 2",
                Name = "template2",
                RouteState = RouteState.Active,

                //UserId = "testUser1"
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
            return route;
        }

        public static RouteDO TestRouteHealthDemo()
        {
            var healthRoute = new RouteDO
            {
                Id = 23,
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
                Id = 1,
                Description = "DO-982 Process Node Template Test",
                Name = "RouteWithSubroutes",
                RouteState = RouteState.Active,
            };

            for (int i = 1; i <= 4; ++i)
            {
                var curSubrouteDO = new SubrouteDO()
                {
                    Id = i,
                    Name = string.Format("curSubrouteDO-{0}", i),
                    ParentActivity = curRouteDO,
                };
                curRouteDO.Activities.Add(curSubrouteDO);
            }

            return curRouteDO;
        }

        public static RouteDO TestRouteWithSubscribeEvent()
        {
            RouteDO processTemplateDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                DockyardAccountDO testUser = TestDockyardAccount1();
                uow.UserRepository.Add(testUser);

                processTemplateDO = new RouteDO()
                {
                    Id = 23,
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    RouteState = RouteState.Active,
                    DockyardAccount = testUser
                };
                uow.RouteRepository.Add(processTemplateDO);

                var actionTemplate = ActionTemplate();

                var processDo = new ProcessDO()
                {
                    Id = 1,
                    CrateStorage = EnvelopeIdCrateJson(),
                    RouteId = processTemplateDO.Id,
                    ProcessState = 1
                };
                uow.ProcessRepository.Add(processDo);



                SubrouteDO subrouteDO = new SubrouteDO()
                {
                    ParentActivity = processTemplateDO,
                    StartingSubroute = true
                };
                uow.SubrouteRepository.Add(subrouteDO);
                processTemplateDO.Activities = new List<ActivityDO> {subrouteDO};
                processTemplateDO.StartingSubroute = subrouteDO;


                var actionDo = new ActionDO()
                {
                    ParentActivity = processTemplateDO,
                    ParentActivityId = processTemplateDO.Id,
                    Name = "testaction",

                    Id = 1,
                    ActivityTemplateId = actionTemplate.Id,
                    ActivityTemplate = actionTemplate,
                    Ordering = 1
                };
                ICrate crate = ObjectFactory.GetInstance<ICrate>();

                var serializer = new JsonSerializer();
                EventSubscriptionCM eventSubscriptionMS = new EventSubscriptionCM();
                eventSubscriptionMS.Subscriptions = new List<string>();
                eventSubscriptionMS.Subscriptions.Add("DocuSign Envelope Sent");
                eventSubscriptionMS.Subscriptions.Add("Write to SQL AZure");

                var eventReportJSON = serializer.Serialize(eventSubscriptionMS);

                CrateDTO crateDTO = crate.Create("Standard Event Subscriptions", eventReportJSON, "Standard Event Subscriptions");
                actionDo.UpdateCrateStorageDTO(new List<CrateDTO>() { crateDTO });

                uow.ActionRepository.Add(actionDo);
                subrouteDO.Activities.Add(actionDo);

                uow.SaveChanges();
            }

            return processTemplateDO;
        }

        public static RouteDO TestRoute3()
        {
            var curRouteDO = new RouteDO
            {
                Id = 1,
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                RouteState = RouteState.Active,
            };

            for (int i = 1; i <= 2; ++i)
            {
                var curSubrouteDO = new SubrouteDO()
                {
                    Id = i,
                    Name = string.Format("curSubrouteDO-{0}", i),
                    ParentActivity = curRouteDO,
                    Activities = FixtureData.TestActionList1(),
                };
                curRouteDO.Activities.Add(curSubrouteDO);
            }

            return curRouteDO;
        }

        public static RouteDO TestRouteNoMatchingParentActivity()
        {
            var curRouteDO = new RouteDO
            {
                Id = 1,
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                RouteState = RouteState.Active,
            };

            for (int i = 1; i <= 2; ++i)
            {
                var curSubrouteDO = new SubrouteDO()
                {
                    Id = i,
                    Name = string.Format("curSubrouteDO-{0}", i),
                    ParentActivity = curRouteDO,
                    Activities = FixtureData.TestActionListParentActivityID12()
                };
                curRouteDO.Activities.Add(curSubrouteDO);
            }

            return curRouteDO;
        }

        public static RouteDO TestRouteWithStartingSubroute()
        {
            var curRouteDO = new RouteDO
            {
                Id = 1,
                Description = "DO-1124 Proper  deletion of Route",
                Name = "TestRouteWithStartingSubroutes",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Id = 1,
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentActivity = curRouteDO,
                StartingSubroute = true
            };
            curRouteDO.Activities.Add(curSubrouteDO);

            //FixtureData.TestActionList1 .TestActionList_ImmediateActions();
    
            return curRouteDO;
        }


        public static RouteDO TestRouteWithStartingSubrouteAndActionList()
        {
            var curRouteDO = new RouteDO
            {
                Id = 1,
                Description = "DO-1124 Proper  deletion of Route",
                Name = "TestRouteWithStartingSubroutes",
                RouteState = RouteState.Active,
            };

            var curSubrouteDO = new SubrouteDO()
            {
                Id = 1,
                Name = string.Format("curSubrouteDO-{0}", 1),
                ParentActivity = curRouteDO,
                StartingSubroute = true
            };
            curRouteDO.Activities.Add(curSubrouteDO);

            var curImmediateActionList = FixtureData.TestActionList_ImmediateActions();
            
            curSubrouteDO.Activities.AddRange(curImmediateActionList);

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
                ParentActivity = curRouteDO,
                StartingSubroute = true
            };
            curRouteDO.Activities.Add(curSubrouteDO);


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
    }
}