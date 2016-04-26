module dockyard.controllers {
    'use strict';

    export interface IReportIncidentListScope extends ng.IScope {
        GetFacts: () => void;
        incidentRecords: Array<model.IncidentDTO>;
        isSelectedRow: (row: model.IncidentDTO) => boolean;
        selectRow: (row: model.IncidentDTO) => void;
        shrinkData: (str: string) => string;

        filter: any;
        query: model.HistoryQueryDTO;
        promise: ng.IPromise<model.HistoryResultDTO>;
        result: model.HistoryResultDTO;
        getHistory: () => void;
        removeFilter: () => void;
    }

    class ReportIncidentController {

        public static $inject = [
            '$scope',
            'ReportIncidentService'
        ];

        constructor(private $scope: IReportIncidentListScope, private ReportIncidentService: services.IReportIncidentService) {

            $scope.query = new model.HistoryQueryDTO();
            $scope.query.itemPerPage = 10;
            $scope.query.page = 1;

            this.getHistory();  
            /*
            ReportIncidentService.query().$promise.then(incidentRecords => {
                $scope.incidentRecords = incidentRecords;
            });
            */

            $scope.filter = {
                options: {
                    debounce: 500
                }
            };

            $scope.getHistory = <() => void>angular.bind(this, this.getHistory);
            $scope.removeFilter = <() => void>angular.bind(this, this.removeFilter);

            var _selectedRow: model.IncidentDTO = null;

            $scope.selectRow = (row: model.IncidentDTO) => {
                if (_selectedRow === row) {
                    _selectedRow = null;
                }
                else {
                    _selectedRow = row;
                }
            };

            $scope.isSelectedRow = (row: model.IncidentDTO) => (_selectedRow === row);

            $scope.shrinkData = (str: string) => {
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
        }
        
        private removeFilter() {
            this.$scope.query.filter = null;
            this.$scope.filter.showFilter = false;
            this.getHistory();
        }

        private getHistory() {
            this.$scope.promise = this.ReportIncidentService.getByQuery(this.$scope.query).$promise;
            this.$scope.promise.then((data: model.HistoryResultDTO) => {
                this.$scope.result = data;
            });
        }     
    }

    app.controller('ReportIncidentController', ReportIncidentController);
}