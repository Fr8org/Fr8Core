module dockyard.controllers {
    'use strict';

    export interface IReportIncidentListScope extends ng.IScope {
        GetFacts: () => void;
        dtOptionsBuilder: any;
        dtColumnDefs: any;
        incidentRecords: Array<model.IncidentDTO>;
        isSelectedRow: (row: model.IncidentDTO) => boolean;
        selectRow: (row: model.IncidentDTO) => void;
        shrinkData: (str: string) => string;
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

            var _selectedRow: model.IncidentDTO = null;

            $scope.dtOptionsBuilder = DTOptionsBuilder.newOptions().withPaginationType('full_numbers').withDisplayLength(10)
                .withOption('order', [[0, 'desc'], [6, 'desc']]);
            $scope.dtColumnDefs = this.getColumnDefs();

            $scope.selectRow = function (row: model.IncidentDTO) {
                if (_selectedRow === row) {
                    _selectedRow = null;
                }
                else {
                    _selectedRow = row;
                }
            };

            $scope.isSelectedRow = function (row: model.IncidentDTO) {
                return _selectedRow === row;
            };

            $scope.shrinkData = function (str: string) {
                var result = '';
                if (str) {
                    var lines = str.split('\n');
                    var i;

                    for (i = 0; i < Math.min(5, lines.length); ++i) {
                        if (result) {
                            result += '\n';
                        }

                        result += lines[i];
                    }
                }

                if (result.length > 400) {
                    result = result.substr(0, 400);
                }

                return result;
            };
        };

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