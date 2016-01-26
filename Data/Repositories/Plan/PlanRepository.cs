using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Migrations;

namespace Data.Repositories.Plan
{
    public class PlanRepository
    {
        private readonly IUnitOfWork _uow;

        public PlanRepository(IUnitOfWork uow)
        {
            _uow = uow;
        }


        public TRouteNode GetById<TRouteNode>(Guid id)
            where TRouteNode : RouteNodeDO
        {
            return null;
        }

        public void UpdateProperties<TRouteNode>(TRouteNode node)
            where TRouteNode : RouteNodeDO
        {

        }

        public void UpdateRecusive<TRouteNode>(TRouteNode node)
            where TRouteNode : RouteNodeDO
        {

        }

        public void Delete<TRouteNode>(TRouteNode node)
            where TRouteNode : RouteNodeDO
        {
        }

        private void LoadRoute(Guid routeId)
        {

        }
    }
}
