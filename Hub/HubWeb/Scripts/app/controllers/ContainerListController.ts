/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IContainerListScope extends ng.IScope {
        filter: any;
        query: model.PagedQueryDTO;
        promise: ng.IPromise<model.PagedResultDTO<interfaces.IContainerVM>>;
        result: model.PagedResultDTO<interfaces.IContainerVM>;
        removeFilter: () => void;
        loadContainers: () => void;
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
            '$state'
        ];

        constructor(
            private $scope: IContainerListScope,
            private ContainerService: services.IContainerService,
            private $state) {

            $scope.query = new model.PagedQueryDTO();
            $scope.query.itemPerPage = 10;
            $scope.query.page = 1;
            $scope.query.orderBy = "-createDate";
            $scope.query.isCurrentUser = true;

            $scope.filter = {
                options: {
                    debounce: 500
                }
            };

            $scope.loadContainers = <() => void>angular.bind(this, this.loadContainers);
            $scope.removeFilter = <() => void>angular.bind(this, this.removeFilter);

            $scope.$on('$viewContentLoaded', () => {
                // initialize core components
                Metronic.initAjax();
            });

            $scope.$watch('query.filter', (newValue, oldValue) => {
                var bookmark: number = 1;
                if (!oldValue) {
                    bookmark = $scope.query.page;
                }
                if (newValue !== oldValue) {
                    $scope.query.page = 1;
                }
                if (!newValue) {
                    $scope.query.page = bookmark;
                }

                this.loadContainers();
            });

            $scope.goToContainerDetailsPage = <(container: interfaces.IContainerVM) => void>angular.bind(this, this.goToContainerDetailsPage);
        }

        private removeFilter() {
            this.$scope.query.filter = null;
            this.$scope.filter.showFilter = false;
            this.loadContainers();
        }

        private loadContainers() {
            this.$scope.promise = this.ContainerService.getByQuery(this.$scope.query).$promise;
            this.$scope.promise.then((data: model.PagedResultDTO<interfaces.IContainerVM>) => {
                this.$scope.result = data;
            });
        }

        private goToContainerDetailsPage(containerId) {
            this.$state.go('containerDetails', { id: containerId });
        }

    }
    app.controller('ContainerListController', ContainerListController);
}