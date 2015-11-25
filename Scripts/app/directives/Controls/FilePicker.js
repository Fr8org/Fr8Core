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
            function FilePicker() {
                var controller = ['$scope', '$modal', 'FileService', function ($scope, $modal, FileService) {
                        $scope.selectedFile = null;
                        var OnFileUploadSuccess = function (fileDTO) {
                            $scope.selectedFile = fileDTO;
                            $scope.$root.$broadcast("fp-success", fileDTO);
                            $scope.field.value = fileDTO.cloudStorageUrl;
                            $scope.$root.$broadcast("onChange", new pca.ChangeEventArgs($scope.field));
                        };
                        var OnFileUploadFail = function (status) {
                            alert('sorry file upload failed with status: ' + status);
                        };
                        $scope.OnFileSelect = function ($file) {
                            FileService.uploadFile($file).then(OnFileUploadSuccess, OnFileUploadFail);
                        };
                        $scope.Save = function () {
                            if ($scope.selectedFile === null) {
                                //raise some kind of error to prevent continuing
                                alert('No file was selected!!!!!!');
                                return;
                            }
                            //we should assign id of selected file to model value
                            //this._$scope.field.value = this._fileDTO.id.toString();
                            alert('Selected FileDO ID -> ' + $scope.selectedFile.id.toString());
                            //TODO add this file's id to CrateDO
                        };
                        var OnExistingFileSelected = function (fileDTO) {
                            $scope.selectedFile = fileDTO;
                        };
                        var OnFilesLoaded = function (filesDTO) {
                            var modalInstance = $modal.open({
                                animation: true,
                                templateUrl: '/AngularTemplate/FileSelectorModal',
                                controller: 'FilePicker__FileSelectorModalController',
                                size: 'm',
                                resolve: {
                                    files: function () { return filesDTO; }
                                }
                            });
                            modalInstance.result.then(OnExistingFileSelected);
                        };
                        $scope.ListExistingFiles = function () {
                            FileService.listFiles().then(OnFilesLoaded);
                        };
                    }];
                return {
                    restrict: 'E',
                    templateUrl: '/AngularTemplate/FilePicker',
                    controller: controller,
                    scope: {
                        field: '='
                    }
                };
            }
            filePicker.FilePicker = FilePicker;
            app.directive('filePicker', FilePicker);
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