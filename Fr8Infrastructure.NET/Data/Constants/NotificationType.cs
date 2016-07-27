namespace Fr8.Infrastructure.Data.Constants
{
    // Don't forget to add corresponding type to NotificationType.ts
    public enum NotificationType
    {
        GenericSuccess = 1,     // fr8pusher_generic_success
        GenericFailure = 2,     // fr8pusher_generic_failure
        GenericInfo = 3,        // fr8pusher_activity_execution_info
        TerminalEvent = 4,      // fr8pusher_terminal_event
        ExecutionStopped = 5
    }
}
