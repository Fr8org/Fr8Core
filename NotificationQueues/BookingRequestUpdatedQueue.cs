using Data.Infrastructure;

namespace HubWeb.NotificationQueues
{
    public class BookingRequestUpdatedQueue : PersonalNotificationQueue<BookingRequestUpdatedData>
    {
        public BookingRequestUpdatedQueue() 
        {
            //AlertManager.AlertConversationMemberAdded += id =>
            //{
            //    if (ObjectID == id)
            //        AppendUpdate(new BookingRequestUpdatedData()
            //            {
            //                BookingRequestId = id
            //            });
            //};
        }
    }

    public class BookingRequestUpdatedData
    {
        public int BookingRequestId { get; set; }
    }
}