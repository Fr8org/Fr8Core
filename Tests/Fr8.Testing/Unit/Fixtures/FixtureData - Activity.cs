using System;
using System.Collections.Generic;
using Data.Entities;

namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        

        public class GuidIdStore
        {
            private readonly static GuidIdStore _instance = new GuidIdStore();

            public static GuidIdStore Instance
            {
                get { return _instance; }
            }


            private Dictionary<int, Guid> _guids = new Dictionary<int, Guid>();
            private Dictionary<Guid, int> _reverse = new Dictionary<Guid, int>();

            public Guid Get(int id)
            {
                lock (_guids)
                {
                    Guid result;
                    if (!_guids.TryGetValue(id, out result))
                    {
                        result = Guid.NewGuid();
                        _guids.Add(id, result);
                        _reverse.Add(result, id);
                    }

                    return result;
                }
            }

            public int Reverse(Guid guid)
            {
                int result;
                if (!_reverse.TryGetValue(guid, out result))
                {
                    throw new ApplicationException("No Guid -> Int map found.");
                }

                return result;
            }
        }

        public static Guid GetTestGuidById(int id)
        {
            return GuidIdStore.Instance.Get(id);
        }

        public static int GetTestIdByGuid(Guid guid)
        {
            return GuidIdStore.Instance.Reverse(guid);
        }

        public static PlanNodeDO TestActivity57()
        {
            return new PlanNodeDO
            {
                Id = GetTestGuidById(57),
                Ordering = 2,
                ParentPlanNodeId = GetTestGuidById(54)
            };

        }

        public static PlanNodeDO TestActivityNotExists()
        {
            return new PlanNodeDO
            {
                Id = GetTestGuidById(57),
                Ordering = 2,
                ParentPlanNodeId = GetTestGuidById(54)
            };

        }

        public static PlanNodeDO TestActivityTree()
        {
            var tree = new PlanNodeDO
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                ChildNodes = new List<PlanNodeDO>
                {
                    new PlanNodeDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentPlanNodeId = GetTestGuidById(1)
                    },
                    new PlanNodeDO
                    {
                        Id = GetTestGuidById(43),
                        ParentPlanNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(43)
                            },
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentPlanNodeId = GetTestGuidById(43)
                            },
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentPlanNodeId = GetTestGuidById(43)
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
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentPlanNodeId = GetTestGuidById(52)
                            },
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(54),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new PlanNodeDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 1
                                    },
                                    new PlanNodeDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 2
                                    },
                                    new PlanNodeDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentPlanNodeId = GetTestGuidById(54),
                                        Ordering = 3
                                    },

                                }
                            },
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(55),
                                ParentPlanNodeId = GetTestGuidById(52),
                                Ordering = 3
                            },

                        }
                    },
                    new PlanNodeDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentPlanNodeId = GetTestGuidById(1),
                        ChildNodes = new List<PlanNodeDO>
                        {
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(60),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 1
                            },
                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(61),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 2,

                                ChildNodes = new List<PlanNodeDO>
                                {
                                    new PlanNodeDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 1
                                    },
                                    new PlanNodeDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 2
                                    },
                                    new PlanNodeDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentPlanNodeId = GetTestGuidById(61),
                                        Ordering = 3
                                    },
                                }
                            },

                            new PlanNodeDO
                            {
                                Id = GetTestGuidById(62),
                                ParentPlanNodeId = GetTestGuidById(59),
                                Ordering = 3
                            },
                        },

                    }
                }
            };

            FixParentActivityReferences(tree);

            return tree;

        }

        private static void FixParentActivityReferences(PlanNodeDO root)
        {
            var activitiesIndex = new Dictionary<Guid, PlanNodeDO>();

            TraverseActivityTree(root, activitiesIndex);

            foreach (var activityDo in activitiesIndex.Values)
            {
                PlanNodeDO temp = null;

                if (activityDo.ParentPlanNodeId != null)
                {
                    activitiesIndex.TryGetValue(activityDo.ParentPlanNodeId.Value, out temp);
                }

                activityDo.ParentPlanNode = temp;
            }
        }

        private static void TraverseActivityTree(PlanNodeDO root, Dictionary<Guid, PlanNodeDO> allActivities)
        {
            allActivities.Add(root.Id, root);

            foreach (var activityDo in root.ChildNodes)
            {
                TraverseActivityTree(activityDo, allActivities);
            }
        }

    }
}