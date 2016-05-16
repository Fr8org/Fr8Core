/// <reference path="../_all.ts"/>

module dockyard.services {

    export const pusherNotifierSuccessEvent = 'fr8pusher_generic_success';
    export const pusherNotifierFailureEvent = 'fr8pusher_generic_failure';
    export const pusherNotifierExecutionEvent = 'fr8pusher_activity_execution_info';
    export const pusherNotifierTerminalEvent = 'fr8pusher_terminal_event';

    export interface IPusherNotifierService {
        bindEventToChannel(channel: string, event: string, callback: Function, context?: any): void;
        bindEventToClient(event: string, callback: Function, context?: any): void;
        removeEvent(channel: string, event: string): void;
        removeEventHandler(channel: string, event: string, handler: Function): void;
        removeAllHandlersForContext(channel: string, context: any);
        removeHandlerForAllEvents(channel: string, handler: Function): void;
        removeAllEvents(channel: string): void;
        disconnect(): void;
    }

    declare var appKey: string;

    class PusherNotifierService implements IPusherNotifierService {
        private client: pusherjs.pusher.Pusher;
        private pusher: any;

        constructor(private $pusher: any) {
            this.client = new Pusher(appKey, { encrypted: true });
            this.pusher = $pusher(this.client);
        }

        public bindEventToChannel(channel: string, event: string, callback: Function, context?: any): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.subscribe(channel);
            channelInstance.bind(event, callback, context);
        }

        public bindEventToClient(event: string, callback: Function, context?: any): void {
            this.pusher.bind(event, callback, context);
        }

        public removeEvent(channel: string, event: string): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(event);
            }
        }

        public removeEventHandler(channel: string, event: string, handler: Function): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(event, handler);
            }
        }

        public removeAllHandlersForContext(channel: string, context: any) {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(null, null, context);
            }
        }

        public removeHandlerForAllEvents(channel: string, handler: Function) {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(null, handler);
            }
        }

        public removeAllEvents(channel: string): void {
            channel = this.buildChannelName(channel);
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind();
            }
        }

        public disconnect(): void {
            this.pusher.disconnect();
        }

        private buildChannelName(email: string) {
            // If you change the way how channel name is constructed, be sure to change it also
            // in the server-side code (PusherNotifier.cs). 
            // URI encoding is necessary to remove the characters that Pusher does not support for 
            // Channel name. Since it also does not support %, we replace it either. 
            return 'fr8pusher_' + encodeURI(email).replace('%', '=');
        }
    }

    app.factory('PusherNotifierService', ['$pusher', ($pusher: any): IPusherNotifierService =>
        new PusherNotifierService($pusher)
    ]);
}  