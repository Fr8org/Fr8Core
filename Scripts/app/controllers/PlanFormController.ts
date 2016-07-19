/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IPlanScope extends ng.IScope {
        ptvm: interfaces.IPlanFullDTO;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        planBuilder: any
    }

    class PlanFormController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'PlanService',
            '$stateParams',
            'StringService',
            '$state'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IPlanScope,
            private PlanService: services.IPlanService,
            private $stateParams: any,
            private StringService: services.IStringService,
            private $state: ng.ui.IStateService) {

            $scope.$on('$viewContentLoaded', function () {
                // initialize core components
                Metronic.initAjax();
            });
            //Load detailed information
            var id : string = $stateParams.id;
            if (/^[0-9]+$/.test(id) && parseInt(id) > 0) {
                $scope.ptvm = PlanService.get({ id: $stateParams.id });
            }

            //Save button
            $scope.submit = function (isValid) {
                if (isValid) {
                    if (!$scope.ptvm.plan.planState) {
                        $scope.ptvm.plan.planState = dockyard.model.PlanState.Inactive;
                    }

                    if (!$scope.ptvm.plan.visibility) {
                        $scope.ptvm.plan.visibility = dockyard.model.PlanVisibility.Standard;
                    }

                    var result = PlanService.save($scope.ptvm.plan);

                    result.$promise
                        .then(() => {

                            $state.go('plan.planBuilder', { id: result.plan.id});
                            //window.location.href = 'plans/' + result.plan.id + '/builder';
                        })
                        .catch(function (e) {
                            switch (e.status) {
                                case 404:
                                    $scope.errorMessage = StringService.plan["error404"];
                                    break;
                                case 400:
                                    $scope.errorMessage = StringService.plan["error400"];
                                    break;
                                default:
                                    $scope.errorMessage = StringService.plan["error"];
                                    break;
                            }
                        });
                }
            };
        }
    }

    app.controller('PlanFormController', PlanFormController);
}