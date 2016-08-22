module dockyard.controllers {
    'use strict';

    export interface IReportFactListScope extends ng.IScope {
        filter: any;
        query: model.PagedQueryDTO;
        promise: ng.IPromise<model.PagedResultDTO<model.FactDTO>>;
        result: model.PagedResultDTO<model.FactDTO>;
        getHistory: () => void;
        removeFilter: () => void;
        expandItem: (historyItem: model.HistoryItemDTO) => void;
        selected: any;
        canSeeOtherUserIncidents: boolean;
        isAllowedToSeeResults: boolean;
    }

    class ReportFactController {

        public static $inject = [
            '$scope',
            'ReportService'
        ];

        constructor(private $scope: IReportFactListScope, private ReportService: services.IReportService) {
            $scope.canSeeOtherUserIncidents = false;
            ReportService.canSeeOtherUserHistory()
                .$promise.then((result) => {
                    $scope.canSeeOtherUserIncidents = result.hasManageUserPrivilege;
                });
            $scope.selected = [];

            $scope.query = new model.PagedQueryDTO();
            $scope.query.itemPerPage = 10;
            $scope.query.page = 1;
            $scope.query.orderBy = "-createdDate";
            $scope.query.isCurrentUser = true;

            $scope.isAllowedToSeeResults = true;


            $scope.filter = {
                options: {
                    debounce: 500
                }
            };

            $scope.getHistory = <() => void>angular.bind(this, this.getHistory);
            $scope.removeFilter = <() => void>angular.bind(this, this.removeFilter);
            $scope.expandItem = <(historyItem: model.HistoryItemDTO) => void>angular.bind(this, this.expandItem);

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

                this.getHistory();
            });
        }

        private expandItem(historyItem: model.HistoryItemDTO) {
            if ((<any>historyItem).$isExpanded) {
                (<any>historyItem).$isExpanded = false;
            } else {
                (<any>historyItem).$isExpanded = true;
            }
        }

        private removeFilter() {
            this.$scope.query.filter = null;
            this.$scope.filter.showFilter = false;
            this.getHistory();
        }

        private getHistory() {
            this.$scope.promise = this.ReportService.getFactsByQuery(this.$scope.query).$promise;
            this.$scope.promise.then((data: model.PagedResultDTO<model.FactDTO>) => {
                this.$scope.isAllowedToSeeResults = true;
                this.$scope.result = data;
            }, (reason) => {
                if (reason.status === 403) {
                    this.$scope.isAllowedToSeeResults = false;
                }
            });
        }
    }

    app.controller('ReportFactController', ReportFactController);
}