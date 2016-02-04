using System;
using Data.Entities;

namespace Data.Repositories.Plan
{
    internal class LoadedRoute
    {
        public readonly RouteNodeDO Root;
        public bool IsDeleted;
        public bool IsNew;

        public LoadedRoute(RouteNodeDO root, bool isNew = false)
        {
            IsNew = isNew;
            Root = root;
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