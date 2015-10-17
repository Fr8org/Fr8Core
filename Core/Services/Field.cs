using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
    public class Field
    {
        private readonly IRouteNode _routeNode;
        private readonly IAction _action;
        private readonly ICrateManager _crate;

        public Field()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _action = ObjectFactory.GetInstance<IAction>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }


        //TODO convert to real direction enum
        public bool Exists(int direction, RouteNodeDO curRouteNode, string label)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<ActionDO> routeNodes;
                if (direction == 1)
                {
                    routeNodes = (List<ActionDO>) _routeNode.GetUpstreamActivities(uow, curRouteNode).OfType<ActionDO>();
                }
                else
                {
                    routeNodes = (List<ActionDO>)_routeNode.GetDownstreamActivities(uow, curRouteNode).OfType<ActionDO>();
                }

                foreach (var upstreamRouteNode in routeNodes)
                {
                    var crates = _action.GetCratesByManifestType(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, upstreamRouteNode.CrateStorageDTO());
                    foreach (var crate in crates)
                    {
                        var designTimeFieldsCM = _crate.GetStandardDesignTimeFields(crate);
                        if (designTimeFieldsCM.Fields.Any(field => field.Key == label))
                        {
                            return true;
                        }
                    }
                }

            }

            return false;
        }
    }
}
