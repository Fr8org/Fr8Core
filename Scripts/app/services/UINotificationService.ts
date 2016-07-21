/// <reference path="../_all.ts" />

/*
    The service implements toast notifications
*/

module dockyard.services {

    export interface IUINotificationService {
        notify: (title: string, message: string, status: dockyard.enums.UINotificationStatus, options: any) => void
    }

    declare var noty: any;

    class UINotificationService implements IUINotificationService {
        constructor(private noty: any) {
        }

        public notify(title: string, message: string, status: dockyard.enums.UINotificationStatus, options: any) {
            // Determines notification type and add necessary attributes
            switch (status) {
                case dockyard.enums.UINotificationStatus.Success:
                    var n = noty({
                        text: message
                        });
                    break;
                case dockyard.enums.UINotificationStatus.Info:

                    break;
                case dockyard.enums.UINotificationStatus.Warning:

                    break;
                case dockyard.enums.UINotificationStatus.Error:

                    break;
                case dockyard.enums.UINotificationStatus.Alert:
                    break
            }
        }
    }

    //app.factory('UINotificationService', [IUINotificationService => new UINotificationService()]);
}