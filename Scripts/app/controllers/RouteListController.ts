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
        activeRoutesCategory: Array<interfaces.IRouteVM>;
        inActiveRoutesCategory: Array<interfaces.IRouteVM>;
        reArrangeRoutes: (route: interfaces.IRouteVM) => void;
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

            // This is to reArrangeRoutes so that plans get rendered in desired sections i.e Running or Plans Library
            $scope.$on('planExecutionCompleted-rearrangePlans', (event, route) => {
                this.reArrangeRoutes(route);
            });

            //Load Process Templates view model
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10);
            $scope.dtColumnDefs = this.getColumnDefs();
            $scope.activeRoutes = RouteService.getbystatus({ id: null, status: 2, category: '' });
            $scope.inActiveRoutes = RouteService.getbystatus({ id: null, status: 1, category: '' });
            $scope.activeRoutesCategory = RouteService.getbystatus({ id: null, status: 2, category: 'report' });
            $scope.inActiveRoutesCategory = RouteService.getbystatus({ id: null, status: 1, category: 'report' });
            $scope.executeRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.executeRoute);
            $scope.goToRoutePage = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.goToRoutePage);
            $scope.goToRouteDetailsPage = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.goToRouteDetailsPage);
            $scope.deleteRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.deleteRoute);
            $scope.activateRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.activateRoute);
            $scope.deactivateRoute = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.deactivateRoute);
            $scope.reArrangeRoutes = <(route: interfaces.IRouteVM) => void>angular.bind(this, this.reArrangeRoutes);
        }

        private reArrangeRoutes(route) {
            var routeIndex = null;
            if (route.routeState === 1) {
                routeIndex = this.$scope.activeRoutes.map(function (r) { return r.id }).indexOf(route.id);
                if (routeIndex > -1) {
                    this.$scope.inActiveRoutes.push(route);
                    this.$scope.activeRoutes.splice(routeIndex, 1);
                    this.$scope.activeRoutes = this.$scope.activeRoutes;
                }
            } else {
                routeIndex = this.$scope.inActiveRoutes.map(function (r) { return r.id }).indexOf(route.id);
                if (routeIndex > -1) {
                    this.$scope.activeRoutes.push(route);
                    this.$scope.inActiveRoutes.splice(routeIndex, 1);
                    this.$scope.inActiveRoutes = this.$scope.inActiveRoutes;
                }
            }
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
            this.RouteService.deactivate({ planId: route.id }).$promise.then((result) => {
                location.reload();
            }, () => {
                //deactivation failed
                //TODO show some kind of error message
            });
        }
        private executeRoute(route, $event) {
            // If Plan is inactive, activate it in-order to move under Running section
            var isInactive = route.routeState === 1;
            if (isInactive) {
                route.routeState = 2;
                this.reArrangeRoutes(route);
            }
            if ($event.ctrlKey) {
                this.$modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/_AddPayloadModal',
                    controller: 'PayloadFormController', resolve: { planId: () => route.id }
                });
            }
            else {
                this.RouteService
                    .runAndProcessClientAction(route.id)
                    .then((data) => {
                        if (isInactive && data && data.currentPlanType === 1) {
                            // mark plan as Inactive as it is Run Once and then rearrange
                            route.routeState = 1;
                            this.reArrangeRoutes(route);
                        }
                    })
                    .catch((failResponse) => {
                        if (failResponse.data.details === "GuestFail") {
                            location.href = "DockyardAccount/RegisterGuestUser";
                        } else {
                            if (isInactive && failResponse.toLowercase() === '1') {
                                // mark plan as Inactive as it is Run Once and then rearrange
                                route.routeState = 1;
                                this.reArrangeRoutes(route);
                            }
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