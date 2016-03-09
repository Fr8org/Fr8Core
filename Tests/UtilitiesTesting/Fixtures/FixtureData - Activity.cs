using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
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

        public static RouteNodeDO TestActivity57()
        {
            return new RouteNodeDO
            {
                Id = GetTestGuidById(57),
                Ordering = 2,
                ParentRouteNodeId = GetTestGuidById(54)
            };

        }

        public static RouteNodeDO TestActivityNotExists()
        {
            return new RouteNodeDO
            {
                Id = GetTestGuidById(57),
                Ordering = 2,
                ParentRouteNodeId = GetTestGuidById(54)
            };

        }

        public static RouteNodeDO TestActivityTree()
        {
            var tree = new RouteNodeDO
            {
                Id = GetTestGuidById(1),
                Ordering = 1,
                ChildNodes = new List<RouteNodeDO>
                {
                    new RouteNodeDO
                    {
                        Id = GetTestGuidById(23),
                        Ordering = 1,
                        ParentRouteNodeId = GetTestGuidById(1)
                    },
                    new RouteNodeDO
                    {
                        Id = GetTestGuidById(43),
                        ParentRouteNodeId = GetTestGuidById(1),
                        Ordering = 2,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(44),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(43)
                            },
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(46),
                                Ordering = 2,
                                ParentRouteNodeId = GetTestGuidById(43)
                            },
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(48),
                                Ordering = 3,
                                ParentRouteNodeId = GetTestGuidById(43)
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
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(53),
                                Ordering = 1,
                                ParentRouteNodeId = GetTestGuidById(52)
                            },
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(54),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new RouteNodeDO
                                    {
                                        Id = GetTestGuidById(56),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 1
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = GetTestGuidById(57),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 2
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = GetTestGuidById(58),
                                        ParentRouteNodeId = GetTestGuidById(54),
                                        Ordering = 3
                                    },

                                }
                            },
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(55),
                                ParentRouteNodeId = GetTestGuidById(52),
                                Ordering = 3
                            },

                        }
                    },
                    new RouteNodeDO
                    {
                        Id = GetTestGuidById(59),
                        Ordering = 4,
                        ParentRouteNodeId = GetTestGuidById(1),
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(60),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 1
                            },
                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(61),
                                ParentRouteNodeId = GetTestGuidById(59),
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new RouteNodeDO
                                    {
                                        Id = GetTestGuidById(63),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 1
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = GetTestGuidById(64),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 2
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = GetTestGuidById(65),
                                        ParentRouteNodeId = GetTestGuidById(61),
                                        Ordering = 3
                                    },
                                }
                            },

                            new RouteNodeDO
                            {
                                Id = GetTestGuidById(62),
                                ParentRouteNodeId = GetTestGuidById(59),
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