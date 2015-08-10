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

        public ActionListDO GetByKey(int curActionListId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curActionListDO = uow.ActionListRepository.GetByKey(curActionListId);
                if (curActionListDO == null)
                    throw new ArgumentNullException("actionListId");

                return curActionListDO;
            }
        }

        public void AddAction(ActionDO curActionDO, string position)
        {
            if (!curActionDO.ActionListId.HasValue)
                throw new NullReferenceException("ActionListId");

            var curActionList = GetByKey(curActionDO.ActionListId.Value);
            if (string.IsNullOrEmpty(position) || position.Equals("last", StringComparison.OrdinalIgnoreCase))
                Reorder(curActionList, curActionDO, position);
            else
                throw new NotSupportedException("Unsupported value causing problems for Action ordering in ActionList.");
            curActionList.Actions.Add(curActionDO);
            if (curActionList.CurrentAction == null)
                curActionList.CurrentAction = curActionList.Actions.OrderBy(action => action.Ordering).FirstOrDefault();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionListRepository.Add(curActionList);
                uow.SaveChanges();
            }
        }

        private void Reorder(ActionListDO curActionListDO, ActionDO curActionDO, string position)
        {
            int ordering = curActionListDO.Actions.Select(action => action.Ordering).Max();
            curActionDO.Ordering = ordering + 1;
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
