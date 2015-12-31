module dockyard.controllers {
    'use strict';

    export interface IReportFactListScope extends ng.IScope {
        GetFacts: () => void;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        factRecords: Array<model.FactDTO>;
    }

    class ReportFactController {

        public static $inject = [
            '$rootScope',
            '$scope',
            'ReportFactService',
            '$modal',
            '$compile',
            '$q',
            'DTOptionsBuilder',
            'DTColumnDefBuilder'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IReportFactListScope,
            private ReportFactService: services.IReportFactService,
            private $modal,
            private $compile: ng.ICompileService,
            private $q: ng.IQService,
            private DTOptionsBuilder,
            private DTColumnDefBuilder) {
            ReportFactService.query().$promise.then(factRecords => {
                $scope.factRecords = factRecords;
            });
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10)
                .withOption('order', [[1, 'desc'], [6, 'desc']]);
            $scope.dtColumnDefs = this.getColumnDefs();
        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(1)
                    .renderWith(function (data, type, full, meta) {
                        if (data != null || data != undefined) {
                            var dateValue = new Date(data);
                            var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                            return date;
                        }
                    }),
                this.DTColumnDefBuilder.newColumnDef(4)
                    .renderWith(function (data, type, full, meta) {
                        if (data != null || data != undefined) {
                            var dateValue = new Date(data);
                            if (dateValue.getFullYear() == 1)
                                return "";
                            var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                            return date;
                        }
                    })
            ];
        }
    }

    app.controller('ReportFactController', ReportFactController);
}