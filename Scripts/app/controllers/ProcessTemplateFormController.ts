/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IProcessTemplateScope extends ng.IScope {
        ptvm: interfaces.IProcessTemplateVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        processBuilder: any
    }

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
            private $scope: IProcessTemplateScope,
            private ProcessTemplateService: services.IProcessTemplateService,
            private $stateParams: any,
            private StringService: services.IStringService) {

            $scope.$on('$viewContentLoaded', function () {
                // initialize core components
                Metronic.initAjax();
            });
            //Load detailed information
            var id : string = $stateParams.id;
            if (/^[0-9]+$/.test(id) && parseInt(id) > 0) {
                $scope.ptvm = ProcessTemplateService.get({ id: $stateParams.id });
            }

            //Save button
            $scope.submit = function (isValid) {
                if (isValid) {
                    if (!$scope.ptvm.processTemplateState) {
                        $scope.ptvm.processTemplateState = dockyard.model.ProcessState.Inactive;
                    }

                    var result = ProcessTemplateService.save($scope.ptvm);

                    result.$promise
                        .finally(function () {
                            console.log(result);
                            $rootScope.lastResult = "success";
                            window.location.href = '#processes/' + result.id + '/builder';
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