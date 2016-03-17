using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;

using Hub.Interfaces;
using Hub.Managers;

namespace Hub.Services
{
    public class Field : IField
    {
        private readonly IPlanNode _planNode;
        private readonly IActivity _activity;
        private readonly ICrateManager _crate;

        public Field()
        {
            _planNode = ObjectFactory.GetInstance<IPlanNode>();
            _activity = ObjectFactory.GetInstance<IActivity>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        public bool Exists(FieldValidationDTO data)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curAction = _activity.GetById(uow, data.CurrentActivityId);

                List<ActivityDO> planNodes;
                switch (data.Direction)
                {
                    case ActivityDirection.Up:
                        planNodes = _planNode.GetUpstreamActivities(uow, curAction).OfType<ActivityDO>().ToList();
                        break;
                    case ActivityDirection.Down:
                        planNodes = _planNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>().ToList();
                    break;
                    case ActivityDirection.Both:
                    planNodes = _planNode.GetUpstreamActivities(uow, curAction).OfType<ActivityDO>().ToList();
                    planNodes.AddRange(_planNode.GetDownstreamActivities(uow, curAction).OfType<ActivityDO>().ToList());
                    break;
                    default:
                    throw new InvalidEnumArgumentException("Unknown ActivityDirection type");
                }


                foreach (var upstreamPlanNode in planNodes)
                {
                    //var crates = _crate.GetCratesByManifestType(CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME, upstreamPlanNode.CrateStorageDTO());

                    foreach (var crate in _crate.GetStorage(upstreamPlanNode.CrateStorage).CratesOfType<FieldDescriptionsCM>())
                    {
                        if (data.CrateLabel != null && data.CrateLabel != crate.Label)
                        {
                            continue;
                        }

                        if (crate.Content.Fields.Any(field => field.Key == data.FieldName))
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
