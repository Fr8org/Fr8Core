using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Crates;
using Data.Interfaces.Manifests;
using Hub.Exceptions;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Hub.Interfaces;
using Data.Infrastructure;
using Data.Interfaces.DataTransferObjects.Helpers;
using Hub.Managers;

namespace Hub.Services
{
    public class Container : Hub.Interfaces.IContainer
    {

        // Declarations

        private readonly IProcessNode _processNode;
        private readonly IRouteNode _activity;
        private readonly ICrateManager _crate;

        public Container()
        {
            _processNode = ObjectFactory.GetInstance<IProcessNode>();
            _activity = ObjectFactory.GetInstance<IRouteNode>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
        }

        private void AddOperationalStateCrate(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            using (var crateStorage = _crate.UpdateStorage(() => curContainerDO.CrateStorage))
            {
                var operationalStatus = new OperationalStateCM();
                var operationsCrate = Crate.FromContent("Operational Status", operationalStatus);
                crateStorage.Add(operationsCrate);
            }

            uow.SaveChanges();
        }

        private ActivityResponseDTO GetCurrentActivityResponse(ContainerDO curContainerDO)
        {
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().Single();
            return operationalState.CurrentActivityResponse;
        }

        public List<ContainerDO> LoadContainers(IUnitOfWork uow, PlanDO plan)
        {
            return uow.ContainerRepository.GetQuery().Where(x => x.PlanId == plan.Id).ToList();
        }

        private async Task ProcessCurrentActivityResponse(IUnitOfWork uow, ContainerDO curContainerDo, ActivityResponseDTO response)
        {
            //extract the type value from the activity response
            ActivityResponse activityResponse = ActivityResponse.Null;
            if (response != null) Enum.TryParse(response.Type, out activityResponse);

            switch (activityResponse)
            {
                case ActivityResponse.ExecuteClientActivity:
                case ActivityResponse.Success:
                case ActivityResponse.ReProcessChildren:
                case ActivityResponse.Null://let's assume this is success for now
                case ActivityResponse.JumpToActivity:
                case ActivityResponse.JumpToSubplan:

                    break;
                case ActivityResponse.RequestSuspend:
                    curContainerDo.ContainerState = ContainerState.Pending;
                    break;

                case ActivityResponse.Error:
                    //TODO retry activity execution until 3 errors??
                    //so we are able to show the specific error that is embedded inside the container we are sending back that container to client
                    ErrorDTO error = response.TryParseErrorDTO(out error) ? error : null;
                    throw new ErrorResponseException(Mapper.Map<ContainerDO, ContainerDTO>(curContainerDo), error?.Message);
                case ActivityResponse.RequestTerminate:
                    //FR-2163 - If action response requests for termination, we make the container as Completed to avoid unwanted errors.
                    curContainerDo.ContainerState = ContainerState.Completed;
                    EventManager.ProcessingTerminatedPerActivityResponse(curContainerDo, ActivityResponse.RequestTerminate);

                    break;

                

                default:
                    throw new Exception("Unknown activity state on activity with id " + curContainerDo.CurrentRouteNodeId);
            }
        }


        /* 
        *          a
        *       b     c 
        *     d   E  f  g  
        * 
        * 
        * We traverse this tree in this order a-b-d-E-b-c-f-g-c-a-NULL 
        */
        /// <summary>
        /// Moves to next Route and returns action state of this new plan
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <param name="skipChildren"></param>
        private ActivityState MoveToNextRoute(IUnitOfWork uow, ContainerDO curContainerDO, bool skipChildren)
        {
            var state = ActivityState.InitialRun;
            var currentNode = uow.PlanRepository.GetById<RouteNodeDO>(curContainerDO.CurrentRouteNodeId);
            
            // we need this to make tests wokring. If we leave currentroutenode not null, MockDB will restore CurrentRouteNodeId. 
            // EF should just igone navigational porperty null value if corresponding foreign key is not null.
            curContainerDO.CurrentRouteNode = null;

            if (skipChildren || currentNode.ChildNodes.Count == 0)
            {
                var nextSibling = _activity.GetNextSibling(currentNode);
                if (nextSibling == null)
                {
                    curContainerDO.CurrentRouteNodeId = currentNode.ParentRouteNode != null ? currentNode.ParentRouteNode.Id : (Guid?)null;
                    state = ActivityState.ReturnFromChildren;
                }
                else
                {
                    curContainerDO.CurrentRouteNodeId = nextSibling.Id;
                }
            }
            else
            {
                var firstChild = _activity.GetFirstChild(currentNode);
                curContainerDO.CurrentRouteNodeId = firstChild.Id;
            }

            uow.SaveChanges();
            return state;
        }

        /// <summary>
        /// Run current action and return it's response
        /// </summary>
        /// <param name="uow"></param>
        /// <param name="curContainerDO"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private async Task<ActivityResponseDTO> ProcessActivity(IUnitOfWork uow, ContainerDO curContainerDO, ActivityState state)
        {
            await _activity.Process(curContainerDO.CurrentRouteNodeId.Value, state, curContainerDO);
            return GetCurrentActivityResponse(curContainerDO);
        }

        private bool ShouldSkipChildren(ContainerDO curContainerDO, ActivityState state, ActivityResponse response)
        {
            //first let's check if there is a child action related response
            if (response == ActivityResponse.SkipChildren)
            {
                return true;
            }
            else if (response == ActivityResponse.ReProcessChildren)
            {
                return false;
            }

            //otherwise we will assume this is a regular action
            //so we will process it's children once

            if (state == ActivityState.InitialRun)
            {
                return false;
            }
            else if (state == ActivityState.ReturnFromChildren)
            {
                return true;
            }

            throw new Exception("This shouldn't happen");
        }

        private bool HasOperationalStateCrate(ContainerDO curContainerDO)
        {
            
            var storage = _crate.GetStorage(curContainerDO.CrateStorage);
            var operationalState = storage.CrateContentsOfType<OperationalStateCM>().FirstOrDefault();
            return operationalState != null;
        }

        private Guid? GetFirstActivityOfSubplan(ContainerDO curContainerDO, Guid subplanId)
        {
            var subplan = curContainerDO.Plan.Subroutes.FirstOrDefault(s => s.Id == subplanId);
            return subplan?.ChildNodes.OrderBy(c => c.Ordering).FirstOrDefault()?.Id;
        }

        public async Task Run(IUnitOfWork uow, ContainerDO curContainerDO)
        {
            if (curContainerDO == null)
                throw new ArgumentNullException("ContainerDO is null");

            //if payload already has operational state create we shouldn't create another
            if (!HasOperationalStateCrate(curContainerDO))
            {
                AddOperationalStateCrate(uow, curContainerDO);
            }

            curContainerDO.ContainerState = ContainerState.Executing;
            uow.SaveChanges();

            if (curContainerDO.CurrentRouteNodeId == null)
            {
                throw new ArgumentNullException("CurrentActivity is null. Cannot execute CurrentActivity");
            }

            var actionState = ActivityState.InitialRun;
            while (curContainerDO.CurrentRouteNodeId != null)
            {
                var activityResponseDTO = await ProcessActivity(uow, curContainerDO, actionState);
                
                //extract ActivityResponse type from result
                ActivityResponse activityResponse = ActivityResponse.Null;
                if (activityResponseDTO != null)
                    Enum.TryParse(activityResponseDTO.Type, out activityResponse);

                ResponseMessageDTO responseMessage;

                switch (activityResponse)
                {
                    case ActivityResponse.ExecuteClientActivity:
                    case ActivityResponse.Success:
                    case ActivityResponse.ReProcessChildren:
                    case ActivityResponse.Null://let's assume this is success for now

                        var shouldSkipChildren = ShouldSkipChildren(curContainerDO, actionState, activityResponse);
                        actionState = MoveToNextRoute(uow, curContainerDO, shouldSkipChildren);

                        break;
                    case ActivityResponse.RequestSuspend:
                        curContainerDO.ContainerState = ContainerState.Pending;
                        return;

                    case ActivityResponse.Error:
                        //TODO retry activity execution until 3 errors??
                        //so we are able to show the specific error that is embedded inside the container we are sending back that container to client
                        ErrorDTO error = activityResponseDTO.TryParseErrorDTO(out error) ? error : null;
                        throw new ErrorResponseException(Mapper.Map<ContainerDO, ContainerDTO>(curContainerDO), error?.Message);
                    case ActivityResponse.RequestTerminate:
                        //FR-2163 - If action response requests for termination, we make the container as Completed to avoid unwanted errors.
                        curContainerDO.ContainerState = ContainerState.Completed;
                        EventManager.ProcessingTerminatedPerActivityResponse(curContainerDO, ActivityResponse.RequestTerminate);

                        return;

                    case ActivityResponse.JumpToActivity:
                        actionState = ActivityState.InitialRun;
                        activityResponseDTO.TryParseResponseMessageDTO(out responseMessage);
                        curContainerDO.CurrentRouteNodeId = (Guid) responseMessage.Details;
                        break;

                    case ActivityResponse.JumpToSubplan:
                        actionState = ActivityState.InitialRun;
                        activityResponseDTO.TryParseResponseMessageDTO(out responseMessage);
                        var subplanId = (Guid)responseMessage.Details;
                        curContainerDO.CurrentRouteNodeId = GetFirstActivityOfSubplan(curContainerDO ,subplanId);
                        break;

                    default:
                        throw new Exception("Unknown activity state on activity with id " + curContainerDO.CurrentRouteNodeId);
                }
                
                
            }

            if(curContainerDO.ContainerState == ContainerState.Executing)
            {
                curContainerDO.ContainerState = ContainerState.Completed;
                uow.SaveChanges();
            }
        }

        // Return the Containers of current Account
        public IList<ContainerDO> GetByFr8Account(IUnitOfWork unitOfWork, Fr8AccountDO account, bool isAdmin = false, Guid? id = null)
        {
            if (account.Id == null)
                throw new ApplicationException("UserId must not be null");

            var containerRepository = unitOfWork.ContainerRepository.GetQuery();
            
      
            return (id == null
               ? containerRepository.Where(container => container.Plan.Fr8Account.Id == account.Id)
               : containerRepository.Where(container => container.Id == id && container.Plan.Fr8Account.Id == account.Id)).ToList();

        }
    }
}