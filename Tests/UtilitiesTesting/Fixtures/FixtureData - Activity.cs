using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;

namespace UtilitiesTesting.Fixtures
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

            public Guid Get(int id)
            {
                lock (_guids)
                {
                    Guid result;
                    if (!_guids.TryGetValue(id, out result))
                    {
                        _guids.Add(id, result);
                    }

                    return result;
                }
            }
        }

        public static Guid TestGuid_Id(int id)
        {
            return GuidIdStore.Instance.Get(id);
        }

        public static RouteNodeDO TestActivity57()
        {
            return new RouteNodeDO
            {
                Id = TestGuid_Id(57),
                Ordering = 2,
                ParentRouteNodeId = TestGuid_Id(54)
            };

        }

        public static RouteNodeDO TestActivityNotExists()
        {
            return new RouteNodeDO
            {
                Id = TestGuid_Id(57),
                Ordering = 2,
                ParentRouteNodeId = TestGuid_Id(54)
            };

        }

        public static RouteNodeDO TestActivityTree()
        {
            var tree = new RouteNodeDO
            {
                Id = TestGuid_Id(1),
                Ordering = 1,
                ChildNodes = new List<RouteNodeDO>
                {
                    new RouteNodeDO
                    {
                        Id = TestGuid_Id(23),
                        Ordering = 1,
                        ParentRouteNodeId = TestGuid_Id(1)
                    },
                    new RouteNodeDO
                    {
                        Id = TestGuid_Id(43),
                        ParentRouteNodeId = TestGuid_Id(1),
                        Ordering = 2,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(44),
                                Ordering = 1,
                                ParentRouteNodeId = TestGuid_Id(43)
                            },
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(46),
                                Ordering = 2,
                                ParentRouteNodeId = TestGuid_Id(43)
                            },
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(48),
                                Ordering = 3,
                                ParentRouteNodeId = TestGuid_Id(43)
                            },

                        }
                    },
                    new RouteNodeDO
                    {
                        Id = TestGuid_Id(52),
                        Ordering = 3,
                        ParentRouteNodeId = TestGuid_Id(1),
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(53),
                                Ordering = 1,
                                ParentRouteNodeId = TestGuid_Id(52)
                            },
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(54),
                                ParentRouteNodeId = TestGuid_Id(52),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new RouteNodeDO
                                    {
                                        Id = TestGuid_Id(56),
                                        ParentRouteNodeId = TestGuid_Id(54),
                                        Ordering = 1
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = TestGuid_Id(57),
                                        ParentRouteNodeId = TestGuid_Id(54),
                                        Ordering = 2
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = TestGuid_Id(58),
                                        ParentRouteNodeId = TestGuid_Id(54),
                                        Ordering = 3
                                    },

                                }
                            },
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(55),
                                ParentRouteNodeId = TestGuid_Id(52),
                                Ordering = 3
                            },

                        }
                    },
                    new RouteNodeDO
                    {
                        Id = TestGuid_Id(59),
                        Ordering = 4,
                        ParentRouteNodeId = TestGuid_Id(1),
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(60),
                                ParentRouteNodeId = TestGuid_Id(59),
                                Ordering = 1
                            },
                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(61),
                                ParentRouteNodeId = TestGuid_Id(59),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new RouteNodeDO
                                    {
                                        Id = TestGuid_Id(63),
                                        ParentRouteNodeId = TestGuid_Id(61),
                                        Ordering = 1
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = TestGuid_Id(64),
                                        ParentRouteNodeId = TestGuid_Id(61),
                                        Ordering = 2
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = TestGuid_Id(65),
                                        ParentRouteNodeId = TestGuid_Id(61),
                                        Ordering = 3
                                    },
                                }
                            },

                            new RouteNodeDO
                            {
                                Id = TestGuid_Id(62),
                                ParentRouteNodeId = TestGuid_Id(59),
                                Ordering = 3
                            },
                        },

                    }
                }
            };

            FixParentActivityReferences(tree);

            return tree;

        }

        private static void FixParentActivityReferences(RouteNodeDO root)
        {
            var activitiesIndex = new Dictionary<Guid, RouteNodeDO>();

            TraverseActivityTree(root, activitiesIndex);

            foreach (var activityDo in activitiesIndex.Values)
            {
                RouteNodeDO temp = null;

                if (activityDo.ParentRouteNodeId != null)
                {
                    activitiesIndex.TryGetValue(activityDo.ParentRouteNodeId.Value, out temp);
                }

                activityDo.ParentRouteNode = temp;
            }
        }

        private static void TraverseActivityTree(RouteNodeDO root, Dictionary<Guid, RouteNodeDO> allActivities)
        {
            allActivities.Add(root.Id, root);

            foreach (var activityDo in root.ChildNodes)
            {
                TraverseActivityTree(activityDo, allActivities);
            }
        }

    }
}