// to enable disable, service hitting real backend comment this line and 
// uncomment second line

app.run([
    '$httpBackend', httpBackend => {
        var validation = (url) => {
            return url.indexOf("/apimock") === -1;
        }
        // Fake AppInsights request
        httpBackend.when('POST', 'https://dc.services.visualstudio.com/v2/track').respond({});

        httpBackend.whenGET(validation).passThrough();
        httpBackend.whenPOST(validation).passThrough();
        httpBackend.whenPUT(validation).passThrough();
        httpBackend.whenDELETE(validation).passThrough();
    }   
]);

// this is the to delay mock http requests
// https://mrgamer.github.io/2013/10/19/full-backend-mock-with-angularjs/
app.factory('delayHTTP', ($q, $timeout) => {
    return {
        request(request) {
            if (request.url.indexOf("/apimocks") === -1)
                return $q.resolve(request);

            var delayedResponse = $q.defer();
            $timeout(() => {
                delayedResponse.resolve(request);
            }, Math.floor(600 + (Math.random() * 1400)));
            return delayedResponse.promise;
        },
        response(response) {
            var deferResponse = $q.defer();

            if (response.config.timeout && response.config.timeout.then) {
                response.config.timeout.then(() => {
                    deferResponse.reject();
                });
            } else {
                deferResponse.resolve(response);
            }

            return $timeout(() => {
                deferResponse.resolve(response);
                return deferResponse.promise;
            });
        }
    };
})
// delay HTTP
    .config(['$httpProvider', ($httpProvider) => {
        //i don't know why this is here.
        //$httpProvider.interceptors.push('delayHTTP');
    }]);