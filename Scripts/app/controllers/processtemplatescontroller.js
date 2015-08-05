/// <reference path="../_all.ts" />
/*
    Detail (view/add/edit) controller
*/
var dockyard;
(function (dockyard) {
    var controllers;
    (function (controllers) {
        'use strict';
        /*
            List controller
        */
        var ProcessTemplatesController = (function () {
            function ProcessTemplatesController($rootScope, $scope, ProcessTemplateService, $modal, $filter) {
                this.$rootScope = $rootScope;
                this.$scope = $scope;
                this.ProcessTemplateService = ProcessTemplateService;
                this.$modal = $modal;
                this.$filter = $filter;
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
                };
                //Delete link
                $scope.remove = function (pt) {
                    $modal.open({
                        animation: true,
                        templateUrl: 'modalDeleteConfirmation',
                        controller: 'ProcessTemplatesController__DeleteConfirmation',
                        size: null
                    }).result.then(function (selectedItem) {
                        //Deletion confirmed
                        ProcessTemplateService.delete({ id: pt.Id }).$promise.then(function () {
                            $rootScope.lastResult = "success";
                            $scope.ptvms;
                            window.location.href = '#processes';
                        });
                        //Remove from local collection
                        $scope.ptvms = $filter('filter')($scope.ptvms, function (value, index) { return value.Id !== pt.Id; });
                    }, function () {
                        //Deletion cancelled
                    });
                };
            }
            // $inject annotation.
            // It provides $injector with information about dependencies to be injected into constructor
            // it is better to have it close to the constructor, because the parameters must match in count and type.
            // See http://docs.angularjs.org/guide/di
            ProcessTemplatesController.$inject = [
                '$rootScope',
                '$scope',
                'ProcessTemplateService',
                '$modal',
                '$filter'
            ];
            return ProcessTemplatesController;
        })();
        app.controller('ProcessTemplatesController', ProcessTemplatesController);
        /*
            A simple controller for Delete confirmation dialog.
            Note: here goes a simple (not really a TypeScript) way to define a controller.
            Not as a class but as a lambda function.
        */
        app.controller('ProcessTemplatesController__DeleteConfirmation', function ($scope, $modalInstance) {
            $scope.ok = function () {
                $modalInstance.close();
            };
            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };
        });
    })(controllers = dockyard.controllers || (dockyard.controllers = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=processtemplatescontroller.js.map