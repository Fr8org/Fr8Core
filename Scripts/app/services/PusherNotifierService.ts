/// <reference path="../_all.ts"/>

module dockyard.services {

    export interface IPusherNotifierService {
        bindEventToChannel(channel: string, event: string, callback: Function): void;
        bindEventToAllChannels(event: string, callback: Function): void;
        unbindEvent(channel: string, event: string): void;
        unbindAllEvents(event: string): void;
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
            this.pusher.bind(channel, event, callback);
        }

        public bindEventToAllChannels(event: string, callback: Function): void {
            this.pusher.bind(event, callback);
        }

        public unbindEvent(channel: string, event: string): void {
            this.pusher.unbind(channel, event);
        }

        public unbindAllEvents(event: string) {
            this.pusher.unbindAll(event);
        }

        public disconnect(): void {
            this.pusher.disconnect();
        }
    }

    app.factory('PusherNotifierService', ['$pusher', ($pusher: any): IPusherNotifierService =>
        new PusherNotifierService($pusher)
    ]);
}  