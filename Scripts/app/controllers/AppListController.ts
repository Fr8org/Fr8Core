/// <reference path="../_all.ts" />

/*
    Detail (view/add/edit) controller
*/
module dockyard.controllers {
    'use strict';

    export interface IAppListScope extends ng.IScope {
        filter: any;
        query: model.PlanQueryDTO;
        promise: ng.IPromise<model.PlanResultDTO>;
        result: model.PlanResultDTO;
        removeFilter: () => void;
        loadApps: () => void;
    }

    /*
      AppList Controller 
    */
    class AppListController {
        // $inject static.
        // http://nick.perfectedz.com/angular-typescript-di-tip/

        public static $inject = [
            '$scope',
            'PlanService',
            '$state'
        ];

        constructor(
            private $scope: IAppListScope,
            private PlanService: services.IPlanService,
            private $state) {

            let query = new model.PlanQueryDTO();
            query.planPerPage = 10;
            query.page = 1;
            query.appsOnly = true;
            query.orderBy = "-createDate";
            $scope.query = query;
            $scope
            $scope.filter = {
                options: {
                    debounce: 500
                }
            };

            $scope.loadApps = <() => void>angular.bind(this, this.loadApps);
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

                this.loadApps();
            });

            //$scope.goToContainerDetailsPage = <(container: interfaces.IContainerVM) => void>angular.bind(this, this.goToContainerDetailsPage);
        }

        private removeFilter() {
            this.$scope.query.filter = null;
            this.$scope.filter.showFilter = false;
            this.loadApps();
        }

        private loadApps() {
            this.$scope.promise = this.PlanService.getByQuery(this.$scope.query).$promise;
            this.$scope.promise.then((data: model.PlanResultDTO) => {
                this.$scope.result = data;
            });
        }
    }
    app.controller('AppListController', AppListController);
}