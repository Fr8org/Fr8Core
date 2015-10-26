/// <reference path="../_all.ts"/>

module dockyard.services {

    export interface IPusherNotifierService {
        bindEventToChannel(channel: string, event: string, callback: Function): void;
        bindEventToAllChannels(event: string, callback: Function): void;
        unbindEvent(channel: string, event: string, callback: Function): void;
        unbindAllEvents(event: string, callback: Function): void;
        disconnect(): void;
    }

    class PusherNotifierService implements IPusherNotifierService {
        private client: pusherjs.pusher.Pusher;
        private pusher: any;
        private appKey: string = '123dd339500fed0ddd78';

        constructor(private $pusher: any) {
            this.client = new Pusher(this.appKey, { encrypted: true });
            this.pusher = $pusher(this.client);
        }        

        public bindEventToChannel(channel: string, event: string, callback: Function): void {
            var channelInstance = this.pusher.subscribe(channel);
            channelInstance.bind(event, callback);
        }

        public bindEventToAllChannels(event: string, callback: Function): void {
            this.pusher.bind(event, callback);
        }

        public unbindEvent(channel: string, event: string, callback: Function): void {
            var channelInstance = this.pusher.channel(channel);
            if (channelInstance != undefined) {
                channelInstance.unbind(event, callback);
            }
        }

        public unbindAllEvents(event: string, callback: Function) {
            this.pusher.unbind(event, callback);
        }

        public disconnect(): void {
            this.pusher.disconnect();
        }
    }

    app.factory('PusherNotifierService', ['$pusher', ($pusher: any): IPusherNotifierService =>
        new PusherNotifierService($pusher)
    ]);
}  