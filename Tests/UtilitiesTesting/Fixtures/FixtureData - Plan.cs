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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities.Serializers.Json;

namespace UtilitiesTesting.Fixtures
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
                PlanState = PlanState.Active,


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
                PlanState = PlanState.Active,

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
                PlanState = PlanState.Active,
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
                PlanState = PlanState.Active,
            };

            for (int i = 1; i <= 4; ++i)
            {
                var curSubPlanDO = new SubPlanDO()
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
                    PlanState = PlanState.Active,
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

                SubPlanDO subPlaneDO = new SubPlanDO()
                {
                    ParentPlanNode = planDO,
                    StartingSubPlan = true
                };
                planDO.ChildNodes = new List<PlanNodeDO> { subPlaneDO };
                planDO.StartingSubPlan = subPlaneDO;

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
                    PlanState = PlanState.Active,
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


                SubPlanDO subPlanDO = new SubPlanDO()
                {
                    ParentPlanNode = planDO,
                    StartingSubPlan = true
                };
                planDO.ChildNodes = new List<PlanNodeDO> { subPlanDO };
                planDO.StartingSubPlan = subPlanDO;


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
                subPlanDO.ChildNodes.Add(actionDo);

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
                PlanState = PlanState.Active,
            };

            for (int i = 2; i <= 3; ++i)
            {
                var curSubPlanDO = new SubPlanDO()
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
                PlanState = PlanState.Active,
            };

            for (int i = 2; i <= 3; ++i)
            {
                var curSubPlanDO = new SubPlanDO()
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
                PlanState = PlanState.Active,
            };

            var curSubPlanDO = new SubPlanDO()
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
                PlanState = PlanState.Active,
            };

            var curSubPlanDO = new SubPlanDO()
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
                PlanState = PlanState.Active,
            };

            var curSubPlanDO = new SubPlanDO()
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
                PlanState = PlanState.Active,
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
                PlanState = PlanState.Active,
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
                PlanState = PlanState.Active,
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
                PlanState = PlanState.Active

            };

            return curPlanDO;
        }
    }
}