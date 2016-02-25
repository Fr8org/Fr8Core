module dockyard.controllers {

    export interface IFindObjectsResultsScope extends ng.IScope {
        payload: any;
        id: string;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        columns: any;
        data: any;
        title: string;
    }

    export class FindObjectsResultsController {
        public static $inject = [
            '$scope',
            '$q',
            'ContainerService',
            '$stateParams',
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            'CrateHelper'
        ];

        constructor(
            private $scope: IFindObjectsResultsScope,
            private $q,
            private ContainerService: services.IContainerService,
            private $stateParams: any,
            private DTOptionsBuilder: any,
            private DTColumnDefBuilder: any,
            private CrateHelper: services.CrateHelper) {

            var self = this;

            $scope.payload = ContainerService.getPayload({ id: $stateParams.id });

            $scope.title = 'Loading data...';
            $scope.data = [];
            $scope.columns = [];

            $scope.dtOptionsBuilder = DTOptionsBuilder
                .newOptions()
                .withOption('order', [[0, 'asc']])
                .withPaginationType('full_numbers')
                .withDisplayLength(10);

            $scope.dtColumnDefs = [];

            $scope.payload.$promise
                .then(function () {
                    var crate = self.CrateHelper.findByLabel(<model.CrateStorage>($scope.payload.crateStorage), 'Sql Query Result');
                    var contents = <any>crate.contents;

                    $scope.title = contents.Name || 'Find Objects Results';

                    if (contents.PayloadObjects && contents.PayloadObjects.length) {
                        // Create columns.
                        var po = contents.PayloadObjects[0].PayloadObject;
                        var i = 0;

                        angular.forEach(po, function (it) {
                            $scope.columns.push(it.key);
                            $scope.dtColumnDefs.push(self.DTColumnDefBuilder.newColumnDef(i));
                            ++i;
                        });

                        // Create data.
                        angular.forEach(contents.PayloadObjects, function (po) {
                            var row = [];
                            angular.forEach(po.PayloadObject, function (it) {
                                row.push(it.value);
                            });

                            $scope.data.push(row);
                        });
                    }
                });
        }
    }

    app.controller('FindObjectsResultsController', FindObjectsResultsController);
}