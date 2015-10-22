module dockyard.services {
  
    app.factory('PusherNotifierService', ['$pusher', $pusher => {
            var channels;
            var client = new Pusher('123dd339500fed0ddd78', { encrypted: true });
            var pusher = $pusher(client);

            return {
                bindEventToChannel: (channel, event , callback) => {
                    if (!channel || !event || !callback) {
                        return false;                  
                    }
                    if (!channels[channel]) {
                        channels[channel] = pusher.subscribe(channel);
                    }
                    channels[channel].bind(event, callback);   
                    return true;
                },
                bindEventToAllChannels: (event, callback) => {
                    if (!event || !callback) {
                        return false;
                    }
                    channels.forEach(channel => {
                        channel.bind(event, callback);   
                    });
                    return true;
                },
                unbindEvent: (channel, event) => {
                    if (channel && event && channels[channel]) {
                        channels[channel].unbind(event);
                        return true;
                    }
                    return false;
                }
            };
        }
     ]);
}  