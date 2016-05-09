/// <reference path="../_all.ts" />

module dockyard.controllers.OrganizationController {
    'use strict';
    import filePickerEvents = dockyard.Fr8Events.FilePicker;
    export interface IOrganizationControllerScope extends ng.IScope {
        name: string;
        themeName: string;
        backgroundColor: string;
        logoUrl: string;
        id: number;
        message: string;
        updateOrganization: () => void;
        OnFileSelect: ($file: any) => void;
    }
    
    class OrganizationController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'OrganizationService',
            'UserService',
            'FileService'
        ];

        constructor(
            private $scope: IOrganizationControllerScope,
            private OrganizationService: services.IOrganizationService,
            private UserService: services.IUserService,
            private FileService: services.IFileService) {
            

            UserService.getCurrentUser().$promise.then(function (currentUser: interfaces.IUserDTO){
                var organizationId = currentUser.organizationId;
                OrganizationService.get({ id: organizationId }).$promise.then(function (organization: interfaces.IOrganizationVM) {
                    $scope.name = organization.name;
                    $scope.id = organization.id;
                    $scope.themeName = organization.themeName;
                    $scope.backgroundColor = organization.backgroundColor;
                    $scope.logoUrl = organization.logoUrl;
                });
            });

            $scope.updateOrganization = function () {
                var organization: model.OrganizationDTO = { 
                    id: $scope.id,
                    name: $scope.name,
                    themeName: $scope.themeName,
                    backgroundColor: $scope.backgroundColor,
                    logoUrl: $scope.logoUrl
                }
                OrganizationService.update(organization).$promise.then(printResultMessage);
            };

            var printResultMessage = function () {
                $scope.message = "Your changes have been saved successfully"
            }
            $scope.OnFileSelect = $file => {
                FileService.uploadFile($file).then(OnFileUploadSuccess);
            };

            var OnFileUploadSuccess = function (fileDTO: interfaces.IFileDescriptionDTO) {
                $scope.logoUrl = (<dockyard.model.FileDTO>fileDTO).cloudStorageUrl;
            };
            
        }
    }

    app.controller('OrganizationController', OrganizationController);
}