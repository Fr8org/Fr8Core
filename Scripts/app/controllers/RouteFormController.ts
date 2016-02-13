/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IRouteScope extends ng.IScope {
        ptvm: interfaces.IRouteVM;
        submit: (isValid: boolean) => void;
        errorMessage: string;
        routeBuilder: any
    }

    class RouteFormController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$rootScope',
            '$scope',
            'RouteService',
            '$stateParams',
            'StringService'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IRouteScope,
            private RouteService: services.IRouteService,
            private $stateParams: any,
            private StringService: services.IStringService) {

            $scope.$on('$viewContentLoaded', function () {
                // initialize core components
                Metronic.initAjax();
            });
            //Load detailed information
            var id : string = $stateParams.id;
            if (/^[0-9]+$/.test(id) && parseInt(id) > 0) {
                $scope.ptvm = RouteService.get({ id: $stateParams.id });
            }

            //Save button
            $scope.submit = function (isValid) {
                if (isValid) {
                    if (!$scope.ptvm.routeState) {
                        $scope.ptvm.routeState = dockyard.model.RouteState.Inactive;
                    }

                    var result = RouteService.save($scope.ptvm);

                    result.$promise
                        .then(() => {
                            $rootScope.lastResult = "success";
                            window.location.href = '#plans/' + result.id + '/builder';
                        })
                        .catch(function (e) {
                            switch (e.status) {
                                case 404:
                                    $scope.errorMessage = StringService.route["error404"];
                                    break;
                                case 400:
                                    $scope.errorMessage = StringService.route["error400"];
                                    break;
                                default:
                                    $scope.errorMessage = StringService.route["error"];
                                    break;
                            }
                        });
                }
            };
        }
    }

    app.controller('RouteFormController', RouteFormController);
}