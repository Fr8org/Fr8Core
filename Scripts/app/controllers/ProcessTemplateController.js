/// <reference path="../_all.ts" />
/*
    Detail (view/add/edit) controller
*/
var dockyard;
(function (dockyard) {
    var controllers;
    (function (controllers) {
        'use strict';
        var ProcessTemplateController = (function () {
            function ProcessTemplateController($rootScope, $scope, ProcessTemplateService, $stateParams, StringService) {
                this.$rootScope = $rootScope;
                this.$scope = $scope;
                this.ProcessTemplateService = ProcessTemplateService;
                this.$stateParams = $stateParams;
                this.StringService = StringService;
                $scope.$on('$viewContentLoaded', function () {
                    // initialize core components
                    Metronic.initAjax();
                });
                //Load detailed information
                var id = $stateParams.id;
                if (/^[0-9]+$/.test(id) && parseInt(id) > 1) {
                    $scope.ptvm = ProcessTemplateService.get({ id: $stateParams.id });
                }
                //Save button
                $scope.submit = function (isValid) {
                    if (isValid) {
                        ProcessTemplateService.save($scope.ptvm).$promise
                            .then(function () {
                            $rootScope.lastResult = "success";
                            window.location.href = '#processes';
                        })
                            .catch(function (e) {
                            switch (e.status) {
                                case 404:
                                    $scope.errorMessage = StringService.processTemplate["error404"];
                                    break;
                                case 400:
                                    $scope.errorMessage = StringService.processTemplate["error400"];
                                    break;
                                default:
                                    $scope.errorMessage = StringService.processTemplate["error"];
                                    break;
                            }
                        });
                    }
                };
                // BEGIN ProcessBuilder event handlers.
                var criteriaIdSeq = 0;
                var actionIdSeq = 0;
                $scope.pbAddCriteriaClick = function () {
                    $scope.processBuilder.addCriteria({ id: ++criteriaIdSeq });
                };
                $scope.pbCriteriaClick = function (criteriaId) {
                    $scope.processBuilder.removeCriteria(criteriaId);
                };
                $scope.pbAddActionClick = function (criteriaId) {
                    $scope.processBuilder.addAction(criteriaId, { id: ++actionIdSeq });
                };
                $scope.pbActionClick = function (criteriaId, actionId) {
                    $scope.processBuilder.removeAction(criteriaId, actionId);
                };
                // END ProcessBuilder event handlers.
            }
            // $inject annotation.
            // It provides $injector with information about dependencies to be injected into constructor
            // it is better to have it close to the constructor, because the parameters must match in count and type.
            // See http://docs.angularjs.org/guide/di
            ProcessTemplateController.$inject = [
                '$rootScope',
                '$scope',
                'ProcessTemplateService',
                '$stateParams',
                'StringService'
            ];
            return ProcessTemplateController;
        })();
        app.controller('ProcessTemplateController', ProcessTemplateController);
    })(controllers = dockyard.controllers || (dockyard.controllers = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=ProcessTemplateController.js.map