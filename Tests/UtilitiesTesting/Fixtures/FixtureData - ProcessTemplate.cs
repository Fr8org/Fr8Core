<<<<<<< HEAD
﻿using Core.Interfaces;
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
		public static ProcessTemplateDO TestProcessTemplate1()
		{          
			var processTemplate = new ProcessTemplateDO
			{
				Id = 33,
                Description = "descr 1",
				Name = "template1",
				ProcessTemplateState = ProcessTemplateState.Active,

              
                
			};
			return processTemplate;
		}

        public static ProcessTemplateDO TestProcessTemplate2()
        {
            var processTemplate = new ProcessTemplateDO
            {
                Id = 50,
                Description = "descr 2",
                Name = "template2",
                ProcessTemplateState = ProcessTemplateState.Active,

                //UserId = "testUser1"
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
            return processTemplate;
        }

        public static ProcessTemplateDO TestProcessTemplateHealthDemo()
        {
            var healthProcessTemplate = new ProcessTemplateDO
            {
                Id = 23,
                Description = "DO-866 HealthDemo Integration Test",
                Name = "HealthDemoIntegrationTest",
                ProcessTemplateState = ProcessTemplateState.Active,
            };
            return healthProcessTemplate;
        }

        public static ProcessTemplateDO TestProcessTemplateWithProcessNodeTemplates()
        {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "DO-982 Process Node Template Test",
                Name = "ProcessTemplateWithProcessNodeTemplates",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            for(int i = 1; i <= 4; ++i)
            {
                var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    Id = i,
                    Name = string.Format("curProcessNodeTemplateDO-{0}", i),
                    ProcessTemplate = curProcessTemplateDO,
                };
                curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);
            }

            return curProcessTemplateDO;
        }

        public static ProcessTemplateDO TestProcessTemplateWithSubscribeEvent()
        {
            ProcessTemplateDO processTemplateDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                DockyardAccountDO testUser = TestDockyardAccount1();
                uow.UserRepository.Add(testUser);

                processTemplateDO = new ProcessTemplateDO()
                {
                    Id = 23,
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    ProcessTemplateState = ProcessTemplateState.Active,
                    DockyardAccount = testUser
                };
                uow.ProcessTemplateRepository.Add(processTemplateDO);

                var actionTemplate = ActionTemplate();

                var processDo = new ProcessDO()
                {
                    Id = 1,
                    CrateStorage = EnvelopeIdCrateJson(),
                    ProcessTemplateId = processTemplateDO.Id,
                    ProcessState = 1
                };
                uow.ProcessRepository.Add(processDo);

               

                ProcessNodeTemplateDO processNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    ProcessTemplate = processTemplateDO
                };
                uow.ProcessNodeTemplateRepository.Add(processNodeTemplateDO);


                var actionListDo = new ActionListDO()
                {
                    Process = processDo,
                    ProcessID = processDo.Id,
                    Id = 1,
                    ActionListType = ActionListType.Immediate,
                    ProcessNodeTemplate = processNodeTemplateDO,
                    Ordering = 2
                };
                uow.ActionListRepository.Add(actionListDo);

                var actionDo = new ActionDO()
                {
                    ParentActivity = actionListDo,
                    ParentActivityId = actionListDo.Id,
                    ActionState = ActionState.Unstarted,
                    Name = "testaction",

                    Id = 1,
                    ActivityTemplateId = actionTemplate.Id,
                    ActivityTemplate = actionTemplate,
                    Ordering = 1
                };
                ICrate crate = ObjectFactory.GetInstance<ICrate>();

                var serializer = new JsonSerializer();
                EventSubscriptionMS eventSubscriptionMS = new EventSubscriptionMS();
                eventSubscriptionMS.Subscriptions = new List<string>();
                eventSubscriptionMS.Subscriptions.Add("DocuSign Envelope Sent");
                eventSubscriptionMS.Subscriptions.Add("Write to SQL AZure");

                var eventReportJSON = serializer.Serialize(eventSubscriptionMS);

                CrateDTO crateDTO = crate.Create("Standard Event Subscriptions", eventReportJSON, "Standard Event Subscriptions");
                actionDo.UpdateCrateStorageDTO(new List<CrateDTO>() { crateDTO });

                uow.ActionRepository.Add(actionDo);
                actionListDo.Activities.Add(actionDo);
                uow.ActionListRepository.Attach(actionListDo);

                uow.SaveChanges();
            }

            return processTemplateDO;
        }

        public static ProcessTemplateDO TestProcessTemplate3()
        {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            for (int i = 1; i <= 2; ++i)
            {
                var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    Id = i,
                    Name = string.Format("curProcessNodeTemplateDO-{0}", i),
                    ProcessTemplate = curProcessTemplateDO,
                    ActionLists = FixtureData.TestActionList1(),
                };
                curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);
            }

            return curProcessTemplateDO;
        }
    }
=======
﻿using Core.Interfaces;
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
        public static ProcessTemplateDO TestProcessTemplate1()
        {
            var processTemplate = new ProcessTemplateDO
            {
				Id = 33,
                Description = "descr 1",
                Name = "template1",
                ProcessTemplateState = ProcessTemplateState.Active,

              
                
            };
            return processTemplate;
        }

        public static ProcessTemplateDO TestProcessTemplate2()
        {
            var processTemplate = new ProcessTemplateDO
            {
                Id = 50,
                Description = "descr 2",
                Name = "template2",
                ProcessTemplateState = ProcessTemplateState.Active,

                //UserId = "testUser1"
                //DockyardAccount = FixtureData.TestDockyardAccount1()
            };
            return processTemplate;
        }

        public static ProcessTemplateDO TestProcessTemplateHealthDemo()
        {
            var healthProcessTemplate = new ProcessTemplateDO
            {
                Id = 23,
                Description = "DO-866 HealthDemo Integration Test",
                Name = "HealthDemoIntegrationTest",
                ProcessTemplateState = ProcessTemplateState.Active,
            };
            return healthProcessTemplate;
        }

        public static ProcessTemplateDO TestProcessTemplateWithProcessNodeTemplates()
        {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "DO-982 Process Node Template Test",
                Name = "ProcessTemplateWithProcessNodeTemplates",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            for (int i = 1; i <= 4; ++i)
            {
                var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    Id = i,
                    Name = string.Format("curProcessNodeTemplateDO-{0}", i),
                    ProcessTemplate = curProcessTemplateDO,
                };
                curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);
            }

            return curProcessTemplateDO;
        }

        public static ProcessTemplateDO TestProcessTemplateWithSubscribeEvent()
        {
            ProcessTemplateDO processTemplateDO;
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                DockyardAccountDO testUser = TestDockyardAccount1();
                uow.UserRepository.Add(testUser);

                processTemplateDO = new ProcessTemplateDO()
                {
                    Id = 23,
                    Description = "HealthDemo Integration Test",
                    Name = "StandardEventTesting",
                    ProcessTemplateState = ProcessTemplateState.Active,
                    DockyardAccount = testUser
                };
                uow.ProcessTemplateRepository.Add(processTemplateDO);

                var actionTemplate = ActionTemplate();

                var processDo = new ProcessDO()
                {
                    Id = 1,
                    CrateStorage = EnvelopeIdCrateJson(),
                    ProcessTemplateId = processTemplateDO.Id,
                    ProcessState = 1
                };
                uow.ProcessRepository.Add(processDo);



                ProcessNodeTemplateDO processNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    ProcessTemplate = processTemplateDO
                };
                uow.ProcessNodeTemplateRepository.Add(processNodeTemplateDO);


                var actionListDo = new ActionListDO()
                {
                    Process = processDo,
                    ProcessID = processDo.Id,
                    Id = 1,
                    ActionListType = ActionListType.Immediate,
                    ProcessNodeTemplate = processNodeTemplateDO,
                    Ordering = 2
                };
                uow.ActionListRepository.Add(actionListDo);

                var actionDo = new ActionDO()
                {
                    ParentActivity = actionListDo,
                    ParentActivityId = actionListDo.Id,
                    ActionState = ActionState.Unstarted,
                    Name = "testaction",

                    Id = 1,
                    ActivityTemplateId = actionTemplate.Id,
                    ActivityTemplate = actionTemplate,
                    Ordering = 1
                };
                ICrate crate = ObjectFactory.GetInstance<ICrate>();

                var serializer = new JsonSerializer();
                EventSubscriptionMS eventSubscriptionMS = new EventSubscriptionMS();
                eventSubscriptionMS.Subscriptions = new List<string>();
                eventSubscriptionMS.Subscriptions.Add("DocuSign Envelope Sent");
                eventSubscriptionMS.Subscriptions.Add("Write to SQL AZure");

                var eventReportJSON = serializer.Serialize(eventSubscriptionMS);

                CrateDTO crateDTO = crate.Create("Standard Event Subscriptions", eventReportJSON, "Standard Event Subscriptions");
                actionDo.UpdateCrateStorageDTO(new List<CrateDTO>() { crateDTO });

                uow.ActionRepository.Add(actionDo);
                actionListDo.Activities.Add(actionDo);
                uow.ActionListRepository.Attach(actionListDo);

                uow.SaveChanges();
            }

            return processTemplateDO;
        }

        public static ProcessTemplateDO TestProcessTemplate3()
        {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "DO-1040 Process Template Test",
                Name = "Poress template",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            for (int i = 1; i <= 2; ++i)
            {
                var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
                {
                    Id = i,
                    Name = string.Format("curProcessNodeTemplateDO-{0}", i),
                    ProcessTemplate = curProcessTemplateDO,
                    ActionLists = FixtureData.TestActionList1(),
                };
                curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);
            }

            return curProcessTemplateDO;
        }

        public static ProcessTemplateDO TestProcessTemplateWithStartingProcessNodeTemplates()
        {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Id = 1,
                Description = "DO-1124 Proper  deletion of ProcessTemplate",
                Name = "TestProcessTemplateWithStartingProcessNodeTemplates",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Id = 1,
                Name = string.Format("curProcessNodeTemplateDO-{0}", 1),
                ProcessTemplate = curProcessTemplateDO,
                StartingProcessNodeTemplate = true
            };
            curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);


            return curProcessTemplateDO;
        }

        public static ProcessTemplateDO TestProcessTemplateWithStartingProcessNodeTemplates_ID0()
            {
            var curProcessTemplateDO = new ProcessTemplateDO
            {
                Description = "DO-1124 Proper  deletion of ProcessTemplate",
                Name = "TestProcessTemplateWithStartingProcessNodeTemplates_ID0",
                ProcessTemplateState = ProcessTemplateState.Active,
                ProcessNodeTemplates = new List<ProcessNodeTemplateDO>(),
            };

            var curProcessNodeTemplateDO = new ProcessNodeTemplateDO()
            {
                Name = string.Format("curProcessNodeTemplateDO-{0}", 1),
                ProcessTemplate = curProcessTemplateDO,
                StartingProcessNodeTemplate = true
            };
            curProcessTemplateDO.ProcessNodeTemplates.Add(curProcessNodeTemplateDO);


            return curProcessTemplateDO;
        }
    }
>>>>>>> b0bea344ae15462f3bf208bbfca6c7338b61166d
}