using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Newtonsoft.Json;
using StructureMap;

namespace Core.Services
{
    public class Subroute : ISubroute
    {

        private readonly ICrateManager _crate;
        private readonly IRouteNode _routeNode;

        public Subroute()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        /// <summary>
        /// Create Subroute entity with required children criteria entity.
        /// </summary>
        public void Store(IUnitOfWork uow, SubrouteDO subroute )
        {
            if (subroute == null)
            {
                subroute = ObjectFactory.GetInstance<SubrouteDO>();
            }

            uow.SubrouteRepository.Add(subroute);
            
            // Saving criteria entity in repository.
            var criteria = new CriteriaDO()
            {
                Subroute = subroute,
                CriteriaExecutionType = CriteriaExecutionType.WithoutConditions
            };
            uow.CriteriaRepository.Add(criteria);
            
            //we don't want to save changes here, to enable upstream transactions
        }

        // <summary>
        /// Creates noew Subroute entity and add it to RouteDO. If RouteDO has no child subroute created route becomes starting subroute.
        /// </summary>
        public SubrouteDO Create(IUnitOfWork uow, RouteDO route, string name)
        {
            var subroute = new SubrouteDO();

            uow.SubrouteRepository.Add(subroute);

            if (route != null)
            {
                if (!route.Subroutes.Any())
                {
                    route.StartingSubroute = subroute;
                    subroute.StartingSubroute = true;
                }
            }

            subroute.Name = name;



            return subroute;
        }

        /// <summary>
        /// Update Subroute entity.
        /// </summary>
        public void Update(IUnitOfWork uow, SubrouteDO subroute)
        {
            if (subroute == null)
            {
                throw new Exception("Updating logic was passed a null SubrouteDO");
            }

            var curSubroute = uow.SubrouteRepository.GetByKey(subroute.Id);
            if (curSubroute == null)
            {
                throw new Exception(string.Format("Unable to find criteria by id = {0}", subroute.Id));
            }

            curSubroute.Name = subroute.Name;
            curSubroute.NodeTransitions = subroute.NodeTransitions;
            uow.SaveChanges();
        }

        /// <summary>
        /// Remove Subroute and children entities by id.
        /// </summary>
        public void Delete(IUnitOfWork uow, int id)
        {
            var subroute = uow.SubrouteRepository.GetByKey(id);

            if (subroute == null)
            {
                throw new Exception(string.Format("Unable to find Subroute by id = {0}", id));
            }

            // Remove all actions.

            

          //  subroute.Activities.ForEach(x => uow.ActivityRepository.Remove(x));

//            uow.SaveChanges();
//            
//            // Remove Criteria.
//            uow.CriteriaRepository
//                .GetQuery()
//                .Where(x => x.SubrouteId == id)
//                .ToList()
//                .ForEach(x => uow.CriteriaRepository.Remove(x));
//
//            uow.SaveChanges();

            // Remove Subroute.
            //uow.SubrouteRepository.Remove(subroute);


            ObjectFactory.GetInstance<IRouteNode>().Delete(uow, subroute);

            uow.SaveChanges();
        }

        public void AddAction(IUnitOfWork uow, ActionDO curActionDO)
        {
            var subroute = uow.SubrouteRepository.GetByKey(curActionDO.ParentRouteNodeId);

            if (subroute == null)
            {
                throw new Exception(string.Format("Unable to find Subroute by id = {0}", curActionDO.ParentRouteNodeId));
            }

            curActionDO.Ordering = subroute.ChildNodes.Count > 0 ? subroute.ChildNodes.Max(x => x.Ordering) + 1 : 1;

            subroute.ChildNodes.Add(curActionDO);

            uow.SaveChanges();
        }

        public void DeleteAction(int id)
        {
            //we are using Kludge solution for now
            //https://maginot.atlassian.net/wiki/display/SH/Action+Deletion+and+Reordering

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {

                var curAction = uow.RouteNodeRepository.GetQuery().FirstOrDefault(al => al.Id == id);
                if (curAction == null)
                {
                    throw new InvalidOperationException("Unknown RouteNode with id: " + id);
                }

                var downStreamActivities = _routeNode.GetDownstreamActivities(uow, curAction).OfType<ActionDO>();
                //we should clear values of configuration controls

                foreach (var downStreamActivity in downStreamActivities)
                {
                    var crateStorage = downStreamActivity.CrateStorageDTO();
                    var cratesToReset = _crate.GetCratesByManifestType(CrateManifests.STANDARD_CONF_CONTROLS_NANIFEST_NAME, crateStorage).ToList();
                    foreach (var crateDTO in cratesToReset)
                    {
                        var configurationControls = _crate.GetStandardConfigurationControls(crateDTO);
                        foreach (var controlDefinitionDTO in configurationControls.Controls)
                        {
                            (controlDefinitionDTO as IResettable).Reset();
                        }
                        crateDTO.Contents = JsonConvert.SerializeObject(configurationControls);
                    }

                    if (cratesToReset.Any())
                    {
                        downStreamActivity.CrateStorage = JsonConvert.SerializeObject(crateStorage);
                    }
                }
                uow.RouteNodeRepository.Remove(curAction);
                uow.SaveChanges();
            }

        }
    }
}
