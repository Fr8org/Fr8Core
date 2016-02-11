using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories.Plan;
using Data.States;
using DayPilot.Web.Mvc.Events.Navigator;
using Hub.StructureMap;
using NUnit.Framework;
using StructureMap;
using UtilitiesTesting;

namespace DockyardTest.Repositories.Plan
{
   
    class PlanStorageProviderMonitor : IPlanStorageProvider
    {
        private RouteNodeDO _route;
        public RouteSnapshot.Changes SubmittedChanges;

        public PlanStorageProviderMonitor(RouteNodeDO route)
        {
            RouteTreeHelper.Visit(route, (x, y) =>
            {
                x.ParentRouteNodeId = y != null ? y.Id : (Guid?) null;
                x.ParentRouteNode = y;
            });

            _route = route;
        }

        public RouteNodeDO LoadPlan(Guid planMemberId)
        {

            return _route;
        }

        public void Update(RouteSnapshot.Changes changes)
        {
            SubmittedChanges = changes;
        }

        public IQueryable<PlanDO> GetPlanQuery()
        {
            throw new NotImplementedException();
        }

        public IQueryable<ActivityDO> GetActivityQuery()
        {
            throw new NotImplementedException();
        }

        public IQueryable<RouteNodeDO> GetNodesQuery()
        {
            throw new NotImplementedException();
        }
    }

    internal class ExpectedChanges
    {
        public readonly List<Guid> Deleted = new List<Guid>();
        public readonly List<Guid> Inserted = new List<Guid>();
        public readonly List<ExpectedObjectChange> Updates = new List<ExpectedObjectChange>();
    }

    internal class ExpectedObjectChange
    {
        public readonly Guid ObjectId;
        public readonly String PropertyName;
        public readonly object Value;

        public ExpectedObjectChange(Guid objectId, string propertyName, object value)
        {
            ObjectId = objectId;
            PropertyName = propertyName;
            Value = value;
        }
    }

    [TestFixture]
    [Category("PlanRepositoryTests")]
    public class PlanRepositoryTests : BaseTest
    {
        [SetUp]
        public void Setup()
        {
            base.SetUp();
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        private static Guid NewGuid(int id)
        {
            return new Guid(id, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0);
        }

        private RouteNodeDO GenerateRefTree()
        {
            return new ActivityDO
            {
                Id = NewGuid(1),
                Label = "Base1",
                ChildNodes =
                {
                    new ActivityDO
                    {
                        Id = NewGuid(2),
                        Label = "Base2",
                    },
                    new ActivityDO()
                    {
                        Id = NewGuid(3),
                        Label = "Base3",
                    }
                }
            };
        }

        private PlanDO GenerateTestRoute()
        {
            return new PlanDO
            {
                Id = NewGuid(13),
                Name = "Plan",
                RouteState = RouteState.Active,
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
                        RootRouteNodeId = NewGuid(13),
                        Id = NewGuid(1),
                        ActivityTemplate = new ActivityTemplateDO
                        {
                            TerminalId = 1,
                            Id = 1,
                            Name = "New template",
                        },
                        ActivityTemplateId = 1,
                        AuthorizationToken = new AuthorizationTokenDO
                        {
                             TerminalID = 1,
                            Id = NewGuid(34),
                        },
                        AuthorizationTokenId = NewGuid(34),
                        Label = "label",
                        CrateStorage = "crates",
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
                            Id = 1,
                            Name = "New template",
                        },
                        ActivityTemplateId = 1,
                                RootRouteNodeId = NewGuid(13),
                                Id = NewGuid(2),
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
                            Id = 1,
                            Name = "New template",
                        },
                        ActivityTemplateId = 1,
                                RootRouteNodeId = NewGuid(13),
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

        private static bool AreEquals(RouteNodeDO a, RouteNodeDO b)
        {
            var snapShotA = new RouteSnapshot(a, false);
            var snapShotB = new RouteSnapshot(b, false);

            return !snapShotB.Compare(snapShotA).HasChanges;
        }

        private void ValidateChanges(List<Guid> expected, List<RouteNodeDO> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            foreach (var id in expected)
            {
                var localId = id;
                Assert.IsTrue(actual.Any(x => x.Id == localId));
            }
        }

        private void ValidateChanges (ExpectedChanges expectedChanges, RouteSnapshot.Changes actualChanges)
        {
            ValidateChanges(expectedChanges.Inserted, actualChanges.Insert);
            ValidateChanges(expectedChanges.Deleted, actualChanges.Delete);

            Assert.AreEqual(expectedChanges.Updates.Count, actualChanges.Update.Count);

            foreach (var update in expectedChanges.Updates.GroupBy(x=>x.ObjectId))
            {
                var updateId = update.Key;
                var expectedUpdate = update.ToArray();
                var actualUpdate = actualChanges.Update.FirstOrDefault(x => x.Node.Id == updateId);
                
                Assert.NotNull(actualUpdate);
                Assert.AreEqual(expectedUpdate.Length, actualUpdate.ChangedProperties.Count);

                foreach (var propUpdate in expectedUpdate)
                {
                    var name = propUpdate.PropertyName;
                    var actualProp = actualUpdate.ChangedProperties.FirstOrDefault(x => x.Name == name);
                    
                    Assert.NotNull(actualProp);
                    Assert.AreEqual(propUpdate.Value, actualProp.GetValue(actualUpdate.Node));
                }
            }
        }

        [Test]
        public void CanGetById()
        {
            var provider = new PlanStorageProviderMonitor(GenerateRefTree());
            var repository = new PlanRepository(new PlanStorage(new PlanCache(new ExpirationStrategyMock()), provider));

            Assert.AreEqual(NewGuid(2), repository.GetById<ActivityDO>(NewGuid(2)).Id);
        }
        
        [Test]
        public void CanUpdateProperties()
        {
            var provider = new PlanStorageProviderMonitor(GenerateRefTree());
            var repository = new PlanRepository(new PlanStorage(new PlanCache(new ExpirationStrategyMock()), provider));
            var expectedChanges = new ExpectedChanges
            {
                Updates =
                {
                    new ExpectedObjectChange(NewGuid(2), "Label", "newName"),
                    new ExpectedObjectChange(NewGuid(3), "Label", "newName3")
                }
                
            };

            repository.GetById<ActivityDO>(NewGuid(2)).Label = "newName";
            repository.GetById<ActivityDO>(NewGuid(3)).Label = "newName3";

            repository.SaveChanges();

            ValidateChanges(expectedChanges, provider.SubmittedChanges);
        }

        [Test]
        public void CanUpdateStructure()
        {
            var provider = new PlanStorageProviderMonitor(GenerateRefTree());
            var repository = new PlanRepository(new PlanStorage(new PlanCache(new ExpirationStrategyMock()), provider));
            var expectedChanges = new ExpectedChanges
            {
                Inserted = { NewGuid(4) },
                Deleted = { NewGuid(3) }
            };

            repository.GetById<ActivityDO>(NewGuid(3)).Label = "newName";
            repository.GetById<ActivityDO>(NewGuid(3)).RemoveFromParent();
            repository.GetById<ActivityDO>(NewGuid(1)).ChildNodes.Add(new ActivityDO
            {
                Id = NewGuid(4),
                Label = "Base4",
            });

            repository.SaveChanges();

            ValidateChanges(expectedChanges, provider.SubmittedChanges);
        }
        
        [Test]
        public void CanAddRoute()
        {
            var provider = new PersistentPlanStorage(null);
            var cache = new PlanCache(new ExpirationStrategyMock());
            var repository = new PlanRepository(new PlanStorage(cache, provider));

            repository.Add(GenerateTestRoute());

            repository.SaveChanges();

            var loadedPlan = provider.LoadPlan(Guid.Empty);

            Assert.IsTrue(AreEquals(GenerateTestRoute(), loadedPlan));
            Assert.IsTrue(AreEquals(repository.GetById<PlanDO>(NewGuid(13)), GenerateTestRoute()));
        }


        [Test]
        public void CanAddRouteWithEmptyDefaultIds()
        {
            var provider = new PersistentPlanStorage(null);
            var cache = new PlanCache(new ExpirationStrategyMock());
            var repository = new PlanRepository(new PlanStorage(cache, provider));

            var refRoute = GenerateTestRoute();
            RouteTreeHelper.Visit(refRoute, x => x.Id = Guid.Empty);

            repository.Add(refRoute);
            repository.SaveChanges();

            RouteTreeHelper.Visit(refRoute, x => Assert.IsTrue(x.Id != Guid.Empty));

            var loadedPlan = provider.LoadPlan(Guid.Empty);

            Assert.IsTrue(AreEquals(refRoute, loadedPlan));
        }

        [Test]
        public void CanAddRouteInEF()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActivityTemplateRepository.Add(new ActivityTemplateDO
                {
                    TerminalId = 1,
                    
                    Id = 1,
                    Name = "New template",
                });

                uow.UserRepository.Add(new Fr8AccountDO()
                {
                    Id = "acoountId",
                    FirstName = "Account"
                });

                uow.TerminalRepository.Add(new TerminalDO()
                {
                    Id=1,
                    TerminalStatus = TerminalStatus.Active,
                    Name = "terminal",
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
                    Version = "1",
                    Id = 1,
                    TerminalStatus = 1
                });
                uow.PlanRepository.Add(GenerateTestRoute());
                uow.SaveChanges();
            }

            var container = ObjectFactory.Container.CreateChildContainer();

            container.Configure(x=>x.For<IPlanCache>().Use<PlanCache>().Singleton());

            using (var uow = container.GetInstance<IUnitOfWork>())
            {
                var loadedPlan = uow.PlanRepository.GetById<PlanDO>(NewGuid(13));
                 Assert.IsTrue(AreEquals(GenerateTestRoute(), loadedPlan));
            }
        }

        [Test]
        public void CanPersistPropertyChanges()
        {
            var provider = new PlanStorageProviderMonitor(GenerateRefTree());
            var cache = new PlanCache(new ExpirationStrategyMock());
            var repository = new PlanRepository(new PlanStorage(cache, provider));

            repository.GetById<ActivityDO>(NewGuid(2)).Label = "newName";
            repository.GetById<ActivityDO>(NewGuid(3)).Label = "newName3";

            repository.SaveChanges();

            repository = new PlanRepository(new PlanStorage(cache, provider));

            Assert.AreEqual("newName", repository.GetById<ActivityDO>(NewGuid(2)).Label);
            Assert.AreEqual("newName3", repository.GetById<ActivityDO>(NewGuid(3)).Label);
        }

    }
}
