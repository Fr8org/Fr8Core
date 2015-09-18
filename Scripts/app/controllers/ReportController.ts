module dockyard.controllers {
    'use strict';

    export interface IReportFactListScope extends ng.IScope {
        GetFacts: () => void;
        dtOptionsBuilder: any;
        dtColumnBuilder: any;
        dtInstance: any;
    }
    export interface IReportIncidentListScope extends ng.IScope {
        GetFacts: () => void;
        dtOptionsBuilder: any;
        dtColumnBuilder: any;
        dtInstance: any;
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
            'DTColumnBuilder'
        ];

        //private _facts: array;
        private _facts: Array<model.FactDTO>;

        constructor(
            private $rootScope: interfaces.IAppRootScope,            
            private $scope: IReportFactListScope,
            private ReportFactService:services.IReportFactService,
            private $modal,
            private $compile: ng.ICompileService,
            private $q: ng.IQService,
            private DTOptionsBuilder,
            private DTColumnBuilder)
        {           
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
                .withOption('createdRow', onRowCreate)
                .withOption('order', [[2, 'desc'], [9, 'desc']]);  
               
        }

        private OnRowCreate(row: any) {
            //datatables doesn't compile inserted rows. to access to scope we need to compile them
            //i think source of datatables should be changed to compile rows with it's parent scope (which is ours)
            this.$compile(angular.element(row).contents())(this.$scope);
        }

        private GetDataTableColumns() {
            return [
                this.DTColumnBuilder.newColumn('Id').withTitle('Id').notVisible(),
                this.DTColumnBuilder.newColumn('Activity').withTitle('Activity'),               
                this.DTColumnBuilder.newColumn('CreateDate').withTitle('Created Date')
                    .renderWith(function (data, type, full, meta) {
                    if (data != null || data != undefined) {                        
                        var dateValue = new Date(parseInt(full.CreateDate.replace(/\/Date\((.*?)\)\//gi, "$1")))
                        var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                        return date;
                    }
                    })                ,
                this.DTColumnBuilder.newColumn('CreatedBy').withTitle('Created By').notVisible(),
                this.DTColumnBuilder.newColumn('CreatedByID').withTitle('Created By Id').notVisible(),
                this.DTColumnBuilder.newColumn('CustomerId').withTitle('Customer Id'),
                this.DTColumnBuilder.newColumn('Data').withTitle('Data'),
                this.DTColumnBuilder.newColumn('LastUpdated').withTitle('Last Updated')
                    .renderWith(function (data, type, full, meta) {
                        if (data != null || data != undefined) {
                            var dateValue = new Date(parseInt(full.LastUpdated.replace(/\/Date\((.*?)\)\//gi, "$1")))
                            var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                            return date;
                        }
                    }),
                this.DTColumnBuilder.newColumn('ObjectId').withTitle('Object Id'),
                this.DTColumnBuilder.newColumn('PrimaryCategory').withTitle('Primary Category'),
                this.DTColumnBuilder.newColumn('SecondaryCategory').withTitle('Secondary Category'),
                this.DTColumnBuilder.newColumn('Status').withTitle('Status')                
            ];
        }        
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
            'DTColumnBuilder'
        ];

        //private _incidents: any;
        private _incidents: Array<model.IncidentDTO>;

        constructor(
            private $rootScope: interfaces.IAppRootScope,            
            private $scope: IReportIncidentListScope,
            private ReportIncidentService: services.IReportIncidentService,
            private $modal,
            private $compile: ng.ICompileService,
            private $q: ng.IQService,
            private DTOptionsBuilder,
            private DTColumnBuilder) {            
            this._incidents = ReportIncidentService.query();
            $scope.dtOptionsBuilder = this.GetDataTableOptionsFromTemplates();
            $scope.dtColumnBuilder = this.GetDataTableColumns();
            $scope.dtInstance = {};

        }

        //this function will be called on every reloadData call to data-table
        //angular removes $promise property of _processTemplates after successful load
        //so we need to manage promises manually
        private ResolveProcessTemplatesPromise() {
            if (this._incidents.$promise) {
                return this._incidents.$promise;
            }

            return this.$q.when(this._incidents);
        }

        private GetDataTableOptionsFromTemplates() {
            var onRowCreate = <(row: any) => void> angular.bind(this, this.OnRowCreate);
            var resolveData = <() => void> angular.bind(this, this.ResolveProcessTemplatesPromise);
            return this.DTOptionsBuilder
                .fromFnPromise(resolveData)
                .withPaginationType('full_numbers')
                .withOption('createdRow', onRowCreate)
                .withOption('order', [[2, 'desc'],[7,'desc']]);          
        }

        private OnRowCreate(row: any) {
            //datatables doesn't compile inserted rows. to access to scope we need to compile them
            //i think source of datatables should be changed to compile rows with it's parent scope (which is ours)
            this.$compile(angular.element(row).contents())(this.$scope);
        }

        private GetDataTableColumns() {
            return [
                this.DTColumnBuilder.newColumn('Id').withTitle('Id').notVisible(),
                this.DTColumnBuilder.newColumn('Activity').withTitle('Activity'),               
                this.DTColumnBuilder.newColumn('CreateDate').withTitle('Created Date')
                    .renderWith(function (data, type, full, meta) {
                        if (data != null || data != undefined) {
                            var dateValue = new Date(parseInt(full.CreateDate.replace(/\/Date\((.*?)\)\//gi, "$1")));
                            var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                            return date;
                        }
                    }),                
                this.DTColumnBuilder.newColumn('CustomerId').withTitle('Customer Id'),
                this.DTColumnBuilder.newColumn('Data').withTitle('Data'),
                this.DTColumnBuilder.newColumn('LastUpdated').withTitle('Last Updated')
                    .renderWith(function (data, type, full, meta) {
                        if (data != null || data != undefined) {
                            var dateValue = new Date(parseInt(full.LastUpdated.replace(/\/Date\((.*?)\)\//gi, "$1")))
                            var date = dateValue.toLocaleDateString() + ' ' + dateValue.toLocaleTimeString();
                            return date;
                        }
                    }),
                this.DTColumnBuilder.newColumn('ObjectId').withTitle('Object Id'),
                this.DTColumnBuilder.newColumn('PrimaryCategory').withTitle('Primary Category'),
                this.DTColumnBuilder.newColumn('SecondaryCategory').withTitle('Secondary Category'),
                this.DTColumnBuilder.newColumn('Status').withTitle('Status'),
                this.DTColumnBuilder.newColumn('IsHighPriority').withTitle('Is High Priority'),
                this.DTColumnBuilder.newColumn('Priority').withTitle('Priority')                
            ];
        }
    }

    app.controller('ReportFactController', ReportFactController); 
    app.controller('ReportIncidentController', ReportIncidentController);
}