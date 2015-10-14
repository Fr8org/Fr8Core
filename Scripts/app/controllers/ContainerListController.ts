/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IContainerListScope extends ng.IScope {
         dtOptionsBuilder: any;
         dtColumnDefs: any;
         currentAccountContainers: Array<interfaces.IContainerVM>;
         goToContainerDetailsPage: (container: interfaces.IContainerVM) => void;
    }

    /*
        Container controller
    */
    class ContainerListController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'ContainerService',
            '$modal',
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            '$state'
        ];

        constructor(
            private $scope: IContainerListScope,
            private ContainerService: services.IContainerService,
            private $modal,
            private DTOptionsBuilder,
            private DTColumnDefBuilder,
            private $state) {

            $scope.$on('$viewContentLoaded', () => {
                // initialize core components
                Metronic.initAjax();
            });

            //Load Process Templates view model
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10);   
            $scope.dtColumnDefs = this.getColumnDefs(); 
            $scope.currentAccountContainers = ContainerService.getAll({ id: null });
            $scope.goToContainerDetailsPage = <(container: interfaces.IContainerVM) => void> angular.bind(this, this.goToContainerDetailsPage);

        }

        private goToContainerDetailsPage(containerId) {
            this.$state.go('containerDetails', { id: containerId });
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0),
                this.DTColumnDefBuilder.newColumnDef(1),
                this.DTColumnDefBuilder.newColumnDef(2).notSortable(),
                this.DTColumnDefBuilder.newColumnDef(3).notSortable()
            ];
        }

    }
    app.controller('ContainerListController', ContainerListController);
}