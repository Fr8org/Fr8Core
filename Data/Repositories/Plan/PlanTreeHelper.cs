using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;

namespace Data.Repositories.Plan
{
    public static class PlanTreeHelper
    {
        /**********************************************************************************/

        public static PlanNodeDO CloneWithStructure(PlanNodeDO source, Action<PlanNodeDO> nodeCallback = null)
        {
            var clone = source.Clone();

            if (nodeCallback != null)
            {
                nodeCallback(clone);
            }

            foreach (var child in source.ChildNodes)
            {
                var clonedChild = CloneWithStructure(child, nodeCallback);

                clonedChild.ParentPlanNode = clone;
                clone.ChildNodes.Add(clonedChild);
            }

            return clone;
        }

        /**********************************************************************************/

        public static List<PlanNodeDO> Linearize(PlanNodeDO root)
        {
            var nodes = new List<PlanNodeDO>();

            Linearize(root, nodes);

            return nodes;
        }

        /**********************************************************************************/

        public static List<PlanNodeDO> LinearizeOrdered(PlanNodeDO root)
        {
            var nodes = new List<PlanNodeDO>();

            LinearizeOrdered(root, nodes);

            return nodes;
        }

        /**********************************************************************************/

        public static void Visit(PlanNodeDO root, Action<PlanNodeDO> visitor)
        {
            Visit(root, x =>
            {
                visitor(x);
                return true;
            });
        }

        /**********************************************************************************/

        public static void Visit(PlanNodeDO root, Func<PlanNodeDO, bool> vistor)
        {
            Visit(root, null, (x, y) => vistor(x));
        }

        /**********************************************************************************/

        public static void Visit(PlanNodeDO root, Action<PlanNodeDO, PlanNodeDO> vistor)
        {
            Visit(root, null, (x, y) =>
            {
                vistor(x, y);
                return true;
            });
        }

        /**********************************************************************************/

        public static void Visit(PlanNodeDO root, Func<PlanNodeDO, PlanNodeDO, bool> visitor)
        {
            Visit(root, null, visitor);
        }

        /**********************************************************************************/

        public static void VisitOrdered(PlanNodeDO root, Action<PlanNodeDO> visitor)
        {
            VisitOrdered(root, x =>
            {
                visitor(x);
                return true;
            });
        }

        /**********************************************************************************/

        public static void VisitOrdered(PlanNodeDO root, Func<PlanNodeDO, bool> vistor)
        {
            VisitOrdered(root, null, (x, y) => vistor(x));
        }

        /**********************************************************************************/

        public static void VisitOrdered(PlanNodeDO root, Action<PlanNodeDO, PlanNodeDO> vistor)
        {
            VisitOrdered(root, null, (x, y) =>
            {
                vistor(x, y);
                return true;
            });
        }

        /**********************************************************************************/

        public static void VisitOrdered(PlanNodeDO root, Func<PlanNodeDO, PlanNodeDO, bool> visitor)
        {
            VisitOrdered(root, null, visitor);
        }


        /**********************************************************************************/

        private static void Visit(PlanNodeDO root, PlanNodeDO parent, Func<PlanNodeDO, PlanNodeDO, bool> vistor)
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

            foreach (var planNodeDo in root.ChildNodes)
            {
                Visit(planNodeDo, root, vistor);
            }
        }

        /**********************************************************************************/

        private static void VisitOrdered(PlanNodeDO root, PlanNodeDO parent, Func<PlanNodeDO, PlanNodeDO, bool> vistor)
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

            foreach (var planNodeDo in root.ChildNodes.OrderBy(x => x.Ordering))
            {
                VisitOrdered(planNodeDo, root, vistor);
            }
        }

        /**********************************************************************************/

        public static void Linearize(PlanNodeDO root, List<PlanNodeDO> nodes)
        {
            Visit(root, x =>
            {
                nodes.Add(x);
                return true;
            });
        }

        /**********************************************************************************/

        public static void LinearizeOrdered(PlanNodeDO root, List<PlanNodeDO> nodes)
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