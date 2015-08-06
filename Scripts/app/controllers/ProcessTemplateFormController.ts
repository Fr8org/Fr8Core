/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    class ProcessTemplateFormController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'ProcessTemplateService',
            '$stateParams',
            'StringService'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: interfaces.IProcessTemplateScope,
            private ProcessTemplateService: services.IProcessTemplateVMService,
            private $stateParams: any,
            private StringService: services.IStringService) {

            $scope.$on('$viewContentLoaded', function () {
                // initialize core components
                Metronic.initAjax();
            });

            //Load detailed information
            var id : string = $stateParams.id;
            if (/^[0-9]+$/.test(id) && parseInt(id) > 1) {
                $scope.ptvm = ProcessTemplateService.get({ id: $stateParams.id });
            }

            //Save button
            $scope.submit = function (isValid) {
                if (isValid) {
                    ProcessTemplateService.save($scope.ptvm).$promise
                        .finally(function () {
                            $rootScope.lastResult = "success";
                            window.location.href = '#processes/' + id + '/builder';
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
        }
    }

    app.controller('ProcessTemplateFormController', ProcessTemplateFormController);
}