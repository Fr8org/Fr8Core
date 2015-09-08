/// <reference path="../../_all.ts" />
module dockyard.directives.filePicker {
    'use strict';

    export enum MessageType {
    }

    export interface IFilePickerScope extends ng.IScope {
        OnFileSelect: ($file: any) => void;
        ListExistingFiles: () => void;
        Save: () => void;
        field: model.FileField;
    }

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    class FilePicker implements ng.IDirective {
        public link: (scope: IFilePickerScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public templateUrl = '/AngularTemplate/FilePicker';
        public controller: ($scope: IFilePickerScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
        public scope = {
            field: '=',
            accept: '@'
        };
        public restrict = 'E';
        private _$element: ng.IAugmentedJQuery;
        private _$scope: IFilePickerScope;
        private _fileDTO: interfaces.IFileDTO;

        constructor(private $rootScope: interfaces.IAppRootScope, private $modal: any, private FileService: IFileService) {
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
                //Controller goes here
                $scope.OnFileSelect = <($file: any) => void> angular.bind(this, this.OnFileSelect);
                $scope.ListExistingFiles = <() => void> angular.bind(this, this.ListExistingFiles);
                $scope.Save = <() => void> angular.bind(this, this.Save);
            };
        }

        private OnFileUploadSuccess(fileDTO: interfaces.IFileDTO) {
            console.log('File was uploaded successfully');
            console.log(fileDTO);
            this._fileDTO = fileDTO;
        }

        private OnFileUploadFail(status: any) {
            console.log('error status: ' + status);
        }

        private OnFileSelect($file) {
            var onFileUploadSuccess = <(fileDTO: interfaces.IFileDTO) => void> angular.bind(this, this.OnFileUploadSuccess);
            var onFileUploadFail = <(status: any) => void> angular.bind(this, this.OnFileUploadFail);
            console.log('Uploading file');
            this.FileService.uploadFile($file).then(onFileUploadSuccess, onFileUploadFail);
        }

        private Save() {
            if (this._fileDTO === null) {
                //raise some kind of error to prevent continuing
                alert('No file was selected!!!!!!');
                return;
            }
            
            //we should assign id of selected file to model value
            //this._$scope.field.value = this._fileDTO.id.toString();
            alert('Selected FileDO ID -> ' + this._fileDTO.id.toString());
        }

        private OnExistingFileSelected(fileDTO: interfaces.IFileDTO) {
            console.log('File was selected successfully');
            console.log(fileDTO);
            this._fileDTO = fileDTO;
        }

        private OnFilesLoaded(filesDTO: Array<interfaces.IFileDTO>) {
            console.log('Listing files');
            console.log(filesDTO);
            
            //THIS IS FOR DEMO ONLY. i (@bahadirbozdag) will fix this -----------------------------------

            var modalInstance = this.$modal.open({
                templateUrl: 'fileSelectorModal.html',
                controller: ['$scope', '$modalInstance', 'files', ($modalScope: any, $modalInstance: any, files: Array<interfaces.IFileDTO>) => {
                    $modalScope.files = files;

                    $modalScope.selectFile = (file: interfaces.IFileDTO) => {
                        $modalInstance.close(file);
                    };

                    $modalScope.cancel  = () => {
                        $modalInstance.dismiss();
                    };
                }],
                size: 'm',
                resolve: {
                    files: () => filesDTO
                }
            });

            var onExistingFileSelected = <(fileDTO: interfaces.IFileDTO) => void> angular.bind(this, this.OnExistingFileSelected);
            modalInstance.result.then(onExistingFileSelected);

            //END OF DIRTY CODE
        }

        private ListExistingFiles() {
            var onFilesLoaded = <(filesDTO: Array<interfaces.IFileDTO>) => void> angular.bind(this, this.OnFilesLoaded);
            //load existing files
            console.log('List existing files was clicked');
            this.FileService.listFiles().then(onFilesLoaded);
        }

        //The factory function returns Directive object as per Angular requirements
        public static Factory() {
            var directive = ($rootScope: interfaces.IAppRootScope, $modal: any, FileService: any) => {
                return new FilePicker($rootScope, $modal, FileService);
            };

            directive['$inject'] = ['$rootScope', '$modal', 'FileService'];
            return directive;
        }
    }

    app.directive('filePicker', FilePicker.Factory());

    //TODO talk to alex and move this class to services folder !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    interface IFileService {
        uploadFile(file: any): any;
        listFiles(): ng.IPromise<Array<interfaces.IFileDTO>>
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
                url: '/api/files',
                file: file
            }).progress((event: any) => {
                console.log('Loaded: ' + event.loaded + ' / ' + event.total);
            })
            .success((fileDTO: interfaces.IFileDTO) => {
                 deferred.resolve(fileDTO);
            })
            .error((data: any, status: any) => {
                deferred.reject(status);
            });

            return deferred.promise;
        }

        public listFiles(): ng.IPromise<Array<interfaces.IFileDTO>> {
            var deferred = this.$q.defer();
            this.$http.get<Array<interfaces.IFileDTO>>('/api/files').then(resp => {
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

}