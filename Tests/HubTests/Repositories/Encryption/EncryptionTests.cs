using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Encryption;
using Data.Repositories.Plan;
using Data.States;
using Fr8.Infrastructure.Data.States;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;
using Fr8.Testing.Unit;
using Fr8.Testing.Unit.Fixtures;


namespace HubTests.Repositories.Encryption
{
    [TestFixture]
    [Category("EncryptionService")]
    public class EncryptionTests : BaseTest
    {
        private class EncryptionServiceMock : IEncryptionService
        {
            public readonly HashSet<string> DataToEncrypt = new HashSet<string>();
            public readonly HashSet<string> DecryptedData = new HashSet<string>(); 

            public byte[] EncryptData(string peerId, string data)
            {
                DataToEncrypt.Add(data);
                return Encrypt(peerId, data);
            }

            public static byte[] Encrypt(string peerId, string data)
            {
                return Encoding.Default.GetBytes(peerId + data);
            }

            public byte[] EncryptData(string peerId, byte[] data)
            {
                throw new NotImplementedException();
            }

            public string DecryptString(string peerId, byte[] encryptedData)
            {
                var decryptedString = Encoding.Default.GetString(encryptedData);

                if (string.IsNullOrEmpty(peerId))
                {
                    DecryptedData.Add(decryptedString);
                    return decryptedString;
                }

                if (!decryptedString.StartsWith(peerId))
                {
                    throw new InvalidOperationException("Can't decrypt data because it is belong to another user");
                }

                var decryptedData = decryptedString.Substring(peerId.Length);

                DecryptedData.Add(decryptedData);

                return decryptedData;
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
                PlanState = PlanState.Executing,
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
                            TerminalId = FixtureData.GetTestGuidById(2),
                            Id = FixtureData.GetTestGuidById(1),
                            Name = "New template",
                        },
                        ActivityTemplateId = FixtureData.GetTestGuidById(1),
                        AuthorizationToken = new AuthorizationTokenDO
                        {
                             TerminalID = FixtureData.GetTestGuidById(2),
                            Id = NewGuid(34),
                        },
                        AuthorizationTokenId = NewGuid(34),
                        Label = "label",
                        CrateStorage = "storage " + NewGuid(1),
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
                                    TerminalId = FixtureData.GetTestGuidById(2),
                                    Id = FixtureData.GetTestGuidById(1),
                                    Name = "New template",
                                },
                                ActivityTemplateId = FixtureData.GetTestGuidById(1),
                                RootPlanNodeId = NewGuid(13),
                                Id = NewGuid(2),
                                CrateStorage = "storage " + NewGuid(2),
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
                                    TerminalId = FixtureData.GetTestGuidById(2),
                                    Id = FixtureData.GetTestGuidById(1),
                                    Name = "New template",
                                },
                                CrateStorage = "storage " + NewGuid(3),
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
                    TerminalId = FixtureData.GetTestGuidById(1),

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
                    Id = FixtureData.GetTestGuidById(1),
                    TerminalStatus = TerminalStatus.Active,
                    Name = "terminal",
                    Label = "term",
                    Version = "1",
                    OperationalState = OperationalState.Active,
                    ParticipationState = ParticipationState.Approved,
                    Endpoint = "http://localhost:11111"
                });

                uow.AuthorizationTokenRepository.Add(new AuthorizationTokenDO
                {
                    TerminalID = FixtureData.GetTestGuidById(1),
                    Id = NewGuid(34),
                });

                uow.TerminalRepository.Add(new TerminalDO
                {
                    Name = "asdfasdf",
                    Label = "asdf",
                    Version = "1",
                    Id = FixtureData.GetTestGuidById(1),
                    TerminalStatus = 1,
                    OperationalState = OperationalState.Active,
                    ParticipationState = ParticipationState.Approved,
                    Endpoint = "http://localhost:11111"
                });
                uow.PlanRepository.Add(plan);
                uow.SaveChanges();
            }
        }

        [Test]
        public void IsEncryptionProviderCalledOnActivityInsert()
        {
            var container = ObjectFactory.Container.CreateChildContainer();
            var mockedEncryptionProvider = new EncryptionServiceMock();

            container.Configure(x =>
            {
                x.For<IPlanCache>().Use<PlanCache>().Singleton();
                x.For<IEncryptionService>().Use(mockedEncryptionProvider);
            });
            
            StoreTestPlan(container);
            
            Assert.AreEqual(3, mockedEncryptionProvider.DataToEncrypt.Count, "Invalid number of data encryption calls");

            var expected = new[]
            {
                "storage " + NewGuid(1),
                "storage " + NewGuid(2),
                "storage " + NewGuid(3),
            };

            foreach (var s in expected)
            {
                Assert.IsTrue(mockedEncryptionProvider.DataToEncrypt.Contains(s), "Missing encrypted data: " + s);
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activities = uow.PlanRepository.GetActivityQueryUncached().ToArray();

                foreach (var activity in activities)
                {
                    var exprectedEncryptedData = EncryptionServiceMock.Encrypt(activity.Fr8AccountId, "storage " + activity.Id);
                    
                    Assert.AreEqual(exprectedEncryptedData.Length, activity.EncryptedCrateStorage.Length, $"Activity {activity.Id} has invalid encrypted storage data. Invalid size.");

                    for (int i = 0; i < exprectedEncryptedData.Length; i++)
                    {
                        Assert.AreEqual(exprectedEncryptedData[i], activity.EncryptedCrateStorage[i], $"Activity {activity.Id} has invalid encrypted storage data at {i}");
                    }
                }
            }
        }
        
        [Test]
        public void IsEncryptionProviderCalledOnActivityLoad()
        {
            var container = ObjectFactory.Container.CreateChildContainer();
            var mockedEncryptionProvider = new EncryptionServiceMock();

            container.Configure(x =>
            {
                x.For<IPlanCache>().Use<PlanCache>().Singleton();
                x.For<IEncryptionService>().Use(mockedEncryptionProvider);
            });

            StoreTestPlan(container);

            container = ObjectFactory.Container.CreateChildContainer();
            mockedEncryptionProvider = new EncryptionServiceMock();

            container.Configure(x =>
            {
                x.For<IPlanCache>().Use<PlanCache>().Singleton();
                x.For<IEncryptionService>().Use(mockedEncryptionProvider);
            });


            var expected = new[]
            {
                "storage " + NewGuid(1),
                "storage " + NewGuid(2),
                "storage " + NewGuid(3),
            };

            using (var uow = container.GetInstance<IUnitOfWork>())
            {
                var activities = uow.PlanRepository.GetById<PlanNodeDO>(NewGuid(13)).GetDescendants().OfType<ActivityDO>().ToArray();

                Assert.AreEqual(3, mockedEncryptionProvider.DecryptedData.Count, "Invalid number of data decryption calls");

                foreach (var s in expected)
                {
                    Assert.IsTrue(mockedEncryptionProvider.DecryptedData.Contains(s), "Missing decrypted data: " + s);
                }

                foreach (var activity in activities)
                {
                    Assert.AreEqual("storage " + activity.Id, activity.CrateStorage, $"Activity {activity.Id} has invalid decrypted crate storage");
                    Assert.IsNull(activity.EncryptedCrateStorage, $"Encrypted crate storage data is present for activity {activity.Id}");
                }
            }
        }

        private static Guid NewGuid(int id)
        {
            return new Guid(id, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
        }
    }
}
