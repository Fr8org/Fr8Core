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
      Container Controller 
    */
    class ContainerListController {
        // $inject static.
        // http://nick.perfectedz.com/angular-typescript-di-tip/
        
        public static $inject = [
            '$scope',
            'ContainerService',
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            '$state'
        ];

        constructor(
            private $scope: IContainerListScope,
            private ContainerService: services.IContainerService,
            private DTOptionsBuilder,
            private DTColumnDefBuilder,
            private $state) {

            $scope.$on('$viewContentLoaded', () => {
                // initialize core components
                Metronic.initAjax();
            });

            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withOption('order', [[3, 'desc']]).withPaginationType('full_numbers').withDisplayLength(10);   
            $scope.dtColumnDefs = this.getColumnDefs(); 
            $scope.currentAccountContainers = ContainerService.getAll({ id: null });
            console.log($scope.currentAccountContainers);
            $scope.goToContainerDetailsPage = <(container: interfaces.IContainerVM) => void> angular.bind(this, this.goToContainerDetailsPage);
        }

        private goToContainerDetailsPage(containerId) {
            this.$state.go('containerDetails', { id: containerId });
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0),
                this.DTColumnDefBuilder.newColumnDef(1),
                this.DTColumnDefBuilder.newColumnDef(2),
                this.DTColumnDefBuilder.newColumnDef(3),
                this.DTColumnDefBuilder.newColumnDef(4),
                this.DTColumnDefBuilder.newColumnDef(5)
            ];
        }

    }
    app.controller('ContainerListController', ContainerListController);
}