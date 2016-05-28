module dockyard.services {
    export class UpstreamExtractor {
        constructor(
            private $http: ng.IHttpService,
            private $q: ng.IQService
        ) {
        }

        public getAvailableData(activityId: string, availability: string) {
            var defer = this.$q.defer();

            var url = '/api/plan_nodes/signals/?id=' + activityId
                + '&availability=' + availability;

            this.$http.get(url)
                .then((res) => {
                    defer.resolve(res.data);
                });

            return defer.promise;
        }
    }
}

app.service('UpstreamExtractor', [ '$http', '$q', dockyard.services.UpstreamExtractor ]); 
