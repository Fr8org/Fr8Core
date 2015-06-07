using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace KwasantWeb.NotificationQueues
{
    public static class PersonalNotificationQueues
    {
        //Define new queues with a string constant
        //Remember to update GetQueueByName
        public const String StrBookingRequestUpdatedQueue = @"BookingRequestUpdatedQueue";


        public static IPersonalNotificationQueue GetQueueByName(String name)
        {
            switch (name)
            {
                case StrBookingRequestUpdatedQueue:
                    return new BookingRequestUpdatedQueue();
            }
            return null;
        }
    }

    public static class SharedNotificationQueues
    {
        //Define new queues with a string constant, and a static instance of your queue.

        public const String StrBookingRequestReservedForUserQueue = @"BookingRequestReservedForUserQueue";
        public const String StrPollBookingRequestResponseQueue = @"PollBookingRequestResponseQueue";
        public const String StrHighPriorityIncidentsQueue = @"HighPriorityIncidentsQueue";
        public const String StrUserNotificationQueue = @"UserNotificationQueue";

        private static readonly Dictionary<string, IStaticQueue> Queues = new Dictionary<string, IStaticQueue>
            {
                { StrBookingRequestReservedForUserQueue, new BookingRequestReservedForUserQueue() },
                { StrPollBookingRequestResponseQueue, new NewBookingRequestResponseQueue() },
                { StrHighPriorityIncidentsQueue, new HighPriorityIncidentsQueue() },
                { StrUserNotificationQueue, new UserNotificationQueue() }
            };

        public static IStaticQueue GetQueueByName(String name)
        {
            IStaticQueue queue;
            if (Queues.TryGetValue(name, out queue))
            {
                return queue;
            }
            return null;
        }

        public static void Begin()
        {
            new Thread(() =>
            {
                var staticQueues = Queues.Values.ToArray();

                while (true)
                {
                    Thread.Sleep(60 * 1000);
                    foreach (var staticQueue in staticQueues)
                    {
                        staticQueue.PruneOldEntries();
                    }
                }
            }).Start();
        } 
    }
}