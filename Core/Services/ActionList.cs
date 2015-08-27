using AutoMapper;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
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
        private readonly IAction _action;
        public ActionList()
        {
            _action = ObjectFactory.GetInstance<IAction>();
        }

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
            {
                throw new ArgumentNullException("ActionList is missing a CurrentAction");
            }
            else
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    //if status is unstarted, change it to in-process. If status is completed or error, throw an exception.
                    try
                    {
                        if (curActionListDO.ActionListState == ActionListState.Unstarted)
                        {
                            curActionListDO.ActionListState = ActionListState.Inprocess;
                            uow.ActionListRepository.Attach(curActionListDO);
                            uow.SaveChanges();

                            var actionOrdering = curActionListDO.Actions.OrderBy(o => o.Ordering).Select(s => s.Ordering);
                            foreach (var order in actionOrdering)
                            {
                                //if return string is "completed", it sets the CurrentAction to the next Action in the list
                                //if not complete set actionlistdo to error
                                _action.Process(curActionListDO.CurrentAction);
                                if (curActionListDO.CurrentAction.ActionState == ActionState.Completed || curActionListDO.CurrentAction.ActionState == ActionState.InProcess)
                                {
                                    ActionDO actionDO = curActionListDO.Actions.OrderBy(o => o.Ordering)
                                        .Where(o => o.Ordering > curActionListDO.CurrentAction.Ordering).DefaultIfEmpty(null).FirstOrDefault();

                                    if (actionDO != null)
                                        curActionListDO.CurrentAction = actionDO;
                                }
                                else
                                {
                                    throw new Exception(string.Format("Action List ID: {0}. Action status returned: {1}", curActionListDO.Id, curActionListDO.CurrentAction.ActionState));
                                }
                            }

                            curActionListDO.ActionListState = ActionListState.Completed;
                            uow.ActionListRepository.Attach(curActionListDO);
                            uow.SaveChanges();
                        }
                        else
                        {
                            throw new Exception(string.Format("Action List ID: {0} status is not unstarted.", curActionListDO.Id));
                        }
                    }
                    catch (Exception ex)
                    {
                        curActionListDO.ActionListState = ActionListState.Error;
                        uow.ActionListRepository.Attach(curActionListDO);
                        uow.SaveChanges();

                        throw new Exception(ex.Message);
                    }
                }
            }
        }
    }
}
