using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Enums;
using Core.Interfaces;
using Core.Managers;
using Data.Constants;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using StructureMap;

namespace Core.Services
{
    public class Field : IField
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

        public bool Exists(FieldCheckDTO data)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = _action.GetById(uow, data.CurrentActionId);

                List<ActionDO> routeNodes;
                switch (data.Direction)
                {
                    case ActivityDirection.Up:
                        routeNodes = (List<ActionDO>)_routeNode.GetUpstreamActivities(uow, curAction).OfType<ActionDO>();
                        break;
                    case ActivityDirection.Down:
                        routeNodes = (List<ActionDO>)_routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>();
                    break;
                    case ActivityDirection.Both:
                        routeNodes = (List<ActionDO>)_routeNode.GetUpstreamActivities(uow, curAction).OfType<ActionDO>();
                        routeNodes.AddRange(_routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>());
                    break;
                    default:
                    throw new InvalidEnumArgumentException("Unknown ActivityDirection type");
                }


                foreach (var upstreamRouteNode in routeNodes)
                {
                    var crates = _crate.GetCratesByManifestType(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, upstreamRouteNode.CrateStorageDTO());
                    foreach (var crate in crates)
                    {
                        if (data.CrateLabel != null && data.CrateLabel != crate.Label)
                        {
                            continue;
                        }

                        var designTimeFieldsCM = _crate.GetStandardDesignTimeFields(crate);
                        if (designTimeFieldsCM.Fields.Any(field => field.Key == data.FieldName))
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
