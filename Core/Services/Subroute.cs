using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Managers;
using Data.Constants;
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
        private readonly IAction _action;

        public Subroute()
        {
            _routeNode = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _action = ObjectFactory.GetInstance<IAction>();
        }

        /// <summary>
        /// Create Subroute entity with required children criteria entity.
        /// </summary>
        public void Create(IUnitOfWork uow, SubrouteDO subroute )
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

            curActionDO.Ordering = subroute.RouteNodes.Count > 0 ? subroute.RouteNodes.Max(x => x.Ordering) + 1 : 1;

            subroute.RouteNodes.Add(curActionDO);

            uow.SaveChanges();
        }

        protected CrateDTO GetValidationErrors(CrateStorageDTO crateStorage)
        {
            return crateStorage.CrateDTO.FirstOrDefault(crateDTO => 
                crateDTO.Label == "Validation Errors" && crateDTO.ManifestType == CrateManifests.DESIGNTIME_FIELDS_MANIFEST_NAME);
        }

        protected async Task<bool> ValidateDownstreamActionsAndDelete(int id)
        {
            var validationErrors = new List<CrateDTO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //we should backup this action to see it's effect to downstream actions on deletion
                //with asNoTracking we can keep a copy of curAction on memory
                var curActionBackup = await uow.RouteNodeRepository.GetQuery().AsNoTracking().SingleAsync(a => a.Id == id);
                //TODO refactor this code for a better way
                

                //to prevent one more call to db; attach object by it's id and delete it
                var curAction = new ActionDO { Id = id };
                uow.RouteNodeRepository.Attach(curAction);
                uow.RouteNodeRepository.Remove(curAction);
                uow.SaveChanges();

                var downstreamActions = _routeNode.GetDownstreamActivities(uow, curActionBackup).OfType<ActionDO>();
                //lets start multithreaded calls
                var configureTaskList = new List<Task<ActionDTO>>();
                foreach (var downstreamAction in downstreamActions)
                {
                    configureTaskList.Add(_action.Configure(downstreamAction));
                }

                await Task.WhenAll(configureTaskList);

                //collect plugin responses
                //all tasks are completed by now
                var pluginResponseList = configureTaskList.Select(t => t.Result);

                foreach (var pluginResponse in pluginResponseList)
                {
                    var pluginError = GetValidationErrors(pluginResponse.CrateStorage);
                    if (pluginError != null)
                    {
                        validationErrors.Add(pluginError);
                    }
                }

                //if there are validation errors restore curActionBackup
                if (validationErrors.Count > 0)
                {
                    //restore it
                    uow.RouteNodeRepository.Add(curActionBackup);
                    uow.SaveChanges();
                }
                else
                {
                    //TODO update ordering of downstream actions
                }

            }
            return validationErrors.Count < 1;
        }

        public async Task<string> DeleteAction(int id, bool confirmed)
        {
            if (confirmed)
            {
                //we can assume that there has been some validation errors on previous call
                //but user still wants to delete this action
                //lets use kludge solution
                DeleteActionKludge(id);
            }
            else
            {
                bool isDeleted = await ValidateDownstreamActionsAndDelete(id);
                if (!isDeleted)
                {
                    //we need user confirmation
                    return "we need confirmation";
                }
            }
            return "we have deleted it";
        }

        protected void DeleteActionKludge(int id)
        {
            //Kludge solution
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
