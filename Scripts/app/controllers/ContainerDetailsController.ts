/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    export interface IContainerDetailsScope extends ng.IScope {
        container: interfaces.IContainerVM;
        payload: interfaces.IContainerVM;
        facts: Array<interfaces.IReportFactVM>;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        errorMessage: string;
        id: string;
        loadContent: () => void;
        loadHistory: () => void;
        showContent: boolean;
    }

    class ContainerDetailsController {
        // $inject static.
        // http://nick.perfectedz.com/angular-typescript-di-tip/

        public static $inject = [
            '$scope',
            'ContainerService',
            'DTOptionsBuilder',
            'DTColumnDefBuilder',
            '$stateParams'
        ];

        constructor(
            private $scope: IContainerDetailsScope,
            private ContainerService: services.IContainerService,
            private DTOptionsBuilder,
            private DTColumnDefBuilder,
            private $stateParams: any) {

            $scope.id = $stateParams.id;
            $scope.showContent = true;
            // initialize DataTables
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withOption('order', [[0, 'desc']]).withPaginationType('full_numbers').withDisplayLength(10);
            $scope.dtColumnDefs = this.getColumnDefs();
            
            function fetchContent() {
                if (!$scope.container) { // this is to avoid un-ncessary roundtrips
                    $scope.container = ContainerService.getSingle({ id: $scope.id });
                }
                if (!$scope.payload) { // this is to avoid un-ncessary roundtrips
                    $scope.payload = ContainerService.getPayload({ id: $scope.id });
                }
            }
            fetchContent();

            $scope.loadContent = function () {
                $scope.showContent = true;
                fetchContent();
            };
            $scope.loadHistory = function () {
                $scope.showContent = false;
                if (!$scope.facts || $scope.facts.length <= 0) {
                    $scope.facts = ContainerService.getFacts({ objectId: $scope.id });
                }
            };
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0),
                this.DTColumnDefBuilder.newColumnDef(1),
                this.DTColumnDefBuilder.newColumnDef(2),
                this.DTColumnDefBuilder.newColumnDef(3),
                this.DTColumnDefBuilder.newColumnDef(4),
                this.DTColumnDefBuilder.newColumnDef(5),
                this.DTColumnDefBuilder.newColumnDef(6),
                this.DTColumnDefBuilder.newColumnDef(7)
            ];
        }

    }

    app.controller('ContainerDetailsController', ContainerDetailsController);
}