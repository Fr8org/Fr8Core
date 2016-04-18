/// <reference path="../_all.ts" />

module dockyard.controllers {
    'use strict';

    import filePickerEvents = dockyard.Fr8Events.FilePicker;

    export interface ManageFileListScope extends ng.IScope {
        UploadFile: (file: interfaces.IFileVM) => void;
        DeleteFile: (file: interfaces.IFileVM) => void;
        DetailFile: (file: interfaces.IFileVM) => void;
        AddFile: (file: interfaces.IFileVM) => void;
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
            '$timeout',
            'UIHelperService'
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
            private $timeout: ng.ITimeoutService,
            private uiHelperService: services.IUIHelperService) {

            this._manageFiles = ManageFileService.query();

            $scope.dtColumnBuilder = this.GetDataTableColumns();
            $scope.dtOptionsBuilder = this.GetDataTableOptionsFromFiles();

            $scope.dtInstance = {};
            $scope.UploadFile = <(file: interfaces.IFileVM) => void> angular.bind(this, this.UploadFile);
            $scope.DeleteFile = <(file: interfaces.IFileVM) => void>angular.bind(this, this.DeleteFile);
            $scope.DetailFile = <(file: interfaces.IFileVM) => void>angular.bind(this, this.DetailFile);
            $scope.AddFile = <(file: interfaces.IFileVM) => void> angular.bind(this, this.AddFile);

            $scope.$on(<any>filePickerEvents.FP_SUCCESS, function (event, fileDTO) {
                $scope.AddFile(fileDTO);
            });

        }

        private AddFile(file) {
            this._manageFiles.push(file);
            this.$scope.dtInstance.reloadData();
        }

        private GetDataTableColumns() {
            var me = this;

            return [
                this.DTColumnBuilder.newColumn('id').withTitle('Id').notVisible(),
                this.DTColumnBuilder.newColumn(null)
                    .withTitle('Original File Name')
                    .renderWith(function (data: interfaces.IFileVM, type, full, meta) {
                        var fileNameRow = '<div ng-click="DetailFile(' + data.id + ')">' + data.originalFileName + '</div>';
                        return fileNameRow;
                    }),
                this.DTColumnBuilder.newColumn(null)
                    .withTitle('Tags')
                    .renderWith(function (data: interfaces.IFileVM, type, full, meta) {
                        var tags = data.tags == null ? "" : data.tags;
                        var fileNameRow = '<div ng-click="DetailFile(' + data.id + ')">' + tags + '</div>';
                        return fileNameRow;
                    }),
                this.DTColumnBuilder.newColumn(null)
                    .withTitle('Actions')
                    .notSortable()
                    .renderWith(function (data: interfaces.IFileVM, type, full, meta) {
                        var actionButtons = '<button type="button" class="btn btn-sm red" ng-click="DeleteFile(' + data.id + ', $event)">Delete</button>';
                        return actionButtons;
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
        //angular removes $promise property of _plans after successful load
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
            //i think source of datatables should be changed to compile rows with it's parent scope (which is ours)*/
            this.$compile(angular.element(row).contents())(this.$scope);

            /*angular.element(row).bind('click', function () {
                ctrl.$state.go('processTemplateDetails', { id: data.id });
            });*/
        }

        private DetailFile(fileId) {
            this.$state.go('fileDetail', { id: fileId });
        }

        private UploadFile($event) {
            $event.preventDefault();
            $event.stopPropagation();
            this.$timeout(function () {
                angular.element("#filePicker").find('div').find('div').trigger('click');
            }, 0);
        };

        private DeleteFile(fileId, $event) {
            $event.preventDefault();
            $event.stopPropagation();

            var me = this;

            var alertMessage = new model.AlertDTO();
            alertMessage.title = "Delete Confirmation";
            alertMessage.body = "Are you sure that you wish to delete this file?";

            this.uiHelperService
                .openConfirmationModal(alertMessage).then(function () {

                    //Deletion confirmed
                    me.ManageFileService.delete({ id: fileId }).$promise.then(function () {
                        me.$rootScope.lastResult = "success";
                        //now loop through our existing templates and remove from local memory
                        for (var i = 0; i < me._manageFiles.length; i++) {
                            if (me._manageFiles[i].id === fileId) {
                                me._manageFiles.splice(i, 1);
                                me.$scope.dtInstance.reloadData();
                                break;
                            }
                        }
                    });
                });
        }
    }
    app.controller('ManageFileListController', ManageFileListController);
}