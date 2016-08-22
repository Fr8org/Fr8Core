module dockyard.controllers {
    'use strict';

    export interface IReportIncidentListScope extends ng.IScope {
        filter: any;
        query: model.PagedQueryDTO;
        promise: ng.IPromise<model.PagedResultDTO<model.IncidentDTO>>;
        result: model.PagedResultDTO<model.IncidentDTO>;
        getHistory: () => void;
        removeFilter: () => void;
        openModal: (historyItem: model.HistoryItemDTO) => void;
        selected: any;
        isAllowedToSeeResults: boolean;
        canSeeOtherUserIncidents: boolean;
    }

    class ReportIncidentController {

        public static $inject = [
            '$scope',
            '$modal',
            'ReportService'
        ];

        constructor(private $scope: IReportIncidentListScope, private $modal: any, private ReportService: services.IReportService) {
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
            $scope.openModal = <(historyItem: model.HistoryItemDTO) => void>angular.bind(this, this.openModal);

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

        private openModal(historyItem: model.HistoryItemDTO) {
            var modalInstance = this.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/ReportIncidentModal',
                controller: 'ReportIncidentModalController',
                size: 'lg',
                resolve: {
                    historyItem: () => historyItem
                }
            });
        }

        private removeFilter() {
            this.$scope.query.filter = null;
            this.$scope.filter.showFilter = false;
            this.getHistory();
        }

        private getHistory() {
            this.$scope.promise = this.ReportService.getIncidentsByQuery(this.$scope.query).$promise;
            this.$scope.promise.then((data: model.PagedResultDTO<model.IncidentDTO>) => {
                this.$scope.isAllowedToSeeResults = true;
                this.$scope.result = data;
            }, (reason) => {
                if (reason.status === 403) {
                    this.$scope.isAllowedToSeeResults = false;
                }
            });
        }
    }

    app.controller('ReportIncidentController', ReportIncidentController);

    app.controller('ReportIncidentModalController', ['$scope', '$modalInstance', 'historyItem', ($scope: any, $modalInstance: any, historyItem: interfaces.IHistoryItemDTO): void => {

        $scope.historyItem = historyItem;

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };
    }]);
}