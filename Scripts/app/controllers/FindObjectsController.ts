module dockyard.controllers {

    export class FindObjectsController {
        public static $inject = [
            '$scope',
            '$http',
            '$location'
        ];

        constructor(
            private $scope,
            private $http: ng.IHttpService,
            private $location: ng.ILocationService) {

            $http.post('/api/routes/createFindObjectsRoute', {})
                .then(function (res: any) {
                    $location.path('/routes/' + res.data.id + '/builder');
                });
        }
    }

    app.controller('FindObjectsController', FindObjectsController);
}