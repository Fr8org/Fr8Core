using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;
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

        public bool Exists(GetCrateDirection direction, ActionDO curAction, string fieldName)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                List<ActionDO> routeNodes;
                switch (direction)
                {
                    case GetCrateDirection.Upstream:
                        routeNodes = (List<ActionDO>)_routeNode.GetUpstreamActivities(uow, curAction).OfType<ActionDO>();
                        break;
                    case GetCrateDirection.Downstream:
                        routeNodes = (List<ActionDO>)_routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>();
                    break;
                    case GetCrateDirection.None:
                        routeNodes = (List<ActionDO>)_routeNode.GetUpstreamActivities(uow, curAction).OfType<ActionDO>();
                        routeNodes.AddRange(_routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>());
                    break;
                    default:
                        throw new InvalidEnumArgumentException("Unknown GetCrateDirection type");
                }


                foreach (var upstreamRouteNode in routeNodes)
                {
                    var crates = _action.GetCratesByManifestType(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, upstreamRouteNode.CrateStorageDTO());
                    foreach (var crate in crates)
                    {
                        var designTimeFieldsCM = _crate.GetStandardDesignTimeFields(crate);
                        if (designTimeFieldsCM.Fields.Any(field => field.Key == fieldName))
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
