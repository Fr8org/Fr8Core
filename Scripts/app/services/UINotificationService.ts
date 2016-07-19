/// <reference path="../_all.ts" />

/*
    The service implements toast notifications
*/

module dockyard.services {

    export interface IUINotificationService {
        notify: (title: string, message: string, status: dockyard.enums.UINotificationStatus, options: any) => void
    }

    class UINotificationService implements IUINotificationService {
        constructor(private ngToast: any) {
        }

        public notify(title: string, message: string, status: dockyard.enums.UINotificationStatus, options: any) {
            // Determines notification type and add necessary attributes
            switch (status) {
                case dockyard.enums.UINotificationStatus.Success:

                    break;
                case dockyard.enums.UINotificationStatus.Info:

                    break;
                case dockyard.enums.UINotificationStatus.Warning:

                    break;
                case dockyard.enums.UINotificationStatus.Error:

                    break;
            }
        }
    }

    app.factory('UINotificationService', ['ngToast', (ngToast: any): IUINotificationService => new UINotificationService(ngToast)]);
}