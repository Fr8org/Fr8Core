app.controller('TestNotifyController', ['$scope', 'PusherNotifierService', function ($scope, p) {
    var callback = function () {
        alert();
    }
    p.bindEventToChannel('c1', 'e3', callback);
    p.unbindEvent('c1', 'e3', callback);
}]);