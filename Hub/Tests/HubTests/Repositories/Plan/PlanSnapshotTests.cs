using System;
using Data.Entities;
using Data.Repositories.Plan;
using Hub.StructureMap;
using NUnit.Framework;
using Fr8.Testing.Unit;
using Assert = NUnit.Framework.Assert;

namespace HubTests.Repositories.Plan
{
    [TestFixture]
    [Category("PlanRepositoryTests")]
    public class PlanSnapshotTests : BaseTest
    {

        [SetUp]
        public void Setup()
        {
            base.SetUp();
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }

        private PlanNodeDO GenerateRefTree()
        {
            return new ActivityDO
            {
                Id = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0),
                Label = "Base1",
                ChildNodes =
                {
                    new ActivityDO()
                    {
                        Id = new Guid(2, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base2",
                    },
                    new ActivityDO()
                    {
                        Id = new Guid(3, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base3",
                    }
                }
            };
        }

        private PlanNodeDO GenerateChangedTree_v1()
        {
            return new ActivityDO
            {
                Id = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0),
                Label = "Base1_",
                ChildNodes =
                {
                    new ActivityDO()
                    {
                        Id = new Guid(2, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base2_",
                    },
                    new ActivityDO()
                    {
                        Id = new Guid(3, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base3_",
                    }
                }
            };
        }

        private PlanNodeDO GenerateChangedTree_v3()
        {
            return new ActivityDO
            {
                Id = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0),
                Label = "Base1",
                ChildNodes =
                {
                    new ActivityDO()
                    {
                        Id = new Guid(4, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base4",
                    },

                    new ActivityDO()
                    {
                        Id = new Guid(2, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base2",
                    },

                    new ActivityDO()
                    {
                        Id = new Guid(3, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base3",
                    }
                }
            };
        }


        private PlanNodeDO GenerateChangedTree_v2()
        {
            return new ActivityDO
            {
                Id = new Guid(1, (short)0, (short)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0, (byte)0),
                Label = "Base1",
                ChildNodes =
                {
                    new ActivityDO()
                    {
                        Id = new Guid(3, (short) 0, (short) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0, (byte) 0),
                        Label = "Base3",
                    }
                }
            };
        }

        [Test]
        public void CanDetectPropertiesChanges()
        {
            var snapshot1 = new PlanSnapshot(GenerateRefTree(), false);
            var snapshot2 = new PlanSnapshot(GenerateChangedTree_v1(), false);

            var changes = snapshot2.Compare(snapshot1);

            Assert.AreEqual(0, changes.Insert.Count);
            Assert.AreEqual(0, changes.Delete.Count);
            Assert.AreEqual(3, changes.Update.Count);

        }

        [Test]
        public void CanDetectDeletion()
        {
            var snapshot1 = new PlanSnapshot(GenerateRefTree(), false);
            var snapshot2 = new PlanSnapshot(GenerateChangedTree_v2(), false);

            var changes = snapshot2.Compare(snapshot1);

            Assert.AreEqual(0, changes.Insert.Count);
            Assert.AreEqual(1, changes.Delete.Count);
            Assert.AreEqual(0, changes.Update.Count);

        }

        [Test]
        public void CanDetectInsertion()
        {
            var snapshot1 = new PlanSnapshot(GenerateRefTree(), false);
            var snapshot2 = new PlanSnapshot(GenerateChangedTree_v3(), false);

            var changes = snapshot2.Compare(snapshot1);

            Assert.AreEqual(1, changes.Insert.Count);
            Assert.AreEqual(0, changes.Delete.Count);
            Assert.AreEqual(0, changes.Update.Count);

        }

    }
}
