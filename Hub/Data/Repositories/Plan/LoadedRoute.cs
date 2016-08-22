using System;
using Data.Entities;

namespace Data.Repositories.Plan
{
    internal class LoadedPlan
    {
        public readonly PlanNodeDO Root;
        public bool IsDeleted;
        public bool IsNew;
        public PlanSnapshot Snapshot;

        public LoadedPlan(PlanNodeDO root, bool isNew = false)
        {
            IsNew = isNew;
            Root = root;

            if (isNew)
            {
                Snapshot = new PlanSnapshot();
            }
            else
            {
                RebuildSnapshot();
            }
        }

        public PlanSnapshot RebuildSnapshot()
        {
            var prev = Snapshot;
            
            Snapshot = new PlanSnapshot(Root, true);

            return prev;
        }

        public PlanNodeDO Find(Guid nodeId)
        {
            if (IsDeleted)
            {
                return null;
            }

            PlanNodeDO result = null;

            PlanTreeHelper.Visit(Root, x =>
            {
                if (x.Id == nodeId)
                {
                    result = x;
                    return false;
                }

                return true;
            });
            
            return result;
        }
    }
}