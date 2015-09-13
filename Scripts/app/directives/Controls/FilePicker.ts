/// <reference path="../../_all.ts" />
module dockyard.directives.filePicker {
    'use strict';

    export interface IFilePickerScope extends ng.IScope {
        OnFileSelect: ($file: any) => void;
        ListExistingFiles: () => void;
        Save: () => void;
        field: model.FileField;
        selectedFile: interfaces.IFileDescriptionDTO;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class FilePicker implements ng.IDirective {
        public link: (scope: IFilePickerScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/FilePicker';
        public controller: ($scope: IFilePickerScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '='
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IFilePickerScope;

        constructor(private $modal: any, private FileService: IFileService) {
            FilePicker.prototype.link = (
                scope: IFilePickerScope,
                element: ng.IAugmentedJQuery,
                attrs: ng.IAttributes) => {

                //Link function goes here
            };

            FilePicker.prototype.controller = (
                $scope: IFilePickerScope,
                $element: ng.IAugmentedJQuery,
                $attrs: ng.IAttributes) => {
                this._$element = $element;
                this._$scope = $scope;
                this.FileService = FileService;
                this._$scope.selectedFile = null;
                $scope.OnFileSelect = <($file: any) => void> angular.bind(this, this.OnFileSelect);
                $scope.ListExistingFiles = <() => void> angular.bind(this, this.ListExistingFiles);
                $scope.Save = <() => void> angular.bind(this, this.Save);
            };
        }

        private OnFileUploadSuccess(fileDTO: interfaces.IFileDescriptionDTO) {
            this._$scope.selectedFile = fileDTO;
        }

        private OnFileUploadFail(status: any) {
            alert('sorry file upload failed with status: ' + status);
        }

        private OnFileSelect($file) {
            var onFileUploadSuccess = <(fileDTO: interfaces.IFileDTO) => void> angular.bind(this, this.OnFileUploadSuccess);
            var onFileUploadFail = <(status: any) => void> angular.bind(this, this.OnFileUploadFail);
            this.FileService.uploadFile($file).then(onFileUploadSuccess, onFileUploadFail);
        }

        private Save() {
            if (this._$scope.selectedFile === null) {
                //raise some kind of error to prevent continuing
                alert('No file was selected!!!!!!');
                return;
            }
            
            //we should assign id of selected file to model value
            //this._$scope.field.value = this._fileDTO.id.toString();
            alert('Selected FileDO ID -> ' + this._$scope.selectedFile.id.toString());
            //TODO add this file's id to CrateDO
        }

        private OnExistingFileSelected(fileDTO: interfaces.IFileDescriptionDTO) {
            this._$scope.selectedFile = fileDTO;
        }

        private OnFilesLoaded(filesDTO: Array<interfaces.IFileDescriptionDTO>) {

            var modalInstance = this.$modal.open({
                animation: true,
                templateUrl: '/AngularTemplate/FileSelectorModal',
                controller: 'FilePicker__FileSelectorModalController',
                size: 'm',
                resolve: {
                    files: () => filesDTO
                }
            });

            var onExistingFileSelected = <(fileDTO: interfaces.IFileDescriptionDTO) => void> angular.bind(this, this.OnExistingFileSelected);
            modalInstance.result.then(onExistingFileSelected);
        }

        private ListExistingFiles() {
            var onFilesLoaded = <(filesDTO: Array<interfaces.IFileDescriptionDTO>) => void> angular.bind(this, this.OnFilesLoaded);
            this.FileService.listFiles().then(onFilesLoaded);
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($modal: any, FileService: IFileService) => {
                return new FilePicker($modal, FileService);
            };

            directive['$inject'] = ['$modal', 'FileService'];
            return directive;
        }
    }

    app.directive('filePicker', FilePicker.Factory());

    //TODO talk to alex and move this class to services folder? !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    interface IFileService {
        uploadFile(file: any): any;
        listFiles(): ng.IPromise<Array<interfaces.IFileDescriptionDTO>>
    }


    /*
        General data persistance methods for FileDirective.
    */
    class FileService implements IFileService {
        constructor(
            private $http: ng.IHttpService,
            private $q: ng.IQService,
            private UploadService: any
            ) { }


        public uploadFile(file: any): any {
            var deferred = this.$q.defer();

            this.UploadService.upload({
                url: '/files',
                file: file
            }).progress((event: any) => {
                console.log('Loaded: ' + event.loaded + ' / ' + event.total);
            })
                .success((fileDTO: interfaces.IFileDescriptionDTO) => {
                 deferred.resolve(fileDTO);
            })
            .error((data: any, status: any) => {
                deferred.reject(status);
            });

            return deferred.promise;
        }

        public listFiles(): ng.IPromise<Array<interfaces.IFileDescriptionDTO>> {
            var deferred = this.$q.defer();
            this.$http.get<Array<interfaces.IFileDescriptionDTO>>('/files').then(resp => {
                deferred.resolve(resp.data);
            }, err => {
                deferred.reject(err);
            });
            return deferred.promise;
            
        }
    }

    /*
        Register FileService with AngularJS. Upload dependency comes from ng-file-upload module
    */
    app.factory('FileService', ['$http', '$q', 'Upload',
        ($http, $q, UploadService) => {
            return new FileService($http, $q, UploadService);
    }]);

    /*
    A simple controller for Listing existing files dialog.
    Note: here goes a simple (not really a TypeScript) way to define a controller. 
    Not as a class but as a lambda function.
*/
    app.controller('FilePicker__FileSelectorModalController', ['$scope', '$modalInstance', 'files', ($scope: any, $modalInstance: any, files: Array<interfaces.IFileDescriptionDTO>): void => {

        $scope.files = files;

        $scope.selectFile = (file: interfaces.IFileDescriptionDTO) => {
            $modalInstance.close(file);
        };

        $scope.cancel = () => {
            $modalInstance.dismiss();
        };

    }]);

}