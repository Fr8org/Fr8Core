using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace KwasantWeb.NotificationQueues
{
    /*
     * The following classes define queues for alerts to be displayed to users while they browse the wesbite.
     * There are two types of queues:
     * 1. PersonalNotificationQueue<T>
     *      - This queue is for things like an update on a page. It is not an important update, but can be something like 'The booking request recieved a new email, click here to refresh'
     *      - If no one is viewing the page which is affected by an update, no event listener is executed
     *      - This means that we only listen to events which will affect us
     *      - The queue is valid for the lifetime of the session, after which it will be cleared up
     *      
     * 2. SharedNotificationQueue<T>
     *      - This queue is for important messages, which we need to listen for even if no user is online
     *      - This can be used for things like 'A new booking request has been assigned to you'
     *      - These queues are static, and exist for the lifetime of the application
     *      - When a user logs in, they register their interest in particular queues
     *      - When new messages arrive in the static queue, they are forwarded to the interested parties
     *      - When messages are retrieved, they are marked as 'viewed' and removed from the queue
     *      - Occasionally, a process will run and prune the queues
     *          - Pruning does two things:
     *          - 1. Removing messages which have expired. When this happens, we execute the 'ObjectExpired' method
     *               Subclasses of this queue implement 'ObjectExpired'. Currently, we email users about a new booking request if they never recieved an update on screen
     *          - 2. Removing stale interested parties
     *               Interested party queues need to be cleaned to prevent memory leaks. 
     *               When we stop recieving 'GetUpdates' messages, we decide that the user is no longer connected, and delete their queue
     *  
     *  These queues have been designed to be simple to use and ensure thread safety and try to minimize the memory footprint
     *  
     *  To define a new queue, simply implement PersonalNotificationQueue<T> or SharedNotificationQueue<T>
     *  Once your queue is implemented, please refer to StaticAlertQueues.cs and define your queue and implement GetQueueByName
     *  Be sure to put it in the correct class (Personal vs Shared)!
     *  
     *  If your queue should always be polled (on any page), then:
     *  1. Go to Views/Shared/QueuePollingScript
     *  2. Define a new function at the bottom of the file, (see pollNewBookingRequest for an example)
     *  3. Update registerPolling to call your new queue function
     *  4. Note, individual pages can override the queue function if they require different functionality (different popup for example)
     *     The queues can also be disabled (see NewBookingRequestForUserQueueListenerEnabled for an example)
     * 
     *  If your queue is specific for a page, then:
     *  1. Take a look at Dashboard/Index
     *  2. Use the method getUpdateForPage. 
     *     The first parameter is the queue name, the second parameter is the ID used to build the page.
     *     For example, for Dashboard/Index the ID would be the ID of the booking request
     */

    public interface IPersonalNotificationQueue
    {
        int ObjectID { get; set; }
        IEnumerable<object> GetUpdates();
    }

    public interface IPersonalNotificationQueue<out T> : IPersonalNotificationQueue
    {
        IEnumerable<T> GetUpdates(Func<T, bool> predicate);
    }

    public class PersonalNotificationQueue<T> : IPersonalNotificationQueue<T>
        where T : class
    {
        public int ObjectID { get; set; }
        private readonly SynchronizedCollection<T> _baseCollection = new SynchronizedCollection<T>();

        /// <summary>
        /// Get all updates in the collection. Retrieved updates are removed. Non-typed. Threadsafe.
        /// </summary>
        public IEnumerable<object> GetUpdates()
        {
            return GetUpdates(null);
        }

        /// <summary>
        /// Get all updates in the collection. Retrieved updates are removed. Threadsafe.
        /// </summary>
        /// <param name="predicate">An optional predicate to retrieve updates. If null, all updates are returned</param>
        public IEnumerable<T> GetUpdates(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                var itemsToReturn = new List<T>(_baseCollection);
                _baseCollection.Clear();
                return itemsToReturn;
            }
            else
            {
                var itemsToReturn = new List<T>(_baseCollection.Where(predicate));
                foreach (var itemToReturn in itemsToReturn)
                    _baseCollection.Remove(itemToReturn);

                return itemsToReturn;
            }
        }

        /// <summary>
        /// Appends an update to the queue. Threadsafe.
        /// </summary>
        /// <param name="update">Update to append</param>
        protected void AppendUpdate(T update)
        {
            _baseCollection.Add(update);
        }
    }

    public interface IStaticQueue
    {
        void PruneOldEntries();
    }
    public interface ISharedNotificationQueue
    {
        IEnumerable<object> GetUpdates(String guid);
    }

    public interface ISharedNotificationQueue<out T> : ISharedNotificationQueue
    {
        void RegisterInterest(string guid);
        IEnumerable<T> GetUpdates(String guid, Func<T, bool> predicate);
    }
    public class SharedNotificationQueue<T> : ISharedNotificationQueue<T>, IStaticQueue
        where T : class//, IUserUpdateData
    {
        private readonly TimeSpan _expireInterestedPartiesAfter = TimeSpan.FromMinutes(15);
        private readonly ConcurrentDictionary<Object, DateTime> _objectExpirations = new ConcurrentDictionary<object, DateTime>();
        private readonly SynchronizedCollection<T> _baseCollection = new SynchronizedCollection<T>(); 
        private readonly ConcurrentDictionary<String, SynchronizedCollection<T>> _interestedPartyQueues = new ConcurrentDictionary<String, SynchronizedCollection<T>>();

        /// <summary>
        /// How long to wait before expiring update messages
        /// </summary>
        protected virtual TimeSpan ExpireUpdateAfter
        {
            get { return TimeSpan.FromMinutes(5); }
        }

        /// <summary>
        /// Register interest in this queue.
        /// </summary>
        /// <param name="guid">Unique guid generated by the AlertController</param>
        public void RegisterInterest(string guid)
        {
            var newList = new SynchronizedCollection<T>();
            _interestedPartyQueues[guid] = newList;

            MarkExpiration(newList, _expireInterestedPartiesAfter);
        }

        /// <summary>
        /// Get all updates in the collection. Retrieved updates are removed. Non-typed. Threadsafe.
        /// </summary>
        /// <param name="guid">Unique guid generated by the AlertController</param>
        public IEnumerable<object> GetUpdates(string guid)
        {
            return GetUpdates(guid, null);
        }

        /// <summary>
        /// Get all updates in the collection. Retrieved updates are removed. Non-typed. Threadsafe.
        /// </summary>
        /// <param name="guid">Unique guid generated by the AlertController</param>
        /// <param name="predicate">An optional predicate to retrieve updates. If null, all updates are returned</param>
        public virtual IEnumerable<T> GetUpdates(String guid, Func<T, bool> predicate)
        {
            if (_interestedPartyQueues.ContainsKey(guid))
            {
                var interestedPartyQueue = _interestedPartyQueues[guid];

                MarkExpiration(interestedPartyQueue, _expireInterestedPartiesAfter);

                var updates = predicate != null
                    ? interestedPartyQueue.Where(predicate).ToList()
                    : interestedPartyQueue.ToList();

                MarkUpdatesRead(updates);   // This may be changed to have an explict 'read' call (if the user clicks the notification)
                                            // For now, though - we assume they read it when it's displayed

                if (predicate == null)
                    interestedPartyQueue.Clear();
                else
                {
                    foreach (var update in updates)
                        interestedPartyQueue.Remove(update);
                }
                return updates;
            }

            return new T[0];
        }

        /// <summary>
        /// Appends an update to the queue. Threadsafe.
        /// </summary>
        /// <param name="update">Update to append</param>
        /// <param name="expireUpdateAfter">Expires in</param>
        protected void AppendUpdate(T update, TimeSpan expireUpdateAfter = default(TimeSpan))
        {
            if (_interestedPartyQueues.Any())
            {
                MarkExpiration(update, expireUpdateAfter == default(TimeSpan) ? ExpireUpdateAfter : expireUpdateAfter);

                foreach (var interestedPartyQueue in _interestedPartyQueues)
                {
                    _interestedPartyQueues[interestedPartyQueue.Key].Add(update);
                }
                _baseCollection.Add(update);
            }
            else
            {
                ObjectExpired(update);
            }
        }


        private void MarkUpdatesRead(IEnumerable<T> updates)
        {
            MarkUpdatesRead(updates.ToArray());
        }

        private void MarkUpdatesRead(params T[] updates)
        {
            foreach (var update in updates)
                _baseCollection.Remove(update);
        }

        private void MarkExpiration(Object obj, TimeSpan timeSpan)
        {
            _objectExpirations[obj] = DateTime.Now.Add(timeSpan);
        }

        private bool ObjectHasExpired(Object obj)
        {
            if (_objectExpirations.ContainsKey(obj))
                return _objectExpirations[obj] < DateTime.Now;

            return false;
        }

        /// <summary>
        /// Called when an update expires
        /// </summary>
        /// <param name="obj">Expired update</param>
        protected virtual void ObjectExpired(T obj)
        {
            //Do nothing - overridable
        }

        /// <summary>
        /// Remove all old interested parties and expired updates
        /// </summary>
        public void PruneOldEntries()
        {
            //Prune listeners
            foreach (var interestedPartyQueue in _interestedPartyQueues)
            {
                if (ObjectHasExpired(interestedPartyQueue.Value))
                {
                    const int maxAttempts = 5;
                    var currAttempt = 1;
                    bool success;
                    do
                    {
                        SynchronizedCollection<T> garbage;
                        success = _interestedPartyQueues.TryRemove(interestedPartyQueue.Key, out garbage);
                    } while (!success && currAttempt++ < maxAttempts);
                }
            }

            //Remove expired updates and dispatch the call on them
            var clonedList = new List<T>(_baseCollection);
            foreach (var update in clonedList)
            {
                if (ObjectHasExpired(update))
                {
                    ObjectExpired(update);
                    _baseCollection.Remove(update);
                }
            }
        }
    }

    public interface IUserUpdateData
    {
        String UserID { get; set; }
    }

    public interface IRoleUpdateData
    {
        String[] RoleNames { get; }
    }
}