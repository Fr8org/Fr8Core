/// <reference path="../_all.ts" />

/*
    The service implements toast notifications
*/

module dockyard.services {

    export interface IUINotificationService {
        notify: (message: string, status: dockyard.enums.UINotificationStatus, options: any) => any
    }

    declare var noty: Noty;

    class UINotificationService implements IUINotificationService {
        public notify(message: string, status: dockyard.enums.UINotificationStatus, options: any) {
            // For more options please look at noty library
            if (!options) {
                options = {
                    layout: 'topCenter',    // 'bottom', 'bottomCenter', 'bottomLeft', 'bottomRight', 'center', 'centerLeft', centerRight'
                                            // 'inline', 'top', 'topCenter', 'topLeft', 'topRight'
                    theme: 'relax',         // 'default' or 'relax'
                    dismissQueue: true,     // If you want to use queue feature set this true
                    timeout: 5000,          // delay for closing event. Set false for sticky notifications
                    force: false,           // adds notification to the beginning of queue when set to true
                    maxVisible: 5,          // you can set max visible notification for dismissQueue true option,
                    killer: false,          // for close all notifications before show
                    closeWith: ['click'],   // ['click', 'button', 'hover', 'backdrop'] // backdrop click will close all notifications
                }
            }

            // Determines notification type and add necessary attributes
            options['text'] = message;
            switch (status) {
                case dockyard.enums.UINotificationStatus.Success:
                    options['type'] = 'success';
                    break;
                case dockyard.enums.UINotificationStatus.Info:
                    options['type'] = 'information';
                    break;
                case dockyard.enums.UINotificationStatus.Warning:
                    options['type'] = 'warning';
                    break;
                case dockyard.enums.UINotificationStatus.Error:
                    options['type'] = 'error';
                    break;
                case dockyard.enums.UINotificationStatus.Alert:
                    options['type'] = 'alert';
                    break
            }
            return noty(options);
        }
    }

    app.factory('UINotificationService', [() => new UINotificationService()]);
}