module dockyard.interfaces {
    export interface IUINotificationService {

        // Main notification calls
        notifyToast: (message: string, status: dockyard.enums.UINotificationStatus, options: any) => Noty;
        notifyActivityStream: (subject: string, message: string, type: dockyard.enums.NotificationType, isCollapsed: boolean) => void;

        // Pusher related methods
        bindEventFromFrontEnd(callback: Function): void;
        bindEventToChannel(channel: string, event: string, callback: Function, context?: any): void;
        bindEventToClient(event: string, callback: Function, context?: any): void;
        removeEvent(channel: string, event: string): void;
        removeEventHandler(channel: string, event: string, handler: Function): void;
        removeAllHandlersForContext(channel: string, context: any);
        removeHandlerForAllEvents(channel: string, handler: Function): void;
        removeAllEvents(channel: string): void;
        disconnect(): void;
    }
} 
