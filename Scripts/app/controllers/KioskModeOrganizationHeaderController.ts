/// <reference path="../_all.ts" />

module dockyard.controllers.KioskModeOrganizationHeaderController {
    'use strict';
    export interface IKioskModeOrganizationHeaderController extends ng.IScope {
        name: string;
        themeName: string;
        backgroundColor: string;
        logoUrl: string;
    }

    class KioskModeOrganizationHeaderController {
        // $inject annotation.
        // It provides $injector with information about dependencies to be injected into constructor
        // it is better to have it close to the constructor, because the parameters must match in count and type.
        // See http://docs.angularjs.org/guide/di
        public static $inject = [
            '$scope',
            'OrganizationService',
            'UserService',
        ];

        constructor(
            private $scope: IKioskModeOrganizationHeaderController,
            private OrganizationService: services.IOrganizationService,
            private UserService: services.IUserService) {

            UserService.getCurrentUser().$promise.then((currentUser: interfaces.IUserDTO) => {
                var organizationId = currentUser.organizationId;
                if (organizationId !== null) {
                    OrganizationService.get({ id: organizationId }).$promise.then((organization: interfaces.IOrganizationVM) => {
                        $scope.name = organization.name;
                        $scope.themeName = organization.themeName;
                        $scope.backgroundColor = organization.backgroundColor;
                        $scope.logoUrl = organization.logoUrl;
                    });
                }
                else {
                    $scope.name = "App Launcher";
                    $scope.themeName = "Default Theme";
                    $scope.backgroundColor = "#368da5";
                    $scope.logoUrl = "/Content/img/dockyard_logo.png";
                }
            });
        }
    }

    app.controller('KioskModeOrganizationHeaderController', KioskModeOrganizationHeaderController);
}