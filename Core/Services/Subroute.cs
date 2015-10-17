using System;
using System.Linq;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

namespace Core.Services
{
    public class Subroute : ISubroute
    {
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

        public SubrouteDO Create(IUnitOfWork uow)
        {
            var subroute = ObjectFactory.GetInstance<SubrouteDO>();

            Create(uow, subroute);

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

            curActionDO.Ordering = subroute.RouteNodes.Count > 0 ? subroute.RouteNodes.Max(x => x.Ordering) + 1 : 1;

            subroute.RouteNodes.Add(curActionDO);

            uow.SaveChanges();
        }
    }
}
