app.controller('TestNotifyController', ['$scope', 'PusherNotifierService', function ($scope, p: dockyard.services.IPusherNotifierService) {
    var context = { title : 'Pusher' };
    var handler = function (): void {
       alert('My name is ' + this.title);
    };
    p.bindEventToChannel('c1', 'e1', handler, context);
    p.removeEvent('c1', 'e1');
}]);