/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    /*
        List controller
    */
    class ProcessTemplatesController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'ProcessTemplateService',
            '$modal',
            '$filter'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: interfaces.IProcessTemplatesScope,
            private ProcessTemplateService: services.IProcessTemplateVMService,
            private $modal,
            private $filter) {

            //Clear the last result value (but still allow time for the confirmation message to show up)
            setTimeout(function () {
                delete $rootScope.lastResult;
            }, 300);

            $scope.$on('$viewContentLoaded', function () {
                // initialize core components
                Metronic.initAjax();
            });

            //Load Process Templates view model
            $scope.ptvms = ProcessTemplateService.query();

            //Detail/edit link
            $scope.nav = function (pt) {
                window.location.href = '#processes/' + pt.Id;
            }

            //Delete link
            $scope.remove = function (pt) {
                $modal.open({
                    animation: true,
                    templateUrl: 'modalDeleteConfirmation',
                    controller: 'ProcessTemplatesController__DeleteConfirmation',
 
                }).result.then(function (selectedItem) {
                    //Deletion confirmed
                    ProcessTemplateService.delete({ id: pt.Id }).$promise.then(function () {
                        $rootScope.lastResult = "success";
                        $scope.ptvms
                        window.location.href = '#processes';
                    });

                    //Remove from local collection
                    $scope.ptvms = $filter('filter')($scope.ptvms, function (value, index) { return value.Id !== pt.Id; })

                }, function () {
                    //Deletion cancelled
                });
            };
        }
    }
    app.controller('ProcessTemplatesController', ProcessTemplatesController);

    /*
        A simple controller for Delete confirmation dialog.
        Note: here goes a simple (not really a TypeScript) way to define a controller. 
        Not as a class but as a lambda function.
    */
    app.controller('ProcessTemplatesController__DeleteConfirmation', ($scope: any, $modalInstance: any): void => {
        $scope.ok = function () {
            $modalInstance.close();
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    });
}