module dockyard.controllers {
    'use strict';

    export interface IReportFactListScope extends ng.IScope {
        GetFacts: () => void;
        dtOptionsBuilder: any;
        dtColumnBuilder: any;
        dtInstance: any;
    }

    class ReportFactController {

        public static $inject = [
            '$rootScope',
            //'$http',
            '$scope',
            'ReportFactService',
            '$modal',
            '$compile',
            '$q',
            'DTOptionsBuilder',
            'DTColumnBuilder'
        ];

        //private _facts: any;
        private _facts: Array<model.FactDTO>;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            //private $http: ng.IHttpService,
            private $scope: IReportFactListScope,
            private ReportFactService:services.IReportFactService,
            private $modal,
            private $compile: ng.ICompileService,
            private $q: ng.IQService,
            private DTOptionsBuilder,
            private DTColumnBuilder)
        {
            debugger;
            //this.GetData($http);
           
            this._facts = ReportFactService.query();  
            $scope.dtOptionsBuilder = this.GetDataTableOptionsFromTemplates();
            $scope.dtColumnBuilder = this.GetDataTableColumns();   
            $scope.dtInstance = {};
        
        }

        //this function will be called on every reloadData call to data-table
        //angular removes $promise property of _processTemplates after successful load
        //so we need to manage promises manually
        private ResolveProcessTemplatesPromise() {
            if (this._facts.$promise) {
                return this._facts.$promise;
            }

            return this.$q.when(this._facts);
        }

        private GetDataTableOptionsFromTemplates() {
            var onRowCreate = <(row: any) => void> angular.bind(this, this.OnRowCreate);
            var resolveData = <() => void> angular.bind(this, this.ResolveProcessTemplatesPromise);
            return this.DTOptionsBuilder
                .fromFnPromise(resolveData)
                .withPaginationType('full_numbers')
                .withOption('createdRow', onRowCreate);
        }

        private OnRowCreate(row: any) {
            //datatables doesn't compile inserted rows. to access to scope we need to compile them
            //i think source of datatables should be changed to compile rows with it's parent scope (which is ours)
            this.$compile(angular.element(row).contents())(this.$scope);
        }

        private GetDataTableColumns() {
            return [
                this.DTColumnBuilder.newColumn('id').withTitle('Id').notVisible(),
                this.DTColumnBuilder.newColumn('activity').withTitle('Activity'),
                this.DTColumnBuilder.newColumn('customer_id').withTitle('customer id'),
                this.DTColumnBuilder.newColumn('data').withTitle('Status')
                //    .renderWith(function (data, type, full, meta) {
                //        if (data.ProcessTemplateState === 1) {
                //            return '<span class="bold font-green-haze">Inactive</span>';
                //        } else {
                //            return '<span class="bold font-green-haze">Active</span>';
                //        }

                //    }),
                //this.DTColumnBuilder.newColumn(null)
                //    .withTitle('Actions')
                //    .notSortable()
                //    .renderWith(function (data: interfaces.IProcessTemplateVM, type, full, meta) {
                //        var deleteButton = '<button type="button" class="btn btn-sm red" ng-click="DeleteProcessTemplate(' + data.id + ');">Delete</button>';
                //        var editButton = '<button type="button" class="btn btn-sm green" ng-click="GoToProcessTemplatePage(' + data.id + ');">Edit</button>';
                //        return deleteButton + editButton;
                //    })
            ];
        }

        //private GetData($http) {
        //    $http.get('/report/getfacts')
        //        .then(function (resp) {
        //        var objects = angular.fromJson(resp.data);
        //        //angular.forEach(objects, function (it) {
        //        //        console.log(it);
        //        //        //$scope.actionTypes.push(
        //        //        //    new model.ActivityTemplate(
        //        //        //        it.id,
        //        //        //        it.name,
        //        //        //        it.version,
        //        //        //        it.componentActivities
        //        //        //        )
        //        //        //    );
        //        //    });
        //        });
        //}
    }

    app.controller('ReportFactController', ReportFactController);
}