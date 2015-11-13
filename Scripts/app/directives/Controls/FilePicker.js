/// <reference path="../../_all.ts" />
var dockyard;
(function (dockyard) {
    var directives;
    (function (directives) {
        var filePicker;
        (function (filePicker) {
            'use strict';
            var pca = dockyard.directives.paneConfigureAction;
            //More detail on creating directives in TypeScript: 
            //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
            //export function FilePicker(): ng.IDirective {
            var FilePicker = (function () {
                function FilePicker($modal, FileService) {
                    var _this = this;
                    this.$modal = $modal;
                    this.FileService = FileService;
                    this.templateUrl = '/AngularTemplate/FilePicker';
                    this.scope = {
                        field: '='
                    };
                    this.restrict = 'E';
                    FilePicker.prototype.link = function (scope, element, attrs) {
                        //Link function goes here
                    };
                    FilePicker.prototype.controller = function ($scope, $element, $attrs) {
                        _this._$element = $element;
                        _this._$scope = $scope;
                        _this.FileService = FileService;
                        _this._$scope.selectedFile = null;
                        $scope.OnFileSelect = angular.bind(_this, _this.OnFileSelect);
                        $scope.ListExistingFiles = angular.bind(_this, _this.ListExistingFiles);
                        $scope.Save = angular.bind(_this, _this.Save);
                    };
                }
                FilePicker.prototype.OnFileUploadSuccess = function (fileDTO) {
                    this._$scope.selectedFile = fileDTO;
                    this._$scope.$root.$broadcast("fp-success", fileDTO);
                    this._$scope.field.value = fileDTO.cloudStorageUrl;
                    this._$scope.$root.$broadcast("onChange", new pca.ChangeEventArgs("select_file"));
                };
                FilePicker.prototype.OnFileUploadFail = function (status) {
                    alert('sorry file upload failed with status: ' + status);
                };
                FilePicker.prototype.OnFileSelect = function ($file) {
                    var onFileUploadSuccess = angular.bind(this, this.OnFileUploadSuccess);
                    var onFileUploadFail = angular.bind(this, this.OnFileUploadFail);
                    this.FileService.uploadFile($file).then(onFileUploadSuccess, onFileUploadFail);
                };
                FilePicker.prototype.Save = function () {
                    if (this._$scope.selectedFile === null) {
                        //raise some kind of error to prevent continuing
                        alert('No file was selected!!!!!!');
                        return;
                    }
                    //we should assign id of selected file to model value
                    //this._$scope.field.value = this._fileDTO.id.toString();
                    alert('Selected FileDO ID -> ' + this._$scope.selectedFile.id.toString());
                    //TODO add this file's id to CrateDO
                };
                FilePicker.prototype.OnExistingFileSelected = function (fileDTO) {
                    this._$scope.selectedFile = fileDTO;
                };
                FilePicker.prototype.OnFilesLoaded = function (filesDTO) {
                    var modalInstance = this.$modal.open({
                        animation: true,
                        templateUrl: '/AngularTemplate/FileSelectorModal',
                        controller: 'FilePicker__FileSelectorModalController',
                        size: 'm',
                        resolve: {
                            files: function () { return filesDTO; }
                        }
                    });
                    var onExistingFileSelected = angular.bind(this, this.OnExistingFileSelected);
                    modalInstance.result.then(onExistingFileSelected);
                };
                FilePicker.prototype.ListExistingFiles = function () {
                    var onFilesLoaded = angular.bind(this, this.OnFilesLoaded);
                    this.FileService.listFiles().then(onFilesLoaded);
                };
                //The factory function returns Directive object as per Angular requirements
                FilePicker.Factory = function () {
                    var directive = function ($modal, FileService) {
                        return new FilePicker($modal, FileService);
                    };
                    directive['$inject'] = ['$modal', 'FileService'];
                    return directive;
                };
                return FilePicker;
            })();
            app.directive('filePicker', FilePicker.Factory());
            app.filter('formatInput', function () {
                return function (input) {
                    if (input) {
                        return 'Selected File : ' + input.substring(input.lastIndexOf('/') + 1, input.length);
                    }
                    return input;
                };
            });
            /*
                General data persistance methods for FileDirective.
            */
            var FileService = (function () {
                function FileService($http, $q, UploadService) {
                    this.$http = $http;
                    this.$q = $q;
                    this.UploadService = UploadService;
                }
                FileService.prototype.uploadFile = function (file) {
                    var deferred = this.$q.defer();
                    this.UploadService.upload({
                        url: '/files',
                        file: file
                    }).progress(function (event) {
                        console.log('Loaded: ' + event.loaded + ' / ' + event.total);
                    })
                        .success(function (fileDTO) {
                        deferred.resolve(fileDTO);
                    })
                        .error(function (data, status) {
                        deferred.reject(status);
                    });
                    return deferred.promise;
                };
                FileService.prototype.listFiles = function () {
                    var deferred = this.$q.defer();
                    this.$http.get('/files').then(function (resp) {
                        deferred.resolve(resp.data);
                    }, function (err) {
                        deferred.reject(err);
                    });
                    return deferred.promise;
                };
                return FileService;
            })();
            /*
                Register FileService with AngularJS. Upload dependency comes from ng-file-upload module
            */
            app.factory('FileService', ['$http', '$q', 'Upload',
                function ($http, $q, UploadService) {
                    return new FileService($http, $q, UploadService);
                }]);
            /*
            A simple controller for Listing existing files dialog.
            Note: here goes a simple (not really a TypeScript) way to define a controller.
            Not as a class but as a lambda function.
        */
            app.controller('FilePicker__FileSelectorModalController', ['$scope', '$modalInstance', 'files', function ($scope, $modalInstance, files) {
                    $scope.files = files;
                    $scope.selectFile = function (file) {
                        $modalInstance.close(file);
                    };
                    $scope.cancel = function () {
                        $modalInstance.dismiss();
                    };
                }]);
        })(filePicker = directives.filePicker || (directives.filePicker = {}));
    })(directives = dockyard.directives || (dockyard.directives = {}));
})(dockyard || (dockyard = {}));
//# sourceMappingURL=FilePicker.js.map