using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using TerminalBase.BaseClasses;

namespace TerminalBase.Infrastructure.Behaviors
{
    public class ReconfigurationItem
    {
        public Func<ReconfigurationContext, Task<bool>> HasActivityMethod { get; set; }
        public Func<ReconfigurationContext, Task<ActivityDO>> CreateActivityMethod { get; set; }
        public Func<ReconfigurationContext, Task<ActivityDO>> ConfigureActivityMethod { get; set; }
        public int ChildActivityIndex { get; set; }
    }

    public class ReconfigurationContext
    {
        public ReconfigurationContext()
        {
            AdditionalItems = new List<ReconfigurationItem>();
        }

        public ActivityDO SolutionActivity { get; set; }
        public AuthorizationTokenDO AuthToken { get; set; }
        public IReadOnlyList<ReconfigurationItem> Items { get; set; }
        public List<ReconfigurationItem> AdditionalItems { get; set; }
    }


    public class ReconfigurationListBehavior
    {
        private BaseTerminalActivity _activity;

        public ReconfigurationListBehavior(BaseTerminalActivity activity)
        {
            _activity = activity;
        }

        public async Task ReconfigureActivities(ActivityDO solution,
            AuthorizationTokenDO authToken, IReadOnlyList<ReconfigurationItem> items)
        {
            var queue = new Queue<ReconfigurationItem>(items);

            if (solution.ChildNodes == null)
            {
                solution.ChildNodes = new List<RouteNodeDO>();
            }

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();

                var context = new ReconfigurationContext()
                {
                    SolutionActivity = solution,
                    AuthToken = authToken,
                    Items = items
                };

                if (!await item.HasActivityMethod(context))
                {
                    var childActivityByIndex = solution.ChildNodes
                        .SingleOrDefault(x => x.Ordering == item.ChildActivityIndex);

                    if (childActivityByIndex != null)
                    {
                        await _activity.HubCommunicator.DeleteActivity(
                            childActivityByIndex.Id,
                            _activity.CurrentFr8UserId
                        );

                        solution.ChildNodes.Remove(childActivityByIndex);
                    }

                    await item.CreateActivityMethod(context);
                }
                else
                {
                    await item.ConfigureActivityMethod(context);
                }

                if (context.AdditionalItems.Count > 0)
                {
                    foreach (var additionalItem in context.AdditionalItems)
                    {
                        if (!queue.Any(x => x.ChildActivityIndex == additionalItem.ChildActivityIndex))
                        {
                            queue.Enqueue(additionalItem);
                        }
                    }
                }
            }
        }
    }
}
