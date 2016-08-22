/// <reference path="../_all.ts" />

/*
    The service implements toast notifications
*/
module dockyard.services {

    declare var noty: Noty;
    declare var appKey: string;

    class UINotificationService implements interfaces.IUINotificationService {
        private pusherClient: pusherjs.pusher.Pusher;
        private pusher: any;
        private timeout: ng.ITimeoutService;
        private notifierSubscribers: Array<Function> = [];

        constructor(private $pusher: any, private UserService: IUserService, $timeout: ng.ITimeoutService) {
            this.pusherClient = new Pusher(appKey, { encrypted: true });
            this.pusher = $pusher(this.pusherClient);
            this.timeout = $timeout;
        }

        // Uses noty for toast messages
        public notifyToast (message: string, status: dockyard.enums.UINotificationStatus, options: any) {
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
        };

        // Uses ActivityStream for notifications
        public notifyActivityStream(subject: string, message: string, type: dockyard.enums.NotificationType, isCollapsed?: boolean) {
            var data = {
                'Subject': subject,
                'Message': message,
                'NotificationType': type,
                'isCollapsed': isCollapsed
            };

            for (var i = 0; i < this.notifierSubscribers.length; i++) {
                var currentCallback = this.notifierSubscribers[i];
                currentCallback.call(this, data);
            }
        };

        // Pusher related methods
        public bindEventFromFrontEnd(callback: Function): void {
            this.notifierSubscribers.push(callback);
        };

        public bindEventToChannel(channel: string, event: string, callback: Function, context?: any): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.subscribe(channel);
            channelInstance.bind(event, callback, context);
        };

        public bindEventToClient(event: string, callback: Function, context?: any): void {
            this.pusher.bind(event, callback, context);
        };

        public removeEvent(channel: string, event: string): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(event);
            }
        };

        public removeEventHandler(channel: string, event: string, handler: Function): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(event, handler);
            }
        };

        public removeAllHandlersForContext(channel: string, context: any) {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(null, null, context);
            }
        };

        public removeHandlerForAllEvents(channel: string, handler: Function) {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(null, handler);
            }
        };

        public removeAllEvents(channel: string): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind();
            }
        };

        public disconnect(): void {
            this.pusher.disconnect();
        };

        // If you change the way how channel name is constructed, be sure to change it also
        // in the server-side code (PusherNotifier.cs). 
        private buildChannelName(email: string) {
            // URI encoding is necessary to remove the characters that Pusher does not support for 
            // Channel name. Since it also does not support %, we replace it either. 
            return 'fr8pusher_' + encodeURI(email).replace('%', '=');
        };
    }

    app.factory('UINotificationService', ['$pusher', 'UserService', '$timeout', ($pusher: any, UserService: IUserService, $timeout: ng.ITimeoutService): interfaces.IUINotificationService =>
        new UINotificationService($pusher, UserService, $timeout)
    ]);

}
