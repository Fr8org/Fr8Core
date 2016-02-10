using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public static class RouteTreeHelper
    {
        /**********************************************************************************/
        
        public static RouteNodeDO CloneWithStructure(RouteNodeDO source, Action<RouteNodeDO> nodeCallback = null)
        {
            var clone = source.Clone();

            if (nodeCallback != null)
            {
                nodeCallback(clone);
            }

            foreach (var child in source.ChildNodes)
            {
                var clonedChild = CloneWithStructure(child, nodeCallback);

                clonedChild.ParentRouteNode = clone;
                clone.ChildNodes.Add(clonedChild);
            }

            return clone;
        }

        /**********************************************************************************/

        public static List<RouteNodeDO> Linearize(RouteNodeDO root)
        {
            var nodes = new List<RouteNodeDO>();

            Linearize(root, nodes);

            return nodes;
        }

        /**********************************************************************************/

        public static List<RouteNodeDO> LinearizeOrdered(RouteNodeDO root)
        {
            var nodes = new List<RouteNodeDO>();

            LinearizeOrdered(root, nodes);

            return nodes;
        }

        /**********************************************************************************/

        public static void Visit(RouteNodeDO root, Action<RouteNodeDO> visitor)
        {
            Visit(root, x =>
            {
                visitor(x);
                return true;
            });
        }

        /**********************************************************************************/

        public static void Visit(RouteNodeDO root, Func<RouteNodeDO, bool>  vistor)
        {
            Visit(root, null, (x, y) => vistor(x));
        }

        /**********************************************************************************/

        public static void Visit(RouteNodeDO root, Action<RouteNodeDO, RouteNodeDO> vistor)
        {
            Visit(root, null, (x, y) => 
            {
                vistor(x, y);
                return true;
            });
        }

        /**********************************************************************************/

        public static void Visit(RouteNodeDO root, Func<RouteNodeDO, RouteNodeDO, bool> visitor)
        {
            Visit(root, null, visitor);
        }

        /**********************************************************************************/

        public static void VisitOrdered(RouteNodeDO root, Action<RouteNodeDO> visitor)
        {
            VisitOrdered(root, x =>
            {
                visitor(x);
                return true;
            });
        }

        /**********************************************************************************/

        public static void VisitOrdered(RouteNodeDO root, Func<RouteNodeDO, bool> vistor)
        {
            VisitOrdered(root, null, (x, y) => vistor(x));
        }

        /**********************************************************************************/

        public static void VisitOrdered(RouteNodeDO root, Action<RouteNodeDO, RouteNodeDO> vistor)
        {
            VisitOrdered(root, null, (x, y) =>
            {
                vistor(x, y);
                return true;
            });
        }

        /**********************************************************************************/

        public static void VisitOrdered(RouteNodeDO root, Func<RouteNodeDO, RouteNodeDO, bool> visitor)
        {
            VisitOrdered(root, null, visitor);
        }


        /**********************************************************************************/

        private static void Visit(RouteNodeDO root, RouteNodeDO parent, Func<RouteNodeDO, RouteNodeDO, bool> vistor)
        {
            if (root == null)
            {
                return;
            }

            if (!vistor(root, parent))
            {
                return;
            }

            if (root.ChildNodes == null)
            {
                return;
            }

            foreach (var routeNodeDo in root.ChildNodes)
            {
                Visit(routeNodeDo, root, vistor);
            }
        }

        /**********************************************************************************/

        private static void VisitOrdered(RouteNodeDO root, RouteNodeDO parent, Func<RouteNodeDO, RouteNodeDO, bool> vistor)
        {
            if (root == null)
            {
                return;
            }

            if (!vistor(root, parent))
            {
                return;
            }

            if (root.ChildNodes == null)
            {
                return;
            }

            foreach (var routeNodeDo in root.ChildNodes.OrderBy(x=>x.Ordering))
            {
                Visit(routeNodeDo, root, vistor);
            }
        }

        /**********************************************************************************/

        public static void Linearize(RouteNodeDO root, List<RouteNodeDO> nodes)
        {
            Visit(root, x =>
            {
                nodes.Add(x);
                return true;
            });
        }

        /**********************************************************************************/

        public static void LinearizeOrdered(RouteNodeDO root, List<RouteNodeDO> nodes)
        {
            VisitOrdered(root, x =>
            {
                nodes.Add(x);
                return true;
            });
        }

        /**********************************************************************************/
    }
}