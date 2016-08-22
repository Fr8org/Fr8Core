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

                    var cratesDescription = <model.IncomingCratesDTO>res.data;

                    for (var i = 0; i < cratesDescription.availableCrates.length; i++) {

                        var crateDescription = cratesDescription.availableCrates[i];

                        for (var j = 0; j < crateDescription.fields.length; j ++) {
                            var field = cratesDescription.availableCrates[i].fields[j];

                            field.sourceCrateLabel = crateDescription.label;
                            field.sourceActivityId = crateDescription.sourceActivityId;
                            field.availability = crateDescription.availability;
                        }
                    }
                  
                    defer.resolve(cratesDescription);
                });

            return defer.promise;
        }
    }
}

app.service('UpstreamExtractor', [ '$http', '$q', dockyard.services.UpstreamExtractor ]); 
