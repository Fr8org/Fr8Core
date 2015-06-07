using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Entities.CTE;
using Data.Interfaces;
using StructureMap;

namespace Data.Repositories
{
    public class ProfileNodeRepository : GenericRepository<ProfileNodeDO>, IProfileNodeRepository
    {
        internal ProfileNodeRepository(IUnitOfWork uow)
            : base(uow)
        {

        }
    }

    public static class ProfileNodeQueryExtensions
    {
        public static IEnumerable<ProfileNodeDO> WithAncestors(this IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            return ObjectFactory.GetInstance<IProfileNodeHierarchy>().WithAncestors(query, uow);
        }

        public static IEnumerable<ProfileNodeDO> WithDescendants(this IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            return ObjectFactory.GetInstance<IProfileNodeHierarchy>().WithDescendants(query, uow);
        }

        public static IEnumerable<ProfileNodeDO> WithFullHeirarchy(this IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            return ObjectFactory.GetInstance<IProfileNodeHierarchy>().WithFullHeirarchy(query, uow);
        }
    }

    public interface IProfileNodeHierarchy
    {
        IQueryable<ProfileNodeDO> WithAncestors(IQueryable<ProfileNodeDO> query, IUnitOfWork uow);
        IQueryable<ProfileNodeDO> WithDescendants(IQueryable<ProfileNodeDO> query, IUnitOfWork uow);
        IQueryable<ProfileNodeDO> WithFullHeirarchy(IQueryable<ProfileNodeDO> query, IUnitOfWork uow);
    }

    public class ProfileNodeHierarchy : IProfileNodeHierarchy
    {
        public IQueryable<ProfileNodeDO> WithAncestors(IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            var originalQuery = uow.Db.Set<ProfileNodeDO>().AsQueryable();
            var cteQuery = uow.Db.Set<ProfileNodeAncestorsCTE>().AsQueryable();

            return cteQuery
                    .Where(r => query.Select(profileNodeDO => profileNodeDO.Id).Contains(r.AnchorNodeID))
                    .Join(
                        originalQuery,
                        (cte => cte.ProfileNodeID),
                        (pn => pn.Id),
                        (cte, pn) => pn
                    );
        }

        public IQueryable<ProfileNodeDO> WithDescendants(IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            var originalQuery = uow.Db.Set<ProfileNodeDO>().AsQueryable();
            var cteQuery = uow.Db.Set<ProfileNodeDescendantsCTE>().AsQueryable();

            return cteQuery
                    .Where(r => query.Select(profileNodeDO => profileNodeDO.Id).Contains(r.AnchorNodeID))
                    .Join(
                        originalQuery,
                        (cte => cte.ProfileNodeID),
                        (pn => pn.Id),
                        (cte, pn) => pn
                    );
        }

        public IQueryable<ProfileNodeDO> WithFullHeirarchy(IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            return WithAncestors(query, uow).Union(WithDescendants(query, uow)).Distinct();
        }
    }

    //The following class retrieves heirarchy without CTEs. It's inefficient for big trees, but we're only using it for the mocked DB (as our View is unaccessable).
    //It will, however, work against a normal database, but it's not advised due to performance issues (each level requires a round trip to the DB)
    public class ProfileNodeHierarchyWithoutCTE : IProfileNodeHierarchy
    {
        public IQueryable<ProfileNodeDO> WithAncestors(IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            var allNodes = query.ToList();
            var currentNodes = allNodes;
            while (true)
            {
                var nextParentIDs = currentNodes.Select(n => n.ParentNodeID).ToList();
                var newQuery = uow.ProfileNodeRepository.GetQuery().Where(pn => nextParentIDs.Contains(pn.Id)).ToList();
                if (!newQuery.Any())
                    break;
                
                allNodes.AddRange(newQuery);
                currentNodes = newQuery;
            }
            return allNodes.Distinct().AsQueryable();
        }

        public IQueryable<ProfileNodeDO> WithDescendants(IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            var allNodes = query.ToList();
            var currentNodes = allNodes;
            while (true)
            {
                var nextChildrenIDs = currentNodes.Select(n => n.Id).ToList();
                var newQuery = uow.ProfileNodeRepository.GetQuery().Where(pn => pn.ParentNodeID.HasValue && nextChildrenIDs.Contains(pn.ParentNodeID.Value)).ToList();
                if (!newQuery.Any())
                    break;

                allNodes.AddRange(newQuery);
                currentNodes = newQuery;
            }
            return allNodes.Distinct().AsQueryable();
        }

        public IQueryable<ProfileNodeDO> WithFullHeirarchy(IQueryable<ProfileNodeDO> query, IUnitOfWork uow)
        {
            return WithAncestors(query, uow).Union(WithDescendants(query, uow)).Distinct();
        }
    }

    public interface IProfileNodeRepository : IGenericRepository<ProfileNodeDO>
    {

    }
}