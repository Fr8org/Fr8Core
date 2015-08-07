using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public class ActionList : IActionList
    {
        public IEnumerable<ActionListDO> GetAll()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.ActionListRepository.GetAll();
            }
        }

        public ActionListDO Get(int id)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var action = uow.ActionListRepository.GetByKey(id);
                if (action == null)
                    throw new ArgumentNullException("actionId");

                return action;
            }
        }

        public bool AddAction(ActionDO curActionDO, string position)
        {
            if (!curActionDO.ActionListId.HasValue)
                throw new NullReferenceException("ActionListId");

            var actionList = Get(curActionDO.ActionListId.Value);
            int ordering = 0;
            if (!string.IsNullOrEmpty(position) && position.Equals("last", StringComparison.OrdinalIgnoreCase))
            {
                ordering = actionList.ActionOrdering.Select(action => action.Ordering).Max();
                curActionDO.Ordering = ordering + 1;
            }
            else
                curActionDO.Ordering = 0; // Temporarily setting default value. Nee to discuss what should it be exactly.

            actionList.ActionOrdering.Add(curActionDO);

            if (actionList.CurrentAction == null)
                actionList.CurrentAction = actionList.ActionOrdering.OrderBy(action => action.Ordering).FirstOrDefault();
            
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionListRepository.Add(actionList);
                uow.SaveChanges();
            }
            return true;
        }

        public void Process(ActionListDO curActionListDO)
        {
            if (curActionListDO.CurrentAction == null)
                throw new ArgumentNullException("ActionList is missing a CurrentAction");
            else
            {
                //here we will call Action#Process(curActionListDO.CurrentAction curAction);
            }
        }
    }
}
