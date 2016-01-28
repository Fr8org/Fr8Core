using System;
using System.Collections.Generic;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public static class RouteTreeHelper
    {
        /**********************************************************************************/
        
        public static RouteNodeDO CloneWithStructure(RouteNodeDO source)
        {
            var clone = source.Clone();

            foreach (var child in source.ChildNodes)
            {
                var clonedChild = CloneWithStructure(child);

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

        private static void Visit(RouteNodeDO root, RouteNodeDO parent, Func<RouteNodeDO, RouteNodeDO, bool> vistor)
        {
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

        public static void Linearize(RouteNodeDO root, List<RouteNodeDO> nodes)
        {
            Visit(root, x =>
            {
                nodes.Add(x);
                return true;
            });
        }

        /**********************************************************************************/
    }
}