module dockyard.services {
    export class UpstreamExtractor {
        constructor(
            private $http: ng.IHttpService,
            private $q: ng.IQService
        ) {
        }

        public clear() {
        }

        // So far extracts only Fields Descriptions.
        public extractUpstreamData(activityId: string) {
            var defer = this.$q.defer();

            this.$http
                .get('/api/routenodes/upstream_fields/?id=' + activityId)
                .then((res) => {
                    defer.resolve(res.data);
                });

            return defer.promise;
        }
    }
}

app.service('UpstreamExtractor', [ '$http', '$q', dockyard.services.UpstreamExtractor ]); 
