module dockyard.controllers {

    app.controller('TestController', ['$scope', 'PusherNotifierService',
        ($scope, pusherNotifierService) => {

            pusherNotifierService.bindEventToChannel('channel1', 'event1', () => {
                alert('HelloWorld1');
            });
            pusherNotifierService.bindEventToChannel('channel2', 'event2', () => {
                alert('HelloWorld2');
            });
            pusherNotifierService.bindEventToAllChannels('event3', () => {
                alert('HelloWorld3');
            });

        }]);
}