// to enable disable, service hitting real backend comment this line and 
// uncomment second line
//app.constant('urlPrefix', '/apimocks');
app.constant('urlPrefix', '/api');
app.run([
    '$httpBackend', function (httpBackend) {
        var validation = function (url) {
            return url.indexOf("/apimock") === -1;
        };
        httpBackend.whenGET(validation).passThrough();
        httpBackend.whenGET(validation).passThrough();
        httpBackend.whenGET(validation).passThrough();
        httpBackend.whenPOST(validation).passThrough();
        httpBackend.whenPUT(validation).passThrough();
        httpBackend.whenDELETE(validation).passThrough();
    }
]);
// this is the to delay mock http requests
// https://mrgamer.github.io/2013/10/19/full-backend-mock-with-angularjs/
app.factory('delayHTTP', function ($q, $timeout) {
    return {
        request: function (request) {
            if (request.url.indexOf("/apimocks") === -1)
                return $q.resolve(request);
            var delayedResponse = $q.defer();
            $timeout(function () {
                delayedResponse.resolve(request);
            }, Math.floor(600 + (Math.random() * 1400)));
            return delayedResponse.promise;
        },
        response: function (response) {
            var deferResponse = $q.defer();
            if (response.config.timeout && response.config.timeout.then) {
                response.config.timeout.then(function () {
                    deferResponse.reject();
                });
            }
            else {
                deferResponse.resolve(response);
            }
            return $timeout(function () {
                deferResponse.resolve(response);
                return deferResponse.promise;
            });
        }
    };
})
    .config(['$httpProvider', 'urlPrefix', function ($httpProvider, urlPrefix) {
        $httpProvider.interceptors.push('delayHTTP');
    }]);
