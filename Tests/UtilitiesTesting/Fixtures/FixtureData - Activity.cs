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

        public static RouteNodeDO TestActivity57()
        {
            return new RouteNodeDO
            {
                Id = 57,
                Ordering = 2,
                ParentRouteNodeId = 54
            };

        }

        public static RouteNodeDO TestActivityNotExists()
        {
            return new RouteNodeDO
            {
                Id = 57,
                Ordering = 2,
                ParentRouteNodeId = 54
            };

        }

        public static RouteNodeDO TestActivityTree()
        {
            var tree = new RouteNodeDO
            {
                Id = 1,
                Ordering = 1,
                ChildNodes = new List<RouteNodeDO>
                {
                    new RouteNodeDO
                    {
                        Id = 23,
                        Ordering = 1,
                        ParentRouteNodeId = 1
                    },
                    new RouteNodeDO
                    {
                        Id = 43,
                        ParentRouteNodeId = 1,
                        Ordering = 2,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = 44,
                                Ordering = 1,
                                ParentRouteNodeId = 43
                            },
                            new RouteNodeDO
                            {
                                Id = 46,
                                Ordering = 2,
                                ParentRouteNodeId = 43
                            },
                            new RouteNodeDO
                            {
                                Id = 48,
                                Ordering = 3,
                                ParentRouteNodeId = 43
                            },

                        }
                    },
                    new RouteNodeDO
                    {
                        Id = 52,
                        Ordering = 3,
                        ParentRouteNodeId = 1,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = 53,
                                Ordering = 1,
                                ParentRouteNodeId = 52
                            },
                            new RouteNodeDO
                            {
                                Id = 54,
                                ParentRouteNodeId = 52,
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new RouteNodeDO
                                    {
                                        Id = 56,
                                        ParentRouteNodeId = 54,
                                        Ordering = 1
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = 57,
                                        ParentRouteNodeId = 54,
                                        Ordering = 2
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = 58,
                                        ParentRouteNodeId = 54,
                                        Ordering = 3
                                    },

                                }
                            },
                            new RouteNodeDO
                            {
                                Id = 55,
                                ParentRouteNodeId = 52,
                                Ordering = 3
                            },

                        }
                    },
                    new RouteNodeDO
                    {
                        Id = 59,
                        Ordering = 4,
                        ParentRouteNodeId = 1,
                        ChildNodes = new List<RouteNodeDO>
                        {
                            new RouteNodeDO
                            {
                                Id = 60,
                                ParentRouteNodeId = 59,
                                Ordering = 1
                            },
                            new RouteNodeDO
                            {
                                Id = 61,
                                ParentRouteNodeId = 59,
                                Ordering = 2,

                                ChildNodes = new List<RouteNodeDO>
                                {
                                    new RouteNodeDO
                                    {
                                        Id = 63,
                                        ParentRouteNodeId = 61,
                                        Ordering = 1
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = 64,
                                        ParentRouteNodeId = 61,
                                        Ordering = 2
                                    },
                                    new RouteNodeDO
                                    {
                                        Id = 65,
                                        ParentRouteNodeId = 61,
                                        Ordering = 3
                                    },
                                }
                            },

                            new RouteNodeDO
                            {
                                Id = 62,
                                ParentRouteNodeId = 59,
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
            var activitiesIndex = new Dictionary<int, RouteNodeDO>();

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

        private static void TraverseActivityTree(RouteNodeDO root, Dictionary<int, RouteNodeDO> allActivities)
        {
            allActivities.Add(root.Id, root);

            foreach (var activityDo in root.ChildNodes)
            {
                TraverseActivityTree(activityDo, allActivities);
            }
        }

    }
}