module dockyard.controllers {
    'use strict';

    export interface IReportIncidentListScope extends ng.IScope {
        GetFacts: () => void;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        incidentRecords: Array<model.IncidentDTO>;
    }

    class ReportIncidentController {

        public static $inject = [
            '$rootScope',
            '$scope',
            'ReportIncidentService',
            '$modal',
            '$compile',
            '$q',
            'DTOptionsBuilder',
            'DTColumnDefBuilder'
        ];

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: IReportIncidentListScope,
            private ReportIncidentService: services.IReportIncidentService,
            private $modal,
            private $compile: ng.ICompileService,
            private $q: ng.IQService,
            private DTOptionsBuilder,
            private DTColumnDefBuilder) {           
            ReportIncidentService.query().$promise.then(incidentRecords => {
                $scope.incidentRecords = incidentRecords;
            });
            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10)
                .withOption('order', [[1, 'desc'], [6, 'desc']]);
            $scope.dtColumnDefs = this.getColumnDefs();

        }

        private getColumnDefs() {
            return [
                this.DTColumnDefBuilder.newColumnDef(0)
                    .renderWith(function (data, type, full, meta) {
                        if (data != null || data != undefined) {
                            var dateValue = new Date(data);
                            var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                            return date;
                        }
                    }),
            ];
        }       
    }

    app.controller('ReportIncidentController', ReportIncidentController);
}