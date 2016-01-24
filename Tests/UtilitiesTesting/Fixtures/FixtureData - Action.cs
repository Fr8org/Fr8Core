using System;
using System.Collections.Generic;
using Data.Control;
using Data.Crates;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Hub.Interfaces;
using Hub.Managers;
using Newtonsoft.Json.Linq;

namespace UtilitiesTesting.Fixtures
{
    partial class FixtureData
    {
        public static ActivityDO TestActionWriteSqlServer1()
        {
            var actionTemplate = ActionTemplate();

            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(54),
                Name = "Write to Sql Server",
                CrateStorage = "",
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActivityDO;
        }

        public static ActivityTemplateDO ActionTemplate()
        {
            return new ActivityTemplateDO()
            {
                Id = 1,
                Name = "Send an Email",
                Terminal = new TerminalDO
                {
                    Name = "Send an Email",
                    Version = "1",
                    Endpoint = "",
                    TerminalStatus = TerminalStatus.Active,
                    Secret = Guid.NewGuid().ToString()
                },

                Version = "1"
            };
        }

        public static ActivityTemplateDO ActivityTemplateSMS()
        {
            return new ActivityTemplateDO()
            {
                Id = 1,
                Name = "Send a Text (SMS) Message",
                Terminal = new TerminalDO
                {
                    Name = "Send a Text (SMS) Message",
                    Version = "1",
                    Endpoint = "",
                    TerminalStatus = TerminalStatus.Active,
                    Secret = Guid.NewGuid().ToString()
                },
                Version = "1"
            };
        }

        public static ActivityDO TestActivity1()
        {
            var actionTemplate = ActionTemplate();
            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(1),
                Name = "Action 1",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
            return curActivityDO;
        }

        public static ActivityDO TestActivity2()
        {
            var actionTemplate = ActionTemplate();
            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(2),
                Name = "Action 2",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActivityDO;
        }

        public ActivityDO TestActivity3()
        {
            var actionTemplate = ActionTemplate();
            var origActionDO = new ActivityDO()
            {
                ParentRouteNodeId = null,
                Name = "type 1",
                Id = GetTestGuidById(34),
                CrateStorage = "config settings",

                Ordering = 3,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };

            return origActionDO;
        }

        public static ActivityDO TestActivity4()
        {
            var actionTemplate = ActionTemplate();
            var curActivityDO = new ActivityDO
            {
                Id = GetTestGuidById(3),
                Name = "Send an Email",
                ActivityTemplateId = actionTemplate.Id,
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
                Name = "Send a Text (SMS) Message",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActivityDO;
        }

        public static ActivityDO TestActivity6()
        {
            var actionTemplate = ActionTemplate();
            actionTemplate.Name = null;

            return new ActivityDO
            {
                Id = GetTestGuidById(6),
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity7()
        {
            var actionTemplate = ActionTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(7),
                Name = "Action 7",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 3,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity8(RouteNodeDO parentActivity)
        {
            var actionTemplate = ActionTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(8),
                Name = "Action 8",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 4,
                ParentRouteNode = parentActivity,

                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity10()
        {
            var actionTemplate = ActionTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(5),
                Name = "Action 5",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity9()
        {
            var actionTemplate = ActionTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity20()
        {
            var actionTemplate = ActionTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(1),
                Name = "Action 1",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity21()
        {
            var actionTemplate = ActionTemplate();
            return new ActivityDO
            {
                Id = GetTestGuidById(2),
                Name = "Action 2",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivity22()
        {
            var actionTemplate = FixtureData.ActionTemplate();

            return new ActivityDO
            {
                Id = GetTestGuidById(10),
                Name = "WriteToAzureSql",
                ParentRouteNodeId = GetTestGuidById(1),
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
                Name = "Action 2",
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO IntegrationTestActivity()
        {
            //string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";
            var actionTemplate = ActionTemplate();


            var routeDo = TestRoute2();

            var containerDO = new ContainerDO()
            {
                Id = TestContainer_Id_1(),
                ContainerState = 1,
                RouteId = routeDo.Id,
                Route = routeDo
            };

            var subrouteDo = new SubrouteDO()
            {
                Id = GetTestGuidById(1),
                Name = "C",
                ParentRouteNodeId = routeDo.Id,
                ParentRouteNode = routeDo,
                RootRouteNodeId = routeDo.Id,
                RootRouteNode = routeDo
            };


            var actionDo = new ActivityDO()
            {
                ParentRouteNode = subrouteDo,
                ParentRouteNodeId = GetTestGuidById(1),
                Name = "testaction",

                Id = GetTestGuidById(1),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(actionDo))
            {
                updater.CrateStorage.Add(GetEnvelopeIdCrate());
            }

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(()=>containerDO.CrateStorage))
            {
                updater.CrateStorage.Add(GetEnvelopeIdCrate());
            }

            return actionDo;
        }

        public static Crate GetEnvelopeIdCrate(string curEnvelopeId = "11f41f43-57bd-4568-86f5-9ceabdaafc43")
        {
            var crateFields = new List<FieldDTO>()
                    {
                        new FieldDTO() { Key = "EnvelopeId", Value = curEnvelopeId },
                        new FieldDTO() { Key = "ExternalEventType", Value = "1" },
                        new FieldDTO() { Key = "RecipientId", Value= "1" }
                    };


            return Crate.FromJson("Event Data", JToken.FromObject(crateFields));
        }
        
        public static ActivityDO TestActivityHealth1()
        {
            var actionDo = new ActivityDO
            {
                Id = GetTestGuidById(1),

                Name = "testaction",
                CrateStorage = "config settings",
                ParentRouteNodeId = GetTestGuidById(88),
                ActivityTemplateId = FixtureData.TestActivityTemplate1().Id
            };
            return actionDo;
        }

        public static ActivityDO TestActivityUnstarted()
        {
            var actionTemplate = ActionTemplate();
            //string envelopeId = "F02C3D55-F6EF-4B2B-B0A0-02BF64CA1E09";

            var routeDo = new RouteDO()
            {
                Id = GetTestGuidById(1),
                Name = "A",
                Description = "B",
                RouteState = RouteState.Active
            };

            var containerDO = new ContainerDO()
            {
                Id = TestContainer_Id_1(),
                ContainerState = 1,
                RouteId = routeDo.Id,
                Route = routeDo
            };

            var subrouteDo = new SubrouteDO()
            {
                Id = GetTestGuidById(1),
                Name = "C",
                ParentRouteNodeId = routeDo.Id,
                ParentRouteNode = routeDo,
                RootRouteNodeId = routeDo.Id,
                RootRouteNode = routeDo
            };

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
            {
                updater.CrateStorage.Add(GetEnvelopeIdCrate());
            }

            return new ActivityDO
            {
                Id = GetTestGuidById(1),
                Name = "testaction",
                ParentRouteNode = routeDo,

                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActivityDO TestActivityAuthenticate1()
        {
            TerminalDO curTerminalDO = new TerminalDO()
            {
                Id = 1,
                Name = "AzureSqlServer",
                TerminalStatus = 1,
                Version = "1",
                AuthenticationType = AuthenticationType.None,
                Secret = Guid.NewGuid().ToString()
            };

            ActivityTemplateDO curActivityTemplateDO = new ActivityTemplateDO
            {
                Id = 1,
                //ActionType = "Write to Sql Server",
                //ParentPluginRegistration = "pluginAzureSqlServer",
                Version = "v1",
                Terminal = curTerminalDO,
                TerminalId = 1,
            };



            var curRouteDO = new RouteDO
            {
                Id = GetTestGuidById(1),
                Description = "descr 1",
                Name = "template1",
                RouteState = RouteState.Active,
                Fr8Account = FixtureData.TestDockyardAccount1()
            };

            var curContainerDO = new ContainerDO()
            {
                Id = TestContainer_Id_1(),
                RouteId = GetTestGuidById(1),
                Route = curRouteDO
            };


            var subroute = new SubrouteDO(true)
            {
                ParentRouteNode = curRouteDO,
                ParentRouteNodeId = curRouteDO.Id,
                RootRouteNodeId = curRouteDO.Id,
                RootRouteNode = curRouteDO
            };

            ActivityDO curActivityDO = new ActivityDO();
            curActivityDO.Id = GetTestGuidById(3);
            curActivityDO.ParentRouteNode = subroute;
            curActivityDO.ParentRouteNodeId = subroute.Id;
            curActivityDO.ActivityTemplateId = 1;
            curActivityDO.ActivityTemplate = curActivityTemplateDO;
            curActivityDO.Name = "testaction";

            subroute.ChildNodes.Add(curActivityDO);

            //  curActivityDO.ConfigurationSettings = "config settings";
            //  curActivityDO.ParentActionListId = 1;

            // curActionListDO.Actions.Add(curActivityDO);

            //   curActivityDO.ParentActionList = curActionListDO;



            return curActivityDO;
        }

        public static ActivityDO WaitForDocuSignEvent_Activity()
        {
            string templateId = "58521204-58af-4e65-8a77-4f4b51fef626";
            var actionTemplate = ActionTemplate();
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

                Name = "testaction",

                Id = GetTestGuidById(1),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };

            var fields = new List<ControlDefinitionDTO>()
            {
                fieldSelectDockusignTemplate
            };

            using (var updater = _crate.UpdateStorage(actionDo))
            {
                updater.CrateStorage.Add(Crate.FromContent("Configuration_Controls", new StandardConfigurationControlsCM(fields)));
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

        public static ActivityDO TestAction57()
        {
            return new ActivityDO()
            {
                Id = GetTestGuidById(57),
                Ordering = 2,
                ParentRouteNodeId = GetTestGuidById(54)
            };

        }       

        public static ActivityDO TestActionTree()
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
                CrateStorage=  crateStorage,
                 
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage = crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new RouteNodeDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentRouteNodeId = GetTestGuidById(1),
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(52),
                                CrateStorage=  crateStorage
                            },
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(54),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                        CrateStorage=  crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 2,
                                        CrateStorage=  crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },

                                }
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(55),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(61),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage=  crateStorage,
                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage=  crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },
                                }
                            },

                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 3,
                                CrateStorage=  crateStorage
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
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentRouteNodeId = GetTestGuidById(1),
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(52),
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(54),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                CrateStorage=  crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 2
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },

                                }
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(55),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage = crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(61),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage=  crateStorage,
                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },
                                }
                            },

                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 3,
                                CrateStorage = crateStorage
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
                Name = "Action with state active",
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
                Name = "Action with state deactive",
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
                Name = "Action with state error",
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
                Name = "Action with state in-process",
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
//                RouteId = TestRoute2().Id,
//                ContainerState = 1
//            };
//
//            var actionDo = new ActionDO()
//            {
//                Name = "testaction",
//                Id = 57,
//                Ordering = 2,
//                ParentRouteNodeId = 54,
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
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new RouteNodeDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentRouteNodeId = GetTestGuidById(1),
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(52),
                                CrateStorage=  crateStorage
                            },
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(54),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 2
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },

                                }
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(55),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage = crateStorage
                            }
                        }
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage = crateStorage
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(61),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage = crateStorage,
                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage
                                    },
                                    new ActivityDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },
                                }
                            },

                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentRouteNodeId = GetTestGuidById(59),
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
            var curActionTemplate = FixtureData.ActionTemplate();

            ActivityDO curAction = new ActivityDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ActivityTemplate = curActionTemplate,
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActivityDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage,
                         ActivityTemplate = curActionTemplate,
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage = crateStorage,
                        ActivityTemplate = curActionTemplate,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage,
                                ActivityTemplate = curActionTemplate,
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43),
                               CrateStorage = crateStorage,
                                ActivityTemplate = curActionTemplate,
                            }
                        }
                    },
                    new RouteNodeDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentRouteNodeId = GetTestGuidById(1),
                    },
                    new ActivityDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage = crateStorage,
                        ActivityTemplate = curActionTemplate,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActivityDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage = crateStorage,
                                ActivityTemplate = curActionTemplate,
                            },
                            new ActivityDO
                            {
                                Id = GetTestGuidById(62),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 3,
                                CrateStorage = crateStorage,
                                ActivityTemplate = curActionTemplate,
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
                Name = "Action 1",
                CrateStorage = "config settings",
                ParentRouteNodeId = GetTestGuidById(1),
                ActivityTemplateId = FixtureData.TestActivityTemplate1().Id
            };
            return actionDo;
        }

        public static ActivityDO ConfigureTwilioActivity()
        {
            var actionTemplate = FixtureData.TwilioActionTemplateDTO();

            var activityDO = new ActivityDO()
            {
                Name = "testaction",
                Id = GetTestGuidById(57),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = "",
            };

            return activityDO;
    }
    }
}