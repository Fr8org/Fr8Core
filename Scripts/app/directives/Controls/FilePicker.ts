/// <reference path="../../_all.ts" />
module dockyard.directives.filePicker {
    'use strict';

    export interface IFilePickerScope extends ng.IScope {
        OnFileSelect: ($file: any) => void;
        ListExistingFiles: () => void;
        Save: () => void;
        field: model.File;
        selectedFile: interfaces.IFileDescriptionDTO;
        change: () => (field: model.ControlDefinitionDTO) => void;
    }

    import pca = dockyard.directives.paneConfigureAction;
    import filePickerEvents = dockyard.Fr8Events.FilePicker;

    //More detail on creating directives in TypeScript: 
    //http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
    export function FilePicker(): ng.IDirective {

        var controller = ['$scope', '$modal', 'FileService', function ($scope: IFilePickerScope, $modal: any, FileService: services.IFileService) {

            $scope.selectedFile = null;

            var OnFileUploadSuccess = function (fileDTO: interfaces.IFileDescriptionDTO) {
                $scope.selectedFile = fileDTO;
                $scope.$root.$broadcast(<any>filePickerEvents.FP_SUCCESS, fileDTO);
                $scope.field.value = (<dockyard.model.FileDTO>fileDTO).cloudStorageUrl;
                if ($scope.change != null && angular.isFunction($scope.change)) {
                    $scope.change()($scope.field);
                }
                //$scope.$root.$broadcast("onChange", new pca.ChangeEventArgs($scope.field));
            };
            var OnFileUploadFail = function(status: any) {
                alert('sorry file upload failed with status: ' + status);
            };
            $scope.OnFileSelect = $file => {
                FileService.uploadFile($file).then(OnFileUploadSuccess, OnFileUploadFail);
            };
            $scope.Save = () => {
                if ($scope.selectedFile === null) {
                    //raise some kind of error to prevent continuing
                    alert('Please select a file to upload.');
                    return;
                }
            
                //we should assign id of selected file to model value
                //this._$scope.field.value = this._fileDTO.id.toString();
                alert('Selected FileDO ID -> ' + $scope.selectedFile.id.toString());
                //TODO add this file's id to CrateDO
            };
            var OnExistingFileSelected = function(fileDTO: interfaces.IFileDescriptionDTO) {
                $scope.selectedFile = fileDTO;
            };
            var OnFilesLoaded = function(filesDTO: Array<interfaces.IFileDescriptionDTO>) {

                var modalInstance = $modal.open({
                    animation: true,
                    templateUrl: '/AngularTemplate/FileSelectorModal',
                    controller: 'FilePicker__FileSelectorModalController',
                    size: 'm',
                    resolve: {
                        files: () => filesDTO
                    }
                });

                modalInstance.result.then(OnExistingFileSelected);
            };
            $scope.ListExistingFiles = () => {
                FileService.listFiles().then(OnFilesLoaded);
            };
        }];

        return {
            restrict: 'E',
            templateUrl: '/AngularTemplate/FilePicker',
            controller: controller,
            scope: {
                field: '=',
                change: '&'
            }
        };
    }

    app.directive('filePicker', FilePicker);

    app.filter('formatInput', () => input => {
        if (input) {
            var decode = decodeURIComponent(input);
            return 'Selected File : ' + decode.substring(decode.lastIndexOf('/') + 1, decode.length);
        }
        return decodeURIComponent(input);
    });

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