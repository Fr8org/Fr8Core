/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';
    
    export interface ManageFileListScope extends ng.IScope {
        UploadFile: (file: interfaces.IFileVM) => void;
     /*   GoToProcessTemplatePage: (processTemplate: interfaces.IProcessTemplateVM) => void;
        DeleteProcessTemplate: (processTemplate: interfaces.IProcessTemplateVM) => void;*/
        dtOptionsBuilder: any;
        dtColumnBuilder: any;
        dtInstance: any;
    }

    class ManageFileListController {

        public static $inject = [
            '$rootScope',
            '$scope',
            'ManageFileService',
            '$modal',
            '$compile',
            '$q',
            'DTOptionsBuilder',
            'DTColumnBuilder',
            '$state',
            '$timeout'
        ];

        private _manageFiles: Array<interfaces.IFileVM>;

        constructor(
            private $rootScope: interfaces.IAppRootScope,
            private $scope: ManageFileListScope,
            private ManageFileService: services.IManageFileService,
            private $modal,
            private $compile: ng.ICompileService,
            private $q: ng.IQService,
            private DTOptionsBuilder,
            private DTColumnBuilder,
            private $state,
            private $timeout: ng.ITimeoutService) {

            this._manageFiles = ManageFileService.query();

            console.log(this._manageFiles);

            $scope.dtColumnBuilder = this.GetDataTableColumns();
            $scope.dtOptionsBuilder = this.GetDataTableOptionsFromFiles();

            $scope.dtInstance = {};
            $scope.UploadFile = <(file: interfaces.IFileVM) => void> angular.bind(this, this.uploadFile);
        }

        private GetDataTableColumns() {
            return [
                this.DTColumnBuilder.newColumn('id').withTitle('Id').notVisible(),
                this.DTColumnBuilder.newColumn('originalFileName').withTitle('Original File Name'),
                this.DTColumnBuilder.newColumn(null)
                    .withTitle('Actions')
                    .notSortable()
                    .renderWith(function (data: interfaces.IProcessTemplateVM, type, full, meta) {
                        var deleteButton = '<button type="button" class="btn btn-sm red" ng-click="DeleteProcessTemplate(' + data.id + ', $event);">Delete</button>';
                        return deleteButton;
                    })
            ];
        }

        private GetDataTableOptionsFromFiles() {
            var OnRowCreate = <(row: any) => void> angular.bind(this, this.OnRowCreate);
            var resolveData = <() => void> angular.bind(this, this.ResolveFilesPromise);
            return this.DTOptionsBuilder
                .fromFnPromise(resolveData)
                .withPaginationType('full_numbers')
                .withOption('createdRow', OnRowCreate);
        }

        //this function will be called on every reloadData call to data-table
        //angular removes $promise property of _processTemplates after successful load
        //so we need to manage promises manually
        private ResolveFilesPromise() {
            if (this._manageFiles.$promise) {
                return this._manageFiles.$promise;
            }
            return this.$q.when(this._manageFiles);
        }

        private OnRowCreate(row: any, data: any) {
            /*var ctrl = this;
            //datatables doesn't compile inserted rows. to access to scope we need to compile them
            //i think source of datatables should be changed to compile rows with it's parent scope (which is ours)
            this.$compile(angular.element(row).contents())(this.$scope);
            */

            /*angular.element(row).bind('click', function () {
                ctrl.$state.go('processTemplateDetails', { id: data.id });
            });*/
        }

        /* file picker hook*/

        private uploadFile($event) {
            $event.stopPropagation(); 

            var t = angular.element("#filePicker").find('div').find('div').get();
            $(t).attr('accept','pdf');
            console.log(t);
            this.$timeout(function () {
                angular.element("#filePicker").find('div').find('div').trigger('click');
            }, 0);


        };
    }
    app.controller('ManageFileListController', ManageFileListController);
}