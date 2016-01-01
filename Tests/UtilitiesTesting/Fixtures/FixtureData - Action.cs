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
        public static ActionDO TestActionWriteSqlServer1()
        {
            var actionTemplate = ActionTemplate();

            var curActionDO = new ActionDO
            {
                Id = GetTestGuidById(54),
                Name = "Write to Sql Server",
                CrateStorage = "",
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActivityTemplateDO ActionTemplate()
        {
            return new ActivityTemplateDO()
            {
                Id = 1,
                Name = "Send an Email",
                Terminal = new TerminalDO { Name = "Send an Email", Version = "1", Endpoint = "", TerminalStatus = TerminalStatus.Active },

                Version = "1"
            };
        }

        public static ActivityTemplateDO ActivityTemplateSMS()
        {
            return new ActivityTemplateDO()
            {
                Id = 1,
                Name = "Send a Text (SMS) Message",
                Terminal = new TerminalDO { Name = "Send a Text (SMS) Message", Version = "1", Endpoint = "", TerminalStatus = TerminalStatus.Active },
                Version = "1"
            };
        }

        public static ActionDO TestAction1()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Id = GetTestGuidById(1),
                Name = "Action 1",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
            return curActionDO;
        }

        public static ActionDO TestAction2()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Id = GetTestGuidById(2),
                Name = "Action 2",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public ActionDO TestAction3()
        {
            var actionTemplate = ActionTemplate();
            var origActionDO = new ActionDO()
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

        public static ActionDO TestAction4()
        {
            var actionTemplate = ActionTemplate();
            var curActionDO = new ActionDO
            {
                Id = GetTestGuidById(3),
                Name = "Send an Email",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActionDO TestAction5()
        {
            var actionTemplate = ActivityTemplateSMS();
            var curActionDO = new ActionDO
            {
                Id = GetTestGuidById(4),
                Name = "Send a Text (SMS) Message",
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
            return curActionDO;
        }

        public static ActionDO TestAction6()
        {
            var actionTemplate = ActionTemplate();
            actionTemplate.Name = null;

            return new ActionDO
            {
                Id = GetTestGuidById(6),
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction7()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = GetTestGuidById(7),
                Name = "Action 7",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 3,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction8(RouteNodeDO parentActivity)
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
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

        public static ActionDO TestAction10()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = GetTestGuidById(5),
                Name = "Action 5",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction9()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = GetTestGuidById(2),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction20()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = GetTestGuidById(1),
                Name = "Action 1",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 1,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction21()
        {
            var actionTemplate = ActionTemplate();
            return new ActionDO
            {
                Id = GetTestGuidById(2),
                Name = "Action 2",
                ParentRouteNodeId = GetTestGuidById(1),
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestAction22()
        {
            var actionTemplate = FixtureData.ActionTemplate();

            return new ActionDO
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

        public static ActionDO TestAction23()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActionDO
            {
                Id = GetTestGuidById(2),
                Name = "Action 2",
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO IntegrationTestAction()
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
                ParentRouteNode = routeDo
            };


            var actionDo = new ActionDO()
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
        
        public static ActionDO TestActionHealth1()
        {
            var actionDo = new ActionDO
            {
                Id = GetTestGuidById(1),

                Name = "testaction",
                CrateStorage = "config settings",
                ParentRouteNodeId = GetTestGuidById(88),
                ActivityTemplateId = FixtureData.TestActivityTemplate1().Id
            };
            return actionDo;
        }

        public static ActionDO TestActionUnstarted()
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
                ParentRouteNode = routeDo
            };

            using (var updater = ObjectFactory.GetInstance<ICrateManager>().UpdateStorage(() => containerDO.CrateStorage))
            {
                updater.CrateStorage.Add(GetEnvelopeIdCrate());
            }

            return new ActionDO
            {
                Id = GetTestGuidById(1),
                Name = "testaction",
                ParentRouteNode = routeDo,

                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate
            };
        }

        public static ActionDO TestActionAuthenticate1()
        {
            TerminalDO curTerminalDO = new TerminalDO()
            {
                Id = 1,
                Name = "AzureSqlServer",
                TerminalStatus = 1,
                Version = "1",
                AuthenticationType = AuthenticationType.None
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
            };

            ActionDO curActionDO = new ActionDO();
            curActionDO.Id = GetTestGuidById(3);
            curActionDO.ParentRouteNode = subroute;
            curActionDO.ParentRouteNodeId = subroute.Id;
            curActionDO.ActivityTemplateId = 1;
            curActionDO.ActivityTemplate = curActivityTemplateDO;
            curActionDO.Name = "testaction";

            subroute.ChildNodes.Add(curActionDO);

            //  curActionDO.ConfigurationSettings = "config settings";
            //  curActionDO.ParentActionListId = 1;

            // curActionListDO.Actions.Add(curActionDO);

            //   curActionDO.ParentActionList = curActionListDO;



            return curActionDO;
        }

        public static ActionDO WaitForDocuSignEvent_Action()
        {
            string templateId = "58521204-58af-4e65-8a77-4f4b51fef626";
            var actionTemplate = ActionTemplate();
            ICrateManager _crate = ObjectFactory.GetInstance<ICrateManager>();
            IAction _action = ObjectFactory.GetInstance<IAction>();

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

            var actionDo = new ActionDO()
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

        public static AuthorizationTokenDO TestActionAuthenticate2()
        {
            AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
            {
                Token = "TestToken",
                AuthorizationTokenState = AuthorizationTokenState.Active
            };
            return curAuthorizationTokenDO;
        }

        public static AuthorizationTokenDO TestActionAuthenticate3()
        {
            AuthorizationTokenDO curAuthorizationTokenDO = new AuthorizationTokenDO()
            {
                Token = "TestToken",
                AuthorizationTokenState = AuthorizationTokenState.Revoked
            };
            return curAuthorizationTokenDO;
        }

        public static ActionDO TestAction57()
        {
            return new ActionDO()
            {
                Id = GetTestGuidById(57),
                Ordering = 2,
                ParentRouteNodeId = GetTestGuidById(54)
            };

        }       

        public static ActionDO TestActionTree()
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
            
            ActionDO curAction = new ActionDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage=  crateStorage,
                 
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActionDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage = crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActionDO
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
                            new ActionDO
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
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                        CrateStorage=  crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 2,
                                        CrateStorage=  crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },

                                }
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(55),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage=  crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(61),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage=  crateStorage,
                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage=  crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },
                                }
                            },

                            new ActionDO
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

        public static ActionDO CreateTestActionTreeWithOnlyActionDo()
        {
            var curCratesDTO = FixtureData.TestCrateDTO1();
            var crateStorageDTO = new CrateStorage();
            crateStorageDTO.AddRange(curCratesDTO);
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            string crateStorage = JsonConvert.SerializeObject(crateManager.ToDto(crateStorageDTO));
            


            ActionDO curAction = new ActionDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActionDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                         CrateStorage=  crateStorage
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(52),
                        Ordering = 3,
                        ParentRouteNodeId = GetTestGuidById(1),
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(52),
                                CrateStorage=  crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(54),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                CrateStorage=  crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 2
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },

                                }
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(55),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage=  crateStorage
                            },

                        }
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage = crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage=  crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(61),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage=  crateStorage,
                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },
                                }
                            },

                            new ActionDO
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

        public static ActionDO TestActionStateActive()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActionDO
            {
                Id = GetTestGuidById(2),
                Name = "Action with state active",
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
        }

        public static ActionDO TestActionStateDeactive()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActionDO
            {
                Id = GetTestGuidById(2),
                Name = "Action with state deactive",
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
        }

        public static ActionDO TestActionStateError()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActionDO
            {
                Id = GetTestGuidById(2),
                Name = "Action with state error",
                Ordering = 2,
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
            };
        }

        public static ActionDO TestActionStateInProcess()
        {
            var actionTemplate = FixtureData.TestActivityTemplateDO1();
            return new ActionDO
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

        public static ActionDO ConfigureTestActionTree()
        {
            var crateStorageDTO = new CrateStorage();
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            string crateStorage = JsonConvert.SerializeObject(crateManager.ToDto(crateStorageDTO));


            ActionDO curAction = new ActionDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActionDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage=  crateStorage
                            },
                            new ActionDO
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
                            new ActionDO
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
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 2
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },

                                }
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(55),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 3,
                                CrateStorage = crateStorage
                            }
                        }
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage = crateStorage
                            },
                            new ActionDO
                            {
                                Id = GetTestGuidById(61),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 2,
                                CrateStorage = crateStorage,
                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 1,
                                        CrateStorage = crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 2,
                                        CrateStorage = crateStorage
                                    },
                                    new ActionDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 3,
                                        CrateStorage = crateStorage
                                    },
                                }
                            },

                            new ActionDO
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
        public static ActionDO TestActionTreeWithActionTemplates()
        {
           var curCratesDTO = FixtureData.TestCrateDTO1();
            var crateStorageDTO = new CrateStorage();
            crateStorageDTO.AddRange(curCratesDTO);
            var crateManager = ObjectFactory.GetInstance<ICrateManager>();
            string crateStorage = JsonConvert.SerializeObject(crateManager.ToDto(crateStorageDTO));
            var curActionTemplate = FixtureData.ActionTemplate();

            ActionDO curAction = new ActionDO()
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                CrateStorage = crateStorage,
                ActivityTemplate = curActionTemplate,
                ChildNodes = new List<RouteNodeDO>
                {
                    new ActionDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage=  crateStorage,
                         ActivityTemplate = curActionTemplate,
                    },
                    new ActionDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        CrateStorage = crateStorage,
                        ActivityTemplate = curActionTemplate,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43),
                                CrateStorage = crateStorage,
                                ActivityTemplate = curActionTemplate,
                            },
                            new ActionDO
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
                    new ActionDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                        CrateStorage = crateStorage,
                        ActivityTemplate = curActionTemplate,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new ActionDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1,
                                CrateStorage = crateStorage,
                                ActivityTemplate = curActionTemplate,
                            },
                            new ActionDO
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

        public static ActionDO TestActionProcess()
        {
            var actionDo = new ActionDO
            {
                Id = GetTestGuidById(1),
                Name = "Action 1",
                CrateStorage = "config settings",
                ParentRouteNodeId = GetTestGuidById(1),
                ActivityTemplateId = FixtureData.TestActivityTemplate1().Id
            };
            return actionDo;
        }

        public static ActionDO ConfigureTwilioAction()
        {
            var actionTemplate = FixtureData.TwilioActionTemplateDTO();

            var actionDO = new ActionDO()
            {
                Name = "testaction",
                Id = GetTestGuidById(57),
                ActivityTemplateId = actionTemplate.Id,
                ActivityTemplate = actionTemplate,
                CrateStorage = "",
            };

            return actionDO;
    }
    }
}