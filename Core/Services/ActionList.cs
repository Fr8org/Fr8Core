using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;

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

        public ActionListDO GetByKey(IUnitOfWork uow, int curActionListId)
        {
            var curActionListDO = uow.ActionListRepository.GetByKey(curActionListId);
            if (curActionListDO == null)
                throw new ArgumentNullException("actionListId");

            return curActionListDO;
        }

        public void AddAction(ActionDO curActionDO, string position)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                uow.ActionRepository.Attach(curActionDO);
                if (!curActionDO.ParentActivityId.HasValue)
                    throw new NullReferenceException("ActionListId");

                var curActionList = GetByKey(uow, curActionDO.ParentActivityId.Value);
                if (string.IsNullOrEmpty(position) || position.Equals("last", StringComparison.OrdinalIgnoreCase))
                    Reorder(curActionList, curActionDO, position);
                else
                    throw new NotSupportedException("Unsupported value causing problems for Action ordering in ActionList.");

                if (curActionList.CurrentActivity == null)
                {
                    curActionList.CurrentActivity =
                        curActionList.Activities.OrderBy(action => action.Ordering).FirstOrDefault();
                }
                //uow.ActionRepository.Add(curActionDO);
                //uow.ActionListRepository.Add(curActionList);
                uow.SaveChanges();
            }
        }

        private void Reorder(ActionListDO curActionListDO, ActionDO curActionDO, string position)
        {
            int ordering = 0;
            if (curActionListDO.Activities.Count > 0)
            {
                ordering = curActionListDO.Activities.Select(action => action.Ordering).Max();
            }
            curActionDO.Ordering = ordering + 1;
        }

        //if the list is unstarted, set it to inprocess

        //until curActionList is Completed  
        //    if currentActivity is an Action, process it
        //    else it's an ActionList, call recursively
        public void Process(ActionListDO curActionList, ProcessDO curProcessDO, IUnitOfWork uow)
        {

            //We assume that any unstarted ActionList that makes it to here should be put into process
            if (curActionList.ActionListState == ActionListState.Unstarted && curActionList.CurrentActivity != null) //need to add pending state for asynchronous cases
            {
                SetState(curActionList, ActionListState.Inprocess, uow);
            }


            if (curActionList.ActionListState != ActionListState.Inprocess) //need to add pending state for asynchronous cases
            {
                throw new ArgumentException("tried to process an ActionList that was not in state=InProcess");
            }

            if (curActionList.CurrentActivity == null)
            {
                throw new ArgumentException("An ActionList with a null CurrentActivity should not get this far. It should be Completed or Unstarted");
            }

            //main processing loop for the Activities belonging to this ActionList
            while (curActionList.ActionListState == ActionListState.Inprocess)
            {
                try
                {
                    var currentActivity = curActionList.CurrentActivity;

                    //if the current activity is an Action, just process it
                    //if the current activity is iself an ActionList, then recursively call ActionList#Process
                    if (currentActivity is ActionListDO)
                    {
                        Process((ActionListDO)currentActivity, curProcessDO, uow);
                    }
                    else
                    {
                        ProcessAction(curActionList, curProcessDO, uow);
                    }


                }
                catch (Exception ex)
                {
                    SetState(curActionList, ActionListState.Error, uow);
                    throw new Exception(ex.Message);
                }


            }
            SetState(curActionList, ActionListState.Completed, uow); //TODO probably need to test for this



        }

        private void SetState(ActionListDO actionListDO, int actionListState, IUnitOfWork uow)
        {
            actionListDO.ActionListState = actionListState;
            uow.ActionListRepository.Attach(actionListDO);
            uow.SaveChanges();
        }

        public void ProcessAction(ActionListDO curActionList, ProcessDO curProcessDO, IUnitOfWork uow)
        {
            _action.PrepareToExecute((ActionDO)curActionList.CurrentActivity, curProcessDO, uow);
            UpdateActionListState(curActionList);
        }


        public void UpdateActionListState(ActionListDO curActionListDO)
        {

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //update CurrentActivity pointer
                if (curActionListDO.CurrentActivity is ActionDO)
                {
                    if (((ActionDO)curActionListDO.CurrentActivity).ActionState == ActionState.Active ||
                        ((ActionDO)curActionListDO.CurrentActivity).ActionState == ActionState.InProcess)
                    {
                        ActionDO actionDO = curActionListDO.Activities
                            .OfType<ActionDO>()
                            .OrderBy(o => o.Ordering)
                            .Where(o => o.Ordering > curActionListDO.CurrentActivity.Ordering)
                            .DefaultIfEmpty(null)
                            .FirstOrDefault();

                        if (actionDO != null)
                            curActionListDO.CurrentActivity = actionDO;
                        else
                        {
                            //we're done, no more activities to process in this list
                            curActionListDO.CurrentActivity = null;
                            curActionListDO.ActionListState = ActionListState.Completed;
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Action List ID: {0}. Action status returned: {1}",
                            curActionListDO.Id, ((ActionDO)curActionListDO.CurrentActivity).ActionState));
                    }
                    uow.ActionListRepository.Attach(curActionListDO);
                    uow.SaveChanges();
                }
            }

        }
    }
}