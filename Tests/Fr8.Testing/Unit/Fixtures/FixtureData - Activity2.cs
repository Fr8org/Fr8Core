using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.States;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.States;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Managers;
using Newtonsoft.Json.Linq;
using Fr8.TerminalBase.Models;
using Fr8.TerminalBase.Interfaces;
using Fr8.Infrastructure.Data.Managers;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static ActivityDO TestActivityWriteSqlServer1()
        {
            var actionTemplate = ActivityTemplate();

            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(54),
                CrateStorage = "",
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActivityDO;
        }

        public static ActivityTemplateDO ActivityTemplate()
        {
            return new ActivityTemplateDO()
            {
                Id = GetTestGuidById(1),
                Name = "Send an Email",
                Terminal = new TerminalDO
                {
                    Name = "Send an Email",
                    Label = "Send an Email",
                    Version = "1",
                    Endpoint = "",
                    TerminalStatus = TerminalStatus.Active,
                    Secret = Guid.NewGuid().ToString(),
                    OperationalState = OperationalState.Active,
                    Id = FixtureData.GetTestGuidById(1),
                    ParticipationState = ParticipationState.Approved
                },

                Version = "1"
            };
        }
        public static ActivityTemplateSummaryDTO ActivityTemplate2()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Send an Email",
                TerminalName = "Send an Email",
                TerminalVersion = "1",
                Version = "1"
            };
        }

        public static ActivityTemplateSummaryDTO ActivityTemplateDummy()
        {
            return new ActivityTemplateSummaryDTO()
            {
                Name = "Test",
                TerminalName = "Test",
                TerminalVersion = "1",
                Version = "1"
            };
        }

        public static ActivityTemplateDO ActivityTemplateSMS()
        {
            return new ActivityTemplateDO()
            {
                Id = GetTestGuidById(1),
                Name = "Send a Text (SMS) Message",
                Terminal = new TerminalDO
                {
                    Name = "Send a Text (SMS) Message",
                    Label = "Send a Text (SMS) Message",
                    Version = "1",
                    Endpoint = "",
                    OperationalState = OperationalState.Active,
                    TerminalStatus = TerminalStatus.Active,
                    Secret = Guid.NewGuid().ToString()
                },
                Version = "1"
            };
        }

        public static ActivityDO TestActivity1()
        {
            var actionTemplate = ActivityTemplate();
            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(1),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
            return curActivityDO;
        }

        public static ActivityDO TestActivity2()
        {
            var actionTemplate = ActivityTemplate();
            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(2),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActivityDO;
        }

        public ActivityDO TestActivity3()
        {
            var actionTemplate = ActivityTemplate();
            var origActionDO = new ActivityDO()
            {
                ParentPlanNodeId = null,
                Id = GetTestGuidById(34),
                CrateStorage = "{}",

                Ordering = 3,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };

            return origActionDO;
        }

        public static ActivityDO TestActivity4()
        {
            var actionTemplate = ActivityTemplate();
            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(3),
                ActivityTemplateId = actionTemplate.Id,
                Fr8Account = FixtureData.TestDockyardAccount1(),
                ActivityTemplate = actionTemplate
            };
            return curActivityDO;
        }

        public static ActivityDO TestActivity5()
        {
            var actionTemplate = ActivityTemplateSMS();
            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(4),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActivityDO;
        }

        public static ActivityDO TestActivity6()
        {
            var actionTemplate = ActivityTemplate();
            actionTemplate.Name = null;

            return new ActivityDO
            {
                Id = GetTestGuidById(6),
                ParentPlanNodeId = GetTestGuidById(1),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity7()
        {
            var actionTemplate = ActivityTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(7),
                ParentPlanNodeId = GetTestGuidById(1),
                Ordering = 3,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity8(PlanNodeDO parentActivity)
        {
            var actionTemplate = ActivityTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(8),
                ParentPlanNodeId = GetTestGuidById(1),
                Ordering = 4,
                ParentPlanNode = parentActivity,

                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity10()
        {
            var actionTemplate = ActivityTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(5),
                ParentPlanNodeId = GetTestGuidById(1),
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity9()
        {
            var actionTemplate = ActivityTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity20()
        {
            var actionTemplate = ActivityTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(1),
                ParentPlanNodeId = GetTestGuidById(1),
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity21()
        {
            var actionTemplate = ActivityTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                ParentPlanNodeId = GetTestGuidById(1),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity22()
        {
            var actionTemplate = FixtureData.ActivityTemplate();

            return new ActivityDO
            {
                Id = GetTestGuidById(10),
                ParentPlanNodeId = GetTestGuidById(1),
                CrateStorage = "JSON Config Settings",

                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate

            };
        }

        public static ActivityDO TestActivity23()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityContext TestActivityContext1()
        {
            var activityTemplateDTO = new ActivityTemplateSummaryDTO
            {
                Name = "Type1",
                Version = "1"
            };
            var activityPayload = new ActivityPayload
            {
                Id = GetTestGuidById(2),
                Name = "Type2",
                ActivityTemplate = activityTemplateDTO,
                CrateStorage = new CrateStorage()
            };
            var activityContext = new ActivityContext
            {
               // HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = activityPayload,
                AuthorizationToken = AuthToken_TerminalIntegration()
            };
            return activityContext;
        }
        public static ActivityContext TestActivityContextWithoutAuthorization()
        {
            var activityPayload = new ActivityPayload
            {
                Id = GetTestGuidById(2),
                ActivityTemplate = ActivityTemplate2(),
                CrateStorage = new CrateStorage()
            };
            var activityContext = new ActivityContext
            {
               // HubCommunicator = ObjectFactory.GetInstance<IHubCommunicator>(),
                ActivityPayload = activityPayload,
            };
            return activityContext;
        }
        public static ContainerExecutionContext ContainerExecutionContext1()
        {
            var containerExecutionContext = new ContainerExecutionContext
            {
                PayloadStorage = PayloadDTO2(),
                ContainerId = TestContainer_Id_1()
            };
            
            return containerExecutionContext;
        }

        public static ActivityDO IntegrationTestActivity()
        {
            //string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var actionTemplate = ActivityTemplate();


            var planDo = TestPlan2();

            var containerDO = new ContainerDO()
            {
                Id = TestContainer_Id_1(),
                State = 1,
                PlanId = planDo.Id,
                Plan = planDo
            };

            var subPlanDo = new SubplanDO()
            {
                Id = GetTestGuidById(1),
                Name = "C",
                ParentPlanNodeId = planDo.Id,
                ParentPlanNode = planDo,
                RootPlanNodeId = planDo.Id,
            };


            var actionDo = new ActivityDO()
            {
                ParentPlanNode = subPlanDo,
                ParentPlanNodeId = GetTestGuidById(1),
                Id = GetTestGuidById(1),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().GetUpdatableStorage(actionDo))
            {
                crateStorage.Add(GetEnvelopeIdCrate());
            }

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
            {
                crateStorage.Add(GetEnvelopeIdCrate());
            }

            return actionDo;
        }

        public static Crate GetEnvelopeIdCrate(string curEnvelopeId = "11f41f43-57bd-4568-86f5-9ceabdaafc43")
        {
            var crateFields = new List<KeyValueDTO>()
                    {
                        new KeyValueDTO() { Key = "EnvelopeId", Value = curEnvelopeId },
                        new KeyValueDTO() { Key = "ExternalEventType", Value = "1" },
                        new KeyValueDTO() { Key = "RecipientId", Value= "1" }
                    };


            return Crate.FromJson("Event Data", JToken.FromObject(crateFields));
        }
        
        public static ActivityDO TestActivityHealth1()
        {
            var actionDo = new ActivityDO
            {
                Id = GetTestGuidById(1),

                CrateStorage = "config settings",
                ParentPlanNodeId = GetTestGuidById(88),
                ActivityTemplateId = FixtureData.TestActivityTemplate1().Id
            };
            return actionDo;
        }

        public static ActivityDO TestActivityUnstarted()
        {
            var actionTemplate = ActivityTemplate();
            //string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";

            var planDo = new PlanDO()
            {
                Id = GetTestGuidById(1),
                Name = "A",
                Description = "B",
                PlanState = PlanState.Executing
            };

            var containerDO = new ContainerDO()
            {
                Id = TestContainer_Id_1(),
                State = 1,
                PlanId = planDo.Id,
                Plan = planDo
            };

            var subPlanDo = new SubplanDO()
            {
                Id = GetTestGuidById(1),
                Name = "C",
                ParentPlanNodeId = planDo.Id,
                ParentPlanNode = planDo,
                RootPlanNodeId = planDo.Id,
            };

            using (var crateStorage = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
            {
                crateStorage.Add(GetEnvelopeIdCrate());
            }

            return new ActivityDO
            {
                Id = GetTestGuidById(1),
                ParentPlanNode = planDo,

                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivityAuthenticate1()
        {
            TerminalDO curTerminalDO = new TerminalDO()
            {
                Id = FixtureData.GetTestGuidById(1),
                Name = "AzureSqlServer",
                Label = "AzureSqlServer",
                TerminalStatus = 1,
                Version = "1",
                AuthenticationType = AuthenticationType.None,
                Secret = Guid.NewGuid().ToString()
            };

            ActivityTemplateDO curActivityTemplateDO = new ActivityTemplateDO
            {
                Id = GetTestGuidById(1),
                //ActionType = "Write to Sql Server",
                //ParentPluginRegistration = "pluginAzureSqlServer",
                Version = "v1",
                Terminal = curTerminalDO,
                TerminalId = FixtureData.GetTestGuidById(1),
            };



            var curPlanDO = new PlanDO
            {
                Id = GetTestGuidById(1),
                Description = "descr 1",
                Name = "template1",
                PlanState = PlanState.Executing,
                Fr8Account = FixtureData.TestDockyardAccount1()
            };

            var curContainerDO = new ContainerDO()
            {
                Id = TestContainer_Id_1(),
                PlanId = GetTestGuidById(1),
                Plan = curPlanDO
            };


            var subPlan = new SubplanDO(true)
            {
                ParentPlanNode = curPlanDO,
                ParentPlanNodeId = curPlanDO.Id,
                RootPlanNodeId = curPlanDO.Id,
              //  RootRouteNode = curPlanDO
            };

            ActivityDO curActivityDO = new ActivityDO();
            curActivityDO.Id = GetTestGuidById(3);
            curActivityDO.ParentPlanNode = subPlan;
            curActivityDO.ParentPlanNodeId = subPlan.Id;
            curActivityDO.ActivityTemplateId = GetTestGuidById(1);
            curActivityDO.ActivityTemplate = curActivityTemplateDO;
            
            subPlan.ChildNodes.Add(curActivityDO);

            //  curActivityDO.ConfigurationSettings = "config settings";
            //  curActivityDO.ParentActionListId = 1;

            // curActionListDO.Actions.Add(curActivityDO);

            //   curActivityDO.ParentActionList = curActionListDO;



            return curActivityDO;
        }


        public static PlanDO GetPlan(PlanNodeDO node)
        {
            while (node != null)
            {
                if (node is PlanDO)
                {
                    return (PlanDO)node;
                }

                node = node.ParentPlanNode;
            }

            throw new InvalidOperationException("No Plan found for activity");
        }

        public static ActivityDO WaitForDocuSignEvent_Activity()
        {
            string templateId = "58521204-58af-4e65-8a77-4f4b51fef626";
            var actionTemplate = ActivityTemplate();
            ICrateManager _crate = ObjectFactory.GetInstance<ICrateManager>();
            IActivity _activity = ObjectFactory.GetInstance<IActivity>();

            var fieldSelectDockusignTemplate = new DropDownList()
            {
                Label = "Select DocuSign Template",
                Name = "Selected_DocuSign_Template",
                Required = true,
                Value = templateId,
                Events = new List<ControlEvent>() {
                     new ControlEvent("onSelect", "requestConfiguration")
                }
            };

            var actionDo = new ActivityDO()
            {
                
                Id = GetTestGuidById(1),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };

            var fields = new List<ControlDefinitionDTO>()
            {
                fieldSelectDockusignTemplate
            };

            using (var crateStorage = _crate.GetUpdatableStorage(actionDo))
            {
                crateStorage.Add(Crate.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(fields)));
            }

            return actionDo;
        }

        public static AuthorizationTokenDO TestActivityAuthenticate2()
        {
            AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
            {
                Token = "TestToken",
                AuthorizationTokenState = AuthorizationTokenState.Active
            };
            return curAuthorizationTokenDO;
        }

        public static AuthorizationTokenDO TestActivityAuthenticate3()
        {
            AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
            {
                Token = "TestToken",
                AuthorizationTokenState = AuthorizationTokenState.Revoked
            };
            return curAuthorizationTokenDO;
        }

        public static ActivityPayload TestAction257()
        {
            return new ActivityPayload()
            {
                Id = GetTestGuidById(57),
                Ordering = 2,
                ParentPlanNodeId = GetTestGuidById(54),
                ActivityTemplate = ActivityTemplateDummy(),
                CrateStorage = new CrateStorage()
                //ActivityTemplateId = GetTestGuidById(1)
            };

        }       

        public static ActivityDO TestActivity2Tree()
        {
            var curCratesDTO1 = FixtureData.TestCrateDTO1();
            var curCratesDTO2 = FixtureData.TestCrateDTO2();
            var curCratesDTO3 = FixtureData.TestCrateDTO3();
            var crateStorageDTO = new CrateStorage();
            crateStorageDTO.AddRange(curCratesDTO1);
            crateStorageDTO.AddRange(curCratesDTO2);
            crateStorageDTO.AddRange(curCratesDTO3);
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            string crateStorage = JsonConvert.SerializeObject(crateManager.ToDto(crateStorageDTO));
            
            ActivityDO curAction = new ActivityDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ActivityTemplateId = GetTestGuidById(1),
                ChildNodes = new List<PlanNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentPlanNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentPlanNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage = crateStorage,
                         ActivityTemplateId = GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                            },

                        }
                    },
                    new PlanNodeDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentPlanNodeId = GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(52),
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                            },
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(54),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                        CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 2,
                                        CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage,
                         ActivityTemplateId =GetTestGuidById(1)
                                    },

                                }
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(55),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                            },

                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentPlanNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage,
                         ActivityTemplateId =GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(61),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1),
                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                                    },
                                }
                            },

                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 3,
                                CrateStorage=  crateStorage,
                         ActivityTemplateId = GetTestGuidById(1)
                            },
                        },

                    }
                }
            };
            return curAction;
        }

        public static ActivityDO CreateTestActivityTreeWithOnlyActivityDo()
        {
            
            var curCratesDTO = FixtureData.TestCrateDTO1();
            var crateStorageDTO = new CrateStorage();
            crateStorageDTO.AddRange(curCratesDTO);
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            string crateStorage = JsonConvert.SerializeObject(crateManager.ToDto(crateStorageDTO));
            


            ActivityDO curAction = new ActivityDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ActivityTemplateId = GetTestGuidById(1),
                ChildNodes = new List<PlanNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentPlanNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage,
                        ActivityTemplateId =GetTestGuidById(1)
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentPlanNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage=  crateStorage,
                        ActivityTemplateId = GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage,
                                ActivityTemplateId =GetTestGuidById(1)
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                            },

                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentPlanNodeId = GetTestGuidById(1),
                        ActivityTemplateId = GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(52),
                                CrateStorage=  crateStorage,
                                ActivityTemplateId =GetTestGuidById(1)
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(54),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 2,
                                ActivityTemplateId =GetTestGuidById(1),
                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                        CrateStorage=  crateStorage,
                                        ActivityTemplateId =GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 2,
                                        ActivityTemplateId = GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage,
                                        ActivityTemplateId = GetTestGuidById(1)
                                    },

                                }
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(55),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage=  crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                            },

                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentPlanNodeId = GetTestGuidById(1),
                        CrateStorage = crateStorage,
                        ActivityTemplateId = GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage=  crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(61),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage=  crateStorage,
                                ActivityTemplateId = GetTestGuidById(1),
                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage = crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                                    },
                                }
                            },

                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 3,
                                CrateStorage = crateStorage,
                                ActivityTemplateId = GetTestGuidById(1)
                            },
                        },

                    }
                }
            };

            FixParentActivityReferences(curAction);

            return curAction;
        }

        public static ActivityDO TestActivityStateActive()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
        }

        public static ActivityDO TestActivityStateDeactive()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
        }

        public static ActivityDO TestActivityStateError()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
        }

        public static ActivityDO TestActivityStateInProcess()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
        }

//        public static ActionDO ConfigureTestAction57()
//        {
//            var actionTemplate = ActionTemplate();
//
//            var containerDO = new ContainerDO()
//            {
//                Id = 1,
//                CrateStorage = EnvelopeIdCrateJson(),
//                planId = TestPlan2().Id,
//                ContainerState = 1
//            };
//
//            var actionDo = new ActionDO()
//            {
//                Name = "testaction",
//                Id = 57,
//                Ordering = 2,
//                ParentPlanNodeId = 54,
//                ActivityTemplateId = actionTemplate.Id,
//                ActivityTemplate = actionTemplate,
//                CrateStorage = EnvelopeIdCrateJson()
//            };
//
//            return actionDo;
//        }

        public static ActivityDO ConfigureTestActivityTree()
        {
            var crateStorageDTO = new CrateStorage();
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            string crateStorage = JsonConvert.SerializeObject(crateManager.ToDto(crateStorageDTO));


            ActivityDO curAction = new ActivityDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ChildNodes = new List<PlanNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentPlanNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentPlanNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new PlanNodeDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentPlanNodeId = GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(52),
                                CrateStorage=  crateStorage
                            },
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(54),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 2
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },

                                }
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(55),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage = crateStorage
                            }
                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentPlanNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage = crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(61),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage = crateStorage,
                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },
                                }
                            },

                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 3,
                                CrateStorage=  crateStorage
                            },
                        },

                    }
                }
            };

            FixParentActivityReferences(curAction);

            return curAction;
        }
        public static ActivityDO TestActivityTreeWithActivityTemplates()
        {
           var curCratesDTO = FixtureData.TestCrateDTO1();
            var crateStorageDTO = new CrateStorage();
            crateStorageDTO.AddRange(curCratesDTO);
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            string crateStorage = JsonConvert.SerializeObject(crateManager.ToDto(crateStorageDTO));
            var curActionTemplate = FixtureData.ActivityTemplate();

            ActivityDO curAction = new ActivityDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ActivityTemplate = curActionTemplate,
                ActivityTemplateId = curActionTemplate.Id,
                ChildNodes = new List<PlanNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentPlanNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage,
                         ActivityTemplate = curActionTemplate,
                          ActivityTemplateId = curActionTemplate.Id,
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentPlanNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage = crateStorage,
                        ActivityTemplate = curActionTemplate,
                         ActivityTemplateId = curActionTemplate.Id,
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage,
                              
                                ActivityTemplate = curActionTemplate,
                                 ActivityTemplateId = curActionTemplate.Id,
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentPlanNodeId = GetTestGuidById(43),
                               CrateStorage = crateStorage,
                               
                                ActivityTemplate = curActionTemplate,
                                 ActivityTemplateId = curActionTemplate.Id,
                            }
                        }
                    },
                    new PlanNodeDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentPlanNodeId = GetTestGuidById(1),
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentPlanNodeId = GetTestGuidById(1),
                        CrateStorage = crateStorage,
                        ActivityTemplate = curActionTemplate,
                         ActivityTemplateId = curActionTemplate.Id,
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage = crateStorage,
                                
                                ActivityTemplate = curActionTemplate,
                                 ActivityTemplateId = curActionTemplate.Id,
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 3,
                                CrateStorage = crateStorage,
                               
                                ActivityTemplate = curActionTemplate,
                                 ActivityTemplateId = curActionTemplate.Id,
                            }
                        }
                    }
                }
            };

            FixParentActivityReferences(curAction);
            return curAction;
        }

        public static ActivityDO TestActivityProcess()
        {
            var actionDo = new ActivityDO
            {
                Id = GetTestGuidById(1),
                CrateStorage = "config settings",
                ParentPlanNodeId = GetTestGuidById(1),
                ActivityTemplateId = FixtureData.TestActivityTemplate1().Id
            };
            return actionDo;
        }

        public static ActivityDO ConfigureTwilioActivity()
        {
            var actionTemplate = FixtureData.TwilioActivityTemplateDTO();

            var activityDO = new ActivityDO()
            {
                Id = GetTestGuidById(57),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = "",
            };

            return activityDO;
    }
    }
}