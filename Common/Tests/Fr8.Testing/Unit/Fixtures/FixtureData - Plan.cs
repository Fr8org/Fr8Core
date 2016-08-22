using System.Collections.Generic;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.Managers;
using Hub.Managers;
using Fr8.Infrastructure.Utilities.Serializers.Json;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Testing.Unit.Fixtures
{
    public partial class FixtureData
    {
        public static PlanDO TestPlan1()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(33),
                Description = "descr 1",
                Name = "template1",
                PlanState = PlanState.Executing,


            };
            return plan;
        }

        public static PlanDO TestPlan2()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(50),
                Description = "descr 2",
                Name = "template2",
                PlanState = PlanState.Executing,

                //UserId = "testUser1"
                //Fr8Account = FixtureData.TestDockyardAccount1()
            };
            return plan;
        }

        public static PlanDO TestPlanHealthDemo()
        {
            var healthPlan = new PlanDO
            {
                Id = GetTestGuidById(23),
                Description = "DO-866 HealthDemo Integration Test",
                Name = "HealthDemoIntegrationTest",
                PlanState = PlanState.Executing,
            };
            return healthPlan;
        }

        public static PlanDO TestPlanWithSubPlans()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-982 Process Node Template Test",
                Name = "PlanWithSubPlans",
                PlanState = PlanState.Executing,
            };

            for (int i = 1; i <= 4; ++i)
            {
                var curSubPlanDO = new SubplanDO()
                {
                    Id = GetTestGuidById(i),
                    Name = string.Format("curSubPlanDO-{0}", i),
                    ParentPlanNode = curPlanDO,
                };
                curPlanDO.ChildNodes.Add(curSubPlanDO);
            }

            return curPlanDO;
        }

        public static PlanDO TestPlanWithSubscribeEvent()
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
                    PlanState = PlanState.Executing,
                    Fr8Account = testUser
                };
                uow.PlanRepository.Add(planDO);

                var actionTemplate = ActivityTemplate();

                var containerDO = new ContainerDO()
                {
                    Id = TestContainer_Id_1(),
                    PlanId = planDO.Id,
                    State = 1
                };

                using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
                {
                    crateStorage.Add(GetEnvelopeIdCrate());
                }

                uow.ContainerRepository.Add(containerDO);

                uow.ActivityTemplateRepository.Add(actionTemplate);

                SubplanDO subPlaneDO = new SubplanDO()
                {
                    ParentPlanNode = planDO,
                    StartingSubPlan = true
                };
                planDO.ChildNodes = new List<PlanNodeDO> { subPlaneDO };
                planDO.StartingSubplan = subPlaneDO;

                uow.ActivityTemplateRepository.Add(actionTemplate);

                var actionDo = new ActivityDO()
                {
                    ParentPlanNode = planDO,
                    ParentPlanNodeId = planDO.Id,
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

                using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(actionDo))
                {
                    crateStorage.Add(Crate.FromContent("Standard Event Subscriptions", eventSubscriptionMS));
                }

                subPlaneDO.ChildNodes.Add(actionDo);

                uow.SaveChanges();
            }

            return planDO;
        }

        public static PlanDO TestPlanWithSubscribeEvent(Fr8AccountDO user, int idOffset = 0)
        {
            PlanDO planDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.UserRepository.Add(user);

                planDO = new PlanDO()
                {
                    Id = GetTestGuidById(23 + idOffset),
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    PlanState = PlanState.Executing,
                    Fr8Account = user
                };
                uow.PlanRepository.Add(planDO);

                var actionTemplate = ActivityTemplate();

                var containerDO = new ContainerDO()
                {
                    Id = TestContainer_Id_1(),
                    PlanId = planDO.Id,
                    State = 1
                };

                using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
                {
                    crateStorage.Add(GetEnvelopeIdCrate());
                }

                uow.ContainerRepository.Add(containerDO);
                uow.ActivityTemplateRepository.Add(actionTemplate);


                SubplanDO subplanDO = new SubplanDO()
                {
                    ParentPlanNode = planDO,
                    StartingSubPlan = true
                };
                planDO.ChildNodes = new List<PlanNodeDO> { subplanDO };
                planDO.StartingSubplan = subplanDO;


                var actionDo = new ActivityDO()
                {
                    Id = GetTestGuidById(12 + idOffset),
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

                using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(actionDo))
                {
                    crateStorage.Add(Crate.FromContent("Standard Event Subscriptions", eventSubscriptionMS));
                }

                // uow.ActivityRepository.Add(actionDo);
                subplanDO.ChildNodes.Add(actionDo);

                uow.SaveChanges();
            }
            return planDO;
        }


        public static PlanDO TestPlan3()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                PlanState = PlanState.Executing,
            };

            for (int i = 2; i <= 3; ++i)
            {
                var curSubPlanDO = new SubplanDO()
                {
                    Id = GetTestGuidById(i),
                    Name = string.Format("curSubPlanDO-{0}", i),
                    ParentPlanNode = curPlanDO,
                    ChildNodes = FixtureData.TestActivityList1(i * 3),
                };
                curPlanDO.ChildNodes.Add(curSubPlanDO);
            }

            return curPlanDO;
        }

        public static PlanDO TestPlanNoMatchingParentActivity()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                PlanState = PlanState.Executing,
            };

            for (int i = 2; i <= 3; ++i)
            {
                var curSubPlanDO = new SubplanDO()
                {
                    Id = GetTestGuidById(i),
                    Name = string.Format("curSubPlanDO-{0}", i),
                    ParentPlanNode = curPlanDO,
                    ChildNodes = FixtureData.TestActivityListParentActivityID12()
                };
                curPlanDO.ChildNodes.Add(curSubPlanDO);
            }

            return curPlanDO;
        }

        public static PlanDO TestPlanWithStartingSubPlan()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1124 Proper  deletion of Plan",
                Name = "TestPlanWithStartingSubPlans",
                PlanState = PlanState.Executing,
            };

            var curSubPlanDO = new SubplanDO()
            {
                Id = GetTestGuidById(1),
                Name = string.Format("curSubPlanDO-{0}", 1),
                ParentPlanNode = curPlanDO,
                StartingSubPlan = true
            };
            curPlanDO.ChildNodes.Add(curSubPlanDO);

            //FixtureData.TestActionList1 .TestActionList_ImmediateActions();

            return curPlanDO;
        }


        public static PlanDO TestPlanWithStartingSubPlanAndActivityList()
        {
            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "DO-1124 Proper  deletion of Plan",
                Name = "TestPlanWithStartingSubPlan",
                PlanState = PlanState.Executing,
            };

            var curSubPlanDO = new SubplanDO()
            {
                Id = GetTestGuidById(2),
                Name = string.Format("curSubPlanDO-{0}", 1),
                ParentPlanNode = curPlanDO,
                StartingSubPlan = true
            };
            curPlanDO.ChildNodes.Add(curSubPlanDO);

            var curImmediateActionList = FixtureData.TestActivityList_ImmediateActivities();

            foreach (var node in curImmediateActionList)
            {
                curSubPlanDO.ChildNodes.Add(node);
            }

            return curPlanDO;
        }


        public static PlanDO TestPlanWithStartingSubPlans_ID0()
        {
            var curPlanDO = new PlanDO
            {
                Description = "DO-1124 Proper  deletion of Plan",
                Name = "TestPlanWithStartingSubPlans_ID0",
                PlanState = PlanState.Executing,
            };

            var curSubPlanDO = new SubplanDO()
            {
                Name = string.Format("curSubPlanDO-{0}", 1),
                ParentPlanNode = curPlanDO,
                StartingSubPlan = true
            };
            curPlanDO.ChildNodes.Add(curSubPlanDO);


            return curPlanDO;
        }

        public static PlanDO TestPlan_CanCreate()
        {
            var curPlanDO = new PlanDO
            {
                Description = "DO-1217 Unit Tests for Process#Create",
                Name = "DO-1217",
                PlanState = PlanState.Executing,
            };
            return curPlanDO;
        }

        public static PlanDO TestPlan4()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(30),
                Description = "Description 4",
                Name = "Plan 4",
                PlanState = PlanState.Executing,
                Fr8Account = FixtureData.TestDockyardAccount5()
            };
            return plan;
        }

        public static PlanDO TestPlan5()
        {
            var plan = new PlanDO
            {
                Id = GetTestGuidById(40),
                Description = "Description 5",
                Name = "Plan 5",
                PlanState = PlanState.Executing,
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
                PlanState = PlanState.Executing
            };

            return curPlanDO;
        }
    }
}