/// <reference path="../_all.ts"/>

module dockyard.services {

    export interface IPusherNotifierService {
        bindEventToChannel(channel: string, event: string, callback: Function): void;
        bindEventToAllChannels(event: string, callback: Function): void;
        unbindEvent(channel: string, event: string): void;
        unbindAllEvents(event: string): void;
    }

    app.factory('PusherNotifierService', ['$pusher', ($pusher): IPusherNotifierService => {
            var client = new Pusher('123dd339500fed0ddd78', { encrypted: true });
            var pusher = $pusher(client);
            return {
                bindEventToChannel(channel: string, event: string, callback: Function): void {
                    pusher.bind(channel, event, callback);
                },
                bindEventToAllChannels(event: string, callback: Function): void {
                    pusher.bind(event, callback);
                },
                unbindEvent(channel: string, event: string): void {
                    pusher.unbind(channel, event);
                },
                unbindAllEvents(event: string) {
                    pusher.unbindAll(event);
                }
            };
        }
    ]);
}  