using System;
using Data.Entities;

namespace Data.Repositories.Plan
{
    internal class LoadedRoute
    {
        public readonly RouteNodeDO Root;
        public bool IsDeleted;
        public bool IsNew;
        public RouteSnapshot Snapshot;

        public LoadedRoute(RouteNodeDO root, bool isNew = false)
        {
            IsNew = isNew;
            Root = root;
            Snapshot = isNew ? new RouteSnapshot() : new RouteSnapshot(root, true);
        }

        public RouteSnapshot.Changes GetChanges()
        {
            var current = new RouteSnapshot(Root, false);
            return Snapshot.Compare(current);
        }

        public RouteNodeDO Find(Guid nodeId)
        {
            if (IsDeleted)
            {
                return null;
            }

            RouteNodeDO result = null;

            RouteTreeHelper.Visit(Root, x =>
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