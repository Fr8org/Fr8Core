using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Encryption;
using Data.Repositories.Plan;
using Data.States;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting.Fixtures;

namespace HubTests.Repositories.Encryption
{
    [TestFixture]
    [Category("EncryptionProvider")]
    public class EncryptionTests
    {
        private class EncryptionProviderMock : IEncryptionProvider
        {
            public readonly HashSet<string> DataToEncrypt = new HashSet<string>();

            public byte[] EncryptData(string peerId, string data)
            {
                DataToEncrypt.Add(data);
                return null;
            }

            public byte[] EncryptData(string peerId, byte[] data)
            {
                throw new NotImplementedException();
            }

            public string DecryptString(string peerId, byte[] encryptedData)
            {
                return null;
            }

            public byte[] DecryptByteArray(string peerId, byte[] encryptedData)
            {
                throw new NotImplementedException();
            }
        }
        
        private PlanDO GenerateTestPlan()
        {
            return new PlanDO
            {
                Id = NewGuid(13),
                Name = "Plan",
                PlanState = PlanState.Active,
                Description = "PlanDesc",
                Fr8Account = new Fr8AccountDO()
                {
                    Id = "acoountId",
                    FirstName = "Account"
                },
                Fr8AccountId = "acoountId",
                ChildNodes =
                {
                    new ActivityDO
                    {
                        RootPlanNodeId = NewGuid(13),
                        Id = NewGuid(1),
                        ActivityTemplate = new ActivityTemplateDO
                        {
                            TerminalId = 1,
                            Id = FixtureData.GetTestGuidById(1),
                            Name = "New template",
                        },
                        ActivityTemplateId = FixtureData.GetTestGuidById(1),
                        AuthorizationToken = new AuthorizationTokenDO
                        {
                             TerminalID = 1,
                            Id = NewGuid(34),
                        },
                        AuthorizationTokenId = NewGuid(34),
                        Label = "label",
                        CrateStorage = "stroage " + NewGuid(1),
                        Ordering = 6666,
                        Fr8Account = new Fr8AccountDO()
                        {
                            Id = "acoountId",
                            FirstName = "Account"
                        },
                        Fr8AccountId = "acoountId",
                        ChildNodes =
                        {
                            new ActivityDO
                            {
                                ActivityTemplate = new ActivityTemplateDO
                                {
                                    TerminalId = 1,
                                    Id = FixtureData.GetTestGuidById(1),
                                    Name = "New template",
                                },
                                ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                RootPlanNodeId = NewGuid(13),
                                Id = NewGuid(2),
                                CrateStorage = "stroage " + NewGuid(2),
                                Fr8Account = new Fr8AccountDO()
                                {
                                    Id = "acoountId",
                                    FirstName = "Account"
                                },
                                Fr8AccountId = "acoountId",
                            },
                            new ActivityDO
                            {
                                ActivityTemplate = new ActivityTemplateDO
                                {
                                    TerminalId = 1,
                                    Id = FixtureData.GetTestGuidById(1),
                                    Name = "New template",
                                },
                                CrateStorage = "stroage " + NewGuid(3),
                                ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                RootPlanNodeId = NewGuid(13),
                                Id = NewGuid(3),
                                Fr8Account = new Fr8AccountDO()
                                {
                                    Id = "acoountId",
                                    FirstName = "Account"
                                },
                                Fr8AccountId = "acoountId",
                            }
                        }
                    }
                }
            };
        }

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }


        private void StoreTestPlan(IContainer container)
        {
            var plan = GenerateTestPlan();

            using (var uow = container.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(new ActivityTemplateDO
                {
                    TerminalId = 1,

                    Id = FixtureData.GetTestGuidById(1),
                    Name = "New template",
                });

                uow.UserRepository.Add(new Fr8AccountDO()
                {
                    Id = "acoountId",
                    FirstName = "Account"
                });

                uow.TerminalRepository.Add(new TerminalDO()
                {
                    Id = 1,
                    TerminalStatus = TerminalStatus.Active,
                    Name = "terminal",
                    Label = "term",
                    Version = "1"

                });

                uow.AuthorizationTokenRepository.Add(new AuthorizationTokenDO
                {
                    TerminalID = 1,
                    Id = NewGuid(34),
                });

                uow.TerminalRepository.Add(new TerminalDO
                {
                    Name = "asdfasdf",
                    Label = "asdf",
                    Version = "1",
                    Id = 1,
                    TerminalStatus = 1
                });
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
            }
        }

        [Test]
        public void IsEncryptionProviderCalledOnActivityInsert()
        {
            var container = ObjectFactory.Container.CreateChildContainer();
            var mockedEncryptionProvider = new EncryptionProviderMock();

            container.Configure(x =>
            {
                x.For<IPlanCache>().Use<PlanCache>().Singleton();
                x.For<IEncryptionProvider>().Use(mockedEncryptionProvider);
            });
            
            StoreTestPlan(container);
            
            Assert.AreEqual(3, mockedEncryptionProvider.DataToEncrypt.Count, "Invalid number of data encryption calls");

            var expected = new[]
            {
                "stroage " + NewGuid(1),
                "stroage " + NewGuid(2),
                "stroage " + NewGuid(3),
            };

            foreach (var s in expected)
            {
                Assert.IsTrue(mockedEncryptionProvider.DataToEncrypt.Contains(s), "Missing encrypted data: " + s);
            }
        }

        private static Guid NewGuid(int id)
        {
            return new Guid(id, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
        }
    }
}
