/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IRouteListScope extends ng.IScope {
        executeRoute: (route: interfaces.IRouteVM) => void;
        goToRoutePage: (route: interfaces.IRouteVM) => void;
        goToRouteDetailsPage: (route: interfaces.IRouteVM) => void;
        deleteRoute: (route: interfaces.IRouteVM) => void;
        activateRoute: (route: interfaces.IRouteVM) => void;
        deactivateRoute: (route: interfaces.IRouteVM) => void;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        activeRoutes: Array<interfaces.IRouteVM>;
        inActiveRoutes: Array<interfaces.IRouteVM>;
    }

    /*
        List controller
    */
    class RouteListController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'RouteService',
            '$modal',
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            '$state'
        ];

        constructor(
            private $scope: IRouteListScope,
            private RouteService: services.IRouteService,
            private $modal,
            private DTOptionsBuilder,
            private DTColumnDefBuilder,
            private $state: ng.ui.IStateService) {

            $scope.$on('$viewContentLoaded', () => {
                // initialize core components
                Metronic.initAjax();
            });

            //Load Process Templates view model
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10);
            $scope.dtColumnDefs = this.getColumnDefs();
            $scope.activeRoutes = RouteService.getbystatus({ id: null, status: 2 });
            $scope.inActiveRoutes = RouteService.getbystatus({ id: null, status: 1 });
            $scope.executeRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.executeRoute);
            $scope.goToRoutePage = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.goToRoutePage);
            $scope.goToRouteDetailsPage = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.goToRouteDetailsPage);
            $scope.deleteRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.deleteRoute);
            $scope.activateRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.activateRoute);
            $scope.deactivateRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.deactivateRoute);
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0),
                this.DTColumnDefBuilder.newColumnDef(1),
                this.DTColumnDefBuilder.newColumnDef(2)
                    .renderWith(function (data, type, full, meta) {
                        if (data != null || data != undefined) {
                            var dateValue = new Date(data);
                            if (dateValue.getFullYear() == 1)
                                return "";
                            var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                            return date;
                        }
                    }),               
                this.DTColumnDefBuilder.newColumnDef(3).notSortable(),
                this.DTColumnDefBuilder.newColumnDef(4).notSortable()
            ];
        }

        private activateRoute(route) {
            this.RouteService.activate({ planId: route.id, routeBuilderActivate: false }).$promise.then((result) => {
                if (result != null && result.status === "validation_error" && result.redirectToRoute) {
                    this.goToRoutePage(route.id);
                }
                else {
                    location.reload();
                }
            }, (failResponse) => {
                //activation failed
                if (failResponse.data.details === "GuestFail") {
                    location.href = "DockyardAccount/RegisterGuestUser";
                }
            });

        }
        private deactivateRoute(route) {
            this.RouteService.deactivate(route).$promise.then((result) => {
                location.reload();
            }, () => {
                //deactivation failed
                //TODO show some kind of error message
            });
        }
        private executeRoute(planId, $event) {
            if ($event.ctrlKey) {
                this.$modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/_AddPayloadModal',
                    controller: 'PayloadFormController', resolve: { planId: () => planId }
                });
            }
            else {
                this.RouteService
                    .runAndProcessClientAction(planId)
                    .catch((failResponse) => {
                        if (failResponse.data.details === "GuestFail") {
                            location.href = "DockyardAccount/RegisterGuestUser";
                        }
                    });
            }
        }

        private goToRoutePage(planId) {
            this.$state.go('routeBuilder', { id: planId });
        }

        private goToRouteDetailsPage(planId) {
            this.$state.go('routeDetails', { id: planId });
        }

        private deleteRoute(planId: string, isActive: number) {
            //to save closure of our controller
            var self = this;
            this.$modal.open({
                animation: true,
                templateUrl: 'modalDeleteConfirmation',
                controller: 'RouteListController__DeleteConfirmation'

            }).result.then(() => {
                //Deletion confirmed
                this.RouteService.delete({ id: planId }).$promise.then(() => {
                    var procTemplates = isActive === 2 ? self.$scope.activeRoutes : self.$scope.inActiveRoutes;
                    //now loop through our existing templates and remove from local memory
                    for (var i = 0; i < procTemplates.length; i++) {
                        if (procTemplates[i].id === planId) {
                            procTemplates.splice(i, 1);
                            break;
                        }
                    }
                });
            });
        }
    }
    app.controller('RouteListController', RouteListController);

    /*
        A simple controller for Delete confirmation dialog.
        Note: here goes a simple (not really a TypeScript) way to define a controller. 
        Not as a class but as a lambda function.
    */
    app.controller('RouteListController__DeleteConfirmation', ['$scope', '$modalInstance', ($scope: any, $modalInstance: any): void => {
        $scope.ok = () => {
            $modalInstance.close();
        };

        $scope.cancel = () => {
            $modalInstance.dismiss('cancel');
        };
    }]);
}