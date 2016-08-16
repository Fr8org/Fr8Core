module dockyard.interfaces {
    export interface IUINotificationService {
        notify: (message: string, status: dockyard.enums.UINotificationStatus, options: any) => any
    }
} 